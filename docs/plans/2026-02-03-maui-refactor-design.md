# GourmetClient MAUI Refactor Design

## Overview

Migrate the existing Windows WPF application to .NET MAUI for cross-platform support (Windows, Mac, iOS, Android).

## Design Decisions

### Target Platforms
- Windows (desktop)
- Mac Catalyst (desktop)
- iOS (mobile)
- Android (mobile)

### Update Mechanism
- **Mobile**: App Store / Google Play (no in-app updates)
- **Desktop**: Velopack with GitHub Releases

---

## Section 1: Core Network Layer (Zero Changes)

The entire `Network/` folder is copied unchanged. Web scraping logic must remain identical.

**Files to copy as-is:**
- `Network/WebClientBase.cs`
- `Network/GourmetWebClient.cs`
- `Network/VentopayWebClient.cs`
- `Network/LoginHandle.cs`
- `Network/GourmetCacheService.cs`
- `Network/BillingCacheService.cs`
- `Network/GourmetApi/*`

**Rationale:** Modifying any scraping logic risks account blocks on external services.

---

## Section 2: File System Abstraction

### Interface

```csharp
public interface IAppDataPaths
{
    string AppDataDirectory { get; }
    string CacheDirectory { get; }
    string SettingsFilePath { get; }
}
```

### Implementation

```csharp
public class MauiAppDataPaths : IAppDataPaths
{
    public string AppDataDirectory => FileSystem.AppDataDirectory;
    public string CacheDirectory => FileSystem.CacheDirectory;
    public string SettingsFilePath => Path.Combine(AppDataDirectory, "settings.json");
}
```

### Platform Paths

| Platform | AppDataDirectory |
|----------|------------------|
| Windows | `C:\Users\{user}\AppData\Local\GourmetClient` |
| Mac | `~/Library/Application Support/GourmetClient` |
| iOS | App sandbox `/Documents` |
| Android | App sandbox `/data/data/com.company.gourmetclient/files` |

---

## Section 3: Credential Storage

### Approach: AES Encryption (Cross-Platform)

```csharp
public interface ICredentialService
{
    Task SaveCredentialsAsync(string key, string username, string password);
    Task<(string username, string password)?> GetCredentialsAsync(string key);
    Task DeleteCredentialsAsync(string key);
}
```

### Implementation

```csharp
public class AesCredentialService : ICredentialService
{
    private readonly IAppDataPaths _paths;
    private readonly byte[] _key;  // Derived from device-specific identifier

    public AesCredentialService(IAppDataPaths paths)
    {
        _paths = paths;
        _key = DeriveKeyFromDevice();
    }

    private byte[] DeriveKeyFromDevice()
    {
        // Use a combination of:
        // - App-specific GUID (embedded in app)
        // - Platform identifier
        var baseId = "GourmetClient-" + DeviceInfo.Platform;
        return SHA256.HashData(Encoding.UTF8.GetBytes(baseId));
    }

    public async Task SaveCredentialsAsync(string key, string username, string password)
    {
        var data = JsonSerializer.Serialize(new { username, password });
        var encrypted = EncryptionHelper.EncryptWithAes(data, _key);
        var filePath = Path.Combine(_paths.AppDataDirectory, $"{key}.cred");
        await File.WriteAllBytesAsync(filePath, encrypted);
    }

    public async Task<(string username, string password)?> GetCredentialsAsync(string key)
    {
        var filePath = Path.Combine(_paths.AppDataDirectory, $"{key}.cred");
        if (!File.Exists(filePath)) return null;

        var encrypted = await File.ReadAllBytesAsync(filePath);
        var decrypted = EncryptionHelper.DecryptWithAes(encrypted, _key);
        var cred = JsonSerializer.Deserialize<CredentialData>(decrypted);
        return (cred.Username, cred.Password);
    }
}
```

---

## Section 4: Navigation Structure

### AppShell with Tabs

```
┌─────────────────────────────────────┐
│  [Menu]   [Billing]   [Settings]    │  ← Tab bar
├─────────────────────────────────────┤
│                                     │
│         Content Area                │
│                                     │
└─────────────────────────────────────┘
```

### Implementation

```xml
<!-- AppShell.xaml -->
<Shell>
    <TabBar>
        <ShellContent Title="Menu" Icon="menu_icon.png"
                      ContentTemplate="{DataTemplate views:MenuOrderPage}" />
        <ShellContent Title="Billing" Icon="billing_icon.png"
                      ContentTemplate="{DataTemplate views:BillingPage}" />
        <ShellContent Title="Settings" Icon="settings_icon.png"
                      ContentTemplate="{DataTemplate views:SettingsPage}" />
    </TabBar>
</Shell>
```

### Page Mapping

| WPF Window/Control | MAUI Page |
|--------------------|-----------|
| MainWindow | AppShell |
| MenuOrderView | MenuOrderPage |
| BillingView | BillingPage |
| SettingsView | SettingsPage |
| NotificationPopup | MAUI Toast/Snackbar |

---

## Section 5: MVVM Migration

### ViewModelBase

```csharp
// Using CommunityToolkit.Mvvm
public partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage;
}
```

### Command Migration

**WPF (before):**
```csharp
public ICommand RefreshCommand { get; }

public MenuOrderViewModel()
{
    RefreshCommand = new RelayCommand(async () => await RefreshAsync(), () => !IsBusy);
}
```

**MAUI (after):**
```csharp
public partial class MenuOrderViewModel : ViewModelBase
{
    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshAsync()
    {
        // implementation
    }

    private bool CanRefresh() => !IsBusy;
}
```

### Dependency Injection Setup

```csharp
// MauiProgram.cs
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    builder.UseMauiApp<App>();

    // Services
    builder.Services.AddSingleton<IAppDataPaths, MauiAppDataPaths>();
    builder.Services.AddSingleton<ICredentialService, AesCredentialService>();
    builder.Services.AddSingleton<GourmetWebClient>();
    builder.Services.AddSingleton<VentopayWebClient>();
    builder.Services.AddSingleton<GourmetCacheService>();
    builder.Services.AddSingleton<BillingCacheService>();

    // ViewModels
    builder.Services.AddTransient<MenuOrderViewModel>();
    builder.Services.AddTransient<BillingViewModel>();
    builder.Services.AddTransient<SettingsViewModel>();

    // Pages
    builder.Services.AddTransient<MenuOrderPage>();
    builder.Services.AddTransient<BillingPage>();
    builder.Services.AddTransient<SettingsPage>();

    // Platform-specific
#if WINDOWS || MACCATALYST
    builder.Services.AddSingleton<IUpdateService, VelopackUpdateService>();
#else
    builder.Services.AddSingleton<IUpdateService, NoOpUpdateService>();
#endif

    return builder.Build();
}
```

---

## Section 6: Update Mechanism

### Mobile (iOS & Android) - App Store only

- No in-app update logic
- Users update via App Store / Google Play
- Settings page hides update section on mobile

### Desktop (Windows & Mac) - Velopack

```csharp
// MauiProgram.cs - Desktop only
#if WINDOWS || MACCATALYST
using Velopack;

public static MauiApp CreateMauiApp()
{
    VelopackApp.Build().Run();  // Handle install/update hooks

    var builder = MauiApp.CreateBuilder();
    // ... rest of setup
}
#endif
```

```csharp
public interface IUpdateService
{
    bool IsSupported { get; }
    Task<bool> CheckForUpdateAsync();
    Task DownloadAndApplyAsync();
}

// Desktop implementation
#if WINDOWS || MACCATALYST
public class VelopackUpdateService : IUpdateService
{
    private readonly UpdateManager _manager = new("https://github.com/patrickl92/GourmetClient/releases");

    public bool IsSupported => true;

    public async Task<bool> CheckForUpdateAsync()
    {
        var update = await _manager.CheckForUpdatesAsync();
        return update != null;
    }

    public async Task DownloadAndApplyAsync()
    {
        var update = await _manager.CheckForUpdatesAsync();
        await _manager.DownloadUpdatesAsync(update);
        _manager.ApplyUpdatesAndRestart(update);
    }
}
#endif

// Mobile - stub implementation
#if IOS || ANDROID
public class NoOpUpdateService : IUpdateService
{
    public bool IsSupported => false;
    public Task<bool> CheckForUpdateAsync() => Task.FromResult(false);
    public Task DownloadAndApplyAsync() => Task.CompletedTask;
}
#endif
```

---

## Section 7: Project Structure

```
src/
└── GourmetClient.Maui/
    ├── GourmetClient.Maui.csproj
    ├── MauiProgram.cs                 # DI setup, Velopack init
    ├── App.xaml / App.xaml.cs         # App lifecycle
    ├── AppShell.xaml / AppShell.cs    # Navigation structure
    │
    ├── Platforms/                     # Platform bootstrapping (auto-generated)
    │   ├── Android/
    │   │   └── MainActivity.cs
    │   ├── iOS/
    │   │   └── AppDelegate.cs
    │   ├── MacCatalyst/
    │   │   └── AppDelegate.cs
    │   └── Windows/
    │       └── App.xaml.cs
    │
    ├── Core/                          # UNCHANGED from WPF
    │   ├── Network/                   # ✅ Copy as-is
    │   ├── Model/                     # ✅ Copy as-is
    │   ├── Serialization/             # ✅ Copy as-is
    │   └── Notifications/             # ✅ Copy as-is
    │
    ├── ViewModels/                    # Adapted from WPF
    │   ├── ViewModelBase.cs
    │   ├── MenuOrderViewModel.cs
    │   ├── BillingViewModel.cs
    │   └── SettingsViewModel.cs
    │
    ├── Views/                         # New MAUI pages
    │   ├── MenuOrderPage.xaml
    │   ├── BillingPage.xaml
    │   └── SettingsPage.xaml
    │
    ├── Services/                      # Platform abstractions
    │   ├── IUpdateService.cs
    │   ├── ICredentialService.cs
    │   └── Implementations/
    │       ├── VelopackUpdateService.cs
    │       └── AesCredentialService.cs
    │
    ├── Converters/                    # MAUI value converters
    │   ├── BoolToVisibilityConverter.cs
    │   └── AllergensToStringConverter.cs
    │
    ├── Resources/
    │   ├── Styles/
    │   │   ├── Colors.xaml
    │   │   └── Styles.xaml
    │   ├── Fonts/
    │   └── Images/
    │
    └── Utils/
        ├── EncryptionHelper.cs        # ✅ Copy as-is
        └── HttpClientHelper.cs        # ✅ Copy as-is
```

---

## Section 8: Dependencies

```xml
<!-- GourmetClient.Maui.csproj -->
<ItemGroup>
    <!-- MAUI essentials -->
    <PackageReference Include="Microsoft.Maui.Controls" Version="9.*" />
    <PackageReference Include="Microsoft.Maui.Controls.Compatibility" Version="9.*" />

    <!-- MVVM toolkit (replaces custom commands) -->
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.*" />

    <!-- Existing dependencies (unchanged) -->
    <PackageReference Include="HtmlAgilityPack" Version="1.12.*" />
    <PackageReference Include="Semver" Version="3.*" />

    <!-- Desktop updates -->
    <PackageReference Include="Velopack" Version="0.*" Condition="$(TargetFramework.Contains('windows')) Or $(TargetFramework.Contains('maccatalyst'))" />
</ItemGroup>

<!-- Target frameworks -->
<PropertyGroup>
    <TargetFrameworks>net9.0-android;net9.0-ios;net9.0-maccatalyst;net9.0-windows10.0.19041.0</TargetFrameworks>
</PropertyGroup>
```

**Removed dependencies:**
- `Microsoft.Xaml.Behaviors.Wpf` - WPF-specific
- `Microsoft.Extensions.Primitives` - Likely not needed

**New dependencies:**
- `CommunityToolkit.Mvvm` - Source-generated commands, ObservableObject
- `Velopack` - Desktop updates (conditional)

---

## Section 9: Implementation Steps

### Phase 1: Project Setup
1. Create new MAUI solution and project
2. Configure target frameworks and dependencies
3. Set up folder structure

### Phase 2: Core Layer (Zero-risk)
4. Copy `Network/` folder unchanged
5. Copy `Model/` folder unchanged
6. Copy `Serialization/` folder unchanged
7. Copy `Notifications/` folder unchanged
8. Copy `Utils/EncryptionHelper.cs` and `Utils/HttpClientHelper.cs`
9. Create `IAppDataPaths` interface and implementation
10. Update file path references to use injected paths

### Phase 3: Services Layer
11. Implement `ICredentialService` with AES encryption
12. Implement `IUpdateService` (Velopack for desktop, no-op for mobile)
13. Set up dependency injection in `MauiProgram.cs`

### 🧪 Checkpoint A: Verify build compiles for all platforms
```bash
dotnet build -f net9.0-windows10.0.19041.0
dotnet build -f net9.0-android
dotnet build -f net9.0-ios
```

### Phase 4: ViewModels
14. Port `ViewModelBase` using CommunityToolkit.Mvvm
15. Port `MenuOrderViewModel` (adapt commands)
16. Port `BillingViewModel`
17. Port `SettingsViewModel`
18. Port `NotificationsViewModel`

### Phase 5: Views
19. Create `AppShell` with tab navigation
20. Build `MenuOrderPage` (main menu list + ordering)
21. Build `BillingPage` (transaction list)
22. Build `SettingsPage` (credentials, cache settings)
23. Style per platform for native feel

### 🧪 Checkpoint B: Functional testing on all platforms

| Test | Windows | Android | iOS |
|------|---------|---------|-----|
| App launches | ☐ | ☐ | ☐ |
| Settings page renders | ☐ | ☐ | ☐ |
| Credentials save/load | ☐ | ☐ | ☐ |
| Login to Gourmet | ☐ | ☐ | ☐ |
| Login to Ventopay | ☐ | ☐ | ☐ |
| Menu list loads | ☐ | ☐ | ☐ |
| Menu ordering works | ☐ | ☐ | ☐ |
| Order cancellation works | ☐ | ☐ | ☐ |
| Billing data loads | ☐ | ☐ | ☐ |
| Offline cache viewing | ☐ | ☐ | ☐ |
| Navigation between tabs | ☐ | ☐ | ☐ |

### Phase 6: Polish & Platform-Specific
24. App icons and splash screens per platform
25. Platform-specific UI tweaks
26. Velopack integration for desktop
27. Mac Catalyst testing

### 🧪 Checkpoint C: Final verification

| Test | Windows | Android | iOS | Mac |
|------|---------|---------|-----|-----|
| Full workflow end-to-end | ☐ | ☐ | ☐ | ☐ |
| Update mechanism (desktop) | ☐ | N/A | N/A | ☐ |
| Native feel confirmed | ☐ | ☐ | ☐ | ☐ |
| Performance acceptable | ☐ | ☐ | ☐ | ☐ |

---

## Critical Reminders

⚠️ **DO NOT MODIFY:**
- Any code in `Network/` folder
- XPath selectors
- Regex patterns
- Form parameter names/values
- URL paths
- Date format strings
- Cookie handling behavior
- Login/logout sequences

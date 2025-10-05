# iOS UI Implementation for GC MVVM Project

## Overview
This implementation brings the iOS-optimized UI from the MVU project to the new GC MVVM project, providing a native iOS experience with bottom navigation, swipe gestures, and Apple design guidelines.

## Files Created

### Core Files
- **MainView.iOS.cs** - Main iOS view with bottom navigation, swipe gestures, and page management
- **MenuView.iOS.cs** - Menu view with iOS-optimized card layout
- **SettingsView.iOS.cs** - Settings view with mobile-friendly form inputs
- **AboutView.iOS.cs** - About page with touch-friendly buttons and layout
- **BillingView.iOS.cs** - Billing view with mobile-optimized display

### Utility Files
- **Utils/PlatformDetector.cs** - Platform detection utility for iOS/Android/Desktop

### Modified Files
- **MainViewModel.cs** - Extended with navigation properties and commands
- **MainViewHostControl.cs** - Updated to use iOS views on iOS platform with reactive updates

## Features Implemented

### 1. Bottom Navigation with Page Indicators
- 4 pages: Menu (Gourmet), Billing (Rechnung), Settings (Einstellungen), About (Über)
- Visual page indicators at the bottom
- Active page highlighted with iOS blue (#007AFF)
- Tap indicators to navigate directly to pages

### 2. Swipe Gesture Navigation
- Swipe left/right to navigate between pages
- 80px threshold for gesture activation
- Smooth transitions between pages

### 3. iOS Design Guidelines
- **Colors:**
  - Background: #000000 (dark) / #F2F2F7 (light)
  - Card background: #1C1C1E (dark) / White (light)
  - Accent: #007AFF (iOS blue)
  - Secondary text: #8E8E93
  - Error: #FF3B30

- **Typography:**
  - Title: 20-28pt, Bold
  - Body: 14-17pt
  - Secondary: 12-14pt

- **Layout:**
  - 12pt margins
  - 12pt corner radius on cards
  - Touch-friendly spacing (min 44pt touch targets)

### 4. Top Bar
- Dynamic title based on current page
- Username display on menu page
- Action buttons (refresh, save) context-sensitive

### 5. Error Display
- Red banner with dismiss button
- Wrapping text for long messages
- Auto-updates when error state changes

### 6. Reactive Updates
- View rebuilds automatically when:
  - Page changes (CurrentPageIndex)
  - Error message changes
  - Username changes
- Uses MVVM PropertyChanged pattern

## Architecture

### MVVM Pattern
```
MainViewModel (State)
    ↓ PropertyChanged
MainViewHostControl (Coordinator)
    ↓ Rebuild on changes
MainViewIOS (View Builder)
    ↓ Delegates to
MenuViewIOS, SettingsViewIOS, AboutViewIOS, BillingViewIOS
```

### Platform Detection
```csharp
if (PlatformDetector.IsIOS) {
    // Use iOS-specific views
    Content = MainViewIOS.Create(viewModel);
} else {
    // Use standard XAML views
    Content = new MainView();
}
```

## Comparison with MVU Implementation

| Aspect | MVU | MVVM (GC) |
|--------|-----|-----------|
| State Management | AppState record | MainViewModel class |
| Updates | Msg dispatch | PropertyChanged events |
| View Creation | Static Create() | Reactive rebuild |
| Navigation | NavigateToPage msg | NavigateToPageCommand |
| Error Handling | ErrorOccurred msg | ErrorMessage property |

## Usage

The iOS UI activates automatically when running on iOS devices:

```csharp
var hostControl = new MainViewHostControl();
// On iOS: Shows iOS-optimized UI with bottom navigation
// On Desktop: Shows standard XAML UI
```

### Navigation

```csharp
// Navigate to page
viewModel.NavigateToPageCommand.Execute(2); // Settings

// Or via swipe gestures (automatic)
```

### Error Display

```csharp
// Show error
viewModel.ErrorMessage = "Connection failed";

// Clear error
viewModel.ClearErrorCommand.Execute(null);
```

## Testing Notes

Due to iOS workload requirements, full compilation and testing on Linux is limited. The implementation:
- ✅ Follows exact same patterns as MVU iOS views
- ✅ Uses matching styles and colors
- ✅ Implements same gesture handling
- ⚠️ Requires macOS with Xcode for full iOS build
- ⚠️ Can be tested via iOS Simulator or device

## Future Enhancements

1. **Data Integration**
   - Connect to actual menu/billing services
   - Load real settings from storage
   - Implement save/load functionality

2. **Advanced Features**
   - Pull-to-refresh gestures
   - Animated transitions
   - Haptic feedback
   - Dark mode toggle

3. **Performance**
   - Virtualized scrolling for large lists
   - Image caching
   - Async data loading

## Files Structure

```
src/GC.Views/
├── MainView.iOS.cs          # Main iOS container
├── MenuView.iOS.cs          # Menu page
├── SettingsView.iOS.cs      # Settings page
├── AboutView.iOS.cs         # About page
├── BillingView.iOS.cs       # Billing page
├── MainViewHostControl.cs   # Platform router
└── Utils/
    └── PlatformDetector.cs  # Platform detection
```

## Key Design Decisions

1. **Static Factory Pattern**: Views are created via static `Create()` methods, matching MVU pattern
2. **Full Rebuild on Change**: Entire view rebuilds when properties change (simple, reliable)
3. **Platform Detection**: Uses `OperatingSystem.IsIOS()` for compile-time and runtime detection
4. **No XAML for iOS**: Pure C# code for iOS views for maximum control
5. **Shared ViewModels**: Same ViewModel works for both desktop and iOS UIs

## Migration from MVU

For developers familiar with the MVU version:

| MVU Concept | MVVM Equivalent |
|-------------|-----------------|
| `dispatch(new NavigateToPage(1))` | `viewModel.NavigateToPageCommand.Execute(1)` |
| `state.CurrentPageIndex` | `viewModel.CurrentPageIndex` |
| `state.ErrorMessage` | `viewModel.ErrorMessage` |
| `dispatch(new ClearError())` | `viewModel.ClearErrorCommand.Execute(null)` |

The UI structure and styling are identical, only the state management pattern differs.

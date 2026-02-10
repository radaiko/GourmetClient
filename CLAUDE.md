# GourmetClient

Cross-platform .NET MAUI application (.NET 10.0) that scrapes two external websites for company cafeteria menu and billing data.

## README Requirements

The README must always include a credit line linking to https://github.com/patrickl92/GourmetClient as the base/original project this was forked from.

## Critical Warning

**DO NOT ALTER THE WEB SCRAPING LOGIC.** The application performs website scraping (not API calls). Any deviation from the exact request sequence, headers, timing, or parameter values will trigger account blocks on the external services.

## Project Structure

```
src/
├── GourmetClient.sln
└── GourmetClient.Maui/
    ├── GourmetClient.Maui.csproj
    ├── MauiProgram.cs              # DI registration, app startup
    ├── App.xaml / AppShell.xaml     # Shell with tab navigation
    ├── Core/
    │   ├── Network/                # ALL WEB SCRAPING LOGIC HERE
    │   │   ├── WebClientBase.cs    # Base HTTP client, session management
    │   │   ├── GourmetWebClient.cs # Gourmet system scraping
    │   │   ├── VentopayWebClient.cs# Ventopay system scraping
    │   │   ├── LoginHandle.cs      # Session lifecycle (ref-counted)
    │   │   ├── GourmetCacheService.cs  # Menu/order cache
    │   │   ├── BillingCacheService.cs  # Billing data aggregation
    │   │   └── GourmetApi/         # JSON request/response models
    │   ├── Model/                  # Domain models
    │   ├── Settings/               # UserSettings, GourmetSettingsService
    │   ├── Serialization/          # JSON serialization DTOs
    │   └── Notifications/          # In-app notification system
    ├── Services/                   # Platform abstractions
    │   ├── IAppDataPaths.cs
    │   ├── ICredentialService.cs
    │   ├── IUpdateService.cs
    │   └── Implementations/
    │       ├── MauiAppDataPaths.cs
    │       ├── AesCredentialService.cs
    │       ├── VelopackUpdateService.cs  # Desktop only
    │       └── NoOpUpdateService.cs      # Mobile fallback
    ├── ViewModels/                 # CommunityToolkit.Mvvm ViewModels
    ├── Pages/                      # XAML pages (Menus, Orders, Billing, Settings)
    ├── Converters/                 # XAML value converters
    ├── Utils/
    │   ├── HttpClientHelper.cs     # HTTP client creation, proxy handling
    │   ├── HttpClientResult.cs
    │   ├── EncryptionHelper.cs
    │   └── ExtensionMethods.cs     # HtmlAgilityPack extensions
    └── Platforms/                  # Platform-specific entry points
```

## Two External Data Sources

### 1. Gourmet System (Menu & Orders)

- **Base URL**: `https://alaclickneu.gourmet.at/`
- **Client**: `Core/Network/GourmetWebClient.cs`
- **Purpose**: Menu data, order management, billing

### 2. Ventopay System (Billing)

- **Base URL**: `https://my.ventopay.com/mocca.website/`
- **Client**: `Core/Network/VentopayWebClient.cs`
- **Hardcoded Company ID**: `0da8d3ec-0178-47d5-9ccd-a996f04acb61`
- **Purpose**: Transaction/billing data from cafeteria POS

---

## Gourmet Web Scraping Specification

### Authentication Flow

**DO NOT MODIFY THIS SEQUENCE:**

1. **GET** `https://alaclickneu.gourmet.at/start/`
2. **Extract CSRF token** from hidden input named `ufprt`
3. **POST** `https://alaclickneu.gourmet.at/` with form data:
   ```
   Username: {username}
   Password: {password}
   RememberMe: false       # MUST be "false" (string)
   ufprt: {csrf_token}
   ```
4. **Verify login** by checking for regex pattern:
   ```regex
   <a href="https://alaclickneu.gourmet.at/einstellungen/" class="navbar-link">
   ```

### User Information Extraction

After login, extract from `start` page:

| Field | XPath |
|-------|-------|
| Username | `//div[@class='userfield']//span[@class='loginname']` |
| ShopModelId | `//input[@id='shopModel']/@value` |
| EaterId | `//input[@id='eater']/@value` |
| StaffGroupId | `//input[@id='staffGroup']/@value` |

### Menu Data Extraction

**Endpoint**: `https://alaclickneu.gourmet.at/menus?page={0-9}`

**Pagination**: Loop pages 0-9, stop when no `//a[contains(@class, 'menues-next')]` exists.

**Menu Item XPath**: `//div[@class='meal']`

| Field | Extraction Method |
|-------|-------------------|
| Menu ID | `.//div[@class='open_info menu-article-detail']` `data-id` attribute |
| Day | `.//div[@class='open_info menu-article-detail']` `data-date` attribute (format: `MM-dd-yyyy`) |
| Title | `.//div[@class='title']` first child text |
| Subtitle | `.//div[@class='subtitle']` text |
| Allergens | `.//li[@class='allergen']` text (comma-separated letters) |
| Available | Presence of `.//input[@type='checkbox' and @class='menu-clicked']` |

**Category Detection** (regex on title):
```regex
MENÜ\s+([I]{1,3})    # Matches MENÜ I, MENÜ II, MENÜ III
SUPPE & SALAT        # Literal match
```

### Order Operations

**Ordered Menus Endpoint**: `https://alaclickneu.gourmet.at/bestellungen`

**Order Item XPath**: `//div[contains(@class, 'order-item')]`

| Field | Extraction Method |
|-------|-------------------|
| Position ID | `.//input[@name='cp_PositionId']/@value` |
| Eating Cycle ID | `.//input[@name='cp_EatingCycleId_{positionId}']/@value` |
| Date | `.//input[@name='cp_Date_{positionId}']/@value` (format: `dd.MM.yyyy HH:mm:ss`) |
| Title | `.//div[@class='title']` text |
| Approved | Presence of class `confirmed` on radio input, or `fa fa-check` icon |

### JSON APIs

#### Add to Cart

**POST** `https://alaclickneu.gourmet.at/umbraco/api/AlaCartApi/AddToMenuesCart`

```json
{
  "eaterId": "string",
  "shopModelId": "string",
  "staffgroupId": "string",
  "dates": [
    {
      "date": "MM-dd-yyyy",
      "menuIds": ["id1"]
    }
  ]
}
```

#### Get Billing

**POST** `https://alaclickneu.gourmet.at/umbraco/api/AlaMyBillingApi/GetMyBillings`

```json
{
  "eaterId": "string",
  "shopModelId": "string",
  "checkLastMonthNumber": "0"  // 0 = current month
}
```

---

## Ventopay Web Scraping Specification

### Authentication Flow

**DO NOT MODIFY THIS SEQUENCE:**

1. **GET** `https://my.ventopay.com/mocca.website/Login.aspx`
2. **Extract ASP.NET state** from hidden inputs:
   - `__VIEWSTATE`
   - `__VIEWSTATEGENERATOR`
   - `__EVENTVALIDATION`
3. **POST** `https://my.ventopay.com/mocca.website/Login.aspx` with form data:
   ```
   DropDownList1: 0da8d3ec-0178-47d5-9ccd-a996f04acb61  # HARDCODED
   TxtUsername: {username}
   TxtPassword: {password}
   BtnLogin: Login
   languageRadio: DE
   __VIEWSTATE: {extracted}
   __VIEWSTATEGENERATOR: {extracted}
   __EVENTVALIDATION: {extracted}
   ```
4. **Verify login** by checking for regex pattern:
   ```regex
   <a\s+href="Ausloggen.aspx">
   ```

### Transaction Data Extraction

**Transaction List**: `https://my.ventopay.com/mocca.website/Transaktionen.aspx`
- Parameters: `fromDate` and `untilDate` (format: `dd.MM.yyyy`)
- XPath: `//div[@class='content']//div[@class='transact']`
- Extract transaction IDs from `id` attribute

**Transaction Details**: `https://my.ventopay.com/mocca.website/Rechnung.aspx?id={transactionId}`

| Field | Extraction Method |
|-------|-------------------|
| DateTime | `//span[@id='ContentPlaceHolder1_LblTimestamp']` |
| DateTime Format | Regex: `(\d+)\.\s+([a-zA-z]+)\s+(\d+)\s+-\s+(\d+):(\d+)` |
| Restaurant | `//span[@id='ContentPlaceHolder1_LblRestaurantInfo']` split by `<br>` |
| Items | `//div[@class='rechnungpart']//table//tbody` rows (excluding `rechnungsdetail` rows) |

**Item Row Columns**:
- Column 0: Count (format: `2x`)
- Column 1: Item name
- Column 4: Cost (German format: `12,34`)

**Filter Rule**: Skip transactions where restaurant name contains "Gourmet" AND location does NOT contain "Kaffeeautomat".

---

## Session & Cookie Management

**Location**: `Core/Network/WebClientBase.cs`

1. **Single CookieContainer per client instance** - cookies persist across all requests
2. **LoginHandle pattern** with reference counting - logout triggers when all handles disposed
3. **Thread-safe** via `SemaphoreSlim` and lock objects
4. **HttpClient is reused** - only recreated on network errors

### Proxy Handling

**Location**: `Utils/HttpClientHelper.cs`

1. On iOS/Mac Catalyst: uses default system handler (no custom proxy support)
2. On other platforms: detect system proxy via `WebRequest.DefaultWebProxy`
3. On 407 error: retry with proxy + default credentials
4. On DNS error: retry without proxy

---

## Things That Will Break Accounts

1. **Missing CSRF tokens** - every form needs fresh `ufprt` (Gourmet) or `__VIEWSTATE` (Ventopay)
2. **Wrong date formats** - Gourmet uses `MM-dd-yyyy`, Ventopay uses `dd.MM.yyyy`
3. **Missing form parameters** - all hidden inputs must be included
4. **Wrong parameter values** - `RememberMe` must be literal `"false"`, not boolean
5. **Changing request order** - login must complete before data requests
6. **Modifying hardcoded company ID** - Ventopay requires exact UUID
7. **Adding custom User-Agent** - application uses default .NET UA for scraping
8. **Rate limiting/delays** - there is intentionally NO throttling; adding delays may cause session timeout
9. **Changing edit mode logic** - order cancellation requires exact form state management

---

## Architecture

- **DI**: All services registered in `MauiProgram.cs` via `IServiceCollection`
- **MVVM**: Uses CommunityToolkit.Mvvm for ViewModels
- **Navigation**: Shell-based tab navigation (Menus, Orders, Billing, Settings)
- **Updates**: Velopack on desktop (Windows/Mac), no-op on mobile
- **Settings**: JSON file in app data directory via `GourmetSettingsService`
- **Caching**: Menu data cached to JSON file with configurable validity (default 4 hours)

## Build & Run

```bash
dotnet build src/GourmetClient.sln

# Mac Catalyst
dotnet build src/GourmetClient.Maui/GourmetClient.Maui.csproj -f net10.0-maccatalyst

# Android
dotnet build src/GourmetClient.Maui/GourmetClient.Maui.csproj -f net10.0-android
```

## Dependencies

- HtmlAgilityPack 1.12.x - HTML parsing
- CommunityToolkit.Mvvm 8.x - MVVM toolkit
- Semver 3.x - Version comparison
- Velopack 0.x - Desktop auto-updates (Windows/Mac only)
- Microsoft.Extensions.Logging.Debug 10.x

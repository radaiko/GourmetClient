# GourmetClient

Windows WPF application (.NET 9.0) that scrapes two external websites for company cafeteria menu and billing data.

## Critical Warning

**DO NOT ALTER THE WEB SCRAPING LOGIC.** The application performs website scraping (not API calls). Any deviation from the exact request sequence, headers, timing, or parameter values will trigger account blocks on the external services.

## Project Structure

```
src/GourmetClient/
├── Network/                    # ALL WEB SCRAPING LOGIC HERE
│   ├── WebClientBase.cs        # Base HTTP client, session management
│   ├── GourmetWebClient.cs     # Gourmet system scraping
│   ├── VentopayWebClient.cs    # Ventopay system scraping
│   ├── LoginHandle.cs          # Session lifecycle management
│   ├── GourmetCacheService.cs  # Menu/order cache
│   ├── BillingCacheService.cs  # Billing data aggregation
│   └── GourmetApi/             # JSON request/response models
├── Utils/
│   └── HttpClientHelper.cs     # HTTP client creation, proxy handling
├── Settings/
│   └── UserSettings.cs         # Credentials storage
└── Update/
    └── UpdateService.cs        # GitHub release checking
```

## Two External Data Sources

### 1. Gourmet System (Menu & Orders)

- **Base URL**: `https://alaclickneu.gourmet.at/`
- **Client**: `Network/GourmetWebClient.cs`
- **Purpose**: Menu data, order management, billing

### 2. Ventopay System (Billing)

- **Base URL**: `https://my.ventopay.com/mocca.website/`
- **Client**: `Network/VentopayWebClient.cs`
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
| Menu ID | `data-id` attribute |
| Day | `data-date` attribute (format: `MM-dd-yyyy`, e.g., `06-30-2025`) |
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
| Eating Cycle ID | `.//input[@name='cp_EatingCycleId_*']/@value` |
| Date | `.//input[@name='cp_Date_*']/@value` (format: `30.06.2025 00:00:00`) |
| Title | `.//div[@class='title']` text |
| Approved | Presence of class `confirmed` or `fa-check` |

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
      "menuIds": ["id1", "id2"]
    }
  ]
}
```

**Response**:
```json
{
  "success": true,
  "message": "string"
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
- XPath: `//div[@class='content']//div[@class='transact']`
- Extract transaction IDs from `id` attribute

**Transaction Details**: `https://my.ventopay.com/mocca.website/Rechnung.aspx?id={transactionId}`

| Field | Extraction Method |
|-------|-------------------|
| DateTime | `//span[@id='ContentPlaceHolder1_LblTimestamp']` |
| DateTime Format | Regex: `(\d+)\.\s+([a-zA-z]+)\s+(\d+)\s+-\s+(\d+):(\d+)` |
| Restaurant | `//span[@id='ContentPlaceHolder1_LblRestaurantInfo']` split by `<br>` |
| Items | `//div[@class='rechnungpart']//table//tbody` rows |

**Item Row Columns**:
- Column 0: Count (format: `2x`)
- Column 1: Item name
- Column 4: Cost (German format: `12,34`)

**Filter Rule**: Skip transactions where restaurant contains "Gourmet" AND location does NOT contain "Kaffeeautomat".

---

## Session & Cookie Management

### Critical Implementation Details

**Location**: `Network/WebClientBase.cs`

1. **Single CookieContainer per client instance** - cookies persist across all requests
2. **LoginHandle pattern** with reference counting - logout triggers when all handles disposed
3. **Thread-safe** via `SemaphoreSlim` and lock objects
4. **HttpClient is reused** - only recreated on network errors

### Proxy Handling

**Location**: `Utils/HttpClientHelper.cs`

1. Detect system proxy via `WebRequest.DefaultWebProxy`
2. Attempt without proxy first
3. On 407 error: retry with proxy + default credentials
4. On DNS error: retry without proxy

---

## Things That Will Break Accounts

1. **Missing CSRF tokens** - every form needs fresh `ufprt` (Gourmet) or `__VIEWSTATE` (Ventopay)
2. **Wrong date formats** - Gourmet uses `MM-dd-yyyy`, Ventopay uses `dd.MM.yyyy HH:mm:ss`
3. **Missing form parameters** - all hidden inputs must be included
4. **Wrong parameter values** - `RememberMe` must be literal `"false"`, not boolean
5. **Changing request order** - login must complete before data requests
6. **Modifying hardcoded company ID** - Ventopay requires exact UUID
7. **Adding custom User-Agent** - application uses default .NET UA for scraping
8. **Rate limiting/delays** - there is intentionally NO throttling; adding delays may cause session timeout
9. **Changing edit mode logic** - order cancellation requires exact form state management

---

## Cross-Platform Migration Notes

### Network Layer (Safe to Port)

The `Network/` folder contains platform-agnostic HTTP logic using `HttpClient`. This can be ported to:
- .NET MAUI (shared code)
- Xamarin (shared code)
- Native (requires reimplementation)

### HTML Parsing

Uses HtmlAgilityPack. Cross-platform alternatives:
- .NET MAUI/Xamarin: HtmlAgilityPack works
- Native iOS: use NSXMLParser or SwiftSoup
- Native Android: use Jsoup

### Must Preserve Exactly

1. All XPath selectors
2. All regex patterns (compiled as `[GeneratedRegex]`)
3. Form parameter names and values
4. URL paths and query parameters
5. Date format strings
6. Cookie handling behavior
7. Login/logout sequence

---

## Build & Run

```bash
dotnet build src/GourmetClient/GourmetClient.csproj
dotnet run --project src/GourmetClient/GourmetClient.csproj
```

## Dependencies

- HtmlAgilityPack 1.12.2 - HTML parsing
- Microsoft.Extensions.Primitives 9.0.7
- Microsoft.Xaml.Behaviors.Wpf 1.1.135 - WPF behaviors
- Semver 3.0.0 - Version comparison

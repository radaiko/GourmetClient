# GourmetClient

Expo React Native app that scrapes two external websites for company cafeteria menu and billing data.

## README Requirements

The README must always include a credit line linking to https://github.com/patrickl92/GourmetClient as the base/original project this was forked from.

## Critical Warning

**DO NOT ALTER THE WEB SCRAPING LOGIC.** The application performs website scraping (not API calls). Any deviation from the exact request sequence, headers, timing, or parameter values will trigger account blocks on the external services.

## Project Structure

```
src/app/                          # Expo React Native app (mobile)
├── app/                          # Expo Router screens
│   ├── (tabs)/                   # Tab navigation (Menus, Orders, Billing, Settings)
│   └── _layout.tsx               # Root layout
├── src-rn/
│   ├── api/                      # Web scraping layer
│   │   ├── gourmetClient.ts      # Gourmet HTTP client (cookie-managed Axios)
│   │   ├── gourmetParser.ts      # Cheerio-based HTML parsing
│   │   ├── gourmetApi.ts         # High-level Gourmet operations
│   │   ├── ventopayClient.ts     # Ventopay HTTP client
│   │   ├── ventopayParser.ts     # Ventopay HTML parsing
│   │   └── ventopayApi.ts        # High-level Ventopay operations
│   ├── store/                    # Zustand stores
│   │   ├── authStore.ts          # Gourmet auth state
│   │   ├── ventopayAuthStore.ts  # Ventopay auth state
│   │   ├── menuStore.ts          # Menu data + caching
│   │   ├── orderStore.ts         # Order management
│   │   └── billingStore.ts       # Billing from both sources
│   ├── components/               # UI components
│   ├── hooks/                    # Custom React hooks
│   ├── theme/                    # Theming
│   ├── types/                    # TypeScript types
│   └── utils/
│       ├── constants.ts          # All URLs and config
│       └── dateUtils.ts          # Date formatting helpers
├── package.json
└── tsconfig.json
analysis/                         # Playwright findings document
```

## Tech Stack

- Expo SDK 54, React Native 0.81.5, React 19.1.0
- Expo Router (file-based navigation with tabs)
- Zustand (state management)
- Cheerio (HTML parsing)
- Axios + tough-cookie (HTTP client with cookie jar)
- expo-secure-store (credential storage)
- TypeScript 5.9

## Two External Data Sources

### 1. Gourmet System (Menu & Orders)

- **Base URL**: `https://alaclickneu.gourmet.at/`
- **Client**: `src-rn/api/gourmetClient.ts`
- **Purpose**: Menu data, order management, billing

### 2. Ventopay System (Billing)

- **Base URL**: `https://my.ventopay.com/mocca.website/`
- **Client**: `src-rn/api/ventopayClient.ts`
- **Hardcoded Company ID**: `0da8d3ec-0178-47d5-9ccd-a996f04acb61`
- **Purpose**: Transaction/billing data from cafeteria POS

---

## Gourmet Web Scraping Specification

### Authentication Flow

**DO NOT MODIFY THIS SEQUENCE:**

1. **GET** `https://alaclickneu.gourmet.at/start/`
2. **Extract** hidden fields `ufprt` AND `__ncforminfo` from the form
3. **POST** `https://alaclickneu.gourmet.at/start/` with form data:
   ```
   Username: {username}
   Password: {password}
   RememberMe: false       # MUST be "false" (string)
   ufprt: {csrf_token}
   __ncforminfo: {extracted}
   ```
4. **Verify login** by checking for regex pattern:
   ```regex
   <a href="https://alaclickneu.gourmet.at/einstellungen/" class="navbar-link">
   ```

### Critical: `__ncforminfo` Field

Every form on the Gourmet site includes both `ufprt` AND `__ncforminfo` hidden fields. **Both must be extracted and sent with every form POST.** Missing `__ncforminfo` is detected as bot behavior and triggers account bans.

### Critical: `multipart/form-data` Encoding

Every form on the Gourmet site uses `enctype="multipart/form-data"`. **All form POSTs must use `multipart/form-data`**, not `application/x-www-form-urlencoded`. Sending URL-encoded data causes the server to silently reject the login (returns HTTP 200 with login page instead of 302 redirect).

### User Information Extraction

After login, extract from the page:

| Field | Selector |
|-------|----------|
| Username | `.loginname` text |
| ShopModelId | `#shopModel` value |
| EaterId | `#eater` value |
| StaffGroupId | `#staffGroup` value |

### Menu Data Extraction

**Endpoint**: `https://alaclickneu.gourmet.at/menus?page={0-9}`

**Pagination**: Loop pages 0-9, stop when no `a.menues-next` link exists.

**Menu Item Selector**: `div.meal`

| Field | Extraction Method |
|-------|-------------------|
| Menu ID | `.open_info.menu-article-detail` `data-id` attribute |
| Day | `.open_info.menu-article-detail` `data-date` attribute (format: `MM-dd-yyyy`) |
| Title | `.title` first child text |
| Subtitle | `.subtitle` text |
| Allergens | `li.allergen` text (single element, comma-separated letters) |
| Available | Presence of `input[type=checkbox].menu-clicked` |

**Category Detection** (regex on title):
```regex
MENÜ\s+([I]{1,3})    # Matches MENÜ I, MENÜ II, MENÜ III
SUPPE & SALAT        # Literal match
```

**Key**: Menu IDs are per-category, not per-item. All MENÜ I items share one ID.

### Order Operations

**Ordered Menus Endpoint**: `https://alaclickneu.gourmet.at/bestellungen`

**Order Item Selector**: `div.order-item`

| Field | Extraction Method |
|-------|-------------------|
| Position ID | `input[name=cp_PositionId]` value |
| Eating Cycle ID | `input[name=cp_EatingCycleId_{positionId}]` value |
| Date | `input[name=cp_Date_{positionId}]` value (format: `dd.MM.yyyy HH:mm:ss`) |
| Title | `.title` text |
| Approved | Presence of `.confirmed` class or `fa fa-check` icon |

**Forms on orders page** (all POST to `/bestellungen/`):
1. Main order form: `ufprt` + `__ncforminfo`
2. Cancel form: `ufprt` + `__ncforminfo`
3. Edit mode toggle (class `form-toggleEditMode`): `editMode=True` + `ufprt` + `__ncforminfo`

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
  "checkLastMonthNumber": "0"
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
- Selector: `div.content div.transact`
- Extract transaction IDs from `id` attribute

**Transaction Details**: `https://my.ventopay.com/mocca.website/Rechnung.aspx?id={transactionId}`

| Field | Extraction Method |
|-------|-------------------|
| DateTime | `#ContentPlaceHolder1_LblTimestamp` text |
| DateTime Format | Regex: `(\d+)\.\s+([a-zA-z]+)\s+(\d+)\s+-\s+(\d+):(\d+)` |
| Restaurant | `#ContentPlaceHolder1_LblRestaurantInfo` split by `<br>` |
| Items | `div.rechnungpart table tbody` rows (excluding `rechnungsdetail` rows) |

**Item Row Columns**:
- Column 0: Count (format: `2x`)
- Column 1: Item name
- Column 4: Cost (German format: `12,34`)

**Filter Rule**: Skip transactions where restaurant name contains "Gourmet" AND location does NOT contain "Kaffeeautomat".

---

## Session & Cookie Management

- **tough-cookie** with **axios-cookiejar-support** for automatic cookie persistence
- Each client (`gourmetClient.ts`, `ventopayClient.ts`) maintains its own cookie jar
- Cookies persist across all requests within a session

---

## Things That Will Break Accounts

1. **Missing `__ncforminfo`** - every Gourmet form needs both `ufprt` AND `__ncforminfo`
2. **Wrong Content-Type** - Gourmet forms require `multipart/form-data`, NOT `application/x-www-form-urlencoded`
3. **Missing CSRF tokens** - fresh `ufprt` (Gourmet) or `__VIEWSTATE` (Ventopay) per request
3. **Wrong date formats** - Gourmet uses `MM-dd-yyyy`, Ventopay uses `dd.MM.yyyy`
4. **Missing form parameters** - all hidden inputs must be included
5. **Wrong parameter values** - `RememberMe` must be literal `"false"`, not boolean
6. **Changing request order** - login must complete before data requests
7. **Modifying hardcoded company ID** - Ventopay requires exact UUID
8. **Rate limiting/delays** - there is intentionally NO throttling; adding delays may cause session timeout
9. **Changing edit mode logic** - order cancellation requires exact form state management

---

## Build & Run

```bash
cd src/app
npm install

# iOS
npx expo run:ios

# Android
npx expo run:android

# Dev server
npx expo start
```

# Playwright Website Analysis - Gourmet System

**Date**: 2026-02-10
**URL**: https://alaclickneu.gourmet.at/

## Critical Finding: Missing `__ncforminfo` Field

The existing .NET MAUI app **does NOT send the `__ncforminfo` hidden field** that exists on every form. This is almost certainly why accounts get banned - the server detects incomplete form submissions as bot behavior.

Every form on the site includes both `ufprt` AND `__ncforminfo` hidden fields. Both must be extracted and sent.

---

## Login Page (`/start/`)

### Form Structure
- **Two forms** on the page, both POST to `/start/`
- **Form 1 (Login)**: Main login form
- **Form 2 (Forgot Password)**: Password reset form

### Login Form Fields
| Field | Type | Notes |
|-------|------|-------|
| `Username` | text | placeholder: "Benutzername" |
| `Password` | password | Has show/hide toggle button |
| `RememberMe` | checkbox | value="true" when checked |
| `RememberMe` | hidden | value="false" (ASP.NET MVC pattern) |
| `ufprt` | hidden | CSRF token, changes per page load |
| `__ncforminfo` | hidden | **NEW - not in CLAUDE.md spec!** Changes per page load |

### Login POST Behavior
- POST to `/start/` (form action is `/start/`)
- On success: redirects to `/menus/` (HTTP 302)
- Cookies are set during redirect

### Login Verification
- Check for: `<a href="https://alaclickneu.gourmet.at/einstellungen/" class="navbar-link">`
- This appears in the header as: `Willkommen, <span class="loginname">{username}</span>`

---

## Menu Page (`/menus/`)

### User Info Extraction
All found as hidden inputs on the menus page:
| Field | Selector | Example Value |
|-------|----------|---------------|
| shopModelId | `#shopModel` | `{uuid}` |
| eaterId | `#eater` | `{uuid}` |
| staffGroupId | `#staffGroup` | `{uuid}` |
| loginName | `.loginname` | `{username}` |
| location | `.userfield .location-seperator` | `Standort: {company name}, {company id}` |

### Page Layout
```
section.content
  > div.row-wrapper
    > div.row.hide-sm-down          (desktop layout)
      > div.menu-row                (one per category: MENÜ I, II, III, SUPPE & SALAT)
        > div.menu-row-item         (one per day, 4 days shown)
          > div.meal                (single menu item)
    > div.row.hide-sm-up            (mobile layout - flat list of all 16 meals)
```

### Menu Item Structure (`.meal`)
```html
<div class="meal">
  <div class="open_info menu-article-detail" data-id="{menuId}" data-date="MM-dd-yyyy">
    <div class="title">
      MENÜ I
      <div class="subtitle">Spaghetti Carbonara</div>
    </div>
  </div>
  <ul class="allergens">
    <li class="allergen">A, G, O</li>  <!-- single li, comma-separated -->
  </ul>
  <div class="bottom-line">
    <div class="price"><span>6,00 EUR</span></div>
    <div class="buttons">
      <button class="btn btn-border btn-thin menu-article-detail" data-id="{menuId}" data-date="MM-dd-yyyy">
        <i class="fa fa-info"></i>
      </button>
      <!-- Only present if orderable: -->
      <input type="checkbox" id="{menuId}{date}-desktop" class="menu-clicked" data-id="{menuId}" data-date="MM-dd-yyyy">
      <label data-id="{menuId}" data-date="MM-dd-yyyy" class="menu-clicked-label-desktop"></label>
    </div>
  </div>
</div>
```

### Key Observations
1. **Menu ID (`data-id`)**: Same for all items in a category (e.g., all MENÜ I share one ID)
2. **Date format**: `MM-dd-yyyy` (confirmed: "02-10-2026")
3. **Allergens**: Single `<li class="allergen">` with comma-separated letters (e.g., "A, G, O")
4. **Availability**: Determined by presence of `<input type="checkbox" class="menu-clicked">`
5. **Price**: In `<div class="price"><span>6,00 EUR</span></div>` (6.00 for menus, 2.50 for soup & salad)
6. **Category detection**: Title text starts with "MENÜ I", "MENÜ II", "MENÜ III", or "SUPPE & SALAT"

### Pagination
- "Nächste Seite" link with class containing `menues-next`
- URL pattern: `/menus/?page=1`, `/menus/?page=2`, etc.
- Page 0 is the default (no `page` parameter needed)

### Submit Order Button
```html
<a class="btn btn-primary add-to-menues-cart disabled">Bestellen</a>
```
- Starts disabled, enabled via JavaScript when items are checked

### Notifications API
- Called automatically after page load via AngularJS
- Endpoint: `POST /umbraco/api/AlaEaterNotificationsApi/GetNotifications`
- Initialized with: `vm.init('shopModelId', 'eaterId')`

---

## Orders Page (`/bestellungen/`)

### Page Structure
- Header: "Meine Bestellung"
- Subheader: "Wählen Sie bitte Ihre Essenszeit. Guten Appetit!"
- Columns: Tag (Day), Speisen (Food), Preis (Price)
- Summary: Summe (Total) and Ihr Preis (Your Price)

### Forms (3 total, all POST to `/bestellungen/`)
1. **Main order form**: `ufprt` + `__ncforminfo`
2. **Cancel form**: `ufprt` + `__ncforminfo`
3. **Edit mode toggle**: class `form-toggleEditMode`
   - Fields: `editMode` (value: "True"), `ufprt`, `__ncforminfo`

### Actions
- "Weitere Speisen" link -> `/start/`
- "Bestellung ändern" button -> triggers edit mode form

---

## Discrepancies vs CLAUDE.md Spec

| Item | CLAUDE.md | Actual |
|------|-----------|--------|
| `__ncforminfo` field | NOT mentioned | Present on ALL forms, MUST be sent |
| Allergens structure | `//li[@class='allergen']` (multiple) | Single `<li>` with comma-separated text |
| Price field | Not mentioned | `<div class="price"><span>6,00 EUR</span></div>` |
| Checkbox ID | Not documented | `{menuId}{date}-desktop` format |
| Notifications API | Not mentioned | `GetNotifications` called after login |
| Login POST URL | `https://alaclickneu.gourmet.at/` | Form action is `/start/` |
| Logout | Not fully documented | Button click triggers form POST with `ufprt` + `__ncforminfo` |
| Form action | POST to base URL | POST to `/start/` for login |
| Edit mode form | `form_toggleEditMode` (id) | `form-toggleEditMode` (class) |

---

## Recommendations for React Native Implementation

1. **Always extract and send `__ncforminfo`** along with `ufprt` on every form POST
2. **Use exact form action URLs** (`/start/` not `/`)
3. **Parse allergens** as single string from `<li class="allergen">`, split by comma
4. **Extract price** from `.price span` text
5. **Check checkbox presence** for availability, not date-based logic
6. **Handle the cookie consent dialog** - may need to dismiss or ignore

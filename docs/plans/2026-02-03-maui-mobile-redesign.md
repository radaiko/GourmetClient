# GourmetClient MAUI Mobile Redesign

## Overview

Port the WPF GourmetClient to .NET MAUI with a mobile-first UI redesign optimized for iOS and Android.

## Navigation Structure

**Bottom Tab Bar** with 4 tabs:
- **Menus** (home) - Browse available menus by day
- **Orders** - View current/past orders
- **Billing** - Transaction history from Ventopay
- **Settings** - Login credentials, app info

```
AppShell
├── MenusPage (Tab 1 - default)
├── OrdersPage (Tab 2)
├── BillingPage (Tab 3)
└── SettingsPage (Tab 4)
```

## Menus Page

**Layout:**
- Top bar: Current date with left/right arrows
- Swipeable content: CarouselView, one day per page
- FAB: Bottom-right, visible when pending changes exist

**Day Content:**
- Vertical list of menu items grouped by category
- Categories: "Menü I", "Menü II", "Menü III", "Suppe & Salat"
- Menu card: title, subtitle, allergens, state indicator
- Tap to toggle order/cancel state

**Menu Item States:**
- Available (not ordered) - can mark for order
- Ordered (confirmed) - can mark for cancel
- Pending order (marked, not submitted)
- Pending cancel (marked, not submitted)
- Not available - disabled

## Orders Page

**Layout:**
- Top bar: "My Orders"
- Filter: "Upcoming" | "Past" segmented control
- Content: Ordered menus grouped by date

**Order Card:**
- Date header
- Menu title and category
- Status badge: Confirmed / Pending approval
- Swipe-to-cancel for upcoming orders

## Billing Page

**Layout:**
- Top bar: "Billing" with month picker
- Grouped transaction summary (collapsible sections)

**Groups:**
1. Food - meal purchases
2. Drinks - beverage purchases
3. Other - unclassified

**Each item:** Count, name, unit price, total

## Settings Page

**Sections:**
1. Gourmet Account (username, password, status)
2. Ventopay Account (username, password, status)
3. App (updates, version)
4. About (credits)

## Shared Components

**FAB:** Shared across Menus/Orders, shows pending count, opens confirmation sheet.

**Services:** Reuse Network/ layer unchanged from WPF project.

## Technical Notes

- Reuse existing Network/ classes (web scraping logic must not change)
- Share Model/ classes where possible
- MVVM pattern with services as singletons

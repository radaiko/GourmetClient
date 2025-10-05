# iOS UI Visual Layout

## Main View Structure

```
┌─────────────────────────────────────────┐
│  Top Bar (Dynamic)                      │
│  ┌───────────────────────────────────┐  │
│  │ Gourmet              ⟳            │  │  <- Page 0: Menu
│  │ username                           │  │
│  └───────────────────────────────────┘  │
├─────────────────────────────────────────┤
│  Error Panel (if error exists)          │
│  ┌───────────────────────────────────┐  │
│  │ ⚠ Error message here          ✕  │  │
│  └───────────────────────────────────┘  │
├─────────────────────────────────────────┤
│                                         │
│  Main Content Area                      │
│  (Swipeable between pages)              │
│                                         │
│  ┌───────────────────────────────────┐ │
│  │                                   │ │
│  │   Page Content Here               │ │
│  │   (Menu/Billing/Settings/About)   │ │
│  │                                   │ │
│  └───────────────────────────────────┘ │
│                                         │
├─────────────────────────────────────────┤
│  Page Indicators                        │
│        ━━━━  ━  ━  ━                   │  <- Active page (wider)
│         0   1  2  3                     │
└─────────────────────────────────────────┘
```

## Page 0: Menu View

```
┌─────────────────────────────────────────┐
│  🍽️ Willkommen                          │
│                                         │
│  Bitte konfigurieren Sie Ihre          │
│  Anmeldedaten in den Einstellungen,    │
│  um Menüs anzuzeigen.                  │
│                                         │
└─────────────────────────────────────────┘
```

## Page 1: Billing View

```
┌─────────────────────────────────────────┐
│  Rechnung                               │
│  Ihre Abrechnungsinformationen          │
│                                         │
│  ┌───────────────────────────────────┐ │
│  │           💳                       │ │
│  │  Keine Abrechnungsdaten verfügbar │ │
│  │                                   │ │
│  │  Bitte konfigurieren Sie Ihre     │ │
│  │  VentoPay-Anmeldedaten in den     │ │
│  │  Einstellungen.                   │ │
│  └───────────────────────────────────┘ │
└─────────────────────────────────────────┘
```

## Page 2: Settings View

```
┌─────────────────────────────────────────┐
│  Einstellungen                          │
│  Konfiguration der Anmeldedaten und     │
│  Anwendungseinstellungen                │
│                                         │
│  ┌───────────────────────────────────┐ │
│  │ Gourmet Anmeldedaten              │ │
│  │ ┌───────────────────────────────┐ │ │
│  │ │ Benutzername                  │ │ │
│  │ └───────────────────────────────┘ │ │
│  │ ┌───────────────────────────────┐ │ │
│  │ │ Passwort                      │ │ │
│  │ └───────────────────────────────┘ │ │
│  └───────────────────────────────────┘ │
│                                         │
│  ┌───────────────────────────────────┐ │
│  │ VentoPay Anmeldedaten             │ │
│  │ ┌───────────────────────────────┐ │ │
│  │ │ Benutzername                  │ │ │
│  │ └───────────────────────────────┘ │ │
│  │ ┌───────────────────────────────┐ │ │
│  │ │ Passwort                      │ │ │
│  │ └───────────────────────────────┘ │ │
│  └───────────────────────────────────┘ │
│                                         │
│  ┌───────────────────────────────────┐ │
│  │ Anwendungseinstellungen           │ │
│  │ ☐ Automatische Updates            │ │
│  │ ☐ Mit Windows starten             │ │
│  └───────────────────────────────────┘ │
└─────────────────────────────────────────┘
```

## Page 3: About View

```
┌─────────────────────────────────────────┐
│  Version: 1.0.0                         │
│  [Versionsinformationen]                │
│                                         │
│  Dieses Programm wird ohne Garantie     │
│  ausgeliefert. Verwendung auf eigene    │
│  Verantwortung.                         │
│                                         │
│  Icons erstellt von:                    │
│  [Freepik]                              │
│  von                                    │
│  [www.flaticon.com]                     │
└─────────────────────────────────────────┘
```

## Gesture Navigation

```
Page 0 (Menu)
    │
    ├──> Swipe Left ──> Page 1 (Billing)
    │                       │
    │                       ├──> Swipe Left ──> Page 2 (Settings)
    │                       │                       │
    │                       │                       ├──> Swipe Left ──> Page 3 (About)
    │                       │                       │                       │
    │                       │                       ←── Swipe Right ───────┘
    │                       │                       │
    │                       ←── Swipe Right ────────┘
    │                       │
    ←── Swipe Right ────────┘
```

## Color Scheme

### Light Mode
```
Background:       #F2F2F7 (Light Gray)
Card Background:  #FFFFFF (White)
Primary Text:     #000000 (Black)
Secondary Text:   #8E8E93 (Gray)
Accent:           #007AFF (iOS Blue)
Error:            #FF3B30 (Red)
```

### Dark Mode
```
Background:       #000000 (Black)
Card Background:  #1C1C1E (Dark Gray)
Primary Text:     #FFFFFF (White)
Secondary Text:   #8E8E93 (Gray)
Accent:           #007AFF (iOS Blue)
Error:            #FF3B30 (Red)
```

## Interactive Elements

### Page Indicators (Bottom)
```
Inactive: ━  (8px × 8px circle)
Active:   ━━━━  (18px × 8px rounded rectangle)
Color:    #8E8E93 (inactive) / #007AFF (active)
```

### Top Bar Buttons
```
Size:     44px × 44px (minimum touch target)
Icons:    ⟳ (refresh), ⎙ (save), ✓ (order)
Style:    Transparent background, colored text
```

### Error Banner
```
Background: rgba(#FF3B30, 0.15)
Border:     2px bottom, #FF3B30
Text:       #FF3B30
Button:     ✕ (close)
```

## Responsive Behavior

- **Portrait**: All views stack vertically with scroll
- **Landscape**: Same layout (mobile-first design)
- **Card Widths**: Auto-expand to screen width minus margins
- **Touch Targets**: Minimum 44pt × 44pt for all interactive elements
- **Scroll**: Vertical scroll within each page, horizontal swipe between pages

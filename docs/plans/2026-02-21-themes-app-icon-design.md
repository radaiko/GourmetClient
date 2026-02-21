# Design: 5 Accent Themes with Dynamic App Icons

**Issue**: #12
**Date**: 2026-02-21

## Summary

Add 5 selectable accent color themes to the Settings screen. Each theme changes the app-wide accent color and dynamically switches the app icon to match. Light/dark mode remains a separate, independent toggle.

## Accent Colors (Food-Inspired Palette)

| Theme Name | ID | Light Accent | Dark Accent | Description |
|------------|-----|-------------|-------------|-------------|
| Orange | `orange` | `#D4501A` | `#FF6B35` | Current default, warm cafeteria orange |
| Emerald | `emerald` | `#2E7D4F` | `#4CAF7D` | Fresh, leafy green |
| Berry | `berry` | `#A62547` | `#E04868` | Rich berry red |
| Golden | `golden` | `#C08B1A` | `#E8B03E` | Warm golden yellow |
| Ocean | `ocean` | `#2563A8` | `#4A90D9` | Deep calming blue |

Each accent color also derives:
- `primaryDark`: Darker variant for pressed states (light mode)
- `primarySurface`: Tinted background for selected states
- `glassPrimary`: Translucent variant for glass effects

## App Icon Design

- **Background**: System-adaptive (white/light in light mode, dark in dark mode) — supports iOS 18+ light/dark/tinted variants
- **Foreground**: Crossed fork & knife rendered in the theme's accent color
- **Format**: 1024x1024 PNG per variant
- **Variants**: 5 icons (one per theme), each with iOS + Android foreground assets

### Icon Switching

- **Library**: `@g9k/expo-dynamic-app-icon` (supports Expo SDK 54)
- **iOS**: Immediate switch via `setAlternateIconName`. System shows a brief "You have changed the icon" alert
- **Android**: Activity-alias mechanism. Switch happens silently using the DEFAULT-state workaround
- **Desktop/Web**: No icon switching (not applicable)

## Settings UI

The existing "Darstellung" (Appearance) section gains a new **Akzentfarbe** (Accent Color) sub-section below the light/dark/system toggle.

### Accent Color Picker

- Row of 5 colored circles (32px diameter)
- Each circle filled with the theme's light-mode accent color
- Selected circle: checkmark overlay + ring border in the accent color
- Tapping a circle immediately updates the accent and app icon

## Architecture

### State Management

Extend `themeStore.ts`:
- New `accentColor` property: `'orange' | 'emerald' | 'berry' | 'golden' | 'ocean'`
- Default: `'orange'`
- Persisted via AsyncStorage (same as existing `themePreference`)
- `setAccentColor(color)` action that updates state + calls `setAppIcon()`

### Color Generation

Extend `colors.ts`:
- `AccentColorConfig` map: accent ID → `{ light, dark, primaryDark, primarySurface, glassPrimary }` for both light and dark modes
- `getColorsForAccent(accent, isDark)` function that returns the full `Colors` object with the accent's primary colors swapped in
- `LightColors` and `DarkColors` become base templates; accent values override the primary-related fields

### Theme Hook

Update `useTheme.ts`:
- Read `accentColor` from `themeStore`
- Pass it to `getColorsForAccent()` to compute the final color set
- Return value stays the same `{ colors, isDark, colorScheme }`

### Dynamic Icon Integration

In `themeStore.ts` `setAccentColor()`:
```typescript
import { setAppIcon } from '@g9k/expo-dynamic-app-icon';

setAccentColor: (color) => {
  set({ accentColor: color });
  if (Platform.OS !== 'web') {
    setAppIcon(color === 'orange' ? null : color);
  }
}
```

## Files Changed

1. **`src/app/src-rn/theme/colors.ts`** — Accent color configs + `getColorsForAccent()` function
2. **`src/app/src-rn/store/themeStore.ts`** — Add `accentColor` state, `setAccentColor` action, icon switching
3. **`src/app/src-rn/theme/useTheme.ts`** — Use accent color when computing colors
4. **`src/app/app/(tabs)/settings.tsx`** — Accent color picker UI in Darstellung section
5. **`src/app/app.json`** — `@g9k/expo-dynamic-app-icon` plugin configuration with 4 alternate icons
6. **`src/app/assets/icons/`** — 5 icon variants (iOS PNGs + Android foreground PNGs)
7. **`src/app/src-rn/theme/platformStyles.ts`** — No changes needed (already uses `colors.primary` dynamically)

## Testing

- Unit tests for `getColorsForAccent()` — verify all 5 accents produce valid color sets
- Unit tests for `themeStore` — verify accent persistence and state changes
- Visual verification on iOS Simulator for each theme
- Verify icon switching works on iOS (system alert appears) and Android

## Out of Scope

- Desktop (Tauri) icon switching — not supported at runtime
- Custom user-defined colors — only the 5 predefined themes
- Per-screen accent overrides — accent is app-wide

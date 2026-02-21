# Accent Themes & Dynamic App Icons Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add 5 selectable accent color themes that change the app-wide primary color and dynamically switch the app icon to match.

**Architecture:** Extend the existing `themeStore` with an `accentColor` field persisted via AsyncStorage. A new `accentColors` config map in `colors.ts` maps each accent ID to its light/dark color variants. `useTheme()` merges the selected accent into the base light/dark palette. The `@g9k/expo-dynamic-app-icon` library handles runtime icon switching on iOS/Android.

**Tech Stack:** Expo SDK 54, Zustand, `@g9k/expo-dynamic-app-icon`, SVG (for icon generation), `sharp` (for PNG rendering)

---

### Task 1: Install `@g9k/expo-dynamic-app-icon`

**Files:**
- Modify: `src/app/package.json`

**Step 1: Install the package**

Run: `cd src/app && npx expo install @g9k/expo-dynamic-app-icon`

**Step 2: Verify installation**

Run: `cd src/app && node -e "require('@g9k/expo-dynamic-app-icon')"`
Expected: No error

**Step 3: Commit**

```bash
git add src/app/package.json src/app/package-lock.json
git commit -m "chore: install @g9k/expo-dynamic-app-icon (#12)"
```

---

### Task 2: Add accent color configuration to `colors.ts`

**Files:**
- Modify: `src/app/src-rn/theme/colors.ts`
- Create: `src/app/src-rn/__tests__/theme/colors.test.ts`

**Step 1: Write the failing test**

Create `src/app/src-rn/__tests__/theme/colors.test.ts`:

```typescript
import { ACCENT_COLORS, getColorsForAccent, AccentColorId, LightColors, DarkColors } from '../../theme/colors';

describe('ACCENT_COLORS', () => {
  it('has exactly 5 accent color entries', () => {
    expect(Object.keys(ACCENT_COLORS)).toHaveLength(5);
  });

  it('contains orange, emerald, berry, golden, ocean', () => {
    expect(ACCENT_COLORS).toHaveProperty('orange');
    expect(ACCENT_COLORS).toHaveProperty('emerald');
    expect(ACCENT_COLORS).toHaveProperty('berry');
    expect(ACCENT_COLORS).toHaveProperty('golden');
    expect(ACCENT_COLORS).toHaveProperty('ocean');
  });

  it('each accent has light and dark primary colors', () => {
    for (const id of Object.keys(ACCENT_COLORS) as AccentColorId[]) {
      const accent = ACCENT_COLORS[id];
      expect(accent.light.primary).toMatch(/^#[0-9A-Fa-f]{6}$/);
      expect(accent.dark.primary).toMatch(/^#[0-9A-Fa-f]{6}$/);
      expect(accent.light.primaryDark).toMatch(/^#[0-9A-Fa-f]{6}$/);
      expect(accent.light.primarySurface).toMatch(/^#[0-9A-Fa-f]{6}$/);
      expect(accent.dark.primarySurface).toMatch(/^#[0-9A-Fa-f]{6}$/);
    }
  });
});

describe('getColorsForAccent', () => {
  it('returns base LightColors for orange in light mode', () => {
    const colors = getColorsForAccent('orange', false);
    expect(colors.primary).toBe(LightColors.primary);
    expect(colors.primaryDark).toBe(LightColors.primaryDark);
    expect(colors.background).toBe(LightColors.background);
  });

  it('returns base DarkColors for orange in dark mode', () => {
    const colors = getColorsForAccent('orange', true);
    expect(colors.primary).toBe(DarkColors.primary);
    expect(colors.primaryDark).toBe(DarkColors.primaryDark);
    expect(colors.background).toBe(DarkColors.background);
  });

  it('overrides primary colors for emerald in light mode', () => {
    const colors = getColorsForAccent('emerald', false);
    expect(colors.primary).toBe(ACCENT_COLORS.emerald.light.primary);
    expect(colors.primaryDark).toBe(ACCENT_COLORS.emerald.light.primaryDark);
    expect(colors.primarySurface).toBe(ACCENT_COLORS.emerald.light.primarySurface);
    // Non-primary colors remain unchanged
    expect(colors.background).toBe(LightColors.background);
    expect(colors.success).toBe(LightColors.success);
  });

  it('overrides primary colors for berry in dark mode', () => {
    const colors = getColorsForAccent('berry', true);
    expect(colors.primary).toBe(ACCENT_COLORS.berry.dark.primary);
    expect(colors.primaryDark).toBe(ACCENT_COLORS.berry.dark.primaryDark);
    expect(colors.primarySurface).toBe(ACCENT_COLORS.berry.dark.primarySurface);
    // Non-primary colors remain unchanged
    expect(colors.background).toBe(DarkColors.background);
  });

  it('overrides glassPrimary for non-orange accents', () => {
    const colors = getColorsForAccent('ocean', false);
    expect(colors.glassPrimary).toBe(ACCENT_COLORS.ocean.light.glassPrimary);
    expect(colors.glassPrimary).not.toBe(LightColors.glassPrimary);
  });

  it('returns correct colors for all 5 accents', () => {
    const accents: AccentColorId[] = ['orange', 'emerald', 'berry', 'golden', 'ocean'];
    for (const accent of accents) {
      const light = getColorsForAccent(accent, false);
      const dark = getColorsForAccent(accent, true);
      expect(light.primary).toBe(ACCENT_COLORS[accent].light.primary);
      expect(dark.primary).toBe(ACCENT_COLORS[accent].dark.primary);
    }
  });
});
```

**Step 2: Run test to verify it fails**

Run: `cd src/app && npx jest src-rn/__tests__/theme/colors.test.ts --no-coverage`
Expected: FAIL — `ACCENT_COLORS` and `getColorsForAccent` not exported

**Step 3: Implement accent color config and `getColorsForAccent`**

Add to the bottom of `src/app/src-rn/theme/colors.ts`:

```typescript
export type AccentColorId = 'orange' | 'emerald' | 'berry' | 'golden' | 'ocean';

interface AccentVariant {
  primary: string;
  primaryDark: string;
  primarySurface: string;
  glassPrimary: string;
}

interface AccentConfig {
  light: AccentVariant;
  dark: AccentVariant;
  label: string;
}

export const ACCENT_COLORS: Record<AccentColorId, AccentConfig> = {
  orange: {
    light: {
      primary: '#D4501A',
      primaryDark: '#B84415',
      primarySurface: '#FFF1EB',
      glassPrimary: useFlatStyle ? '#FFF1EB' : 'rgba(212,80,26,0.08)',
    },
    dark: {
      primary: '#FF6B35',
      primaryDark: '#D4501A',
      primarySurface: '#2A1A10',
      glassPrimary: useFlatStyle ? '#2A1A10' : 'rgba(255,107,53,0.14)',
    },
    label: 'Orange',
  },
  emerald: {
    light: {
      primary: '#2E7D4F',
      primaryDark: '#236B3F',
      primarySurface: '#E8F5ED',
      glassPrimary: useFlatStyle ? '#E8F5ED' : 'rgba(46,125,79,0.08)',
    },
    dark: {
      primary: '#4CAF7D',
      primaryDark: '#2E7D4F',
      primarySurface: '#102A1A',
      glassPrimary: useFlatStyle ? '#102A1A' : 'rgba(76,175,125,0.14)',
    },
    label: 'Smaragd',
  },
  berry: {
    light: {
      primary: '#A62547',
      primaryDark: '#8C1E3B',
      primarySurface: '#FCEEF2',
      glassPrimary: useFlatStyle ? '#FCEEF2' : 'rgba(166,37,71,0.08)',
    },
    dark: {
      primary: '#E04868',
      primaryDark: '#A62547',
      primarySurface: '#2A1018',
      glassPrimary: useFlatStyle ? '#2A1018' : 'rgba(224,72,104,0.14)',
    },
    label: 'Beere',
  },
  golden: {
    light: {
      primary: '#C08B1A',
      primaryDark: '#A07415',
      primarySurface: '#FDF5E3',
      glassPrimary: useFlatStyle ? '#FDF5E3' : 'rgba(192,139,26,0.08)',
    },
    dark: {
      primary: '#E8B03E',
      primaryDark: '#C08B1A',
      primarySurface: '#2A2210',
      glassPrimary: useFlatStyle ? '#2A2210' : 'rgba(232,176,62,0.14)',
    },
    label: 'Gold',
  },
  ocean: {
    light: {
      primary: '#2563A8',
      primaryDark: '#1E528C',
      primarySurface: '#EBF2FC',
      glassPrimary: useFlatStyle ? '#EBF2FC' : 'rgba(37,99,168,0.08)',
    },
    dark: {
      primary: '#4A90D9',
      primaryDark: '#2563A8',
      primarySurface: '#101A2A',
      glassPrimary: useFlatStyle ? '#101A2A' : 'rgba(74,144,217,0.14)',
    },
    label: 'Ozean',
  },
};

export function getColorsForAccent(accent: AccentColorId, isDark: boolean): Colors {
  const base = isDark ? DarkColors : LightColors;
  const variant = ACCENT_COLORS[accent][isDark ? 'dark' : 'light'];
  return {
    ...base,
    primary: variant.primary,
    primaryDark: variant.primaryDark,
    primarySurface: variant.primarySurface,
    glassPrimary: variant.glassPrimary,
  };
}
```

**Step 4: Run test to verify it passes**

Run: `cd src/app && npx jest src-rn/__tests__/theme/colors.test.ts --no-coverage`
Expected: All PASS

**Step 5: Run full test suite to check no regressions**

Run: `cd src/app && npm test`
Expected: All 178+ tests pass

**Step 6: Commit**

```bash
git add src/app/src-rn/theme/colors.ts src/app/src-rn/__tests__/theme/colors.test.ts
git commit -m "feat: add accent color configuration with 5 theme variants (#12)"
```

---

### Task 3: Extend `themeStore` with accent color state

**Files:**
- Modify: `src/app/src-rn/store/themeStore.ts`
- Create: `src/app/src-rn/__tests__/store/themeStore.test.ts`

**Step 1: Write the failing test**

Create `src/app/src-rn/__tests__/store/themeStore.test.ts`:

```typescript
import { useThemeStore } from '../../store/themeStore';

// Reset store between tests
beforeEach(() => {
  useThemeStore.setState({ preference: 'system', accentColor: 'orange' });
});

describe('themeStore', () => {
  it('has default accent color of orange', () => {
    expect(useThemeStore.getState().accentColor).toBe('orange');
  });

  it('setAccentColor updates the accent color', () => {
    useThemeStore.getState().setAccentColor('emerald');
    expect(useThemeStore.getState().accentColor).toBe('emerald');
  });

  it('setAccentColor works for all accent values', () => {
    const accents = ['orange', 'emerald', 'berry', 'golden', 'ocean'] as const;
    for (const accent of accents) {
      useThemeStore.getState().setAccentColor(accent);
      expect(useThemeStore.getState().accentColor).toBe(accent);
    }
  });

  it('preserves preference when changing accent', () => {
    useThemeStore.getState().setPreference('dark');
    useThemeStore.getState().setAccentColor('berry');
    expect(useThemeStore.getState().preference).toBe('dark');
    expect(useThemeStore.getState().accentColor).toBe('berry');
  });
});
```

**Step 2: Run test to verify it fails**

Run: `cd src/app && npx jest src-rn/__tests__/store/themeStore.test.ts --no-coverage`
Expected: FAIL — `accentColor` property does not exist

**Step 3: Update themeStore**

Replace `src/app/src-rn/store/themeStore.ts` with:

```typescript
import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { Platform } from 'react-native';
import type { AccentColorId } from '../theme/colors';

export type ThemePreference = 'system' | 'light' | 'dark';

interface ThemeState {
  preference: ThemePreference;
  accentColor: AccentColorId;
  setPreference: (preference: ThemePreference) => void;
  setAccentColor: (color: AccentColorId) => void;
}

export const useThemeStore = create<ThemeState>()(
  persist(
    (set) => ({
      preference: 'system',
      accentColor: 'orange',
      setPreference: (preference) => set({ preference }),
      setAccentColor: (color) => {
        set({ accentColor: color });
        if (Platform.OS !== 'web') {
          try {
            const { setAppIcon } = require('@g9k/expo-dynamic-app-icon');
            setAppIcon(color === 'orange' ? null : color);
          } catch {
            // Icon switching unavailable (web/desktop or library not linked)
          }
        }
      },
    }),
    {
      name: 'theme-preference',
      storage: createJSONStorage(() => AsyncStorage),
    }
  )
);
```

**Step 4: Run test to verify it passes**

Run: `cd src/app && npx jest src-rn/__tests__/store/themeStore.test.ts --no-coverage`
Expected: All PASS

**Step 5: Run full test suite**

Run: `cd src/app && npm test`
Expected: All tests pass

**Step 6: Commit**

```bash
git add src/app/src-rn/store/themeStore.ts src/app/src-rn/__tests__/store/themeStore.test.ts
git commit -m "feat: add accent color state to themeStore (#12)"
```

---

### Task 4: Update `useTheme` hook to use accent colors

**Files:**
- Modify: `src/app/src-rn/theme/useTheme.ts`

**Step 1: Update the hook**

Replace `src/app/src-rn/theme/useTheme.ts` with:

```typescript
import { useMemo } from 'react';
import { useColorScheme } from 'react-native';
import { useThemeStore } from '../store/themeStore';
import { Colors, getColorsForAccent } from './colors';

interface ThemeResult {
  colors: Colors;
  isDark: boolean;
  colorScheme: 'light' | 'dark';
}

export function useTheme(): ThemeResult {
  const systemScheme = useColorScheme();
  const preference = useThemeStore((s) => s.preference);
  const accentColor = useThemeStore((s) => s.accentColor);

  const colorScheme =
    preference === 'system' ? (systemScheme ?? 'light') : preference;

  const isDark = colorScheme === 'dark';
  const colors = useMemo(
    () => getColorsForAccent(accentColor, isDark),
    [accentColor, isDark]
  );

  return { colors, isDark, colorScheme };
}
```

**Step 2: Run full test suite to verify no regressions**

Run: `cd src/app && npm test`
Expected: All tests pass

**Step 3: Commit**

```bash
git add src/app/src-rn/theme/useTheme.ts
git commit -m "feat: wire useTheme hook to accent color selection (#12)"
```

---

### Task 5: Add accent color picker UI to Settings

**Files:**
- Modify: `src/app/app/(tabs)/settings.tsx`

**Step 1: Add accent color picker to the appearance section**

In `settings.tsx`, add the import at the top:

```typescript
import { ACCENT_COLORS, AccentColorId } from '../../src-rn/theme/colors';
```

In the component body (after the `setThemePreference` line), add:

```typescript
const accentColor = useThemeStore((s) => s.accentColor);
const setAccentColor = useThemeStore((s) => s.setAccentColor);
```

Replace the `appearanceCard` variable with:

```typescript
const ACCENT_OPTIONS = Object.entries(ACCENT_COLORS) as [AccentColorId, typeof ACCENT_COLORS[AccentColorId]][];

const appearanceCard = (
  <View style={isWideLayout ? styles.desktopCard : styles.appearanceSection}>
    {!isWideLayout && <View style={styles.divider} />}
    <Text style={styles.sectionTitle}>Darstellung</Text>
    <View style={styles.themeRow}>
      {THEME_OPTIONS.map((opt) => (
        <Pressable
          key={opt.value}
          style={[
            styles.themeOption,
            themePreference === opt.value && styles.themeOptionActive,
          ]}
          onPress={() => setThemePreference(opt.value)}
        >
          <Text
            style={[
              styles.themeOptionText,
              themePreference === opt.value && styles.themeOptionTextActive,
            ]}
          >
            {opt.label}
          </Text>
        </Pressable>
      ))}
    </View>

    <Text style={styles.accentLabel}>Akzentfarbe</Text>
    <View style={styles.accentRow}>
      {ACCENT_OPTIONS.map(([id, config]) => (
        <Pressable
          key={id}
          style={styles.accentOption}
          onPress={() => setAccentColor(id)}
        >
          <View
            style={[
              styles.accentCircle,
              { backgroundColor: config.light.primary },
              accentColor === id && { borderColor: config.light.primary, borderWidth: 3 },
            ]}
          >
            {accentColor === id && (
              <Text style={styles.accentCheck}>✓</Text>
            )}
          </View>
          <Text
            style={[
              styles.accentOptionText,
              accentColor === id && { color: colors.primary },
            ]}
          >
            {config.label}
          </Text>
        </Pressable>
      ))}
    </View>
  </View>
);
```

Add these styles to the `createStyles` function:

```typescript
accentLabel: {
  fontSize: isCompactDesktop ? 12 : 13,
  fontWeight: '600',
  color: c.textSecondary,
  marginTop: isCompactDesktop ? 14 : 20,
  marginBottom: isCompactDesktop ? 8 : 12,
},
accentRow: {
  flexDirection: 'row',
  justifyContent: 'flex-start',
  gap: isCompactDesktop ? 12 : 16,
},
accentOption: {
  alignItems: 'center',
  gap: isCompactDesktop ? 4 : 6,
},
accentCircle: {
  width: isCompactDesktop ? 28 : 36,
  height: isCompactDesktop ? 28 : 36,
  borderRadius: isCompactDesktop ? 14 : 18,
  borderWidth: 2,
  borderColor: 'transparent',
  alignItems: 'center',
  justifyContent: 'center',
},
accentCheck: {
  color: '#fff',
  fontSize: isCompactDesktop ? 13 : 16,
  fontWeight: '700',
},
accentOptionText: {
  fontSize: isCompactDesktop ? 10 : 12,
  color: c.textTertiary,
  fontWeight: '500',
},
```

**Step 2: Run full test suite to verify no regressions**

Run: `cd src/app && npm test`
Expected: All tests pass

**Step 3: Commit**

```bash
git add src/app/app/(tabs)/settings.tsx
git commit -m "feat: add accent color picker to Settings screen (#12)"
```

---

### Task 6: Generate app icon SVGs and PNGs for all 5 themes

**Files:**
- Create: `src/app/assets/icons/icon-orange.svg`
- Create: `src/app/assets/icons/icon-emerald.svg`
- Create: `src/app/assets/icons/icon-berry.svg`
- Create: `src/app/assets/icons/icon-golden.svg`
- Create: `src/app/assets/icons/icon-ocean.svg`
- Create: `src/app/scripts/generate-icons.ts`

The new icon design: **white/light background** with **crossed fork & knife** in the accent color. The fork and knife are crossed in an X pattern (the issue specifies they are crossed).

**Step 1: Create the icon generation script**

Create `src/app/scripts/generate-icons.ts`:

```typescript
import * as fs from 'fs';
import * as path from 'path';

const ACCENTS: Record<string, { color: string; gradientEnd: string }> = {
  orange: { color: '#D4501A', gradientEnd: '#B84415' },
  emerald: { color: '#2E7D4F', gradientEnd: '#236B3F' },
  berry: { color: '#A62547', gradientEnd: '#8C1E3B' },
  golden: { color: '#C08B1A', gradientEnd: '#A07415' },
  ocean: { color: '#2563A8', gradientEnd: '#1E528C' },
};

function generateSvg(id: string, color: string, gradientEnd: string): string {
  return `<svg xmlns="http://www.w3.org/2000/svg" width="1024" height="1024" viewBox="0 0 1024 1024">
  <defs>
    <linearGradient id="bg-${id}" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" style="stop-color:#FFFFFF"/>
      <stop offset="100%" style="stop-color:#F0F0F2"/>
    </linearGradient>
    <linearGradient id="accent-${id}" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" style="stop-color:${color}"/>
      <stop offset="100%" style="stop-color:${gradientEnd}"/>
    </linearGradient>
  </defs>

  <!-- Background -->
  <rect width="1024" height="1024" rx="224" fill="url(#bg-${id})"/>

  <!-- Subtle plate circle -->
  <circle cx="512" cy="512" r="320" fill="none" stroke="${color}" stroke-opacity="0.08" stroke-width="6"/>
  <circle cx="512" cy="512" r="260" fill="none" stroke="${color}" stroke-opacity="0.05" stroke-width="3"/>

  <!-- Crossed Fork and Knife -->
  <g transform="translate(512, 512) rotate(-30) translate(-512, -512)">
    <!-- Fork -->
    <g transform="translate(400, 220)" fill="url(#accent-${id})">
      <rect x="10" y="0" width="14" height="130" rx="7"/>
      <rect x="38" y="0" width="14" height="130" rx="7"/>
      <rect x="66" y="0" width="14" height="130" rx="7"/>
      <rect x="94" y="0" width="14" height="130" rx="7"/>
      <rect x="10" y="120" width="98" height="34" rx="6"/>
      <rect x="40" y="144" width="38" height="440" rx="19"/>
    </g>
  </g>

  <g transform="translate(512, 512) rotate(30) translate(-512, -512)">
    <!-- Knife -->
    <g transform="translate(510, 220)" fill="url(#accent-${id})">
      <path d="M 20 0 C 20 0, 90 12, 90 70 L 90 155 L 20 155 Z"/>
      <rect x="20" y="145" width="70" height="34" rx="6"/>
      <rect x="36" y="169" width="38" height="420" rx="19"/>
    </g>
  </g>
</svg>`;
}

function generateAdaptiveSvg(id: string, color: string, gradientEnd: string): string {
  // Android adaptive: no background (background color set separately), just the foreground
  return `<svg xmlns="http://www.w3.org/2000/svg" width="1024" height="1024" viewBox="0 0 1024 1024">
  <defs>
    <linearGradient id="accent-a-${id}" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" style="stop-color:${color}"/>
      <stop offset="100%" style="stop-color:${gradientEnd}"/>
    </linearGradient>
  </defs>

  <!-- Crossed Fork and Knife -->
  <g transform="translate(512, 512) rotate(-30) translate(-512, -512)">
    <g transform="translate(400, 220)" fill="url(#accent-a-${id})">
      <rect x="10" y="0" width="14" height="130" rx="7"/>
      <rect x="38" y="0" width="14" height="130" rx="7"/>
      <rect x="66" y="0" width="14" height="130" rx="7"/>
      <rect x="94" y="0" width="14" height="130" rx="7"/>
      <rect x="10" y="120" width="98" height="34" rx="6"/>
      <rect x="40" y="144" width="38" height="440" rx="19"/>
    </g>
  </g>

  <g transform="translate(512, 512) rotate(30) translate(-512, -512)">
    <g transform="translate(510, 220)" fill="url(#accent-a-${id})">
      <path d="M 20 0 C 20 0, 90 12, 90 70 L 90 155 L 20 155 Z"/>
      <rect x="20" y="145" width="70" height="34" rx="6"/>
      <rect x="36" y="169" width="38" height="420" rx="19"/>
    </g>
  </g>
</svg>`;
}

const outDir = path.join(__dirname, '..', 'assets', 'icons');
fs.mkdirSync(outDir, { recursive: true });

for (const [id, { color, gradientEnd }] of Object.entries(ACCENTS)) {
  const svg = generateSvg(id, color, gradientEnd);
  fs.writeFileSync(path.join(outDir, `icon-${id}.svg`), svg);
  console.log(`Generated icon-${id}.svg`);

  const adaptiveSvg = generateAdaptiveSvg(id, color, gradientEnd);
  fs.writeFileSync(path.join(outDir, `adaptive-icon-${id}.svg`), adaptiveSvg);
  console.log(`Generated adaptive-icon-${id}.svg`);
}

console.log('\nDone! Now convert SVGs to PNGs:');
console.log('  npx tsx scripts/render-icons.ts');
```

**Step 2: Create the PNG rendering script**

Create `src/app/scripts/render-icons.ts`:

```typescript
import * as fs from 'fs';
import * as path from 'path';
import sharp from 'sharp';

const iconsDir = path.join(__dirname, '..', 'assets', 'icons');
const assetsDir = path.join(__dirname, '..', 'assets');

async function main() {
  const svgFiles = fs.readdirSync(iconsDir).filter(f => f.endsWith('.svg'));

  for (const svgFile of svgFiles) {
    const svgPath = path.join(iconsDir, svgFile);
    const pngFile = svgFile.replace('.svg', '.png');
    const pngPath = path.join(iconsDir, pngFile);

    await sharp(svgPath)
      .resize(1024, 1024)
      .png()
      .toFile(pngPath);

    console.log(`Rendered ${pngFile}`);
  }

  // Also update the default icon.png and adaptive-icon.png from the orange variant
  await sharp(path.join(iconsDir, 'icon-orange.svg'))
    .resize(1024, 1024)
    .png()
    .toFile(path.join(assetsDir, 'icon.png'));
  console.log('Updated assets/icon.png from orange variant');

  await sharp(path.join(iconsDir, 'adaptive-icon-orange.svg'))
    .resize(1024, 1024)
    .png()
    .toFile(path.join(assetsDir, 'adaptive-icon.png'));
  console.log('Updated assets/adaptive-icon.png from orange variant');
}

main().catch(console.error);
```

**Step 3: Install sharp as devDependency and run generation**

Run:
```bash
cd src/app && npm install -D sharp @types/sharp
npx tsx scripts/generate-icons.ts
npx tsx scripts/render-icons.ts
```

Expected: 10 SVG files + 10 PNG files in `assets/icons/`, updated `assets/icon.png` and `assets/adaptive-icon.png`

**Step 4: Verify output**

Run: `ls -la src/app/assets/icons/`
Expected: `icon-{orange,emerald,berry,golden,ocean}.{svg,png}` and `adaptive-icon-{orange,emerald,berry,golden,ocean}.{svg,png}`

**Step 5: Also update the main icon.svg and adaptive-icon.svg to the new crossed design**

Copy the orange SVG as the new default:
```bash
cp src/app/assets/icons/icon-orange.svg src/app/assets/icon.svg
cp src/app/assets/icons/adaptive-icon-orange.svg src/app/assets/adaptive-icon.svg
```

**Step 6: Commit**

```bash
git add src/app/assets/ src/app/scripts/generate-icons.ts src/app/scripts/render-icons.ts src/app/package.json src/app/package-lock.json
git commit -m "feat: generate crossed fork & knife icons for all 5 accent themes (#12)"
```

---

### Task 7: Configure `@g9k/expo-dynamic-app-icon` in `app.json`

**Files:**
- Modify: `src/app/app.json`

**Step 1: Add the plugin config**

Add `@g9k/expo-dynamic-app-icon` to the `plugins` array in `app.json`. The plugin takes an object where each key is an icon name, and values point to iOS and Android assets:

```json
{
  "expo": {
    ...existing config...,
    "plugins": [
      "expo-router",
      "expo-secure-store",
      "expo-font",
      [
        "@g9k/expo-dynamic-app-icon",
        {
          "emerald": {
            "ios": "./assets/icons/icon-emerald.png",
            "android": {
              "foregroundImage": "./assets/icons/adaptive-icon-emerald.png",
              "backgroundColor": "#F0F0F2"
            }
          },
          "berry": {
            "ios": "./assets/icons/icon-berry.png",
            "android": {
              "foregroundImage": "./assets/icons/adaptive-icon-berry.png",
              "backgroundColor": "#F0F0F2"
            }
          },
          "golden": {
            "ios": "./assets/icons/icon-golden.png",
            "android": {
              "foregroundImage": "./assets/icons/adaptive-icon-golden.png",
              "backgroundColor": "#F0F0F2"
            }
          },
          "ocean": {
            "ios": "./assets/icons/icon-ocean.png",
            "android": {
              "foregroundImage": "./assets/icons/adaptive-icon-ocean.png",
              "backgroundColor": "#F0F0F2"
            }
          }
        }
      ]
    ]
  }
}
```

Note: "orange" is NOT listed because it's the default icon (already set as `icon` in the main config). When `setAppIcon(null)` is called, it resets to the default.

Also update the main icon and adaptive icon references to use the new orange icon:

```json
"icon": "./assets/icons/icon-orange.png",
```

And update the android adaptive icon:

```json
"android": {
  ...
  "adaptiveIcon": {
    "foregroundImage": "./assets/icons/adaptive-icon-orange.png",
    "backgroundColor": "#F0F0F2"
  }
}
```

**Step 2: Commit**

```bash
git add src/app/app.json
git commit -m "feat: configure dynamic app icon plugin with 4 alternate icons (#12)"
```

---

### Task 8: Visual verification on iOS Simulator

**Step 1: Prebuild and run**

Run:
```bash
cd src/app && npx expo prebuild --clean
cd src/app && npx expo run:ios
```

**Step 2: Navigate to Settings tab**

Use iOS Simulator MCP tools to:
1. Take a screenshot to see the Settings screen
2. Tap on the Settings tab (gear icon)
3. Verify the "Darstellung" section shows the light/dark toggle AND the new accent color picker below it
4. Verify 5 colored circles are visible with labels

**Step 3: Test each accent color**

For each of the 5 colors:
1. Tap the accent circle
2. Verify the UI accent color changes immediately (buttons, active states, tab icons)
3. Check that the app icon change dialog appears (iOS)
4. Take screenshots for verification

**Step 4: Test light/dark mode independence**

1. Switch to dark mode
2. Verify accent color persists
3. Switch accent while in dark mode
4. Verify both settings are independent

---

### Task 9: Run full test suite and final verification

**Step 1: Run all tests**

Run: `cd src/app && npm test`
Expected: All tests pass (original 178+ plus new theme tests)

**Step 2: Verify test count increased**

The new tests added:
- `__tests__/theme/colors.test.ts` — ~7 tests
- `__tests__/store/themeStore.test.ts` — ~4 tests

**Step 3: Final commit if any cleanup needed**

```bash
git add -A
git commit -m "test: verify all tests pass with accent theme changes (#12)"
```

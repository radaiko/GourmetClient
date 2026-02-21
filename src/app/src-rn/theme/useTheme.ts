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

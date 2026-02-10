import { useMemo } from 'react';
import { useColorScheme } from 'react-native';
import { useThemeStore } from '../store/themeStore';
import { Colors, LightColors, DarkColors } from './colors';

interface ThemeResult {
  colors: Colors;
  isDark: boolean;
  colorScheme: 'light' | 'dark';
}

export function useTheme(): ThemeResult {
  const systemScheme = useColorScheme();
  const preference = useThemeStore((s) => s.preference);

  const colorScheme =
    preference === 'system' ? (systemScheme ?? 'light') : preference;

  const isDark = colorScheme === 'dark';
  const colors = useMemo(() => (isDark ? DarkColors : LightColors), [isDark]);

  return { colors, isDark, colorScheme };
}

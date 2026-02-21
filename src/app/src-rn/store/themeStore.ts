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
            setAppIcon(color === 'orange' ? null : color, false);
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

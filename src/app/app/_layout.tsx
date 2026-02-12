import { useEffect } from 'react';
import { Platform } from 'react-native';
import { Stack } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
// IMPORTANT: tauriHttp must be imported BEFORE any store modules.
// The module patches axios.create at load time so Zustand stores get
// Tauri-aware Axios instances when they call axios.create() during init.
import '../src-rn/utils/tauriHttp';
import { useAuthStore } from '../src-rn/store/authStore';
import { useVentopayAuthStore } from '../src-rn/store/ventopayAuthStore';
import { useTheme } from '../src-rn/theme/useTheme';

export default function RootLayout() {
  const gourmetLoginWithSaved = useAuthStore((s) => s.loginWithSaved);
  const ventopayLoginWithSaved = useVentopayAuthStore((s) => s.loginWithSaved);
  const { colorScheme } = useTheme();

  useEffect(() => {
    gourmetLoginWithSaved();
    ventopayLoginWithSaved();
  }, [gourmetLoginWithSaved, ventopayLoginWithSaved]);

  useEffect(() => {
    if (Platform.OS === 'web') {
      document.documentElement.style.colorScheme = colorScheme;
    }
  }, [colorScheme]);

  return (
    <>
      <Stack>
        <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
      </Stack>
      <StatusBar style="auto" />
    </>
  );
}

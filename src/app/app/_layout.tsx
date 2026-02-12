import { useEffect } from 'react';
import { Stack } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import { installTauriHttpProxy } from '../src-rn/utils/tauriHttp';
import { useAuthStore } from '../src-rn/store/authStore';
import { useVentopayAuthStore } from '../src-rn/store/ventopayAuthStore';

// Patch Axios to route HTTP through Rust on desktop (no-op on native/browser)
installTauriHttpProxy();

export default function RootLayout() {
  const gourmetLoginWithSaved = useAuthStore((s) => s.loginWithSaved);
  const ventopayLoginWithSaved = useVentopayAuthStore((s) => s.loginWithSaved);

  useEffect(() => {
    gourmetLoginWithSaved();
    ventopayLoginWithSaved();
  }, [gourmetLoginWithSaved, ventopayLoginWithSaved]);

  return (
    <>
      <Stack>
        <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
      </Stack>
      <StatusBar style="auto" />
    </>
  );
}

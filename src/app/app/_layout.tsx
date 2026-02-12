import { Stack } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import { installTauriHttpProxy } from '../src-rn/utils/tauriHttp';

// Patch Axios to route HTTP through Rust on desktop (no-op on native/browser)
installTauriHttpProxy();

export default function RootLayout() {
  return (
    <>
      <Stack>
        <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
      </Stack>
      <StatusBar style="auto" />
    </>
  );
}

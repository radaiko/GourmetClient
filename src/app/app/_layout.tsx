import { useEffect } from 'react';
import { Platform } from 'react-native';
import { Stack } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import { PostHogProvider } from 'posthog-react-native';
// IMPORTANT: tauriHttp must be imported BEFORE any store modules.
// The module patches axios.create at load time so Zustand stores get
// Tauri-aware Axios instances when they call axios.create() during init.
import '../src-rn/utils/tauriHttp';
import { useAuthStore } from '../src-rn/store/authStore';
import { useVentopayAuthStore } from '../src-rn/store/ventopayAuthStore';
import { useTheme } from '../src-rn/theme/useTheme';
import { DialogProvider } from '../src-rn/components/DialogProvider';

function AppContent() {
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
    <DialogProvider>
      <Stack>
        <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
      </Stack>
      <StatusBar style="auto" />
    </DialogProvider>
  );
}

export default function RootLayout() {
  if (__DEV__) {
    return <AppContent />;
  }

  return (
    <PostHogProvider
      apiKey="phc_F2Bzuz5BQGxVxsj73fl0REhelkw6DP99YbrDsrVnIHo"
      options={{
        host: 'https://eu.i.posthog.com',
        captureNativeAppLifecycleEvents: true,
        enableSessionReplay: true,
        sessionReplayConfig: {
          maskAllTextInputs: true,
          maskAllImages: false,
        },
        errorTracking: {
          autocapture: {
            uncaughtExceptions: true,
            unhandledRejections: true,
          },
        },
      }}
      autocapture={{
        captureTouches: true,
        captureScreens: true,
        captureLifecycleEvents: true,
      }}
    >
      <AppContent />
    </PostHogProvider>
  );
}

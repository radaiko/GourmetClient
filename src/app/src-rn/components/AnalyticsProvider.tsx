import { ReactNode, useEffect } from 'react';
import { Appearance, Platform } from 'react-native';
import { PostHogProvider, usePostHog } from 'posthog-react-native';
import Constants from 'expo-constants';
import * as Device from 'expo-device';
import * as Network from 'expo-network';

const POSTHOG_KEY = 'phc_F2Bzuz5BQGxVxsj73fl0REhelkw6DP99YbrDsrVnIHo';
const POSTHOG_HOST = 'https://eu.i.posthog.com';

/** Registers device & app super properties on every event. */
function RegisterSuperProperties() {
  const posthog = usePostHog();

  useEffect(() => {
    const osName = Platform.OS === 'ios' ? 'iOS' : 'Android';
    const osVersion =
      Platform.OS === 'ios'
        ? String(Platform.Version)
        : String(
            (Platform.constants as Record<string, unknown>).Release ??
              Platform.Version,
          );
    const appBuild =
      Platform.OS === 'ios'
        ? Constants.expoConfig?.ios?.buildNumber
        : Constants.expoConfig?.android?.versionCode?.toString();

    posthog.register({
      $os: osName,
      $os_version: osVersion,
      $app_version: Constants.expoConfig?.version,
      $app_build: appBuild,
      $device_model: Device.modelName,
      $appearance: Appearance.getColorScheme() ?? 'unknown',
    });

    Network.getNetworkStateAsync().then((state) => {
      posthog.register({
        $network_type: state.type?.toLowerCase() ?? 'unknown',
      });
    });
  }, [posthog]);

  return null;
}

export function AnalyticsProvider({ children }: { children: ReactNode }) {
  return (
    <PostHogProvider
      apiKey={POSTHOG_KEY}
      options={{
        host: POSTHOG_HOST,
        captureAppLifecycleEvents: true,
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
      }}
    >
      <RegisterSuperProperties />
      {children}
    </PostHogProvider>
  );
}

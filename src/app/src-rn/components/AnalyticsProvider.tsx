import { ReactNode } from 'react';
import { PostHogProvider } from 'posthog-react-native';

const POSTHOG_KEY = 'phc_F2Bzuz5BQGxVxsj73fl0REhelkw6DP99YbrDsrVnIHo';
const POSTHOG_HOST = 'https://eu.i.posthog.com';

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
      {children}
    </PostHogProvider>
  );
}

import { ReactNode, useEffect } from 'react';
import posthog from 'posthog-js';
import { PostHogProvider } from 'posthog-js/react';
import Constants from 'expo-constants';

const POSTHOG_KEY = 'phc_F2Bzuz5BQGxVxsj73fl0REhelkw6DP99YbrDsrVnIHo';
const POSTHOG_HOST = 'https://eu.i.posthog.com';

export function AnalyticsProvider({ children }: { children: ReactNode }) {
  useEffect(() => {
    posthog.init(POSTHOG_KEY, {
      api_host: POSTHOG_HOST,
      autocapture: true,
      capture_pageview: true,
      capture_pageleave: true,
      session_recording: {
        maskAllInputs: true,
        maskTextSelector: '*',
      },
      persistence: 'localStorage',
    });

    const appearance = window.matchMedia('(prefers-color-scheme: dark)').matches
      ? 'dark'
      : 'light';

    posthog.register({
      $app_version: Constants.expoConfig?.version,
      $appearance: appearance,
    });
  }, []);

  return (
    <PostHogProvider client={posthog}>
      {children}
    </PostHogProvider>
  );
}

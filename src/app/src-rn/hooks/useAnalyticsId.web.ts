import { usePostHog } from 'posthog-js/react';

/** Returns the anonymous PostHog distinct ID (web/desktop). */
export function useAnalyticsId(): string | null {
  const posthog = usePostHog();
  return posthog?.get_distinct_id() ?? null;
}

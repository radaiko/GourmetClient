import { useEffect, useState } from 'react';
import { usePostHog } from 'posthog-react-native';

/**
 * Returns the anonymous PostHog distinct ID, or null when unavailable.
 *
 * In __DEV__ mode, AnalyticsProvider is not mounted so usePostHog() would
 * throw. We export a no-op hook for dev and the real hook for production.
 * __DEV__ is a compile-time constant so the branch is consistent across
 * renders (no hook-ordering issues).
 */
function useAnalyticsIdProd(): string | null {
  const posthog = usePostHog();
  const [id, setId] = useState<string | null>(null);

  useEffect(() => {
    setId(posthog.getDistinctId());
  }, [posthog]);

  return id;
}

function useAnalyticsIdDev(): string | null {
  return null;
}

export const useAnalyticsId = __DEV__ ? useAnalyticsIdDev : useAnalyticsIdProd;

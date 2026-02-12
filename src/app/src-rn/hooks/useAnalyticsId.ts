import { useEffect, useState } from 'react';
import { usePostHog } from 'posthog-react-native';

/** Returns the anonymous PostHog distinct ID (native). */
export function useAnalyticsId(): string | null {
  const posthog = usePostHog();
  const [id, setId] = useState<string | null>(null);

  useEffect(() => {
    posthog.getDistinctId().then(setId);
  }, [posthog]);

  return id;
}

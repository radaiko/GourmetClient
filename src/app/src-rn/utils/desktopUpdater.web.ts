import { create } from 'zustand';
import { isDesktop } from './platform';

const CHECK_INTERVAL_MS = 60 * 60 * 1000; // 1 hour

async function tauriInvoke<T>(cmd: string, args?: Record<string, unknown>): Promise<T> {
  const internals = (window as any).__TAURI_INTERNALS__;
  if (!internals?.invoke) throw new Error('Not in Tauri context');
  return internals.invoke(cmd, args);
}

interface UpdateState {
  pendingVersion: string | null;
  checking: boolean;
}

export const useUpdateStore = create<UpdateState>(() => ({
  pendingVersion: null,
  checking: false,
}));

async function checkAndDownload(): Promise<void> {
  if (!isDesktop()) return;
  if (useUpdateStore.getState().pendingVersion) return; // already downloaded

  useUpdateStore.setState({ checking: true });
  try {
    const version = await tauriInvoke<string | null>('download_update');
    if (version) {
      useUpdateStore.setState({ pendingVersion: version });
    }
  } catch (error) {
    console.error('Background update check failed:', error);
  } finally {
    useUpdateStore.setState({ checking: false });
  }
}

export async function applyUpdate(): Promise<void> {
  await tauriInvoke('install_update');
}

export async function checkForDesktopUpdates(userInitiated = false): Promise<void> {
  if (!isDesktop()) return;

  useUpdateStore.setState({ checking: true });
  try {
    const version = await tauriInvoke<string | null>('download_update');
    if (version) {
      useUpdateStore.setState({ pendingVersion: version });
      if (userInitiated) {
        window.alert(`Version ${version} downloaded. You can update now or it will apply on next restart.`);
      }
    } else if (userInitiated) {
      window.alert('You are on the latest version.');
    }
  } catch (error) {
    console.error('Update check failed:', error);
    if (userInitiated) {
      window.alert('Failed to check for updates.');
    }
  } finally {
    useUpdateStore.setState({ checking: false });
  }
}

// Auto-start background checks on desktop
if (isDesktop()) {
  // Check on startup (small delay to not block app init)
  setTimeout(checkAndDownload, 5000);
  // Check every hour
  setInterval(checkAndDownload, CHECK_INTERVAL_MS);
}

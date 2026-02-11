import { isDesktop } from './platform';

async function tauriInvoke<T>(cmd: string, args?: Record<string, unknown>): Promise<T> {
  const internals = (window as any).__TAURI_INTERNALS__;
  if (!internals?.invoke) throw new Error('Not in Tauri context');
  return internals.invoke(cmd, args);
}

export async function checkForDesktopUpdates(userInitiated = false): Promise<void> {
  if (!isDesktop()) return;

  try {
    const version = await tauriInvoke<string | null>('check_for_updates');

    if (version) {
      const shouldUpdate = window.confirm(
        `Version ${version} is available. Update now?`,
      );
      if (shouldUpdate) {
        await tauriInvoke('install_update');
      }
    } else if (userInitiated) {
      window.alert('You are on the latest version.');
    }
  } catch (error) {
    console.error('Update check failed:', error);
    if (userInitiated) {
      window.alert('Failed to check for updates.');
    }
  }
}

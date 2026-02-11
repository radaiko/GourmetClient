import { Platform } from 'react-native';

export type AppPlatform = 'ios' | 'android' | 'desktop' | 'web';

export function getAppPlatform(): AppPlatform {
  if (Platform.OS === 'web') {
    return typeof window !== 'undefined' &&
      ('__TAURI_INTERNALS__' in window || '__TAURI__' in window)
      ? 'desktop'
      : 'web';
  }
  return Platform.OS as 'ios' | 'android';
}

export const isDesktop = (): boolean => getAppPlatform() === 'desktop';
export const isWeb = (): boolean => Platform.OS === 'web';
export const isNative = (): boolean => Platform.OS !== 'web';

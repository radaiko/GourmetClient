describe('platform utils', () => {
  beforeEach(() => {
    jest.resetModules();
    // Clean up any TAURI globals
    delete (globalThis as any).__TAURI_INTERNALS__;
    delete (globalThis as any).__TAURI__;
    // Provide a window-like object for platform.ts which checks `window`
    (globalThis as any).window = globalThis;
  });

  afterEach(() => {
    delete (globalThis as any).__TAURI_INTERNALS__;
    delete (globalThis as any).__TAURI__;
  });

  it('returns ios when Platform.OS is ios', () => {
    jest.mock('react-native', () => ({
      Platform: { OS: 'ios' },
    }));
    const { getAppPlatform, isNative } = require('../../utils/platform');
    expect(getAppPlatform()).toBe('ios');
    expect(isNative()).toBe(true);
  });

  it('returns web when Platform.OS is web and no TAURI', () => {
    jest.mock('react-native', () => ({
      Platform: { OS: 'web' },
    }));
    const { getAppPlatform, isWeb, isNative } = require('../../utils/platform');
    expect(getAppPlatform()).toBe('web');
    expect(isWeb()).toBe(true);
    expect(isNative()).toBe(false);
  });

  it('returns desktop when Platform.OS is web and TAURI exists', () => {
    jest.mock('react-native', () => ({
      Platform: { OS: 'web' },
    }));
    (globalThis as any).__TAURI_INTERNALS__ = {};
    const { getAppPlatform, isDesktop } = require('../../utils/platform');
    expect(getAppPlatform()).toBe('desktop');
    expect(isDesktop()).toBe(true);
  });

  it('returns android when Platform.OS is android', () => {
    jest.mock('react-native', () => ({
      Platform: { OS: 'android' },
    }));
    const { getAppPlatform, isNative } = require('../../utils/platform');
    expect(getAppPlatform()).toBe('android');
    expect(isNative()).toBe(true);
  });
});

const mockSetAppIcon = jest.fn();
jest.mock('@g9k/expo-dynamic-app-icon', () => ({
  setAppIcon: (...args: unknown[]) => mockSetAppIcon(...args),
}));

import { useThemeStore } from '../../store/themeStore';

// Reset store between tests
beforeEach(() => {
  useThemeStore.setState({ preference: 'system', accentColor: 'orange' });
  mockSetAppIcon.mockClear();
});

describe('themeStore', () => {
  it('has default accent color of orange', () => {
    expect(useThemeStore.getState().accentColor).toBe('orange');
  });

  it('setAccentColor updates the accent color', () => {
    useThemeStore.getState().setAccentColor('emerald');
    expect(useThemeStore.getState().accentColor).toBe('emerald');
  });

  it('setAccentColor works for all accent values', () => {
    const accents = ['orange', 'emerald', 'berry', 'golden', 'ocean'] as const;
    for (const accent of accents) {
      useThemeStore.getState().setAccentColor(accent);
      expect(useThemeStore.getState().accentColor).toBe(accent);
    }
  });

  it('preserves preference when changing accent', () => {
    useThemeStore.getState().setPreference('dark');
    useThemeStore.getState().setAccentColor('berry');
    expect(useThemeStore.getState().preference).toBe('dark');
    expect(useThemeStore.getState().accentColor).toBe('berry');
  });

  it('calls setAppIcon(null) when switching to orange (default)', () => {
    useThemeStore.getState().setAccentColor('emerald');
    mockSetAppIcon.mockClear();
    useThemeStore.getState().setAccentColor('orange');
    expect(mockSetAppIcon).toHaveBeenCalledWith(null);
  });

  it('calls setAppIcon with accent name for non-orange themes', () => {
    useThemeStore.getState().setAccentColor('emerald');
    expect(mockSetAppIcon).toHaveBeenCalledWith('emerald');

    useThemeStore.getState().setAccentColor('berry');
    expect(mockSetAppIcon).toHaveBeenCalledWith('berry');
  });
});

import { useThemeStore } from '../../store/themeStore';

// Reset store between tests
beforeEach(() => {
  useThemeStore.setState({ preference: 'system', accentColor: 'orange' });
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
});

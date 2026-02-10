export interface Colors {
  // Surfaces
  background: string;
  surface: string;
  surfaceVariant: string;

  // Text
  textPrimary: string;
  textSecondary: string;
  textTertiary: string;

  // Brand
  primary: string;
  primaryDark: string;
  primarySurface: string;

  // Borders
  border: string;
  borderInput: string;

  // Status
  success: string;
  successSurface: string;
  successText: string;
  successBorder: string;

  warning: string;
  warningSurface: string;
  warningText: string;
  warningBorder: string;

  error: string;
  errorSurface: string;
  errorText: string;

  // Overlay
  overlay: string;

  // Glass surfaces (rgba with alpha for translucency)
  glassSurface: string;
  glassSurfaceVariant: string;

  // Glass highlights (subtle edge borders)
  glassHighlight: string;
  glassShadowEdge: string;

  // Tinted glass for status contexts
  glassSuccess: string;
  glassWarning: string;
  glassError: string;
  glassPrimary: string;

  // Blur config
  blurTint: string;
  blurIntensity: number;
  blurIntensityStrong: number;
  blurIntensitySubtle: number;
}

export const LightColors: Colors = {
  background: '#f5f5f5',
  surface: '#fff',
  surfaceVariant: '#f5f5f5',

  textPrimary: '#333',
  textSecondary: '#666',
  textTertiary: '#999',

  primary: '#4a90d9',
  primaryDark: '#3a7bc8',
  primarySurface: '#e3f0ff',

  border: '#e0e0e0',
  borderInput: '#ddd',

  success: '#4caf50',
  successSurface: '#e8f5e9',
  successText: '#2e7d32',
  successBorder: '#4caf50',

  warning: '#ff9800',
  warningSurface: '#fff3e0',
  warningText: '#e65100',
  warningBorder: '#ffcc80',

  error: '#f44336',
  errorSurface: '#ffebee',
  errorText: '#c62828',

  overlay: 'rgba(255,255,255,0.7)',

  // Glass
  glassSurface: 'rgba(255,255,255,0.68)',
  glassSurfaceVariant: 'rgba(245,245,245,0.60)',

  glassHighlight: 'rgba(255,255,255,0.80)',
  glassShadowEdge: 'rgba(0,0,0,0.08)',

  glassSuccess: 'rgba(76,175,80,0.15)',
  glassWarning: 'rgba(255,152,0,0.15)',
  glassError: 'rgba(244,67,54,0.15)',
  glassPrimary: 'rgba(74,144,217,0.12)',

  blurTint: 'systemThinMaterial',
  blurIntensity: 40,
  blurIntensityStrong: 80,
  blurIntensitySubtle: 25,
};

export const DarkColors: Colors = {
  background: '#121212',
  surface: '#1e1e1e',
  surfaceVariant: '#2c2c2c',

  textPrimary: '#e0e0e0',
  textSecondary: '#aaa',
  textTertiary: '#777',

  primary: '#6aadf0',
  primaryDark: '#4a90d9',
  primarySurface: '#1a2d3d',

  border: '#333',
  borderInput: '#444',

  success: '#4caf50',
  successSurface: '#1b3a1b',
  successText: '#66bb6a',
  successBorder: '#2e7d32',

  warning: '#ff9800',
  warningSurface: '#3d2e10',
  warningText: '#ffb74d',
  warningBorder: '#f57c00',

  error: '#f44336',
  errorSurface: '#3d1a1a',
  errorText: '#ef5350',

  overlay: 'rgba(0,0,0,0.7)',

  // Glass
  glassSurface: 'rgba(30,30,30,0.70)',
  glassSurfaceVariant: 'rgba(44,44,44,0.60)',

  glassHighlight: 'rgba(255,255,255,0.15)',
  glassShadowEdge: 'rgba(0,0,0,0.40)',

  glassSuccess: 'rgba(76,175,80,0.20)',
  glassWarning: 'rgba(255,152,0,0.20)',
  glassError: 'rgba(244,67,54,0.20)',
  glassPrimary: 'rgba(106,173,240,0.18)',

  blurTint: 'systemThickMaterialDark',
  blurIntensity: 50,
  blurIntensityStrong: 90,
  blurIntensitySubtle: 30,
};

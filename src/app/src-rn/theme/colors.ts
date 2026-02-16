import { useFlatStyle } from '../utils/platform';

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
  background: '#F5F5F7',
  surface: '#fff',
  surfaceVariant: '#EDEDF0',

  textPrimary: '#1D1D1F',
  textSecondary: '#6E6E73',
  textTertiary: '#AEAEB2',

  primary: '#D4501A',
  primaryDark: '#B84415',
  primarySurface: '#FFF1EB',

  border: '#D6D6D8',
  borderInput: '#CECED0',

  success: '#34A853',
  successSurface: '#EBF5EE',
  successText: '#1E7E34',
  successBorder: '#34A853',

  warning: '#F5A623',
  warningSurface: '#FEF6E6',
  warningText: '#B57A10',
  warningBorder: '#F5C564',

  error: '#D93025',
  errorSurface: '#FCE8E6',
  errorText: '#B3261E',

  overlay: 'rgba(255,255,255,0.7)',

  // Glass (opaque on Android since BlurView is not used)
  glassSurface: useFlatStyle ? '#ffffff' : 'rgba(255,255,255,0.70)',
  glassSurfaceVariant: useFlatStyle ? '#EDEDF0' : 'rgba(237,237,240,0.60)',

  glassHighlight: useFlatStyle ? '#D6D6D8' : 'rgba(255,255,255,0.85)',
  glassShadowEdge: useFlatStyle ? '#D6D6D8' : 'rgba(0,0,0,0.06)',

  glassSuccess: useFlatStyle ? '#EBF5EE' : 'rgba(52,168,83,0.10)',
  glassWarning: useFlatStyle ? '#FEF6E6' : 'rgba(245,166,35,0.10)',
  glassError: useFlatStyle ? '#FCE8E6' : 'rgba(217,48,37,0.10)',
  glassPrimary: useFlatStyle ? '#FFF1EB' : 'rgba(212,80,26,0.08)',

  blurTint: 'systemThinMaterial',
  blurIntensity: 40,
  blurIntensityStrong: 80,
  blurIntensitySubtle: 25,
};

export const DarkColors: Colors = {
  background: '#000000',
  surface: '#1C1C1E',
  surfaceVariant: '#2C2C2E',

  textPrimary: '#F5F5F7',
  textSecondary: '#A1A1A6',
  textTertiary: '#636366',

  primary: '#FF6B35',
  primaryDark: '#D4501A',
  primarySurface: '#2A1A10',

  border: '#38383A',
  borderInput: '#48484A',

  success: '#34A853',
  successSurface: '#142018',
  successText: '#5DB075',
  successBorder: '#2E7D32',

  warning: '#F5A623',
  warningSurface: '#2A1E0E',
  warningText: '#F5C564',
  warningBorder: '#B57A10',

  error: '#EA4335',
  errorSurface: '#2A1614',
  errorText: '#F28B82',

  overlay: 'rgba(0,0,0,0.7)',

  // Glass (opaque on Android since BlurView is not used)
  glassSurface: useFlatStyle ? '#1C1C1E' : 'rgba(28,28,30,0.72)',
  glassSurfaceVariant: useFlatStyle ? '#2C2C2E' : 'rgba(44,44,46,0.60)',

  glassHighlight: useFlatStyle ? '#38383A' : 'rgba(255,255,255,0.12)',
  glassShadowEdge: useFlatStyle ? '#000000' : 'rgba(0,0,0,0.40)',

  glassSuccess: useFlatStyle ? '#142018' : 'rgba(52,168,83,0.16)',
  glassWarning: useFlatStyle ? '#2A1E0E' : 'rgba(245,166,35,0.16)',
  glassError: useFlatStyle ? '#2A1614' : 'rgba(234,67,53,0.16)',
  glassPrimary: useFlatStyle ? '#2A1A10' : 'rgba(255,107,53,0.14)',

  blurTint: 'systemThickMaterialDark',
  blurIntensity: 50,
  blurIntensityStrong: 90,
  blurIntensitySubtle: 30,
};

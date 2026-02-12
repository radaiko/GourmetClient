import { Platform, ViewStyle } from 'react-native';
import { Colors } from './colors';

const isAndroid = Platform.OS === 'android';

/** Card / surface container — glass on iOS, solid + elevation on Android */
export function cardSurface(c: Colors): ViewStyle {
  if (isAndroid) {
    return {
      backgroundColor: c.surface,
      borderRadius: 12,
      borderWidth: 1,
      borderColor: c.border,
      elevation: 2,
    };
  }
  return {
    backgroundColor: c.glassSurface,
    borderRadius: 18,
    borderTopWidth: 1,
    borderLeftWidth: 0.5,
    borderBottomWidth: 0.5,
    borderRightWidth: 0,
    borderTopColor: c.glassHighlight,
    borderLeftColor: c.glassHighlight,
    borderBottomColor: c.glassShadowEdge,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.08,
    shadowRadius: 12,
    elevation: 3,
  };
}

/** Smaller surface (badges, status banners) */
export function bannerSurface(c: Colors): ViewStyle {
  if (isAndroid) {
    return {
      backgroundColor: c.surface,
      borderRadius: 12,
      borderWidth: 1,
      borderColor: c.border,
      elevation: 1,
    };
  }
  return {
    backgroundColor: c.glassSurface,
    borderRadius: 14,
    borderTopWidth: 1,
    borderLeftWidth: 0.5,
    borderBottomWidth: 0.5,
    borderRightWidth: 0,
    borderTopColor: c.glassHighlight,
    borderLeftColor: c.glassHighlight,
    borderBottomColor: c.glassShadowEdge,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.06,
    shadowRadius: 6,
    elevation: 2,
  };
}

/** Status-tinted banner (error, warning, success) */
export function tintedBanner(c: Colors, bg: string): ViewStyle {
  if (isAndroid) {
    return {
      backgroundColor: bg,
      borderRadius: 12,
      borderWidth: 1,
      borderColor: c.border,
      elevation: 1,
    };
  }
  return {
    backgroundColor: bg,
    borderRadius: 14,
    borderTopWidth: 1,
    borderLeftWidth: 0.5,
    borderBottomWidth: 0.5,
    borderRightWidth: 0,
    borderTopColor: c.glassHighlight,
    borderLeftColor: c.glassHighlight,
    borderBottomColor: c.glassShadowEdge,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.08,
    shadowRadius: 6,
    elevation: 2,
  };
}

/** Primary button */
export function buttonPrimary(c: Colors): ViewStyle {
  if (isAndroid) {
    return {
      backgroundColor: c.primary,
      borderRadius: 12,
      elevation: 2,
    };
  }
  return {
    backgroundColor: c.primary,
    borderRadius: 14,
    borderTopWidth: 1,
    borderTopColor: 'rgba(255,255,255,0.30)',
    borderLeftWidth: 0.5,
    borderLeftColor: 'rgba(255,255,255,0.15)',
    borderBottomWidth: 0,
    borderRightWidth: 0,
    shadowColor: c.primary,
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.25,
    shadowRadius: 8,
    elevation: 3,
  };
}

/** Secondary / outlined button */
export function buttonSecondary(c: Colors): ViewStyle {
  if (isAndroid) {
    return {
      backgroundColor: c.surface,
      borderRadius: 12,
      borderWidth: 1,
      borderColor: c.primary,
      elevation: 0,
    };
  }
  return {
    backgroundColor: c.glassSurface,
    borderRadius: 14,
    borderTopWidth: 1,
    borderLeftWidth: 0.5,
    borderBottomWidth: 0.5,
    borderRightWidth: 0,
    borderTopColor: c.glassHighlight,
    borderLeftColor: c.glassHighlight,
    borderBottomColor: c.glassShadowEdge,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.06,
    shadowRadius: 6,
    elevation: 2,
  };
}

/** Danger button */
export function buttonDanger(c: Colors): ViewStyle {
  if (isAndroid) {
    return {
      backgroundColor: c.error,
      borderRadius: 12,
      elevation: 2,
    };
  }
  return {
    backgroundColor: c.error,
    borderRadius: 14,
    borderTopWidth: 1,
    borderTopColor: 'rgba(255,255,255,0.25)',
    borderLeftWidth: 0.5,
    borderLeftColor: 'rgba(255,255,255,0.12)',
    borderBottomWidth: 0,
    borderRightWidth: 0,
  };
}

/** Text input field */
export function inputField(c: Colors): ViewStyle {
  if (isAndroid) {
    return {
      backgroundColor: c.surface,
      borderRadius: 12,
      borderWidth: 1,
      borderColor: c.borderInput,
      paddingHorizontal: 14,
      paddingVertical: 12,
      elevation: 0,
    };
  }
  return {
    backgroundColor: c.glassSurface,
    borderRadius: 14,
    paddingHorizontal: 14,
    paddingVertical: 12,
    borderTopWidth: 1,
    borderLeftWidth: 0.5,
    borderBottomWidth: 0.5,
    borderRightWidth: 0,
    borderTopColor: c.glassHighlight,
    borderLeftColor: c.glassHighlight,
    borderBottomColor: c.glassShadowEdge,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.04,
    shadowRadius: 4,
    elevation: 1,
  };
}

/** Navigation arrow / circular icon button */
export function circleButton(c: Colors): ViewStyle {
  if (isAndroid) {
    return {
      backgroundColor: c.surfaceVariant,
      borderRadius: 24,
      elevation: 1,
    };
  }
  return {
    backgroundColor: c.glassSurfaceVariant,
    borderRadius: 24,
    borderTopWidth: 1,
    borderLeftWidth: 0.5,
    borderBottomWidth: 0.5,
    borderTopColor: c.glassHighlight,
    borderLeftColor: c.glassHighlight,
    borderBottomColor: c.glassShadowEdge,
    borderRightWidth: 0,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.06,
    shadowRadius: 6,
    elevation: 2,
  };
}

/** Desktop sidebar surface — glass bg, right border, right shadow */
export function sidebarSurface(c: Colors): ViewStyle {
  return {
    backgroundColor: c.glassSurface,
    borderRightWidth: 0.5,
    borderRightColor: c.glassShadowEdge,
    shadowColor: '#000',
    shadowOffset: { width: 4, height: 0 },
    shadowOpacity: 0.08,
    shadowRadius: 12,
  };
}

/** Desktop side-panel surface — glass bg, right border */
export function panelSurface(c: Colors): ViewStyle {
  return {
    backgroundColor: c.glassSurface,
    borderRightWidth: 0.5,
    borderRightColor: c.glassShadowEdge,
  };
}

/** Floating action button */
export function fab(c: Colors): ViewStyle {
  if (isAndroid) {
    return {
      backgroundColor: c.primary,
      borderRadius: 16,
      elevation: 6,
    };
  }
  return {
    backgroundColor: c.primary,
    borderRadius: 30,
    borderTopWidth: 1,
    borderTopColor: 'rgba(255,255,255,0.40)',
    borderLeftWidth: 0.5,
    borderLeftColor: 'rgba(255,255,255,0.20)',
    borderBottomWidth: 0,
    borderRightWidth: 0,
    elevation: 8,
    shadowColor: c.primary,
    shadowOffset: { width: 0, height: 6 },
    shadowOpacity: 0.35,
    shadowRadius: 16,
  };
}

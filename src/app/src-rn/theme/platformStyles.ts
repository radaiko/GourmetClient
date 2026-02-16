import { Platform, ViewStyle } from 'react-native';
import { Colors } from './colors';
import { isDesktop } from '../utils/platform';

const isAndroid = Platform.OS === 'android';
const isDesktopWeb = isDesktop();

/** Card / surface container — glass on iOS, solid + elevation on Android, flat on desktop */
export function cardSurface(c: Colors): ViewStyle {
  if (isAndroid) {
    return {
      backgroundColor: c.surface,
      borderRadius: 14,
      borderWidth: 1,
      borderColor: c.border,
      elevation: 1,
    };
  }
  if (isDesktopWeb) {
    return {
      backgroundColor: c.surface,
      borderRadius: 4,
      borderWidth: 1,
      borderColor: c.border,
    };
  }
  return {
    backgroundColor: c.glassSurface,
    borderRadius: 16,
    borderWidth: 0.5,
    borderColor: c.glassShadowEdge,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.06,
    shadowRadius: 8,
    elevation: 2,
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
  if (isDesktopWeb) {
    return {
      backgroundColor: c.surface,
      borderRadius: 4,
      borderWidth: 1,
      borderColor: c.border,
    };
  }
  return {
    backgroundColor: c.glassSurface,
    borderRadius: 12,
    borderWidth: 0.5,
    borderColor: c.glassShadowEdge,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.04,
    shadowRadius: 4,
    elevation: 1,
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
  if (isDesktopWeb) {
    return {
      backgroundColor: bg,
      borderRadius: 4,
      borderWidth: 1,
      borderColor: c.border,
    };
  }
  return {
    backgroundColor: bg,
    borderRadius: 12,
    borderWidth: 0.5,
    borderColor: c.glassShadowEdge,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.04,
    shadowRadius: 4,
    elevation: 1,
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
  if (isDesktopWeb) {
    return {
      backgroundColor: c.primary,
      borderRadius: 4,
    };
  }
  return {
    backgroundColor: c.primary,
    borderRadius: 14,
    shadowColor: c.primary,
    shadowOffset: { width: 0, height: 3 },
    shadowOpacity: 0.20,
    shadowRadius: 6,
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
  if (isDesktopWeb) {
    return {
      backgroundColor: c.surface,
      borderRadius: 4,
      borderWidth: 1,
      borderColor: c.primary,
    };
  }
  return {
    backgroundColor: c.glassSurface,
    borderRadius: 14,
    borderWidth: 1,
    borderColor: c.border,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.04,
    shadowRadius: 4,
    elevation: 1,
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
  if (isDesktopWeb) {
    return {
      backgroundColor: c.error,
      borderRadius: 4,
    };
  }
  return {
    backgroundColor: c.error,
    borderRadius: 14,
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
  if (isDesktopWeb) {
    return {
      backgroundColor: c.surface,
      borderRadius: 4,
      borderWidth: 1,
      borderColor: c.borderInput,
      paddingHorizontal: 10,
      paddingVertical: 7,
    };
  }
  return {
    backgroundColor: c.glassSurface,
    borderRadius: 12,
    paddingHorizontal: 14,
    paddingVertical: 12,
    borderWidth: 0.5,
    borderColor: c.glassShadowEdge,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.03,
    shadowRadius: 3,
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
  if (isDesktopWeb) {
    return {
      backgroundColor: c.surfaceVariant,
      borderRadius: 24,
    };
  }
  return {
    backgroundColor: c.glassSurfaceVariant,
    borderRadius: 24,
    borderWidth: 0.5,
    borderColor: c.glassShadowEdge,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.04,
    shadowRadius: 4,
    elevation: 1,
  };
}

/** Desktop sidebar surface — solid on desktop, glass on web */
export function sidebarSurface(c: Colors): ViewStyle {
  if (isDesktopWeb) {
    return {
      backgroundColor: c.surface,
      borderRightWidth: 1,
      borderRightColor: c.border,
    };
  }
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

/** Desktop side-panel surface — solid on desktop, glass on web */
export function panelSurface(c: Colors): ViewStyle {
  if (isDesktopWeb) {
    return {
      backgroundColor: c.surface,
      borderRightWidth: 1,
      borderRightColor: c.border,
    };
  }
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
      elevation: 4,
    };
  }
  if (isDesktopWeb) {
    return {
      backgroundColor: c.primary,
      borderRadius: 4,
      shadowColor: '#000',
      shadowOffset: { width: 0, height: 1 },
      shadowOpacity: 0.12,
      shadowRadius: 3,
    };
  }
  return {
    backgroundColor: c.primary,
    borderRadius: 28,
    elevation: 6,
    shadowColor: c.primary,
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.25,
    shadowRadius: 12,
  };
}

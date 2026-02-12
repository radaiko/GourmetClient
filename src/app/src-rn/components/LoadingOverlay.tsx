import { ActivityIndicator, Platform, StyleSheet, View } from 'react-native';
import { AdaptiveBlurView } from './AdaptiveBlurView';
import { useFlatStyle } from '../utils/platform';
import { useTheme } from '../theme/useTheme';
import { Colors } from '../theme/colors';
import { bannerSurface } from '../theme/platformStyles';

export function LoadingOverlay() {
  const { colors } = useTheme();
  const styles = createStyles(colors);

  const spinner = (
    <View style={styles.spinnerContainer}>
      <ActivityIndicator size="large" color={colors.primary} />
    </View>
  );

  if (useFlatStyle) {
    return <View style={styles.opaqueContainer}>{spinner}</View>;
  }

  return (
    <AdaptiveBlurView
      intensity={colors.blurIntensityStrong}
      tint={colors.blurTint as any}
      style={styles.container}
    >
      {spinner}
    </AdaptiveBlurView>
  );
}

const createStyles = (c: Colors) =>
  StyleSheet.create({
    container: {
      ...StyleSheet.absoluteFillObject,
      justifyContent: 'center',
      alignItems: 'center',
      zIndex: 10,
    },
    opaqueContainer: {
      ...StyleSheet.absoluteFillObject,
      justifyContent: 'center',
      alignItems: 'center',
      zIndex: 10,
      backgroundColor: c.overlay,
    },
    spinnerContainer: {
      borderRadius: 24,
      padding: 28,
      ...bannerSurface(c),
      elevation: 6,
    },
  });

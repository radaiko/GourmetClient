import { ActivityIndicator, Platform, StyleSheet, View } from 'react-native';
import { BlurView } from 'expo-blur';
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

  if (Platform.OS === 'android') {
    return <View style={styles.androidContainer}>{spinner}</View>;
  }

  return (
    <BlurView
      intensity={colors.blurIntensityStrong}
      tint={colors.blurTint as any}
      style={styles.container}
    >
      {spinner}
    </BlurView>
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
    androidContainer: {
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

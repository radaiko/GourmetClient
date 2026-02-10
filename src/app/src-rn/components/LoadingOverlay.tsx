import { ActivityIndicator, StyleSheet, View } from 'react-native';
import { BlurView } from 'expo-blur';
import { useTheme } from '../theme/useTheme';
import { Colors } from '../theme/colors';

export function LoadingOverlay() {
  const { colors } = useTheme();
  const styles = createStyles(colors);

  return (
    <BlurView
      intensity={colors.blurIntensityStrong}
      tint={colors.blurTint as any}
      style={styles.container}
    >
      <View style={styles.spinnerContainer}>
        <ActivityIndicator size="large" color={colors.primary} />
      </View>
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
    spinnerContainer: {
      backgroundColor: c.glassSurface,
      borderRadius: 24,
      padding: 28,
      borderTopWidth: 1,
      borderLeftWidth: 0.5,
      borderBottomWidth: 0.5,
      borderTopColor: c.glassHighlight,
      borderLeftColor: c.glassHighlight,
      borderBottomColor: c.glassShadowEdge,
      borderRightWidth: 0,
      shadowColor: '#000',
      shadowOffset: { width: 0, height: 8 },
      shadowOpacity: 0.20,
      shadowRadius: 24,
      elevation: 6,
    },
  });

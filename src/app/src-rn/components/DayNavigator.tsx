import { Platform, Pressable, StyleSheet, Text, View } from 'react-native';
import { AdaptiveBlurView } from './AdaptiveBlurView';
import { formatDisplayDate } from '../utils/dateUtils';
import { useTheme } from '../theme/useTheme';
import { Colors } from '../theme/colors';
import { circleButton } from '../theme/platformStyles';
interface DayNavigatorProps {
  dates: Date[];
  selectedDate: Date;
  onSelectDate: (date: Date) => void;
}

export function DayNavigator({ dates, selectedDate, onSelectDate }: DayNavigatorProps) {
  const { colors } = useTheme();
  const styles = createStyles(colors);

  const currentIndex = dates.findIndex(
    (d) => d.toDateString() === selectedDate.toDateString()
  );

  const goBack = () => {
    if (currentIndex > 0) {
      onSelectDate(dates[currentIndex - 1]);
    }
  };

  const goForward = () => {
    if (currentIndex < dates.length - 1) {
      onSelectDate(dates[currentIndex + 1]);
    }
  };

  const content = (
    <View style={styles.container}>
      <Pressable
        onPress={goBack}
        style={[styles.arrow, currentIndex <= 0 && styles.arrowDisabled]}
        disabled={currentIndex <= 0}
      >
        <Text style={styles.arrowText}>&#x276E;</Text>
      </Pressable>

      <View style={styles.dateContainer}>
        <Text style={styles.dateText}>
          {formatDisplayDate(selectedDate)}
        </Text>
        <Text style={styles.pageText}>
          {currentIndex + 1} / {dates.length}
        </Text>
      </View>

      <Pressable
        onPress={goForward}
        style={[styles.arrow, currentIndex >= dates.length - 1 && styles.arrowDisabled]}
        disabled={currentIndex >= dates.length - 1}
      >
        <Text style={styles.arrowText}>&#x276F;</Text>
      </Pressable>
    </View>
  );

  if (Platform.OS === 'android') {
    return <View style={styles.androidWrapper}>{content}</View>;
  }

  return (
    <AdaptiveBlurView
      intensity={colors.blurIntensity}
      tint={colors.blurTint as any}
      style={styles.blurWrapper}
    >
      {content}
    </AdaptiveBlurView>
  );
}

const createStyles = (c: Colors) =>
  StyleSheet.create({
    blurWrapper: {
      borderBottomWidth: 0.5,
      borderBottomColor: c.glassShadowEdge,
    },
    androidWrapper: {
      backgroundColor: c.surface,
      borderBottomWidth: 1,
      borderBottomColor: c.border,
      elevation: 2,
    },
    container: {
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'space-between',
      paddingHorizontal: 16,
      paddingVertical: 12,
    },
    arrow: {
      width: 48,
      height: 48,
      justifyContent: 'center',
      alignItems: 'center',
      ...circleButton(c),
    },
    arrowDisabled: {
      opacity: 0.3,
    },
    arrowText: {
      fontSize: 20,
      color: c.textPrimary,
    },
    dateContainer: {
      alignItems: 'center',
    },
    dateText: {
      fontSize: 19,
      fontWeight: '600',
      color: c.textPrimary,
    },
    pageText: {
      fontSize: 12,
      color: c.textTertiary,
      marginTop: 2,
    },
  });

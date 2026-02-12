import { Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { formatDisplayDate } from '../utils/dateUtils';
import { useTheme } from '../theme/useTheme';
import { Colors } from '../theme/colors';
import { panelSurface } from '../theme/platformStyles';

interface DateListPanelProps {
  dates: Date[];
  selectedDate: Date;
  onSelectDate: (date: Date) => void;
  width: number;
}

export function DateListPanel({ dates, selectedDate, onSelectDate, width }: DateListPanelProps) {
  const { colors } = useTheme();
  const styles = createStyles(colors, width);

  return (
    <View style={styles.wrapper}>
      <Text style={styles.header}>Termine</Text>
      <ScrollView style={styles.scroll} showsVerticalScrollIndicator={false}>
        {dates.map((date) => {
          const isSelected = date.toDateString() === selectedDate.toDateString();
          return (
            <Pressable
              key={date.toISOString()}
              style={[styles.item, isSelected && styles.itemActive]}
              onPress={() => onSelectDate(date)}
            >
              {isSelected && <View style={styles.activeAccent} />}
              <Text style={[styles.itemText, isSelected && styles.itemTextActive]}>
                {formatDisplayDate(date)}
              </Text>
            </Pressable>
          );
        })}
      </ScrollView>
    </View>
  );
}

const createStyles = (c: Colors, width: number) =>
  StyleSheet.create({
    wrapper: {
      width,
      ...panelSurface(c),
    },
    header: {
      fontSize: 10,
      fontWeight: '700',
      color: c.textTertiary,
      textTransform: 'uppercase',
      letterSpacing: 0.8,
      paddingHorizontal: 12,
      paddingTop: 12,
      paddingBottom: 4,
    },
    scroll: {
      flex: 1,
      paddingHorizontal: 6,
    },
    item: {
      paddingVertical: 6,
      paddingHorizontal: 10,
      borderRadius: 4,
      position: 'relative',
    },
    itemActive: {
      backgroundColor: c.glassPrimary,
    },
    activeAccent: {
      position: 'absolute',
      left: 0,
      top: 4,
      bottom: 4,
      width: 2,
      borderRadius: 1,
      backgroundColor: c.primary,
    },
    itemText: {
      fontSize: 13,
      fontWeight: '400',
      color: c.textSecondary,
    },
    itemTextActive: {
      color: c.primary,
      fontWeight: '600',
    },
  });

import { Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { useTheme } from '../theme/useTheme';
import { Colors } from '../theme/colors';
import { panelSurface, buttonPrimary } from '../theme/platformStyles';

type Tab = 'upcoming' | 'past';

const VIEW_OPTIONS: { value: Tab; label: string }[] = [
  { value: 'upcoming', label: 'Upcoming' },
  { value: 'past', label: 'Past' },
];

interface OrdersPanelProps {
  width: number;
  activeTab: Tab;
  onSelectTab: (tab: Tab) => void;
  upcomingCount: number;
  pastCount: number;
  unconfirmedCount: number;
  onConfirm: () => void;
  loading: boolean;
}

export function OrdersPanel({
  width,
  activeTab,
  onSelectTab,
  upcomingCount,
  pastCount,
  unconfirmedCount,
  onConfirm,
  loading,
}: OrdersPanelProps) {
  const { colors } = useTheme();
  const styles = createStyles(colors, width);

  const countFor = (tab: Tab) => (tab === 'upcoming' ? upcomingCount : pastCount);

  return (
    <View style={styles.wrapper}>
      <ScrollView showsVerticalScrollIndicator={false}>
        {/* View selector */}
        <Text style={styles.sectionHeader}>View</Text>
        <View style={styles.section}>
          {VIEW_OPTIONS.map((opt) => {
            const isSelected = activeTab === opt.value;
            return (
              <Pressable
                key={opt.value}
                style={[styles.filterItem, isSelected && styles.filterItemActive]}
                onPress={() => onSelectTab(opt.value)}
              >
                {isSelected && <View style={styles.activeAccent} />}
                <Text style={[styles.filterText, isSelected && styles.filterTextActive]}>
                  {opt.label} ({countFor(opt.value)})
                </Text>
              </Pressable>
            );
          })}
        </View>

        {/* Summary */}
        <Text style={styles.sectionHeader}>Summary</Text>
        <View style={styles.summarySection}>
          <View style={styles.summaryRow}>
            <Text style={styles.summaryLabel}>Upcoming</Text>
            <Text style={styles.summaryValue}>{upcomingCount}</Text>
          </View>
          <View style={styles.summaryRow}>
            <Text style={styles.summaryLabel}>Past</Text>
            <Text style={styles.summaryValue}>{pastCount}</Text>
          </View>
          <View style={styles.summaryRow}>
            <Text style={styles.summaryLabel}>Unconfirmed</Text>
            <Text
              style={[
                styles.summaryValue,
                unconfirmedCount > 0 && { color: colors.warning },
              ]}
            >
              {unconfirmedCount}
            </Text>
          </View>
        </View>

        {/* Confirm section */}
        {unconfirmedCount > 0 && (
          <>
            <Text style={styles.sectionHeader}>Confirm</Text>
            <View style={styles.confirmSection}>
              <Pressable
                style={styles.confirmButton}
                onPress={onConfirm}
                disabled={loading}
              >
                <Text style={styles.confirmButtonText}>Confirm All</Text>
              </Pressable>
            </View>
          </>
        )}
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
    sectionHeader: {
      fontSize: 10,
      fontWeight: '700',
      color: c.textTertiary,
      textTransform: 'uppercase',
      letterSpacing: 0.8,
      paddingHorizontal: 12,
      paddingTop: 12,
      paddingBottom: 4,
    },
    section: {
      paddingHorizontal: 6,
    },
    filterItem: {
      paddingVertical: 6,
      paddingHorizontal: 10,
      borderRadius: 4,
      position: 'relative',
    },
    filterItemActive: {
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
    filterText: {
      fontSize: 13,
      fontWeight: '400',
      color: c.textSecondary,
    },
    filterTextActive: {
      color: c.primary,
      fontWeight: '600',
    },
    summarySection: {
      paddingHorizontal: 12,
      paddingTop: 4,
      gap: 6,
    },
    summaryRow: {
      flexDirection: 'row',
      justifyContent: 'space-between',
      alignItems: 'center',
    },
    summaryLabel: {
      fontSize: 12,
      color: c.textTertiary,
      fontWeight: '500',
    },
    summaryValue: {
      fontSize: 14,
      fontWeight: '700',
      color: c.textPrimary,
    },
    confirmSection: {
      paddingHorizontal: 12,
      paddingTop: 6,
    },
    confirmButton: {
      paddingVertical: 6,
      alignItems: 'center',
      ...buttonPrimary(c),
    },
    confirmButtonText: {
      color: '#fff',
      fontWeight: '700',
      fontSize: 12,
    },
  });

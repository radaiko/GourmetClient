import { Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { useTheme } from '../theme/useTheme';
import { Colors } from '../theme/colors';
import { panelSurface } from '../theme/platformStyles';
import { BillingSource } from '../store/billingStore';

interface MonthOption {
  key: string;
  label: string;
  offset: number;
}

interface Totals {
  total: number;
  count: number;
  subsidy: number;
}

const SOURCE_FILTERS: { value: BillingSource; label: string }[] = [
  { value: 'all', label: 'Alle' },
  { value: 'gourmet', label: 'Kantine' },
  { value: 'ventopay', label: 'Automaten' },
];

function formatCurrency(value: number): string {
  return value.toLocaleString('de-AT', { style: 'currency', currency: 'EUR' });
}

interface BillingFiltersPanelProps {
  width: number;
  monthOptions: MonthOption[];
  selectedMonthIndex: number;
  onSelectMonth: (index: number) => void;
  sourceFilter: BillingSource;
  onSetSourceFilter: (source: BillingSource) => void;
  totals: Totals;
}

export function BillingFiltersPanel({
  width,
  monthOptions,
  selectedMonthIndex,
  onSelectMonth,
  sourceFilter,
  onSetSourceFilter,
  totals,
}: BillingFiltersPanelProps) {
  const { colors } = useTheme();
  const styles = createStyles(colors, width);

  return (
    <View style={styles.wrapper}>
      <ScrollView showsVerticalScrollIndicator={false}>
        {/* Month selector */}
        <Text style={styles.sectionHeader}>Monat</Text>
        <View style={styles.section}>
          {monthOptions.map((opt, idx) => {
            const isSelected = selectedMonthIndex === idx;
            return (
              <Pressable
                key={opt.key}
                style={[styles.filterItem, isSelected && styles.filterItemActive]}
                onPress={() => onSelectMonth(idx)}
              >
                {isSelected && <View style={styles.activeAccent} />}
                <Text style={[styles.filterText, isSelected && styles.filterTextActive]}>
                  {opt.label}
                </Text>
              </Pressable>
            );
          })}
        </View>

        {/* Source filter */}
        <Text style={styles.sectionHeader}>Quelle</Text>
        <View style={styles.section}>
          {SOURCE_FILTERS.map((sf) => {
            const isSelected = sourceFilter === sf.value;
            return (
              <Pressable
                key={sf.value}
                style={[styles.filterItem, isSelected && styles.filterItemActive]}
                onPress={() => onSetSourceFilter(sf.value)}
              >
                {isSelected && <View style={styles.activeAccent} />}
                <Text style={[styles.filterText, isSelected && styles.filterTextActive]}>
                  {sf.label}
                </Text>
              </Pressable>
            );
          })}
        </View>

        {/* Summary */}
        <Text style={styles.sectionHeader}>Übersicht</Text>
        <View style={styles.summarySection}>
          <View style={styles.summaryRow}>
            <Text style={styles.summaryLabel}>Gesamt</Text>
            <Text style={styles.summaryValue}>{formatCurrency(totals.total)}</Text>
          </View>
          <View style={styles.summaryRow}>
            <Text style={styles.summaryLabel}>Belege</Text>
            <Text style={styles.summaryValue}>{totals.count}</Text>
          </View>
          {totals.subsidy > 0 && (
            <View style={styles.summaryRow}>
              <Text style={styles.summaryLabel}>Zuschuss</Text>
              <Text style={[styles.summaryValue, { color: colors.success }]}>
                {formatCurrency(totals.subsidy)}
              </Text>
            </View>
          )}
        </View>
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
  });

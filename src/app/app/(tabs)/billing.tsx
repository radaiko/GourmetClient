import { useCallback, useMemo } from 'react';
import {
  ActivityIndicator,
  FlatList,
  Pressable,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { useFocusEffect } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useAuthStore } from '../../src-rn/store/authStore';
import { useVentopayAuthStore } from '../../src-rn/store/ventopayAuthStore';
import { useBillingStore, BillingSource } from '../../src-rn/store/billingStore';
import { BillCard, BillingEntry } from '../../src-rn/components/BillCard';
import { useTheme } from '../../src-rn/theme/useTheme';
import { Colors } from '../../src-rn/theme/colors';

const SOURCE_FILTERS: { value: BillingSource; label: string }[] = [
  { value: 'all', label: 'All' },
  { value: 'gourmet', label: 'Gourmet' },
  { value: 'ventopay', label: 'Ventopay' },
];

function formatCurrency(value: number): string {
  return value.toLocaleString('de-AT', { style: 'currency', currency: 'EUR' });
}

export default function BillingScreen() {
  const { colors } = useTheme();
  const insets = useSafeAreaInsets();
  const styles = useMemo(() => createStyles(colors), [colors]);

  const { status: gourmetAuthStatus } = useAuthStore();
  const { status: ventopayAuthStatus } = useVentopayAuthStore();

  const {
    loading,
    error,
    selectedMonthIndex,
    sourceFilter,
    getMonthOptions,
    getSelectedGourmetBilling,
    getSelectedVentopayBilling,
    fetchBilling,
    fetchVentopayBilling,
    selectMonth,
    setSourceFilter,
    loadCachedMonths,
  } = useBillingStore();

  const monthOptions = getMonthOptions();
  const gourmetBilling = getSelectedGourmetBilling();
  const ventopayBilling = getSelectedVentopayBilling();

  // Build unified entries sorted by date (newest first)
  const entries = useMemo((): BillingEntry[] => {
    const result: BillingEntry[] = [];

    if (sourceFilter !== 'ventopay' && gourmetBilling) {
      for (const bill of gourmetBilling.bills) {
        result.push({ source: 'gourmet', data: bill });
      }
    }

    if (sourceFilter !== 'gourmet' && ventopayBilling) {
      for (const tx of ventopayBilling.transactions) {
        result.push({ source: 'ventopay', data: tx });
      }
    }

    result.sort((a, b) => {
      const dateA = a.source === 'gourmet' ? a.data.billDate : a.data.date;
      const dateB = b.source === 'gourmet' ? b.data.billDate : b.data.date;
      return dateB.getTime() - dateA.getTime();
    });

    return result;
  }, [sourceFilter, gourmetBilling, ventopayBilling]);

  // Compute totals based on active filter
  const totals = useMemo(() => {
    let gourmetTotal = 0;
    let ventopayTotal = 0;

    if (sourceFilter !== 'ventopay' && gourmetBilling) {
      gourmetTotal = gourmetBilling.totalBilling;
    }
    if (sourceFilter !== 'gourmet' && ventopayBilling) {
      ventopayTotal = ventopayBilling.total;
    }

    return {
      total: gourmetTotal + ventopayTotal,
      gourmetTotal,
      ventopayTotal,
      subsidy: sourceFilter !== 'ventopay' ? (gourmetBilling?.totalSubsidy ?? 0) : 0,
      count: entries.length,
    };
  }, [sourceFilter, gourmetBilling, ventopayBilling, entries.length]);

  const hasAnyAuth = gourmetAuthStatus === 'authenticated' || ventopayAuthStatus === 'authenticated';

  useFocusEffect(
    useCallback(() => {
      if (hasAnyAuth) {
        loadCachedMonths().catch(() => {}).finally(() => {
          if (gourmetAuthStatus === 'authenticated') fetchBilling();
          if (ventopayAuthStatus === 'authenticated') fetchVentopayBilling();
        });
      }
    }, [hasAnyAuth, gourmetAuthStatus, ventopayAuthStatus, loadCachedMonths, fetchBilling, fetchVentopayBilling])
  );

  if (!hasAnyAuth) {
    return (
      <View style={[styles.container, styles.center, { paddingTop: insets.top }]}>
        <Text style={styles.emptyText}>Login required</Text>
      </View>
    );
  }

  const hasData = entries.length > 0;

  return (
    <View style={[styles.container, { paddingTop: insets.top }]}>
      {/* Month selector */}
      <View style={styles.monthSelector}>
        {monthOptions.map((opt, idx) => (
          <Pressable
            key={opt.key}
            style={[styles.monthTab, selectedMonthIndex === idx && styles.monthTabActive]}
            onPress={() => selectMonth(idx)}
          >
            <Text
              style={[
                styles.monthTabText,
                selectedMonthIndex === idx && styles.monthTabTextActive,
              ]}
              numberOfLines={1}
            >
              {opt.label}
            </Text>
          </Pressable>
        ))}
      </View>

      {/* Source filter tabs */}
      <View style={styles.sourceFilter}>
        {SOURCE_FILTERS.map((sf) => (
          <Pressable
            key={sf.value}
            style={[styles.sourceTab, sourceFilter === sf.value && styles.sourceTabActive]}
            onPress={() => setSourceFilter(sf.value)}
          >
            <Text
              style={[
                styles.sourceTabText,
                sourceFilter === sf.value && styles.sourceTabTextActive,
              ]}
            >
              {sf.label}
            </Text>
          </Pressable>
        ))}
      </View>

      {error && (
        <View style={styles.errorBanner}>
          <Text style={styles.errorText}>{error}</Text>
        </View>
      )}

      {loading && !hasData && (
        <View style={styles.center}>
          <ActivityIndicator size="large" color={colors.primary} />
        </View>
      )}

      {hasData && (
        <>
          {/* Summary bar */}
          <View style={styles.summaryBar}>
            <View style={styles.summaryItem}>
              <Text style={styles.summaryLabel}>Total</Text>
              <Text style={styles.summaryValue}>{formatCurrency(totals.total)}</Text>
            </View>
            <View style={styles.summaryItem}>
              <Text style={styles.summaryLabel}>Bills</Text>
              <Text style={styles.summaryValue}>{totals.count}</Text>
            </View>
            {totals.subsidy > 0 && (
              <View style={styles.summaryItem}>
                <Text style={styles.summaryLabel}>Subsidy</Text>
                <Text style={[styles.summaryValue, { color: colors.success }]}>
                  {formatCurrency(totals.subsidy)}
                </Text>
              </View>
            )}
          </View>

          <FlatList
            data={entries}
            keyExtractor={(item) =>
              item.source === 'gourmet'
                ? `g-${item.data.billNr}`
                : `v-${item.data.id}`
            }
            contentContainerStyle={styles.list}
            renderItem={({ item }) => <BillCard entry={item} />}
          />
        </>
      )}

      {!loading && !hasData && (
        <View style={styles.center}>
          <Text style={styles.emptyText}>No billing data for this month</Text>
        </View>
      )}
    </View>
  );
}

const createStyles = (c: Colors) =>
  StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: c.background,
    },
    center: {
      flex: 1,
      justifyContent: 'center',
      alignItems: 'center',
      padding: 20,
    },
    monthSelector: {
      flexDirection: 'row',
      backgroundColor: c.glassSurface,
      borderBottomWidth: 0.5,
      borderBottomColor: c.glassHighlight,
    },
    monthTab: {
      flex: 1,
      paddingVertical: 14,
      alignItems: 'center',
      borderBottomWidth: 3,
      borderBottomColor: 'transparent',
    },
    monthTabActive: {
      borderBottomColor: c.primary,
    },
    monthTabText: {
      fontSize: 13,
      fontWeight: '600',
      color: c.textTertiary,
    },
    monthTabTextActive: {
      color: c.primary,
    },
    sourceFilter: {
      flexDirection: 'row',
      paddingHorizontal: 16,
      paddingVertical: 8,
      gap: 8,
    },
    sourceTab: {
      paddingHorizontal: 14,
      paddingVertical: 6,
      borderRadius: 20,
      backgroundColor: c.glassSurface,
      borderWidth: 1,
      borderColor: c.glassHighlight,
    },
    sourceTabActive: {
      backgroundColor: c.glassPrimary,
      borderColor: c.primary,
    },
    sourceTabText: {
      fontSize: 12,
      fontWeight: '600',
      color: c.textTertiary,
    },
    sourceTabTextActive: {
      color: c.primary,
    },
    summaryBar: {
      flexDirection: 'row',
      justifyContent: 'space-around',
      backgroundColor: c.glassSurface,
      paddingVertical: 12,
      paddingHorizontal: 16,
      marginHorizontal: 16,
      marginTop: 4,
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
    },
    summaryItem: {
      alignItems: 'center',
    },
    summaryLabel: {
      fontSize: 11,
      color: c.textTertiary,
      fontWeight: '600',
      marginBottom: 2,
    },
    summaryValue: {
      fontSize: 18,
      fontWeight: '700',
      color: c.textPrimary,
    },
    list: {
      padding: 16,
      paddingBottom: 100,
    },
    emptyText: {
      fontSize: 16,
      color: c.textTertiary,
    },
    errorBanner: {
      backgroundColor: c.glassError,
      padding: 12,
      marginHorizontal: 16,
      marginTop: 8,
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
    },
    errorText: {
      color: c.errorText,
      fontSize: 13,
    },
  });

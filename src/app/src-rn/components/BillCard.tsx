import { StyleSheet, Text, View } from 'react-native';
import { GourmetBill } from '../types/billing';
import { VentopayTransaction } from '../types/ventopay';
import { isCompactDesktop } from '../utils/platform';
import { useTheme } from '../theme/useTheme';
import { Colors } from '../theme/colors';
import { cardSurface } from '../theme/platformStyles';

/** Unified type for rendering both Gourmet and Ventopay entries */
export type BillingEntry =
  | { source: 'gourmet'; data: GourmetBill }
  | { source: 'ventopay'; data: VentopayTransaction };

interface BillCardProps {
  entry: BillingEntry;
}

function formatCurrency(value: number): string {
  return value.toLocaleString('de-AT', { style: 'currency', currency: 'EUR' });
}

function formatBillDate(date: Date): string {
  return date.toLocaleDateString('de-AT', {
    weekday: 'short',
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  });
}

function formatBillTime(date: Date): string {
  return date.toLocaleTimeString('de-AT', {
    hour: '2-digit',
    minute: '2-digit',
  });
}

function GourmetBillCard({ bill, colors }: { bill: GourmetBill; colors: Colors }) {
  const styles = createStyles(colors);

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <View>
          <Text style={styles.date}>{formatBillDate(bill.billDate)}</Text>
          <Text style={styles.time}>{formatBillTime(bill.billDate)}</Text>
        </View>
        <View style={styles.headerRight}>
          <View style={[styles.badge, styles.badgeGourmet]}>
            <Text style={styles.badgeText}>Kantine</Text>
          </View>
          <Text style={styles.billing}>{formatCurrency(bill.billing)}</Text>
        </View>
      </View>

      <View style={styles.items}>
        {bill.items.map((item, idx) => (
          <View key={`${item.id}-${idx}`} style={styles.itemRow}>
            <Text style={styles.itemCount}>{item.count}x</Text>
            <Text style={styles.itemDescription} numberOfLines={1}>
              {item.description}
            </Text>
            <Text style={styles.itemTotal}>{formatCurrency(item.total)}</Text>
          </View>
        ))}
      </View>
    </View>
  );
}

function VentopayBillCard({ transaction, colors }: { transaction: VentopayTransaction; colors: Colors }) {
  const styles = createStyles(colors);

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <View>
          <Text style={styles.date}>{formatBillDate(transaction.date)}</Text>
          <Text style={styles.time}>{formatBillTime(transaction.date)}</Text>
        </View>
        <View style={styles.headerRight}>
          <View style={[styles.badge, styles.badgeVentopay]}>
            <Text style={styles.badgeText}>Automaten</Text>
          </View>
          <Text style={styles.billing}>{formatCurrency(transaction.amount)}</Text>
        </View>
      </View>

      {transaction.restaurant ? (
        <Text style={styles.restaurant} numberOfLines={1}>
          {transaction.restaurant}
        </Text>
      ) : null}
    </View>
  );
}

export function BillCard({ entry }: BillCardProps) {
  const { colors } = useTheme();

  if (entry.source === 'gourmet') {
    return <GourmetBillCard bill={entry.data} colors={colors} />;
  }
  return <VentopayBillCard transaction={entry.data} colors={colors} />;
}

const createStyles = (c: Colors) =>
  StyleSheet.create({
    container: {
      padding: isCompactDesktop ? 10 : 16,
      marginBottom: isCompactDesktop ? 4 : 8,
      ...cardSurface(c),
    },
    header: {
      flexDirection: 'row',
      justifyContent: 'space-between',
      alignItems: 'flex-start',
      marginBottom: isCompactDesktop ? 8 : 12,
    },
    headerRight: {
      alignItems: 'flex-end',
      gap: 4,
    },
    date: {
      fontSize: isCompactDesktop ? 13 : 14,
      fontWeight: '600',
      color: c.textPrimary,
    },
    time: {
      fontSize: 12,
      color: c.textTertiary,
      marginTop: 2,
    },
    billing: {
      fontSize: isCompactDesktop ? 15 : 18,
      fontWeight: '700',
      color: c.primary,
    },
    badge: {
      paddingHorizontal: 8,
      paddingVertical: 2,
      borderRadius: 8,
    },
    badgeGourmet: {
      backgroundColor: c.glassPrimary,
    },
    badgeVentopay: {
      backgroundColor: c.glassSuccess,
    },
    badgeText: {
      fontSize: 10,
      fontWeight: '700',
      color: c.textSecondary,
      textTransform: 'uppercase',
      letterSpacing: 0.5,
    },
    restaurant: {
      fontSize: 13,
      color: c.textSecondary,
      marginTop: -4,
      marginBottom: 4,
    },
    items: {
      gap: 6,
    },
    itemRow: {
      flexDirection: 'row',
      alignItems: 'center',
    },
    itemCount: {
      fontSize: 13,
      color: c.textTertiary,
      width: 28,
    },
    itemDescription: {
      flex: 1,
      fontSize: 14,
      color: c.textSecondary,
    },
    itemTotal: {
      fontSize: 13,
      color: c.textTertiary,
      marginLeft: 8,
    },
  });

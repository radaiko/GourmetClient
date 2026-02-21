import { useState, useEffect } from 'react';
import { ActivityIndicator, Pressable, StyleSheet, Text, View } from 'react-native';
import { GourmetOrderedMenu } from '../types/order';
import { formatDisplayDate, isCancellationCutoff } from '../utils/dateUtils';
import { useFlatStyle, isCompactDesktop } from '../utils/platform';
import { useTheme } from '../theme/useTheme';
import { Colors } from '../theme/colors';
import { cardSurface } from '../theme/platformStyles';

interface OrderItemProps {
  order: GourmetOrderedMenu;
  menuDescription?: string;
  isCancelling: boolean;
  onCancel: () => void;
  canCancel: boolean;
}

export function OrderItem({ order, menuDescription, isCancelling, onCancel, canCancel }: OrderItemProps) {
  const { colors } = useTheme();
  const styles = createStyles(colors);
  const [cutoff, setCutoff] = useState(() => isCancellationCutoff(order.date));

  useEffect(() => {
    if (cutoff) return; // already locked
    const timer = setInterval(
      () => setCutoff(isCancellationCutoff(order.date)),
      30_000, // re-check every 30s
    );
    return () => clearInterval(timer);
  }, [order.date, cutoff]);

  const disabled = cutoff;

  return (
    <View style={[styles.container, isCancelling && styles.containerCancelling]}>
      <View style={styles.left}>
        <Text style={styles.date}>{formatDisplayDate(order.date)}</Text>
        <Text style={styles.categoryLabel}>{order.title}</Text>
        <Text style={styles.title} numberOfLines={2}>
          {menuDescription || order.subtitle || order.title}
        </Text>
      </View>
      <View style={styles.right}>
        {order.approved ? (
          <View style={styles.badge}>
            <Text style={styles.badgeText}>Bestätigt</Text>
          </View>
        ) : (
          <View style={[styles.badge, styles.badgePending]}>
            <Text style={[styles.badgeText, styles.badgePendingText]}>Ausstehend</Text>
          </View>
        )}
        {canCancel && (
          isCancelling ? (
            <ActivityIndicator size="small" color={colors.error} style={styles.cancelButton} />
          ) : (
            <Pressable
              style={[styles.cancelButton, cutoff && styles.cancelButtonDisabled]}
              onPress={onCancel}
              hitSlop={8}
              disabled={disabled}
            >
              <Text style={[styles.cancelX, cutoff && styles.cancelXDisabled]}>&#x2715;</Text>
            </Pressable>
          )
        )}
      </View>
    </View>
  );
}

const createStyles = (c: Colors) =>
  StyleSheet.create({
    container: {
      flexDirection: 'row',
      justifyContent: 'space-between',
      alignItems: 'center',
      padding: isCompactDesktop ? 10 : 16,
      marginBottom: isCompactDesktop ? 4 : 8,
      ...cardSurface(c),
    },
    containerCancelling: {
      opacity: 0.6,
    },
    left: {
      flex: 1,
      marginRight: 12,
    },
    right: {
      alignItems: 'flex-end',
      gap: 8,
    },
    date: {
      fontSize: 12,
      color: c.textTertiary,
      fontWeight: '600',
      marginBottom: 2,
    },
    categoryLabel: {
      fontSize: 11,
      fontWeight: '700',
      color: c.primary,
      letterSpacing: 0.5,
      marginBottom: 2,
    },
    title: {
      fontSize: isCompactDesktop ? 13 : 16,
      color: c.textPrimary,
    },
    badge: {
      backgroundColor: useFlatStyle ? c.successSurface : c.glassSuccess,
      paddingHorizontal: 10,
      paddingVertical: 4,
      borderRadius: 12,
      borderWidth: useFlatStyle ? 1 : 0.5,
      borderColor: c.success,
    },
    badgeText: {
      fontSize: 11,
      fontWeight: '600',
      color: c.successText,
    },
    badgePending: {
      backgroundColor: useFlatStyle ? c.warningSurface : c.glassWarning,
      borderColor: c.warning,
    },
    badgePendingText: {
      color: c.warningText,
    },
    cancelButton: {
      width: 32,
      height: 32,
      borderRadius: 16,
      backgroundColor: useFlatStyle ? c.errorSurface : c.glassError,
      justifyContent: 'center',
      alignItems: 'center',
      borderWidth: useFlatStyle ? 1 : 0.5,
      borderColor: c.error,
    },
    cancelButtonDisabled: {
      opacity: 0.4,
      borderColor: c.textTertiary,
      backgroundColor: useFlatStyle ? c.surfaceVariant : c.glassSurfaceVariant,
    },
    cancelX: {
      fontSize: 16,
      fontWeight: '700',
      color: c.error,
    },
    cancelXDisabled: {
      color: c.textTertiary,
    },
  });

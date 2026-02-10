import { ActivityIndicator, Pressable, StyleSheet, Text, View } from 'react-native';
import { GourmetOrderedMenu } from '../types/order';
import { formatDisplayDate } from '../utils/dateUtils';
import { useTheme } from '../theme/useTheme';
import { Colors } from '../theme/colors';

interface OrderItemProps {
  order: GourmetOrderedMenu;
  isCancelling: boolean;
  onCancel: () => void;
  canCancel: boolean;
}

export function OrderItem({ order, isCancelling, onCancel, canCancel }: OrderItemProps) {
  const { colors } = useTheme();
  const styles = createStyles(colors);

  return (
    <View style={[styles.container, isCancelling && styles.containerCancelling]}>
      <View style={styles.left}>
        <Text style={styles.date}>{formatDisplayDate(order.date)}</Text>
        <Text style={styles.categoryLabel}>{order.title}</Text>
        <Text style={styles.title} numberOfLines={2}>
          {order.subtitle || order.title}
        </Text>
      </View>
      <View style={styles.right}>
        {order.approved ? (
          <View style={styles.badge}>
            <Text style={styles.badgeText}>Confirmed</Text>
          </View>
        ) : (
          <View style={[styles.badge, styles.badgePending]}>
            <Text style={[styles.badgeText, styles.badgePendingText]}>Pending</Text>
          </View>
        )}
        {canCancel && (
          isCancelling ? (
            <ActivityIndicator size="small" color={colors.error} style={styles.cancelButton} />
          ) : (
            <Pressable
              style={styles.cancelButton}
              onPress={onCancel}
              hitSlop={8}
            >
              <Text style={styles.cancelX}>&#x2715;</Text>
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
      backgroundColor: c.glassSurface,
      borderRadius: 16,
      padding: 16,
      marginBottom: 8,
      borderTopWidth: 1,
      borderLeftWidth: 0.5,
      borderBottomWidth: 0.5,
      borderRightWidth: 0,
      borderTopColor: c.glassHighlight,
      borderLeftColor: c.glassHighlight,
      borderBottomColor: c.glassShadowEdge,
      shadowColor: '#000',
      shadowOffset: { width: 0, height: 3 },
      shadowOpacity: 0.06,
      shadowRadius: 8,
      elevation: 2,
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
      fontSize: 16,
      color: c.textPrimary,
    },
    badge: {
      backgroundColor: c.glassSuccess,
      paddingHorizontal: 10,
      paddingVertical: 4,
      borderRadius: 12,
      borderWidth: 0.5,
      borderColor: c.success,
    },
    badgeText: {
      fontSize: 11,
      fontWeight: '600',
      color: c.successText,
    },
    badgePending: {
      backgroundColor: c.glassWarning,
      borderColor: c.warning,
    },
    badgePendingText: {
      color: c.warningText,
    },
    cancelButton: {
      width: 32,
      height: 32,
      borderRadius: 16,
      backgroundColor: c.glassError,
      justifyContent: 'center',
      alignItems: 'center',
      borderWidth: 0.5,
      borderColor: c.error,
    },
    cancelX: {
      fontSize: 16,
      fontWeight: '700',
      color: c.error,
    },
  });

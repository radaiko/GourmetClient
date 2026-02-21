import { ActivityIndicator, Pressable, StyleSheet, Text, View } from 'react-native';
import { GourmetMenuItem } from '../types/menu';
import { isOrderingCutoff } from '../utils/dateUtils';
import { useFlatStyle, isCompactDesktop } from '../utils/platform';
import { useTheme } from '../theme/useTheme';
import { Colors } from '../theme/colors';
import { cardSurface } from '../theme/platformStyles';

interface MenuCardProps {
  item: GourmetMenuItem;
  isSelected: boolean;
  ordered: boolean;
  onToggle: () => void;
  onCancel?: () => void;
  isCancelling?: boolean;
}

export function MenuCard({ item, isSelected, ordered, onToggle, onCancel, isCancelling }: MenuCardProps) {
  const { colors } = useTheme();
  const styles = createStyles(colors);
  const cutoff = isOrderingCutoff(item.day);
  const canOrder = item.available && !ordered && !cutoff;

  return (
    <Pressable
      style={[
        styles.card,
        ordered && styles.cardOrdered,
        isSelected && styles.cardSelected,
        (!item.available || cutoff) && !ordered && styles.cardDisabled,
      ]}
      onPress={canOrder ? onToggle : undefined}
      disabled={!canOrder}
    >
      <View style={styles.badgeRow}>
        {ordered && (
          <View style={styles.orderedBadge}>
            <Text style={styles.orderedBadgeText}>Bestellt</Text>
          </View>
        )}
        {!ordered && !item.available && (
          <View style={styles.stockBadge}>
            <Text style={styles.stockBadgeText}>Ausverkauft</Text>
          </View>
        )}
        {cutoff && !ordered && item.available && (
          <View style={styles.cutoffBadge}>
            <Text style={styles.cutoffBadgeText}>Geschlossen</Text>
          </View>
        )}
      </View>
      <Text
        style={[styles.subtitle, isSelected && styles.textSelected]}
        numberOfLines={4}
      >
        {item.subtitle}
      </Text>
      <View style={styles.bottomRow}>
        <Text style={[styles.allergens, isSelected && styles.textSelected]} numberOfLines={1}>
          {item.allergens.length > 0 ? `Allergene: ${item.allergens.join(', ')}` : ''}
        </Text>
        {onCancel ? (
          <Pressable
            style={styles.cancelButton}
            onPress={onCancel}
            disabled={isCancelling}
          >
            {isCancelling ? (
              <ActivityIndicator size="small" color={colors.error} />
            ) : (
              <Text style={styles.cancelText}>Stornieren</Text>
            )}
          </Pressable>
        ) : (
          <Text style={[styles.price, isSelected && styles.textSelected]}>
            {item.price}
          </Text>
        )}
      </View>
      {isSelected && (
        <View style={styles.checkmark}>
          <Text style={styles.checkmarkText}>&#10003;</Text>
        </View>
      )}
    </Pressable>
  );
}

const createStyles = (c: Colors) =>
  StyleSheet.create({
    card: {
      padding: isCompactDesktop ? 10 : 16,
      marginBottom: isCompactDesktop ? 6 : 10,
      position: 'relative',
      ...cardSurface(c),
    },
    cardOrdered: {
      backgroundColor: useFlatStyle ? c.successSurface : c.glassSuccess,
      borderColor: useFlatStyle ? c.successBorder : undefined,
    },
    cardSelected: {
      backgroundColor: c.primary,
      borderColor: useFlatStyle ? c.primaryDark : undefined,
      borderTopColor: useFlatStyle ? c.primaryDark : 'rgba(255,255,255,0.30)',
      borderLeftColor: useFlatStyle ? c.primaryDark : 'rgba(255,255,255,0.15)',
      borderBottomColor: useFlatStyle ? c.primaryDark : c.primaryDark,
    },
    cardDisabled: {
      opacity: 0.5,
    },
    badgeRow: {
      flexDirection: 'row',
      gap: 6,
      marginBottom: 2,
    },
    orderedBadge: {
      backgroundColor: useFlatStyle ? c.successSurface : c.glassSuccess,
      paddingHorizontal: 8,
      paddingVertical: 2,
      borderRadius: 12,
      borderWidth: useFlatStyle ? 1 : 0.5,
      borderColor: c.success,
    },
    orderedBadgeText: {
      fontSize: 10,
      fontWeight: '700',
      color: c.successText,
    },
    stockBadge: {
      backgroundColor: useFlatStyle ? c.errorSurface : c.glassError,
      paddingHorizontal: 8,
      paddingVertical: 2,
      borderRadius: 12,
      borderWidth: useFlatStyle ? 1 : 0.5,
      borderColor: c.error,
    },
    stockBadgeText: {
      fontSize: 10,
      fontWeight: '700',
      color: c.errorText,
    },
    cutoffBadge: {
      backgroundColor: useFlatStyle ? c.warningSurface : c.glassWarning,
      paddingHorizontal: 8,
      paddingVertical: 2,
      borderRadius: 12,
      borderWidth: useFlatStyle ? 1 : 0.5,
      borderColor: c.warning,
    },
    cutoffBadgeText: {
      fontSize: 10,
      fontWeight: '700',
      color: c.warningText,
    },
    bottomRow: {
      flexDirection: 'row',
      justifyContent: 'space-between',
      alignItems: 'center',
      marginTop: 6,
    },
    price: {
      fontSize: 13,
      fontWeight: '600',
      color: c.textSecondary,
      flexShrink: 0,
      marginLeft: 8,
    },
    subtitle: {
      fontSize: isCompactDesktop ? 13 : 16,
      color: c.textPrimary,
      lineHeight: isCompactDesktop ? 18 : 21,
    },
    textSelected: {
      color: '#fff',
    },
    allergens: {
      flex: 1,
      fontSize: 11,
      color: c.textTertiary,
    },
    cancelButton: {
      paddingHorizontal: 12,
      paddingVertical: 4,
      borderRadius: 12,
      borderWidth: 1,
      borderColor: c.error,
      marginLeft: 8,
    },
    cancelText: {
      fontSize: 12,
      fontWeight: '600',
      color: c.error,
    },
    checkmark: {
      position: 'absolute',
      top: 8,
      right: 8,
      width: 24,
      height: 24,
      borderRadius: 12,
      backgroundColor: '#fff',
      justifyContent: 'center',
      alignItems: 'center',
    },
    checkmarkText: {
      color: c.primary,
      fontWeight: '700',
      fontSize: 14,
    },
  });

import { Platform, Pressable, StyleSheet, Text, View } from 'react-native';
import { GourmetMenuItem } from '../types/menu';
import { isOrderingCutoff } from '../utils/dateUtils';
import { useTheme } from '../theme/useTheme';
import { Colors } from '../theme/colors';
import { cardSurface } from '../theme/platformStyles';

interface MenuCardProps {
  item: GourmetMenuItem;
  isSelected: boolean;
  onToggle: () => void;
}

export function MenuCard({ item, isSelected, onToggle }: MenuCardProps) {
  const { colors } = useTheme();
  const styles = createStyles(colors);
  const cutoff = isOrderingCutoff(item.day);
  const canOrder = item.available && !item.ordered && !cutoff;

  return (
    <Pressable
      style={[
        styles.card,
        item.ordered && styles.cardOrdered,
        isSelected && styles.cardSelected,
        (!item.available || cutoff) && styles.cardDisabled,
      ]}
      onPress={canOrder ? onToggle : undefined}
      disabled={!canOrder}
    >
      <View style={styles.badgeRow}>
        {item.ordered && (
          <View style={styles.orderedBadge}>
            <Text style={styles.orderedBadgeText}>Ordered</Text>
          </View>
        )}
        {cutoff && !item.ordered && (
          <View style={styles.cutoffBadge}>
            <Text style={styles.cutoffBadgeText}>Closed</Text>
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
          {item.allergens.length > 0 ? `Allergens: ${item.allergens.join(', ')}` : ''}
        </Text>
        <Text style={[styles.price, isSelected && styles.textSelected]}>
          {item.price}
        </Text>
      </View>
      {isSelected && (
        <View style={styles.checkmark}>
          <Text style={styles.checkmarkText}>&#10003;</Text>
        </View>
      )}
    </Pressable>
  );
}

const isAndroid = Platform.OS === 'android';

const createStyles = (c: Colors) =>
  StyleSheet.create({
    card: {
      padding: 16,
      marginBottom: 10,
      position: 'relative',
      ...cardSurface(c),
    },
    cardOrdered: {
      backgroundColor: isAndroid ? c.successSurface : c.glassSuccess,
      borderColor: isAndroid ? c.successBorder : undefined,
    },
    cardSelected: {
      backgroundColor: c.primary,
      borderColor: isAndroid ? c.primaryDark : undefined,
      borderTopColor: isAndroid ? c.primaryDark : 'rgba(255,255,255,0.30)',
      borderLeftColor: isAndroid ? c.primaryDark : 'rgba(255,255,255,0.15)',
      borderBottomColor: isAndroid ? c.primaryDark : c.primaryDark,
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
      backgroundColor: isAndroid ? c.successSurface : c.glassSuccess,
      paddingHorizontal: 8,
      paddingVertical: 2,
      borderRadius: 12,
      borderWidth: isAndroid ? 1 : 0.5,
      borderColor: c.success,
    },
    orderedBadgeText: {
      fontSize: 10,
      fontWeight: '700',
      color: c.successText,
    },
    cutoffBadge: {
      backgroundColor: isAndroid ? c.warningSurface : c.glassWarning,
      paddingHorizontal: 8,
      paddingVertical: 2,
      borderRadius: 12,
      borderWidth: isAndroid ? 1 : 0.5,
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
      fontSize: 16,
      color: c.textPrimary,
      lineHeight: 21,
    },
    textSelected: {
      color: '#fff',
    },
    allergens: {
      flex: 1,
      fontSize: 11,
      color: c.textTertiary,
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

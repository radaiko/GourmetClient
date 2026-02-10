import { Pressable, StyleSheet, Text, View } from 'react-native';
import { GourmetMenuItem } from '../types/menu';
import { isOrderingCutoff } from '../utils/dateUtils';
import { useTheme } from '../theme/useTheme';
import { Colors } from '../theme/colors';

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

const createStyles = (c: Colors) =>
  StyleSheet.create({
    card: {
      backgroundColor: c.glassSurface,
      borderRadius: 18,
      padding: 16,
      marginBottom: 10,
      borderTopWidth: 1,
      borderLeftWidth: 0.5,
      borderBottomWidth: 0.5,
      borderRightWidth: 0,
      borderTopColor: c.glassHighlight,
      borderLeftColor: c.glassHighlight,
      borderBottomColor: c.glassShadowEdge,
      position: 'relative',
      shadowColor: '#000',
      shadowOffset: { width: 0, height: 4 },
      shadowOpacity: 0.08,
      shadowRadius: 12,
      elevation: 3,
    },
    cardOrdered: {
      backgroundColor: c.glassSuccess,
      borderTopColor: c.glassHighlight,
    },
    cardSelected: {
      backgroundColor: c.primary,
      borderTopColor: 'rgba(255,255,255,0.30)',
      borderLeftColor: 'rgba(255,255,255,0.15)',
      borderBottomColor: c.primaryDark,
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
      backgroundColor: c.glassSuccess,
      paddingHorizontal: 8,
      paddingVertical: 2,
      borderRadius: 12,
      borderWidth: 0.5,
      borderColor: c.success,
    },
    orderedBadgeText: {
      fontSize: 10,
      fontWeight: '700',
      color: c.successText,
    },
    cutoffBadge: {
      backgroundColor: c.glassWarning,
      paddingHorizontal: 8,
      paddingVertical: 2,
      borderRadius: 12,
      borderWidth: 0.5,
      borderColor: c.warning,
    },
    cutoffBadgeText: {
      fontSize: 10,
      fontWeight: '700',
      color: c.warningText,
    },
    category: {
      fontSize: 14,
      fontWeight: '700',
      color: c.primary,
      letterSpacing: 0.5,
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

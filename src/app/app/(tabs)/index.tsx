import { useCallback, useEffect, useMemo } from 'react';
import {
  ActivityIndicator,
  FlatList,
  Pressable,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { useNavigation } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useAuthStore } from '../../src-rn/store/authStore';
import { useMenuStore, OrderProgress } from '../../src-rn/store/menuStore';
import { MenuCard } from '../../src-rn/components/MenuCard';
import { DayNavigator } from '../../src-rn/components/DayNavigator';
import { LoadingOverlay } from '../../src-rn/components/LoadingOverlay';
import { GourmetMenuItem, GourmetMenuCategory } from '../../src-rn/types/menu';
import { formatGourmetDate, localDateKey } from '../../src-rn/utils/dateUtils';
import { useTheme } from '../../src-rn/theme/useTheme';
import { Colors } from '../../src-rn/theme/colors';

const ORDER_PROGRESS_LABELS: Record<NonNullable<OrderProgress>, string> = {
  adding: 'Adding to cart...',
  confirming: 'Confirming order...',
  refreshing: 'Updating menus...',
};

const CATEGORY_ORDER = [
  GourmetMenuCategory.Menu1,
  GourmetMenuCategory.Menu2,
  GourmetMenuCategory.Menu3,
  GourmetMenuCategory.SoupAndSalad,
  GourmetMenuCategory.Unknown,
];

export default function MenusScreen() {
  const { colors } = useTheme();
  const insets = useSafeAreaInsets();
  const styles = useMemo(() => createStyles(colors), [colors]);

  const navigation = useNavigation();
  const { status: authStatus, loginWithSaved } = useAuthStore();
  const {
    items,
    loading,
    refreshing,
    error,
    selectedDate,
    pendingOrders,
    orderProgress,
    fetchMenus,
    refreshAvailability,
    setSelectedDate,
    togglePendingOrder,
    submitOrders,
    getAvailableDates,
    getMenusForDate,
    getPendingCount,
  } = useMenuStore();

  useEffect(() => {
    if (authStatus === 'idle') {
      loginWithSaved();
    }
  }, [authStatus, loginWithSaved]);

  const triggerRefresh = useCallback(() => {
    const auth = useAuthStore.getState().status;
    if (auth !== 'authenticated') return;
    const cached = useMenuStore.getState().items.length > 0;
    if (cached) {
      refreshAvailability();
    } else {
      fetchMenus();
    }
  }, [fetchMenus, refreshAvailability]);

  useEffect(() => {
    const unsubscribe = navigation.addListener('focus', triggerRefresh);
    return unsubscribe;
  }, [navigation, triggerRefresh]);

  useEffect(() => {
    if (authStatus === 'authenticated') {
      triggerRefresh();
    }
  }, [authStatus, triggerRefresh]);

  const dates = getAvailableDates();
  const menuItems = getMenusForDate(selectedDate);
  const pendingCount = getPendingCount();

  const grouped = CATEGORY_ORDER.map((cat) => ({
    category: cat,
    items: menuItems.filter((item) => item.category === cat),
  })).filter((group) => group.items.length > 0);

  const isPending = useCallback(
    (item: GourmetMenuItem) => {
      const key = `${item.id}|${localDateKey(item.day)}`;
      return pendingOrders.has(key);
    },
    [pendingOrders]
  );

  if (authStatus === 'idle' || authStatus === 'loading') {
    return (
      <View style={styles.center}>
        <LoadingOverlay />
      </View>
    );
  }

  if (authStatus === 'error' || authStatus === 'no_credentials') {
    return (
      <View style={styles.center}>
        <Text style={styles.errorText}>Not logged in</Text>
        <Text style={styles.hintText}>Go to Settings to enter credentials</Text>
      </View>
    );
  }

  return (
    <View style={[styles.container, { paddingTop: insets.top }]}>
      {dates.length > 0 && (
        <DayNavigator
          dates={dates}
          selectedDate={selectedDate}
          onSelectDate={setSelectedDate}
        />
      )}

      {refreshing && !orderProgress && (
        <View style={styles.refreshBanner}>
          <ActivityIndicator size="small" color={colors.primary} />
          <Text style={styles.refreshBannerText}>Updating...</Text>
        </View>
      )}

      {orderProgress && (
        <View style={styles.orderProgressBanner}>
          <ActivityIndicator size="small" color="#fff" />
          <Text style={styles.orderProgressText}>
            {ORDER_PROGRESS_LABELS[orderProgress]}
          </Text>
        </View>
      )}

      {loading && !orderProgress && <LoadingOverlay />}

      {error && (
        <View style={styles.errorBanner}>
          <Text style={styles.errorBannerText}>{error}</Text>
        </View>
      )}

      <FlatList
        data={grouped}
        keyExtractor={(group) => group.category}
        contentContainerStyle={styles.list}
        renderItem={({ item: group }) => (
          <View style={styles.categoryGroup}>
            {group.category !== GourmetMenuCategory.SoupAndSalad && (
              <Text style={styles.categoryTitle}>{group.category}</Text>
            )}
            {group.items.map((item) => (
              <MenuCard
                key={`${item.id}-${formatGourmetDate(item.day)}`}
                item={item}
                isSelected={isPending(item)}
                onToggle={() => togglePendingOrder(item.id, item.day)}
              />
            ))}
          </View>
        )}
        ListEmptyComponent={
          !loading ? (
            <View style={styles.center}>
              <Text style={styles.emptyText}>No menus available</Text>
            </View>
          ) : null
        }
      />

      {pendingCount > 0 && !orderProgress && (
        <Pressable style={styles.fab} onPress={submitOrders}>
          <Text style={styles.fabText}>
            Order ({pendingCount})
          </Text>
        </Pressable>
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
      backgroundColor: c.background,
    },
    list: {
      padding: 16,
      paddingBottom: 100,
    },
    categoryGroup: {
      marginBottom: 16,
    },
    categoryTitle: {
      fontSize: 22,
      fontWeight: '600',
      color: c.primary,
      letterSpacing: 0.5,
      marginBottom: 8,
      paddingLeft: 4,
    },
    refreshBanner: {
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'center',
      gap: 8,
      paddingVertical: 8,
      marginHorizontal: 16,
      marginTop: 8,
      backgroundColor: c.glassPrimary,
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
      shadowOpacity: 0.06,
      shadowRadius: 6,
      elevation: 2,
    },
    refreshBannerText: {
      fontSize: 12,
      color: c.primary,
      fontWeight: '500',
    },
    orderProgressBanner: {
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'center',
      gap: 8,
      paddingVertical: 10,
      marginHorizontal: 16,
      marginTop: 8,
      backgroundColor: c.primary,
      borderRadius: 14,
      borderTopWidth: 1,
      borderTopColor: 'rgba(255,255,255,0.30)',
      borderLeftWidth: 0.5,
      borderLeftColor: 'rgba(255,255,255,0.15)',
      borderBottomWidth: 0.5,
      borderBottomColor: c.primaryDark,
      borderRightWidth: 0,
      shadowColor: c.primary,
      shadowOffset: { width: 0, height: 4 },
      shadowOpacity: 0.30,
      shadowRadius: 8,
      elevation: 3,
    },
    orderProgressText: {
      fontSize: 13,
      color: '#fff',
      fontWeight: '600',
    },
    errorText: {
      fontSize: 18,
      fontWeight: '600',
      color: c.error,
    },
    hintText: {
      fontSize: 14,
      color: c.textTertiary,
      marginTop: 8,
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
    errorBannerText: {
      color: c.errorText,
      fontSize: 13,
    },
    fab: {
      position: 'absolute',
      bottom: 80,
      right: 24,
      backgroundColor: c.primary,
      paddingHorizontal: 24,
      paddingVertical: 14,
      borderRadius: 30,
      borderTopWidth: 1,
      borderTopColor: 'rgba(255,255,255,0.40)',
      borderLeftWidth: 0.5,
      borderLeftColor: 'rgba(255,255,255,0.20)',
      borderBottomWidth: 0,
      borderRightWidth: 0,
      elevation: 8,
      shadowColor: c.primary,
      shadowOffset: { width: 0, height: 6 },
      shadowOpacity: 0.35,
      shadowRadius: 16,
    },
    fabText: {
      color: '#fff',
      fontSize: 16,
      fontWeight: '700',
    },
  });

import { useCallback, useMemo, useState } from 'react';
import {
  Alert,
  FlatList,
  Platform,
  Pressable,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { useFocusEffect } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useAuthStore } from '../../src-rn/store/authStore';
import { useOrderStore } from '../../src-rn/store/orderStore';
import { OrderItem } from '../../src-rn/components/OrderItem';
import { LoadingOverlay } from '../../src-rn/components/LoadingOverlay';
import { DesktopContentWrapper } from '../../src-rn/components/DesktopContentWrapper';
import { useTheme } from '../../src-rn/theme/useTheme';
import { Colors } from '../../src-rn/theme/colors';
import { tintedBanner, buttonPrimary } from '../../src-rn/theme/platformStyles';

type Tab = 'upcoming' | 'past';

export default function OrdersScreen() {
  const { colors } = useTheme();
  const insets = useSafeAreaInsets();
  const styles = useMemo(() => createStyles(colors), [colors]);

  const { status: authStatus } = useAuthStore();
  const {
    loading,
    cancellingId,
    error,
    fetchOrders,
    confirmOrders,
    cancelOrder,
    getUpcomingOrders,
    getPastOrders,
    getUnconfirmedCount,
  } = useOrderStore();

  const [activeTab, setActiveTab] = useState<Tab>('upcoming');

  useFocusEffect(
    useCallback(() => {
      if (authStatus === 'authenticated') {
        fetchOrders();
      }
    }, [authStatus, fetchOrders])
  );

  const upcoming = getUpcomingOrders();
  const past = getPastOrders();
  const orders = activeTab === 'upcoming' ? upcoming : past;
  const unconfirmedCount = getUnconfirmedCount();

  const handleCancel = (positionId: string, title: string) => {
    Alert.alert(
      'Cancel Order',
      `Cancel "${title}"?`,
      [
        { text: 'Keep', style: 'cancel' },
        {
          text: 'Cancel Order',
          style: 'destructive',
          onPress: () => cancelOrder(positionId),
        },
      ]
    );
  };

  if (authStatus !== 'authenticated') {
    return (
      <View style={styles.center}>
        <Text style={styles.hintText}>Login required</Text>
      </View>
    );
  }

  return (
    <DesktopContentWrapper maxWidth={700}>
      <View style={[styles.container, { paddingTop: insets.top }]}>
        <View style={styles.tabs}>
          <Pressable
            style={[styles.tab, activeTab === 'upcoming' && styles.tabActive]}
            onPress={() => setActiveTab('upcoming')}
          >
            <Text style={[styles.tabText, activeTab === 'upcoming' && styles.tabTextActive]}>
              Upcoming ({upcoming.length})
            </Text>
          </Pressable>
          <Pressable
            style={[styles.tab, activeTab === 'past' && styles.tabActive]}
            onPress={() => setActiveTab('past')}
          >
            <Text style={[styles.tabText, activeTab === 'past' && styles.tabTextActive]}>
              Past ({past.length})
            </Text>
          </Pressable>
        </View>

        {unconfirmedCount > 0 && activeTab === 'upcoming' && (
          <View style={styles.confirmBanner}>
            <Text style={styles.confirmBannerText}>
              {unconfirmedCount} unconfirmed order{unconfirmedCount > 1 ? 's' : ''}
            </Text>
            <Pressable
              style={styles.confirmButton}
              onPress={confirmOrders}
              disabled={loading}
            >
              <Text style={styles.confirmButtonText}>Confirm</Text>
            </Pressable>
          </View>
        )}

        {loading && <LoadingOverlay />}

        {error && (
          <View style={styles.errorBanner}>
            <Text style={styles.errorText}>{error}</Text>
          </View>
        )}

        <FlatList
          data={orders}
          keyExtractor={(item) => item.positionId}
          contentContainerStyle={styles.list}
          renderItem={({ item }) => (
            <OrderItem
              order={item}
              isCancelling={cancellingId === item.positionId}
              onCancel={() => handleCancel(item.positionId, item.title)}
              canCancel={activeTab === 'upcoming' && cancellingId === null}
            />
          )}
          ListEmptyComponent={
            !loading ? (
              <View style={styles.center}>
                <Text style={styles.emptyText}>
                  {activeTab === 'upcoming' ? 'No upcoming orders' : 'No past orders'}
                </Text>
              </View>
            ) : null
          }
        />
      </View>
    </DesktopContentWrapper>
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
    tabs: {
      flexDirection: 'row',
      backgroundColor: Platform.OS === 'android' ? c.surface : c.glassSurface,
      borderBottomWidth: Platform.OS === 'android' ? 1 : 0.5,
      borderBottomColor: Platform.OS === 'android' ? c.border : c.glassHighlight,
    },
    tab: {
      flex: 1,
      paddingVertical: 14,
      alignItems: 'center',
      borderBottomWidth: 3,
      borderBottomColor: 'transparent',
    },
    tabActive: {
      borderBottomColor: c.primary,
    },
    tabText: {
      fontSize: 14,
      fontWeight: '600',
      color: c.textTertiary,
    },
    tabTextActive: {
      color: c.primary,
    },
    confirmBanner: {
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'space-between',
      padding: 12,
      marginHorizontal: 16,
      marginTop: 8,
      ...tintedBanner(c, c.glassWarning),
    },
    confirmBannerText: {
      color: c.warningText,
      fontSize: 14,
      fontWeight: '600',
      flex: 1,
    },
    confirmButton: {
      paddingHorizontal: 20,
      paddingVertical: 8,
      ...buttonPrimary(c),
    },
    confirmButtonText: {
      color: '#fff',
      fontWeight: '700',
      fontSize: 14,
    },
    list: {
      padding: 16,
      paddingBottom: 100,
    },
    hintText: {
      fontSize: 16,
      color: c.textTertiary,
    },
    emptyText: {
      fontSize: 16,
      color: c.textTertiary,
    },
    errorBanner: {
      padding: 12,
      marginHorizontal: 16,
      marginTop: 8,
      ...tintedBanner(c, c.glassError),
    },
    errorText: {
      color: c.errorText,
      fontSize: 13,
    },
  });

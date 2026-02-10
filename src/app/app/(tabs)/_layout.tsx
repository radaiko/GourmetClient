import { Tabs } from 'expo-router';
import { Ionicons } from '@expo/vector-icons';
import { BlurView } from 'expo-blur';
import { Pressable, StyleSheet, View } from 'react-native';
import type { BottomTabBarProps } from '@react-navigation/bottom-tabs';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useTheme } from '../../src-rn/theme/useTheme';

const ICONS: Record<string, { outline: keyof typeof Ionicons.glyphMap; filled: keyof typeof Ionicons.glyphMap }> = {
  index: { outline: 'restaurant-outline', filled: 'restaurant' },
  orders: { outline: 'receipt-outline', filled: 'receipt' },
  billing: { outline: 'wallet-outline', filled: 'wallet' },
  settings: { outline: 'settings-outline', filled: 'settings' },
};

function GlassTabBar({ state, descriptors, navigation }: BottomTabBarProps) {
  const { colors } = useTheme();
  const insets = useSafeAreaInsets();

  return (
    <View style={[styles.tabBarWrapper, { bottom: Math.max(insets.bottom, 8) }]}>
      <View style={styles.pill}>
        <BlurView
          intensity={colors.blurIntensity}
          tint={colors.blurTint as any}
          style={StyleSheet.absoluteFill}
        />
        <View
          style={[
            StyleSheet.absoluteFill,
            { backgroundColor: colors.glassSurface },
          ]}
        />
        <View
          style={[
            StyleSheet.absoluteFill,
            {
              borderWidth: 0.5,
              borderRadius: 28,
              borderTopColor: colors.glassHighlight,
              borderLeftColor: colors.glassHighlight,
              borderBottomColor: colors.glassShadowEdge,
              borderRightColor: colors.glassShadowEdge,
            },
          ]}
        />
        <View style={styles.iconRow}>
          {state.routes.map((route, index) => {
            const isFocused = state.index === index;
            const icon = ICONS[route.name];
            if (!icon) return null;

            const onPress = () => {
              const event = navigation.emit({
                type: 'tabPress',
                target: route.key,
                canPreventDefault: true,
              });
              if (!isFocused && !event.defaultPrevented) {
                navigation.navigate(route.name);
              }
            };

            return (
              <Pressable
                key={route.key}
                onPress={onPress}
                style={styles.tabButton}
                hitSlop={8}
              >
                <Ionicons
                  name={isFocused ? icon.filled : icon.outline}
                  size={26}
                  color={isFocused ? colors.primary : colors.textTertiary}
                />
              </Pressable>
            );
          })}
        </View>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  tabBarWrapper: {
    position: 'absolute',
    left: 0,
    right: 0,
    alignItems: 'center',
  },
  pill: {
    width: 248,
    height: 56,
    borderRadius: 28,
    overflow: 'hidden',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 8 },
    shadowOpacity: 0.15,
    shadowRadius: 32,
    elevation: 6,
  },
  iconRow: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 20,
  },
  tabButton: {
    width: 44,
    height: 44,
    alignItems: 'center',
    justifyContent: 'center',
  },
});

export default function TabLayout() {
  return (
    <Tabs
      tabBar={(props) => <GlassTabBar {...props} />}
      screenOptions={{ headerShown: false }}
    >
      <Tabs.Screen name="index" options={{ title: 'Menus' }} />
      <Tabs.Screen name="orders" options={{ title: 'Orders' }} />
      <Tabs.Screen name="billing" options={{ title: 'Billing' }} />
      <Tabs.Screen name="settings" options={{ title: 'Settings' }} />
    </Tabs>
  );
}

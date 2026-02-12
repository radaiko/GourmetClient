import { useState, useEffect } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import type { BottomTabBarProps } from '@react-navigation/bottom-tabs';
import { AdaptiveBlurView } from './AdaptiveBlurView';
import { useTheme } from '../theme/useTheme';
import { useDesktopLayout } from '../hooks/useDesktopLayout';
import { Colors } from '../theme/colors';
import { sidebarSurface } from '../theme/platformStyles';

const ICONS: Record<string, { outline: keyof typeof Ionicons.glyphMap; filled: keyof typeof Ionicons.glyphMap; label: string }> = {
  index: { outline: 'restaurant-outline', filled: 'restaurant', label: 'Menus' },
  orders: { outline: 'receipt-outline', filled: 'receipt', label: 'Orders' },
  billing: { outline: 'wallet-outline', filled: 'wallet', label: 'Billing' },
  settings: { outline: 'settings-outline', filled: 'settings', label: 'Settings' },
};

export function DesktopSidebar({ state, navigation }: BottomTabBarProps) {
  const { colors } = useTheme();
  const { sidebarWidth } = useDesktopLayout();
  const styles = createStyles(colors, sidebarWidth);
  const [version, setVersion] = useState(require('../../package.json').version);

  useEffect(() => {
    const internals = (window as any).__TAURI_INTERNALS__;
    if (internals?.invoke) {
      internals.invoke('plugin:app|version').then((v: string) => setVersion(v)).catch(() => {});
    }
  }, []);

  return (
    <View style={styles.wrapper}>
      <AdaptiveBlurView
        intensity={colors.blurIntensityStrong}
        tint={colors.blurTint as any}
        style={StyleSheet.absoluteFill}
      />
      <View style={[StyleSheet.absoluteFill, styles.surface]} />

      <Text style={styles.appName}>Gourmet Client</Text>

      <View style={styles.nav}>
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
              style={[styles.navItem, isFocused && styles.navItemActive]}
            >
              {isFocused && <View style={styles.activeAccent} />}
              <Ionicons
                name={isFocused ? icon.filled : icon.outline}
                size={20}
                color={isFocused ? colors.primary : colors.textTertiary}
              />
              <Text style={[styles.navLabel, isFocused && styles.navLabelActive]}>
                {icon.label}
              </Text>
            </Pressable>
          );
        })}
      </View>

      <Text style={styles.version}>v{version}</Text>
    </View>
  );
}

const createStyles = (c: Colors, sidebarWidth: number) =>
  StyleSheet.create({
    wrapper: {
      width: sidebarWidth,
      overflow: 'hidden',
      ...sidebarSurface(c),
    },
    surface: {
      backgroundColor: c.glassSurface,
    },
    appName: {
      fontSize: 15,
      fontWeight: '700',
      color: c.textPrimary,
      paddingHorizontal: 20,
      paddingTop: 24,
      paddingBottom: 20,
      letterSpacing: 0.3,
    },
    nav: {
      flex: 1,
      paddingHorizontal: 10,
      gap: 2,
    },
    navItem: {
      flexDirection: 'row',
      alignItems: 'center',
      gap: 12,
      paddingVertical: 10,
      paddingHorizontal: 12,
      borderRadius: 10,
      position: 'relative',
    },
    navItemActive: {
      backgroundColor: c.glassPrimary,
    },
    activeAccent: {
      position: 'absolute',
      left: 0,
      top: 6,
      bottom: 6,
      width: 3,
      borderRadius: 2,
      backgroundColor: c.primary,
    },
    navLabel: {
      fontSize: 14,
      fontWeight: '500',
      color: c.textTertiary,
    },
    navLabelActive: {
      color: c.primary,
      fontWeight: '600',
    },
    version: {
      fontSize: 11,
      color: c.textTertiary,
      paddingHorizontal: 20,
      paddingBottom: 16,
    },
  });

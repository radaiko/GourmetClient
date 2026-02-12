import { useState, useEffect } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import type { BottomTabBarProps } from '@react-navigation/bottom-tabs';
import { useTheme } from '../theme/useTheme';
import { useDesktopLayout } from '../hooks/useDesktopLayout';
import { Colors } from '../theme/colors';
import { sidebarSurface } from '../theme/platformStyles';
import { useUpdateStore, applyUpdate, checkForDesktopUpdates } from '../utils/desktopUpdater';

const ICONS: Record<string, { outline: keyof typeof Ionicons.glyphMap; filled: keyof typeof Ionicons.glyphMap; label: string }> = {
  index: { outline: 'restaurant-outline', filled: 'restaurant', label: 'Menus' },
  orders: { outline: 'receipt-outline', filled: 'receipt', label: 'Orders' },
  billing: { outline: 'wallet-outline', filled: 'wallet', label: 'Billing' },
  settings: { outline: 'settings-outline', filled: 'settings', label: 'Settings' },
};

export function DesktopSidebar({ state, navigation }: BottomTabBarProps) {
  const { colors } = useTheme();
  const { sidebarWidth, sidebarCollapsed, toggleSidebar } = useDesktopLayout();
  const styles = createStyles(colors, sidebarWidth, sidebarCollapsed);
  const [version, setVersion] = useState(require('../../package.json').version);
  const pendingVersion = useUpdateStore((s) => s.pendingVersion);
  const checkingUpdates = useUpdateStore((s) => s.checking);

  useEffect(() => {
    const internals = (window as any).__TAURI_INTERNALS__;
    if (internals?.invoke) {
      internals.invoke('plugin:app|version').then((v: string) => setVersion(v)).catch(() => {});
    }
  }, []);

  return (
    <View style={styles.wrapper}>
      <View style={styles.header}>
        {!sidebarCollapsed && <Text style={styles.appName}>Gourmet Client</Text>}
        <Pressable onPress={toggleSidebar} style={styles.collapseButton}>
          <Ionicons
            name={sidebarCollapsed ? 'chevron-forward' : 'chevron-back'}
            size={14}
            color={colors.textTertiary}
          />
        </Pressable>
      </View>

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
                size={16}
                color={isFocused ? colors.primary : colors.textTertiary}
              />
              {!sidebarCollapsed && (
                <Text style={[styles.navLabel, isFocused && styles.navLabelActive]}>
                  {icon.label}
                </Text>
              )}
            </Pressable>
          );
        })}
      </View>

      {pendingVersion && (
        <Pressable
          onPress={applyUpdate}
          style={sidebarCollapsed ? styles.updateHintCollapsed : styles.updateHint}
        >
          <Ionicons name="download-outline" size={14} color={colors.primary} />
          {!sidebarCollapsed && (
            <Text style={styles.updateText} numberOfLines={1}>
              v{pendingVersion}
            </Text>
          )}
        </Pressable>
      )}
      {!sidebarCollapsed && (
        <Pressable onPress={() => checkForDesktopUpdates(true)} disabled={checkingUpdates}>
          <Text style={styles.version}>
            {checkingUpdates ? 'Checking...' : `v${version}`}
          </Text>
        </Pressable>
      )}
    </View>
  );
}

const createStyles = (c: Colors, sidebarWidth: number, collapsed: boolean) =>
  StyleSheet.create({
    wrapper: {
      width: sidebarWidth,
      overflow: 'hidden',
      ...sidebarSurface(c),
    },
    header: {
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: collapsed ? 'center' : 'space-between',
      paddingHorizontal: collapsed ? 0 : 14,
      paddingTop: 10,
      paddingBottom: 8,
    },
    appName: {
      fontSize: 12,
      fontWeight: '700',
      color: c.textPrimary,
      letterSpacing: 0.3,
      textTransform: 'uppercase',
      flex: 1,
    },
    collapseButton: {
      width: 24,
      height: 24,
      alignItems: 'center',
      justifyContent: 'center',
      borderRadius: 4,
    },
    nav: {
      flex: 1,
      paddingHorizontal: collapsed ? 4 : 6,
      gap: 1,
    },
    navItem: {
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: collapsed ? 'center' : 'flex-start',
      gap: collapsed ? 0 : 8,
      paddingVertical: 6,
      paddingHorizontal: collapsed ? 0 : 10,
      borderRadius: 4,
      position: 'relative',
    },
    navItemActive: {
      backgroundColor: c.glassPrimary,
    },
    activeAccent: {
      position: 'absolute',
      left: 0,
      top: 4,
      bottom: 4,
      width: 2,
      borderRadius: 1,
      backgroundColor: c.primary,
    },
    navLabel: {
      fontSize: 13,
      fontWeight: '400',
      color: c.textTertiary,
    },
    navLabelActive: {
      color: c.primary,
      fontWeight: '600',
    },
    updateHint: {
      flexDirection: 'row',
      alignItems: 'center',
      gap: 6,
      marginHorizontal: 6,
      marginBottom: 4,
      paddingVertical: 5,
      paddingHorizontal: 10,
      borderRadius: 4,
      backgroundColor: c.primarySurface,
      borderWidth: 1,
      borderColor: c.primary,
    },
    updateHintCollapsed: {
      alignItems: 'center',
      justifyContent: 'center',
      marginHorizontal: 4,
      marginBottom: 4,
      paddingVertical: 5,
      borderRadius: 4,
      backgroundColor: c.primarySurface,
      borderWidth: 1,
      borderColor: c.primary,
    },
    updateText: {
      fontSize: 11,
      fontWeight: '600',
      color: c.primary,
      flex: 1,
    },
    version: {
      fontSize: 10,
      color: c.textTertiary,
      paddingHorizontal: 14,
      paddingBottom: 10,
    },
  });

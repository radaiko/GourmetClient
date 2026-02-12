import { useMemo } from 'react';
import { useWindowDimensions } from 'react-native';
import { create } from 'zustand';
import { isDesktop } from '../utils/platform';

const SIDEBAR_WIDTH_EXPANDED = 180;
const SIDEBAR_WIDTH_COLLAPSED = 48;
const PANEL_WIDTH = 200;
const WIDE_BREAKPOINT = 700;

interface SidebarState {
  collapsed: boolean;
  toggle: () => void;
}

const useSidebarStore = create<SidebarState>((set) => ({
  collapsed: false,
  toggle: () => set((s) => ({ collapsed: !s.collapsed })),
}));

export function useDesktopLayout() {
  const { width, height } = useWindowDimensions();
  const isDesktopPlatform = useMemo(() => isDesktop(), []);
  const isWideLayout = isDesktopPlatform && width >= WIDE_BREAKPOINT;
  const collapsed = useSidebarStore((s) => s.collapsed);
  const toggleSidebar = useSidebarStore((s) => s.toggle);

  return {
    isWideLayout,
    sidebarWidth: collapsed ? SIDEBAR_WIDTH_COLLAPSED : SIDEBAR_WIDTH_EXPANDED,
    sidebarCollapsed: collapsed,
    toggleSidebar,
    panelWidth: PANEL_WIDTH,
    windowWidth: width,
    windowHeight: height,
  };
}

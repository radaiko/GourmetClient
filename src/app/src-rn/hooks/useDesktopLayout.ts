import { useMemo } from 'react';
import { useWindowDimensions } from 'react-native';
import { isDesktop } from '../utils/platform';

const SIDEBAR_WIDTH = 220;
const PANEL_WIDTH = 260;
const WIDE_BREAKPOINT = 700;

export function useDesktopLayout() {
  const { width, height } = useWindowDimensions();
  const isDesktopPlatform = useMemo(() => isDesktop(), []);
  const isWideLayout = isDesktopPlatform && width >= WIDE_BREAKPOINT;

  return {
    isWideLayout,
    sidebarWidth: SIDEBAR_WIDTH,
    panelWidth: PANEL_WIDTH,
    windowWidth: width,
    windowHeight: height,
  };
}

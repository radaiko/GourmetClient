import { View } from 'react-native';
import { useDesktopLayout } from '../hooks/useDesktopLayout';
import { useTheme } from '../theme/useTheme';

interface DesktopContentWrapperProps {
  maxWidth?: number;
  children: React.ReactNode;
}

export function DesktopContentWrapper({ maxWidth = 800, children }: DesktopContentWrapperProps) {
  const { isWideLayout } = useDesktopLayout();
  const { colors } = useTheme();

  if (!isWideLayout) {
    return <>{children}</>;
  }

  return (
    <View style={{ flex: 1, alignItems: 'center', backgroundColor: colors.background }}>
      <View style={{ flex: 1, width: '100%', maxWidth }}>
        {children}
      </View>
    </View>
  );
}

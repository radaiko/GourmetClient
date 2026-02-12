import { View } from 'react-native';
import { useDesktopLayout } from '../hooks/useDesktopLayout';

interface DesktopContentWrapperProps {
  maxWidth?: number;
  children: React.ReactNode;
}

export function DesktopContentWrapper({ maxWidth = 800, children }: DesktopContentWrapperProps) {
  const { isWideLayout } = useDesktopLayout();

  if (!isWideLayout) {
    return <>{children}</>;
  }

  return (
    <View style={{ flex: 1, alignItems: 'center' }}>
      <View style={{ flex: 1, width: '100%', maxWidth }}>
        {children}
      </View>
    </View>
  );
}

import React from 'react';
import { View, StyleProp, ViewStyle } from 'react-native';

interface AdaptiveBlurViewProps {
  intensity?: number;
  tint?: string;
  style?: StyleProp<ViewStyle>;
  children?: React.ReactNode;
}

export function AdaptiveBlurView({ intensity = 50, style, children }: AdaptiveBlurViewProps) {
  const blurPx = Math.round(intensity * 0.4);
  return (
    <View
      style={[
        style,
        {
          backdropFilter: `blur(${blurPx}px)`,
          WebkitBackdropFilter: `blur(${blurPx}px)`,
        } as any,
      ]}
    >
      {children}
    </View>
  );
}

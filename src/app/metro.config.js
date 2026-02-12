const { getDefaultConfig } = require('expo/metro-config');

const config = getDefaultConfig(__dirname);

// Fix: Add 'react-native' condition for web platform so packages like Zustand
// resolve to their CJS builds (which use process.env.NODE_ENV) instead of ESM
// builds (which use import.meta.env that breaks in non-module script tags).
config.resolver.unstable_conditionsByPlatform.web = [
  'browser',
  'react-native',
];

module.exports = config;

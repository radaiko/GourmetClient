# GourmetClient

Cross-platform Expo React Native app for company cafeteria menu ordering and billing. Scrapes two external websites (Gourmet and Ventopay) to view menus, place orders, and track expenses.

## Platforms

- Android
- iOS

## Quick Start

```bash
cd src/app

# Install dependencies
npm install

# iOS Simulator (macOS only, requires Xcode)
npx expo run:ios

# Android Emulator (requires Android SDK + an AVD)
npx expo run:android
```

Copy `src/app/.env.example` to `src/app/.env.local` and fill in your credentials for dev/simulator login.

## Tech Stack

- Expo SDK 54 / React Native 0.81.5 / React 19
- Expo Router (file-based navigation)
- Zustand (state management)
- Cheerio (HTML parsing for web scraping)
- Axios + tough-cookie (HTTP + cookie management)
- expo-secure-store (credential storage)
- TypeScript

## Credits

Based on [GourmetClient](https://github.com/patrickl92/GourmetClient) by patrickl92.

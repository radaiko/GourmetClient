# SnackPilot

Cross-platform app for company cafeteria menu ordering and billing. Connects to the Kantine and Automaten systems to view menus, place orders, and track expenses.

Built with Expo React Native (mobile) and Tauri v2 (desktop).

## Platforms

- iOS
- Android
- macOS (desktop via Tauri)
- Windows (desktop via Tauri)
- Linux (desktop via Tauri)

## Quick Start

### Mobile

```bash
cd src/app
npm install

# iOS Simulator (macOS only, requires Xcode)
npx expo run:ios

# Android Emulator (requires Android SDK + an AVD)
npx expo run:android
```

### Desktop

```bash
cd src/desktop
npm install

# Dev mode (opens desktop window with Expo web dev server)
npm run dev

# Production build (creates .app/.dmg on macOS, .msi on Windows, .deb on Linux)
npm run build
```

**Prerequisites**: Rust toolchain, Node.js

### Environment

Copy `.env.example` to `.env` at the project root and fill in your credentials.

## Tech Stack

- **Mobile**: Expo SDK 54, React Native 0.81.5, React 19
- **Desktop**: Tauri v2 (Rust + webview)
- **Navigation**: Expo Router (file-based tabs)
- **State**: Zustand
- **Scraping**: Cheerio (HTML parsing), Axios (HTTP)
- **Credentials**: expo-secure-store (native), localStorage (desktop)
- **Auto-updates**: Velopack (desktop, via GitHub Releases)
- **Languages**: TypeScript, Rust

## Project Structure

```
src/app/                    # Expo React Native app (mobile + web)
├── app/                    # Expo Router screens (tabs: Menus, Orders, Billing, Settings)
├── src-rn/
│   ├── api/                # Web scraping layer (Gourmet + Ventopay)
│   ├── store/              # Zustand stores (auth, menu, order, billing)
│   ├── components/         # UI components
│   ├── hooks/              # Custom React hooks
│   ├── theme/              # Theming
│   ├── types/              # TypeScript types
│   └── utils/              # Helpers (platform detection, storage, date formatting)
└── __tests__/              # Test suites (178 tests across 13 files)

src/desktop/                # Tauri v2 desktop wrapper
├── src-tauri/              # Rust backend (Velopack auto-updates)
└── package.json            # Desktop build scripts
```

## Testing

```bash
cd src/app
npm test                    # Run all tests
npm run test:watch          # Watch mode
npm run test:coverage       # With coverage report
npm run record-fixtures     # Re-record HTML fixtures from live sites (requires .env)
```

Tests use a record & replay strategy with sanitized HTML fixtures.

## Credits

Based on [GourmetClient](https://github.com/patrickl92/GourmetClient) by patrickl92.

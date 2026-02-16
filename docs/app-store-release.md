# App Store Release Guide

## Prerequisites

- Apple Developer Program membership ($99/year)
- EAS CLI: `npm install -g eas-cli`
- Logged in: `eas login` (account: radaiko)

## Releasing an Update

### 1. Build

```bash
cd src/app
eas build --platform ios --profile production
```

The `buildNumber` auto-increments on each build (configured in `eas.json`).

If you bumped the user-facing version, update `version` in `app.json` first:
```json
"version": "1.2.0"
```

### 2. Submit to TestFlight

```bash
eas submit --platform ios --latest
```

Or build + submit in one step:
```bash
eas build --platform ios --auto-submit
```

### 3. Release from TestFlight

1. Go to [App Store Connect](https://appstoreconnect.apple.com)
2. Select **SnackPilot**
3. Wait for the build to finish processing (~15-30 min)
4. Test via TestFlight on your device
5. When ready: create a new version, select the build, click **Submit for Review**

Apple review typically takes 1-3 days.

## Quick Reference

| Action | Command |
|--------|---------|
| Build iOS | `eas build --platform ios --profile production` |
| Submit latest build | `eas submit --platform ios --latest` |
| Build + auto-submit | `eas build --platform ios --auto-submit` |
| Check build status | `eas build:list` |
| Manage credentials | `eas credentials` |

## App Details

| Field | Value |
|-------|-------|
| Bundle ID | `dev.radaiko.gourmetclient` |
| ASC App ID | `6753957109` |
| Expo account | `radaiko` |
| EAS Project ID | `efb12eb3-0729-4ea2-a3db-8026d95db7d3` |

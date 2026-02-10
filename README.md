# GourmetClient

Cross-platform .NET MAUI app for company cafeteria menu ordering and billing. Scrapes two external websites (Gourmet and Ventopay) to view menus, place orders, and track expenses.

## Platforms

- Android
- iOS
- macOS (Mac Catalyst)
- Windows

## Quick Start (Simulators)

```bash
# iOS Simulator (macOS only, requires Xcode)
./scripts/run-ios-simulator.sh

# Android Emulator (requires Android SDK + an AVD)
./scripts/run-android-emulator.sh
```

The scripts auto-detect a running simulator/emulator. If none is running, they boot the first available one.

## Build & Run

```bash
# Solution
dotnet build src/GourmetClient.sln

# Mac Catalyst
dotnet build src/GourmetClient.Maui/GourmetClient.Maui.csproj -f net10.0-maccatalyst
dotnet run --project src/GourmetClient.Maui/GourmetClient.Maui.csproj -f net10.0-maccatalyst

# Android
dotnet build src/GourmetClient.Maui/GourmetClient.Maui.csproj -f net10.0-android
```

## Dependencies

- HtmlAgilityPack - HTML parsing for web scraping
- CommunityToolkit.Mvvm - MVVM support
- Semver - Version comparison
- Velopack - Desktop auto-updates (Windows and Mac only)

## Credits

Based on [GourmetClient](https://github.com/patrickl92/GourmetClient) by patrickl92.

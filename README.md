# GourmetClient
Vereinfachte Essenbestellung für Gourmet.

## Inhalt
- Download (Endanwender)
- Schnellstart (Entwicklung)
- Architektur
- MVU Überblick
- Plattformen
- Nächste Schritte
- Beiträge

## Todos
- [ ] pull-down support
- [ ] display save button for MenuView
- [ ] prevent ordering after 9am Vienna time
- [ ] implement intelligent caching for menu
- [ ] implement BillingView
- [ ] update desktop application

## Download (Endanwender)
1. Aktuelle Version herunterladen: https://github.com/radaiko/GourmetClient/releases/latest (Datei `GourmetClient.zip`).
2. ZIP in einen beliebigen Ordner entpacken (nicht unter `C:\Programme` oder `C:\Programme (x86)` – sonst können Updates scheitern).
3. `GourmetClient.exe` starten.

## Schnellstart (Entwicklung)
Voraussetzungen: .NET 9 SDK. Für iOS zusätzlich Xcode + iOS Workload:
```bash
dotnet workload install ios   # nur falls noch nicht installiert
```

Desktop starten (Debug):
```bash
dotnet run -c Debug --project src/GourmetClient.Desktop/GourmetClient.Desktop.csproj
```

Nur bauen (alle Projekte):
```bash
dotnet build GourmetClient.sln -c Debug
```

iOS (Simulator) bauen:
```bash
dotnet build src/GourmetClient.iOS/GourmetClient.iOS.csproj -c Debug -f net9.0-ios26.0
```

iOS ausführen (Simulator / ausgewähltes Device):
```bash
dotnet build src/GourmetClient.iOS/GourmetClient.iOS.csproj -t:Run -f net9.0-ios26.0
```

## Architektur
Die Lösung ist in eine MVU Kern-Bibliothek und Plattform-Startprojekte getrennt:

| Projekt | Rolle |
|---------|-------|
| `GourmetClient.Core` | Geschäftslogik, Modelle, Netzwerk, Caching |
| `GourmetClient.MVU` | Gemeinsame UI-Logik (Avalonia + FuncUI), multi-target (`net9.0`, `net9.0-ios26.0`) |
| `GourmetClient.Desktop` | Desktop Head-Projekt (classic desktop lifetime) |
| `GourmetClient.iOS` | iOS Head-Projekt (SingleView-Lifetime) |
| `AvaloniaTest/*` | Referenz / Beispiel (separat, optional) |

Zentrale Paketversionen liegen in `Directory.Build.props` (Root). Keine lokalen Overrides mehr.

## MVU Überblick
- `AppState`: Gesamter UI-Zustand
- `MvuDispatcher`: State-Reducer + Change Notifications
- Views (FuncUI): reine Funktionen (State + Dispatch → UI)
- iOS-spezifische Varianten mit `*.iOS.cs` + Laufzeitabfragen in `PlatformDetector`

## Plattformen
- Desktop: `MainWindow` (HostWindow) + MVU Dispatcher
- iOS: SingleView (`App.iOS.cs` setzt `MainViewHostControl` als Root)
- Erweiterbar: Android (analog zu iOS), Browser (separat, falls gewünscht)

## Beiträge
Issues & Pull Requests willkommen.

---
Hinweis: Dieses README fokussiert sich auf Kerninformationen. Historische Details stehen im Git-Verlauf.

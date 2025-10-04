# GourmetClient
Vereinfachte Essenbestellung für Gourmet

## Download (Endanwender)
1. Die Datei "GourmetClient.zip" herunterladen: https://github.com/patrickl92/GourmetClient/releases/latest
2. Die heruntergeladene Datei in ein lokales Verzeichnis entpacken
   
   Achtung: **Nicht** nach "*C:\Programme*" oder "*C:\Programme (x86)*", da es sonst zu Problemen mit den automatischen Updates kommen kann
3. Die Datei *GourmetClient.exe* im entpackten Verzeichnis starten

## Architektur (neu)
Die Lösung wurde in eine plattformunabhängige MVU (Model-View-Update) Kern-Bibliothek und Plattform-Startprojekte aufgeteilt.

Komponenten:
- `GourmetClient.Core`: Geschäftslogik, Modelle, Netzwerk, Serialisierung, Caching.
- `GourmetClient.MVU`: Gemeinsame Avalonia + FuncUI MVU UI Logik (Views, State, Dispatcher) – jetzt nur noch eine Bibliothek (kein eigenes `Main`). Multi-Target: `net9.0` & `net9.0-ios26.0`.
- `GourmetClient.Desktop`: Desktop Head-Projekt (Windows/Linux/macOS) – startet `App` aus MVU und setzt das Hauptfenster.
- `GourmetClient.iOS`: iOS Head-Projekt – verwendet SingleView-Lifetime und hostet `MainViewHostControl`.
- `AvaloniaTest/*`: Beispiel / Referenzprojekt (unverändert, dient als Vorlage; nutzt jetzt dieselbe zentrale Avalonia Version).

## Wichtigste Änderungen
- Entfernt: Einstiegspunkt (Program/Main) aus `GourmetClient.MVU`.
- Hinzugefügt: Plattform-spezifische Partial-Klasse `App.iOS.cs` (setzt `ISingleViewApplicationLifetime.MainView`).
- Neue Kopf-Projekte: `GourmetClient.Desktop`, `GourmetClient.iOS` (falls vorher fehlend / nicht genutzt).
- Vereinheitlichte Paketversionen über `Directory.Build.props` (Root). Lokale Overrides entfernt.
- Präprozessor: iOS Code jetzt via `__IOS__` statt `IOS`.

## Build & Run (Entwicklung)
Voraussetzungen: .NET 9 SDK, für iOS: Xcode + iOS Workload (`dotnet workload install ios`).

Desktop (Debug):
```bash
dotnet run --project src/GourmetClient.Desktop/GourmetClient.Desktop.csproj -c Debug
```

Nur bauen:
```bash
dotnet build GourmetClient.sln -c Debug
```

iOS Simulator Build (nur Kompilieren):
```bash
dotnet build src/GourmetClient.iOS/GourmetClient.iOS.csproj -c Debug -f net9.0-ios26.0
```

iOS Ausführen (abhängig von Setup, ggf. Device/Simulator auswählen):
```bash
dotnet build src/GourmetClient.iOS/GourmetClient.iOS.csproj -t:Run -f net9.0-ios26.0
```

## MVU Überblick
- `AppState`: Globaler Zustand
- `MvuDispatcher`: Verwaltet State-Änderungen und ruft View-Updates auf
- Views nutzen `Avalonia.FuncUI` und generieren deklarativ UI aus State + Dispatch Funktion

## Plattform Differenzierung
Runtime Checks (`PlatformDetector`) + iOS-spezifische View-Dateien (`*.iOS.cs`) ermöglichen angepasste UI ohne Code-Duplikation.

## Nächste mögliche Schritte
- Android Head-Projekt analog hinzufügen
- Unit Tests für State-Reducer / Update Logik
- CI Build Pipeline hinzufügen

## Support / Beiträge
PRs und Issues willkommen.

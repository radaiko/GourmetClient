# AI Assistant Context for GourmetClient

## Project Overview
GourmetClient is a desktop application for managing meal pre-orders and billing through Gourmet and Ventopay services. The application is currently implemented as a WPF desktop application but is planned to be migrated to .NET MAUI to support iOS and Android platforms in addition to Windows.

### Current State
- **Existing**: WPF desktop application (Windows only)
- **Target**: .NET MAUI cross-platform application (Windows, iOS, Android)
- **Migration Goal**: Preserve existing functionality while enabling mobile app deployment

## Technical Stack

### Current (WPF)
- **Framework**: .NET 9, C# 13.0
- **UI**: WPF (Windows Presentation Foundation)
- **Architecture**: MVVM pattern
- **Key Libraries**: 
  - HtmlAgilityPack (web scraping)
  - Semver (semantic versioning)
  - System.Text.Json (JSON serialization)

### Target (MAUI)
- **Framework**: .NET 9, C# 13.0
- **UI**: .NET MAUI (Multi-platform App UI)
- **Architecture**: MVVM pattern (preserve existing)
- **Platforms**: Windows, iOS, Android
- **Key Libraries**: Same core libraries where compatible

## Code Standards & Patterns
- Use file-scoped namespaces
- Prefer record types for immutable data models
- Use nullable reference types (`#nullable enable`)
- Follow async/await patterns for I/O operations
- Use dependency injection via `InstanceProvider` (adapt for MAUI DI)
- MVVM pattern with platform-specific considerations

## Key Architectural Components (Reference Implementation)

### Services (Core - Preserve for MAUI)
- `UpdateService` - Handles application updates via GitHub releases
- `GourmetCacheService` - Caches menu data
- `GourmetSettingsService` - Manages application settings
- `NotificationService` - Application notifications (adapt for mobile)

### Network Layer (Core - Preserve for MAUI)
- `WebClientBase` - Base class for web clients
- `GourmetWebClient` - Gourmet service integration
- `VentopayWebClient` - Ventopay service integration

### Models (Core - Preserve for MAUI)
- Use records for immutable data (`GourmetMenu`, `GourmetOrderedMenu`)
- Separate serializable classes in `Serialization` namespace
- Settings classes in `Settings` namespace

### ViewModels (Preserve - MAUI Compatible)
- Existing ViewModels should be largely reusable in MAUI
- May need minor adjustments for cross-platform scenarios

### Views (Migrate - WPF to MAUI)
- Current WPF Views need to be converted to MAUI XAML
- Consider mobile-first responsive design
- Platform-specific adaptations where needed

## Migration Guidelines

### When Converting WPF to MAUI:
1. **Preserve Business Logic**: Keep all services and models intact
2. **Adapt ViewModels**: Minor changes for MAUI-specific patterns
3. **Redesign Views**: Convert WPF XAML to MAUI XAML with mobile considerations
4. **Platform Services**: Use MAUI platform abstractions for file system, notifications, etc.
5. **Dependency Injection**: Migrate from `InstanceProvider` to MAUI's built-in DI container
6. **Mobile Considerations**: Touch-friendly UI, responsive layouts, mobile navigation patterns

### File Organization (Target MAUI Structure):
- **Platforms/**: Platform-specific code (iOS, Android, Windows)
- **Models/**: Business models (preserve existing)
- **Services/**: Core services (preserve existing with adaptations)
- **ViewModels/**: ViewModels (minimal changes)
- **Views/**: MAUI Views (converted from WPF)
- **Resources/**: Shared resources, styles, images

### Key Migration Considerations:
1. **File Storage**: Replace `App.LocalAppDataPath` with `FileSystem.AppDataDirectory`
2. **HTTP Clients**: `HttpClientHelper` may need MAUI-specific adaptations
3. **Notifications**: Replace WPF notifications with MAUI Community Toolkit
4. **Updates**: Desktop update mechanism needs mobile app store considerations
5. **Security**: Credential storage needs platform-specific secure storage

## Development Guidelines

### When Adding New Features (MAUI Context):
1. Follow existing patterns from WPF reference implementation
2. Add proper error handling with custom exceptions
3. Use cancellation tokens for async operations
4. Implement proper caching when applicable
5. Add appropriate logging/notifications (MAUI-compatible)
6. Consider mobile UX patterns and constraints

### Error Handling (Preserve Pattern):
- Use custom exceptions (`GourmetUpdateException`, `GourmetRequestException`, etc.)
- Wrap I/O operations in try-catch blocks
- Provide user-friendly error messages via notifications (adapt for mobile)

## Common Patterns to Follow:
- **Settings**: Load from JSON files using MAUI file system abstractions
- **Caching**: Use semaphores for thread-safe access (preserve)
- **HTTP requests**: Adapt `HttpClientHelper` for MAUI
- **Serialization**: Separate serializable classes from domain models (preserve)
- **Updates**: Consider platform-specific update mechanisms

## Cross-Platform Considerations:
- **iOS**: App Store deployment, iOS-specific UI guidelines
- **Android**: Google Play Store deployment, Material Design considerations
- **Windows**: Preserve existing functionality, Windows App Store option
- **Responsive Design**: Adapt layouts for different screen sizes
- **Platform Services**: Use MAUI abstractions for platform-specific features

## Security Considerations:
- Encrypt stored credentials using MAUI secure storage
- Validate checksums for update packages (where applicable)
- Platform-specific security requirements (iOS Keychain, Android Keystore)

## Reference Implementation
The existing WPF application in `src/GourmetClient/` serves as the reference implementation for business logic, data models, and service patterns. Use this as the foundation for the MAUI migration while adapting UI and platform-specific code as needed.
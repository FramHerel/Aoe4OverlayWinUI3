# AGENTS.md

This file provides guidance to Codex (Codex.ai/code) when working with code in this repository.

## Project Overview

Aoe4OverlayWinUI3 is a WinUI 3 desktop application for Age of Empires IV players. It provides a real-time overlay that displays opponent statistics (win rates, civilization preferences, match history) by fetching data from the Aoe4World public API (https://aoe4world.com/api/v0/). The overlay uses WinUI 3's always-on-top window capability to stay visible during gameplay.

## Architecture

The solution follows a layered MVVM architecture with three projects:

- **Aoe4OverlayWinUI3.Core** - Portable class library containing business logic, data models (Player, GameMatch, LastMatch), and service contracts (IAoe4ApiService). All API calls to Aoe4World happen here via Aoe4ApiService.

- **Aoe4OverlayWinUI3** - The main WinUI 3 application. Contains Views, ViewModels, service implementations, and platform-specific features (overlay window positioning, hotkeys, settings persistence).

- **Aoe4OverlayWinUI3.Core.Tests** - xUnit test project for the Core library.

### Key Architectural Patterns

- **MVVM**: CommunityToolkit.Mvvm provides ObservableObject, ObservableRecipient, and source generators for INotifyPropertyChanged validation.
- **DI**: Microsoft.Extensions.Hosting with service registration in App.xaml.cs. Services are injected through constructor parameters.
- **Messaging**: StrongReferenceMessenger (CommunityToolkit.Mvvm) enables loose coupling. Define message types in the Messages folder (e.g., OverlayStatusChangedMessage, PlayerChangedMessage).
- **Navigation**: NavigationViewService and PageService manage shell navigation. Pages implement INavigationAware interface.

## Build Commands

```bash
# Build Debug (Visual Studio)
# Use the solution file Aoe4OverlayWinUI3.slnx

# Build Release
dotnet build Aoe4OverlayWinUI3.slnx --configuration Release

# Run tests
dotnet test Aoe4OverlayWinUI3.Core.Tests

# Run single test
dotnet test Aoe4OverlayWinUI3.Core.Tests --filter "FullyQualifiedName~TestMethodName"

# Package MSIX (Self-contained deployment)
dotnet publish Aoe4OverlayWinUI3\Aoe4OverlayWinUI3.csproj --configuration Release /p:RuntimeIdentifier=win-x64
```

## Core Components

- **Aoe4ApiService** (Core/Services/): Fetches player data from Aoe4World API. Methods: GetPlayerAsync, GetMatchHistoryAsync, GetLastMatchAsync. Supports lookup by profile ID or player name.

- **OverlayService** (Services/): Manages the overlay window lifecycle (Show/Hide), always-on-top behavior, and hotkey registration via NHotkey.

- **LocalSettingsService** (Services/): Persists user preferences using Windows.ApplicationModel.Core.CoreApplication.Properties. Common keys: SavedProfileId, OverlayWindowRect etc.

- **SettingsStorageExtensions**: Provides additional JSON serialization support for complex settings objects.

## Localization

User-facing strings are stored in resource files under Strings/{locale}/Resources.resw. The app supports English (en-US) and Chinese (zh-cn). Use x:Uid in XAML for localization.

## Code Style

The project uses .editorconfig with explicit conventions: Allman brace style, file-scoped namespaces, var preference over explicit types, and PascalCase for public members. Interface names start with 'I'.

## Deployment

The app targets Windows App SDK 1.8 with self-contained deployment enabled. It builds as an MSIX package (GenerateAppxPackageOnBuild=true), deployable via the Microsoft Store or sideloaded.

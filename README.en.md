 # Aoe4OverlayWinUI3
 
 [中文](README.md) | English
 
 Aoe4OverlayWinUI3 is a real-time query tool designed for *Age of Empires IV* players. Built with WinUI 3, it delivers a native Windows experience to help you know yourself and your enemy on the battlefield.
 
 ---
 
 ## ✨ Key Features
 
 - Quickly retrieve player rankings, win rates, civilization preferences, and recent match history via the [Aoe4World](https://aoe4world.com/) public API.
 - View opponent info without switching out of the game, using an always-on-top overlay window.
 - Search by player name or Profile ID.
 - Customize overlay position, size, color, and opacity.
 - Multi-language UI: 简体中文 / English.
 - Light/Dark theme toggle.
 
 ## 📁 Project Structure
 
 ```
 Aoe4OverlayWinUI3/
 ├── Aoe4OverlayWinUI3/                  # Main WinUI 3 application
 │   ├── Activation/                     # App activation handlers
 │   ├── Assets/                         # Icons, flags, civ icons, rank badges
 │   │   ├── Civs/                       # Civilization icons
 │   │   ├── Countries/                  # Country flags
 │   │   └── Ranks/                      # Rank badges
 │   ├── Behaviors/                      # NavigationView behaviors
 │   ├── Contracts/                      # Service interfaces
 │   ├── Helpers/                        # Converters, extension methods
 │   ├── Messages/                       # MVVM message types
 │   ├── Models/                         # UI models
 │   ├── Services/                       # Service implementations
 │   ├── Strings/                        # Localization resources
 │   │   ├── en-US/                      # English (United States)
 │   │   └── zh-Hans/                    # Simplified Chinese
 │   ├── Styles/                         # XAML style resources
 │   ├── ViewModels/                     # ViewModels
 │   ├── Views/                          # XAML pages
 │   ├── App.xaml / App.xaml.cs          # App entry, DI registration
 │   ├── MainWindow.xaml / .cs           # Main window
 │   ├── Package.appxmanifest            # MSIX package manifest
 │   └── Aoe4OverlayWinUI3.csproj
 ├── Aoe4OverlayWinUI3.Core/             # Core business logic library
 │   ├── Contracts/Services/             # IAoe4ApiService, IFileService
 │   ├── Helpers/                        # JSON serialization utilities
 │   ├── Models/                         # Data models (Player, GameMatch, LastMatch)
 │   ├── Services/                       # Aoe4ApiService, FileService
 │   └── Aoe4OverlayWinUI3.Core.csproj
 ├── Aoe4OverlayWinUI3.Core.Tests/       # Unit tests (xUnit)
 │   ├── Models/
 │   │   ├── GameMatchModelTests.cs
 │   │   ├── LastMatchModelTests.cs
 │   │   ├── PlayerModelTests.cs
 │   │   └── PlayerSearchResponseModelTests.cs
 │   ├── Services/Aoe4ApiServiceTests.cs
 │   └── Aoe4OverlayWinUI3.Core.Tests.csproj
 ├── .github/workflows/build-sideload.yml  # CI: sideload package build
 ├── .editorconfig                       # Code style configuration
 ├── .gitignore
 ├── .vsconfig                           # Visual Studio component recommendations
 ├── Aoe4OverlayWinUI3.slnx              # Solution file
 ├── README.md                           # Chinese README
 ├── README.en.md                        # This file
 ├── AGENTS.md                           # AI agent instructions
 ```
 
 ## 🏗️ Architecture
 
 The project follows a classic three-layer MVVM architecture:
 
 - **Aoe4OverlayWinUI3.Core** — Platform-agnostic core library containing data models, API service contracts, and business logic.
 - **Aoe4OverlayWinUI3** — WinUI 3 desktop application layer with ViewModels, XAML pages, and service implementations (Overlay, Navigation, Settings, etc.).
 - **Aoe4OverlayWinUI3.Core.Tests** — xUnit unit tests targeting the Core library.
 
 Dependency injection (Microsoft.Extensions.Hosting) handles service registration and resolution. ViewModels communicate via CommunityToolkit.Mvvm's `StrongReferenceMessenger` for loose coupling.
 
 ## 📋 Prerequisites
 
 | Dependency | Version |
 |------------|---------|
 | Windows | 10.0.17763.0 or later |
| .NET SDK | 10.0 |
 | Windows App SDK | 2.0.1 |
 | Visual Studio 2022 | 17.14+ (with "Universal Windows Platform development" workload) |
 
 > Use `dotnet workload install` to install required workloads.
 
 ## 🚀 Installation
 
 **Method A: Microsoft Store (Recommended)**
 
<a href="https://apps.microsoft.com/detail/9np6m86kj0t6?referrer=appbadge&cid=Github&mode=full" target="_blank" rel="noopener noreferrer">
	<img src="https://get.microsoft.com/images/en-us%20dark.svg" width="200"/>
</a>

**Method B: GitHub Releases (sideload)**

1. Download the latest `.cer` and `.msixbundle` from the [Releases page](https://github.com/FramHerel/Aoe4OverlayWinUI3/releases).
2. Double-click the `.cer` and install it into "Trusted Root Certification Authorities" and "Trusted People".
3. Double-click the `.msixbundle` to install.

> The sideload package is the same version and code as the Store package. Since it's signed with a self-signed certificate, Windows may warn that the publisher can't be verified — this is expected.
> If Windows prompts that sideloading must be enabled, turn on Developer Mode under Settings → System → For developers and try again.

## 🛠️ Tech Stack
 
 ### Core Frameworks
 - **UI Framework**: WinUI 3 (Windows App SDK 2.0.1)
 - **Runtime**: .NET 10.0
 - **Architecture**: MVVM (CommunityToolkit.Mvvm 8.4.2)
 - **Dependency Injection**: Microsoft.Extensions.Hosting 10.0
 
 ### Key Dependencies
 - **HTTP Communication**: HttpClient (Microsoft.Extensions.Http)
 - **Messaging**: CommunityToolkit.Mvvm (StrongReferenceMessenger)
 - **Window Management**: WinUIEx 2.9.0
 - **Global Hotkeys**: NHotkey.WinUI 3.0.1
 - **JSON Serialization**: Newtonsoft.Json 13.0.2
 - **DataGrid Control**: CommunityToolkit.WinUI.UI.Controls.DataGrid
 - **Settings Control**: CommunityToolkit.WinUI.Controls.SettingsControls
 
 ### Testing
 - **Test Framework**: xUnit 2.9.3
 - **Test SDK**: Microsoft.NET.Test.Sdk 17.14.1
 - **Code Coverage**: coverlet 6.0.4
 
 ### Data Source
 - **API**: [Aoe4World API](https://aoe4world.com/api/v0/) — Public player data API for Age of Empires IV
 
 ### Deployment
 - **Package Format**: MSIX
- **Distribution**: Microsoft Store / GitHub Releases (sideload)
 
 ## 🔧 Build & Run
 
 ### From Visual Studio
 1. Open the solution file `Aoe4OverlayWinUI3.slnx`.
 2. Select **x64** as the target platform in the toolbar.
 3. Press **F5** to build and run in debug mode.
 
 ### From Command Line
 ```bash
 # Build Debug
 dotnet build Aoe4OverlayWinUI3.slnx --configuration Debug
 
 # Build Release (Sideload mode)
 dotnet build Aoe4OverlayWinUI3.slnx --configuration Release /p:Platform=x64
 ```
 
 ### Run Tests
 ```bash
 dotnet test Aoe4OverlayWinUI3.Core.Tests
 ```
 
### Package as MSIX

- **Sideload installer**: Built automatically by CI and published to [GitHub Releases](https://github.com/FramHerel/Aoe4OverlayWinUI3/releases).
- **Store upload package**: Generate an `.msixupload` with the "Create App Packages" wizard in Visual Studio (select Microsoft Store).
 
 ## 🛡️ Privacy & Security
 
 This tool only fetches data through public APIs. It does not read game memory or modify game files.
 
 All player IDs are cached locally only and are never uploaded to any developer servers.
 
 ## 🤝 Contributions & Feedback
 
- Found a bug? Please submit an [Issue](https://github.com/FramHerel/Aoe4OverlayWinUI3/issues).
- Want new features? Feel free to send a [Pull Request](https://github.com/FramHerel/Aoe4OverlayWinUI3/pulls).
 - Please review the project's `.editorconfig` and coding conventions before contributing.
 
 ## 🌐 Localization
 
 The application supports two languages, switching automatically based on system language:
 - English (en-US)
 - 简体中文 (zh-Hans)
 
 Resource files are located under `Aoe4OverlayWinUI3\Strings\`. Translation improvements are welcome.
 
 ## 📄 License
 
 This project is for learning and personal use only.
 
 API data is provided by [Aoe4World](https://aoe4world.com/). Thanks to the Aoe4World community for maintaining the public data API.
 
 Third-party libraries:
 - [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) — MIT License
 - [CommunityToolkit.WinUI](https://github.com/CommunityToolkit/Windows) — MIT License
 - [Windows App SDK](https://github.com/microsoft/WindowsAppSDK) — MIT License
 - [WinUIEx](https://github.com/dotMorten/WinUIEx) — MIT License
 - [NHotkey](https://github.com/thomaslevesque/NHotkey) — MIT License
 - [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) — MIT License
 - [xUnit](https://github.com/xunit/xunit) — Apache 2.0 License
 - [coverlet](https://github.com/coverlet-coverage/coverlet) — MIT License
 
 ## 💡 Notes
 
 - This tool is developed by the community, with data provided by Aoe4World. It has no official affiliation with Microsoft or World's Edge.
 - The current version is a preview. If you encounter any issues, try restarting the application and contact the developer.
 
 ## 💖 Credits
 
 Special Thanks to **[@gearlam](https://github.com/gearlam)** and their open-source project [AoE4_Overlay_CS](https://github.com/gearlam/AoE4_Overlay_CS). This project was deeply inspired by its design philosophy and core logic.

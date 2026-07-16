 # Aoe4OverlayWinUI3
 
 中文 | [English](README.en.md)
 
 **当前版本：v0.9.15 (Preview)**
 
 一款专为《帝国时代 4》(Age of Empires IV) 玩家打造的对局实时查询工具。基于 WinUI 3 构建，提供原生 Windows 体验，助你在战场上知己知彼。
 
 ---
 
 ## ✨ 核心功能
 
 - 通过 [Aoe4World](https://aoe4world.com/) 公开 API 快速获取玩家排位分数、胜率、文明偏好与最近对局记录。
 - 利用置顶窗口（Always-on-Top），无需切出游戏即可查看对手信息。
 - 支持按玩家名或 Profile ID 搜索。
 - 置顶窗口的位置、大小、颜色、透明度均可自定义。
 - 多语言界面：简体中文 / English。
 - 深色/浅色主题切换。
 
 ## 📁 项目结构
 
 ```
 Aoe4OverlayWinUI3/
 ├── Aoe4OverlayWinUI3/                  # 主应用程序（WinUI 3）
 │   ├── Activation/                     # 应用激活处理
 │   ├── Assets/                         # 图标、国旗、文明图标、段位图标
 │   │   ├── Civs/                       # 文明图标
 │   │   ├── Countries/                  # 国家/地区国旗
 │   │   └── Ranks/                      # 段位图标
 │   ├── Behaviors/                      # NavigationView 行为
 │   ├── Contracts/                      # 服务接口
 │   ├── Helpers/                        # 帮助类（转换器、扩展方法）
 │   ├── Messages/                       # MVVM 消息类型
 │   ├── Models/                         # UI 模型
 │   ├── Services/                       # 服务实现（Overlay、Navigation、Settings 等）
 │   ├── Strings/                        # 本地化资源
 │   │   ├── en-US/                      # 英语（美国）
 │   │   └── zh-Hans/                    # 简体中文
 │   ├── Styles/                         # XAML 样式资源
 │   ├── ViewModels/                     # ViewModel
 │   ├── Views/                          # XAML 页面
 │   ├── App.xaml / App.xaml.cs          # 应用入口、DI 注册
 │   ├── MainWindow.xaml / .cs           # 主窗口
 │   ├── Package.appxmanifest            # MSIX 包清单
 │   └── Aoe4OverlayWinUI3.csproj
 ├── Aoe4OverlayWinUI3.Core/             # 核心业务逻辑库
 │   ├── Contracts/Services/             # IAoe4ApiService, IFileService
 │   ├── Helpers/                        # JSON 序列化工具
 │   ├── Models/                         # 数据模型（Player, GameMatch, LastMatch 等）
 │   ├── Services/                       # Aoe4ApiService, FileService
 │   └── Aoe4OverlayWinUI3.Core.csproj
 ├── Aoe4OverlayWinUI3.Core.Tests/       # 单元测试项目（xUnit）
 │   ├── Models/PlayerModelTests.cs
 │   └── Aoe4OverlayWinUI3.Core.Tests.csproj
 ├── .editorconfig                       # 代码风格配置
 ├── .gitignore
 ├── .vsconfig                           # Visual Studio 组件建议
 ├── Aoe4OverlayWinUI3.slnx              # 解决方案文件
 ├── README.md                           # 本文件
 ├── README.en.md                        # 英文版 README
 ├── AGENTS.md                           # AI 代理指令（Codex）
 └── CLAUDE.md                           # AI 代理指令（Claude）
 ```
 
 ## 🏗️ 架构
 
 项目采用经典的三层 MVVM 架构：
 
 - **Aoe4OverlayWinUI3.Core** — 与平台无关的核心层，封装数据模型、API 调用接口和业务逻辑。
 - **Aoe4OverlayWinUI3** — WinUI 3 桌面应用层，包含 ViewModel、XAML 页面和服务实现（Overlay、Navigation、Settings 等）。
 - **Aoe4OverlayWinUI3.Core.Tests** — 针对核心层的 xUnit 单元测试。
 
 依赖注入（Microsoft.Extensions.Hosting）负责注册和解析所有服务。ViewModel 通过 CommunityToolkit.Mvvm 的 `StrongReferenceMessenger` 实现松耦合通信。
 
 ## 📋 环境要求
 
 | 依赖 | 版本 |
 |------|------|
 | Windows | 10.0.17763.0 及以上 |
 | .NET SDK | 10.0 (Preview) |
 | Windows App SDK | 2.0.1 |
 | Visual Studio 2022 | 17.14+（含"通用 Windows 平台开发"工作负载） |
 
 > 推荐使用 `dotnet workload install` 安装所需工作负载。
 
 ## 🚀 安装方式
 
 **方式 A：Microsoft Store (推荐)**
 
 <a href="https://apps.microsoft.com/detail/9np6m86kj0t6?referrer=appbadge&cid=Github&mode=full" target="_blank" rel="noopener noreferrer">
 	<img src="https://get.microsoft.com/images/en-us%20dark.svg" width="200"/>
 </a>
 ## 🛠️ 技术栈
 
 ### 核心框架
 - **UI 框架**：WinUI 3 (Windows App SDK 2.0.1)
 - **运行时**：.NET 10.0
 - **架构模式**：MVVM (CommunityToolkit.Mvvm 8.4.2)
 - **依赖注入**：Microsoft.Extensions.Hosting 10.0
 
 ### 主要依赖
 - **HTTP 通信**：HttpClient (Microsoft.Extensions.Http)
 - **消息通信**：CommunityToolkit.Mvvm (StrongReferenceMessenger)
 - **窗口管理**：WinUIEx 2.9.0
 - **全局热键**：NHotkey.WinUI 3.0.1
 - **JSON 序列化**：Newtonsoft.Json 13.0.2
 - **DataGrid 控件**：CommunityToolkit.WinUI.UI.Controls.DataGrid
 - **设置控件**：CommunityToolkit.WinUI.Controls.SettingsControls
 
 ### 测试
 - **测试框架**：xUnit 2.9.3
 - **测试 SDK**：Microsoft.NET.Test.Sdk 17.14.1
 - **覆盖率**：coverlet 6.0.4
 
 ### 数据源
 - **API**：[Aoe4World API](https://aoe4world.com/api/v0/) — 公开的《帝国时代 4》玩家数据接口
 
 ### 部署
 - **打包格式**：MSIX
 - **发布渠道**：Microsoft Store / Sideload（开发中）
 
 ## 🔧 构建与运行
 
 ### 从 Visual Studio 构建
 1. 打开解决方案文件 `Aoe4OverlayWinUI3.slnx`。
 2. 在工具栏中选择目标平台为 **x64**。
 3. 按 **F5** 编译并运行调试。
 
 ### 从命令行构建
 ```bash
 # 编译 Debug
 dotnet build Aoe4OverlayWinUI3.slnx --configuration Debug
 
 # 编译 Release（Sideload 模式）
 dotnet build Aoe4OverlayWinUI3.slnx --configuration Release /p:Platform=x64
 ```
 
 ### 运行测试
 ```bash
 dotnet test Aoe4OverlayWinUI3.Core.Tests
 ```
 
 ### 打包 MSIX
 ```bash
 # 生成 MSIX 安装包
 dotnet publish Aoe4OverlayWinUI3\Aoe4OverlayWinUI3.csproj --configuration Release /p:RuntimeIdentifier=win-x64
 ```
 
 打包产物位于 `Aoe4OverlayWinUI3\AppPackages\` 目录下，可双击 `.msixbundle` 文件侧载安装。
 
 ## 🛡️ 隐私与安全 (Privacy & Security)
 
 本工具仅通过公开 API 获取数据，不读取游戏内存，也不修改游戏文件。
 
 您输入的玩家 ID 仅做本地缓存，与 API 查询，不会上传至任何与开发者有关的服务器。
 
 ## 🤝 贡献与反馈
 
 - 发现 Bug？请提交 [Issue](https://github.com/FramHerel/Aoe4OverlayWinUI3/issues)。
 - 想要新功能？欢迎发起 [Pull Request](https://github.com/FramHerel/Aoe4OverlayWinUI3/pulls)。
 - 开发前请查阅项目的 `.editorconfig` 和代码风格约定。
 
 ## 🌐 本地化
 
 应用程序支持两种语言界面，通过系统语言自动切换：
 - English (en-US)
 - 简体中文 (zh-Hans)
 
 资源文件位于 `Aoe4OverlayWinUI3\Strings\` 目录下。欢迎提交翻译改进。
 
 ## 📄 许可
 
 该项目仅用于学习和个人用途。
 
 部分 API 数据由 [Aoe4World](https://aoe4world.com/) 提供，感谢 Aoe4World 社区维护的公开数据接口。
 
 使用的第三方库：
 - [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) — MIT License
 - [CommunityToolkit.WinUI](https://github.com/CommunityToolkit/Windows) — MIT License
 - [Windows App SDK](https://github.com/microsoft/WindowsAppSDK) — MIT License
 - [WinUIEx](https://github.com/dotMorten/WinUIEx) — MIT License
 - [NHotkey](https://github.com/thomaslevesque/NHotkey) — MIT License
 - [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) — MIT License
 - [xUnit](https://github.com/xunit/xunit) — Apache 2.0 License
 - [coverlet](https://github.com/coverlet-coverage/coverlet) — MIT License
 
 ## 💡 注意
 
 - 本工具由社区开发，数据由 Aoe4World 提供，与 Microsoft 或 World's Edge 无官方关联。
 - 当前版本为预览版。如果您在使用过程中遇到问题，请尝试重启应用，并与开发者联系。
 
 ## 💖 致谢
 
 > 感谢 **[@gearlam](https://github.com/gearlam)** 及其开源项目 [AoE4_Overlay_CS](https://github.com/gearlam/AoE4_Overlay_CS)。本项目在设计思路和核心逻辑上深受其启发。

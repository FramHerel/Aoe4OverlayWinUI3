# Aoe4OverlayWinUI3

中文 | [English](README.en.md)

Aoe4OverlayWinUI3 是一款专为《帝国时代 4》(Age of Empires IV) 玩家打造的轻量化对局实时查询工具。基于 WinUI 3 构建，提供原生 Windows 体验，助你在战场上知己知彼。

## ✨ 核心功能

- **实时数据抓取**：通过 Aoe4World API 快速获取玩家排位及最近对局记录。

- **游戏置顶覆盖 (Overlay)**：利用 WinUI 3 的窗口置顶技术，无需切出游戏即可查看对手信息。

- **本地偏好记忆**：自动缓存您最近搜索的 Profile ID，启动即查。

## 🚀 安装方式

**方式 A：Microsoft Store (推荐)**
前往微软商店搜索 "Aoe4 Overlay WinUI 3"，点击“获取”即可自动安装。这是最安全、支持自动更新的方式。
[链接](https://apps.microsoft.com/detail/9np6m86kj0t6)

## 🛠️ 技术栈

- **UI 框架**: WinUI 3 (Windows App SDK)

- **运行时**: .NET 10.0

- **网络通信**: HttpClient (Async/Await)

## 🛡️ 隐私与安全 (Privacy & Security)

我们深知账号安全的重要性：

**无侵入性**：本工具仅通过公开 API 获取数据，不读取游戏内存，不修改游戏文件，100% 远离封号风险。

**隐私保护**：所有玩家 ID 仅本地缓存，绝不上传至任何开发者服务器。

**开源透明**：代码完全开源，接受社区监督。

## 🤝 贡献与反馈

- 发现 Bug? 请提交 Issue。

- 想要新功能? 欢迎发起 Pull Request。

- **数据来源声明**：本工具由社区开发，数据由 Aoe4World 提供，与 Microsoft 或 World's Edge 无官方关联。

## 💡 注意

当前版本为预览版。如果您在使用过程中遇到窗口定位异常或数据加载缓慢，请尝试重启应用，并与开发者联系。

## 💖 致谢

> 感谢 **[@gearlam](https://github.com/gearlam)** 及其开源项目 [AoE4_Overlay_CS](https://github.com/gearlam/AoE4_Overlay_CS)。本项目在设计思路和核心逻辑上深受其启发。

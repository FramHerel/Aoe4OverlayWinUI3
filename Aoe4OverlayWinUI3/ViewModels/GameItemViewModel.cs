using Aoe4OverlayWinUI3.Core.Models;
using Aoe4OverlayWinUI3.Helpers;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Aoe4OverlayWinUI3.ViewModels;

public partial class GameItemViewModel : ObservableObject
{
    public string Map
    {
        get;
    }
    public string Kind
    {
        get;
    }
    public string StartTime
    {
        get;
    }
    public string Result
    {
        get;
        private set;
    }
    public string Civilization
    {
        get;
        private set;
    }
    public string Duration
    {
        get; private set;
    }
    // 对局结果状态：true=胜、false=负、null=未知（结果列据此给文本上色）
    public bool? IsWin
    {
        get; private set;
    }

    public GameItemViewModel(GameMatch gameMatch, string currentProfileId)
    {
        // 地图与对局类型：API 返回的是英文标识，这里转成当前语言的显示文本
        Map = GameContentLocalizer.LocalizeMap(gameMatch.Map);
        Kind = GameContentLocalizer.LocalizeKind(gameMatch.Kind);
        StartTime = gameMatch.StartedAt.ToLocalTime().ToString("g");

        // 从对局数据中找到当前玩家的信息
        var currentPlayer = gameMatch.Teams
            .SelectMany(t => t)         // 展平所有玩家
            .Select(pw => pw.Player)    // 解包 PlayerWrapper
            .FirstOrDefault(p => p.ProfileId.ToString() == currentProfileId.ToString());

        // 根据当前玩家的信息初始化展示属性
        Initialize(gameMatch, currentPlayer);

    }

    // 根据当前玩家的信息初始化展示属性
    private void Initialize(GameMatch gameMatch, PlayerDetails currentPlayer)
    {
        // 提取并预处理展示数据
        // 胜负先用 API 原始值判断（true=胜、false=负、其余为未知），再翻译成显示文本
        var playerResult = currentPlayer?.Result;
        IsWin = string.Equals(playerResult, "win", StringComparison.OrdinalIgnoreCase) ? true
            : string.Equals(playerResult, "loss", StringComparison.OrdinalIgnoreCase) ? false
            : null;
        Result = GameContentLocalizer.LocalizeResult(string.IsNullOrWhiteSpace(playerResult) ? "unknown" : playerResult);

        var playerCivilization = currentPlayer?.Civilization;
        Civilization = string.IsNullOrWhiteSpace(playerCivilization) ? "N/A" : GameContentLocalizer.LocalizeCivilization(playerCivilization);

        // 处理时长：秒 -> mm:ss
        if (gameMatch.Duration.HasValue)
        {
            var t = TimeSpan.FromSeconds(gameMatch.Duration.Value);
            Duration = $"{(int)t.TotalMinutes}:{t.Seconds:D2}";
        }
        else
        {
            Duration = "N/A";
        }
    }
}

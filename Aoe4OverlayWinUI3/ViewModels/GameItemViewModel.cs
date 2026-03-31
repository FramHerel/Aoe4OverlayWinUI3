using Aoe4OverlayWinUI3.Core.Models;
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
    public bool IsWin
    {
        get; private set;
    }

    public GameItemViewModel(GameMatch gameMatch, string currentProfileId)
    {
        Map = gameMatch.Map;
        Kind = gameMatch.Kind;
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
        Result = currentPlayer?.Result?.ToUpper() ?? "UNKNOWN";
        IsWin = Result.ToUpper() == "WIN";
        Civilization = currentPlayer?.Civilization ?? "N/A";

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

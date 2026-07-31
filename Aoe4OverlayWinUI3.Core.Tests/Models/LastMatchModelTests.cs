using System.Text.Json;

using Aoe4OverlayWinUI3.Core.Models;

namespace Aoe4OverlayWinUI3.Core.Tests.Models;

public class LastMatchModelTests
{
    // 完整 LastMatch JSON 映射，包含 modes 统计
    [Fact]
    public void LastMatch_ShouldParseNestedJsonCorrectly()
    {
        var json = @"{""game_id"":987654321,""map"":""Lipany"",""kind"":""rm_1v1"",""started_at"":""2024-06-01T20:00:00Z"",""server"":""EU"",""teams"":[[{""name"":""Alice"",""profile_id"":1001,""civilization"":""english"",""rating"":1200,""country"":""cn"",""modes"":{""rm_1v1"":{""rating"":1200,""rank"":42,""rank_level"":""gold"",""games_count"":100,""wins_count"":60,""losses_count"":40,""win_rate"":0.6},""rm_2v2"":{""rating"":1100,""rank"":10,""rank_level"":""silver"",""games_count"":50,""wins_count"":25,""losses_count"":25,""win_rate"":0.5}}}],[{""name"":""Bob"",""profile_id"":2001,""civilization"":""holy_roman_empire"",""rating"":1150,""country"":"""",""modes"":{}}]]}";

        var match = JsonSerializer.Deserialize<LastMatch>(json);

        Assert.NotNull(match);
        Assert.Equal(987654321, match.GameId);
        Assert.Equal("Lipany", match.Map);
        Assert.Equal("rm_1v1", match.Kind);
        Assert.Equal("EU", match.Server);
        Assert.Equal(2, match.Teams.Count);
        Assert.Single(match.Teams[0]);

        var alice = match.Teams[0][0];
        Assert.Equal("Alice", alice.Name);
        Assert.Equal(1001, alice.ProfileId);
        Assert.Equal("english", alice.Civilization);
        Assert.Equal(1200, alice.Rating);
        Assert.Equal("cn", alice.Country);
        Assert.NotNull(alice.Modes);
        Assert.Equal(2, alice.Modes.Count);

        var soloStats = alice.Modes["rm_1v1"];
        Assert.NotNull(soloStats);
        Assert.Equal("gold", soloStats.RankLevel);
        Assert.Equal(100, soloStats.GamesCount);
        Assert.Equal(60, soloStats.WinsCount);
        Assert.Equal(40, soloStats.LossesCount);
        Assert.Equal(0.6, soloStats.WinRate!.Value, 2);
    }

    // SyncActiveStats：匹配当前游戏模式
    [Fact]
    public void SyncActiveStats_ShouldResolveMatchingMode()
    {
        var player = new LastMatchPlayer
        {
            Modes = new Dictionary<string, ModeDetail>
            {
                ["rm_1v1"] = new ModeDetail { RankLevel = "gold" }
            }
        };

        player.SyncActiveStats("rm_1v1");

        Assert.NotNull(player.ActiveStats);
        Assert.Equal("gold", player.ActiveStats.RankLevel);
    }

    // SyncActiveStats：模式不匹配时清空
    [Fact]
    public void SyncActiveStats_ShouldClearWhenModeDoesNotMatch()
    {
        var player = new LastMatchPlayer
        {
            Modes = new Dictionary<string, ModeDetail>
            {
                ["rm_1v1"] = new ModeDetail { RankLevel = "gold" }
            }
        };

        player.SyncActiveStats("rm_2v2");

        Assert.Null(player.ActiveStats);
    }

    // SyncActiveStats：空或 null 模式名
    [Fact]
    public void SyncActiveStats_ShouldHandleNullOrEmptyKind()
    {
        var player = new LastMatchPlayer
        {
            Modes = new Dictionary<string, ModeDetail>
            {
                ["rm_1v1"] = new ModeDetail()
            }
        };

        player.SyncActiveStats(null);
        Assert.Null(player.ActiveStats);

        player.SyncActiveStats(string.Empty);
        Assert.Null(player.ActiveStats);
    }

    // SyncActiveStats：Modes 为 null 时安全
    [Fact]
    public void SyncActiveStats_ShouldHandleNullModes()
    {
        var player = new LastMatchPlayer();

        player.SyncActiveStats("rm_1v1");

        Assert.Null(player.ActiveStats);
    }

    // DisplayName：缺少 Name 时回退到 ProfileId
    [Fact]
    public void DisplayName_ShouldFallBackToProfileId()
    {
        var player = JsonSerializer.Deserialize<LastMatchPlayer>(@"{""profile_id"":1001}");

        Assert.NotNull(player);
        Assert.Equal("ID:1001", player.DisplayName);
    }

    // DisplayName：有 Name 时直接使用
    [Fact]
    public void DisplayName_ShouldUseName()
    {
        var player = new LastMatchPlayer { Name = "Alice", ProfileId = 1001 };

        Assert.Equal("Alice", player.DisplayName);
    }

    // CountryDisplayName：国家代码转大写
    [Fact]
    public void CountryDisplayName_ShouldUppercaseCountry()
    {
        var player = new LastMatchPlayer { Country = "cn" };

        Assert.Equal("CN", player.CountryDisplayName);
    }

    // CountryDisplayName：缺失时返回空字符串
    [Fact]
    public void CountryDisplayName_ShouldBeEmptyWhenMissing()
    {
        var player = new LastMatchPlayer { Country = string.Empty };

        Assert.Equal(string.Empty, player.CountryDisplayName);
    }

    // CivIconPath：下划线文明名转为标题格式
    [Fact]
    public void CivIconPath_ShouldFormatCivilizationName()
    {
        var player = new LastMatchPlayer { Civilization = "holy_roman_empire" };

        Assert.Equal("ms-appx:///Assets/Civs/Holy Roman Empire.webp", player.CivIconPath);
    }

    // CivIconPath：缺失文明时使用 unknown
    [Fact]
    public void CivIconPath_ShouldUseUnknownForMissingCivilization()
    {
        var player = JsonSerializer.Deserialize<LastMatchPlayer>(@"{""profile_id"":1}");

        Assert.NotNull(player);
        Assert.Equal("ms-appx:///Assets/Civs/unknown.png", player.CivIconPath);
    }
}

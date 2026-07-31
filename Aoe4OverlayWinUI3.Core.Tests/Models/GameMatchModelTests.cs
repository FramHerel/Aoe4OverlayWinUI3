using System.Text.Json;

using Aoe4OverlayWinUI3.Core.Models;

namespace Aoe4OverlayWinUI3.Core.Tests.Models;

public class GameMatchModelTests
{
    // 完整对局 JSON 映射
    [Fact]
    public void GameMatch_ShouldParseNestedJsonCorrectly()
    {
        var json = @"{""game_id"":123456789,""map"":""Altai"",""kind"":""rm_2v2"",""duration"":1545,""started_at"":""2024-06-01T12:30:00Z"",""teams"":[[{""player"":{""profile_id"":1001,""name"":""Alice"",""result"":""win"",""civilization"":""english""}},{""player"":{""profile_id"":1002,""name"":""Bob"",""result"":""win"",""civilization"":""french""}}],[{""player"":{""profile_id"":2001,""name"":""Carol"",""result"":""loss"",""civilization"":""holy_roman_empire""}},{""player"":{""profile_id"":2002,""name"":""Dave"",""result"":""loss"",""civilization"":""rus""}}]]}";

        var game = JsonSerializer.Deserialize<GameMatch>(json);

        Assert.NotNull(game);
        Assert.Equal(123456789, game.GameId);
        Assert.Equal("Altai", game.Map);
        Assert.Equal("rm_2v2", game.Kind);
        Assert.Equal(1545, game.Duration);
        Assert.Equal(2024, game.StartedAt.Year);
        Assert.Equal(6, game.StartedAt.Month);
        Assert.Equal(1, game.StartedAt.Day);
        Assert.Equal(2, game.Teams.Count);
        Assert.Equal(2, game.Teams[0].Count);
        Assert.Equal(1001, game.Teams[0][0].Player.ProfileId);
        Assert.Equal("Alice", game.Teams[0][0].Player.Name);
        Assert.Equal("win", game.Teams[0][0].Player.Result);
        Assert.Equal("english", game.Teams[0][0].Player.Civilization);
        Assert.Equal("loss", game.Teams[1][0].Player.Result);
    }

    // 可空字段缺失时不影响反序列化
    [Fact]
    public void GameMatch_ShouldAllowMissingDuration()
    {
        var json = @"{""game_id"":1,""map"":""Altai"",""kind"":""rm_1v1"",""started_at"":""2024-06-01T12:30:00Z"",""teams"":[]}";

        var game = JsonSerializer.Deserialize<GameMatch>(json);

        Assert.NotNull(game);
        Assert.Null(game.Duration);
        Assert.Empty(game.Teams);
    }

    // API 根对象 GamesResponse 的解析
    [Fact]
    public void GamesResponse_ShouldParseGamesList()
    {
        var json = @"{""games"":[{""game_id"":1,""map"":""Altai"",""kind"":""rm_1v1"",""started_at"":""2024-06-01T12:30:00Z"",""teams"":[]},{""game_id"":2,""map"":""Lipany"",""kind"":""rm_2v2"",""started_at"":""2024-06-02T12:30:00Z"",""teams"":[]}]}";

        var response = JsonSerializer.Deserialize<GamesResponse>(json);

        Assert.NotNull(response);
        Assert.NotNull(response.Games);
        Assert.Equal(2, response.Games.Count);
        Assert.Equal(1, response.Games[0].GameId);
        Assert.Equal("Lipany", response.Games[1].Map);
    }
}

using System.Text.Json;

using Aoe4OverlayWinUI3.Core.Models;

namespace Aoe4OverlayWinUI3.Core.Tests.Models;

public class PlayerSearchResponseModelTests
{
    // 搜索接口返回多个玩家
    [Fact]
    public void PlayerSearchResponse_ShouldParsePlayersList()
    {
        var json = @"{""players"":[{""name"":""Alice"",""profile_id"":1001,""steam_id"":""123"",""site_url"":""http://aoe4world.com/players/1001"",""country"":""cn""},{""name"":""Bob"",""profile_id"":2002,""steam_id"":"""",""site_url"":""http://aoe4world.com/players/2002"",""country"":""us""}]}";

        var response = JsonSerializer.Deserialize<PlayerSearchResponse>(json);

        Assert.NotNull(response);
        Assert.NotNull(response.Players);
        Assert.Equal(2, response.Players.Count);
        Assert.Equal("Alice", response.Players[0].Name);
        Assert.Equal(1001, response.Players[0].ProfileId);
        Assert.Equal("Bob", response.Players[1].Name);
    }

    // 搜索无结果时为空列表
    [Fact]
    public void PlayerSearchResponse_ShouldHandleEmptyPlayers()
    {
        var json = @"{""players"":[]}";

        var response = JsonSerializer.Deserialize<PlayerSearchResponse>(json);

        Assert.NotNull(response);
        Assert.NotNull(response.Players);
        Assert.Empty(response.Players);
    }

    // 字段缺失时保持 null
    [Fact]
    public void PlayerSearchResponse_ShouldAllowMissingPlayers()
    {
        var json = @"{}";

        var response = JsonSerializer.Deserialize<PlayerSearchResponse>(json);

        Assert.NotNull(response);
        Assert.Null(response.Players);
    }
}

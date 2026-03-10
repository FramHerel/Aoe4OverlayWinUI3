using System.Text.Json;
using System.Text.Json.Serialization;
using Aoe4OverlayWinUI3.Core.Models;
using Aoe4OverlayWinUI3.Core.Models;
using Xunit;

namespace Aoe4OverlayWinUI3.Core.Tests.Models;

public class PlayerModelTests
{
    // 数据模型单元测试
    [Fact]
    public void Player_ShouldParseBasicJsonCorrectly()
    {
        // 示例字符串
        var json = @"{""name"":""炽焰咆哮虎"",""profile_id"":4635035,""steam_id"":""76561198325518070"",""site_url"":""http://aoe4world.com/players/4635035"",""country"":""cn""}";

        // 执行 (Act)
        var player = JsonSerializer.Deserialize<Player>(json);

        // 断言 (Assert)
        Assert.NotNull(player);
        Assert.Equal("炽焰咆哮虎", player.Name);
        Assert.Equal(4635035, player.ProfileId);
        Assert.Equal("76561198325518070", player.SteamId);
        Assert.Equal("http://aoe4world.com/players/4635035", player.SiteUrl);
        Assert.Equal("cn", player.Country);
    }

    // 缺失 SteamId 字段的情况
    [Fact]
    public void Player_ShouldHandleMissingFieldsGracefully()
    {
        var json = @"{""name"":""炽焰咆哮虎"",""profile_id"":4635035,""steam_id"":"""",""site_url"":""http://aoe4world.com/players/4635035"",""country"":""cn""}";

        var player = JsonSerializer.Deserialize<Player>(json);

        Assert.NotNull(player);
        Assert.Equal(string.Empty, player.SteamId);
    }
}
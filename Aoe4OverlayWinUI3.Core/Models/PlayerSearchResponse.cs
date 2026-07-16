using System.Text.Json.Serialization;

namespace Aoe4OverlayWinUI3.Core.Models;

public class PlayerSearchResponse
{
    [JsonPropertyName("players")]
    public List<Player>? Players
    {
        get; set;
    }
}

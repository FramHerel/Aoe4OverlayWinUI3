using System.Text.Json.Serialization;

namespace Aoe4OverlayWinUI3.Core.Models;

public class Player
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("profile_id")]
    public long ProfileId
    {
        get; set;
    }

    [JsonPropertyName("steam_id")]
    public string? SteamId { get; set; } = string.Empty;

    [JsonPropertyName("site_url")]
    public string SiteUrl { get; set; } = string.Empty;

    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    [JsonIgnore]
    public Uri? SafeSiteUrl => Uri.TryCreate(SiteUrl, UriKind.Absolute, out var uri) ? uri : null;
}
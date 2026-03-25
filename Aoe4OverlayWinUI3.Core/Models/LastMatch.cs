using System.Globalization;
using System.Text.Json.Serialization;
using static Aoe4OverlayWinUI3.Core.Models.LastMatch;

namespace Aoe4OverlayWinUI3.Core.Models;

public class LastMatch
{
    [JsonPropertyName("game_id")]
    public long GameId
    {
        get; set;
    }

    [JsonPropertyName("map")]
    public string Map
    {
        get; set;
    }

    [JsonPropertyName("kind")]
    public string Kind
    {
        get; set;
    }

    [JsonPropertyName("started_at")]
    public DateTime StartedAt
    {
        get; set;
    }
    [JsonPropertyName("server")]
    public string Server
    {
        get; set;
    }

    [JsonPropertyName("teams")]
    public List<List<LastMatchPlayer>> Teams
    {
        get; set;
    }

}
public class LastMatchPlayer
{
    [JsonPropertyName("name")]
    public string Name
    {
        get; set;
    }

    [JsonPropertyName("profile_id")]
    public long ProfileId
    {
        get; set;
    }

    [JsonPropertyName("civilization")]
    public string Civilization
    {
        get; set;
    }

    [JsonPropertyName("rating")]
    public int? Rating
    {
        get; set;
    }

    [JsonPropertyName("country")]
    public string Country
    {
        get; set;
    }

    [JsonPropertyName("modes")]
    public Dictionary<string, ModeDetail> Modes
    {
        get; set;
    }

    [JsonIgnore]
    public ModeDetail? ActiveStats
    {
        get; private set;
    }
    public void SyncActiveStats(string? gameKind)
    {
        if (Modes == null || string.IsNullOrEmpty(gameKind))
        {
            ActiveStats = null;
            return;
        }

        if (Modes.TryGetValue(gameKind, out var stats))
        {
            ActiveStats = stats;
        }
        else
        {
            ActiveStats = null;
        }
    }

    // UI

    [JsonIgnore]
    public string DisplayName => Name ?? $"ID:{ProfileId}";

    // TODO: 增加段位图标
    //[JsonIgnore]
    //public string RankIconPath => ActiveStats?.RankLevel != null
    //? $"ms-appx:///Assets/Ranks/{ActiveStats.RankLevel.ToLower()}.png"
    //: "ms-appx:///Assets/Ranks/unranked.png"; // 兜底图标

    [JsonIgnore]
    public string FlagIconPath => Country != null
    ? $"ms-appx:///Assets/Countries/{Country.ToLower()}.png"
    : "ms-appx:///Assets/Countries/unknown.png";

    [JsonIgnore]
    public string CivIconPath
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Civilization))
            {
                return "ms-appx:///Assets/Civs/unknown.png";
            }

            string spacedCiv = Civilization.Replace("_", " ");
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            string formattedCiv = textInfo.ToTitleCase(spacedCiv);
            return $"ms-appx:///Assets/Civs/{formattedCiv}.webp";
        }
    }
}

public class ModeDetail
{
    [JsonPropertyName("rating")]
    public int? Rating
    {
        get; set;
    }

    [JsonPropertyName("rank")]
    public int? Rank
    {
        get; set;
    }

    [JsonPropertyName("rank_level")]
    public string RankLevel
    {
        get; set;
    }

    [JsonPropertyName("games_count")]
    public int GamesCount
    {
        get; set;
    }

    [JsonPropertyName("wins_count")]
    public int WinsCount
    {
        get; set;
    }

    [JsonPropertyName("losses_count")]
    public int LossesCount
    {
        get; set;
    }

    [JsonPropertyName("win_rate")]
    public float? WinRate
    {
        get; set;
    }
}
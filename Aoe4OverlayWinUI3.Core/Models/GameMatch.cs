using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Aoe4OverlayWinUI3.Core.Models;

// API 返回的根对象
public class GamesResponse
{
    [JsonPropertyName("games")]
    public List<GameMatch> Games
    {
        get; set;
    }
}

// 每场对局的详细信息
public class GameMatch
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
    }// rm_solo 或者 rm_2v2...

    [JsonPropertyName("duration")]
    public int? Duration
    {
        get; set;
    } // 持续时间（秒）

    [JsonPropertyName("started_at")]
    public DateTime StartedAt
    {
        get; set;
    }

    // 对局中的玩家信息
    [JsonPropertyName("teams")]
    public List<List<MatchPlayer>> Teams
    {
        get; set;
    }
}

// 每个玩家在对局中的信息
public class MatchPlayer
{
    [JsonPropertyName("player")]
    public PlayerInfo Player
    {
        get; set;
    }

    [JsonPropertyName("result")]
    public string Result
    {
        get; set;
    } // "win" 或 "loss"

    [JsonPropertyName("civilization")]
    public string Civilization
    {
        get; set;
    }
}

// 玩家基本信息
public class PlayerInfo
{
    [JsonPropertyName("id")]
    public long ProfileId
    {
        get; set;
    }

    [JsonPropertyName("name")]
    public string Name
    {
        get; set;
    }
    [JsonPropertyName("country")]
    public string Country
    {
        get; set;
    }
}
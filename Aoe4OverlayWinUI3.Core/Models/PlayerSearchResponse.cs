using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Aoe4OverlayWinUI3.Core.Models;

public class PlayerSearchResponse
{
    [JsonPropertyName("players")]
    public List<Player>? Players
    {
        get; set;
    }
}

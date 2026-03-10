using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Aoe4OverlayWinUI3.Core.Contracts.Services;
using Aoe4OverlayWinUI3.Core.Models;

namespace Aoe4OverlayWinUI3.Core.Services;

public class Aoe4ApiService:IAoe4ApiService
{
    private readonly HttpClient _httpClient;
    // API 基础路径
    private const string BaseUrl = "https://aoe4world.com/api/v0/";

    public Aoe4ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Player?> GetPlayerAsync(string profileId)
    {
        if (string.IsNullOrEmpty(profileId)) return null;

        try
        {
            // 拼接 URL: https://aoe4world.com/api/v0/players/12345
            var url = $"{BaseUrl}players/{profileId}";

            // 使用 GetFromJsonAsync 直接将结果映射到你的 Player 类
            var player = await _httpClient.GetFromJsonAsync<Player>(url);
            return player;
        }
        catch (HttpRequestException)
        {
            // 404 或网络问题
            return null;
        }
        catch (Exception)
        {
            // 其他未知错误
            return null;
        }
    }
}

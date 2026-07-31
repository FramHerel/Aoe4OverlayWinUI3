using System.Net;
using System.Text;
using System.Text.Json;

using Aoe4OverlayWinUI3.Core.Models;
using Aoe4OverlayWinUI3.Core.Services;

namespace Aoe4OverlayWinUI3.Core.Tests.Services;

public class Aoe4ApiServiceTests
{
    // Aoe4ApiService 的单元测试。
    // 通过 StubHttpMessageHandler 拦截 HttpClient 请求并返回预设响应，
    // 测试全程不访问真实网络，只验证服务自身的分支逻辑。

    private const string BaseUrl = "https://aoe4world.com/api/v0/";

    // 场景：查询字符串是数字时，服务应优先请求 players/{id} 接口。
    // 验证请求 URL 正确，且返回的 Player JSON 能正常反序列化。
    [Fact]
    public async Task GetPlayerAsync_NumericId_UsesPlayerEndpoint()
    {
        using var client = CreateClient(request =>
        {
            Assert.Equal($"{BaseUrl}players/1001", request.RequestUri?.AbsoluteUri);
            return JsonResponse(new Player { Name = "Alice", ProfileId = 1001 });
        });
        var service = new Aoe4ApiService(client);

        var player = await service.GetPlayerAsync("1001");

        Assert.NotNull(player);
        Assert.Equal("Alice", player.Name);
        Assert.Equal(1001, player.ProfileId);
    }

    // 场景：数字 ID 请求返回 404 时，服务应自动回退到名称搜索接口。
    // 测试同时记录请求 URL，确认回退顺序和请求次数符合预期。
    [Fact]
    public async Task GetPlayerAsync_NumericIdNotFound_FallsBackToNameSearch()
    {
        var requests = new List<string>();
        // 每收到一个请求就记录一次，后面用来验证回退流程。
        using var client = CreateClient(request =>
        {
            requests.Add(request.RequestUri!.AbsoluteUri);
            // 第一次请求 players/99999 返回 404，触发名称搜索回退。
            if (request.RequestUri.AbsoluteUri == $"{BaseUrl}players/99999")
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            // 第二次请求 players/search，返回名称搜索结果。
            return JsonResponse(new PlayerSearchResponse
            {
                Players = new List<Player>
                {
                    new Player { Name = "Alice", ProfileId = 99999 }
                }
            });
        });
        var service = new Aoe4ApiService(client);

        var player = await service.GetPlayerAsync("99999");

        // 回退后应返回搜索到的玩家，并确认只发起了两次请求。
        Assert.NotNull(player);
        Assert.Equal("Alice", player.Name);
        Assert.Equal(2, requests.Count);
        Assert.Contains(requests, uri => uri.StartsWith($"{BaseUrl}players/search?query=", StringComparison.Ordinal));
    }

    // 场景：空查询字符串应直接返回 null，不发起任何 HTTP 请求。
    // 若服务真的发起了请求，lambda 会抛异常，测试立即失败。
    [Fact]
    public async Task GetPlayerAsync_EmptyQuery_ReturnsNullWithoutRequest()
    {
        using var client = CreateClient(_ => throw new InvalidOperationException("不应发起请求"));
        var service = new Aoe4ApiService(client);

        // 空字符串属于无效查询，服务应返回 null。
        Assert.Null(await service.GetPlayerAsync(string.Empty));
    }

    // 场景：服务器返回 500 等错误状态时，服务应吞掉异常并返回 null，
    // 避免 Profile/Settings 页面因 API 故障直接崩溃。
    [Fact]
    public async Task GetPlayerAsync_ServerError_ReturnsNull()
    {
        using var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var service = new Aoe4ApiService(client);

        Assert.Null(await service.GetPlayerAsync("Alice"));
    }

    // 场景：比赛历史接口失败时，服务应返回空列表而不是抛异常，
    // 让 GamesList 页面可以正常展示空状态。
    [Fact]
    public async Task GetMatchHistoryAsync_ServerError_ReturnsEmptyList()
    {
        using var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var service = new Aoe4ApiService(client);

        var matches = await service.GetMatchHistoryAsync("1001", 50);

        Assert.Empty(matches);
    }

    // 场景：玩家最近对局不存在时，API 返回 404，服务应返回 null。
    [Fact]
    public async Task GetLastMatchAsync_NotFound_ReturnsNull()
    {
        using var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var service = new Aoe4ApiService(client);

        Assert.Null(await service.GetLastMatchAsync("1001"));
    }

    // 场景：最近对局接口返回错误状态时，服务应返回 null。
    [Fact]
    public async Task GetLastMatchAsync_ServerError_ReturnsNull()
    {
        using var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var service = new Aoe4ApiService(client);

        Assert.Null(await service.GetLastMatchAsync("1001"));
    }

    // 场景：profileId 为空时直接返回 null，不发起请求。
    [Fact]
    public async Task GetLastMatchAsync_EmptyProfile_ReturnsNullWithoutRequest()
    {
        using var client = CreateClient(_ => throw new InvalidOperationException("不应发起请求"));
        var service = new Aoe4ApiService(client);

        // 空 profileId 属于无效参数，服务应直接返回 null。
        Assert.Null(await service.GetLastMatchAsync(string.Empty));
    }

    // 场景：成功获取最近对局后，服务应调用 SyncActiveStats，
    // 按对局 kind 找到匹配的 ModeDetail 并写入 ActiveStats。
    // 这里同时准备 rm_1v1 和 rm_2v2，验证只同步与 kind 匹配的模式。
    [Fact]
    public async Task GetLastMatchAsync_Success_SyncsActiveStatsByKind()
    {
        // 构造一个包含两种模式的最近对局，当前对局模式是 rm_1v1。
        var match = new LastMatch
        {
            GameId = 1,
            Map = "Altai",
            Kind = "rm_1v1",
            Server = "EU",
            Teams = new List<List<LastMatchPlayer>>
            {
                new List<LastMatchPlayer>
                {
                    new LastMatchPlayer
                    {
                        Name = "Alice",
                        ProfileId = 1001,
                        Civilization = "english",
                        Country = "cn",
                        Modes = new Dictionary<string, ModeDetail>
                        {
                            ["rm_1v1"] = new ModeDetail { RankLevel = "gold" },
                            ["rm_2v2"] = new ModeDetail { RankLevel = "silver" }
                        }
                    }
                }
            }
        };

        using var client = CreateClient(request =>
        {
            Assert.Equal($"{BaseUrl}players/1001/games/last", request.RequestUri?.AbsoluteUri);
            return JsonResponse(match);
        });
        var service = new Aoe4ApiService(client);

        var result = await service.GetLastMatchAsync("1001");

        // ActiveStats 应指向 rm_1v1 的 gold，而不是 rm_2v2 的 silver。
        Assert.NotNull(result);
        var player = result.Teams[0][0];
        Assert.Equal("gold", player.ActiveStats?.RankLevel);
    }

    // 创建使用 StubHttpMessageHandler 的 HttpClient，
    // responder 回调负责根据请求返回预设的 HttpResponseMessage。
    private static HttpClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        return new HttpClient(new StubHttpMessageHandler(responder));
    }

    // 把测试对象序列化成 JSON，并包装成 200 OK 的响应内容。
    private static HttpResponseMessage JsonResponse(object body)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };
    }

    // 自定义 HttpMessageHandler，不访问网络，
    // 直接把 responder 的返回值交给 HttpClient 使用。
    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        // 保存响应工厂，后续每个请求都通过它生成响应。
        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        // 同步生成响应并用 Task.FromResult 包装，满足异步调用约定。
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responder(request));
        }
    }
}

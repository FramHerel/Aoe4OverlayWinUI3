using System.Net;
using System.Text;
using System.Text.Json;

using Aoe4OverlayWinUI3.Core.Models;
using Aoe4OverlayWinUI3.Core.Services;

namespace Aoe4OverlayWinUI3.Core.Tests.Services;

public class Aoe4ApiServiceTests
{
    private const string BaseUrl = "https://aoe4world.com/api/v0/";

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

    [Fact]
    public async Task GetPlayerAsync_NumericIdNotFound_FallsBackToNameSearch()
    {
        var requests = new List<string>();
        using var client = CreateClient(request =>
        {
            requests.Add(request.RequestUri!.AbsoluteUri);
            if (request.RequestUri.AbsoluteUri == $"{BaseUrl}players/99999")
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

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

        Assert.NotNull(player);
        Assert.Equal("Alice", player.Name);
        Assert.Equal(2, requests.Count);
        Assert.Contains(requests, uri => uri.StartsWith($"{BaseUrl}players/search?query=", StringComparison.Ordinal));
    }

    [Fact]
    public async Task GetPlayerAsync_EmptyQuery_ReturnsNullWithoutRequest()
    {
        using var client = CreateClient(_ => throw new InvalidOperationException("不应发起请求"));
        var service = new Aoe4ApiService(client);

        Assert.Null(await service.GetPlayerAsync(string.Empty));
    }

    [Fact]
    public async Task GetPlayerAsync_ServerError_ReturnsNull()
    {
        using var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var service = new Aoe4ApiService(client);

        Assert.Null(await service.GetPlayerAsync("Alice"));
    }

    [Fact]
    public async Task GetMatchHistoryAsync_ServerError_ReturnsEmptyList()
    {
        using var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var service = new Aoe4ApiService(client);

        var matches = await service.GetMatchHistoryAsync("1001", 50);

        Assert.Empty(matches);
    }

    [Fact]
    public async Task GetLastMatchAsync_NotFound_ReturnsNull()
    {
        using var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var service = new Aoe4ApiService(client);

        Assert.Null(await service.GetLastMatchAsync("1001"));
    }

    [Fact]
    public async Task GetLastMatchAsync_ServerError_ReturnsNull()
    {
        using var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var service = new Aoe4ApiService(client);

        Assert.Null(await service.GetLastMatchAsync("1001"));
    }

    [Fact]
    public async Task GetLastMatchAsync_EmptyProfile_ReturnsNullWithoutRequest()
    {
        using var client = CreateClient(_ => throw new InvalidOperationException("不应发起请求"));
        var service = new Aoe4ApiService(client);

        Assert.Null(await service.GetLastMatchAsync(string.Empty));
    }

    [Fact]
    public async Task GetLastMatchAsync_Success_SyncsActiveStatsByKind()
    {
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

        Assert.NotNull(result);
        var player = result.Teams[0][0];
        Assert.Equal("gold", player.ActiveStats?.RankLevel);
    }

    private static HttpClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        return new HttpClient(new StubHttpMessageHandler(responder));
    }

    private static HttpResponseMessage JsonResponse(object body)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responder(request));
        }
    }
}

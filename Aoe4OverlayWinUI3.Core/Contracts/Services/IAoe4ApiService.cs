using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aoe4OverlayWinUI3.Core.Models;

namespace Aoe4OverlayWinUI3.Core.Contracts.Services;

public interface IAoe4ApiService
{
    // 根据 ProfileId 获取单个玩家信息
    Task<Player?> GetPlayerAsync(string profileId);

    // 获取比赛历史：请求失败时抛出异常，正常但无对局时返回空列表
    Task<List<GameMatch>> GetMatchHistoryAsync(string profileId, int limit);
    Task<LastMatch?> GetLastMatchAsync(string profileId, CancellationToken ct = default);
}

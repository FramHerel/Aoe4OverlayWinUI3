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

    Task<List<GameMatch>> GetMatchHistoryAsync(string profileId, int limit = 10);
}

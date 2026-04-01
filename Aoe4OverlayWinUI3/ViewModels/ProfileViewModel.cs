using Aoe4OverlayWinUI3.Contracts.Services;
using Aoe4OverlayWinUI3.Core.Contracts.Services;
using Aoe4OverlayWinUI3.Core.Models;
using Aoe4OverlayWinUI3.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Aoe4OverlayWinUI3.ViewModels;

public partial class ProfileViewModel : ObservableRecipient
{
    // 依赖的服务
    private readonly IAoe4ApiService _aoe4ApiService;
    private readonly ILocalSettingsService _localSettingsService;

    // 加载情况
    [ObservableProperty]
    public partial bool IsLoading
    {
        get;
        set;
    }

    // 玩家数据
    [ObservableProperty]
    public partial Player? Player
    {
        get;
        set;
    }

    // 构造函数
    public ProfileViewModel(IAoe4ApiService aoe4ApiService, ILocalSettingsService localSettingsService)
    {
        _aoe4ApiService = aoe4ApiService;
        _localSettingsService = localSettingsService;

        // 注册消息接收器，监听 PlayerChangedMessage
        StrongReferenceMessenger.Default.Register<PlayerChangedMessage>(this, (r, m) =>
        {
            this.Player = m.Value;
        });
    }
    // 加载数据的方法
    public async Task OnNavigatedTo()
    {
        // 如果已经有数据了，就不重复加载
        if (Player != null) return;

        var savedId = await _localSettingsService.ReadSettingAsync<string>("SavedProfileId");
        if (!string.IsNullOrWhiteSpace(savedId))
        {
            await FetchDataAsync(savedId);
        }
    }

    // 从 API 获取数据的方法
    private async Task FetchDataAsync(string id)
    {
        IsLoading = true;
        try
        {
            Player = await _aoe4ApiService.GetPlayerAsync(id);
        }
        finally
        {
            IsLoading = false;
        }
    }
}

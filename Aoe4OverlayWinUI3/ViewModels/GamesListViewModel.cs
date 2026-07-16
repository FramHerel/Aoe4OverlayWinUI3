using System.Collections.ObjectModel;

using Aoe4OverlayWinUI3.Contracts.Services;
using Aoe4OverlayWinUI3.Contracts.ViewModels;
using Aoe4OverlayWinUI3.Core.Contracts.Services;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;

namespace Aoe4OverlayWinUI3.ViewModels;

public partial class GamesListViewModel : ObservableRecipient, INavigationAware
{
    private readonly IAoe4ApiService _aoe4ApiService;

    private readonly ILocalSettingsService _localSettingsService;

    // 自动刷新定时器：每 1 分钟触发一次刷新检查
    private readonly DispatcherTimer _autoRefreshTimer;
    // 上次成功刷新的 UTC 时间戳，用于 10 秒最低间隔限速
    private DateTime _lastRefreshTime = DateTime.MinValue;
    // 最小刷新间隔：10 秒内不允许发起第二次请求
    private static readonly TimeSpan MinRefreshInterval = TimeSpan.FromSeconds(10);
    // 自动刷新间隔：每 60 秒触发一次定时器 tick
    private static readonly TimeSpan AutoRefreshInterval = TimeSpan.FromMinutes(1);

    public ObservableCollection<GameItemViewModel> Games { get; } = [];

    [ObservableProperty]
    public partial bool IsLoading
    {
        get; set;
    }

    public GamesListViewModel(IAoe4ApiService aoe4ApiService, ILocalSettingsService localSettingsService)
    {
        _aoe4ApiService = aoe4ApiService;
        _localSettingsService = localSettingsService;

        _autoRefreshTimer = new DispatcherTimer();
        // 定时器 tick 回调
        _autoRefreshTimer.Tick += OnAutoRefreshTick;
        // 设置自动刷新间隔（OnNavigatedTo 中 Start，OnNavigatedFrom 中 Stop）
        _autoRefreshTimer.Interval = AutoRefreshInterval;
    }

    public async void OnNavigatedTo(object parameter)
    {
        // 进入页面时启动自动刷新定时器
        _autoRefreshTimer.Start();
        await LoadDataAsync();
    }

    public void OnNavigatedFrom()
    {
        // 离开页面时停止自动刷新定时器，避免后台浪费资源
        _autoRefreshTimer.Stop();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        // 受 10 秒最低间隔限制，点击太频繁不会发起请求
        if (!CanRefresh())
        {
            return;
        }

        // 先更新时间戳再请求，确保 10 秒窗口有效
        _lastRefreshTime = DateTime.UtcNow;
        await LoadDataAsync();
    }

    // 定时器回调：每 1 分钟触发一次，受相同 10 秒限速约束
    private void OnAutoRefreshTick(object? sender, object e)
    {
        if (!CanRefresh())
        {
            return;
        }

        _lastRefreshTime = DateTime.UtcNow;
        // 定时器回调中 fire-and-forget 执行数据加载
        _ = LoadDataAsync();
    }

    // 检查距离上次刷新是否已超过 10 秒最小间隔
    private bool CanRefresh()
    {
        return (DateTime.UtcNow - _lastRefreshTime) >= MinRefreshInterval;
    }

    public async Task LoadDataAsync()
    {
        IsLoading = true;
        // 从本地设置中读取保存的 profileId
        var profileId = await _localSettingsService.ReadSettingAsync<string>("SavedProfileId");
        // 如果没有绑定帐户，提示用户先绑定帐户
        if (string.IsNullOrEmpty(profileId))
        {
            // TODO: 添加提示：请先在设置中绑定帐户
            return;
        }

        try
        {
            // 调用 API 获取数据
            // TODO: 请求限制暂时固定为 50，后续可以改成可配置的参数
            var matches = await _aoe4ApiService.GetMatchHistoryAsync(profileId, 50);

            // 转换并填充集合
            Games.Clear();
            foreach (var match in matches)
            {
                // 将 Model 包装 ItemViewModel
                Games.Add(new GameItemViewModel(match, profileId));
            }
        }
        catch (Exception ex)
        {
            // TODO: 处理网络错误
            System.Diagnostics.Debug.WriteLine($"加载对局失败: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}

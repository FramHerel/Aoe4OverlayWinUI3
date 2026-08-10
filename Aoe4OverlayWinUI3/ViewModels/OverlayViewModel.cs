using Aoe4OverlayWinUI3.Contracts.Services;
using Aoe4OverlayWinUI3.Core.Contracts.Services;
using Aoe4OverlayWinUI3.Core.Models;
using Aoe4OverlayWinUI3.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;

namespace Aoe4OverlayWinUI3.ViewModels;

public partial class OverlayViewModel : ObservableRecipient, IDisposable
{
    private readonly IAoe4ApiService _aoe4ApiService;
    private readonly ILocalSettingsService _localSettingsService;

    // 此取消令牌的生命周期与当前 ViewModel 实例一致。
    // 覆盖层窗口关闭时取消令牌，使尚未完成的最近对局请求尽快结束，
    // 避免已关闭窗口对应的 ViewModel 被异步请求继续持有。
    private readonly CancellationTokenSource _disposeCancellationTokenSource = new();
    private readonly CancellationToken _disposeCancellationToken;

    private DispatcherQueueTimer? _refreshTimer;
    private string? _targetProfileId;

    // Dispose 可能被不同的关闭路径重复调用，此标记用于保证清理逻辑只执行一次。
    private bool _isDisposed;

    // 加载情况
    [ObservableProperty]
    public partial bool IsLoading
    {
        get;
        set;
    }

    [ObservableProperty]
    public partial LastMatch? CurrentMatch
    {
        get;
        set;
    }

    public OverlayViewModel(IAoe4ApiService aoe4ApiService, ILocalSettingsService localSettingsService)
    {
        _aoe4ApiService = aoe4ApiService;
        _localSettingsService = localSettingsService;
        _disposeCancellationToken = _disposeCancellationTokenSource.Token;

        // 定时器
        SetupTimer();

        // 热切换
        StrongReferenceMessenger.Default.Register<PlayerChangedMessage>(this, (r, m) =>
        {
            // 消息可能与窗口关闭发生在相邻的 UI 消息循环中。
            // 已释放的实例不再响应玩家切换，也不能重新启动刷新定时器。
            if (_isDisposed)
            {
                return;
            }

            _targetProfileId = m.Value.ProfileId.ToString();
            _ = RefreshDataAsync();
            _refreshTimer?.Start();
        });

        _ = InitializeOverlayAsync();
    }

    private async Task InitializeOverlayAsync()
    {
        _targetProfileId = await _localSettingsService.ReadSettingAsync<string>("SavedProfileId");

        // 读取本地设置期间窗口可能已经关闭。
        // 此处再次检查，防止异步初始化完成后继续请求数据并启动旧实例的定时器。
        if (_disposeCancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (!string.IsNullOrEmpty(_targetProfileId))
        {
            await RefreshDataAsync();
            _refreshTimer?.Start();
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Cannot find profile id, please bind.");
        }
    }

    private void SetupTimer()
    {
        _refreshTimer = DispatcherQueue.GetForCurrentThread().CreateTimer();
        _refreshTimer.Interval = TimeSpan.FromSeconds(30); // 30秒一次

        // 使用具名处理方法代替匿名 lambda，以便 Dispose 时能够准确解除事件订阅。
        _refreshTimer.Tick += OnRefreshTimerTick;
    }

    // async void 仅用于 DispatcherQueueTimer 的事件处理入口；
    // 实际异步刷新逻辑仍封装在返回 Task 的 RefreshDataAsync 中。
    private async void OnRefreshTimerTick(DispatcherQueueTimer sender, object args)
    {
        await RefreshDataAsync();
    }

    private async Task RefreshDataAsync()
    {
        // ViewModel 释放后不再发起新的网络请求。
        if (_disposeCancellationToken.IsCancellationRequested)
        {
            return;
        }

        IsLoading = true;
        try
        {
            if (string.IsNullOrEmpty(_targetProfileId))
            {
                return;
            }

            // 将生命周期取消令牌传入 API：窗口关闭时可以取消正在进行的 HTTP 请求。
            var match = await _aoe4ApiService.GetLastMatchAsync(_targetProfileId, _disposeCancellationToken);

            // 请求结束前窗口可能已经关闭；此时丢弃返回结果，避免更新已释放实例的 UI 状态。
            if (match != null && !_disposeCancellationToken.IsCancellationRequested)
            {
                CurrentMatch = match;
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void Dispose()
    {
        // 保证 Dispose 幂等，避免重复取消令牌或重复释放资源。
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        // 首先取消异步操作，阻止初始化和刷新流程在窗口关闭后继续工作。
        _disposeCancellationTokenSource.Cancel();

        // StrongReferenceMessenger 会强引用接收者，必须显式注销，
        // 否则每次重新打开覆盖层创建的旧 ViewModel 都无法被垃圾回收。
        StrongReferenceMessenger.Default.Unregister<PlayerChangedMessage>(this);

        if (_refreshTimer != null)
        {
            // 停止周期刷新并解除事件订阅，切断 DispatcherQueue 对旧实例的引用链。
            _refreshTimer.Stop();
            _refreshTimer.Tick -= OnRefreshTimerTick;
            _refreshTimer = null;
        }

        // 释放取消令牌持有的内部资源；此类没有终结器，因此通知 GC 无需终结处理。
        _disposeCancellationTokenSource.Dispose();
        GC.SuppressFinalize(this);
    }

}

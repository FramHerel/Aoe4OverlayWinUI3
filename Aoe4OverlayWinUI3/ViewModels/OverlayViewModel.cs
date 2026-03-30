using System;
using System.Collections.Generic;
using System.Text;
using Aoe4OverlayWinUI3.Contracts.Services;
using Aoe4OverlayWinUI3.Core.Contracts.Services;
using Aoe4OverlayWinUI3.Core.Models;
using Aoe4OverlayWinUI3.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using Windows.Services.Maps;
using Windows.UI;

namespace Aoe4OverlayWinUI3.ViewModels;

public partial class OverlayViewModel : ObservableRecipient
{
    private readonly IAoe4ApiService _aoe4ApiService;
    private readonly ILocalSettingsService _localSettingsService;

    private DispatcherQueueTimer? _refreshTimer;
    private string? _targetProfileId;

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

        // 定时器
        SetupTimer();

        // 热切换
        StrongReferenceMessenger.Default.Register<PlayerChangedMessage>(this, (r, m) =>
        {
            _targetProfileId = m.Value.ProfileId.ToString();
            _ = RefreshDataAsync();
            _refreshTimer?.Start();
        });

        _ = InitializeOverlayAsync();
    }

    private async Task InitializeOverlayAsync()
    {
        _targetProfileId = await _localSettingsService.ReadSettingAsync<string>("SavedProfileId");

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
        _refreshTimer.Tick += async (s, e) => await RefreshDataAsync();
    }

    private async Task RefreshDataAsync()
    {
        IsLoading = true;
        try
        {
            if (string.IsNullOrEmpty(_targetProfileId))
            {
                return;
            }

            var match = await _aoe4ApiService.GetLastMatchAsync(_targetProfileId);

            if (match != null)
            {
                CurrentMatch = match;
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

}

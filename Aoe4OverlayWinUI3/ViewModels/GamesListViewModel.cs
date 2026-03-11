using System.Collections.ObjectModel;

using Aoe4OverlayWinUI3.Contracts.Services;
using Aoe4OverlayWinUI3.Contracts.ViewModels;
using Aoe4OverlayWinUI3.Core.Contracts.Services;
using Aoe4OverlayWinUI3.Core.Models;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Aoe4OverlayWinUI3.ViewModels;

public partial class GamesListViewModel : ObservableRecipient, INavigationAware
{
    private readonly IAoe4ApiService _aoe4ApiService;
    private readonly ILocalSettingsService _localSettingsService;

    public ObservableCollection<GameItemViewModel> Games { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    public ObservableCollection<SampleOrder> Source { get; } = new ObservableCollection<SampleOrder>();

    public GamesListViewModel(IAoe4ApiService aoe4ApiService)
    {
        _aoe4ApiService = aoe4ApiService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        Source.Clear();

        await LoadDataAsync();

        //// TODO: Replace with real data.
        //var data = await _sampleDataService.GetGridDataAsync();

        //foreach (var item in data)
        //{
        //    Source.Add(item);
        //}
    }

    public void OnNavigatedFrom()
    {
    }

    
    public async Task LoadDataAsync()
    {
        var profileId = await _localSettingsService.ReadSettingAsync<string>("ProfileId");
        if (string.IsNullOrEmpty(profileId))
        {
            // 添加提示：请先在设置中绑定帐户
            return;
        }

        if (IsLoading) return;
        IsLoading = true;

        try
        {
            // 调用 API 获取数据
            var matches = await _aoe4ApiService.GetMatchHistoryAsync(profileId);

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
            // 处理网络错误
            System.Diagnostics.Debug.WriteLine($"加载对局失败: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}

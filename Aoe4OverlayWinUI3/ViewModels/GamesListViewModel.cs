using System.Collections.ObjectModel;

using Aoe4OverlayWinUI3.Contracts.Services;
using Aoe4OverlayWinUI3.Contracts.ViewModels;
using Aoe4OverlayWinUI3.Core.Contracts.Services;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Aoe4OverlayWinUI3.ViewModels;

public partial class GamesListViewModel : ObservableRecipient, INavigationAware
{
    private readonly IAoe4ApiService _aoe4ApiService;

    private readonly ILocalSettingsService _localSettingsService;

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
    }

    public async void OnNavigatedTo(object parameter)
    {
        await LoadDataAsync();
    }

    public void OnNavigatedFrom()
    {
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
            // 处理网络错误
            System.Diagnostics.Debug.WriteLine($"加载对局失败: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}

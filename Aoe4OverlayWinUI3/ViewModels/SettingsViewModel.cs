using System.Reflection;
using System.Windows.Input;

using Aoe4OverlayWinUI3.Contracts.Services;
using Aoe4OverlayWinUI3.Core.Contracts.Services;
using Aoe4OverlayWinUI3.Core.Models;
using Aoe4OverlayWinUI3.Helpers;
using Aoe4OverlayWinUI3.Messages;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;

namespace Aoe4OverlayWinUI3.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    // 注入 API 服务
    private readonly IAoe4ApiService _aoe4ApiService;
    // 注入本地设置服务
    private readonly ILocalSettingsService _localSettingsService;

    // 注入主题选择服务
    private readonly IThemeSelectorService _themeSelectorService;

    // 用户输入的 ProfileId
    [ObservableProperty]
    private string _searchProfileId = string.Empty;

    // 获取到的玩家信息
    [ObservableProperty]
    private Player? _player;

    // 加载状态
    [ObservableProperty]
    private bool _isLoading;

    // 主题设置
    [ObservableProperty]
    private ElementTheme _elementTheme;

    // 版本描述
    [ObservableProperty]
    private string _versionDescription;

    public ElementTheme[] Themes { get; } = (ElementTheme[])Enum.GetValues(typeof(ElementTheme));

    public ICommand SwitchThemeCommand
    {
        get;
    }

    // 构造函数，注入服务并初始化属性
    public SettingsViewModel(IThemeSelectorService themeSelectorService,IAoe4ApiService aoe4ApiService,ILocalSettingsService localSettingsService)
    {
        _themeSelectorService = themeSelectorService;
        _elementTheme = _themeSelectorService.Theme;    //读取当前主题
        _versionDescription = GetVersionDescription();
        _aoe4ApiService = aoe4ApiService;
        _localSettingsService = localSettingsService;
        _ = LoadSavedIdAsync();

        //SwitchThemeCommand = new RelayCommand<ElementTheme>(
        //    async (param) =>
        //    {
        //        if (ElementTheme != param)
        //        {
        //            ElementTheme = param;
        //            await _themeSelectorService.SetThemeAsync(param);
        //        }
        //    });
    }

    // 监听 ElementTheme 属性变化，自动调用主题切换服务
    partial void OnElementThemeChanged(ElementTheme value)
    {
        if (App.MainWindow == null || App.MainWindow.Content == null)
        {
            return;
        }
        _ = _themeSelectorService.SetThemeAsync(value);
        //System.Diagnostics.Debug.WriteLine($"Value changed to {value}. Close the window now to test.");
    }

    // 获取版本描述，包含应用名称和版本号
    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    // 搜索玩家信息
    [RelayCommand]
    private async Task SearchPlayerAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchProfileId))
            return;

        IsLoading = true;
        try
        {
            var result = await _aoe4ApiService.GetPlayerAsync(SearchProfileId);
            if (result != null)
            {
                Player = result;
                // 将 ProfileId 保存到本地设置，以便下次启动时自动加载
                await _localSettingsService.SaveSettingAsync("SavedProfileId", SearchProfileId);

                StrongReferenceMessenger.Default.Send(new PlayerChangedMessage(Player));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Search failed: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // 加载保存的 ProfileId，并自动搜索更新玩家信息
    private async Task LoadSavedIdAsync()
    {
        var savedId = await _localSettingsService.ReadSettingAsync<string>("SavedProfileId");
        if (!string.IsNullOrEmpty(savedId))
        {
            // 如果当前玩家信息已经是保存的 ID，则不需要再次搜索
            if (Player != null && Player.ProfileId.ToString() == savedId)
            {
                SearchProfileId = savedId;
                return;
            }
            SearchProfileId = savedId;
            await SearchPlayerAsync(); // 自动跑一遍搜索，显示玩家名字
        }
    }


}

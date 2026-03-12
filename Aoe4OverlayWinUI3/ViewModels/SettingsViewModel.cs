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

    // 用户输入的查询内容
    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    // 获取到的玩家信息
    [ObservableProperty]
    public partial Player? Player
    {
        get;
        set;
    }

    // 加载状态
    [ObservableProperty]
    public partial bool IsLoading
    {
        get;
        set;
    }

    // 主题设置
    [ObservableProperty]
    public partial ElementTheme ElementTheme
    {
        get;
        set;
    }

    // 版本描述
    [ObservableProperty]
    public partial string VersionDescription
    {
        get;
        set;
    }
    public ElementTheme[] Themes { get; } = (ElementTheme[])Enum.GetValues(typeof(ElementTheme));

    public ICommand SwitchThemeCommand
    {
        get;
    }

    // 构造函数，注入服务并初始化属性
    public SettingsViewModel(IThemeSelectorService themeSelectorService, IAoe4ApiService aoe4ApiService, ILocalSettingsService localSettingsService)
    {
        _themeSelectorService = themeSelectorService;
        ElementTheme = _themeSelectorService.Theme;    //读取当前主题
        VersionDescription = GetVersionDescription();
        _aoe4ApiService = aoe4ApiService;
        _localSettingsService = localSettingsService;
        _ = LoadSavedIdAsync();
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
        //// 返回本地化的应用名称和版本号
        //return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

        // 直接返回版本号
        return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

    }


    // 搜索玩家信息
    [RelayCommand]
    private async Task SearchPlayerAsync()
    {
        // 去除输入两端的空白字符
        var query = SearchQuery?.Trim();

        // 如果输入为空或仅包含空白字符，则不执行搜索
        if (string.IsNullOrWhiteSpace(query))
        {
            return;
        }

        IsLoading = true;
        try
        {
            var result = await _aoe4ApiService.GetPlayerAsync(query);
            if (result != null)
            {
                Player = result;
                // 将 ProfileId 保存到本地设置，以便下次启动时自动加载
                await _localSettingsService.SaveSettingAsync("SavedProfileId", Player.ProfileId.ToString());

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

    // TODO: 增加一个清除按钮，清除当前玩家信息和保存的 ProfileId
    // TODO: 对查询 API 速率加以限制
    // TODO: 增加错误提示，例如输入无效 ID 或网络错误时显示消息
    // TODO: 覆盖层

    // 加载保存的 ProfileId，并自动搜索更新玩家信息
    private async Task LoadSavedIdAsync()
    {
        var savedId = await _localSettingsService.ReadSettingAsync<string>("SavedProfileId");
        if (!string.IsNullOrEmpty(savedId))
        {
            // 如果当前玩家信息已经是保存的 ID，则不需要再次搜索
            if (Player != null && Player.ProfileId.ToString() == savedId)
            {
                SearchQuery = savedId;
                return;
            }
            SearchQuery = savedId;
            await SearchPlayerAsync(); // 自动跑一遍搜索，显示玩家名字
        }
    }


}

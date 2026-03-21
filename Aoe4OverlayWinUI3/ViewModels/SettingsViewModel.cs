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
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;

using Windows.UI;

using Color = Windows.UI.Color;
using Colors = Microsoft.UI.Colors;

namespace Aoe4OverlayWinUI3.ViewModels;

public partial class SettingsViewModel : ObservableRecipient, IRecipient<OverlayStatusChangedMessage>
{
    // 注入 API 服务
    private readonly IAoe4ApiService _aoe4ApiService;
    // 注入本地设置服务
    private readonly ILocalSettingsService _localSettingsService;
    // 注入主题选择服务
    private readonly IThemeSelectorService _themeSelectorService;
    // 注入覆盖层服务
    private readonly IOverlayService _overlayService;

    // Hotkey 文本
    [ObservableProperty]
    public partial string HotkeyText { get; set; } = "Ctrl + F12";

    [ObservableProperty]
    public partial bool IsListeningForHotkey { get; set; } = false;

    // 背板设置
    [ObservableProperty]
    public partial int SelectedBackdropIndex { get; set; } = 0;

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

    // 覆盖层显示状态
    [ObservableProperty]
    public partial bool IsOverlayEnabled
    {
        get;
        set;
    }

    // 覆盖层编辑状态
    [ObservableProperty]
    public partial bool IsOverlayEditable
    {
        get;
        set;
    }




    // 可用的主题列表
    public ElementTheme[] Themes { get; } = Enum.GetValues<ElementTheme>();

    //  切换主题的命令
    public ICommand SwitchThemeCommand
    {
        get;
    }

    // 构造函数，注入服务并初始化属性
    public SettingsViewModel(IThemeSelectorService themeSelectorService, IAoe4ApiService aoe4ApiService, ILocalSettingsService localSettingsService, IOverlayService overlayService)
    {
        _themeSelectorService = themeSelectorService;
        ElementTheme = _themeSelectorService.Theme;
        VersionDescription = GetVersionDescription();

        _overlayService = overlayService;
        HotkeyText = _overlayService.CurrentHotkeyText;
        Task.Run(async () =>
        {
            var realText = await _overlayService.GetSavedHotkeyTextAsync();
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                HotkeyText = realText;
            });
        });
        IsOverlayEnabled = false;
        WeakReferenceMessenger.Default.Register(this);

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

    public void Receive(OverlayStatusChangedMessage message)
    {
        var dispatcher = DispatcherQueue.GetForCurrentThread();

        dispatcher?.TryEnqueue(() =>
        {
            // 当收到的状态和当前 UI 状态不一致时才赋值
            if (IsOverlayEnabled != message.IsVisible)
            {
                IsOverlayEnabled = message.IsVisible;
            }
        });
    }

    partial void OnIsOverlayEditableChanged(bool value)
    {
        _overlayService.SetOverlayEditMode(value);
    }


    // 
    partial void OnIsOverlayEnabledChanged(bool value)
    {
        _overlayService.ToggleOverlay(value);

        if (!value)
        {
            IsOverlayEditable = false;
        }

        // TODO: 加上保存配置到 LocalSettings 的代码
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
                SearchQuery = savedId;
                return;
            }
            SearchQuery = savedId;
            await SearchPlayerAsync(); // 自动跑一遍搜索，显示玩家名字
        }
    }

    // 背板设置变化时通知 Service 更新覆盖层
    partial void OnSelectedBackdropIndexChanged(int value)
    {
        _overlayService.UpdateBackdrop(value);
    }


    // TODO: 添加翻译
    [RelayCommand]
    private void StartListenHotkey()
    {
        IsListeningForHotkey = true;
        HotkeyText = "Waiting press...";
        _overlayService.UnregisterHotkey("ToggleOverlay");
    }
}

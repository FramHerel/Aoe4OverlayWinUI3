using Aoe4OverlayWinUI3.Contracts.Services;
using Aoe4OverlayWinUI3.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.ComponentModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Core;
using DispatcherQueueTimer = Microsoft.UI.Dispatching.DispatcherQueueTimer;

namespace Aoe4OverlayWinUI3.Views;

public sealed partial class SettingsPage : Page
{
    // 进入录制后超过该时间没有有效结果就结束录制
    private static readonly TimeSpan HotkeyRecordingTimeout = TimeSpan.FromSeconds(10);

    // 松开所有按键后的宽限期，期间再次按键则继续录制
    private static readonly TimeSpan HotkeyReleaseGracePeriod = TimeSpan.FromMilliseconds(600);

    private readonly IOverlayService _overlayService;
    private readonly DispatcherQueueTimer _hotkeyTimeoutTimer;
    private readonly DispatcherQueueTimer _hotkeyReleaseTimer;

    // 本次录制是否收到过按键按下、是否收到过非修饰键按下
    private bool _hotkeyAnyKeyDown;
    private bool _hotkeyNonModifierKeyDown;

    public SettingsViewModel ViewModel
    {
        get;
    }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        _overlayService = App.GetService<IOverlayService>();
        InitializeComponent();

        _hotkeyTimeoutTimer = DispatcherQueue.CreateTimer();
        _hotkeyTimeoutTimer.Interval = HotkeyRecordingTimeout;
        _hotkeyTimeoutTimer.IsRepeating = false;
        _hotkeyTimeoutTimer.Tick += (_, _) => _ = EndHotkeyRecordingAsync(SettingsViewModel.HotkeyErrorReason.NoKey);

        _hotkeyReleaseTimer = DispatcherQueue.CreateTimer();
        _hotkeyReleaseTimer.Interval = HotkeyReleaseGracePeriod;
        _hotkeyReleaseTimer.IsRepeating = false;
        _hotkeyReleaseTimer.Tick += (_, _) => _ = EndHotkeyRecordingAsync(SettingsViewModel.HotkeyErrorReason.NoKey);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        App.MainWindow.Activated += OnMainWindowActivated;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        // 离开设置页时结束录制，避免热键一直停留在注销状态
        _ = EndHotkeyRecordingAsync(SettingsViewModel.HotkeyErrorReason.NoKey);
        App.MainWindow.Activated -= OnMainWindowActivated;
        ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        base.OnNavigatedFrom(e);
    }

    // 录制状态开始/结束时同步计时器
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SettingsViewModel.IsListeningForHotkey))
        {
            return;
        }

        if (!ViewModel.IsListeningForHotkey)
        {
            StopHotkeyTimers();
            return;
        }

        _hotkeyAnyKeyDown = false;
        _hotkeyNonModifierKeyDown = false;
        _hotkeyReleaseTimer.Stop();
        _hotkeyTimeoutTimer.Stop();
        _hotkeyTimeoutTimer.Start();
    }

    // 窗口失焦（例如系统截图工具抢走焦点）时结束录制
    private void OnMainWindowActivated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs args)
    {
        if (args.WindowActivationState == WindowActivationState.Deactivated)
        {
            _ = EndHotkeyRecordingAsync(SettingsViewModel.HotkeyErrorReason.NoKey);
        }
    }

    private async void OnHotkeyPreviewKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (!ViewModel.IsListeningForHotkey)
        {
            return;
        }

        {
            e.Handled = true;
            _hotkeyAnyKeyDown = true;
            // 有新按键，取消“松开所有键”的宽限结束
            _hotkeyReleaseTimer.Stop();

            var key = e.OriginalKey;
            if (key == VirtualKey.Escape)
            {
                ViewModel.IsListeningForHotkey = false;
                ViewModel.ClearHotkeyError();
                await _overlayService.CancelHotkeyUpdate();
                ViewModel.HotkeyText = await _overlayService.GetSavedHotkeyTextAsync();
                return;
            }

            var modifiers = GetCurrentModifiers();

            if (IsModifierKey(key))
            {
                // 按下修饰键，更新 UI 显示，不结束录制
                ViewModel.HotkeyText = _overlayService.GetHotkeyDisplay(VirtualKey.None, modifiers) + " + ...";
                e.Handled = true;
                return;
            }

            _hotkeyNonModifierKeyDown = true;

            bool isFunctionKey = key >= VirtualKey.F1 && key <= VirtualKey.F12;
            if (modifiers == VirtualKeyModifiers.None && !isFunctionKey)
            {
                ViewModel.HotkeyText = "!Retry!";
                return;
            }

            await ApplyHotkeyAsync(key, modifiers);
        }
    }

    private void OnHotkeyPreviewKeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (!ViewModel.IsListeningForHotkey)
        {
            return;
        }

        e.Handled = true;

        if (AnyModifierKeyDown())
        {
            _hotkeyReleaseTimer.Stop();
            return;
        }

        // 只按过修饰键（例如 Win+Shift+S 的 S 被系统截获）时，松开所有键后按“未捕获到有效按键”结束
        if (_hotkeyAnyKeyDown && !_hotkeyNonModifierKeyDown)
        {
            _hotkeyReleaseTimer.Start();
        }
    }

    // 绑定快捷键：成功后保存，失败时保留上一个可用热键
    private async Task ApplyHotkeyAsync(VirtualKey key, VirtualKeyModifiers modifiers)
    {
        var success = _overlayService.UpdateHotkey(key, modifiers);

        ViewModel.IsListeningForHotkey = false;
        // 失败时 CurrentHotkeyText 仍是上一个可用的热键
        ViewModel.HotkeyText = _overlayService.CurrentHotkeyText;

        if (success)
        {
            ViewModel.ClearHotkeyError();
            return;
        }

        // 含 Win 的组合与单独的 F12 由系统保留，其余按被其它程序占用处理
        var isReservedBySystem = modifiers.HasFlag(VirtualKeyModifiers.Windows)
            || (key == VirtualKey.F12 && modifiers == VirtualKeyModifiers.None);
        ViewModel.ShowHotkeyError(isReservedBySystem
            ? SettingsViewModel.HotkeyErrorReason.Reserved
            : SettingsViewModel.HotkeyErrorReason.InUse);
    }

    // 结束录制并恢复上一个可用热键（例如超时、失焦、只按了修饰键）
    private async Task EndHotkeyRecordingAsync(SettingsViewModel.HotkeyErrorReason reason)
    {
        if (!ViewModel.IsListeningForHotkey)
        {
            return;
        }

        ViewModel.IsListeningForHotkey = false;
        await _overlayService.CancelHotkeyUpdate();
        ViewModel.HotkeyText = await _overlayService.GetSavedHotkeyTextAsync();
        ViewModel.ShowHotkeyError(reason);
    }

    private void StopHotkeyTimers()
    {
        _hotkeyTimeoutTimer.Stop();
        _hotkeyReleaseTimer.Stop();
    }

    private static VirtualKeyModifiers GetCurrentModifiers()
    {
        var modifiers = VirtualKeyModifiers.None;
        var keyboard = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread;
        if (keyboard(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down)) modifiers |= VirtualKeyModifiers.Control;
        if (keyboard(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down)) modifiers |= VirtualKeyModifiers.Shift;
        if (keyboard(VirtualKey.Menu).HasFlag(CoreVirtualKeyStates.Down)) modifiers |= VirtualKeyModifiers.Menu; // Alt
        if (keyboard(VirtualKey.LeftWindows).HasFlag(CoreVirtualKeyStates.Down) ||
            keyboard(VirtualKey.RightWindows).HasFlag(CoreVirtualKeyStates.Down)) modifiers |= VirtualKeyModifiers.Windows;
        return modifiers;
    }

    private static bool AnyModifierKeyDown()
    {
        var keyboard = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread;
        return keyboard(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down)
            || keyboard(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down)
            || keyboard(VirtualKey.Menu).HasFlag(CoreVirtualKeyStates.Down) // Alt
            || keyboard(VirtualKey.LeftWindows).HasFlag(CoreVirtualKeyStates.Down)
            || keyboard(VirtualKey.RightWindows).HasFlag(CoreVirtualKeyStates.Down);
    }

    private static bool IsModifierKey(VirtualKey key)
    {
        return key == VirtualKey.Control ||
               key == VirtualKey.Shift ||
               key == VirtualKey.Menu || // Alt
               key == VirtualKey.LeftWindows ||
               key == VirtualKey.RightWindows;
    }

    private void ToCloneRepoCard_Click(object sender, RoutedEventArgs e)
    {
        DataPackage package = new DataPackage();
        package.SetText(gitCloneTextBlock.Text);
        Clipboard.SetContent(package);
    }

    private async void BugRequestCard_Click(object sender, RoutedEventArgs e)
    {
        await Launcher.LaunchUriAsync(new Uri("https://github.com/FramHerel/Aoe4OverlayWinUI3/issues/new/choose"));

    }
}

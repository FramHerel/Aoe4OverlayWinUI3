using Aoe4OverlayWinUI3.Contracts.Services;
using Aoe4OverlayWinUI3.Services;
using Aoe4OverlayWinUI3.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Core;

namespace Aoe4OverlayWinUI3.Views;

public sealed partial class SettingsPage : Page
{
    private readonly IOverlayService _overlayService;
    public SettingsViewModel ViewModel
    {
        get;
    }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        _overlayService = App.GetService<IOverlayService>();
        InitializeComponent();
    }

    private async void OnHotkeyKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (!ViewModel.IsListeningForHotkey)
        {
            return;
        }

        {
            e.Handled = true;
            var key = e.OriginalKey;
            if (key == VirtualKey.Escape)
            {
                ViewModel.IsListeningForHotkey = false;
                _overlayService.CancelHotkeyUpdate();
                ViewModel.HotkeyText = await _overlayService.GetSavedHotkeyTextAsync();
                return;
            }

            var modifiers = VirtualKeyModifiers.None;
            var keyboard = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread;
            if (keyboard(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down)) modifiers |= VirtualKeyModifiers.Control;
            if (keyboard(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down)) modifiers |= VirtualKeyModifiers.Shift;
            if (keyboard(VirtualKey.Menu).HasFlag(CoreVirtualKeyStates.Down)) modifiers |= VirtualKeyModifiers.Menu; // Alt
            if (keyboard(VirtualKey.LeftWindows).HasFlag(CoreVirtualKeyStates.Down) ||
                keyboard(VirtualKey.RightWindows).HasFlag(CoreVirtualKeyStates.Down)) modifiers |= VirtualKeyModifiers.Windows;
            bool isModifierOnly = key == VirtualKey.Control ||
                                 key == VirtualKey.Shift ||
                                 key == VirtualKey.Menu || // Alt
                                 key == VirtualKey.LeftWindows ||
                                 key == VirtualKey.RightWindows;

            if (isModifierOnly)
            {
                // 按下修饰键，更新 UI 显示，不结束录制
                ViewModel.HotkeyText = _overlayService.GetHotkeyDisplay(VirtualKey.None, modifiers) + " + ...";
                e.Handled = true;
                return;
            }

            bool isFunctionKey = key >= VirtualKey.F1 && key <= VirtualKey.F12;
            if (modifiers == VirtualKeyModifiers.None && !isFunctionKey)
            {
                ViewModel.HotkeyText = "!Retry!";
                return;
            }

            // 绑定
            _overlayService.UpdateHotkey(key, modifiers);

            ViewModel.HotkeyText = _overlayService.GetHotkeyDisplay(key, modifiers);

            ViewModel.IsListeningForHotkey = false;
        }
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

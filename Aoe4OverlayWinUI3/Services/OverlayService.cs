using System.Diagnostics;
using Aoe4OverlayWinUI3.Contracts.Services;
using Aoe4OverlayWinUI3.Messages;
using Aoe4OverlayWinUI3.Views;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media;
using NHotkey;
using NHotkey.WinUI;
using Windows.System;

namespace Aoe4OverlayWinUI3.Services;

public class OverlayService : IOverlayService
{
    private OverlayWindow _overlayWindow;

    // 切换覆盖层显示状态的方法
    public OverlayService()
    {
        RegisterHotkey("Hotkey", VirtualKey.F12, VirtualKeyModifiers.Control);
    }
    public void ToggleOverlay(bool enable)
    {
        if (enable)
        {
            if (_overlayWindow == null)
            {
                _overlayWindow = new OverlayWindow();

                // --- 窗口置顶 ---
                _overlayWindow.SetIsAlwaysOnTop(true);

                // --- 任务栏控制 ---
                // 不在任务栏显示，也不出现在 Alt+Tab 切换器中
                _overlayWindow.AppWindow.IsShownInSwitchers = false;
                SetOverlayEditMode(false);

                // --- 鼠标穿透 (关键功能) ---
                // _overlayWindow.SetIsClickThrough(true); 
            }

            // --- 显示窗口 (不夺取焦点) ---
            _overlayWindow.Show();
        }
        else
        {
            // 隐藏窗口
            _overlayWindow?.Hide();
        }
    }

    // 更新背板风格的方法
    public void UpdateBackdrop(int value)
    {
        if (_overlayWindow == null) return;

        _overlayWindow.SystemBackdrop = value switch
        {
            0 => new TransparentTintBackdrop(),
            1 => new MicaBackdrop(),
            2 => new DesktopAcrylicBackdrop(),
            _ => null
        };

    }

    // 设置 Overlay 的 EditMode
    public void SetOverlayEditMode(bool isEditing)
    {
        if (_overlayWindow == null) return;
        if (isEditing)
        {
            // 切换鼠标穿透状态
            //_overlayWindow.SetIsClickThrough(false);
            // 显示边框和标题栏
            if (_overlayWindow.AppWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsResizable = true;
            }
        }
        else
        {
            // 恢复鼠标穿透状态
            if (_overlayWindow.AppWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsResizable = false;
            }
            //_overlayWindow.SetIsClickThrough(true);
        }
    }

    public void SetIsClickThrough(bool isClickThrough)
    {
        if (_overlayWindow == null) return;
        //_overlayWindow.SetIsClickThrough(isClickThrough);
    }

    // 注册快捷键
    public void RegisterHotkey(string name, VirtualKey key, VirtualKeyModifiers modifiers)
    {
        try
        {
            HotkeyManager.Current.AddOrReplace(name, key, modifiers, OnHotkeyInvoked);
        }
        catch (NHotkey.HotkeyAlreadyRegisteredException)
        {
            Debug.WriteLine($"Hotkey {key} has been used!");
        }

    }

    public void UnregisterHotkey(string name)
    {
        HotkeyManager.Current.Remove(name);
    }

    private void OnHotkeyInvoked(object? sender, HotkeyEventArgs e)
    {
        bool newStatus = !(_overlayWindow?.Visible ?? false);
        ToggleOverlay(newStatus);
        WeakReferenceMessenger.Default.Send(new OverlayStatusChangedMessage(newStatus));
        e.Handled = true;
    }
    public void ShutDown()
    {
        // 注销所有热键，防止内存泄漏或系统钩子残留
        UnregisterHotkey("Hotkey");

        // 彻底销毁窗口
        if (_overlayWindow != null)
        {
            _overlayWindow.Close();
            _overlayWindow = null;
        }
    }

}

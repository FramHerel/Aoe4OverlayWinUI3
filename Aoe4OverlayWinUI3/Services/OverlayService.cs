using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Aoe4OverlayWinUI3.Contracts.Services;
using Aoe4OverlayWinUI3.Views;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using NHotkey;
using NHotkey.WinUI;
using Windows.System;
using Windows.UI;
using WinRT.Interop;
using WinUIEx;

namespace Aoe4OverlayWinUI3.Services;

public class OverlayService : IOverlayService
{
    private OverlayWindow _overlayWindow;

    // 切换覆盖层显示状态的方法
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
                _overlayWindow.AppWindow.IsShownInSwitchers=false;

                // --- 移除系统装饰 ---
                //if (_overlayWindow.AppWindow.Presenter is OverlappedPresenter presenter)
                //{
                //    presenter.SetBorderAndTitleBar(false, false);
                //}

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
        HotkeyManager.Current.AddOrReplace(name, key, modifiers, OnHotkeyInvoked);
    }

    public void UnregisterHotkey(string name)
    {
        HotkeyManager.Current.Remove(name);
    }

    private void OnHotkeyInvoked(object sender, HotkeyEventArgs e)
    {
        if (_overlayWindow.Visible)
            _overlayWindow.Hide();
        else
            _overlayWindow.Show();
        e.Handled = true;
    }

}

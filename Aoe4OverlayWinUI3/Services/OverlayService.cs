using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT.Interop;
using WinUIEx;
using Aoe4OverlayWinUI3.Views;
using Aoe4OverlayWinUI3.Contracts.Services;

namespace Aoe4OverlayWinUI3.Services;

public class OverlayService:IOverlayService
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
                if (_overlayWindow.AppWindow.Presenter is OverlappedPresenter presenter)
                {
                    presenter.SetBorderAndTitleBar(false, false);
                }

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

}

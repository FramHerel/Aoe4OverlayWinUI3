using System.Diagnostics;
using Aoe4OverlayWinUI3.Contracts.Services;
using Aoe4OverlayWinUI3.Core.Models;
using Aoe4OverlayWinUI3.Messages;
using Aoe4OverlayWinUI3.Views;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media;
using NHotkey;
using NHotkey.WinUI;
using Windows.System;
using System.Timers;

namespace Aoe4OverlayWinUI3.Services;

public class OverlayService : IOverlayService
{
    private OverlayWindow _overlayWindow;

    private System.Timers.Timer? _saveConfigTimer;

    private readonly ILocalSettingsService _localSettingsService;

    // 切换覆盖层显示状态的方法
    public OverlayService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
        RegisterHotkey("Hotkey", VirtualKey.F12, VirtualKeyModifiers.Control);
    }
    public async Task ToggleOverlay(bool enable)
    {
        if (enable)
        {
            if (_overlayWindow == null)
            {
                _overlayWindow = new OverlayWindow();

                // --- 保存 ---
                _overlayWindow.AppWindow.Changed += (s, e) =>
                {
                    if (e.DidPositionChange || e.DidSizeChange)
                    {
                        RestartSaveTimer(s);
                    }
                };

                // --- 窗口置顶 ---
                _overlayWindow.SetIsAlwaysOnTop(true);

                // --- 任务栏控制 ---
                _overlayWindow.AppWindow.IsShownInSwitchers = false;

                // --- 鼠标穿透 ---
                // _overlayWindow.SetIsClickThrough(true); 

                await RestoreWindowPositionAsync();

                SetOverlayEditMode(false);

            }

            // --- 显示窗口 ---
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
        _saveConfigTimer?.Stop();
        _saveConfigTimer?.Dispose();

        // 彻底销毁窗口
        if (_overlayWindow != null)
        {
            _overlayWindow.Close();
            _overlayWindow = null;
        }
    }

    private async Task RestoreWindowPositionAsync()
    {
        if (_overlayWindow == null) return;
        var savedRect = await _localSettingsService.ReadSettingAsync<OverlayRect?>("OverlayWindowRect");

        if (savedRect.HasValue)
        {
            var rect = savedRect.Value;
            // 应用保存的坐标和大小
            _overlayWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(rect.X, rect.Y, rect.Width, rect.Height));
        }
        else
        {
            // 设置默认位置
            _overlayWindow.CenterOnScreen();
        }
    }
    private void RestartSaveTimer(AppWindow appWindow)
    {
        // 停止并销毁旧计时器
        _saveConfigTimer?.Stop();
        _saveConfigTimer?.Dispose();

        // 500ms 后触发执行保存
        _saveConfigTimer = new System.Timers.Timer(500)
        {
            AutoReset = false
        };
        _saveConfigTimer.Elapsed += async (s, e) =>
        {
            var rect = new OverlayRect
            {
                X = appWindow.Position.X,
                Y = appWindow.Position.Y,
                Width = appWindow.Size.Width,
                Height = appWindow.Size.Height
            };

            await _localSettingsService.SaveSettingAsync("OverlayWindowRect", rect);
            Debug.WriteLine($"Overlay 位置已保存: {rect.X}, {rect.Y}");
        };
        _saveConfigTimer.Start();
    }

}

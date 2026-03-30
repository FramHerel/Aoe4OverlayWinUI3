using System.Diagnostics;
using Aoe4OverlayWinUI3.Contracts.Services;
using Aoe4OverlayWinUI3.Core.Models;
using Aoe4OverlayWinUI3.Messages;
using Aoe4OverlayWinUI3.Views;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using NHotkey;
using NHotkey.WinUI;
using Windows.System;

namespace Aoe4OverlayWinUI3.Services;

public class OverlayService : IOverlayService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SemaphoreSlim _saveLock = new(1, 1);
    private OverlayWindow _overlayWindow;

    private System.Timers.Timer? _saveConfigTimer;

    public string CurrentHotkeyText { get; private set; } = "Ctrl + F12";

    private readonly ILocalSettingsService _localSettingsService;

    // 切换覆盖层显示状态的方法
    public OverlayService(ILocalSettingsService localSettingsService, IServiceProvider serviceProvider)
    {
        _localSettingsService = localSettingsService;
        _serviceProvider = serviceProvider;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // 读取保存的键位，如果没有则使用默认 Ctrl + F12
            var savedKey = await _localSettingsService.ReadSettingAsync<int?>("Hotkey_Key") ?? (int)VirtualKey.F12;
            var savedMod = await _localSettingsService.ReadSettingAsync<int?>("Hotkey_Modifiers") ?? (int)VirtualKey.Control;
            CurrentHotkeyText = GetHotkeyDisplay((VirtualKey)savedKey, (VirtualKeyModifiers)savedMod);

            RegisterHotkey("ToggleOverlay", (VirtualKey)savedKey, (VirtualKeyModifiers)savedMod);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to initialize OverlayService: {ex}");
        }
    }
    public async Task ToggleOverlay(bool enable)
    {
        if (enable)
        {
            if (_overlayWindow != null)
            {
                return;
            }

            // 实例
            _overlayWindow = _serviceProvider.GetRequiredService<OverlayWindow>();

            // --- 保存位置 ---
            _overlayWindow.AppWindow.Changed += OnAppWindowChanged;

            // 关闭出口
            _overlayWindow.Closed += OnOverlayWindowClosed;


            // --- 窗口置顶 ---
            _overlayWindow.SetIsAlwaysOnTop(true);

            // --- 任务栏控制 ---
            _overlayWindow.AppWindow.IsShownInSwitchers = false;

            // --- TODO: 鼠标穿透 ---
            // _overlayWindow.SetIsClickThrough(true); 

            // 恢复位置
            await RestoreWindowPositionAsync();

            SetOverlayEditMode(false);

            // --- 显示窗口 ---
            _overlayWindow.Show();
        }
        else
        {
            // 隐藏窗口
            _overlayWindow?.Close();
        }
    }
    private void OnOverlayWindowClosed(object sender, WindowEventArgs args)
    {
        // 清理引用
        if (_overlayWindow != null)
        {
            _overlayWindow.AppWindow.Changed -= OnAppWindowChanged;
            _overlayWindow.Closed -= OnOverlayWindowClosed;
            _overlayWindow = null;
        }

        // 同步 UI 状态
        WeakReferenceMessenger.Default.Send(new OverlayStatusChangedMessage(false));
        Debug.WriteLine("[OverlayService] Window closed by user, status synced.");
    }
    private void OnAppWindowChanged(AppWindow sender, AppWindowChangedEventArgs args)
    {
        if (args.DidPositionChange || args.DidSizeChange)
        {
            RestartSaveTimer(sender);
        }
    }

    // 更新背板风格的方法
    public void UpdateBackdrop(int value)
    {
        if (_overlayWindow == null)
        {
            return;
        }

        // #80000000 对应：A=128, R=0, G=0, B=0
        var currentColor = Microsoft.UI.ColorHelper.FromArgb(128, 0, 0, 0);

        _overlayWindow.SystemBackdrop = value switch
        {
            0 => new TransparentTintBackdrop { TintColor = currentColor },
            1 => new MicaBackdrop(),
            2 => new DesktopAcrylicBackdrop(),
            _ => null
        };

    }

    // 设置 Overlay 的 EditMode
    public void SetOverlayEditMode(bool isEditing)
    {
        if (_overlayWindow == null)
        {
            return;
        }

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

    // TODO: 添加鼠标点击可穿越功能
    public void SetIsClickThrough(bool isClickThrough)
    {
        if (_overlayWindow == null)
        {
            return;
        }
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
        catch (Exception ex)
        {
            Debug.WriteLine($"Hotkey binding failed: {ex.Message}");
        }

    }

    public void UnregisterHotkey(string name)
    {
        HotkeyManager.Current.Remove(name);
    }

    private async void OnHotkeyInvoked(object? sender, HotkeyEventArgs e)
    {
        try
        {
            bool newStatus = !(_overlayWindow?.Visible ?? false);
            await ToggleOverlay(newStatus);
            WeakReferenceMessenger.Default.Send(new OverlayStatusChangedMessage(newStatus));
            e.Handled = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Hotkey handler error: {ex}");
        }
    }
    public void ShutDown()
    {
        // 注销所有热键，防止内存泄漏或系统钩子残留
        UnregisterHotkey("ToggleOverlay");

        if (_overlayWindow != null)
        {
            _overlayWindow.AppWindow.Changed -= OnAppWindowChanged; // 注销事件
            _overlayWindow.Close(); // Close 会触发释放逻辑
            _overlayWindow = null;
        }

        _saveConfigTimer?.Stop();
        _saveConfigTimer?.Dispose();

    }

    private async Task RestoreWindowPositionAsync()
    {
        if (_overlayWindow == null)
        {
            return;
        }

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
            await _saveLock.WaitAsync();
            try
            {

                var rect = new OverlayRect
                {
                    X = appWindow.Position.X,
                    Y = appWindow.Position.Y,
                    Width = appWindow.Size.Width,
                    Height = appWindow.Size.Height
                };

                await _localSettingsService.SaveSettingAsync("OverlayWindowRect", rect);
                Debug.WriteLine($"Overlay position saved: {rect.X}, {rect.Y}");
            }
            finally
            {
                _saveLock.Release();
            }
        };
        _saveConfigTimer.Start();
    }

    public void UpdateHotkey(VirtualKey key, VirtualKeyModifiers modifiers)
    {
        RegisterHotkey("ToggleOverlay", key, modifiers);
        _ = _localSettingsService.SaveSettingAsync("Hotkey_Key", (int)key);
        _ = _localSettingsService.SaveSettingAsync("Hotkey_Modifiers", (int)modifiers);
        CurrentHotkeyText = GetHotkeyDisplay(key, modifiers);
    }
    public string GetHotkeyDisplay(VirtualKey key, VirtualKeyModifiers modifiers)
    {
        var parts = new List<string>();

        // 修饰键
        if (modifiers.HasFlag(VirtualKeyModifiers.Control)) parts.Add("Ctrl");
        if (modifiers.HasFlag(VirtualKeyModifiers.Shift)) parts.Add("Shift");
        if (modifiers.HasFlag(VirtualKeyModifiers.Menu)) parts.Add("Alt");
        if (modifiers.HasFlag(VirtualKeyModifiers.Windows)) parts.Add("Win");

        // 主键（如果是 None 则跳过）
        if (key != VirtualKey.None)
        {
            parts.Add(key.ToString());
        }

        return parts.Count > 0 ? string.Join(" + ", parts) : " ";
    }

    public async Task<string> GetSavedHotkeyTextAsync()
    {
        var savedKey = await _localSettingsService.ReadSettingAsync<int?>("Hotkey_Key")
                       ?? (int)VirtualKey.F12;

        var savedMod = await _localSettingsService.ReadSettingAsync<int?>("Hotkey_Modifiers")
                       ?? (int)VirtualKey.Control;

        return GetHotkeyDisplay((VirtualKey)savedKey, (VirtualKeyModifiers)savedMod);
    }

    public void CancelHotkeyUpdate()
    {
        InitializeAsync();
    }

}

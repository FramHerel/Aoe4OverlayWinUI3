using Windows.System;

namespace Aoe4OverlayWinUI3.Contracts.Services;

public interface IOverlayService
{
    string CurrentHotkeyText
    {
        get;
    }

    void SetOverlayEditMode(bool isEditing);
    Task ToggleOverlay(bool enable);
    void UpdateBackdrop(int value);
    void RegisterHotkey(string name, VirtualKey key, VirtualKeyModifiers modifiers);
    void UnregisterHotkey(string name);
    void ShutDown();
    void UpdateHotkey(VirtualKey key, VirtualKeyModifiers modifiers);
    string GetHotkeyDisplay(VirtualKey key, VirtualKeyModifiers modifiers);
    Task<string> GetSavedHotkeyTextAsync();
    void CancelHotkeyUpdate();
}

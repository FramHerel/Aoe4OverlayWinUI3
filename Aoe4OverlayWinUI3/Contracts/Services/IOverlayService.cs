using System;
using System.Collections.Generic;
using System.Text;
using Windows.System;
using Windows.UI;

namespace Aoe4OverlayWinUI3.Contracts.Services;

public interface IOverlayService
{
    void SetOverlayEditMode(bool isEditing);
    void ToggleOverlay(bool enable);
    void UpdateBackdrop(int value);
    void RegisterHotkey(string name, VirtualKey key, VirtualKeyModifiers modifiers);
    void UnregisterHotkey(string name);
}

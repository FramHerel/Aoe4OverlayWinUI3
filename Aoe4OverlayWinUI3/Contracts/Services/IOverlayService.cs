using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;

namespace Aoe4OverlayWinUI3.Contracts.Services;

public interface IOverlayService
{
    void ToggleOverlay(bool enable);
    void UpdateBackdrop(int value);
}

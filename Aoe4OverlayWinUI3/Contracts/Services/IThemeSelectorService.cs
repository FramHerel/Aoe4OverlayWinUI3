using Microsoft.UI.Xaml;

namespace Aoe4OverlayWinUI3.Contracts.Services;

public interface IThemeSelectorService
{
    event EventHandler<ElementTheme>? ThemeChanged;

    ElementTheme Theme
    {
        get;
    }

    Task InitializeAsync();

    Task SetThemeAsync(ElementTheme theme);

    Task SetRequestedThemeAsync();
}

using System.Reflection;
using System.Windows.Input;

using Aoe4OverlayWinUI3.Contracts.Services;
using Aoe4OverlayWinUI3.Helpers;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;

namespace Aoe4OverlayWinUI3.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;

    [ObservableProperty]
    private ElementTheme _elementTheme;

    [ObservableProperty]
    private string _versionDescription;

    public ElementTheme[] Themes { get; } = (ElementTheme[])Enum.GetValues(typeof(ElementTheme));

    public ICommand SwitchThemeCommand
    {
        get;
    }

    public SettingsViewModel(IThemeSelectorService themeSelectorService)
    {
        _themeSelectorService = themeSelectorService;
        _elementTheme = _themeSelectorService.Theme;    //读取当前主题
        _versionDescription = GetVersionDescription();

        //SwitchThemeCommand = new RelayCommand<ElementTheme>(
        //    async (param) =>
        //    {
        //        if (ElementTheme != param)
        //        {
        //            ElementTheme = param;
        //            await _themeSelectorService.SetThemeAsync(param);
        //        }
        //    });
    }

    partial void OnElementThemeChanged(ElementTheme value)
    {
        if (App.MainWindow == null || App.MainWindow.Content == null)
        {
            return;
        }
        _ = _themeSelectorService.SetThemeAsync(value);
        //System.Diagnostics.Debug.WriteLine($"Value changed to {value}. Close the window now to test.");
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}

using Aoe4OverlayWinUI3.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Aoe4OverlayWinUI3.Views;

public sealed partial class ProfilePage : Page
{
    public ProfileViewModel ViewModel
    {
        get;
    }

    public ProfilePage()
    {
        ViewModel = App.GetService<ProfileViewModel>();
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.OnNavigatedTo();
    }
}

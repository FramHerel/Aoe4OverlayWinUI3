using Aoe4OverlayWinUI3.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Aoe4OverlayWinUI3.Views;

// For more details, see the documentation at https://docs.microsoft.com/windows/communitytoolkit/controls/datagrid.
public sealed partial class GamesListPage : Page
{
    public GamesListViewModel ViewModel
    {
        get;
    }

    public GamesListPage()
    {
        ViewModel = App.GetService<GamesListViewModel>();
        InitializeComponent();
    }
}

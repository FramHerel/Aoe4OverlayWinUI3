using Aoe4OverlayWinUI3.Behaviors;
using Aoe4OverlayWinUI3.ViewModels;

using Microsoft.UI.Xaml;
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
        // 在代码中设置自定义导航栏头部模板（避免 XAML 静态资源加载顺序问题）
        NavigationViewHeaderBehavior.SetHeaderTemplate(this, (DataTemplate)Resources["GamesListHeaderTemplate"]);
    }
}

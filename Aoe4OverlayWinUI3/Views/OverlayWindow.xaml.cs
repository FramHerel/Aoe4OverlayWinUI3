using Aoe4OverlayWinUI3.ViewModels;
using Microsoft.UI.Windowing;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Aoe4OverlayWinUI3.Views;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class OverlayWindow : WindowEx
{
    public OverlayViewModel ViewModel
    {
        get;
    }
    public OverlayWindow(OverlayViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(CustomTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Collapsed;
        RootGrid.SizeChanged += (s, e) => UpdateDragRegion();

    }

    // 在窗口加载完成后设置拖拽区域
    private void UpdateDragRegion()
    {
        // 获取当前窗口的缩放比例（DPI）
        double scaleAdjustment = GetRasterizationScale();

        int width = (int)(RootGrid.ActualWidth * scaleAdjustment);
        int height = (int)(RootGrid.ActualHeight * scaleAdjustment);

        RectInt32 dragRect;
        dragRect.X = 0;
        dragRect.Y = 0;
        dragRect.Width = width;
        dragRect.Height = height;

        AppWindow.TitleBar.SetDragRectangles([dragRect]);
    }

    // 在窗口大小改变时更新拖拽区域
    private double GetRasterizationScale()
    {
        return RootGrid.XamlRoot?.RasterizationScale ?? 1.0;
    }

    // 背板风格
    public partial class BlurredBackdrop : CompositionBrushBackdrop
    {
        protected override Windows.UI.Composition.CompositionBrush CreateBrush(Windows.UI.Composition.Compositor compositor)
            => compositor.CreateHostBackdropBrush();
    }

    // TODO: 为Overlay加入与主程序一致的主题切换
}

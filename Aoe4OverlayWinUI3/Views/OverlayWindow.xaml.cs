using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Aoe4OverlayWinUI3.ViewModels;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        this.SetIsMaximizable(false);
        this.SetIsMinimizable(false);

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
    public class BlurredBackdrop : CompositionBrushBackdrop
    {
        protected override Windows.UI.Composition.CompositionBrush CreateBrush(Windows.UI.Composition.Compositor compositor)
            => compositor.CreateHostBackdropBrush();
    }
}

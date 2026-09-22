using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

using Windows.UI;

namespace Aoe4OverlayWinUI3.Helpers;

/// <summary>
/// 把对局结果状态（true=胜、false=负、null=未知）转换成结果文本的颜色：
/// 胜利用系统成功色（绿）、战败用系统错误色（红）；
/// 未知结果用系统默认文本色，与其余列保持一致。
/// </summary>
public class GameResultBrushConverter : IValueConverter
{
    // 主题资源取不到时的兜底色，保证结果列始终有颜色标注
    private static readonly Color WinFallbackColor = Color.FromArgb(0xFF, 0x0F, 0x7B, 0x0F);
    private static readonly Color LossFallbackColor = Color.FromArgb(0xFF, 0xC4, 0x2B, 0x1C);
    private static readonly Color UnknownFallbackColor = Color.FromArgb(0xFF, 0x80, 0x80, 0x80);

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            true => ResolveBrush("SystemFillColorSuccessBrush", WinFallbackColor),
            false => ResolveBrush("SystemFillColorCriticalBrush", LossFallbackColor),
            _ => ResolveBrush("TextFillColorPrimaryBrush", UnknownFallbackColor),
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    private static Brush ResolveBrush(string resourceKey, Color fallbackColor)
    {
        return Application.Current.Resources.TryGetValue(resourceKey, out var resource) && resource is Brush brush
            ? brush
            : new SolidColorBrush(fallbackColor);
    }
}

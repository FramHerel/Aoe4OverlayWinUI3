using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Aoe4OverlayWinUI3.Helpers;

public class InverseNullToVisibilityConverter : IValueConverter
{
    // 如果是 null，返回 Visible
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value == null ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

}

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Aoe4OverlayWinUI3.Helpers;

public class InverseBoolToVisibilityConverter : IValueConverter
{
    // true 返回 Collapsed，false 返回 Visible
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is true ? Visibility.Collapsed : Visibility.Visible;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

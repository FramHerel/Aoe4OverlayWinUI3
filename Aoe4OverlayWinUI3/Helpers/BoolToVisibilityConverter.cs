using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Aoe4OverlayWinUI3.Helpers;

public class BoolToVisibilityConverter : IValueConverter
{
    // true 返回 Visible，false 返回 Collapsed
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is true ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;

namespace Aoe4OverlayWinUI3.Helpers;

public class NullToVisibilityConverter :IValueConverter
{
    // 如果是 null，返回 Collapsed
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value == null ? Visibility.Collapsed : Visibility.Visible;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

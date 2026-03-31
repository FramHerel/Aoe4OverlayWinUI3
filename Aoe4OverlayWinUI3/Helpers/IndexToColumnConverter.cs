using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace Aoe4OverlayWinUI3.Helpers;

public class IndexToColumnConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        // 外层 ItemsControl 的容器对象
        if (value is ContentPresenter presenter)
        {
            var itemsControl = ItemsControl.ItemsControlFromItemContainer(presenter);
            if (itemsControl != null)
            {
                int index = itemsControl.IndexFromContainer(presenter);
                return index == 0 ? 0 : 2; // 0队放左，1队放右
            }
        }
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

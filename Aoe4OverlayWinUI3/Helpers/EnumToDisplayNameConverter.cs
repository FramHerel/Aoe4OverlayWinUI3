using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.Windows.ApplicationModel.Resources;

namespace Aoe4OverlayWinUI3.Helpers;

public class EnumToDisplayNameConverter : IValueConverter
{

    private readonly ResourceLoader _resourceLoader = new();
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is ElementTheme theme)
        {
            string resourceKey = $"Settings_Appearance_ComboBox_{theme}";

            try
            {
                var localizedString = _resourceLoader.GetString(resourceKey);
                if (!string.IsNullOrEmpty(localizedString))
                {
                    return localizedString;
                }
            }
            catch
            {
                // 如果资源文件中没有找到对应的键，则返回枚举值的字符串表示
                return theme.ToString();
            }

            return theme switch
            {
                ElementTheme.Default => "Default",
                ElementTheme.Light => "Light",
                ElementTheme.Dark => "Dark",
                _ => theme.ToString()
            };
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace vrchat_osc.Extensions;

public static class StringExtension
{
    public static void Log(this string message)
    {
        MainWindow.Log(message);
    }
}

public class FontFamilyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Fonts.SystemFontFamilies.FirstOrDefault(f => f.Source == (string?)value);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => ((FontFamily)value!).Source;
}
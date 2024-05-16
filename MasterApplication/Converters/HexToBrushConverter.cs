using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MasterApplication.Converters;

class HexToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string hexColor)
        {
            object? hexValue = new BrushConverter().ConvertFrom(hexColor);
            return hexValue != null ? (SolidColorBrush)hexValue : (object)Brushes.Black;
        }

        return Brushes.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

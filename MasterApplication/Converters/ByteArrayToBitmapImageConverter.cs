using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

using MasterApplication.Helpers;

namespace MasterApplication.Converters;

/// <summary>
/// Converter of <see cref="byte[]"/> to <see cref="BitmapImage"/>.
/// </summary>
class ByteArrayToBitmapImageConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is byte[] bytes && bytes.Length > 0)
            return Utils.ByteArrayToBitmapImage(bytes);

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

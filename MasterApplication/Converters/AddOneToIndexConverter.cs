using System.Globalization;
using System.Windows.Data;

namespace MasterApplication.Converters;

/// <summary>
/// Adds one to the index.
/// </summary>
class AddOneToIndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int index = (int)value;
        return ++index;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

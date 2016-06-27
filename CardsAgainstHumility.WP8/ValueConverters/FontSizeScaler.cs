using System;
using System.Globalization;
using System.Windows.Data;

namespace CardsAgainstHumility.WP8.ValueConverters
{
    public class FontSizeScaler : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int iVal = (int)value; 
            return (int)Math.Round(1.3 * iVal);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value / 1.3;
        }
    }
}

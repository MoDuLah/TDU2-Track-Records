using System;
using System.Globalization;
using System.Windows.Data;

namespace TDU2_Track_Records
{
    public class UnitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double numericValue)
            {
                return numericValue == 1 ? "Imperial" : "Metric";
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                return strValue.Equals("Imperial", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            }
            return 0;
        }
    }
}

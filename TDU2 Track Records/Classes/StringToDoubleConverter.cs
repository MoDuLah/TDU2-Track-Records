using System;
using System.Globalization;
using System.Windows.Data;

namespace TDU2_Track_Records.Classes { 
    public class StringToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && double.TryParse(stringValue, out double doubleValue))
            {
                return doubleValue;
            }
    
            return 0; // Default value if conversion fails
        }
    
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }
    }
}
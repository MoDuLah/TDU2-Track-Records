using System;
using System.Globalization;
using System.Windows.Data;

namespace TDU2_Track_Records
{
   
    public class UnitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string system = value as string;
            return system == "Imperial" ? 1 : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int sliderValue = (int)(double)value;
            return sliderValue == 1 ? "Imperial" : "Metric";
        }
    }
}

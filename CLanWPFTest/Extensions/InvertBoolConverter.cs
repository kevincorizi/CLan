using System;
using System.Globalization;
using System.Windows.Data;

namespace CLanWPFTest.Extensions
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InvertBoolConverter : BaseBindingConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool booleanValue = (bool)value;
            return !booleanValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool booleanValue = (bool)value;
            return !booleanValue;
        }
    }
}

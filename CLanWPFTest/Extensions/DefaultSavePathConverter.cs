using System;
using System.Globalization;
using System.Windows.Data;

namespace CLanWPFTest.Extensions
{
    [ValueConversion(typeof(string), typeof(string))]
    public class DefaultSavePathConverter : BaseBindingConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string stringValue = (string)value;
            if(stringValue == "")
            {
                // The default save path will be the home directory of the user
                string homePath = (Environment.OSVersion.Platform == PlatformID.Unix || 
                    Environment.OSVersion.Platform == PlatformID.MacOSX)
                    ? Environment.GetEnvironmentVariable("HOME")
                    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
                return homePath;
            }
            return stringValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // No conversion is required when we store the path back in settings
            return value;
        }
    }
}

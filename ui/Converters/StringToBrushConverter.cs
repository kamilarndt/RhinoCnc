using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RhinoCncSuite.ui.Converters
{
    public class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var colorString = value as string;
            if (string.IsNullOrEmpty(colorString))
                return new SolidColorBrush(Colors.Gray);

            try
            {
                return (Brush)new BrushConverter().ConvertFromString(colorString);
            }
            catch
            {
                return new SolidColorBrush(Colors.Gray);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 
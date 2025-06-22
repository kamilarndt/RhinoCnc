using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RhinoCncSuite.ui.Converters
{
    public class IsLockedToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isLocked)
            {
                return isLocked ? Brushes.White : Brushes.Gray;
            }
            return Brushes.Gray; // Default
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 
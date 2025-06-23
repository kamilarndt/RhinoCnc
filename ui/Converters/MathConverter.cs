using System;
using System.Globalization;
using System.Windows.Data;

namespace RhinoCncSuite.ui.Converters
{
    public class MathConverter : IValueConverter
    {
        public static readonly MathConverter Instance = new MathConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double val && parameter is string param)
            {
                var parts = param.Split(':');
                if (parts.Length == 2)
                {
                    var operation = parts[0];
                    if (double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double operand))
                    {
                        switch (operation.ToLower())
                        {
                            case "add":
                                return val + operand;
                            case "subtract":
                                return val - operand;
                            case "multiply":
                                return val * operand;
                            case "divide":
                                if (operand != 0)
                                    return val / operand;
                                break;
                        }
                    }
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace AutoTestRunner.Converters
{
    public class ResultStatusToColorConverter : IValueConverter
    {
        private readonly static Color PassedColor = Colors.Green;
        private readonly static Color FailedColor = Colors.Red;

        private readonly static Color OnActionColor = (Color)ColorConverter.ConvertFromString("#06B523");
        private readonly static Color StoppedColor = Colors.Transparent;
        private readonly static Color WaitDelayColor = (Color)ColorConverter.ConvertFromString("#ED9E01");
        private readonly static Color WaitTriggerColor = (Color)ColorConverter.ConvertFromString("#ED9E01");
        private readonly static Color CanceledColor = Colors.Gray;
        private readonly static Color SuspendedColor = Colors.Silver;


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValuePas && intValuePas > 0 && parameter != null && parameter.ToString() == "Passed")
            {
                return "Green";
            }
            else if (value is int intValueFail && intValueFail > 0 && parameter != null && parameter.ToString() == "Failed")
            {
                return "Red";
            }
            else if (value is int intValueSkip && intValueSkip > 0 && parameter != null && parameter.ToString() == "Skipped")
            {
                return "#E68A00";
            }
            else if (value is int intPasRes && intPasRes == 1 && parameter != null && parameter.ToString() == "Result")
            {
                return "Black";
            }
            else if (value is int intFailRes && intFailRes == 2 && parameter != null && parameter.ToString() == "Result")
            {
                return "Red";
            }
            else
            {
                return "Transparent";
            }
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

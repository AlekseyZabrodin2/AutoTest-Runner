using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml;

namespace AutoTestRunnerWinUI.Converters
{
    public class AlternatingRowColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int index)
            {
                //    index = (int)value;
                //    return index % 2 == 0 ? new SolidColorBrush(Colors.DimGray) : new SolidColorBrush(Colors.Gray);
                //    //return index % 2 == 0 ? Brushes.LightGray : Brushes.White;
                //    //return index % 2 == 0 ? "#EEEEEE" : Brushes.White;
                //}

                var evenBrushLight = (SolidColorBrush)Application.Current.Resources["ControlAltFillColorTransparentBrush"];
                var oddtBrushLight = (SolidColorBrush)Application.Current.Resources["ControlAltFillColorQuarternaryBrush"];

                var evenBrushDark = (SolidColorBrush)Application.Current.Resources["ControlAltFillColorTransparentBrush"];
                var oddtBrushDark = (SolidColorBrush)Application.Current.Resources["ControlAltFillColorQuarternaryBrush"];


                // Получаем главное окно через App.MainWindow или другим способом, как ты организуешь доступ к окну
                if (App.MainWindow.Content is FrameworkElement rootElement)
                {
                    var currentTheme = rootElement.ActualTheme;

                    // Светлая тема
                    if (currentTheme == ElementTheme.Light)
                    {
                        return index % 2 == 0
                            ? evenBrushLight
                            : oddtBrushLight;
                    }
                    // Темная тема
                    else if (currentTheme == ElementTheme.Dark)
                    {
                        return index % 2 == 0
                            ? evenBrushDark
                            : oddtBrushDark;
                    }
                }
            }

            // Если value не int, вернем цвет по умолчанию
            return new SolidColorBrush(Colors.Transparent);

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

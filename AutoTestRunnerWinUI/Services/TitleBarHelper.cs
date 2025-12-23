using Microsoft.UI;
using Microsoft.UI.Xaml;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace AutoTestRunnerWinUI.Services
{
    internal class TitleBarHelper
    {

        public static void UpdateTitleBar(ElementTheme theme)
        {
            if (App.MainWindow.ExtendsContentIntoTitleBar)
            {
                if (theme == ElementTheme.Default)
                {
                    var uiSettings = new UISettings();
                    var background = uiSettings.GetColorValue(UIColorType.Background);

                    theme = background == Colors.White ? ElementTheme.Light : ElementTheme.Dark;
                }

                if (theme == ElementTheme.Default)
                {
                    theme = Application.Current.RequestedTheme == ApplicationTheme.Light ? ElementTheme.Light : ElementTheme.Dark;
                }

                App.MainWindow.AppWindow.TitleBar.ButtonForegroundColor = theme switch
                {
                    ElementTheme.Dark => Colors.White,
                    ElementTheme.Light => Colors.Black,
                    _ => Colors.Transparent
                };

                App.MainWindow.AppWindow.TitleBar.ButtonHoverForegroundColor = theme switch
                {
                    ElementTheme.Dark => Colors.White,
                    ElementTheme.Light => Colors.Black,
                    _ => Colors.Transparent
                };

                App.MainWindow.AppWindow.TitleBar.ButtonHoverBackgroundColor = theme switch
                {
                    ElementTheme.Dark => Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF),
                    ElementTheme.Light => Color.FromArgb(0x33, 0x00, 0x00, 0x00),
                    _ => Colors.Transparent
                };

                App.MainWindow.AppWindow.TitleBar.ButtonPressedBackgroundColor = theme switch
                {
                    ElementTheme.Dark => Color.FromArgb(0x66, 0xFF, 0xFF, 0xFF),
                    ElementTheme.Light => Color.FromArgb(0x66, 0x00, 0x00, 0x00),
                    _ => Colors.Transparent
                };

                App.MainWindow.AppWindow.TitleBar.BackgroundColor = Colors.Transparent;

            }
        }

        public static void ApplySystemThemeToCaptionButtons()
        {
            var frame = App.AppTitlebar as FrameworkElement;
            if (frame != null)
            {
                UpdateTitleBar(frame.ActualTheme);
            }
        }
    }
}

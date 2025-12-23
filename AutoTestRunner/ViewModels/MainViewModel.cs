using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Media.Animation;

namespace AutoTestRunner.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {

        private readonly NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        [ObservableProperty]
        public static string? _selectedPage;

        [ObservableProperty]
        public bool _isAutoTestRunnerSelected;

        [ObservableProperty]
        public bool _isTestItSelected;

        [ObservableProperty]
        public bool _isBagVisionPageSelected;

        [ObservableProperty]
        public bool _isBatteryScanPageSelected;
                
        private bool _isMenuOpen;
        public bool IsMenuOpen
        {
            get { return _isMenuOpen; }
            set
            {
                _isMenuOpen = value;
                OnPropertyChanged();
                UpdateVisualState();
            }
        }

        [ObservableProperty]
        public bool _isMainWindowEnabled = true;

        [ObservableProperty]
        public int _mainWinBlurEffectRadius = 0;



        [RelayCommand]
        private void ToggleMenu()
        {
            AnimateMenu();
            IsMenuOpen = !IsMenuOpen;
            if (IsMenuOpen)
            {
                MainWinBlurEffectRadius = 5;
                IsMainWindowEnabled = false;
            }
            else
            {
                MainWinBlurEffectRadius = 0;
                IsMainWindowEnabled = true;
            }
        }

        private void AnimateMenu()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                var menuPanel = mainWindow.MenuPanel;
                DoubleAnimation animation = new DoubleAnimation
                {
                    Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                    To = IsMenuOpen ? 60 : 120
                };
                menuPanel.BeginAnimation(FrameworkElement.WidthProperty, animation);
            }
        }

        [RelayCommand]
        private void Navigate(string pageName)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;

            IsAutoTestRunnerSelected = false;
            IsBagVisionPageSelected = false;
            IsBatteryScanPageSelected = false;
            IsTestItSelected = false;

            if (mainWindow != null)
            {
                Uri uri = new Uri($"Views/Pages/{pageName}.xaml", UriKind.Relative);
                mainWindow.MainFrame.Navigate(uri);
            }
            if(pageName == "UniExpertPage")
            {
                IsAutoTestRunnerSelected = true;
            }
            else if (pageName == "BagVisionPage")
            {
                IsBagVisionPageSelected = true;
            }
            else if (pageName == "BatteryScanPage")
            {
                IsBatteryScanPageSelected = true;
            }
            else if (pageName == "TestItPage")
            {
                IsTestItSelected = true;
            }

            SelectedPage = pageName;

            ToggleMenu();
        }

        private void UpdateVisualState()
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            VisualStateManager.GoToElementState(mainWindow.MenuPanel, IsMenuOpen ? "MenuOpen" : "MenuClosed", true);
        }

    }
}

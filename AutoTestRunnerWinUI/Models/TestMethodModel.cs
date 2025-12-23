using AutoTestRunnerWinUI.Models.TestConfig;
using AutoTestRunnerWinUI.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Windows.Media;

namespace AutoTestRunnerWinUI.Models
{
    public partial class TestMethodModel : ObservableObject
    {
        [ObservableProperty]
        public int _indexTest;

        [ObservableProperty]
        private string? _testMethodName;

        [ObservableProperty]
        public bool _isSelected = false;        
        
        [ObservableProperty]
        public int _retryCount = 0;

        [ObservableProperty]
        public int _isSelectedCount;

        [ObservableProperty]
        public int _curentCount = 0;

        //[ObservableProperty]
        private int? _passStatus = 0;

        public int? PassStatus
        {
            get => _passStatus;
            set
            {
                if (value != _passStatus)
                {
                    _passStatus = value;
                    if (_passStatus > 0)
                    {
                        VisibilityPassStatus = Visibility.Visible;
                    }
                    OnPropertyChanged(nameof (PassStatus));
                    OnPropertyChanged(nameof(DisplayPassStatus));
                }
            }
        }

        //[ObservableProperty]
        private int? _failedStatus = 0;

        public int? FailedStatus 
        {
            get => _failedStatus;
            set
            {
                if (value != _failedStatus)
                {
                    _failedStatus = value;
                    if (_failedStatus > 0)
                    {
                        VisibilityFailStatus = Visibility.Visible;
                    }
                    OnPropertyChanged(nameof(FailedStatus));
                    OnPropertyChanged(nameof(DisplayFailedStatus));
                }
            }
        }

        //[ObservableProperty]
        private int? _skipStatus = 0;

        public int? SkipStatus
        {
            get => _skipStatus;
            set
            {
                if (value != _skipStatus)
                {
                    _skipStatus = value;
                    if (_skipStatus > 0)
                    {
                        VisibilitySkipStatus = Visibility.Visible;
                    }
                    OnPropertyChanged(nameof(SkipStatus));
                    OnPropertyChanged(nameof(DisplaySkipStatus));
                }
            }
        }

        public string? DisplayPassStatus => $"Passed [ {PassStatus} ]";

        public string? DisplayFailedStatus => $"Failed [ {FailedStatus} ]";

        public string? DisplaySkipStatus => $"Skipped [ {SkipStatus} ]";

        [ObservableProperty]
        private Visibility? _visibilityPassStatus = Visibility.Collapsed;

        [ObservableProperty]
        private Visibility? _visibilityFailStatus = Visibility.Collapsed;

        [ObservableProperty]
        private Visibility? _visibilitySkipStatus = Visibility.Collapsed;

        [ObservableProperty]
        public string _descriptionAttribute;        

        [ObservableProperty]
        public TestErrorInformation _errorInformation = new();

        [ObservableProperty]
        public TestConfigFile _testConfigFile = new();

        [ObservableProperty]
        public TestConfigProperties _testConfigPropert;

        [ObservableProperty]
        private Brush _checkboxBackground = Brushes.White;

        [ObservableProperty]
        private TestsConfigView _testsConfigView;        

        [ObservableProperty]
        private Visibility _visibilitysConfigButton = Visibility.Collapsed;

        private readonly IServiceProvider _serviceProvider;



        public TestMethodModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            PropertyChanged += UpdateViewModel_PropertyChanged;
        }


        private void UpdateViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RetryCount))
            {
                RetryCount = ((TestMethodModel)sender).RetryCount;
            }
            if (e.PropertyName == nameof(IsSelected))
            {
                if (IsSelected)
                {
                    IsSelectedCount++;
                    CheckboxBackground = Brushes.YellowGreen;
                }
                else
                {
                    IsSelectedCount--;
                    CheckboxBackground = Brushes.White;
                }
            }
        }


        [RelayCommand]
        private async void ChangeJsonSettings()
        {
            TestsConfigView = _serviceProvider.GetRequiredService<TestsConfigView>();
            TestsConfigView.ViewModel.TestConfigProp = TestConfigPropert;
            TestsConfigView.DataContext = TestConfigPropert;

            ContentDialog dialog = new ContentDialog
            {
                XamlRoot = App.MainWindow.Content.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Primary,
                Content = TestsConfigView
            };

            var result = await dialog.ShowAsync();
        }

        public override string? ToString()
        {
            return TestMethodName;
        }
    }
}

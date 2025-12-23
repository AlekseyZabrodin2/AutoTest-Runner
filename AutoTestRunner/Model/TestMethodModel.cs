using AutoTestRunner.ViewModels;
using AutoTestRunner.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;

namespace AutoTestRunner.Model
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

        [ObservableProperty]
        private int? _passStatus = 0;

        [ObservableProperty]
        private int? _failedStatus = 0;

        [ObservableProperty]
        private int? _skipStatus = 0;

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
        private Visibility _visibilitysConfigButton = Visibility.Hidden;

        private readonly IServiceProvider _serviceProvider;



        public TestMethodModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            PropertyChanged += UpdateViewModel_PropertyChanged;
        }

        private void TestsConfigView_Closed(object? sender, EventArgs e)
        {
            var autoTestsViewModel = _serviceProvider.GetRequiredService<AutoTestsViewModel>();
            autoTestsViewModel.IsWindowEnabled = true;
            autoTestsViewModel.BlurEffectRadius = 0;
            TestsConfigView.Closed -= TestsConfigView_Closed;            
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
        private void ChangeJsonSettings()
        {
            TestsConfigView = _serviceProvider.GetRequiredService<TestsConfigView>();
            TestsConfigView.Closed += TestsConfigView_Closed;

            TestsConfigView.DataContext = TestConfigPropert;
            TestConfigPropert.BuildProjectResult = null;
            TestConfigPropert.IsEnabledButtons = false;

            var autoTestsViewModel = _serviceProvider.GetRequiredService<AutoTestsViewModel>();
            autoTestsViewModel.IsWindowEnabled = false;
            autoTestsViewModel.BlurEffectRadius = 5;
            TestsConfigView.Show();
        }



        

    }
}

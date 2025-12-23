using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace AutoTestRunner.Model
{
    public partial class TestSuiteModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<TestSuiteModel>? _selectedTestsCollection = new();

        [ObservableProperty]
        public string? _testClassNameModel;

        [ObservableProperty]
        private ObservableCollection<TestMethodModel>? _testMethodsModel = new();

        [ObservableProperty]
        private TestMethodModel? _methodsModel;

        [ObservableProperty]
        public bool _testSuiteIsSelected = false;

        [ObservableProperty]
        public int _retryTestSuiteCount;

        [ObservableProperty]
        public bool _isRunning;

        [ObservableProperty]
        private int? _numberOfTests = 0;

        [ObservableProperty]
        private bool? _groupIsExpanded = true;

        [ObservableProperty]
        private int? _passStatusTest = 0;

        [ObservableProperty]
        private int? _failedStatusTest = 0;

        [ObservableProperty]
        private int? _skippedStatusTest = 0;

        [ObservableProperty]
        private int? _resultColor = 0;

        [ObservableProperty]
        private Visibility? _visibilityPassResult = Visibility.Collapsed;

        [ObservableProperty]
        private Visibility? _visibilityFailResult = Visibility.Collapsed;

        [ObservableProperty]
        private Visibility? _visibilitySkipResult = Visibility.Collapsed;

        [ObservableProperty]
        public int _selectedTestsInSuiteCount = 0;

        [ObservableProperty]
        private Brush _checkboxSuiteBackground = Brushes.White;

        private readonly IServiceProvider _serviceProvider;


        public TestSuiteModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            PropertyChanged += UpdateViewModel_PropertyChanged;
            TestMethodsModel.CollectionChanged += Update_PropertyChanged;            
        }

        private void Update_PropertyChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var method in TestMethodsModel)
            {
                method.PropertyChanged -= UpdateTestMethodModel_PropertyChanging;
            }
            foreach (var method in TestMethodsModel)
            {
                method.PropertyChanged += UpdateTestMethodModel_PropertyChanging;
            }
        }

        private void UpdateViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RetryTestSuiteCount))
            {
                IsRunning = false;
            }            
            if (!IsRunning)
            {
                foreach (var method in TestMethodsModel)
                {
                    method.RetryCount = RetryTestSuiteCount;
                }
            }
        }

        private void UpdateTestMethodModel_PropertyChanging(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                if (((TestMethodModel)sender).IsSelected)
                {
                    TestSuiteIsSelected = true;

                    SelectedTestsInSuiteCount++;
                    CheckboxSuiteBackground = Brushes.YellowGreen;

                    foreach (var method in TestMethodsModel)
                    {
                        if (!method.IsSelected)
                        {
                            TestSuiteIsSelected = false;
                            break;
                        }
                    }                    
                }
                else
                {
                    SelectedTestsInSuiteCount--;
                    foreach (var method in TestMethodsModel)
                    {
                        TestSuiteIsSelected = false;

                        if (method.IsSelected)
                        {
                            CheckboxSuiteBackground = Brushes.YellowGreen;
                            break;
                        }
                        CheckboxSuiteBackground = Brushes.White;
                    }
                    
                }
            }
        }


        [RelayCommand]
        private void SelectTestMethod()
        {
            var existingModel = SelectedTestsCollection!.FirstOrDefault(model => model.TestClassNameModel == TestClassNameModel);
            var state = TestSuiteIsSelected;

            if (existingModel != null && existingModel.TestSuiteIsSelected)
            {
                if (state)
                {
                    foreach (var testMethod in TestMethodsModel!)
                    {
                        testMethod.IsSelected = true;
                        existingModel.TestSuiteIsSelected = state;
                    }
                }
                else
                {
                    foreach (var testMethod in TestMethodsModel!)
                    {
                        testMethod.IsSelected = false;
                        existingModel.TestSuiteIsSelected = state;
                    }
                }
            }
            else if (!TestSuiteIsSelected)
            {
                foreach (var testMethod in TestMethodsModel!)
                {
                    testMethod.IsSelected = false;
                }
                TestSuiteIsSelected = false;
                SelectedTestsCollection.Remove(existingModel);
            }
            else
            {
                var model = _serviceProvider.GetRequiredService<TestSuiteModel>();
                model.TestClassNameModel = TestClassNameModel;
                model.TestSuiteIsSelected = state;
                foreach (var testMethod in TestMethodsModel!)
                {
                    testMethod.IsSelected = true;
                }
                SelectedTestsCollection.Add(model);
                existingModel = model;
                existingModel.TestSuiteIsSelected = state;
            }
        }


    }
}

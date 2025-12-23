using AutoTestRunnerWinUI.Models.TestConfig;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace AutoTestRunnerWinUI.Models
{
    public partial class TestConfigProperties : ObservableObject
    {
        [ObservableProperty]
        public string? _testName;

        [ObservableProperty]
        public int? _gender;

        [ObservableProperty]
        public string? _lastName;

        [ObservableProperty]
        public string? _firstName;

        [ObservableProperty]
        public string? _middleName;

        [ObservableProperty]
        public string? _address;

        [ObservableProperty]
        public string? _phoneNumber;

        [ObservableProperty]
        public string? _comments;

        [ObservableProperty]
        public int? _repeatNumber;

        [ObservableProperty]
        public bool? _expectedResult;

        [ObservableProperty]
        public TestConfigFile _testConfig;

        [ObservableProperty]
        public TestConfigProperties _testConfigProp;

        private Process? _testRunnerProcess;

        [ObservableProperty]
        public TestMethodModel _testModel;

        [ObservableProperty]
        //[NotifyPropertyChangedFor(nameof(SaveJsonSettingsCanExecute))]
        //[NotifyPropertyChangedFor(nameof(CancelSavingCanExecute))]
        [NotifyCanExecuteChangedFor(nameof(SaveJsonSettingsCommand))]
        [NotifyCanExecuteChangedFor(nameof(CancelSavingCommand))]
        public bool _isEnabledButtons = true;

        [ObservableProperty]
        public Visibility? _visibilitysConfigProperty;

        [ObservableProperty]
        public string? _buildProjectResult;

        [ObservableProperty]
        private Brush _buildProjectResultForeground = new SolidColorBrush(Colors.White);

        private IOptions<TestsWorkDirectory> _testsWorkDirectory;



        //public bool SaveJsonSettingsCanExecute => IsEnabledButtons;
        //public bool CancelSavingCanExecute => IsEnabledButtons;

        private readonly IServiceProvider _serviceProvider;


        public TestConfigProperties(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private void TestConfigProperties_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(TestConfigProp))
            {
                IsEnabledButtons = true;
            }            
        }


        [RelayCommand(CanExecute = nameof(IsEnabledButtons))]
        private void SaveJsonSettings()
        {
            TestConfigProp.PropertyChanged += TestConfigProperties_PropertyChanged;

            ConfigureViewModels(_serviceProvider, "UniExpertPage");

            string filePath = _testsWorkDirectory.Value.PathToTestConfig! + "testsData.json";

            SaveToJsonAsync(filePath);

            IsEnabledButtons = false;
        }

        public void ConfigureViewModels(IServiceProvider serviceProvider, string pageName)
        {
            var optionsSnapshot = serviceProvider.GetRequiredService<IOptionsSnapshot<TestsWorkDirectory>>();

            TestsWorkDirectory selectedConfig = pageName switch
            {
                "UniExpertPage" => optionsSnapshot.Get("UniExpert"),
                "BagVisionPage" => optionsSnapshot.Get("BagVision"),
                "BatteryScanPage" => optionsSnapshot.Get("BatteryScan"),
                _ => throw new ArgumentException("Invalid page name", nameof(pageName))
            };

            _testsWorkDirectory = Options.Create(selectedConfig);
        }

        [RelayCommand(CanExecute = nameof(IsEnabledButtons))]
        private void CancelSaving()
        {
            TestConfigProp.PropertyChanged += TestConfigProperties_PropertyChanged;
            ResetChanges();
        }


        public void SaveToJsonAsync(string filePath)
        {

            CheckConfig();

            UpdateConfig();

            try
            {
                // Обновляем только измененные свойства
                if (!TestConfig.Equals(TestConfig.TestsConstData))
                {
                    TestConfig.TestsConstData = TestConfig.TestsConstData;
                }

                // Сериализация
                //var settings = new JsonSerializerOptions { IgnoreNullValues = true };
                var unicodeOptions = new JsonSerializerOptions
                {
                    //Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

                    WriteIndented = true
                };

                string updatedJson = JsonSerializer.Serialize(TestConfig, unicodeOptions);

                // Перезапись файла JSON с обновленными данными
                File.WriteAllText(filePath, updatedJson);

                // метод ребилда
                //BuildTestProject();

                BuildProjectResult = "Saving successfully";
                BuildProjectResultForeground = new SolidColorBrush(Colors.Green);
            }            
            catch (Exception ex)
            {
                BuildProjectResult = "Saving failed";
                BuildProjectResultForeground = new SolidColorBrush(Colors.Red);
            }
        }


        public TestConfigFile CheckConfig()
        {

            if (TestConfig == null)
            {
                TestConfig = new();
            }

            var testsConstData = _serviceProvider.GetRequiredService<TestConfigFile>();
            TestConfig.TestsConstData = testsConstData.TestsConstData;

            return TestConfig;
        }


        public void UpdateConfig()
        {
            // Получаем новые данные для обновления
            var newConfigProperties = UpdateConfigProperties();

            // Получаем тип объекта TestsConstData
            var testDataType = TestConfig.TestsConstData.GetType();

            // Получаем свойства объекта TestsConstData
            var propertiesConstData = testDataType.GetProperties();

            // Проходим по всем свойствам TestsConstData
            foreach (var property in propertiesConstData)
            {
                // Получаем значение свойства TestsConstData
                var valueConstData = property.GetValue(TestConfig.TestsConstData);

                // Проверяем, что значение не пустое
                if (valueConstData != null)
                {
                    // Проверяем, что тип свойства - класс (не строка)
                    if (property.PropertyType.IsClass && property.PropertyType.Name != "String")
                    {
                        // Получаем внутренние свойства объекта TestsConstData
                        var innerProperties = valueConstData.GetType().GetProperties();

                        // Проходим по внутренним свойствам
                        foreach (var innerProperty in innerProperties)
                        {
                            // Получаем значение внутреннего свойства
                            var innerValue = innerProperty.GetValue(valueConstData);

                            if (innerValue != null)
                            {
                                if (innerValue.ToString() == TestConfigProp.TestName)
                                {
                                    foreach (var innerProp in innerProperties)
                                    {
                                        // Сравниваем соответствующее свойство из новых данных
                                        var matchingProperty = newConfigProperties.GetType().GetProperty(innerProp.Name);

                                        // Если найдено совпадение
                                        if (matchingProperty != null)
                                        {
                                            // Получаем значение из новых данных
                                            var newValue = matchingProperty.GetValue(newConfigProperties);

                                            // Устанавливаем новое значение в TestsConstData
                                            innerProp.SetValue(valueConstData, newValue);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ResetChanges()
        {
            var defaultConfigProperties = CheckConfig();

            var resetTestConfigProp = _serviceProvider.GetRequiredService<TestConfigProperties>();

            var testDataType = defaultConfigProperties.TestsConstData.GetType();
            var propertiesConstData = testDataType.GetProperties();

            foreach (var property in propertiesConstData)
            {
                var valueConstData = property.GetValue(defaultConfigProperties.TestsConstData);

                if (valueConstData != null)
                {
                    if (property.PropertyType.IsClass && property.PropertyType.Name != "String")
                    {
                        var innerProperties = valueConstData.GetType().GetProperties();

                        foreach (var propertyName in innerProperties)
                        {
                            var innerValue = propertyName.GetValue(valueConstData);

                            if (innerValue?.ToString() == TestConfigProp.TestName)
                            {
                                var configProperties = resetTestConfigProp.GetType().GetProperties();

                                foreach (var propertyInfo in innerProperties)
                                {
                                    var propertyInfoValue = propertyInfo.GetValue(valueConstData);

                                    var matchingProperty = configProperties.FirstOrDefault(configProperty => configProperty.Name == propertyInfo.Name);

                                    if (matchingProperty != null)
                                    {
                                        matchingProperty.SetValue(resetTestConfigProp, propertyInfoValue);
                                    }
                                }
                            }
                        }

                    }
                }
            }

            ResetConfigProperties(resetTestConfigProp);

        }


        public TestConfigProperties UpdateConfigProperties()
        {
            var testConfig = _serviceProvider.GetRequiredService<TestConfigProperties>();
            {
                testConfig.TestName = TestConfigProp.TestName;
                testConfig.Gender = TestConfigProp.Gender;
                testConfig.LastName = TestConfigProp.LastName;
                testConfig.FirstName =  TestConfigProp.FirstName;
                testConfig.MiddleName = TestConfigProp.MiddleName;
                testConfig.Address = TestConfigProp.Address;
                testConfig.PhoneNumber = TestConfigProp.PhoneNumber;
                testConfig.Comments = TestConfigProp.Comments;
                testConfig.RepeatNumber = TestConfigProp.RepeatNumber;
                testConfig.ExpectedResult = TestConfigProp.ExpectedResult;
            }

            TestConfigProp = testConfig;

            return TestConfigProp;
        }

        public void ResetConfigProperties(TestConfigProperties defaultProperties)
        {
            TestConfigProp.TestName = defaultProperties.TestName;
            TestConfigProp.Gender = defaultProperties.Gender;
            TestConfigProp.LastName = defaultProperties.LastName;
            TestConfigProp.FirstName = defaultProperties.FirstName;
            TestConfigProp.MiddleName = defaultProperties.MiddleName;
            TestConfigProp.Address = defaultProperties.Address;
            TestConfigProp.PhoneNumber = defaultProperties.PhoneNumber;
            TestConfigProp.Comments = defaultProperties.Comments;
            TestConfigProp.RepeatNumber = defaultProperties.RepeatNumber;
            TestConfigProp.ExpectedResult = defaultProperties.ExpectedResult;

            TestConfigProp.IsEnabledButtons = false;

            IsEnabledButtons = false;
        }
    }
}

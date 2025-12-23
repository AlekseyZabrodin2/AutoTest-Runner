using AutoTestRunner.Model.TestConfig;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;

namespace AutoTestRunner.Model
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
        public bool _isEnabledButtons;

        [ObservableProperty]
        public Visibility? _visibilitysConfigProperty;

        [ObservableProperty]
        public string? _buildProjectResult;

        [ObservableProperty]
        private Brush _buildProjectResultForeground = Brushes.White;



        // public bool SaveJsonSettingsCanExecute => IsEnabledButtons;

        // public bool CancelSavingCanExecute => IsEnabledButtons;

        private readonly IServiceProvider _serviceProvider;


        public TestConfigProperties(IServiceProvider serviceProvider)
        {
            PropertyChanged += TestConfigProperties_PropertyChanged;
            _serviceProvider = serviceProvider;
        }

        private void TestConfigProperties_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName != nameof(IsEnabledButtons))
            {
                IsEnabledButtons = true;
            }            
        }




        [RelayCommand(CanExecute = nameof(IsEnabledButtons))]
        private void SaveJsonSettings()
        {
            string filePath = _serviceProvider.GetRequiredService<IOptions<TestsWorkDirectory>>().Value.PathToTestConfig! + "testsData.json";

            SaveToJsonAsync(filePath);

            IsEnabledButtons = false;
        }

        [RelayCommand(CanExecute = nameof(IsEnabledButtons))]
        private void CancelSaving()
        {
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
                BuildProjectResultForeground = Brushes.Green;
            }            
            catch (Exception ex)
            {
                BuildProjectResult = "Saving failed";
                BuildProjectResultForeground = Brushes.Red;
            }
        }


        public TestConfigFile CheckConfig()
        {

            if (TestConfig == null)
            {
                TestConfig = new();
            }

            IConfiguration configurationTestData = new ConfigurationBuilder()
               .SetBasePath(_serviceProvider.GetRequiredService<IOptions<TestsWorkDirectory>>().Value.PathToTestConfig!)
               .AddJsonFile("testsData.json")
               .Build();

            IServiceCollection configServices = new ServiceCollection();
            configServices.Configure<TestsConstData>(configurationTestData.GetSection("TestsConstData"));

            IServiceProvider configServiceProvider = configServices.BuildServiceProvider();
            var testsConstData = configServiceProvider.GetRequiredService<IOptions<TestsConstData>>().Value;

            TestConfig.TestsConstData = testsConstData;

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
                                if (innerValue.ToString() == TestName)
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

                            if (innerValue?.ToString() == TestName)
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
                testConfig.TestName = TestName;
                testConfig.Gender = Gender;
                testConfig.LastName = LastName;
                testConfig.FirstName = FirstName;
                testConfig.MiddleName = MiddleName;
                testConfig.Address = Address;
                testConfig.PhoneNumber = PhoneNumber;
                testConfig.Comments = Comments;
                testConfig.RepeatNumber = RepeatNumber;
                testConfig.ExpectedResult = ExpectedResult;
            }

            TestConfigProp = testConfig;

            return TestConfigProp;
        }

        public void ResetConfigProperties(TestConfigProperties defaultProperties)
        {
            TestName = defaultProperties.TestName;
            Gender = defaultProperties.Gender;
            LastName = defaultProperties.LastName;
            FirstName = defaultProperties.FirstName;
            MiddleName = defaultProperties.MiddleName;
            Address = defaultProperties.Address;
            PhoneNumber = defaultProperties.PhoneNumber;
            Comments = defaultProperties.Comments;
            RepeatNumber = defaultProperties.RepeatNumber;
            ExpectedResult = defaultProperties.ExpectedResult;

            IsEnabledButtons = false;
        }

        public void BuildTestProject()
        {
            try
            {
                _testRunnerProcess = new();
                TestErrorInformation errorDataAll = new();

                _testRunnerProcess.StartInfo.FileName = "dotnet";
                _testRunnerProcess.StartInfo.Arguments = $"build \"{_serviceProvider.GetRequiredService<IOptions<TestsWorkDirectory>>().Value.PathToTestProject!}\"";
                _testRunnerProcess.StartInfo.UseShellExecute = false;
                _testRunnerProcess.StartInfo.RedirectStandardOutput = true;
                _testRunnerProcess.StartInfo.RedirectStandardError = true;

                _testRunnerProcess.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        GetAllErrorMessage(args, ref errorDataAll);

                        if (!string.IsNullOrEmpty(errorDataAll.ErrorAllDataMessage))
                        {
                            string filePath = "errorBuild_log.log";
                            File.WriteAllText(filePath, errorDataAll.ErrorAllDataMessage);
                        }
                    };
                };

                _testRunnerProcess.Start();
                _testRunnerProcess.BeginOutputReadLine();
                _testRunnerProcess.BeginErrorReadLine();

                _testRunnerProcess.WaitForExit();

                BuildProjectResult = "Rebuild all succeeded";
                BuildProjectResultForeground = Brushes.Green;
            }
            catch (Exception)
            {
                BuildProjectResult = "Rebuild all Failed";
                BuildProjectResultForeground = Brushes.Red;
            }
            
        }

        private void GetAllErrorMessage(DataReceivedEventArgs args, ref TestErrorInformation errorInformation)
        {
            string errorData = args.Data;
            errorInformation.ErrorAllDataMessage += errorData + Environment.NewLine;
        }


    }
}

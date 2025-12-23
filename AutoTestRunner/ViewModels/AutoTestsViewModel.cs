
using AutoTestRunner.Model.TestConfig;
using AutoTestRunner.Model;
using AutoTestRunner.Services;
using AutoTestRunner.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Animation;
using System.Windows;
using System.IO;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using AutoTestRunner.Views.Pages;
using System.Runtime.InteropServices.JavaScript;
using AventStack.ExtentReports.Model;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Threading;
using System.Reactive.Concurrency;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using NUnit.Framework;

namespace AutoTestRunner.ViewModels
{
    public partial class AutoTestsViewModel : ObservableObject
    {
        private readonly IHost _host;
        private readonly NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        [ObservableProperty]
        public int _index;

        [ObservableProperty]
        public int _testsCount;

        [ObservableProperty]
        private string? _selectedTestMethod;

        private bool _areAllGroupsExpanded;
        public bool AreAllGroupsExpanded
        {
            get { return _areAllGroupsExpanded; }
            set
            {
                _areAllGroupsExpanded = value;
                OnPropertyChanged(nameof(AreAllGroupsExpanded));

                foreach (var group in ObservedTestSuiteModel)
                {
                    if (group.GroupIsExpanded == true)
                    {
                        group.GroupIsExpanded = value;
                    }
                    group.GroupIsExpanded = value;
                }
            }
        }

        [ObservableProperty]
        private ObservableCollection<TestSuiteModel>? _observedTestSuiteModel;

        [ObservableProperty]
        private ObservableCollection<TestSuiteModel>? _selectedTestsCollection;

        [ObservableProperty]
        private ObservableCollection<TestMethodModel>? _observedTestMethodModel;

        [ObservableProperty]
        private TestSuiteModel? _selectedTestSuite;

        [ObservableProperty]
        private TestMethodModel? _selectedTestModel;

        [ObservableProperty]
        private CollectionViewSource _collectionViewSource = new();

        [ObservableProperty]
        public TestConfigFile _testConfigFile = new();

        private Process? _testRunnerProcess;



        [ObservableProperty]
        public int _selectedTestsCount;

        [ObservableProperty]
        private string? _selectedTestMethodName;
                
        private bool _selectAll;

        public bool SelectAll
        {
            get => _selectAll;
            set
            {
                _selectAll = value;
                OnPropertyChanged(nameof(SelectAll));
                foreach (var testSuite in ObservedTestSuiteModel)
                {
                    testSuite.TestSuiteIsSelected = value;
                    foreach (var testMethod in testSuite.TestMethodsModel)
                    {
                        testMethod.IsSelected = value;
                        if (testMethod.IsSelected)
                        {
                            AllCheckboxBackground = Brushes.YellowGreen;
                        }
                        else
                        {
                            AllCheckboxBackground = Brushes.White;
                        }
                    }
                }
            }
        }

        [ObservableProperty]
        private Brush _allCheckboxBackground = Brushes.White;

        [ObservableProperty]
        public int _retrySelectedCount = 0;

        [ObservableProperty]
        public int _curentSelectedCount = 0;

        [ObservableProperty]
        public int _completedTestsCount = 0;

        [ObservableProperty]
        private int? _selectedPassStatus = 0;

        [ObservableProperty]
        private int? _selectedFailedStatus = 0;

        [ObservableProperty]
        private int? _selectedSkippedStatus = 0;

        [ObservableProperty]
        private int? _selectedTestResultColor = 0;

        [ObservableProperty]
        public int _selectedTestsInSuiteCount = 0;

        [ObservableProperty]
        private Visibility? _visibilityPassedResult = Visibility.Collapsed;

        [ObservableProperty]
        private Visibility? _visibilityFailedResult = Visibility.Collapsed;

        [ObservableProperty]
        private Visibility? _visibilitySkippedResult = Visibility.Collapsed;

        [ObservableProperty]
        private CancellationTokenSource _stoppingToken;

        [ObservableProperty]
        public static HtmlReport _extentReport;

        public static string? _oldNameFullPath;
        public static string? _htmlReportResult;

        [ObservableProperty]
        public string _testStatusResult;

        [ObservableProperty]
        public TestConfigProperties _testConfig;

        [ObservableProperty]
        public bool _isWindowEnabled = true;

        [ObservableProperty]
        public bool _isTestsWindowEnabled = true;

        [ObservableProperty]
        public int _blurEffectRadius = 0;

        private IOptions<TestsWorkDirectory> _testsWorkDirectory;

        [ObservableProperty]
        private Visibility? _visibilityReportButton = Visibility.Hidden;

        [ObservableProperty]
        private DateTime _timeStartTests;

        [ObservableProperty]
        private DateTime _timeStopTests;

        [ObservableProperty]
        private Stopwatch _totalTimeStopwatch;

        [ObservableProperty]
        private string _totalTimeString = "00:00:00.00";

        [ObservableProperty]
        DispatcherTimer _dispTimer = new(); 


        public AutoTestsViewModel()
        {
            try
            {
                _logger.Trace("AutoTestRunner start to load");

                HostApplicationBuilder builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
                {
                    Args = Environment.GetCommandLineArgs(),
                    ContentRootPath = AppContext.BaseDirectory
                });

                builder.Logging.ClearProviders();
                builder.Logging.AddNLog();

                builder.Services.AddSingleton(this);
                builder.Services.AddTransient<MainViewModel>();
                builder.Services.AddTransient<TestSuiteModel>();
                builder.Services.AddTransient<TestMethodModel>();
                builder.Services.AddTransient<TestConfigProperties>();
                builder.Services.AddTransient<HtmlReport>();

                builder.Services.AddTransient<UniExpertPage>();
                builder.Services.AddTransient<BatteryScanPage>();
                builder.Services.AddTransient<BagVisionPage>();

                builder.Services.AddTransient<TestsConfigView>();

                var serviceProvider = builder.Services.BuildServiceProvider();
                var options = serviceProvider.GetRequiredService<IOptions<MainViewModel>>();
                var selectedPage = options.Value.SelectedPage;

                ConfigBuilder(builder, selectedPage!);

                _host = builder.Build();

                _testsWorkDirectory = _host.Services.GetRequiredService<IOptions<TestsWorkDirectory>>();

                GetTestNames();

                _logger.Trace("App is loaded");

                PropertyChanged += UpdateTestSuitModel_PropertyChanged;

                DispTimer.Tick += new EventHandler(Timer_Tick);
                DispTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void UpdateTestSuitModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RetrySelectedCount))
            {
                foreach (var method in ObservedTestSuiteModel)
                {
                    method.RetryTestSuiteCount = RetrySelectedCount;
                }
            }
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            string currentTime = string.Empty;

            if (TotalTimeStopwatch.IsRunning)
            {
                TimeSpan ts = TotalTimeStopwatch.Elapsed;
                currentTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                TotalTimeString = currentTime;
            }
        }


        public void ConfigBuilder(IHostApplicationBuilder builder, string pageName)
        {
            _logger.Trace("Enter config builder");

            ConfigureViewModels(builder, pageName);

            var pluginOptions = builder.Configuration.GetRequiredSection("WorkDirectorys").Get<TestsWorkDirectory>();

            if (pageName == "UniExpertPage")
            {
                var testsDataConfig = builder.Configuration.AddJsonFile(Path.Combine(pluginOptions!.PathToTestConfig, "testsData.json"));
                var testsConstDataConfig = builder.Services.Configure<TestsConstData>(builder.Configuration.GetSection("TestsConstData"));
            }

            IServiceProvider serviceProvider = builder.Services.BuildServiceProvider();
            var myOptions = serviceProvider.GetRequiredService<IOptions<TestsWorkDirectory>>().Value;
            var testsConstData = serviceProvider.GetRequiredService<IOptions<TestsConstData>>().Value;

            TestConfigFile.TestsConstData = testsConstData;


            var testDataType = testsConstData.GetType();
            var propertiesConstData = testDataType.GetProperties();

            foreach (var property in propertiesConstData)
            {
                var valueConstData = property.GetValue(testsConstData);

                _logger.Trace($"Property name from Config - {property.Name}, value - {valueConstData}");
            }
        }


        public void ConfigureViewModels(IHostApplicationBuilder builder, string pageName)
        {
            if (pageName == "UniExpertPage")
            {
                builder.Configuration.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "AutoTestsDll/UniExpertDll/uniExpertSettings.json"));
                builder.Services.Configure<TestsWorkDirectory>(builder.Configuration.GetSection("WorkDirectorys"));

            }
            else if (pageName == "BagVisionPage")
            {
                builder.Configuration.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "AutoTestsDll/BagVisionDll/bagVisionSettings.json"));
                builder.Services.Configure<TestsWorkDirectory>(builder.Configuration.GetSection("WorkDirectorys"));
            }
            else if (pageName == "BatteryScanPage")
            {
                builder.Configuration.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "AutoTestsDll/BatteryScanDll/batteryScanSettings.json"));
                builder.Services.Configure<TestsWorkDirectory>(builder.Configuration.GetSection("WorkDirectorys"));
            }
        }


        public void GetTestNames()
        {
            _logger.Trace("Enter GetTestNames");

            var assembly = Assembly.LoadFrom(_testsWorkDirectory.Value.AssemblyPath!);
            var testClasses = assembly.GetTypes().Where(t => t.GetMethods().Any(m => m.GetCustomAttributes(typeof(TestMethodAttribute), true).Any() ||
            m.GetCustomAttributes(typeof(TestAttribute), true).Any())); // если нужно изменить порядок отображения тестов// .Reverse();
            ObservedTestSuiteModel = new();
            ObservedTestMethodModel = new();
            SelectedTestSuite = _host.Services.GetRequiredService<TestSuiteModel>();

            foreach (var testClass in testClasses)
            {
                SelectedTestSuite = _host.Services.GetRequiredService<TestSuiteModel>();
                SelectedTestSuite.TestClassNameModel = testClass.Name;

                var testMethods = testClass.GetMethods().Where(m => m.GetCustomAttributes(typeof(TestMethodAttribute), true).Any() ||
                m.GetCustomAttributes(typeof(TestAttribute), true).Any());

                foreach (var method in testMethods)
                {
                    Index++;
                    var descriptionAttribute = method.GetCustomAttribute<Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute>();
                    var description = string.Empty;
                    if (descriptionAttribute != null)
                    {
                        description = descriptionAttribute.Description;
                    }

                    SelectedTestModel = _host.Services.GetRequiredService<TestMethodModel>();
                    SelectedTestModel.TestMethodName = method.Name;
                    SelectedTestModel.DescriptionAttribute = description;
                    SelectedTestModel.IndexTest = Index;

                    var testDataType = TestConfigFile.TestsConstData.GetType();
                    var propertiesConstData = testDataType.GetProperties();

                    foreach (var property in propertiesConstData)
                    {
                        var valueConstData = property.GetValue(TestConfigFile.TestsConstData);

                        if (valueConstData != null)
                        {
                            if (property.PropertyType.IsClass && property.PropertyType.Name != "String")
                            {
                                var innerProperties = valueConstData.GetType().GetProperties();

                                foreach (var propertyName in innerProperties)
                                {
                                    var innerValue = propertyName.GetValue(valueConstData);

                                    if (innerValue?.ToString() == method.Name)
                                    {
                                        SelectedTestModel.TestConfigFile = TestConfigFile;
                                        SelectedTestModel.TestConfigPropert = _host.Services.GetRequiredService<TestConfigProperties>();

                                        var configProperties = SelectedTestModel.TestConfigPropert.GetType().GetProperties();

                                        foreach (var propertyInfo in innerProperties)
                                        {
                                            var propertyInfoValue = propertyInfo.GetValue(valueConstData);

                                            var matchingProperty = configProperties.FirstOrDefault(configProperty => configProperty.Name == propertyInfo.Name);

                                            if (matchingProperty != null)
                                            {
                                                matchingProperty.SetValue(SelectedTestModel.TestConfigPropert, propertyInfoValue);
                                                _logger.Trace($"Property name - {matchingProperty.Name}, value - {propertyInfoValue}");

                                                SelectedTestModel.VisibilitysConfigButton = Visibility.Visible;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    TestsCount = SelectedTestModel.IndexTest;

                    SelectedTestSuite.TestMethodsModel!.Add(SelectedTestModel);
                }
                ObservedTestSuiteModel.Add(SelectedTestSuite);

                SelectedTestSuite.NumberOfTests = SelectedTestSuite.TestMethodsModel.Count;
            }
        }


        public ObservableCollection<TestSuiteModel> GetSelectedTestsCollection()
        {
            SelectedTestsCollection = new();

            SelectedTestSuite = _host.Services.GetRequiredService<TestSuiteModel>();
            SelectedTestsCount = 0;
            CurentSelectedCount = 0;

            foreach (var selectedTestNames in ObservedTestSuiteModel!)
            {
                //SelectedTestSuite = selectedTestNames;

                foreach (var selectedTest in selectedTestNames.TestMethodsModel!)
                {
                    if (selectedTest.IsSelected)
                    {
                        SelectedTestModel = _host.Services.GetRequiredService<TestMethodModel>();
                        SelectedTestsCount++;
                        SelectedTestModel.TestMethodName = selectedTest.TestMethodName;
                        SelectedTestModel.IsSelected = selectedTest.IsSelected;
                        SelectedTestModel.RetryCount = selectedTest.RetryCount;
                        SelectedTestsInSuiteCount = selectedTest.IsSelectedCount;
                        CurentSelectedCount = CurentSelectedCount + selectedTest.RetryCount;

                        SelectedTestSuite.TestMethodsModel!.Add(SelectedTestModel);
                        SelectedTestsCollection.Add(SelectedTestSuite);

                        //for (testName.CurentCount = 0; testName.CurentCount < testName.RetryCount; testName.CurentCount++)
                        //{


                        //}
                    }
                }
            }
            return SelectedTestsCollection;
        }


        [RelayCommand]
        private async Task RunAutoTest()
        {
            _logger.Trace("Enter in RunAutoTest");

            try
            {
                _stoppingToken = new();

                IsTestsWindowEnabled = false;

                ResetTestsResult();

                ExtentReport = _host.Services.GetRequiredService<HtmlReport>();

                ExtentReport.ReportLogger("UI Test");

                TimeStartTests = ExtentReport.TimeTestStart;

                TotalTimeStopwatch = new();
                TotalTimeStopwatch.Start();
                DispTimer.Start();

                _oldNameFullPath = _testsWorkDirectory.Value.OldNameFullPath;

                CancellationToken cancellationToken = _stoppingToken.Token;

                GetSelectedTestsCollection();
                CompletedTestsCount = 0;

                foreach (var testNames in ObservedTestSuiteModel!)
                {
                    SelectedTestSuite = testNames;

                    foreach (var testName in testNames.TestMethodsModel!)
                    {
                        if (testName.IsSelected)
                        {
                            testNames.IsRunning = true;

                            SelectedTestMethod = testName.TestMethodName;

                            if (testName.RetryCount < 1)
                            {
                                MessageBox.Show("Vvedi chislo povtorov, Vasia !", "Info", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                                break;
                            }

                            for (testName.CurentCount = 0; testName.CurentCount < testName.RetryCount; testName.CurentCount++)
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    // Выход из цикла, если операция была отменена
                                    break;
                                }
                                ExtentReport = _host.Services.GetRequiredService<HtmlReport>();

                                ExtentReport.InitializeTests(testName.TestMethodName, SelectedTestSuite.TestClassNameModel);

                                await RunTests(_testsWorkDirectory.Value.AssemblyPath!, testName);
                                CompletedTestsCount++;
                            }
                        }
                    }
                }

                _logger.Trace("Create tests report begin");

                ExtentReport.CreateReport();

                TotalTimeStopwatch.Stop();
                DispTimer.Stop();

                TimeStopTests = ExtentReport.TimeTestStop;

                _logger.Trace("Create tests report end");

                IsTestsWindowEnabled = true;

                _htmlReportResult = _testsWorkDirectory.Value.NewNameFullPath + DateTime.Now.ToString("_dd.MM_HH.mm.ss") + "_Report" + ".html";

                var fileDire = Path.GetDirectoryName(_htmlReportResult);
                if (!Directory.Exists(fileDire))
                {
                    Directory.CreateDirectory(fileDire);
                }

                if (File.Exists(_oldNameFullPath))
                {
                    _logger.Trace("Create tests report");

                    File.Move(_oldNameFullPath!, _htmlReportResult!);                    
                }
                if (File.Exists(_htmlReportResult))
                {
                    VisibilityReportButton = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception during RunAutoTest method: {ex.Message}");
            }
        }


        public async Task RunTests(string projectPath, TestMethodModel testMethod)
        {
            _logger.Trace("Enter in RunTests");

            StringBuilder outputBuilder = new StringBuilder();
            TestErrorInformation errorDataAll = new();
            bool saveData = testMethod.ErrorInformation.ExistErrorAllDataMessage;

            _logger.Info($"Run test \"{projectPath}\" --filter FullyQualifiedName~\"{SelectedTestSuite!.TestClassNameModel}&FullyQualifiedName~{SelectedTestMethod}\"");
            _logger.Info($"\r\nRun test - [ {SelectedTestMethod} ]");

            _testRunnerProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"test \"{projectPath}\" --filter FullyQualifiedName~\"{SelectedTestSuite!.TestClassNameModel}&FullyQualifiedName~{SelectedTestMethod}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                }
            };

            _testRunnerProcess.StartInfo.EnvironmentVariables["DOTNET_CLI_UI_LANGUAGE"] = "en";
            _testRunnerProcess.StartInfo.EnvironmentVariables["LANG"] = "en_US.UTF-8";

            _testRunnerProcess.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    GetAllErrorMessage(args, ref errorDataAll);

                    outputBuilder.AppendLine(args.Data);
                    _logger.Trace($"Begin - {args.Data}");


                    if (Regex.IsMatch(args.Data, "There is no connection.*"))
                    {
                        _logger.Trace("Found Error");
                        ExtentReport.LogErrorMessage($"<b><span style='color:#d9534f;'> There is no connection to the equipment </span></b>");
                    }
                    else if (args.Data.Contains("Passed!"))
                    {
                        _logger.Trace("Get test status Pass");

                        if (!string.IsNullOrEmpty(args.Data))
                        {
                            _logger.Info(args.Data);
                        }

                        testMethod.PassStatus++;
                        TestStatusResult = "Passed";
                        SelectedTestSuite.PassStatusTest++;
                        SelectedPassStatus++;
                        SelectedTestSuite.ResultColor = 1;
                        SelectedTestResultColor = SelectedTestSuite.ResultColor;
                        SelectedTestSuite.VisibilityPassResult = Visibility.Visible;
                        VisibilityPassedResult = Visibility.Visible;
                        ExtentReport.LogStatusPass(testMethod.TestMethodName + " Completed");
                    }
                    else if (args.Data.Contains("Failed!"))
                    {
                        _logger.Trace("Get test status Failed");
                                                
                        if (!string.IsNullOrEmpty(errorDataAll.ErrorAllDataMessage))
                        {
                            _logger.Error(errorDataAll.ErrorAllDataMessage);
                            string filePath = "logsApp/error/Error.log";
                            File.WriteAllText(filePath, errorDataAll.ErrorAllDataMessage);
                        }

                        testMethod.FailedStatus++;
                        TestStatusResult = "Failed";
                        SelectedTestSuite.FailedStatusTest++;
                        SelectedFailedStatus++;
                        SelectedTestSuite.ResultColor = 1;
                        SelectedTestResultColor = SelectedTestSuite.ResultColor;
                        SelectedTestSuite.VisibilityFailResult = Visibility.Visible;
                        VisibilityFailedResult = Visibility.Visible;
                        ExtentReport.LogStatusFail(errorDataAll, testMethod.TestMethodName + " Failed");
                    }
                    else if (args.Data.Contains("Skipped!"))
                    {
                        _logger.Trace("Get test status Skipped");

                        if (!string.IsNullOrEmpty(args.Data))
                        {
                            _logger.Info(args.Data);
                        }

                        testMethod.SkipStatus++;
                        TestStatusResult = "Skipped";
                        SelectedTestSuite.SkippedStatusTest++;
                        SelectedSkippedStatus++;
                        SelectedTestSuite.ResultColor = 1;
                        SelectedTestResultColor = SelectedTestSuite.ResultColor;
                        SelectedTestSuite.VisibilitySkipResult = Visibility.Visible;
                        VisibilitySkippedResult = Visibility.Visible;
                        ExtentReport.LogStatusSkip(testMethod.TestMethodName + "Skipped");
                    }
                }
            };

            _testRunnerProcess.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    outputBuilder.AppendLine(args.Data);
                }
            };

            _testRunnerProcess.Start();
            _testRunnerProcess.BeginOutputReadLine();
            _testRunnerProcess.BeginErrorReadLine();

            await _testRunnerProcess.WaitForExitAsync();

            ExtentReport.GetTestsStatus(TestStatusResult);
        }


        private void GetAllErrorMessage(DataReceivedEventArgs args, ref TestErrorInformation errorInformation)
        {
            string errorData = args.Data;
            if (errorData.Contains("Failed"))
            {
                errorInformation.ExistErrorAllDataMessage = true;
            }

            if (errorInformation.ExistErrorAllDataMessage)
            {
                errorInformation.ErrorAllDataMessage += errorData + Environment.NewLine;
            }

            GetErrorMessage(args, ref errorInformation);
            GetStackTraceMessage(args, ref errorInformation);
            GetStandardOutputMessage(args, ref errorInformation);
        }


        private void GetErrorMessage(DataReceivedEventArgs args, ref TestErrorInformation errorInformation)
        {
            string errorData = args.Data;
            if (errorData.Contains("Error Message"))
            {
                errorInformation.ExistErrorMessage = true;
            }
            if (errorData.Contains("Stack Trace"))
            {
                errorInformation.ExistErrorMessage = false;
            }

            if (errorInformation.ExistErrorMessage)
            {
                errorInformation.ErrorMessage += errorData + Environment.NewLine;
            }

            if (!string.IsNullOrEmpty(errorInformation.ErrorMessage))
            {
                string filePath = "logsApp/error/ErrorMessage.log";
                
                var fileDire = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(fileDire))
                {
                    Directory.CreateDirectory(fileDire);
                }

                File.WriteAllText(filePath, errorInformation.ErrorMessage);
            }
        }


        private void GetStackTraceMessage(DataReceivedEventArgs args, ref TestErrorInformation errorInformation)
        {
            string errorData = args.Data;
            if (errorData.Contains("Stack Trace"))
            {
                errorInformation.ExistStackTraceMessage = true;
            }
            if (errorData.Contains("Standard Output Messages"))
            {
                errorInformation.ExistStackTraceMessage = false;
            }

            if (errorInformation.ExistStackTraceMessage)
            {
                errorInformation.StackTraceMessage += errorData + Environment.NewLine;
            }

            if (!string.IsNullOrEmpty(errorInformation.StackTraceMessage))
            {
                string filePath = "logsApp/error/StackTrace.log";
                File.WriteAllText(filePath, errorInformation.StackTraceMessage);
            }
        }


        private void GetStandardOutputMessage(DataReceivedEventArgs args, ref TestErrorInformation errorInformation)
        {
            string errorData = args.Data;
            if (errorData.Contains("Standard Output Messages"))
            {
                errorInformation.ExistStandardOutputMessage = true;
                errorInformation.ExistStackTraceMessage = false;
            }

            if (errorInformation.ExistStandardOutputMessage)
            {
                errorInformation.StandardOutputMessage += errorData + Environment.NewLine;
            }

            if (!string.IsNullOrEmpty(errorInformation.StandardOutputMessage))
            {
                string filePath = "logsApp/error/StandardOutput.log";
                File.WriteAllText(filePath, errorInformation.StandardOutputMessage);
            }
        }

        public async Task WaitForExitAsync()
        {
            var tcs = new TaskCompletionSource<object>();
            _testRunnerProcess!.EnableRaisingEvents = true;
            _testRunnerProcess.Exited += (s, e) => tcs.SetResult(null);
            await tcs.Task;
        }


        [RelayCommand]
        private void StopAutoTest()
        {
            _logger.Trace("Stop AutoTest Command");
            if (_testRunnerProcess != null)
            {
                _stoppingToken.Cancel();
                _testRunnerProcess.Kill();
            }
        }

        [RelayCommand]
        private void OpenResultReport()
        {
            _logger.Trace("OpenResultReport Command");

            if (File.Exists(_htmlReportResult))
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo()
                {
                    FileName = _htmlReportResult,
                    UseShellExecute = true
                };
                Process.Start(processStartInfo);
            }
        }

        [RelayCommand]
        private void GroupExpanded()
        {
            AreAllGroupsExpanded = true;
            _logger.Trace($"All Groups Expanded - {AreAllGroupsExpanded}");
        }

        [RelayCommand]
        private void GroupCollapse()
        {
            AreAllGroupsExpanded = false;
            _logger.Trace($"All Groups Expanded - {AreAllGroupsExpanded}");
        }

        private void ResetTestsResult()
        {
            foreach (var testMethod in ObservedTestSuiteModel.ToList())
            {
                testMethod.IsRunning = true;
                testMethod.PassStatusTest = 0;
                testMethod.FailedStatusTest = 0;
                testMethod.ResultColor = 0;
                testMethod.VisibilityFailResult = Visibility.Collapsed;
                testMethod.VisibilityPassResult = Visibility.Collapsed;
                testMethod.VisibilitySkipResult = Visibility.Collapsed;
                foreach (var test in testMethod.TestMethodsModel.ToList())
                {
                    test.PassStatus = 0;
                    test.FailedStatus = 0;
                    test.SkipStatus = 0;
                    test.CurentCount = 0;
                }
            }

            SelectedTestResultColor = 0;
            SelectedPassStatus = 0;
            SelectedFailedStatus = 0;
            SelectedSkippedStatus = 0;
            VisibilityFailedResult = Visibility.Collapsed;
            VisibilityPassedResult = Visibility.Collapsed;
            VisibilitySkippedResult = Visibility.Collapsed;
            VisibilityReportButton = Visibility.Hidden;
            TimeStopTests = new();
        }

    }
}

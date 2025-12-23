using AutoTestRunner.Model;
using AutoTestRunner.Model.TestConfig;
using AutoTestRunner.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace AutoTestRunner.ViewModels
{
    public partial class TestItViewModel : ObservableObject
    {

        private readonly NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        [ObservableProperty]
        private string? _tmsRunnerPath;

        [ObservableProperty]
        private string? _testRunnerPath;

        [ObservableProperty]
        private string? _testAssemblyPath;

        [ObservableProperty]
        private string? _tmsUrl;

        [ObservableProperty]
        private string? _tmsPrivateToken;

        [ObservableProperty]
        private string? _tmsProjectId;

        [ObservableProperty]
        private string? _tmsConfigurationId;

        [ObservableProperty]
        private List<string> _tmsAdapterModeList;

        //[ObservableProperty]
        private string? _tmsAdapterMode;

        public string TmsAdapterMode
        {
            get => _tmsAdapterMode!;
            set
            {
                if(_tmsAdapterMode != null)
                {
                    if (value == "0" || value == "1")
                    {
                        UpdatePropWhenChangeAdapterMode();
                    }
                    else if (value == "2")
                    {
                        TmsTestRunId = null;
                        TmsLabelsOfTestsToRun = null;
                    }
                }                
                SetProperty(ref _tmsAdapterMode, value);
            }
        }

        [ObservableProperty]
        private string? _tmsTestRunId;

        [ObservableProperty]
        private string? _tmsLabelsOfTestsToRun;

        [ObservableProperty]
        private string? _tmsTestRunName;

        [ObservableProperty]
        public string? _logOutPut;

        [ObservableProperty]
        private string? _scriptForRun;

        private ProcessStartInfo? _testRunnerProcessStartInfo;
        private Process? _testRunnerProcess;

        [ObservableProperty]
        private IOptions<TmsSettings> _tmsSettings;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateScriptForRunCommand))]
        [NotifyCanExecuteChangedFor(nameof(EditScriptForRunCommand))]
        [NotifyPropertyChangedFor(nameof(CreateScriptCanExecute))]
        [NotifyPropertyChangedFor(nameof(EditScriptCanExecute))]
        public bool _isEnabledButtons;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartTestsCommand))]
        [NotifyCanExecuteChangedFor(nameof(StopTestRunnerCommand))]
        [NotifyPropertyChangedFor(nameof(StartCanExecute))]
        [NotifyPropertyChangedFor(nameof(StopCanExecute))]
        public bool _isEnabledStartButtons;

        [ObservableProperty]
        private CancellationTokenSource _stoppingToken;

        [ObservableProperty]
        private bool _tmsPropertiesIsEnabled = true;

        private readonly object _fileLock = new object();



        public TestItViewModel()
        {
            ConfigBuilder();

            UpdateTmsConfig();

            CreateScriptForRun();

            TmsAdapterModeList = new List<string> { "0", "1", "2" };
        }

        public bool StartCanExecute => IsEnabledStartButtons;

        public bool StopCanExecute => !IsEnabledStartButtons;

        public bool CreateScriptCanExecute => !IsEnabledButtons;

        public bool EditScriptCanExecute => IsEnabledButtons;



        private void ConfigBuilder()
        {
            _logger.Trace("Enter config builder");

            HostApplicationBuilder builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
            {
                Args = Environment.GetCommandLineArgs(),
                ContentRootPath = AppContext.BaseDirectory
            });

            builder.Configuration.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "tmsSettings.json"));
            builder.Services.Configure<TmsSettings>(builder.Configuration.GetSection("TmsSettings"));

            IServiceProvider serviceProvider = builder.Services.BuildServiceProvider();

            TmsSettings = serviceProvider.GetRequiredService<IOptions<TmsSettings>>();
        }


        [RelayCommand(CanExecute = nameof(StartCanExecute))]
        public async Task StartTests()
        {
            IsEnabledStartButtons = false;

            _stoppingToken = new();
            CancellationToken cancellationToken = _stoppingToken.Token;

            string logFilePath = "C:\\Test\\test_log.txt";
            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }

            LogOutPut = null;

            try
            {
                _logger.Info($"Running PowerShell script: \"{ScriptForRun}\"");

                _testRunnerProcessStartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{ScriptForRun}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (_testRunnerProcess = new Process())
                {
                    _testRunnerProcess.StartInfo = _testRunnerProcessStartInfo;

                    _testRunnerProcess.OutputDataReceived += (sender, args) => 
                    {
                        if (!string.IsNullOrEmpty(args.Data))
                        {
                            Console.WriteLine(args.Data);
                            _logger.Info(args.Data);

                            if (args.Data != null)
                            {
                                AppendLog(args.Data, logFilePath);
                            }
                        }
                    };

                    _testRunnerProcess.ErrorDataReceived += (sender, args) => 
                    { 
                        if (!string.IsNullOrEmpty(args.Data))
                        {
                            Console.WriteLine(args.Data);
                            _logger.Error(args.Data);

                            if (args.Data != null)
                            {
                                AppendLog(args.Data, logFilePath);
                            }
                        } 
                    };

                    _testRunnerProcess.Start();
                    _testRunnerProcess.BeginOutputReadLine();
                    _testRunnerProcess.BeginErrorReadLine();

                    

                    await _testRunnerProcess.WaitForExitAsync(cancellationToken);

                    if (File.Exists(logFilePath))
                    {
                        File.Delete(logFilePath);
                    }

                    IsEnabledStartButtons = true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while running the script: {ex.Message}");
            }
        }

        [RelayCommand(CanExecute = nameof(StopCanExecute))]
        private void StopTestRunner()
        {
            _logger.Trace("Stop AutoTest Command");
            if(_stoppingToken != null)
                _stoppingToken.Cancel();

            foreach (var process in Process.GetProcessesByName("TmsRunner"))
            {
                process.Kill();
                _testRunnerProcess.Kill();
            }

            IsEnabledStartButtons = true;
            IsEnabledButtons = true;
        }

        [RelayCommand(CanExecute = nameof(CreateScriptCanExecute))]
        private void CreateScriptForRun()
        {
            _logger.Trace("Create Script For Run");

            ScriptForRun = CreateRunnerScript(TmsAdapterMode!);

            IsEnabledStartButtons = true;
            IsEnabledButtons = true;
            TmsPropertiesIsEnabled = false;
        }

        [RelayCommand(CanExecute = nameof(EditScriptCanExecute))]
        private void EditScriptForRun()
        {
            IsEnabledStartButtons = false;
            IsEnabledButtons = false;
            TmsPropertiesIsEnabled = true;
        }


        private void AppendLog(string message, string logFilePath)
        {
            /// Если надо записывать лог, с добавлением новых данных сверху
            lock (_fileLock)
            {
                var existingLines = File.Exists(logFilePath) ? File.ReadAllLines(logFilePath).ToList() : new List<string>();

                existingLines.Insert(0, message);
                File.WriteAllLines(logFilePath, existingLines);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    LogOutPut = string.Join(Environment.NewLine, existingLines);
                });


                /// Если надо записывать лог, с добавлением новой строки

                //File.AppendAllText(logFilePath, message + Environment.NewLine);
                //Application.Current.Dispatcher.Invoke(() =>
                //{
                //    LogOutPut += message + Environment.NewLine;
                //});
            }
        }


        public void UpdateTmsConfig()
        {
            TmsRunnerPath = TmsSettings.Value.TmsRunner;
            TestRunnerPath = TmsSettings.Value.VsTestRunner;
            TestAssemblyPath = TmsSettings.Value.TestAssembly;
            TmsUrl = TmsSettings.Value.TmsUrl;
            TmsPrivateToken = TmsSettings.Value.TmsPrivateToken;
            TmsProjectId = TmsSettings.Value.TmsProjectId;
            TmsConfigurationId = TmsSettings.Value.TmsConfigurationId;
            TmsAdapterMode = TmsSettings.Value.TmsAdapterMode!;
            TmsTestRunId = TmsSettings.Value.TmsTestRunId;
            TmsLabelsOfTestsToRun = TmsSettings.Value.TmsLabelsOfTestsToRun;
            TmsTestRunName = TmsSettings.Value.TmsTestRunName;

            IsEnabledButtons = false;
        }

        public void UpdatePropWhenChangeAdapterMode()
        {
            TmsTestRunId = TmsSettings.Value.TmsTestRunId;
            TmsLabelsOfTestsToRun = TmsSettings.Value.TmsLabelsOfTestsToRun;
            TmsTestRunName = TmsSettings.Value.TmsTestRunName;
        }

        public string CreateRunnerScript(string adapterMode)
        {
            var script = string.Empty;

            if (adapterMode == "0")
            {
                script = $@"
                    {TmsRunnerPath} --runner '{TestRunnerPath}' --testassembly '{TestAssemblyPath}' --tmsUrl='{TmsUrl}' --tmsPrivateToken='{TmsPrivateToken}' --tmsProjectId='{TmsProjectId}' --tmsConfigurationId='{TmsConfigurationId}' --tmsAdapterMode='{TmsAdapterMode}' --tmsTestRunId='{TmsTestRunId}' --debug;
                    Remove-Item 'log*.txt' -Force -ErrorAction SilentlyContinue;
                    Write-Host 'Press Enter to continue...';
                ";
            }

            else if (adapterMode == "1")
            {
                script = $@"
                    {TmsRunnerPath} --runner '{TestRunnerPath}' --testassembly '{TestAssemblyPath}' --tmsUrl='{TmsUrl}' --tmsPrivateToken='{TmsPrivateToken}' --tmsProjectId='{TmsProjectId}' --tmsConfigurationId='{TmsConfigurationId}' --tmsAdapterMode='{TmsAdapterMode}' --tmsTestRunId='{TmsTestRunId}' --debug
                    Remove-Item 'log*.txt' -Force -ErrorAction SilentlyContinue;
                    Write-Host 'Press Enter to continue...';
                ";
            }

            else if (adapterMode == "2")
            {
                script = $@"
                    {TmsRunnerPath} --runner '{TestRunnerPath}' --testassembly '{TestAssemblyPath}' --tmsTestRunName={TmsTestRunName} 
                    Remove-Item 'log*.txt' -Force -ErrorAction SilentlyContinue;
                    Write-Host 'Press Enter to continue...';
                ";
            }

            return script;
        }

    }
}

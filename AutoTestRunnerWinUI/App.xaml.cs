using AutoTestRunnerWinUI.Models;
using AutoTestRunnerWinUI.Models.TestConfig;
using AutoTestRunnerWinUI.Services;
using AutoTestRunnerWinUI.Services.FileSettings;
using AutoTestRunnerWinUI.ViewModels;
using AutoTestRunnerWinUI.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using NLog.Extensions.Logging;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AutoTestRunnerWinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost _host;
        private readonly NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IThemeSelectorService _themeSelectorService;
        private readonly ILocalSettingsService _localSettingsService;
        private static WindowsBoundSettings _boundSettings = new();
        private static WindowSizeSelectorService _windowSizeSelectorService = new();
        public static Window MainWindow = new MainWindow();
        public TestConfigFile TestConfigFile = new();
        public static UIElement? AppTitlebar { get; set; }
        private UIElement? _shell = null;

        public static T GetService<T>()
        where T : class
        {
            if ((App.Current as App)!._host.Services.GetService(typeof(T)) is not T service)
            {
                throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
            }

            return service;
        }


        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            try
            {
                _logger.Trace("AutoTestRunner start to load");

                this.InitializeComponent();

                var hostBuilder = Host.CreateDefaultBuilder()
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        if (File.Exists(AppContext.BaseDirectory + "AutoTestsDll/UniExpertDll/uniExpertSettings.json"))
                        {
                            config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "AutoTestsDll/UniExpertDll/uniExpertSettings.json"));
                        }

                        //config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "AutoTestsDll/BagVisionDll/bagVisionSettings.json"));
                        //config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "AutoTestsDll/BatteryScanDll/batteryScanSettings.json"));
                        config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "AppSettings/navigationSettings.json"));
                        config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "reportSettings.json"));
                        config.AddEnvironmentVariables();
                    })
                    .ConfigureServices((context, services) =>
                    {
                        var configuration = context.Configuration;

                        services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
                        services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
                        services.AddSingleton<IActivationService, ActivationService>();
                        services.AddSingleton<IFileService, FileService>();

                        services.AddSingleton(this);
                        services.AddTransient<TestSuiteModel>();
                        services.AddTransient<TestMethodModel>();
                        services.AddTransient<TestConfigProperties>();
                        services.AddTransient<HtmlReport>();

                        services.AddTransient<UniExpertPage>();
                        services.AddTransient<AutoTestsViewModel>();
                        services.AddTransient<TestsConfigView>();
                        services.AddTransient<SettingPageViewModel>();
                        services.AddSingleton<SettingsPage>();
                        services.AddTransient<ShellPage>();
                        services.AddSingleton<ShellViewModel>();
                        services.AddTransient<WindowsBoundSettings>();

                        services.AddTransient<TestsWorkDirectory>();
                        services.AddTransient<TestsConstData>();
                        services.AddTransient<TestConfigFile>();
                        services.AddTransient<NavigationPages>();
                        services.AddTransient<ReportSettings>();

                        services.Configure<NavigationPages>(context.Configuration.GetSection("NavigationPages"));
                        services.Configure<ReportSettings>(context.Configuration.GetSection("ReportSettings"));
                        services.Configure<TestsWorkDirectory>("UniExpert", context.Configuration.GetSection("UniExpertSettings"));
                        //services.Configure<TestsWorkDirectory>("BagVision", context.Configuration.GetSection("BagVisionSettings"));
                        //services.Configure<TestsWorkDirectory>("BatteryScan", context.Configuration.GetSection("BatteryScanSettings"));
                        services.Configure<TestsConstData>(context.Configuration.GetSection("TestsConstData"));

                        var uniExpertOptions = configuration.GetSection("UniExpertSettings").Get<TestsWorkDirectory>();
                        if (uniExpertOptions != null)
                        {
                            var configBuilder = new ConfigurationBuilder();
                            configBuilder.AddJsonFile(Path.Combine(uniExpertOptions.PathToTestConfig, "testsData.json"));
                            var pathToConfig = uniExpertOptions.PathToTestConfig;

                            if (File.Exists(pathToConfig + "testsData.json"))
                            {
                                var newConfig = configBuilder.Build();

                                var testConstData = newConfig.GetSection("TestsConstData").Get<TestsConstData>();

                                var testConfigFile = new TestConfigFile
                                {
                                    TestsConstData = testConstData
                                };
                                services.AddSingleton(testConfigFile);
                            }
                        }                        
                    })
                    .ConfigureLogging((context, logging) =>
                    {
                        logging.ClearProviders();
                        logging.AddNLog();
                    });

                _host = hostBuilder.Build();

                _themeSelectorService = _host.Services.GetRequiredService<IThemeSelectorService>();
                _localSettingsService = _host.Services.GetRequiredService<ILocalSettingsService>();
                _windowSizeSelectorService.LoadWindowSizeFromSettingsAsync(_localSettingsService);

                App.MainWindow.Closed += (sender, args) => SaveWindowState();
                App.MainWindow.SizeChanged += (sender, args) => SizeWindowChanged();

                _logger.Trace("App is loaded");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected async override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

            await App.GetService<IActivationService>().ActivateAsync(args);
        }

        private void SaveWindowState()
        {
            _windowSizeSelectorService.SaveWindowSize(_localSettingsService);
        }

        private void SizeWindowChanged()
        {
            _windowSizeSelectorService.SaveSizeWhenResizing(_localSettingsService);
        }
    }
}

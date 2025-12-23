using AutoTestRunnerWinUI.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestRunnerWinUI.Services.FileSettings
{
    public class ActivationService(IThemeSelectorService themeSelectorService) : IActivationService
    {
        private readonly IThemeSelectorService _themeSelectorService = themeSelectorService;
        private UIElement? _shell = null;

        public async Task ActivateAsync(object activationArgs)
        {
            // Execute tasks before activation.
            await InitializeAsync();

            // Set the MainWindow Content.
            if (App.MainWindow.Content == null)
            {
                _shell = App.GetService<ShellPage>();
                App.MainWindow.Content = _shell ?? new Frame();
            }

            // Activate the MainWindow.
            App.MainWindow.Activate();

            // Execute tasks after activation.
            await StartupAsync();
        }

        private async Task InitializeAsync()
        {
            await _themeSelectorService.InitializeAsync().ConfigureAwait(false);
            await Task.CompletedTask;
        }

        private async Task StartupAsync()
        {
            await _themeSelectorService.SetRequestedThemeAsync();
            await Task.CompletedTask;
        }
    }
}

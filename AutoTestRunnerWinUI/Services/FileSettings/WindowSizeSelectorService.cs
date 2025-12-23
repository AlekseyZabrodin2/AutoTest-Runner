using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Graphics;

namespace AutoTestRunnerWinUI.Services.FileSettings
{
    public partial class WindowSizeSelectorService : ObservableObject
    {
        private static WindowsBoundSettings _boundSettings = new();

        [ObservableProperty]
        private int _currentWindowPositionX;

        [ObservableProperty]
        private int _currentWindowPositionY;

        [ObservableProperty]
        private int _currentWindowSizeWidth;

        [ObservableProperty]
        private int _currentWindowSizeHeight;

        [ObservableProperty]
        private string _currentWindowState;


        public async void LoadWindowSizeFromSettingsAsync(ILocalSettingsService localSettingsService)
        {
            var boundSettings = await localSettingsService.ReadSettingAsync<string>("WindowsBoundSettings");

            if (boundSettings != null)
            {
                _boundSettings = JsonSerializer.Deserialize<WindowsBoundSettings>(boundSettings);

                var positionX = _boundSettings.PositionWindowX;
                var positionY = _boundSettings.PositionWindowY;
                var sizeWidth = _boundSettings.SizeWindowWidth;
                var sizeHeight = _boundSettings.SizeWindowHeight;

                if (_boundSettings.StateWindow == "Maximized")
                {
                    App.MainWindow.AppWindow.MoveAndResize(new RectInt32(positionX, positionY, sizeWidth, sizeHeight));

                    var presenter = App.MainWindow.AppWindow.Presenter as OverlappedPresenter;
                    presenter.Maximize();
                }
                else
                {
                    App.MainWindow.AppWindow.MoveAndResize(new RectInt32(positionX, positionY, sizeWidth, sizeHeight));
                }
            }
        }

        public void SaveWindowSize(ILocalSettingsService localSettingsService)
        {
            var position = App.MainWindow.AppWindow.Position;
            var size = App.MainWindow.AppWindow.Size;

            var presenter = App.MainWindow.AppWindow.Presenter as OverlappedPresenter;
            var state = presenter.State;

            if (state.ToString() == "Maximized" || state.ToString() == "Minimized")
            {
                _boundSettings.PositionWindowX = CurrentWindowPositionX;
                _boundSettings.PositionWindowY = CurrentWindowPositionY;
                _boundSettings.SizeWindowWidth = CurrentWindowSizeWidth;
                _boundSettings.SizeWindowHeight = CurrentWindowSizeHeight;
                _boundSettings.StateWindow = state.ToString();

                localSettingsService.SaveSettingAsync("WindowsBoundSettings", _boundSettings);
                return;
            }

            _boundSettings.PositionWindowX = position.X;
            _boundSettings.PositionWindowY = position.Y;
            _boundSettings.SizeWindowWidth = size.Width;
            _boundSettings.SizeWindowHeight = size.Height;
            _boundSettings.StateWindow = state.ToString();

            localSettingsService.SaveSettingAsync("WindowsBoundSettings", _boundSettings);
        }

        public void SaveSizeWhenResizing(ILocalSettingsService localSettingsService)
        {
            var position = App.MainWindow.AppWindow.Position;
            var size = App.MainWindow.AppWindow.Size;

            var presenter = App.MainWindow.AppWindow.Presenter as OverlappedPresenter;
            var state = presenter.State;

            if (state.ToString() == "Maximized" || state.ToString() == "Minimized")
            {
                CurrentWindowState = state.ToString();
                return;
            }
            CurrentWindowPositionX = position.X;
            CurrentWindowPositionY = position.Y;
            CurrentWindowSizeWidth = size.Width;
            CurrentWindowSizeHeight = size.Height;
            CurrentWindowState = state.ToString();
        }
    }
}

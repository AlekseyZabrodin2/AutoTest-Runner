using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoTestRunnerWinUI.Services;
using AutoTestRunnerWinUI.Services.FileSettings;
using CommunityToolkit.WinUI;

namespace AutoTestRunnerWinUI.ViewModels
{
    public partial class SettingPageViewModel : ObservableObject
    {
        public enum ThemeModeEnum
        {
            Light,
            Dark,
            Use_System_Setting
        }

        private readonly IThemeSelectorService _themeSelectorService;

        [ObservableProperty]
        private Array _themeModes;

        [ObservableProperty]
        public ElementTheme _theme;

        [ObservableProperty]
        private ThemeModeEnum _selectedThemeMode;


        public SettingPageViewModel(IThemeSelectorService themeSelectorService)
        {
            ThemeModes = Enum.GetValues(typeof(ThemeModeEnum)).Cast<ThemeModeEnum>().ToArray();

            _themeSelectorService = themeSelectorService;
            Theme = _themeSelectorService.Theme;

            SwitchTheme(Theme);

            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            ChangeTheme(SelectedThemeMode);
        }

        public void ChangeTheme(ThemeModeEnum selectedTheme)
        {
            if (selectedTheme == ThemeModeEnum.Light)
            {
                SelectedThemeMode = ThemeModeEnum.Light;
                Theme = ElementTheme.Light;
            }
            else if (selectedTheme == ThemeModeEnum.Dark)
            {
                SelectedThemeMode = ThemeModeEnum.Dark;
                Theme = ElementTheme.Dark;
            }
            else
            {
                SelectedThemeMode = ThemeModeEnum.Use_System_Setting;
                Theme = ElementTheme.Default;
            }
            _themeSelectorService.SetThemeAsync(Theme);
        }

        private ThemeModeEnum SwitchTheme(ElementTheme param)
        {
            SelectedThemeMode = param switch
            {
                ElementTheme.Light => ThemeModeEnum.Light,
                ElementTheme.Dark => ThemeModeEnum.Dark,
                _ => ThemeModeEnum.Use_System_Setting
            };

            return SelectedThemeMode;            
        }
    }
}

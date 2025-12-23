using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestRunnerWinUI.Services.FileSettings
{
    public interface IThemeSelectorService
    {
        ElementTheme Theme
        {
            get;
        }

        Task InitializeAsync();

        Task SetThemeAsync(ElementTheme theme);

        Task SetRequestedThemeAsync();
    }
}

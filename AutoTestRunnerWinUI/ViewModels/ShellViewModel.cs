using AutoTestRunnerWinUI.Models;
using AutoTestRunnerWinUI.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AutoTestRunnerWinUI.ViewModels
{
    public partial class ShellViewModel : ObservableRecipient
    {
        [ObservableProperty]
        public string _selectedPage;

        [ObservableProperty]
        public string _pageTitle;

        [ObservableProperty]
        public string _pageTag;

        [ObservableProperty]
        public string _pagePathToDll;

        [ObservableProperty]
        public ObservableCollection<PageModel> _namePagesCollection;

        private readonly IServiceProvider _serviceProvider;


        public ShellViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            GetNavigationPages();
        }


        public void NavigationView(string pageTag, Frame frame)
        {

            SelectedPage = pageTag;

            if (pageTag == "Settings")
            {
                frame.Navigate(typeof(SettingsPage));
            }
            else
            {
                var currentPage = NamePagesCollection.FirstOrDefault(page => page.Tag == pageTag);
                PagePathToDll = currentPage.PathToDll;

                frame.Navigate(typeof(UniExpertPage));
            }
        }

        public void GetNavigationPages()
        {

            NamePagesCollection = new ObservableCollection<PageModel>();

            var navigationSettings = _serviceProvider.GetRequiredService<IOptions<NavigationPages>>();
            var navigationPage = navigationSettings.Value;

            foreach (var page in navigationPage)
            {
                var pageModel = page.Value;

                NamePagesCollection.Add(new PageModel
                {
                    Title = pageModel.Title,
                    Tag = $"{pageModel.Title}Page",
                    PathToDll = pageModel.PathToDll,
                });

                PageTitle = pageModel.Title;
                PageTag = $"{pageModel.Title}Page";
                PagePathToDll = pageModel.PathToDll;
            }
        }

    }
}

using AutoTestRunner.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AutoTestRunner.Views.Pages
{
    /// <summary>
    /// Interaction logic for AutoTestRunner.xaml
    /// </summary>
    public partial class UniExpertPage : Page
    {
        public UniExpertPage()
        {

            InitializeComponent();

            DataContext = new AutoTestsViewModel();
        }
    }
}

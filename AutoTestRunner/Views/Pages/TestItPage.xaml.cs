using System;
using AutoTestRunner.ViewModels;
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
    /// Interaction logic for TestIt.xaml
    /// </summary>
    public partial class TestItPage : Page
    {
        public TestItPage()
        {
            InitializeComponent();

            DataContext = new TestItViewModel();
        }
    }
}

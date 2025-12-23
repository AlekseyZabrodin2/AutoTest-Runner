using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestRunnerWinUI.Models
{
    public class TestTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TestSuiteTemplate { get; set; }
        public DataTemplate TestMethodTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is TestSuiteModel)
                return TestSuiteTemplate;
            if (item is TestMethodModel)
                return TestMethodTemplate;

            return base.SelectTemplateCore(item, container);
        }
    }
}

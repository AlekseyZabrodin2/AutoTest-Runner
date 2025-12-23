using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestRunnerWinUI.Models
{
    public partial class PageModel : ObservableObject
    {
        [ObservableProperty]
        public string _title;

        [ObservableProperty]
        public string _tag;

        [ObservableProperty]
        public string _pathToDll;
    }
}

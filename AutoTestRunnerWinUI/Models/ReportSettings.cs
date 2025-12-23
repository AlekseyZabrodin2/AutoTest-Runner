using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestRunnerWinUI.Models
{
    public partial class ReportSettings : ObservableObject 
    {

        [ObservableProperty]
        public string _extReportPath;

        [ObservableProperty]
        public string _oldNameFullPath;

        [ObservableProperty]
        public string _newNameFullPath;
    }
}

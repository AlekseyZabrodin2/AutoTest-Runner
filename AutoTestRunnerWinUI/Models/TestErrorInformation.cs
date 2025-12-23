using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestRunnerWinUI.Models
{
    public partial class TestErrorInformation : ObservableObject
    {
        [ObservableProperty]
        public string _errorAllDataMessage;

        [ObservableProperty]
        public bool _existErrorAllDataMessage;

        [ObservableProperty]
        public string _errorMessage;

        [ObservableProperty]
        public bool _existErrorMessage;

        [ObservableProperty]
        public string _stackTraceMessage;

        [ObservableProperty]
        public bool _existStackTraceMessage;

        [ObservableProperty]
        public string _standardOutputMessage;

        [ObservableProperty]
        public bool _existStandardOutputMessage;

    }
}

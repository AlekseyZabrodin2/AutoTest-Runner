using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestRunnerWinUI.Models
{
    public class TestsWorkDirectory
    {

        public string AssemblyPath { get; set; }

        public string PathToTestProject { get; set; }

        public string PathToTestConfig { get; set; }

        public string ExtReportPath { get; set; }

        public string OldNameFullPath { get; set; }

        public string NewNameFullPath { get; set; }

    }
}

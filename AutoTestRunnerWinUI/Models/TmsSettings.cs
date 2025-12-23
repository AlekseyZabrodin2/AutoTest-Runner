using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestRunnerWinUI.Models
{
    public class TmsSettings
    {

        public string? TmsRunner { get; set; }

        public string? VsTestRunner { get; set; }

        public string? TestAssembly { get; set; }

        public string? TmsUrl { get; set; }

        public string? TmsPrivateToken { get; set; }

        public string? TmsProjectId { get; set; }

        public string? TmsConfigurationId { get; set; }

        public string? TmsAdapterMode { get; set; }

        public string? TmsTestRunId { get; set; }

        public string? TmsTestRunName { get; set; }

        public string? TmsAutomaticCreationTestCases { get; set; }

        public string? TmsCertValidation { get; set; }

        public string? TmsLabelsOfTestsToRun { get; set; }


    }
}

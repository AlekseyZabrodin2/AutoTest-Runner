using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestRunnerWinUI.Models.TestConfig
{
    public class PropertiesFromConfigFile
    {
        public string? TestClass { get; set; }

        public string? TestName { get; set; }

        public int? Gender { get; set; }

        public string? LastName { get; set; }

        public string? FirstName { get; set; }

        public string? MiddleName { get; set; }

        public string? Address { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Comments { get; set; }

        public int? RepeatNumber { get; set; }

        public bool? ExpectedResult { get; set; }

    }
}

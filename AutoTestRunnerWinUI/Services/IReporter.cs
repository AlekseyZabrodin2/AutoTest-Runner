using AutoTestRunnerWinUI.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestRunnerWinUI.Services
{
    internal interface IReporter
    {

        public void ReportLogger(string testCaseName, IOptions<ReportSettings> options);

        public void InitializeTests(string testContext, string tag);

        public void CreateTest(string testContext);

        public void CreateNode(string node);

        public void CreateTags(string tags);

        public void LogStatusPass(string status);

        public void LogStatusFail(TestErrorInformation errorInformation, string status);

        public void LogStatusSkip(string status);

        public void GetTestsStatus(string testContext);

        public void CreateReport();

    }
}

using AventStack.ExtentReports.MarkupUtils;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoTestRunnerWinUI.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.CodeAnalysis;

namespace AutoTestRunnerWinUI.Services
{
    public partial class HtmlReport : ObservableObject, IReporter
    {
        private readonly NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IServiceProvider _serviceProvider;
        public static ExtentSparkReporter _sparkReporter;
        public static TestContext _testContext;
        public static ExtentReports _report;
        public static ExtentTest _parentTest;
        public static ExtentTest _childTest;

        [ObservableProperty]
        private DateTime _timeTestStart;

        [ObservableProperty]
        private DateTime _timeTestStop;

        public TestContext TestContext { get; set; }


        public HtmlReport(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        public void ReportLogger(string testCaseName, IOptions<ReportSettings> operation)
        {
            _logger.Trace("Enter in ReportLogger");

            var extReportPath = operation.Value.ExtReportPath;

            _sparkReporter = new ExtentSparkReporter(extReportPath);

            var fileDire = Path.GetDirectoryName(extReportPath);
            _logger.Trace($"CreateDirectory {fileDire}");
            if (!Directory.Exists(fileDire))
            {
                Directory.CreateDirectory(fileDire);
            }

            _sparkReporter.Config.DocumentTitle = "UNIEXPERT Test Results Report";
            _sparkReporter.Config.ReportName = "Regression Testing";
            _sparkReporter.Config.Theme = AventStack.ExtentReports.Reporter.Config.Theme.Dark;
            _sparkReporter.Config.TimelineEnabled = false;
            _sparkReporter.Config.TimeStampFormat = "HH:mm:ss";

            _report = new ExtentReports();
            _report.AttachReporter(_sparkReporter);

            _report.AddSystemInfo("Environment", "QA");
            _report.AddSystemInfo("Machine Name", Environment.MachineName);
            _report.AddSystemInfo("User Domain-Name", System.Security.Principal.WindowsIdentity.GetCurrent().Name);
            _report.AddSystemInfo("OS", Environment.OSVersion.VersionString);

            //DateTime timeStart = _report.ReportStartDateTime;
            TimeTestStart = DateTime.Now;
            _report.AddSystemInfo("Start Time Project", TimeTestStart.ToString());

            _logger.Trace("Exit from ReportLogger");
        }


        public void InitializeTests(string testContext, string tag)
        {
            CreateTest(testContext);
            CreateTags(tag);
        }


        public void CreateTest(string testContext)
        {
            _parentTest = _report.CreateTest(testContext);

            //childTest = parentTest.CreateNode("Create Test Node"); // Node can be implemented in another version

            _parentTest.Log(Status.Info, $"Test Start - [ {testContext} ]");

            _parentTest.Log(Status.Info, "Start Test Time");
        }


        public void CreateNode(string node)
        {
            _parentTest.CreateNode(node);
        }


        public void CreateTags(string tags)
        {
            _parentTest.AssignCategory(tags);
        }

        public void LogErrorMessage(string message)
        {
            _parentTest.Log(Status.Error, message);
        }


        public void LogStatusPass(string testName)
        {
            _logger.Trace("Enter in Log Pass Status");

            string pass = "Pass";
            _parentTest.Log(Status.Pass, MarkupHelper.CreateLabel(pass.ToUpperInvariant(), ExtentColor.Green));

            _childTest = _parentTest.CreateNode("Status Pass Node");

            _childTest.Log(Status.Pass, testName);

            _logger.Trace("Exit from Log Pass Status");
        }


        public void LogStatusFail(TestErrorInformation errorInformation, string testName)
        {
            _logger.Trace("Enter in Log Fail Status");

            string faild = "Failed";

            _parentTest.Log(Status.Fail, MarkupHelper.CreateLabel(faild.ToUpperInvariant(), ExtentColor.Red));

            var error = GetTypeException(errorInformation.ErrorMessage);

            _parentTest.Fail(error);

            //_parentTest.Log(Status.Error, $"Failed Exception: - [ <b>{errorInformation.ErrorMessage}</b> ]");

            _childTest = _parentTest.CreateNode("Exception Details");

            _childTest.Log(Status.Fail, testName);

            _childTest.Warning((Status.Warning, errorInformation.StackTraceMessage).ToString());

            _childTest.Log(Status.Warning, errorInformation.StandardOutputMessage);

            _logger.Trace("Exit from Log Fail Status");
        }


        public void LogStatusSkip(string testName)
        {
            _logger.Trace("Enter in Log Skip Status");

            string skip = "Skip";
            _parentTest.Log(Status.Skip, MarkupHelper.CreateLabel(skip.ToUpperInvariant(), ExtentColor.Orange));

            _childTest = _parentTest.CreateNode("Status Skip Node");

            _childTest.Log(Status.Skip, testName);

            _logger.Trace("Exit from Log Skip Status");
        }


        public void GetTestsStatus(string testContext)
        {

            _logger.Trace("Enter in Get tests Status");

            var status = testContext;
            Status logstatus;

            switch (status)
            {
                case "Failed":
                    logstatus = Status.Fail;
                    break;
                case "Passed":
                    logstatus = Status.Pass;
                    break;
                case "Skipped":
                    logstatus = Status.Skip;
                    break;
                case "NotRunnable":
                    logstatus = Status.Error;
                    break;
                //case "Timeout":
                //    logstatus = Status.Fatal;
                //break;
                case "Unknown":
                    logstatus = Status.Warning;
                    break;
                default:
                    logstatus = Status.Error;
                    break;
            }

            _parentTest.Log(logstatus, $"Test ended with status - [ <b>{logstatus}</b> ]");

            //childTest = parentTest.CreateNode("Create Test Report Node"); // Node can be implemented in another version

            _parentTest.Log(Status.Info, "End Test Time");

            _logger.Trace($"Exit from Get tests Status whith status - {logstatus}");
        }

        public void CreateReport()
        {
            _logger.Trace("Enter in Create Report");

            try
            {
                //DateTime timeEnd = _report.ReportEndDateTime.ToLocalTime();
                TimeTestStop = DateTime.Now;
                _report.AddSystemInfo("End Time Project", TimeTestStop.ToString());

                _logger.Trace("Before Flush");
                _report.Flush();
                _logger.Trace("After Flush");
            }
            catch (ArgumentException ex)
            {
                _logger.Error($"ArgumentException during report flush: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception during report flush: {ex.Message}");
            }

            _logger.Trace("Exit from report");
        }

        private Exception GetTypeException(string exceptionMessage)
        {
            var exceptionType = new Exception();

            if (Regex.IsMatch(exceptionMessage, @"Assert\..*"))
            {
                exceptionType = new AssertFailedException(exceptionMessage);
            }
            else if (exceptionMessage.Contains("NullReference"))
            {
                exceptionType = new NullReferenceException(exceptionMessage);
            }
            else if (exceptionMessage.Contains("IndexOutOfRange"))
            {
                exceptionType = new IndexOutOfRangeException(exceptionMessage);
            }
            else if (exceptionMessage.Contains("InvalidOperation"))
            {
                exceptionType = new InvalidOperationException(exceptionMessage);
            }
            else if (exceptionMessage.Contains("ArgumentNullException"))
            {
                exceptionType = new ArgumentNullException(exceptionMessage);
            }
            else if (exceptionMessage.Contains("ArgumentOutOfRangeException"))
            {
                exceptionType = new ArgumentOutOfRangeException(exceptionMessage);
            }
            else if (exceptionMessage.Contains("ArgumentException"))
            {
                exceptionType = new ArgumentException(exceptionMessage);
            }
            else if (exceptionMessage.Contains("FileNotFoundException"))
            {
                exceptionType = new FileNotFoundException(exceptionMessage);
            }
            else if (exceptionMessage.Contains("FormatException"))
            {
                exceptionType = new FormatException(exceptionMessage);
            }
            else if (exceptionMessage.Contains("TimeoutException"))
            {
                exceptionType = new TimeoutException(exceptionMessage);
            }
            else if (exceptionMessage.Contains("UnauthorizedAccessException"))
            {
                exceptionType = new UnauthorizedAccessException(exceptionMessage);
            }
            else if (exceptionMessage.Contains("IOException"))
            {
                exceptionType = new IOException(exceptionMessage);
            }
            else
            {
                exceptionType = new Exception(exceptionMessage);
            }

            return exceptionType;
        }


    }
}

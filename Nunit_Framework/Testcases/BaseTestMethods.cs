using NUnit.Framework;
using Nunit_Framework.PageActions;
using NUnit.Framework.Interfaces;
using RelevantCodes.ExtentReports;
using System;
using System.IO;
using OpenQA.Selenium;
using System.Drawing;

namespace Nunit_Framework.Testcases
{
    [TestFixture]
    public class BaseTestMethods
    {
        protected ExtentReports extentReports;
        protected ExtentReports extentReportSummary;
        protected static ExtentTest test;
        protected static ExtentTest testSummary;
        static String driverDirectory = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).Parent.Parent.Parent.FullName;
        protected String resultDirectory = Path.Combine(driverDirectory, "TestResults");
        protected String reportName, screenshotName, reportSummaryName;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            string resultSummaryDirectory = Path.Combine(resultDirectory, "ReportsSummary", DateTime.Now.ToString("yyyy-MM-dd"));
            reportSummaryName = TestContext.CurrentContext.Test.Name + "_" + DateTime.Now.ToString("yyyyMMdd_HH.mm.ss") + ".html";
            //reportSummaryName = "test.html";
            if (!Directory.Exists(resultSummaryDirectory))
            {
                Directory.CreateDirectory(resultSummaryDirectory);
            }
            extentReportSummary = new ExtentReports(resultSummaryDirectory + "\\" + reportSummaryName, true);
            testSummary = extentReportSummary.StartTest(TestContext.CurrentContext.Test.Name, "Report summary");
            Pages.LoginPage.LaunchSite();
        }

        [TearDown]
        public void TearDown()
        {
            CreateReportResult();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            extentReportSummary.EndTest(testSummary);
            extentReportSummary.Flush();
            BrowserManager.DeleteAllCookies();
            BrowserManager.CloseBrowser();
        }

        protected void StartTest()
        {
            resultDirectory = Path.Combine(resultDirectory, DateTime.Now.ToString("yyyy-MM-dd"), TestContext.CurrentContext.Test.MethodName);
            if (!Directory.Exists(resultDirectory))
            {
                Directory.CreateDirectory(resultDirectory);
            }
            resultDirectory += "\\";
            reportName = TestContext.CurrentContext.Test.MethodName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".html";
            screenshotName = TestContext.CurrentContext.Test.MethodName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";

            extentReports = new ExtentReports(resultDirectory + reportName, true);
            test = extentReports.StartTest(TestContext.CurrentContext.Test.MethodName, "");

            //change report config
            string path = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).Parent.Parent.FullName;
            extentReports.LoadConfig(path + @"\TestData\extent-config.xml");
            extentReportSummary.LoadConfig(path + @"\TestData\extent-config.xml");
        }

        protected void CreateReportResult()
        {
            var status = TestContext.CurrentContext.Result.Outcome.Status;
            var stacktrace = string.IsNullOrEmpty(TestContext.CurrentContext.Result.StackTrace)
                    ? ""
                    : string.Format("<pre>{0}</pre>", TestContext.CurrentContext.Result.StackTrace);
            LogStatus logstatus;

            switch (status)
            {
                case TestStatus.Failed:
                    logstatus = LogStatus.Fail;
                    break;
                case TestStatus.Skipped:
                    logstatus = LogStatus.Skip;
                    break;
                case TestStatus.Inconclusive:
                    logstatus = LogStatus.Unknown;
                    break;
                default:
                    logstatus = LogStatus.Pass;
                    break;
            }

            test.Log(logstatus, "Test ended, result is: " + logstatus + stacktrace);
            if (logstatus == LogStatus.Fail)
            {
                TakeScreenshot(resultDirectory, screenshotName);
                test.Log(LogStatus.Info, "Snapshot below: " + test.AddScreenCapture(resultDirectory + "\\" + screenshotName));
            }
            TestContext.Write("Results has been saved in: " + resultDirectory);
            extentReports.Flush();
            testSummary.AppendChild(test);
            extentReports.EndTest(test);

            resultDirectory = Path.Combine(driverDirectory, "TestResults");
        }

        protected void TakeScreenshot(String resultDirectory, String screenshotName)
        {
            try
            {
                if (!Directory.Exists(resultDirectory))
                    Directory.CreateDirectory(resultDirectory);

                ITakesScreenshot takesScreenshot = BrowserManager.Browser as ITakesScreenshot;

                if (takesScreenshot != null)
                {
                    var screenshot = takesScreenshot.GetScreenshot();
                    string screenshotFilePath = Path.Combine(resultDirectory, screenshotName);
                    screenshot.SaveAsFile(screenshotFilePath, System.Drawing.Imaging.ImageFormat.Png);
                    Console.WriteLine("Screenshot: {0}", new Uri(screenshotFilePath));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while taking screenshot: {0}", ex);

            }
        }

        protected void stepLogging(String logMessage)
        {
            test.Log(LogStatus.Info, "Test step: " + logMessage);
        }
    }
}

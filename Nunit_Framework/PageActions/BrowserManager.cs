using Fenton.Selenium.SuperDriver;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Nunit_Framework.PageActions
{
    public class BrowserManager
    {
        private static readonly Dictionary<String, IWebDriver> context = new Dictionary<String, IWebDriver>();
        public static IWebDriver Browser
        {
            get
            {
                var uri = new Uri(ConfigurationManager.AppSettings["SeleniumHubUrl"]);
                
                string appbrowser = ConfigurationManager.AppSettings["AppBrowser"];
                string grid = ConfigurationManager.AppSettings["grid"];
                if (grid.Equals("yes"))
                {
                    switch (appbrowser)
                    {
                        case "Parallel":
                            {
                                if (context.Count == 0)
                                    context.Add("Driver", new SuperWebDriver(GetDriverSuiteGrid()));
                                break;
                            }
                        case "ie":
                            {
                                if (context.Count == 0)
                                    context.Add("Driver", new RemoteWebDriver(uri, DesiredCapabilities.InternetExplorer()));
                                break;
                            }
                        case "Chrome":
                            {
                                if (context.Count == 0)
                                    context.Add("Driver", new RemoteWebDriver(uri, DesiredCapabilities.Chrome()));
                                break;
                            }
                        default:
                            {
                                if (context.Count == 0)
                                    context.Add("Driver", new RemoteWebDriver(uri, DesiredCapabilities.Firefox()));
                                break;
                            }
                    }
                    return context["Driver"];
                }
                else
                {
                    switch (appbrowser)
                    {
                        case "Parallel":
                            {
                                if (context.Count == 0)
                                    context.Add("Driver", new SuperWebDriver(GetDriverSuiteNonGrid()));
                                break;
                            }
                        case "ie":
                            {
                                if (context.Count == 0)
                                    context.Add("Driver", (IWebDriver)new InternetExplorerDriver());
                                break;
                            }
                        case "Chrome":
                            {
                                if (context.Count == 0)
                                    context.Add("Driver", (IWebDriver)new ChromeDriver());
                                break;
                            }
                        default:
                            {
                                if (context.Count == 0)
                                    context.Add("Driver", (IWebDriver)new FirefoxDriver());
                                break;
                            }
                    }
                    return context["Driver"];
                }
            }
        }
        private static IList<IWebDriver> GetDriverSuiteGrid()
        {
            var uri = new Uri(ConfigurationManager.AppSettings["SeleniumHubUrl"]);
            // Allow some degree of parallelism when creating drivers, which can be slow
            IList<IWebDriver> drivers = new List<Func<IWebDriver>>
            {
                () =>  { return new RemoteWebDriver(uri, DesiredCapabilities.Chrome()); },
                () =>  { return new RemoteWebDriver(uri, DesiredCapabilities.Firefox()); },
                () =>  { return new RemoteWebDriver(uri, DesiredCapabilities.InternetExplorer()); },
            }.AsParallel().Select(d => d()).ToList();
            return drivers;
        }
        private static IList<IWebDriver> GetDriverSuiteNonGrid()
        {
            IList<IWebDriver> drivers = new List<Func<IWebDriver>>
            {
                () =>  { return (IWebDriver)new ChromeDriver(); },
                () =>  { return (IWebDriver)new FirefoxDriver(); },
                () =>  { return (IWebDriver)new InternetExplorerDriver(); },
            }.AsParallel().Select(d => d()).ToList();
            return drivers;
        }

        public static void GoToUrl(string url)
        {
            Browser.Navigate().GoToUrl(url);
        }

        public static void MaximizeWindow()
        {
            Browser.Manage().Window.Maximize();
        }

        public static void DeleteAllCookies()
        {
            Browser.Manage().Cookies.DeleteAllCookies();
        }

        public static void CloseBrowser()
        {
            Browser.Close();
            context.Clear();
        }
    }
}

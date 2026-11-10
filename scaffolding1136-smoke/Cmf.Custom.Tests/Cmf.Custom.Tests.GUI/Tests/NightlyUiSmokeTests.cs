using System;
using Cmf.Custom.Tests.Biz.Common;
using Cmf.Custom.Tests.GUI.PageObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace Cmf.Custom.Tests.GUI.Tests
{
    /// <summary>Browser smoke tests that do not modify MES objects or user roles.</summary>
    [TestClass]
    public class NightlyUiSmokeTests
    {
        private IWebDriver driver;

        private void OpenBrowser()
        {
            var options = new ChromeOptions();
            options.AddArguments("--headless=new", "--no-sandbox", "--disable-dev-shm-usage",
                "--window-size=1440,1000");
            var binary = Environment.GetEnvironmentVariable("CMF_CHROME_BINARY");
            if (!string.IsNullOrWhiteSpace(binary))
            {
                options.BinaryLocation = binary;
            }

            var remote = Environment.GetEnvironmentVariable("CMF_WEBDRIVER_URL");
            if (!string.IsNullOrWhiteSpace(remote))
            {
                driver = new RemoteWebDriver(new Uri(remote), options.ToCapabilities(),
                    TimeSpan.FromSeconds(60));
            }
            else
            {
                var driverDirectory = Environment.GetEnvironmentVariable("CMF_CHROMEDRIVER_DIRECTORY");
                var service = string.IsNullOrWhiteSpace(driverDirectory)
                    ? ChromeDriverService.CreateDefaultService()
                    : ChromeDriverService.CreateDefaultService(driverDirectory);
                driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(60));
            }
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);
            driver.Navigate().GoToUrl(SmokeSettings.Website);
        }

        /// <summary>Always closes the browser, including failed and inconclusive tests.</summary>
        [TestCleanup]
        public void Cleanup()
        {
            driver?.Quit();
            driver?.Dispose();
            driver = null;
        }

        /// <summary>Checks the actual login form using a CMF page object; no credentials are required.</summary>
        [TestMethod]
        [TestCategory("NightlySmoke")]
        [TestCategory("UiPublic")]
        [Timeout(120000)]
        public void LoginPage_ShowsEnabledAccountForm()
        {
            OpenBrowser();
            var page = new NightlyLoginPage(driver);
            Assert.IsTrue(page.AccountInput.Displayed);
            Assert.IsTrue(page.AccountInput.Enabled);
            Assert.IsTrue(page.SubmitButton.Enabled);
            Assert.AreEqual(SmokeSettings.Website.Host, new Uri(driver.Url).Host);
        }

        /// <summary>Signs in and waits for the MES shell rather than the authentication page.</summary>
        [TestMethod]
        [TestCategory("NightlySmoke")]
        [TestCategory("UiAuthenticated")]
        [Timeout(180000)]
        public void SignIn_OpensMesShell()
        {
            SignIn();
            Assert.IsTrue(WaitForMesShell());
        }

        /// <summary>Checks that refreshing an authenticated page preserves the MES session.</summary>
        [TestMethod]
        [TestCategory("NightlySmoke")]
        [TestCategory("UiAuthenticated")]
        [Timeout(180000)]
        public void Refresh_PreservesAuthenticatedSession()
        {
            SignIn();
            Assert.IsTrue(WaitForMesShell());
            driver.Navigate().Refresh();
            Assert.IsTrue(WaitForMesShell());
        }

        private void SignIn()
        {
            var userName = SmokeSettings.Required("CMF_TEST_USERNAME");
            var password = SmokeSettings.Required("CMF_TEST_PASSWORD");
            OpenBrowser();
            new NightlyLoginPage(driver).SignIn(userName, password, SmokeSettings.Website.Host);
        }

        private bool WaitForMesShell()
        {
            var selector = Environment.GetEnvironmentVariable("CMF_TEST_SHELL_SELECTOR")
                ?? "cmf-core-shell-shell";
            return new WebDriverWait(driver, TimeSpan.FromSeconds(60)).Until(browser =>
            {
                var current = new Uri(browser.Url);
                if (!string.Equals(current.Host, SmokeSettings.Website.Host,
                        StringComparison.OrdinalIgnoreCase)
                    || current.AbsolutePath.IndexOf("/SecurityPortal",
                        StringComparison.OrdinalIgnoreCase) >= 0
                    || browser.FindElements(By.CssSelector("input[name='username']")).Count > 0)
                {
                    return false;
                }
                foreach (var shell in browser.FindElements(By.CssSelector(selector)))
                {
                    if (shell.Displayed)
                    {
                        return true;
                    }
                }
                return false;
            });
        }
    }
}

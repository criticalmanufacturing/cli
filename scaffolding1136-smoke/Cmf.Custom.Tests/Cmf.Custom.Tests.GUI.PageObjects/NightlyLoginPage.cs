using System;
using Cmf.Core.PageObjects;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Cmf.Custom.Tests.GUI.PageObjects
{
    /// <summary>A small CMF page object for the nightly Security Portal login form.</summary>
    public sealed class NightlyLoginPage : BasePageObject
    {
        /// <summary>Creates the page object for the current document.</summary>
        /// <param name="driver">The test's browser session.</param>
        public NightlyLoginPage(IWebDriver driver)
            : base(driver, driver.FindElement(By.TagName("body")))
        {
        }

        /// <summary>Gets the account input on the first login step.</summary>
        public IWebElement AccountInput =>
            WaitFor(By.CssSelector("input[name='username']"), Driver);

        /// <summary>Gets the submit button for the visible login form.</summary>
        public IWebElement SubmitButton =>
            WaitFor(By.CssSelector("form button"), Driver);

        /// <summary>Submits the account name and then the password on the next step.</summary>
        /// <param name="userName">The account used only for this browser session.</param>
        /// <param name="password">Password supplied by the process environment.</param>
        /// <param name="expectedHost">The approved Security Portal hostname.</param>
        public void SignIn(string userName, string password, string expectedHost)
        {
            RequireExpectedHost(expectedHost);
            AccountInput.Clear();
            AccountInput.SendKeys(userName);
            SubmitButton.Click();

            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(45));
            var passwordInput = wait.Until(driver =>
            {
                foreach (var element in driver.FindElements(By.CssSelector("input[type='password']")))
                {
                    if (element.Displayed && element.Enabled)
                    {
                        return element;
                    }
                }
                return null;
            });
            RequireExpectedHost(expectedHost);
            passwordInput.SendKeys(password);
            Driver.FindElement(By.CssSelector("form button")).Click();

            // Some Security Portal accounts receive an optional device-enrollment step.
            // Decline it so this disposable browser does not register a trusted device.
            wait.Until(driver =>
            {
                RequireExpectedHost(expectedHost);
                var path = new Uri(driver.Url).AbsolutePath;
                if (path.IndexOf("/login/registerDevice", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    foreach (var button in driver.FindElements(By.XPath(
                        "//button[normalize-space()='No, thanks'] | //a[normalize-space()='No, thanks']")))
                    {
                        if (button.Displayed && button.Enabled)
                        {
                            button.Click();
                            return true;
                        }
                    }
                    return false;
                }
                return path.IndexOf("/SecurityPortal", StringComparison.OrdinalIgnoreCase) < 0;
            });
        }

        private void RequireExpectedHost(string expectedHost)
        {
            var current = new Uri(Driver.Url);
            if (current.Scheme != Uri.UriSchemeHttps
                || !string.Equals(current.Host, expectedHost, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "The login page is not on the expected HTTPS host; credentials were not submitted.");
            }
        }
    }
}

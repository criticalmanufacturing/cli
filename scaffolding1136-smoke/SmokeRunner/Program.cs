using System.Diagnostics;
using Cmf.Custom.Tests.Biz.Common;
using Cmf.Custom.Tests.GUI.PageObjects;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Text.Json;

var userName = SmokeSettings.Required("CMF_TEST_USERNAME");
var password = SmokeSettings.Required("CMF_TEST_PASSWORD");
_ = SmokeSettings.Required("CMF_TEST_TENANT");

var options = new ChromeOptions();
options.AddArguments("--headless=new", "--no-sandbox", "--disable-dev-shm-usage",
    "--window-size=1440,1000");
var binary = Environment.GetEnvironmentVariable("CMF_CHROME_BINARY");
if (!string.IsNullOrWhiteSpace(binary))
{
    options.BinaryLocation = binary;
}
var driverDirectory = Environment.GetEnvironmentVariable("CMF_CHROMEDRIVER_DIRECTORY")
    ?? Environment.CurrentDirectory + "/Cmf.Custom.Tests/Release";

using var driver = new ChromeDriver(
    ChromeDriverService.CreateDefaultService(driverDirectory), options);
try
{
    driver.Navigate().GoToUrl(SmokeSettings.Website);
    new NightlyLoginPage(driver).SignIn(userName, password, SmokeSettings.Website.Host);
    new WebDriverWait(driver, TimeSpan.FromSeconds(60)).Until(browser =>
        browser.FindElements(OpenQA.Selenium.By.CssSelector("cmf-mes-app-root")).Count > 0);
    var token = new WebDriverWait(driver, TimeSpan.FromSeconds(60)).Until(browser =>
        browser.Manage().Cookies.GetCookieNamed("access_token")?.Value);

    Console.WriteLine("Observed service paths: " + driver.ExecuteScript(
        "return JSON.stringify([...new Set(performance.getEntriesByType('resource').filter(x => ['fetch','xmlhttprequest'].includes(x.initiatorType)).map(x=>new URL(x.name).pathname))].slice(0,20));"));
    var plainToken = Uri.UnescapeDataString(token);
    var parts = plainToken.Split('.');
    Console.WriteLine("Session token has JWT format: " + (parts.Length == 3));
    if (parts.Length == 3)
    {
        var payload = parts[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight((payload.Length + 3) / 4 * 4, '=');
        using var claims = JsonDocument.Parse(Convert.FromBase64String(payload));
        if (claims.RootElement.TryGetProperty("aud", out var audience))
        {
            Console.WriteLine("Token audience: " + audience);
        }
        if (claims.RootElement.TryGetProperty("exp", out var expiration))
        {
            Console.WriteLine("Token expires in seconds: " +
                (expiration.GetInt64() - DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
        }
    }
    Console.WriteLine("Authenticated browser session established; running read-only Host tests.");
    var start = new ProcessStartInfo("dotnet") { UseShellExecute = false };
    foreach (var argument in new[]
    {
        "test", "Cmf.Custom.Tests/Cmf.Custom.Tests.Biz/Cmf.Custom.Tests.Biz.csproj",
        "--configuration", "Release", "--no-build", "--no-restore",
        "--settings", "Cmf.Custom.Tests/smoke.runsettings",
        "--filter", "TestCategory=Host",
        "--results-directory", "Cmf.Custom.Tests/TestResults",
        "--logger", "trx;LogFileName=host-authenticated.trx"
    })
    {
        start.ArgumentList.Add(argument);
    }
    start.Environment["CMF_TEST_ACCESS_TOKEN"] = plainToken;
    using var process = Process.Start(start)
        ?? throw new InvalidOperationException("The Host test runner did not start.");
    await process.WaitForExitAsync();
    return process.ExitCode;
}
finally
{
    driver.Quit();
}

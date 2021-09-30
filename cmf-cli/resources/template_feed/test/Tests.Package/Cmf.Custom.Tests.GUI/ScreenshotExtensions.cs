using Cmf.Core.PageObjects;
using OpenQA.Selenium;
using System;
using System.IO;

namespace Cmf.Custom.Tests.GUI
{
    /// <summary>
    /// 
    /// </summary>
    public static class ScreenshotExtensions
    {
        /// <summary>
        /// Take screenshot
        /// </summary>
        /// <param name="testClass"></param>
        /// <param name="step"></param>
        public static void TakeScreenshot(this BaseTestClass testClass, string step)
        {
            Screenshot screenshot = (BaseTestClass.Driver as ITakesScreenshot).GetScreenshot();

            // Get target file path
            string filePath = GenerateResourceFilePath(testClass, "png", "documentationScreenshotPath", step);

            // Save the screenshot
            screenshot.SaveAsFile(filePath, ScreenshotImageFormat.Png);
        }

        private static string GenerateResourceFilePath(BaseTestClass testClass, string extension, string contextPathProperty = null, string fileName = null)
        {
            // Generate the path
            string path = GenerateResourcePath(testClass, contextPathProperty);

            // Make sure the path exists in the system
            Directory.CreateDirectory(path);

            // Generate the filename
            fileName = fileName + "." + extension;

            return Path.Combine(path, fileName);
        }

        /// <summary>
        /// Generate the path where a resource can be saved.
        /// Uses this priority:
        /// 1. reads from test context tag "property"
        /// 2. reads from environment variable Cmf.property
        /// 3. use system temp path
        /// </summary>
        /// <param name="testClass"></param>
        /// <param name="property">The property from the test context where the path is defined</param>
        /// <returns>The path to store a test resource</returns>
        private static string GenerateResourcePath(BaseTestClass testClass, string property = null)
        {
            string path = Path.GetTempPath();

            if (property != null)
            {
                path = GetProperty(testClass, property) ?? path;
            }

            return path;
        }

        /// <summary>
        /// Gets a property from the Test Settings or Environment.
        /// Returns null it none is found.
        /// </summary>
        /// <param name="testClass"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static string GetProperty(BaseTestClass testClass, string propertyName)
        {
            string value = null;

            if (testClass.TestContext.Properties.Contains(propertyName))
            {
                value = testClass.TestContext.Properties[propertyName].ToString();
            }

            if (String.IsNullOrWhiteSpace(value) && Environment.GetEnvironmentVariable(String.Format("Cmf.{0}", propertyName)) != null)
            {
                value = Environment.GetEnvironmentVariable(String.Format("Cmf.{0}", propertyName));
            }

            if (String.IsNullOrWhiteSpace(value) && Environment.GetEnvironmentVariable(String.Format("Cmf_{0}", propertyName).ToUpper()) != null)
            {
                value = Environment.GetEnvironmentVariable(String.Format("Cmf_{0}", propertyName).ToUpper());
            }

            return value;
        }

    }
}

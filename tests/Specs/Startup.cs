using System.Threading.Tasks;
using Cmf.Common.Cli.Objects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace tests.Specs
{
    [TestClass]
    public class Startup
    {
        #region mock services
        class MockNPMClient999 : INPMClient
        {
            public Task<string> GetLatestVersion(bool preRelease = false)
            {
                return Task.FromResult("999.99.99");
            }
        }
        
        class MockNPMClientCurrent : INPMClient
        {
            public Task<string> GetLatestVersion(bool preRelease = false)
            {
                return Task.FromResult(ExecutionContext.CurrentVersion);
            }
        }

        class MockVersionService : IVersionService
        {
            public string CurrentVersion => "1.0.0";
        }
        
        class MockVersionServiceDev : IVersionService
        {
            public string CurrentVersion => "1.0.0-0";
        }
        #endregion

        [TestMethod]
        public async Task NotAtLatestVersion()
        {
            ExecutionContext.ServiceProvider = (new ServiceCollection())
                .AddSingleton<INPMClient, MockNPMClient999>()
                .AddSingleton<IVersionService, MockVersionService>()
                .BuildServiceProvider();

            var logWriter = (new Logging()).GetLogStringWriter();
            
            await Cmf.Common.Cli.Program.VersionChecks();
            
            Assert.IsTrue(logWriter.ToString().Contains("Please update"));
        }
        
        [TestMethod]
        public async Task AtLatestVersion()
        {
            ExecutionContext.ServiceProvider = (new ServiceCollection())
                .AddSingleton<INPMClient, MockNPMClientCurrent>()
                .AddSingleton<IVersionService, MockVersionService>()
                .BuildServiceProvider();

            var logWriter = (new Logging()).GetLogStringWriter();
            
            await Cmf.Common.Cli.Program.VersionChecks();
            
            Assert.IsFalse(logWriter.ToString().Contains("Please update"));
        }

        [TestMethod]
        public async Task InDevVersion()
        {
            ExecutionContext.ServiceProvider = (new ServiceCollection())
                .AddSingleton<INPMClient, MockNPMClientCurrent>()
                .AddSingleton<IVersionService, MockVersionServiceDev>()
                .BuildServiceProvider();
            
            var logWriter = (new Logging()).GetLogStringWriter();
            
            await Cmf.Common.Cli.Program.VersionChecks();
            
            Assert.IsTrue(logWriter.ToString().Contains("You are using development version"));
        }
        
        [TestMethod]
        public async Task InStableVersion()
        {
            ExecutionContext.ServiceProvider = (new ServiceCollection())
                .AddSingleton<INPMClient, MockNPMClientCurrent>()
                .AddSingleton<IVersionService, MockVersionService>()
                .BuildServiceProvider();
            
            var logWriter = (new Logging()).GetLogStringWriter();
            
            await Cmf.Common.Cli.Program.VersionChecks();
            
            Assert.IsFalse(logWriter.ToString().Contains("You are using development version"));
        }
    }
}
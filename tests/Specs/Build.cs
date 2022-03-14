using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Cmf.Common.Cli.Commands;
using Cmf.Common.Cli.Commands.New;
using Cmf.Common.Cli.Enums;
using Cmf.Common.Cli.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace tests.Specs
{
    [TestClass]
    public class Build
    {
        [TestInitialize]
        public void Reset()
        {
            var newCommand = new NewCommand();
            var cmd = new Command("x");
            newCommand.Configure(cmd);

            var console = new TestConsole();
            cmd.Invoke(new[] {
                "--reset"
            }, console);
        }

        [TestMethod]
        public void BusinessBuildWithSuccessTestWithSuccess()
        {
            TestConsole console = RunBuild(new BuildCommand(), "business", "Cmf.Custom.Business", "Cmf.Custom.Common.UnitTests/GenericTestsFailingTests.cs");

            Assert.IsNotNull(console);
            string errors = console.Error.ToString().Trim();
            Assert.AreEqual(0, errors.Length, "Errors found in console: {0}", errors);
        }

        [TestMethod]
        public void BusinessBuildWithSuccessTestFail()
        {
            TestConsole console = RunBuild(new BuildCommand(), "business", "Cmf.Custom.Business", "Cmf.Custom.Common.UnitTests/GenericTests.cs");

            Assert.IsNotNull(console);
            string errors = console.Error.ToString().Trim();
            Assert.AreNotEqual(0, errors.Length, "No errors found in console");
            Assert.IsTrue(errors.Contains($"Command 'dotnet test ' did not finished successfully"), "Wrong errors found in console: {0}", errors);
        }

        /// <summary>
        /// Generic Method to setup build
        /// </summary>
        /// <param name="buildCommand"></param>
        /// <param name="fixtureSubFolder"></param>
        /// <param name="packageName"></param>
        /// <param name="fileToDelete"></param>
        /// <param name="scaffoldingDir"></param>
        /// <returns></returns>
        private TestConsole RunBuild(BuildCommand buildCommand
                                   , string fixtureSubFolder
                                   , string packageName
                                   , string fileToDelete = null
                                   , string scaffoldingDir = null)
        {
            var dir = scaffoldingDir ?? GenericUtilities.GetTmpDirectory();
            var console = new TestConsole();
            var cur = Directory.GetCurrentDirectory();

            try
            {
                // place new fixture: an init'd repository
                if (scaffoldingDir == null)
                {
                    GenericUtilities.CopyFixture($"build/{fixtureSubFolder}", new DirectoryInfo(dir));
                }

                Directory.SetCurrentDirectory($"{dir}/{packageName}");

                if (!string.IsNullOrWhiteSpace(fileToDelete))
                {
                    File.Delete($"{dir}/{packageName}/{fileToDelete}");
                }

                var cmd = new Command("x");
                buildCommand.Configure(cmd);
                var args = new List<string>
                {
                };

                cmd.Invoke(args.ToArray(), console);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
            finally
            {
                if (scaffoldingDir == null)
                {
                    Directory.SetCurrentDirectory(cur);
                    int tries = 3;
                    while (tries > 0)
                    {
                        try
                        {
                            Directory.Delete(dir, true);
                            break;
                        }
                        catch
                        {
                            tries--;
                        }
                    }
                }
            }
            return console;
        }
    }
}
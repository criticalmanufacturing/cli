using Cmf.Common.Cli.Commands;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.Diagnostics;
using System.IO;
using Xunit;
using Assert = tests.AssertWithMessage;

namespace tests.Specs
{
    public class Build
    {
        public Build()
        {
            var newCommand = new NewCommand();
            var cmd = new Command("x");
            newCommand.Configure(cmd);

            var console = new TestConsole();
            cmd.Invoke(new[] {
                "--reset"
            }, console);
        }

        [Fact]
        public void BusinessBuildWithSuccessTestWithSuccess()
        {
            TestConsole console = RunBuild(new BuildCommand(), "business", "Cmf.Custom.Business", "Cmf.Custom.Common.UnitTests/GenericTestsFailingTests.cs");

            Assert.NotNull(console);
            string errors = console.Error.ToString().Trim();
            Assert.True(0 == errors.Length, String.Format("Errors found in console: {0}", errors));
        }

        [Fact]
        public void BusinessBuildWithSuccessTestFail()
        {
            TestConsole console = RunBuild(new BuildCommand(), "business", "Cmf.Custom.Business", "Cmf.Custom.Common.UnitTests/GenericTests.cs");

            Assert.NotNull(console);
            string errors = console.Error.ToString().Trim();
            Assert.False(0 == errors.Length, "No errors found in console");
            Assert.True(errors.Contains($"Command 'dotnet test ' did not finished successfully"), String.Format("Wrong errors found in console: {0}", errors));
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
            var dir = scaffoldingDir ?? GetTmpDirectory();
            var console = new TestConsole();
            var cur = Directory.GetCurrentDirectory();

            try
            {
                // place new fixture: an init'd repository
                if (scaffoldingDir == null)
                {
                    CopyFixture($"build/{fixtureSubFolder}", new DirectoryInfo(dir));
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

        #region helpers

        private static string GetTmpDirectory()
        {
            var tmp = Path.Join(Path.GetTempPath(), Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8));
            Directory.CreateDirectory(tmp);

            Debug.WriteLine("Generating at " + tmp);
            return tmp;
        }

        private static void CopyFixture(string fixtureName, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);
            var source = new DirectoryInfo(System.IO.Path.GetFullPath(
                System.IO.Path.Join(
            AppDomain.CurrentDomain.BaseDirectory,
                        "..", "..", "..", "Fixtures", fixtureName)));
            CopyAll(source, target);
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        #endregion helpers
    }
}
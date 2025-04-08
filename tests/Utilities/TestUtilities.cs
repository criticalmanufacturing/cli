using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Cmf.CLI.Commands;
using Cmf.CLI.Core.Attributes;
using FluentAssertions;
using Xunit;

namespace Cmf.Common.Cli.TestUtilities
{
    /// <summary>
    /// Utilities for tests
    /// </summary>
    public static class TestUtilities
    {
        #region Public Methods

        /// <summary>
        /// Retrieves temp directory adds a guid and creates a dir
        /// </summary>
        /// <returns></returns>
        public static string GetTmpDirectory()
        {
            var tmp = Path.Join(Path.GetTempPath(), Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8));
            Directory.CreateDirectory(tmp);

            Debug.WriteLine("Generating at " + tmp);
            return tmp;
        }

        /// <summary>
        /// Resolves default fixture path 
        /// </summary>
        /// <param name="fixture"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string GetFixturePath(string fixture, string item)
        {
            return System.IO.Path.GetFullPath(
                System.IO.Path.Join(
            AppDomain.CurrentDomain.BaseDirectory,
                        "..", "..", "..", "Fixtures", fixture, item));
        }

        /// <summary>
        /// Copies the fixture to a directory
        /// </summary>
        /// <param name="fixtureName"></param>
        /// <param name="target"></param>
        public static void CopyFixture(string fixtureName, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);
            var source = new DirectoryInfo(System.IO.Path.GetFullPath(
                System.IO.Path.Join(
            AppDomain.CurrentDomain.BaseDirectory,
                        "..", "..", "..", "Fixtures", fixtureName)));
            CopyAll(source, target);
        }

        /// <summary>
        /// Copies the fixture package to a directory
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="target"></param>
        public static void CopyFixturePackage(string packageName, DirectoryInfo target)
        {
            target = Directory.CreateDirectory(Path.Join(target.FullName, packageName));
            var source = new DirectoryInfo(System.IO.Path.GetFullPath(
                Path.Join(
            AppDomain.CurrentDomain.BaseDirectory,
                        "..", "..", "..", "Fixtures", "new-packages", packageName)));
            CopyAll(source, target);
        }

        /// <summary>
        /// Copies all files from a source directory to a target directory
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
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

        /// <summary>
        /// Retrieves a property from a cmfpackage.json file
        /// </summary>
        /// <param name="property"></param>
        /// <param name="cmfpackageJsonPath"></param>
        /// <returns></returns>
        public static string GetPackageProperty(string property, string cmfpackageJsonPath)
        {
            var pkg = GetPackage(cmfpackageJsonPath);
            return pkg.GetProperty(property).GetString();
        }

        /// <summary>
        /// Retrieves the content from a cmfpackage.json file
        /// </summary>
        /// <param name="cmfpackageJsonPath"></param>
        /// <returns></returns>
        public static JsonElement GetPackage(string cmfpackageJsonPath)
        {
            var json = File.ReadAllText(cmfpackageJsonPath);
            return JsonDocument.Parse(json).RootElement;
        }

        /// <summary>
        /// Lists all the files inside a zip file
        /// </summary>
        /// <param name="packageFile"></param>
        /// <returns></returns>
        public static List<string> GetFileEntriesFromZip(string packageFile)
        {
            using (FileStream zipToOpen = new(packageFile, FileMode.Open))
            {
                using (ZipArchive zip = new(zipToOpen, ZipArchiveMode.Read))
                {
                    return zip.Entries.Select(entry => entry.Name).ToList();
                }
            }
        }

        /// <summary>
        /// Create a minimal parser to invoke commands with
        /// The default parser brings a lot of extras that we don't require in the tests
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static Parser GetParser(Command cmd)
        {
            return new CommandLineBuilder(cmd)
                .UseEnvironmentVariableDirective()
                .UseParseDirective()
                .UseParseErrorReporting()
                .UseExceptionHandler()
                .CancelOnProcessTermination()
                .Build();
        }

        /// <summary>
        /// Create a minimal parser to invoke commands with
        /// The default parser brings a lot of extras that we don't require in the tests
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static void TestInvoke<T>(T cmd, string[] args)
            where T : BaseCommand
        {
            var console = new TestConsole();

            var root = new Command("cmf");
            cmd.Configure(root);

            var result = GetParser(root).Invoke(args, console);

            if (result != 0)
            {
                throw new Exception(console.Error.ToString());
            }
        }

        /// <summary>
        /// Create a minimal parser to invoke commands with
        /// The default parser brings a lot of extras that we don't require in the tests
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static async Task TestInvokeAsync<T>(T cmd, string[] args, bool setupParents = false)
            where T : BaseCommand
        {
            var console = new TestConsole();

            var attr = cmd.GetType().GetCustomAttributes<CmfCommandAttribute>(false).First();
            var root= new Command(attr.Name) { IsHidden = attr.IsHidden, Description = attr.Description };
            cmd.Configure(root);

            if (setupParents)
            {
                // Get all types that are marked with CmfCommand attribute
                var commandTypes = typeof(T).Assembly.GetTypes()
                    .Select(type => (type: type, attribute: type.GetCustomAttributes<CmfCommandAttribute>(false).FirstOrDefault()))
                    .Where(cmd => cmd.attribute != null && cmd.attribute.Id != null)
                    .ToDictionary(cmd => cmd.attribute.Id);

                // Commands that depend on root (have no defined parent)
                var currentCmd = commandTypes[attr.Id];
                while (!string.IsNullOrWhiteSpace(currentCmd.attribute.Parent) ||
                       !string.IsNullOrWhiteSpace(currentCmd.attribute.ParentId))
                {
                    var parentCmd = commandTypes[currentCmd.attribute.ParentId];
                    currentCmd = parentCmd;

                    // Create command
                    var cmdInstance = new Command(parentCmd.attribute.Name) { IsHidden = parentCmd.attribute.IsHidden, Description = parentCmd.attribute.Description };
                    cmdInstance.AddCommand(root);
                    root = cmdInstance;

                    // Call "Configure" method
                    BaseCommand cmdHandler = Activator.CreateInstance(parentCmd.type) as BaseCommand;
                    cmdHandler.Configure(cmdInstance);
                    currentCmd = parentCmd;
                }
            }

            var result = await GetParser(root).InvokeAsync(args, console);

            if (result != 0)
            {
                throw new Exception(console.Error.ToString());
            }
        }

        /// <summary>
        /// Validates the content of the zip.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="zipFile">The zip file.</param>
        /// <param name="expectedFiles">The expected files.</param>
        public static void ValidateZipContent(IFileSystem fileSystem, IFileInfo zipFile, List<string> expectedFiles)
        {
            using Stream zipToOpen = fileSystem.FileStream.New(zipFile.FullName, FileMode.Open);
            using ZipArchive zip = new(zipToOpen, ZipArchiveMode.Read);
            // these tuples allow us to rewrite entry paths
            var entriesToExtract = new List<Tuple<ZipArchiveEntry, string>>();
            entriesToExtract.AddRange(zip.Entries.Select(selector: entry => new Tuple<ZipArchiveEntry, string>(entry, entry.FullName)));

            expectedFiles.Count.Should().Be(entriesToExtract.Count);

            foreach (var expectedFile in expectedFiles)
            {
                entriesToExtract.FirstOrDefault(x => x.Item2.Equals(expectedFile)).Should().NotBeNull();
            }
        }

        #endregion
    }
}
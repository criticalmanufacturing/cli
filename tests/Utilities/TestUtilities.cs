using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Enums;
using Cmf.Common.Cli.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

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

        #endregion

        #region public Methods

        #endregion
    }
}
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

namespace Cmf.Common.Cli.Utilities
{
    /// <summary>
    ///
    /// </summary>
    public static class TestUtilities
    {
        #region Public Methods

        public static string GetTmpDirectory()
        {
            var tmp = Path.Join(Path.GetTempPath(), Convert.ToHexString(Guid.NewGuid().ToByteArray()).Substring(0, 8));
            Directory.CreateDirectory(tmp);

            Debug.WriteLine("Generating at " + tmp);
            return tmp;
        }

        public static string GetFixturePath(string fixture, string item)
        {
            return System.IO.Path.GetFullPath(
                System.IO.Path.Join(
            AppDomain.CurrentDomain.BaseDirectory,
                        "..", "..", "..", "Fixtures", fixture, item));
        }

        public static void CopyFixture(string fixtureName, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);
            var source = new DirectoryInfo(System.IO.Path.GetFullPath(
                System.IO.Path.Join(
            AppDomain.CurrentDomain.BaseDirectory,
                        "..", "..", "..", "Fixtures", fixtureName)));
            CopyAll(source, target);
        }

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

        public static string GetPackageProperty(string property, string cmfpackageJsonPath)
        {
            var pkg = GetPackage(cmfpackageJsonPath);
            return pkg.GetProperty(property).GetString();
        }

        public static JsonElement GetPackage(string cmfpackageJsonPath)
        {
            var json = File.ReadAllText(cmfpackageJsonPath);
            return JsonDocument.Parse(json).RootElement;
        }

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
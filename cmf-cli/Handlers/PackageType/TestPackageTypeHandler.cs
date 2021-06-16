using Cmf.Common.Cli.Builders;
using Cmf.Common.Cli.Enums;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text.RegularExpressions;

namespace Cmf.Common.Cli.Handlers
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Cmf.Common.Cli.Handlers.PackageTypeHandler" />
    public class TestPackageTypeHandler : PackageTypeHandler
    {
        #region Private Methods
  
        /// <summary>
        /// Generates the deployment framework manifest.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        /// <exception cref="Cmf.Common.Cli.Utilities.CliException"></exception>
        internal override void GenerateDeploymentFrameworkManifest(IDirectoryInfo packageOutputDir)
        {
            // This package cannot create the DF Manifest
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="TestPackageTypeHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        public TestPackageTypeHandler(CmfPackage cmfPackage) : base(cmfPackage)
        {
            BuildSteps = new IBuildCommand[]
            {
                new DotnetCommand()
                {
                    Command = "restore",
                    DisplayName = "NuGet restore",
                    Solution = new FileInfo(Path.Join(cmfPackage.GetFileInfo().Directory.FullName, "Tests.sln")),
                    NuGetConfig = new FileInfo(Path.Join(FileSystemUtilities.GetProjectRoot(throwException: true).FullName, "NuGet.Config")),
                    WorkingDirectory = cmfPackage.GetFileInfo().Directory
                },
                new DotnetCommand()
                {
                    Command = "build",
                    DisplayName = "Build Tests Solution",
                    Solution = new FileInfo(Path.Join(cmfPackage.GetFileInfo().Directory.FullName, "Tests.sln")),
                    Configuration = "Release",
                    WorkingDirectory = cmfPackage.GetFileInfo().Directory
                }
            };
        }

        /// <summary>
        /// Bumps the specified CMF package.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="buildNr">The version for build Nr.</param>
        /// <param name="bumpInformation">The bump information.</param>
        public override void Bump(string version, string buildNr, Dictionary<string, object> bumpInformation = null)
        {
            base.Bump(version, buildNr, bumpInformation);

            string[] versionTags = null;
            if (!string.IsNullOrWhiteSpace(version))
            {
                versionTags = version.Split('.');
            }

            // Assembly Info
            string[] filesToUpdate = Directory.GetFiles(".", "AssemblyInfo.cs", SearchOption.AllDirectories);
            string pattern = @"Version\(\""[0-9.]*\""\)";
            foreach (var filePath in filesToUpdate)
            {
                string text = File.ReadAllText(filePath);
                var metadataVersionInfo = Regex.Match(text, pattern, RegexOptions.Singleline)?.Value?.Split("\"")[1].Split('.');
                string major = versionTags != null && versionTags.Length > 0 ? versionTags[0] : metadataVersionInfo[0];
                string minor = versionTags != null && versionTags.Length > 1 ? versionTags[1] : metadataVersionInfo[1];
                string patch = versionTags != null && versionTags.Length > 2 ? versionTags[2] : metadataVersionInfo[2];
                string build = !string.IsNullOrEmpty(buildNr) ? buildNr : "0";
                string newVersion = string.Format(@"Version(""{0}.{1}.{2}.{3}"")", major, minor, patch, build);
                text = Regex.Replace(text, pattern, newVersion, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                File.WriteAllText(filePath, text);
            }
        }
    
        #endregion
    }
}
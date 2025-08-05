
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using Cmf.CLI.Builders;
using Cmf.CLI.Commands.restore;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cmf.CLI.Handlers
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="PackageTypeHandler" />
    public class TestPackageTypeHandler : PackageTypeHandler
    {
        #region Private Methods

        /// <summary>
        /// Generates the deployment framework manifest.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        /// <exception cref="CliException"></exception>
        internal override void GenerateDeploymentFrameworkManifest(IDirectoryInfo packageOutputDir)
        {
            // This package cannot create the DF Manifest
            // However, the DF Manifest file usually created by this method is then used
            // to generate the package.json file needed to be able to publish these
            // packages into NPM feeds, for example
            // As such, for Test packages, we simply create the package.json directly
            Log.Debug("Generating tests package.json");
            string path = this.fileSystem.Path.Combine(packageOutputDir.FullName, CoreConstants.PackageJson);

            // Get Template
            string fileContent = ResourceUtilities.GetEmbeddedResourceContent($"{CliConstants.FolderTemplates}/{CmfPackage.PackageType}/{CoreConstants.PackageJson}");

            JObject json = JsonConvert.DeserializeObject<JObject>(fileContent);

            // Replace the placeholder values from the template
            json["name"] = CmfPackage.PackageId;
            json["packageName"] = CmfPackage.Name;
            json["version"] = CmfPackage.Version.ToString();
            json["description"] = CmfPackage.Description;

            // Write back the file to disk
            fileContent = JsonConvert.SerializeObject(json, Formatting.Indented);

            this.fileSystem.File.WriteAllText(path, fileContent);
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
                new ExecuteCommand<RestoreCommand>()
                {
                    Command = new RestoreCommand(),
                    DisplayName = "cmf restore",
                    Execute = command =>
                    {
                        command.Execute(cmfPackage.GetFileInfo().Directory, null);
                    }
                },
                new DotnetCommand()
                {
                    Command = "restore",
                    DisplayName = "NuGet restore",
                    Solution = this.fileSystem.FileInfo.New(this.fileSystem.Path.Join(cmfPackage.GetFileInfo().Directory.FullName, "Tests.sln")),
                    WorkingDirectory = cmfPackage.GetFileInfo().Directory
                },
                new DotnetCommand()
                {
                    Command = "build",
                    DisplayName = "Build Tests Solution",
                    Solution = this.fileSystem.FileInfo.New(this.fileSystem.Path.Join(cmfPackage.GetFileInfo().Directory.FullName, "Tests.sln")),
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
            string[] filesToUpdate = this.fileSystem.Directory.GetFiles(this.CmfPackage.GetFileInfo().DirectoryName, "AssemblyInfo.cs", SearchOption.AllDirectories);
            string pattern = @"Version\(\""[0-9.]*\""\)";
            foreach (var filePath in filesToUpdate)
            {
                string text = this.fileSystem.File.ReadAllText(filePath);
                var metadataVersionInfo = Regex.Match(text, pattern, RegexOptions.Singleline)?.Value?.Split("\"")[1].Split('.');
                string major = versionTags != null && versionTags.Length > 0 ? versionTags[0] : metadataVersionInfo[0];
                string minor = versionTags != null && versionTags.Length > 1 ? versionTags[1] : metadataVersionInfo[1];
                string patch = versionTags != null && versionTags.Length > 2 ? versionTags[2] : metadataVersionInfo[2];
                string build = !string.IsNullOrEmpty(buildNr) ? buildNr : "0";
                string newVersion = string.Format(@"Version(""{0}.{1}.{2}.{3}"")", major, minor, patch, build);
                text = Regex.Replace(text, pattern, newVersion, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                this.fileSystem.File.WriteAllText(filePath, text);
            }
        }
    
        #endregion
    }
}
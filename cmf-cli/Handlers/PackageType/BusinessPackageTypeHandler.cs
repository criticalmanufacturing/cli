using Cmf.CLI.Builders;
using Cmf.CLI.Commands.restore;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Cmf.CLI.Handlers
{
    /// <summary>
    /// </summary>
    /// <seealso cref="PackageTypeHandler"/>
    public class BusinessPackageTypeHandler : PackageTypeHandler
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BusinessPackageTypeHandler"/> class.
        /// </summary>
        /// <param name="cmfPackage">
        ///     The CMF package.
        /// </param>
        public BusinessPackageTypeHandler(CmfPackage cmfPackage) : base(cmfPackage)
        {
            cmfPackage.SetDefaultValues(
            targetDirectory:
                "BusinessTier",
            targetLayer:
                "host",
            steps:
                new List<Step>()
                {
                        new Step(StepType.Generic)
                        {
                            OnExecute = "$(Agent.Root)/agent/scripts/stop_host.ps1"
                        },
                        new Step(StepType.DeployFiles)
                        {
                            ContentPath = "**/*.dll"
                        },
                        new Step(StepType.Generic)
                        {
                            OnExecute = "$(Agent.Root)/agent/scripts/start_host.ps1"
                        }
                 });

            BuildSteps = new IBuildCommand[]
            {
                new ExecuteCommand<RestoreCommand>()
                {
                    Command = new RestoreCommand(fileSystem),
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
                    Solution = this.fileSystem.FileInfo.New(Path.Join(cmfPackage.GetFileInfo().Directory.FullName, "Business.sln")),
                    NuGetConfig = this.fileSystem.FileInfo.New(Path.Join(FileSystemUtilities.GetProjectRoot(this.fileSystem, throwException: true).FullName, "NuGet.Config")),
                    WorkingDirectory = cmfPackage.GetFileInfo().Directory
                },
                new DotnetCommand()
                {
                    Command = "build",
                    DisplayName = "Build Business Solution",
                    Solution = this.fileSystem.FileInfo.New(Path.Join(cmfPackage.GetFileInfo().Directory.FullName, "Business.sln")),
                    WorkingDirectory = cmfPackage.GetFileInfo().Directory,
                    Args = new [] { "--no-restore "}
                },
                new DotnetCommand()
                {
                    Command = "test",
                    DisplayName = "Run Business Unit Tests",
                    WorkingDirectory = cmfPackage.GetFileInfo().Directory,
                    Test = true,
                    Args = new [] { "--collect:\"XPlat Code Coverage\"", "--logger", "trx" }
                }
            };
        }

        /// <summary>
        ///     Bumps the specified CMF package.
        /// </summary>
        /// <param name="version">
        ///     The version.
        /// </param>
        /// <param name="buildNr">
        ///     The version for build Nr.
        /// </param>
        /// <param name="bumpInformation">
        ///     The bump information.
        /// </param>
        public override void Bump(string version, string buildNr, Dictionary<string, object> bumpInformation = null)
        {
            base.Bump(version, buildNr, bumpInformation);

            string[] versionTags = null;
            if (!string.IsNullOrWhiteSpace(version))
            {
                versionTags = version.Split('.');
            }

            // Assembly Info
            string[] filesToUpdate = this.fileSystem.Directory.GetFiles(".", "AssemblyInfo.cs", SearchOption.AllDirectories);
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
    }
}
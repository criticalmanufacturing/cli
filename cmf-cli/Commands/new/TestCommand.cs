using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO.Abstractions;
using System.Text.Json;
using Cmf.CLI.Constants;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Commands.New
{
    /// <summary>
    /// Generates the Test layer structure
    /// </summary>
    [CmfCommand("test", ParentId = "new")]
    public class TestCommand : LayerTemplateCommand
    {
        /// <inheritdoc />
        public TestCommand() : base("tests", PackageType.Tests)
        {
        }

        /// <inheritdoc />
        public TestCommand(IFileSystem fileSystem) : base("tests", PackageType.Tests, fileSystem)
        {
        }

        /// <inheritdoc />
        public override void Configure(Command cmd)
        {
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--version" },
                description: "Package Version",
                getDefaultValue: () => "1.0.0"
            ));
            cmd.Handler = CommandHandler.Create<string>(Execute);
        }

        /// <inheritdoc />
        protected override List<string> GenerateArgs(IDirectoryInfo projectRoot, IDirectoryInfo workingDir, List<string> args)
        {
            return args;
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="version">the package version</param>
        public void Execute(string version)
        {
            var packageName = "Cmf.Custom.Tests";
            var projectRoot = FileSystemUtilities.GetProjectRoot(this.fileSystem);
            var repoType = ExecutionContext.Instance.ProjectConfig.RepositoryType ?? CliConstants.DefaultRepositoryType;
            var projectConfig = ExecutionContext.Instance.ProjectConfig;
            var tenant = projectConfig.Tenant;
            var args = new List<string>()
            {
                // engine options
                "--output", projectRoot.FullName,

                // template symbols
                "--name", packageName,
                "--packageVersion", version,
                "--idSegment", tenant,
                "--Tenant", tenant
            };

            var projectName = projectConfig.ProjectName;
            var restPort = projectConfig.RESTPort;
            var mesVersion = projectConfig.MESVersion;
            var htmlPort = projectConfig.HTMLPort;
            var vmHostname = projectConfig.vmHostname;
            var testScenariosNugetVersion = projectConfig.TestScenariosNugetVersion;
            var isSslEnabled = projectConfig.IsSslEnabled;
            args.AddRange(new[]
            {
                "--projectName", projectName,
                "--repositoryType", repoType.ToString(),
                "--vmHostname", vmHostname,
                "--RESTPort", restPort.ToString(),
                "--testScenariosNugetVersion", testScenariosNugetVersion.ToString(),
                "--HTMLPort", htmlPort.ToString(),
                "--MESVersion", mesVersion.ToString()
            });

            if (isSslEnabled)
            {
                args.Add("--IsSslEnabled");
            }
            
            #region version-specific bits
            args.AddRange(new []{ "--targetFramework", mesVersion.Major > 8 ? mesVersion.Major >= 11 ? "net8.0" : "net6.0" : "netcoreapp3.1" });

            if (mesVersion >= new Version(11, 2, 3))
            {
                args.Add("--hostPerformanceTests");
            }
            #endregion

            this.executedArgs = args.ToArray();
            base.RunCommand(args);
            base.RegisterAsDependencyInParent(packageName, version, projectRoot.FullName, isTestPackage: true);
            base.RegisterAsDependencyInParent("Cmf.Custom.Tests.MasterData", version, projectRoot.FullName, isTestPackage: true);
        }
    }
}

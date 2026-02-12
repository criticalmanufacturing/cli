using Cmf.CLI.Constants;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using System.Linq;
using System;
using System.Threading.Tasks;

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
            var versionOption = new Option<string>("--version")
            {
                Description = "Package Version",
                DefaultValueFactory = _ => "1.0.0"
            };
            cmd.Options.Add(versionOption);

            cmd.SetAction((parseResult, cancellationToken) =>
            {
                var version = parseResult.GetValue(versionOption);
                Execute(version);
                return Task.FromResult(0);
            });
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
            var (organization, product) = GetOrganizationAndProductFromProjectConfig();

            var packageName = $"{organization}.{product}.Tests";

            var projectRoot = FileSystemUtilities.GetProjectRoot(this.fileSystem);
            var repoType = Core.Objects.ExecutionContext.Instance.ProjectConfig.RepositoryType ?? CliConstants.DefaultRepositoryType;
            var projectConfig = Core.Objects.ExecutionContext.Instance.ProjectConfig;
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
                "--MESVersion", mesVersion.ToString(),
                "--Product", product,
                "--Organization", organization
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
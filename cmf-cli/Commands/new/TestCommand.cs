using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.CommandLine;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Cmf.CLI.Services;
using NuGet.Versioning;

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
            var versionOption = new Option<NuGetVersion>("--version")
            {
                Description = "Package Version",
                CustomParser = result => VersionUtilities.ParseVersion(result, text => VersionUtilities.ParseNuGetVersion(text, "--version")),
                DefaultValueFactory = _ => NuGetVersion.Parse("1.0.0")
            };
            cmd.Add(versionOption);

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
        public void Execute(NuGetVersion version)
        {
            CommandUtilities.ThrowIfNoProjectConfig(ExecutionContext.Instance);
            var projectConfig = ExecutionContext.Instance.ProjectConfig;
            var mesVersion = projectConfig.MESVersion ?? throw VersionUtilities.InvalidVersion(null,
                nameof(ProjectConfig.MESVersion), CoreMessages.NuGetVersionExpectedFormat, CoreConstants.ProjectConfigFileName);
            var testScenariosNugetVersion = projectConfig.TestScenariosNugetVersion ?? throw VersionUtilities.InvalidVersion(null,
                nameof(ProjectConfig.TestScenariosNugetVersion), CoreMessages.NuGetVersionExpectedFormat, CoreConstants.ProjectConfigFileName);
            var (organization, product) = GetOrganizationAndProductFromProjectConfig();

            var packageName = $"{organization}.{product}.Tests";

            var projectRoot = FileSystemUtilities.GetProjectRoot(this.fileSystem);
            var repoType = ExecutionContext.Instance.ProjectConfig.RepositoryType ?? CliConstants.DefaultRepositoryType;
            var tenant = projectConfig.Tenant;
            var args = new List<string>()
            {
                // engine options
                "--output", projectRoot.FullName,

                // template symbols
                "--name", packageName,
                "--packageVersion", VersionUtilities.GetVersionText(version),
                "--idSegment", tenant,
                "--Tenant", tenant
            };

            var projectName = projectConfig.ProjectName;
            var legacyProjectConfig = projectConfig as ProjectConfigV1;
            var restPort = legacyProjectConfig?.RESTPort;
            var htmlPort = legacyProjectConfig?.HTMLPort;
            var vmHostname = legacyProjectConfig?.vmHostname;

            args.AddRange(new[] { "--IsSslEnabled", legacyProjectConfig?.IsSslEnabled.ToString().ToLowerInvariant() ?? "true" });

            args.AddRange(new[]
            {
                "--projectName", projectName,
                "--repositoryType", repoType.ToString(),
                "--testScenariosNugetVersion", testScenariosNugetVersion.ToString(),
                "--MESVersion", mesVersion.ToString(),
                "--Product", product,
                "--Organization", organization
            });

            if (!string.IsNullOrEmpty(vmHostname))
            {
                args.AddRange(new[] { "--vmHostname", vmHostname });
            }

            if (restPort.HasValue)
            {
                args.AddRange(new[] { "--RESTPort", restPort.Value.ToString() });
            }

            if (htmlPort.HasValue)
            {
                args.AddRange(new[] { "--HTMLPort", htmlPort.Value.ToString() });
            }
            
            #region version-specific bits
            args.AddRange(new []{ "--targetFramework", ExecutionContext.ServiceProvider.GetService<IDependencyVersionService>().DotNetTargetFramework(mesVersion) });

            if (mesVersion >= new NuGetVersion(11, 2, 3))
            {
                args.Add("--hostPerformanceTests");
            }
            #endregion

            // Switch between version templates
            // Default case is v12 so it supports v12+
            switch (mesVersion.Major)
            {
                case 10:
                case 11:
                    this.CommandName = "tests10";
                    break;
                default:
                    this.CommandName = "tests12";
                    break;
            }

            this.executedArgs = args.ToArray();
            RunCommand(args);
            base.RegisterAsDependencyInParent(packageName, VersionUtilities.GetVersionText(version), projectRoot.FullName, isTestPackage: true);
            base.RegisterAsDependencyInParent("Cmf.Custom.Tests.MasterData", VersionUtilities.GetVersionText(version), projectRoot.FullName, isTestPackage: true);
        }
    }
}

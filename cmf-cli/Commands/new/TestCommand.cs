using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using System.Text.Json;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Commands.New
{
    /// <summary>
    /// Generates the Test layer structure
    /// </summary>
    [CmfCommand("test", Parent = "new")]
    public class TestCommand : LayerTemplateCommand
    {
        /// <inheritdoc />
        public TestCommand() : base("test", PackageType.Tests)
        {
        }

        /// <inheritdoc />
        public TestCommand(IFileSystem fileSystem) : base("test", PackageType.Tests, fileSystem)
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
        protected override List<string> GenerateArgs(IDirectoryInfo projectRoot, IDirectoryInfo workingDir, List<string> args, JsonDocument projectConfig)
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
            var projectConfig = FileSystemUtilities.ReadProjectConfig(this.fileSystem);
            var tenant = projectConfig.RootElement.GetProperty("Tenant").GetString();
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

            var restPort = projectConfig.RootElement.GetProperty("RESTPort").GetString();
            var mesVersion = projectConfig.RootElement.GetProperty("MESVersion").GetString();
            var htmlPort = projectConfig.RootElement.GetProperty("HTMLPort").GetString();
            var vmHostname = projectConfig.RootElement.GetProperty("vmHostname").GetString();
            var testScenariosNugetVersion = projectConfig.RootElement.GetProperty("TestScenariosNugetVersion").GetString();
            var isSslEnabled = projectConfig.RootElement.GetProperty("IsSslEnabled").GetString();
            args.AddRange(new[]
            {
                "--vmHostname", vmHostname,
                "--RESTPort", restPort,
                "--testScenariosNugetVersion", testScenariosNugetVersion,
                "--HTMLPort", htmlPort,
                "--MESVersion", mesVersion
            });

            if (string.Equals(isSslEnabled, "True"))
            {
                args.Add("--IsSslEnabled");
            }
            
            #region version-specific bits
            args.AddRange(new []{ "--targetFramework", Version.Parse(mesVersion).Major > 8 ? "net6.0" : "netcoreapp3.1" });
            #endregion

            this.executedArgs = args.ToArray();
            base.RunCommand(args);
            base.RegisterAsDependencyInParent(packageName, version, projectRoot.FullName, isTestPackage: true);
            base.RegisterAsDependencyInParent("Cmf.Custom.Tests.MasterData", version, projectRoot.FullName, isTestPackage: true);
        }
    }
}

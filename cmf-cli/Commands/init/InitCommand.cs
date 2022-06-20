using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using Cmf.CLI.Constants;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Cmf.CLI.Commands
{
    /// <summary>
    /// Azure DevOps Agent Type
    /// </summary>
    public enum AgentType
    {
        /// <summary>
        /// Cloud agents
        /// </summary>
        Cloud,
        /// <summary>
        /// Self-host agents
        /// </summary>
        Hosted
    }

    // this is public because Execute is public by convention but never invoked externally
    // so this class mirrors the command internal structure and is never used outside
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class InitArguments
    {
        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public IDirectoryInfo workingDir { get; set; }
        public string projectName { get; set; }
        public string rootPackageName { get; set; }
        public string version { get; set; }
        public IFileInfo config { get; set; }
        public IDirectoryInfo deploymentDir { get; set; }
        public Uri repositoryUrl { get; set; }
        public string MESVersion { get; set; }
        public string DevTasksVersion { get; set; }
        public string HTMLStarterVersion { get; set; }
        public string yoGeneratorVersion { get; set; }
        public string nugetVersion { get; set; }
        public string testScenariosNugetVersion { get; set; }
        public IFileInfo infrastructure { get; set; }
        public Uri nugetRegistry { get; set; }
        public Uri npmRegistry { get; set; }
        public Uri azureDevOpsCollectionUrl { get; set; }
        public string agentPool { get; set; }
        public AgentType? agentType { get; set; }
        public IFileInfo ISOLocation { get; set; }
        public string nugetRegistryUsername { get; set; }
        public string nugetRegistryPassword { get; set; }
        public string cmfPipelineRepository { get; set; }
        public string cmfCliRepository { get; set; }
        public string releaseCustomerEnvironment { get; set; }
        public string releaseSite { get; set; }
        public string releaseDeploymentPackage { get; set; }
        public string releaseLicense { get; set; }
        public string releaseDeploymentTarget { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
        // ReSharper restore InconsistentNaming
    }

    /// <summary>
    /// Init command
    /// </summary>
    [CmfCommand("init")]
    public class InitCommand : TemplateCommand
    {
        /// <summary>
        /// constructor
        /// </summary>
        public InitCommand() : base("init")
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem"></param>
        public InitCommand(IFileSystem fileSystem) : base("init", fileSystem)
        {
        }

        /// <summary>
        /// configure command signature
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            cmd.AddArgument(new Argument<string>(
                name: "projectName"
            ));
            cmd.AddArgument(new Argument<string>(
                name: "rootPackageName",
                getDefaultValue: () => "Cmf.Custom.Package"
            ));
            cmd.AddArgument(new Argument<IDirectoryInfo>(
                name: "workingDir",
                parse: (argResult) => Parse<IDirectoryInfo>(argResult, "."),
                isDefault: true
            )
            {
                Description = "Working Directory"
            });
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--version" },
                description: "Package Version",
                getDefaultValue: () => "1.0.0"
            ));
            cmd.AddOption(new Option<IFileInfo>(
                aliases: new[] { "-c", "--config" },
                parseArgument: argResult => Parse<IFileInfo>(argResult),
                isDefault: true,
                description: "Configuration file exported from Setup")
                { IsRequired = true });
            
            // template-time options. These are all mandatory
            cmd.AddOption(new Option<Uri>(
                aliases: new[] { "--repositoryUrl" },
                description: "Git repository URL"
            ) { IsRequired = true });
            cmd.AddOption(new Option<IDirectoryInfo>(
                aliases: new[] { "--deploymentDir" },
                parseArgument: argResult => Parse<IDirectoryInfo>(argResult),
                description: "Deployments directory"
            ){ IsRequired = true });
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--MESVersion" },
                description: "Target MES version"
            ) { IsRequired = true });
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--DevTasksVersion" },
                description: "Critical Manufacturing dev-tasks version"
            ) { IsRequired = true });
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--HTMLStarterVersion" },
                description: "HTML Starter version"
            ) { IsRequired = true });
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--yoGeneratorVersion" },
                description: "@criticalmanufacturing/html Yeoman generator version"
            ) { IsRequired = true });
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--nugetVersion" },
                description: "NuGet versions to target. This is usually the MES version"
            ) { IsRequired = true });
            // TODO: remove this one?
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--testScenariosNugetVersion" },
                description: "Test Scenarios Nuget Version"
            ) { IsRequired = true });

            // infra options
            cmd.AddOption(new Option<IFileInfo>(
                aliases: new[] { "--infra", "--infrastructure" },
                parseArgument: argResult => Parse<IFileInfo>(argResult),
                isDefault: true,
                description: "Infrastructure JSON file"
            ));
            cmd.AddOption(new Option<Uri>(
                aliases: new[] { "--nugetRegistry" },
                description: "NuGet registry that contains the MES packages"
                ));
            cmd.AddOption(new Option<Uri>(
                aliases: new[] { "--npmRegistry" },
                description: "NPM registry that contains the MES packages"
            ));
            cmd.AddOption(new Option<Uri>(
                aliases: new[] { "--azureDevOpsCollectionUrl" },
                description: "The Azure DevOps collection address"
            ));
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--agentPool" },
                description: "Azure DevOps agent pool"
            ));
            cmd.AddOption(new Option<AgentType>(
                aliases: new[] { "--agentType" },
                description: "Type of Azure DevOps agents: Cloud or Hosted"
            ));
            cmd.AddOption(new Option<IFileInfo>(
                aliases: new[] { "--ISOLocation" },
                parseArgument: argResult => Parse<IFileInfo>(argResult),
                isDefault: true,
                description: "MES ISO file"
            ));
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--nugetRegistryUsername" },
                description: "NuGet registry username"
            ));
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--nugetRegistryPassword" },
                description: "NuGet registry password"
            ));
            cmd.AddOption(new Option<Uri>(
                aliases: new[] { "--cmfCliRepository" },
                description: "NPM registry that contains the CLI"
            ));
            cmd.AddOption(new Option<Uri>(
                aliases: new[] { "--cmfPipelineRepository" },
                description: "NPM registry that contains the CLI Pipeline Plugin"
            ));
            
            // container-specific switches
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--releaseCustomerEnvironment" },
                description: "Customer Environment Name defined in DevOpsCenter"
            ));
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--releaseSite" },
                description: "Site defined in DevOpsCenter"
            ));
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--releaseDeploymentPackage" },
                description: "DeploymentPackage defined in DevOpsCenter"
            ));
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--releaseLicense" },
                description: "License defined in DevOpsCenter"
            ));
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--releaseDeploymentTarget" },
                description: "DeploymentTarget defined in DevOpsCenter"
            ));

            // Add the handler
            cmd.Handler = CommandHandler
                .Create((InitArguments args) =>
                {
                    this.Execute(args);
                });
            // no overload accepts these many arguments...
            // .Create<
            //     IDirectoryInfo, // workingDir
            //     string, // rootPackageName,
            //     IFileInfo, // config
            //     IDirectoryInfo, // deploymentDir
            //     string, // MESVersion
            //     string, // DevTasksVersion
            //     string, // HTMLStarterVersion
            //     string, // yoGeneratorVersion
            //     string, // nugetVersion
            //     string, // testScenariosNugetVersion
            //     IFileInfo, // infrastructure
            //     Uri, // nugetRegistry
            //     Uri, // npmRegistry
            //     Uri, // universalRegistry
            //     string, // agentPool
            //     AgentType? // agentType
            // >(this.Execute);
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        internal void Execute(InitArguments x)
        {
            using var activity = ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);
            var args = new List<string>()
            {
                // engine options
                "--output", x.workingDir.FullName,

                // template symbols
                "--customPackageName", x.rootPackageName,
                "--projectName", x.projectName
            };

            if (x.version != null)
            {
                args.AddRange(new [] {"--packageVersion", x.version});
            }

            if (x.deploymentDir != null)
            {
                args.AddRange(new [] {"--deploymentDir", x.deploymentDir.FullName});
                // repositories are sub-folders of deploymentDir 
                args.AddRange(new [] {"--CIRepo", $"{x.deploymentDir.FullName}\\CIPackages"});
                args.AddRange(new [] {"--ADArtifactsRepo", $"{x.deploymentDir.FullName}\\ADArtifacts"});
                args.AddRange(new [] {"--RCRepo", $"{x.deploymentDir.FullName}\\ReleaseCandidates"});
                args.AddRange(new [] {"--DeliveredRepo", $"{x.deploymentDir.FullName}\\Delivered"});
            }

            if (x.repositoryUrl != null)
            {
                args.AddRange(new [] {"--repositoryUrl", x.repositoryUrl.AbsoluteUri});
            }

            if (x.MESVersion != null)
            {
                args.AddRange(new [] {"--MESVersion", x.MESVersion});
            }
            if (x.DevTasksVersion != null)
            {
                args.AddRange(new [] {"--DevTasksVersion", x.DevTasksVersion});
            }
            if (x.HTMLStarterVersion != null)
            {
                args.AddRange(new [] {"--HTMLStarterVersion", x.HTMLStarterVersion});
            }
            if (x.yoGeneratorVersion != null)
            {
                args.AddRange(new [] {"--yoGeneratorVersion", x.yoGeneratorVersion});
            }
            if (x.nugetVersion != null)
            {
                args.AddRange(new [] {"--nugetVersion", x.nugetVersion});
            }
            if (x.testScenariosNugetVersion != null)
            {
                args.AddRange(new [] {"--testScenariosNugetVersion", x.testScenariosNugetVersion});
            }

            #region infrastructure

            if (x.infrastructure != null)
            {
                var infraTxt = this.fileSystem.File.ReadAllText(x.infrastructure.FullName);
                dynamic infraJson = JsonConvert.DeserializeObject(infraTxt);
                if (infraJson != null)
                {
                    x.nugetRegistry ??= GenericUtilities.JsonObjectToUri(infraJson["NuGetRegistry"]);
                    x.npmRegistry ??= GenericUtilities.JsonObjectToUri(infraJson["NPMRegistry"]);
                    x.azureDevOpsCollectionUrl ??= GenericUtilities.JsonObjectToUri(infraJson["AzureDevopsCollectionURL"]);
                    x.agentPool ??= infraJson["AgentPool"]?.Value;
                    if (Enum.TryParse<AgentType>(infraJson["AgentType"]?.Value, out AgentType agentTypeParsed))
                    {
                        x.agentType ??= agentTypeParsed;
                    }
                    if (!string.IsNullOrEmpty(infraJson["NuGetRegistryUsername"]?.Value))
                    {
                        x.nugetRegistryUsername ??= infraJson["NuGetRegistryUsername"]?.Value;
                    }
                    if (!string.IsNullOrEmpty(infraJson["NuGetRegistryPassword"]?.Value))
                    {
                        x.nugetRegistryPassword ??= infraJson["NuGetRegistryPassword"]?.Value;
                    }
                    if(!string.IsNullOrEmpty(infraJson["CmfCliRepository"]?.Value))
                    {
                        x.cmfCliRepository ??= infraJson["CmfCliRepository"]?.Value;
                    }
                    if (!string.IsNullOrEmpty(infraJson["CmfPipelineRepository"]?.Value))
                    {
                        x.cmfPipelineRepository ??= infraJson["CmfPipelineRepository"]?.Value;
                    }
                    }
                }

            if (x.nugetRegistry == null ||
                x.npmRegistry == null ||
                x.azureDevOpsCollectionUrl == null ||
                x.ISOLocation == null ||
                x.agentPool == null)
            {
                throw new CliException("Missing infrastructure options. Either specify an infrastructure file with [--infrastructure] or specify each infrastructure option separately.");
            }

            if (x.nugetRegistry != null)
            {
                args.AddRange(new [] {"--nugetRegistry", x.nugetRegistry.AbsoluteUri});
            }
            if (x.npmRegistry != null)
            {
                args.AddRange(new [] {"--npmRegistry", x.npmRegistry.AbsoluteUri});
            }
            if (x.azureDevOpsCollectionUrl != null)
            {
                args.AddRange(new [] {"--azureDevOpsCollectionUrl", x.azureDevOpsCollectionUrl.AbsoluteUri});
            }
            if (x.ISOLocation != null)
            {
                args.AddRange(new [] {"--ISOLocation", x.ISOLocation.FullName});
            }
            if (x.nugetRegistryUsername != null)
            {
                args.AddRange(new[] { "--nugetRegistryUsername", x.nugetRegistryUsername });
            }
            if (x.nugetRegistryPassword != null)
            {
                args.AddRange(new[] { "--nugetRegistryPassword", x.nugetRegistryPassword });
            }

            args.AddRange(x.cmfCliRepository != null
                ? new[] { "--cmfCliRepository", x.cmfCliRepository }
                : new[] { "--cmfCliRepository", CliConstants.NpmJsUrl });
            args.AddRange(x.cmfPipelineRepository != null
                ? new[] { "--cmfPipelineRepository", x.cmfPipelineRepository }
                : new[] { "--cmfPipelineRepository", CliConstants.NpmJsUrl });
            
            if (!string.IsNullOrEmpty(x.agentPool))
            {
                args.AddRange(new [] {"--agentPool", x.agentPool});
            }
            args.AddRange(new [] {"--agentType", (x.agentType ??= AgentType.Hosted).ToString()});

            
            #endregion
            
            #region container-specific switches
            if (!string.IsNullOrEmpty(x.releaseCustomerEnvironment))
            {
                args.AddRange(new[] { "--releaseCustomerEnvironment", x.releaseCustomerEnvironment });
            }

            if (!string.IsNullOrEmpty(x.releaseSite))
            {
                args.AddRange(new[] { "--releaseSite", x.releaseSite });
            }

            if (!string.IsNullOrEmpty(x.releaseDeploymentPackage))
            {
                // we need to escape the @ symbol to avoid that commandline lib parses it as a file
                // https://github.com/dotnet/command-line-api/issues/816
                x.releaseDeploymentPackage = x.releaseDeploymentPackage.Length > 1 && x.releaseDeploymentPackage[0] == '\\'
                ? x.releaseDeploymentPackage[1..]
                : x.releaseDeploymentPackage;

                args.AddRange(new[] { "--releaseDeploymentPackage", $"{x.releaseDeploymentPackage}" });
            }

            if (!string.IsNullOrEmpty(x.releaseLicense))
            {
                args.AddRange(new[] { "--releaseLicense", x.releaseLicense });
            }

            if (!string.IsNullOrEmpty(x.releaseDeploymentTarget))
            {
                args.AddRange(new[] { "--releaseDeploymentTarget", x.releaseDeploymentTarget });
            }
            #endregion

            #region version-specific bits

            var version = Version.Parse(x.MESVersion);
            args.AddRange(new []{ "--dotnetSDKVersion", version.Major > 8 ? "6.0.201" : "3.1.102" });
            #endregion
            
            if (x.config != null)
            {
                args.AddRange(ParseConfigFile(x.config));
            }

            this.RunCommand(args);

            if (x.config != null)
            {
                var envConfigPath = this.fileSystem.Path.Join(FileSystemUtilities.GetProjectRoot(this.fileSystem, throwException: true).FullName, "EnvironmentConfigs");
                x.config.CopyTo(this.fileSystem.Path.Join(envConfigPath, x.config.Name));
            }
        }
    }
}

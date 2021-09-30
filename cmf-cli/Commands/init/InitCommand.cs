﻿using Cmf.Common.Cli.Attributes;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using Cmf.Common.Cli.Utilities;
using Newtonsoft.Json;

namespace Cmf.Common.Cli.Commands
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
                getDefaultValue: () => "1.1.0"
            ));
            cmd.AddOption(new Option<IFileInfo>(
                aliases: new[] { "-c", "--config" },
                parseArgument: argResult => Parse<IFileInfo>(argResult),
                isDefault: true,
                description: "Configuration file exported from Setup"));
            
            // template-time options. These are all mandatory
            cmd.AddOption(new Option<Uri>(
                aliases: new[] { "--repositoryUrl" },
                description: "Git repository URL"
            ));
            cmd.AddOption(new Option<IDirectoryInfo>(
                aliases: new[] { "--deploymentDir" },
                parseArgument: argResult => Parse<IDirectoryInfo>(argResult),
                isDefault: true,
                description: "Deployments directory (for releases). Don't specify if not using CI-Release."
            ));
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--MESVersion" },
                description: "Target MES version"
            ));
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--DevTasksVersion" },
                description: "Critical Manufacturing dev-tasks version"
            ));
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--HTMLStarterVersion" },
                description: "HTML Starter version"
            ));
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--yoGeneratorVersion" },
                description: "@criticalmanufacturing/html Yeoman generator version"
            ));
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--nugetVersion" },
                description: "NuGet versions to target. This is usually the MES version"
            ));
            // TODO: remove this one?
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--testScenariosNugetVersion" },
                description: "Test Scenarios Nuget Version"
            ));
            
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
                    if (infraJson["ISOLocation"]?.Value != null)
                    {
                        x.ISOLocation ??= this.fileSystem.FileInfo.FromFileName(infraJson["ISOLocation"]?.Value);
                    }
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
                }
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
            if (!string.IsNullOrEmpty(x.agentPool))
            {
                args.AddRange(new [] {"--agentPool", x.agentPool});
            }
            args.AddRange(new [] {"--agentType", (x.agentType ??= AgentType.Hosted).ToString()});
            #endregion

            if (x.config != null)
            {
                args.AddRange(ParseConfigFile(x.config));
            }
            
            this.RunCommand(args);

            if (x.config != null)
            {
                var envConfigPath = this.fileSystem.Path.Join(FileSystemUtilities.GetProjectRoot(this.fileSystem).FullName, "EnvironmentConfigs");
                x.config.CopyTo(this.fileSystem.Path.Join(envConfigPath, x.config.Name));
            }
        }
    }
}

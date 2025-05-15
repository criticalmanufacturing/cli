using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using System.Linq;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Commands;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Utilities;
using Cmf.CLI.Services;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Cmf.CLI.Commands
{
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
        public IFileInfo appConfig { get; set; }
        public IDirectoryInfo deploymentDir { get; set; }
        public string BaseVersion { get; set; }
        public string DevTasksVersion { get; set; }
        public string HTMLStarterVersion { get; set; }
        public string yoGeneratorVersion { get; set; }
        public string ngxSchematicsVersion { get; set; }
        public string nugetVersion { get; set; }
        public string testScenariosNugetVersion { get; set; }
        public IFileInfo infrastructure { get; set; }
        public Uri nugetRegistry { get; set; }
        public Uri npmRegistry { get; set; }
        public IFileInfo ISOLocation { get; set; }
        public string nugetRegistryUsername { get; set; }
        public string nugetRegistryPassword { get; set; }
        public RepositoryType repositoryType { get; set; }
        public string appId { get; set; }
        public string appName { get; set; }
        public string appDescription { get; set; }
        public string appAuthor { get; set; }
        public string appLicensedApplication { get; set; }
        public IFileInfo appIcon { get; set; }

        // ReSharper restore UnusedAutoPropertyAccessor.Global
        // ReSharper restore InconsistentNaming
    }

    /// <summary>
    /// Init command
    /// </summary>
    [CmfCommand("init", Id = "init", Description = "Initialize the content of a new repository for your project")]
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
                name: "projectName",
                parse: (argResult) => ParseArgument<string>(argResult)
            ));
            cmd.AddArgument(new Argument<string>(
                name: "rootPackageName",
                parse: (argResult) => ParseArgument<string>(argResult, "Cmf.Custom.Package"),
                isDefault: true
            ));
            cmd.AddArgument(new Argument<IDirectoryInfo>(
                name: "workingDir",
                parse: (argResult) => ParseArgument<IDirectoryInfo>(argResult, "."),
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
            cmd.AddOption(new Option<IFileInfo>(
                    aliases: ["--appConfig"],
                    parseArgument: argResult => Parse<IFileInfo>(argResult),
                    isDefault: true,
                    description: "App Configuration file")
                { IsRequired = false });
            cmd.AddOption(new Option<RepositoryType>(
                    aliases: new[] { "-t", "--repositoryType" },
                    getDefaultValue: () => CliConstants.DefaultRepositoryType,
                    description: "The type of repository we should initialize. Are we customizing MES or creating a new Application?")
                { IsRequired = true });
            
            // template-time options. These are all mandatory
            cmd.AddOption(new Option<IDirectoryInfo>(
                aliases: new[] { "--deploymentDir" },
                parseArgument: argResult => Parse<IDirectoryInfo>(argResult),
                description: "Deployments directory"
            ){ IsRequired = true });
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--baseVersion", "--MESVersion" },
                description: "Target CM framework/MES version"
            ) { IsRequired = true });
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--DevTasksVersion" },
                description: "Critical Manufacturing dev-tasks version. Only required if you are targeting a version lower than v10."
            ) { IsRequired = false });
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--HTMLStarterVersion" },
                description: "HTML Starter version. Only required if you are targeting a version lower than v10."
            ) { IsRequired = false });
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--yoGeneratorVersion" },
                description: "@criticalmanufacturing/html Yeoman generator version. Only required if you are targeting a version lower than v10."
            ) { IsRequired = false });
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--ngxSchematicsVersion" },
                description: "@criticalmanufacturing/ngx-schematics version. Only required if you are targeting a version equal or higher than v10."
            ) { IsRequired = false });
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

            const string OnlyIfTypeAppWarning = "Use only if repository type is App.";
            cmd.AddOption(new Option<string>(
                aliases: new[] { "--appId" },
                description: $"Application identifier. {OnlyIfTypeAppWarning}"
            )
            { IsRequired = false });

            cmd.AddOption(new Option<string>(
                aliases: new[] { "--appName" },
                description: $"Application name. {OnlyIfTypeAppWarning}"
            ) { IsRequired = false });

            cmd.AddOption(new Option<string>(
                aliases: new[] { "--appAuthor" },
                description: $"Application author. {OnlyIfTypeAppWarning}"
            )
            { IsRequired = false });

            cmd.AddOption(new Option<string>(
                aliases: new[] { "--appDescription" },
                description: $"Application description. {OnlyIfTypeAppWarning}"
            ) { IsRequired = false });

            cmd.AddOption(new Option<string>(
                aliases: new[] { "--appLicensedApplication" },
                description: $"License for new application. {OnlyIfTypeAppWarning}"
            ) { IsRequired = false });

            cmd.AddOption(new Option<IFileInfo>(
                aliases: new[] { "--appIcon" },
                parseArgument: argResult => Parse<IFileInfo>(argResult),
                description: $"Application icon. {OnlyIfTypeAppWarning}"
            ) { IsRequired = false });

            // Add the handler
            cmd.Handler = CommandHandler
                .Create((InitArguments args) =>
                {
                    this.Execute(args);
                });
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
                "--projectName", x.projectName,
                "--repositoryType", x.repositoryType.ToString(),
                "--baseLayer", x.repositoryType == RepositoryType.App ? BaseLayer.Core.ToString() : BaseLayer.MES.ToString(),
                "--CLIVersion", ExecutionContext.CurrentVersion
            };

            if (x.repositoryType == RepositoryType.App)
            {
                var requiredParameters = new[]
                {
                    new { Parameter = "appId", Value = x.appId },
                    new { Parameter = "appName", Value = x.appName },
                    new { Parameter = "appAuthor", Value = x.appAuthor },
                    new { Parameter = "appDescription", Value = x.appDescription },
                    new { Parameter = "appLicensedApplication", Value = x.appLicensedApplication },
                };

                var errors = new List<string>();
                const string AppParameterMissingError = "--{0} is required when repository type is App.";

                foreach (var parameter in requiredParameters)
                {
                    if (string.IsNullOrWhiteSpace(parameter.Value))
                    {
                        errors.Add(string.Format(AppParameterMissingError, parameter.Parameter));
                    }
                }

                if (errors.Count > 0)
                {
                    throw new CliException(string.Join(Environment.NewLine, errors));
                }

                var appIconPath = x.appIcon != null && AppIconUtilities.IsIconValid(x.appIcon.FullName)
                    ? $"assets/{x.appIcon.Name}"
                    : $"assets/{CliConstants.DefaultAppIcon}";

                args.AddRange(new[]
                {
                    "--appName", x.appName,
                    "--appNameLowerNoSpaces", x.appName.ToLower().Replace(" ", ""),
                    "--appId", x.appId,
                    "--appIcon", appIconPath,
                    "--appDescription", x.appDescription,
                    "--appTargetFramework", x.BaseVersion,
                    "--appAuthor", x.appAuthor,
                    "--appLicensedApplication", x.appLicensedApplication
                });
            }

            if (x.version != null)
            {
                args.AddRange(new [] {"--packageVersion", x.version});
            }

            if (x.deploymentDir != null)
            {
                args.AddRange(new [] {"--deploymentDir", x.deploymentDir.FullName});
                args.AddRange(new [] {"--DeliveredRepo", $"{x.deploymentDir.FullName}\\Delivered"});
                args.AddRange(new[] { "--CIRepo", $"{x.deploymentDir.FullName}\\CIPackages" });
            }
            if (x.BaseVersion != null)
            {
                args.AddRange(new [] {"--MESVersion", x.BaseVersion});
            }
            
            args.AddRange(new [] {"--DevTasksVersion", x.DevTasksVersion ?? ""});
            args.AddRange(new [] {"--HTMLStarterVersion", x.HTMLStarterVersion ?? ""});
            args.AddRange(new [] {"--yoGeneratorVersion", x.yoGeneratorVersion ?? ""});
            args.AddRange(new [] {"--ngxSchematicsVersion", x.ngxSchematicsVersion ?? ""});
            
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

            if (x.nugetRegistry == null ||
                x.npmRegistry == null)
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


            
            #endregion           

            #region version-specific bits

            var version = Version.Parse(x.BaseVersion);
            args.AddRange(new []{ "--dotnetSDKVersion", ExecutionContext.ServiceProvider.GetService<IDependencyVersionService>().DotNetSdk(version) });
            
            if (version.Major > 9)
            {
                if (string.IsNullOrWhiteSpace(x.ngxSchematicsVersion))
                {
                    throw new CliException(
                        "--ngxSchematicsVersion is required when targeting a base version of 10 or above.");
                }
            }
            else
            {
                var errors = new List<string>();
                if (string.IsNullOrWhiteSpace(x.DevTasksVersion))
                {
                    errors.Add("--DevTasksVersion is required when targeting a base version lower than 10.");
                }
                if (string.IsNullOrWhiteSpace(x.HTMLStarterVersion))
                {
                    errors.Add("--HTMLStarterVersion is required when targeting a base version lower than 10.");
                }
                if (string.IsNullOrWhiteSpace(x.yoGeneratorVersion))
                {
                    errors.Add("--yoGeneratorVersion is required when targeting a base version lower than 10.");
                }
                if (errors.Count > 0)
                {
                    throw new CliException(string.Join(Environment.NewLine, errors));
                }
            }
            #endregion
            
            if (x.config != null)
            {
                args.AddRange(ParseConfigFile(x.config));
            }
            
            if (x.appConfig != null)
            {
                args.AddRange(new string[] {"--AppEnvironmentConfig", x.appConfig.Name});
            }
            
            this.RunCommand(args);
            
            // Copy app icon to assets
            if (x.appIcon != null)
            {
                var assetsPath = fileSystem.Path.Join(FileSystemUtilities.GetProjectRoot(fileSystem, throwException: true).FullName, "assets");
                x.appIcon.CopyTo(fileSystem.Path.Join(assetsPath, x.appIcon.Name));
            }
            
            // Copy MES config to Environment Configs
            if (x.config != null)
            {
                var envConfigPath = this.fileSystem.Path.Join(FileSystemUtilities.GetProjectRoot(this.fileSystem, throwException: true).FullName, "EnvironmentConfigs");
                x.config.CopyTo(this.fileSystem.Path.Join(envConfigPath, x.config.Name));
                fileSystem.FileInfo.New(this.fileSystem.Path.Join(envConfigPath, ".gitkeep")).Delete();
            }
            
            // Copy app config to Environment Configs
            if (x.appConfig != null)
            {
                var envConfigPath = this.fileSystem.Path.Join(FileSystemUtilities.GetProjectRoot(this.fileSystem, throwException: true).FullName, "EnvironmentConfigs");
                x.appConfig.CopyTo(this.fileSystem.Path.Join(envConfigPath, x.appConfig.Name));
                fileSystem.FileInfo.New(this.fileSystem.Path.Join(envConfigPath, ".gitkeep")).Delete();
            }
            
            if (x.repositoryType == RepositoryType.App)
            {
                Log.Information("Apps need to have an ApplicationVersion package. Make sure you create at least one business package for your application using the addApplicationVersionAssembly flag");
            }
        }

        bool isToIgnoreOptionToken = false;
        /// <summary>
        /// parse argument specific for InitCommand
        /// This is needed until this issue is not fixed:
        /// https://github.com/dotnet/command-line-api/issues/1879#issuecomment-1689816336
        /// this method will check if the argument value starts with - and will ignore
        /// that value and also the next token
        /// </summary>
        /// <typeparam name="T">the (target) type of the argument/parameter</typeparam>
        /// <param name="argResult">the arguments to parse</param>
        /// <param name="default">the default value if no value is passed for the argument</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        private T ParseArgument<T>(ArgumentResult argResult, string @default = null)
        {
            var argValue = @default;

            if (!isToIgnoreOptionToken && argResult.Tokens.Any())
            {
                var isOptionAndShouldBeIgnored = isToIgnoreOptionToken || (bool)argResult.Tokens?.FirstOrDefault()?.Value.StartsWith("-");
                if (isOptionAndShouldBeIgnored)
                {
                    isToIgnoreOptionToken = true;
                }
                else
                {
                    argValue = argResult.Tokens.First().Value;
                }
            }
            //reset flag
            else
            {
                isToIgnoreOptionToken = false;
            }

            if (string.IsNullOrEmpty(argValue))
            {
                return default;
            }

            return typeof(T) switch
            {
                { } dirType when dirType == typeof(IDirectoryInfo) => (T)this.fileSystem.DirectoryInfo.New(argValue),
                { } fileType when fileType == typeof(IFileInfo) => (T)this.fileSystem.FileInfo.New(argValue),
                { } stringType when stringType == typeof(string) => (T)(object)argValue,
                _ => throw new ArgumentOutOfRangeException("This method only parses paths or strings")
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
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
        public Uri deploymentDir { get; set; }
        public Uri ciRepo { get; set; }
        public List<Uri> releaseRepos { get; set; }
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
        public string Tenant { get; set; }
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
            var projectNameArgument = new Argument<string>("projectName");
            projectNameArgument.CustomParser = argResult => ParseArgument<string>(argResult);
            cmd.Arguments.Add(projectNameArgument);

            var rootPackageNameArgument = new Argument<string>("rootPackageName");
            rootPackageNameArgument.CustomParser = argResult => ParseArgument<string>(argResult, "Cmf.Custom.Package");
            rootPackageNameArgument.DefaultValueFactory = _ => "Cmf.Custom.Package";
            cmd.Arguments.Add(rootPackageNameArgument);

            var workingDirArgument = new Argument<IDirectoryInfo>("workingDir")
            {
                Description = "Working Directory"
            };
            workingDirArgument.CustomParser = argResult => ParseArgument<IDirectoryInfo>(argResult, ".");
            workingDirArgument.DefaultValueFactory = _ => ParseArgument<IDirectoryInfo>(null, ".");
            cmd.Arguments.Add(workingDirArgument);

            var versionOption = new Option<string>("--version")
            {
                Description = "Package Version",
                DefaultValueFactory = _ => "1.0.0"
            };
            cmd.Options.Add(versionOption);

            var configOption = new Option<IFileInfo>("--config", "-c")
            {
                Description = "Configuration file exported from Setup",
                Required = true
            };
            configOption.CustomParser = argResult => Parse<IFileInfo>(argResult);
            cmd.Options.Add(configOption);

            var appConfigOption = new Option<IFileInfo>("--appConfig")
            {
                Description = "App Configuration file",
                Required = false
            };
            appConfigOption.CustomParser = argResult => Parse<IFileInfo>(argResult);
            cmd.Options.Add(appConfigOption);

            var repositoryTypeOption = new Option<RepositoryType>("--repositoryType", "-t")
            {
                Description = "The type of repository we should initialize. Are we customizing MES or creating a new Application?",
                DefaultValueFactory = _ => CliConstants.DefaultRepositoryType,
                Required = true
            };
            cmd.Options.Add(repositoryTypeOption);

            // template-time options. These are all mandatory
            var baseVersionOption = new Option<string>("--baseVersion", "--MESVersion")
            {
                Description = "Target CM framework/MES version",
                Required = true
            };
            cmd.Options.Add(baseVersionOption);

            var devTasksVersionOption = new Option<string>("--DevTasksVersion")
            {
                Description = "Critical Manufacturing dev-tasks version. Only required if you are targeting a version lower than v10.",
                Required = false
            };
            cmd.Options.Add(devTasksVersionOption);

            var htmlStarterVersionOption = new Option<string>("--HTMLStarterVersion")
            {
                Description = "HTML Starter version. Only required if you are targeting a version lower than v10.",
                Required = false
            };
            cmd.Options.Add(htmlStarterVersionOption);

            var yoGeneratorVersionOption = new Option<string>("--yoGeneratorVersion")
            {
                Description = "@criticalmanufacturing/html Yeoman generator version. Only required if you are targeting a version lower than v10.",
                Required = false
            };
            cmd.Options.Add(yoGeneratorVersionOption);

            var ngxSchematicsVersionOption = new Option<string>("--ngxSchematicsVersion")
            {
                Description = "@criticalmanufacturing/ngx-schematics version. Only required if you are targeting a version equal or higher than v10.",
                Required = false
            };
            cmd.Options.Add(ngxSchematicsVersionOption);

            var nugetVersionOption = new Option<string>("--nugetVersion")
            {
                Description = "NuGet versions to target. This is usually the MES version",
                Required = true
            };
            cmd.Options.Add(nugetVersionOption);

            var testScenariosNugetVersionOption = new Option<string>("--testScenariosNugetVersion")
            {
                Description = "Test Scenarios Nuget Version",
                Required = true
            };
            cmd.Options.Add(testScenariosNugetVersionOption);

            // repositories
            var deploymentDirOption = new Option<Uri>("--deploymentDir")
            {
                Description = "Deployments directory. Deprecated, supports only file paths/network shares. When using NPM feeds, use --ciRepo and --releaseRepos instead.",
                Required = false,
                CustomParser = argResult => ParseUri(argResult)
            };
            cmd.Options.Add(deploymentDirOption);

            var ciRepoOption = new Option<Uri>("--ciRepo")
            {
                Description = "The repository (network share or NPM feed) where CI packages are published to. Must be passed only and only if --deploymentDir is not.",
                Required = false,
                CustomParser = argResult => ParseUri(argResult)
            };
            cmd.Options.Add(ciRepoOption);

            var releaseReposOption = new Option<List<Uri>>("--releaseRepos")
            {
                Description = "The list of repositories (network shares and/or NPM feeds) where approved packages to be delivered are published to. Must be passed only and only if --deploymentDir is not.",
                Arity = ArgumentArity.ZeroOrMore,
                AllowMultipleArgumentsPerToken = true,
                CustomParser = argResult => ParseUriList(argResult)
            };
            cmd.Options.Add(releaseReposOption);

            var tenantOption = new Option<string>("--tenant")
            {
                Description = "MES Tenant Name",
                Required = false
            };
            cmd.Options.Add(tenantOption);

            // infra options
            var infrastructureOption = new Option<IFileInfo>("--infrastructure", "--infra")
            {
                Description = "Infrastructure JSON file"
            };
            infrastructureOption.CustomParser = argResult => Parse<IFileInfo>(argResult);
            cmd.Options.Add(infrastructureOption);

            var nugetRegistryOption = new Option<Uri>("--nugetRegistry")
            {
                Description = "NuGet registry that contains the MES packages",
                CustomParser = argResult => ParseUri(argResult)
            };
            cmd.Options.Add(nugetRegistryOption);

            var npmRegistryOption = new Option<Uri>("--npmRegistry")
            {
                Description = "NPM registry that contains the MES packages",
                CustomParser = argResult => ParseUri(argResult)
            };
            cmd.Options.Add(npmRegistryOption);

            var isoLocationOption = new Option<IFileInfo>("--ISOLocation")
            {
                Description = "MES ISO file"
            };
            isoLocationOption.CustomParser = argResult => Parse<IFileInfo>(argResult);
            cmd.Options.Add(isoLocationOption);

            var nugetRegistryUsernameOption = new Option<string>("--nugetRegistryUsername")
            {
                Description = "NuGet registry username"
            };
            cmd.Options.Add(nugetRegistryUsernameOption);

            var nugetRegistryPasswordOption = new Option<string>("--nugetRegistryPassword")
            {
                Description = "NuGet registry password"
            };
            cmd.Options.Add(nugetRegistryPasswordOption);

            const string OnlyIfTypeAppWarning = "Use only if repository type is App.";

            var appIdOption = new Option<string>("--appId")
            {
                Description = $"Application identifier. {OnlyIfTypeAppWarning}",
                Required = false
            };
            cmd.Options.Add(appIdOption);

            var appNameOption = new Option<string>("--appName")
            {
                Description = $"Application name. {OnlyIfTypeAppWarning}",
                Required = false
            };
            cmd.Options.Add(appNameOption);

            var appAuthorOption = new Option<string>("--appAuthor")
            {
                Description = $"Application author. {OnlyIfTypeAppWarning}",
                Required = false
            };
            cmd.Options.Add(appAuthorOption);

            var appDescriptionOption = new Option<string>("--appDescription")
            {
                Description = $"Application description. {OnlyIfTypeAppWarning}",
                Required = false
            };
            cmd.Options.Add(appDescriptionOption);

            var appLicensedApplicationOption = new Option<string>("--appLicensedApplication")
            {
                Description = $"License for new application. {OnlyIfTypeAppWarning}",
                Required = false
            };
            cmd.Options.Add(appLicensedApplicationOption);

            var appIconOption = new Option<IFileInfo>("--appIcon")
            {
                Description = $"Application icon. {OnlyIfTypeAppWarning}",
                Required = false
            };
            appIconOption.CustomParser = argResult => Parse<IFileInfo>(argResult);
            cmd.Options.Add(appIconOption);

            // Add the handler
            cmd.SetAction((parseResult, cancellationToken) =>
            {
                var args = new InitArguments
                {
                    projectName = parseResult.GetValue(projectNameArgument),
                    rootPackageName = parseResult.GetValue(rootPackageNameArgument),
                    workingDir = parseResult.GetValue(workingDirArgument),
                    version = parseResult.GetValue(versionOption),
                    config = parseResult.GetValue(configOption),
                    appConfig = parseResult.GetValue(appConfigOption),
                    repositoryType = parseResult.GetValue(repositoryTypeOption),
                    BaseVersion = parseResult.GetValue(baseVersionOption),
                    DevTasksVersion = parseResult.GetValue(devTasksVersionOption),
                    HTMLStarterVersion = parseResult.GetValue(htmlStarterVersionOption),
                    yoGeneratorVersion = parseResult.GetValue(yoGeneratorVersionOption),
                    ngxSchematicsVersion = parseResult.GetValue(ngxSchematicsVersionOption),
                    nugetVersion = parseResult.GetValue(nugetVersionOption),
                    testScenariosNugetVersion = parseResult.GetValue(testScenariosNugetVersionOption),
                    deploymentDir = parseResult.GetValue(deploymentDirOption),
                    ciRepo = parseResult.GetValue(ciRepoOption),
                    releaseRepos = parseResult.GetValue(releaseReposOption),
                    Tenant = parseResult.GetValue(tenantOption),
                    infrastructure = parseResult.GetValue(infrastructureOption),
                    nugetRegistry = parseResult.GetValue(nugetRegistryOption),
                    npmRegistry = parseResult.GetValue(npmRegistryOption),
                    ISOLocation = parseResult.GetValue(isoLocationOption),
                    nugetRegistryUsername = parseResult.GetValue(nugetRegistryUsernameOption),
                    nugetRegistryPassword = parseResult.GetValue(nugetRegistryPasswordOption),
                    appId = parseResult.GetValue(appIdOption),
                    appName = parseResult.GetValue(appNameOption),
                    appAuthor = parseResult.GetValue(appAuthorOption),
                    appDescription = parseResult.GetValue(appDescriptionOption),
                    appLicensedApplication = parseResult.GetValue(appLicensedApplicationOption),
                    appIcon = parseResult.GetValue(appIconOption)
                };

                Execute(args);
                return Task.FromResult(0);
            });
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        internal void Execute(InitArguments x)
        {
            using var activity = Core.Objects.ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);
            var args = new List<string>()
            {
                // engine options
                "--output", x.workingDir.FullName,

                // template symbols
                "--customPackageName", x.rootPackageName,
                "--projectName", x.projectName,
                "--repositoryType", x.repositoryType.ToString(),
                "--baseLayer", x.repositoryType == RepositoryType.App ? BaseLayer.Core.ToString() : BaseLayer.MES.ToString(),
                "--CLIVersion", Core.Objects.ExecutionContext.CurrentVersion
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
                if (x.ciRepo != null)
                {
                    throw new CliException("Invalid option `--ciRepo` when also using `--deploymentDir`. Use one or the other, but not both.");
                }
                if (x.releaseRepos != null && x.releaseRepos.Count > 0)
                {
                    throw new CliException("Invalid option `--releaseRepos` when also using `--deploymentDir`. Use one or the other, but not both.");
                }

                string deploymentDirPath = null;
                bool isUncPath = false;
                
                try
                {
                    // Check if it's an absolute URI first
                    if (x.deploymentDir.IsAbsoluteUri)
                    {
                        try
                        {
                            isUncPath = x.deploymentDir.IsUnc;
                            deploymentDirPath = x.deploymentDir.OriginalString;
                        }
                        catch
                        {
                            // If IsUnc fails, fall back to LocalPath for file URIs
                            deploymentDirPath = x.deploymentDir.IsFile ? 
                                this.fileSystem.DirectoryInfo.New(x.deploymentDir.LocalPath).FullName : 
                                x.deploymentDir.OriginalString;
                        }
                    }
                    else
                    {
                        // For relative URIs, resolve to absolute path
                        var relativePath = x.deploymentDir.OriginalString;
                        deploymentDirPath = this.fileSystem.DirectoryInfo.New(relativePath).FullName;
                        isUncPath = false; // Relative paths converted to absolute are not UNC
                    }
                }
                catch
                {
                    // Last resort: use original string and assume it's not UNC
                    deploymentDirPath = x.deploymentDir.OriginalString;
                    isUncPath = false;
                }

                // Determine the appropriate path separator
                var pathSeparator = isUncPath ? "\\" : System.IO.Path.DirectorySeparatorChar.ToString();

                args.AddRange(new[] { "--deploymentDir", deploymentDirPath });
                args.AddRange(new[] { "--DeliveredRepo", $"{deploymentDirPath}{pathSeparator}Delivered" });
                args.AddRange(new[] { "--CIRepo", $"{deploymentDirPath}{pathSeparator}CIPackages" });
            }
            else
            {
                if (x.ciRepo == null)
                {
                    throw new CliException("Missing option `--ciRepo` or `--deploymentDir`. Please specify one or the other (but not both).");
                }
                if (x.releaseRepos == null || x.releaseRepos.Count == 0)
                {
                    throw new CliException("Missing option `--releaseRepos` or `--deploymentDir`. Please specify one or the other (but not both).");
                }

                args.AddRange(new[] { "--ReleaseRepos", string.Join(";", x.releaseRepos.Select(r => r.AbsoluteUri)) });
                args.AddRange(new[] { "--CIRepo", x.ciRepo.AbsoluteUri });
            }

            if (x.BaseVersion != null)
            {
                args.AddRange(new[] { "--MESVersion", x.BaseVersion });
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
            
            if (!string.IsNullOrEmpty(x.Tenant))
            {
                args.AddRange(new string[] { "--Tenant", x.Tenant });
            }
            
            if (!args.Contains("--Tenant"))
            {
                throw new CliException("Tenant information is missing. Please provide it either in the config file or through the --tenant option.");
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
        /// Parse list of URIs from argument result
        /// </summary>
        /// <param name="argResult">the arguments to parse</param>
        /// <returns>List of parsed URIs or null if no tokens</returns>
        private List<Uri> ParseUriList(ArgumentResult argResult)
        {
            var array = ParseUriArray(argResult);
            return array != null ? new List<Uri>(array) : null;
        }

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

            if (!isToIgnoreOptionToken && argResult != null && argResult.Tokens.Any())
            {
                var firstTokenValue = argResult.Tokens?.FirstOrDefault()?.Value;
                var isOptionAndShouldBeIgnored = isToIgnoreOptionToken || (firstTokenValue?.StartsWith("-") ?? false);
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
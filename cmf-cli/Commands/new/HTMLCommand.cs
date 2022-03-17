using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;
using Cmf.Common.Cli.Builders;
using Cmf.Common.Cli.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cmf.Common.Cli.Commands.New
{
    /// <summary>
    /// Generates Help/Documentation package structure
    /// </summary>
    [CmfCommand("html", Parent = "new")]
    public class HTMLCommand : UILayerTemplateCommand
    {
        private JsonDocument projectConfig = null;

        /// <inheritdoc />
        public HTMLCommand() : base("html", PackageType.HTML)
        {
        }

        /// <inheritdoc />
        public HTMLCommand(IFileSystem fileSystem) : base("html", PackageType.HTML, fileSystem)
        {
        }
        
        /// <inheritdoc />
        public override void Configure(Command cmd)
        {
            base.GetBaseCommandConfig(cmd);
            cmd.AddOption(new Option<IFileInfo>(
                aliases: new[] { "--htmlPkg", "--htmlPackage" },
                description: "Path to the MES Presentation HTML package",
                isDefault: false,
                parseArgument: argResult => Parse<IFileInfo>(argResult)
            )
            { IsRequired = true });
            cmd.Handler = CommandHandler.Create<IDirectoryInfo, string, IFileInfo>(this.Execute);
        }

        /// <inheritdoc />
        protected override List<string> GenerateArgs(IDirectoryInfo projectRoot, IDirectoryInfo workingDir, List<string> args, JsonDocument projectConfig)
        {
            var relativePathToRoot =
                this.fileSystem.Path.Join("..", //always one level deeper
                    this.fileSystem.Path.GetRelativePath(
                        workingDir.FullName,
                        projectRoot.FullName)
                ).Replace("\\", "/");

            this.projectConfig = projectConfig;

            args.AddRange(new []
            {
                "--rootRelativePath", relativePathToRoot 
            });

            return args;
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="workingDir">nearest root package</param>
        /// <param name="version">package version</param>
        /// <param name="htmlPackage">The MES Presentation HTML package path</param>
        public void Execute(IDirectoryInfo workingDir, string version, IFileInfo htmlPackage)
        {
            base.Execute(workingDir, version); // create package base - generate cmfpackage.json
            var nameIdx = Array.FindIndex(base.executedArgs, item => string.Equals(item, "--name"));
            var pkgName = base.executedArgs[nameIdx + 1];
            var htmlStarterVersion = projectConfig.RootElement.GetProperty("HTMLStarterVersion").GetString();
            // clone HTMLStarter content in the folder
            var pkgFolder = workingDir.GetDirectories(pkgName).FirstOrDefault();
            if (!pkgFolder?.Exists ?? false)
            {
                throw new Exception($"Package folder {pkgFolder.Name} does not exist. This is a template error. Please open an issue on GitHub.");
            }
            this.CloneHTMLStarter(htmlStarterVersion, pkgFolder);
            
            // root package.json
            var rootPkgJsonPath = this.fileSystem.Path.Join(pkgFolder.FullName, "package.json");
            var json = fileSystem.File.ReadAllText(rootPkgJsonPath);
            dynamic rootPkgJson = JsonConvert.DeserializeObject(json);
            if (rootPkgJson == null)
            {
                throw new Exception("Could not load package.json");
            }
            var devTasksVersion = projectConfig.RootElement.GetProperty("DevTasksVersion").GetString();
            var yoGeneratorVersion = projectConfig.RootElement.GetProperty("YoGeneratorVersion").GetString();
            var projectName = projectConfig.RootElement.GetProperty("ProjectName").GetString();
            var repositoryURL = projectConfig.RootElement.GetProperty("RepositoryURL").GetString();
            var tenant = projectConfig.RootElement.GetProperty("Tenant").GetString();
            rootPkgJson.devDependencies["@criticalmanufacturing/dev-tasks"] = devTasksVersion;
            rootPkgJson.devDependencies["@criticalmanufacturing/generator-html"] = yoGeneratorVersion;
            rootPkgJson.name = $"customization.{tenant?.ToLowerInvariant()}";
            rootPkgJson.description = $"HTML customization package for {projectName}";
            rootPkgJson.repository.url = repositoryURL;
            json = JsonConvert.SerializeObject(rootPkgJson, Formatting.Indented);
            this.fileSystem.File.WriteAllText(rootPkgJsonPath, json);
            Log.Verbose("Updated package.json");
            
            // .dev-tasks.json
            var devTasksPath = this.fileSystem.Path.Join(pkgFolder.FullName, ".dev-tasks.json");
            var devTasksStr = fileSystem.File.ReadAllText(devTasksPath);
            dynamic devTasksJson = JsonConvert.DeserializeObject(devTasksStr);
            if (devTasksJson == null)
            {
                throw new Exception("Could not load .dev-tasks.json");
            }
            var npmRegistry = projectConfig.RootElement.GetProperty("NPMRegistry").GetString();
            var mesVersion = projectConfig.RootElement.GetProperty("MESVersion").GetString();
            devTasksJson.packagePrefix = "customization";
            devTasksJson.webAppPrefix = "customization";
            devTasksJson.registry = npmRegistry;
            devTasksJson.isWebAppCompilable = true;
            devTasksJson.channel = $"release-{mesVersion?.Replace(".", "")}";
            devTasksStr = JsonConvert.SerializeObject(devTasksJson, Formatting.Indented);
            this.fileSystem.File.WriteAllText(devTasksPath, devTasksStr);
            Log.Verbose("Updated .dev-tasks.json");

            // install dev dependencies/tooling
            Log.Verbose("Executing npm install, this will take a while...");
            (new NPMCommand() { Command = "install", WorkingDirectory = pkgFolder }).Exec();

            var htmlDevTasksConfigPath = this.fileSystem.Path.GetTempFileName();
            var htmlDevTasksConfigJson = 
$@"{{
    ""answers"": {{
        ""packagePrefix"": ""customization"",
        ""registry"": ""{npmRegistry}"",
        ""confirmSkip"": ""y"",
        ""channel"": ""release-{mesVersion?.Replace(".", "")}""
    }}
}}";
            this.fileSystem.File.WriteAllText(htmlDevTasksConfigPath, htmlDevTasksConfigJson);
            var htmlWebAppConfigPath = this.fileSystem.Path.GetTempFileName();
            var htmlWebAppConfigJson = 
@"{
    ""answers"": {
        ""appName"": ""web"",
        ""basePackage"": ""@criticalmanufacturing/mes-ui-web""
    }
}";
            this.fileSystem.File.WriteAllText(htmlWebAppConfigPath, htmlWebAppConfigJson);
            
            // create web app
            // npx yeoman-gen-run --name @criticalmanufacturing/html --config "$pathHTMLConfig" -- --keep
            Log.Verbose("Generate web app, this will take a while...");
            (new NPXCommand()
            {
                Command = "yeoman-gen-run",
                WorkingDirectory = pkgFolder,
                Args = new []
                {
                    "--name", "@criticalmanufacturing/html", "--config", htmlDevTasksConfigPath, "--", "--keep"
                }
            }).Exec();
            // npx yeoman-gen-run --name @criticalmanufacturing/html:application --config "$path"
            (new NPXCommand()
            {
                Command = "yeoman-gen-run",
                WorkingDirectory = pkgFolder,
                Args = new []
                {
                    "--name", "@criticalmanufacturing/html:application", "--config", htmlWebAppConfigPath
                }
            }).Exec();
            Log.Verbose("Web app generated!");
            
            
            Log.Debug("Obtaining sources from MES Presentation HTML package...");
            var htmlPkgConfigJsonStr = FileSystemUtilities.GetFileContentFromPackage(htmlPackage.FullName, "config.json");
            // replace tokens that would break Json parse
            htmlPkgConfigJsonStr = Regex.Replace(htmlPkgConfigJsonStr, @"\$\([^\)]+\)", "0", RegexOptions.Multiline);
            dynamic htmlPkgConfigJson = JsonConvert.DeserializeObject(htmlPkgConfigJsonStr);
            
            // config.json
            var configJsonPath = this.fileSystem.Path.Join(pkgFolder.FullName, "apps", 
                this.fileSystem.Path.Join("customization.web", "config.json"));
            var configJsonStr = fileSystem.File.ReadAllText(configJsonPath);
            configJsonStr = Regex.Replace(configJsonStr, @"\$\([^\)]+\)", "0", RegexOptions.Multiline);
            dynamic configJsonJson = JsonConvert.DeserializeObject(configJsonStr);
            if (configJsonJson == null)
            {
                throw new Exception("Could not load config.json");
            }
            var restPort = int.Parse(projectConfig.RootElement.GetProperty("RESTPort").GetString());
            configJsonJson.host.rest.enableSsl = false;
            configJsonJson.host.rest.address = "localhost";
            configJsonJson.host.rest.port = restPort;
            configJsonJson.host.isLoadBalancerEnabled = false;
            configJsonJson.host.tenant.name = tenant;
            configJsonJson.general.defaultDomain = projectConfig.RootElement.GetProperty("DefaultDomain").GetString();
            configJsonJson.general.environmentName = $"{projectName}Local";
            configJsonJson.version = $"{projectName} $(Build.BuildNumber) - {mesVersion}";
            configJsonJson.packages.available = htmlPkgConfigJson.packages.available;
            configJsonJson.packages.bundlePath = "node_modules/@criticalmanufacturing/mes-ui-web/bundles";
            configJsonStr = JsonConvert.SerializeObject(configJsonJson, Formatting.Indented);
            this.fileSystem.File.WriteAllText(configJsonPath, configJsonStr);
            Log.Verbose("Updated config.json");
            
            // use lodash
            // use custom LBOs
            Log.Verbose("Updating web app package.json");
            var webAppPath = this.fileSystem.Path.Join(pkgFolder.FullName, 
                "apps", "customization.web");
            var webAppPkgJsonPath = this.fileSystem.Path.Join(webAppPath, "package.json");
            var projectRoot = FileSystemUtilities.GetProjectRoot(this.fileSystem);
            var relativePathToRoot =
                //this.fileSystem.Path.Join( //always two levels deep, this is the depth of the business solution projects
                this.fileSystem.Path.GetRelativePath(
                        webAppPath, projectRoot.FullName)
                    //);
                    .Replace("\\", "/");
            var webAppPkgJsonStr= fileSystem.File.ReadAllText(webAppPkgJsonPath);
            dynamic webAppPkgJson = JsonConvert.DeserializeObject(webAppPkgJsonStr);
            webAppPkgJson.dependencies["lodash"] = "3.10.1";
            webAppPkgJson.cmfLinkDependencies["cmf.lbos"] = $"file:{relativePathToRoot}/Libs/LBOs/TypeScript";
            webAppPkgJsonStr = JsonConvert.SerializeObject(webAppPkgJson, Formatting.Indented);
            this.fileSystem.File.WriteAllText(webAppPkgJsonPath, webAppPkgJsonStr);
            
            // remove package locks
            Log.Verbose("Generating accurate package-locks for future installs. This will take a while...");
            var rootPkgLockJsonPath = this.fileSystem.Path.Join(pkgFolder.FullName, "package-lock.json");
            this.fileSystem.File.Delete(rootPkgLockJsonPath);
            var webAppPkgLockJsonPath = this.fileSystem.Path.Join(
                fileSystem.Path.Join(pkgFolder.FullName, "apps", "customization.web"), "package-lock.json");
            this.fileSystem.File.Delete(webAppPkgLockJsonPath);
            // gulp install --update
            (new GulpCommand()
            {
                WorkingDirectory = pkgFolder,
                GulpFile = "gulpfile.js",
                Task = "install",
                DisplayName = "Gulp Install",
                GulpJS = "node_modules/gulp/bin/gulp.js",
                Args = new [] { "--update" },
            }).Exec();
            Log.Information("HTML package generated");
        }
    }
}
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

namespace Cmf.Common.Cli.Commands.New
{
    /// <summary>
    /// Generates Help/Documentation package structure
    /// </summary>
    [CmfCommand("help", Parent = "new")]
    public class HelpCommand : UILayerTemplateCommand
    {
        private JsonDocument projectConfig = null;

        /// <inheritdoc />
        public HelpCommand() : base("help", PackageType.Help)
        {
        }

        /// <inheritdoc />
        public HelpCommand(IFileSystem fileSystem) : base("help", PackageType.Help, fileSystem)
        {
        }
        
        /// <inheritdoc />
        public override void Configure(Command cmd)
        {
            base.GetBaseCommandConfig(cmd);
            cmd.AddOption(new Option<IFileInfo>(
                aliases: new[] { "--docPkg", "--documentationPackage" },
                description: "Path to the MES documentation package",
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
        /// <param name="documentationPackage">The MES documentation package path</param>
        public void Execute(IDirectoryInfo workingDir, string version, IFileInfo documentationPackage)
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
            rootPkgJson.devDependencies["@criticalmanufacturing/dev-tasks"] = devTasksVersion;
            rootPkgJson.devDependencies["@criticalmanufacturing/generator-html"] = yoGeneratorVersion;
            rootPkgJson.name = "cmf.docs.area";
            rootPkgJson.description = $"Help customization package for {projectName}";
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
            devTasksJson.packagePrefix = "cmf.docs.area";
            devTasksJson.webAppPrefix = "cmf.docs.area";
            devTasksJson.registry = npmRegistry;
            devTasksJson.channel = $"release-{mesVersion?.Replace(".", "")}";
            devTasksStr = JsonConvert.SerializeObject(devTasksJson, Formatting.Indented);
            this.fileSystem.File.WriteAllText(devTasksPath, devTasksStr);
            Log.Verbose("Updated .dev-tasks.json");

            // install dev dependencies/tooling
            Log.Verbose("Executing npm install, this will take a while...");
            (new NPMCommand() { Command = "install", WorkingDirectory = pkgFolder }).Exec();

            var helpDevTasksConfigPath = this.fileSystem.Path.GetTempFileName();
            var helpDevTasksConfigJson = 
$@"{{
    ""answers"": {{
        ""packagePrefix"": ""cmf.docs.area"",
        ""registry"": ""{npmRegistry}"",
        ""confirmSkip"": ""y"",
        ""channel"": ""release-{mesVersion?.Replace(".", "")}""
    }}
}}";
            this.fileSystem.File.WriteAllText(helpDevTasksConfigPath, helpDevTasksConfigJson);
            var helpWebAppConfigPath = this.fileSystem.Path.GetTempFileName();
            var helpWebAppConfigJson = 
@"{
    ""answers"": {
        ""appName"": ""web"",
        ""basePackage"": ""other"",
        ""otherPackage"": ""cmf.docs.web""
    }
}";
            this.fileSystem.File.WriteAllText(helpWebAppConfigPath, helpWebAppConfigJson);
            
            // create web app
            // npx yeoman-gen-run --name @criticalmanufacturing/html --config "$pathHTMLConfig"
            Log.Verbose("Generate web app, this will take a while...");
            (new NPXCommand()
            {
                Command = "yeoman-gen-run",
                WorkingDirectory = pkgFolder,
                Args = new []
                {
                    "--name", "@criticalmanufacturing/html", "--config", helpDevTasksConfigPath
                }
            }).Exec();
            // npx yeoman-gen-run --name @criticalmanufacturing/html:application cmf.docs.area.web --config "$path"
            (new NPXCommand()
            {
                Command = "yeoman-gen-run",
                WorkingDirectory = pkgFolder,
                Args = new []
                {
                    "--name", "@criticalmanufacturing/html:application", "cmf.docs.area.web", "--config", helpWebAppConfigPath
                }
            }).Exec();
            Log.Verbose("Web app generated!");
            
            
            Log.Debug("Obtaining sources from MES Documentation package...");
            var docPkgConfigJsonStr = FileSystemUtilities.GetFileContentFromPackage(documentationPackage.FullName, "config.json");
            // replace tokens that would break Json parse
            docPkgConfigJsonStr = Regex.Replace(docPkgConfigJsonStr, @"\$\([^\)]+\)", "0", RegexOptions.Multiline);
            dynamic docPkgConfigJson = JsonConvert.DeserializeObject(docPkgConfigJsonStr);
            var docPkgIndexHtml = FileSystemUtilities.GetFileContentFromPackage(documentationPackage.FullName, "index.html");
            
            // config.json
            var configJsonPath = this.fileSystem.Path.Join(pkgFolder.FullName, "apps", 
                this.fileSystem.Path.Join("cmf.docs.area.web", "config.json"));
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
            configJsonJson.host.tenant.name = projectConfig.RootElement.GetProperty("Tenant").GetString();
            configJsonJson.general.defaultDomain = projectConfig.RootElement.GetProperty("DefaultDomain").GetString();
            configJsonJson.version = $"{projectName} $(Build.BuildNumber) - {mesVersion}";
            configJsonJson.packages.available = docPkgConfigJson.packages.available;
            configJsonJson.packages.Remove("bundlePath");
            if (configJsonJson.packages.bundles != null)
            {
                configJsonJson.packages.bundles.metadata = false;
                configJsonJson.packages.bundles.i18n = false;
            }
            configJsonStr = JsonConvert.SerializeObject(configJsonJson, Formatting.Indented);
            this.fileSystem.File.WriteAllText(configJsonPath, configJsonStr);
            Log.Verbose("Updated config.json");
            
            //index.html (copied from Documentation package)
            var indexHtmlPath = this.fileSystem.Path.Join(pkgFolder.FullName, "apps", 
                this.fileSystem.Path.Join("cmf.docs.area.web", "index.html"));
            this.fileSystem.File.WriteAllText(indexHtmlPath, docPkgIndexHtml.Replace("<base href=\"/Help/\">", "<base href=\"/\">"));

            // generate doc package
            Log.Verbose("Generating documentation package. This will take a while...");
            var tenant = projectConfig.RootElement.GetProperty("Tenant").GetString();
            var assetsPkgName = $"cmf.docs.area.{tenant?.ToLowerInvariant()}";
            var helpPkgConfigPath = this.fileSystem.Path.GetTempFileName();
            var helpPkgConfigJson = 
$@"{{
    ""answers"": {{
        ""packageName"": ""{assetsPkgName}"",
        ""dependencies"": ""{assetsPkgName}""
    }}
}}";
            this.fileSystem.File.WriteAllText(helpPkgConfigPath, helpPkgConfigJson);
            (new NPXCommand()
            {
                Command = "yeoman-gen-run",
                WorkingDirectory = pkgFolder,
                Args = new []
                {
                    // the package name must be in the same argument as the generator name: this is a yeoman-gen-run limitation:
                    // when yeoman-gen-run runs env.run(genName, doneFunc), genName gets split into [name, ...args] which causes args to be empty
                    // the actual invocation should be env.run([genName, ...config.cli.args], doneFunc) 
                    "--name", $@"""@criticalmanufacturing/html:package {tenant?.ToLowerInvariant()}""", "--config", helpPkgConfigPath
                }
            }).Exec();
            Log.Verbose("Generated documentation package");
            
            Log.Verbose("Generating assets...");
            base.ExecuteTemplate("helpSrcPkg", new []
            {
                "--output", this.fileSystem.Path.Join(pkgFolder.FullName, "src", "packages"),
                "--name", assetsPkgName,
                "--Tenant", tenant,
                "--force"
            });
            
            Log.Verbose("Changing web app port and package type...");
            // replace type of package gulpfile
            var assetsPkgGulpFilePath = this.fileSystem.Path.Join(pkgFolder.FullName, "src", 
                this.fileSystem.Path.Join("packages", assetsPkgName, "gulpfile.js"));
            var assetsPkgGulpFile= fileSystem.File.ReadAllText(assetsPkgGulpFilePath);
            this.fileSystem.File.WriteAllText(assetsPkgGulpFilePath, assetsPkgGulpFile.Replace("type: 'module'", "type: 'documentation'"));
            // replace port of webapp gulpfile
            var webAppGulpFilePath = this.fileSystem.Path.Join(pkgFolder.FullName, 
                this.fileSystem.Path.Join("apps", "cmf.docs.area.web", "gulpfile.js"));
            var webAppGulpFile= fileSystem.File.ReadAllText(webAppGulpFilePath);
            this.fileSystem.File.WriteAllText(webAppGulpFilePath, webAppGulpFile.Replace("defaultPort: 7000", "defaultPort: 7001"));
            Log.Information("Help package generated");
        }
    }
}
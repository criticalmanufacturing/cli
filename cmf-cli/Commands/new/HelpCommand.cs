using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using Cmf.CLI.Builders;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Services;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cmf.CLI.Commands.New
{
    /// <summary>
    /// Generates Help/Documentation package structure
    /// </summary>
    [CmfCommand("help", ParentId = "new", Id = "new_help")]
    public class HelpCommand : UILayerTemplateCommand
    {
        private string schematicsVersion = "";

        private string dfPackageNamePascalCase = "";
        private string assetsPkgName = "";

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
                description: "Path to the MES documentation package (required for MES versions up to 9.x)",
                parseArgument: argResult => Parse<IFileInfo>(argResult)
            )
            { IsRequired = false });
            cmd.Handler = CommandHandler.Create<IDirectoryInfo, string, IFileInfo>(this.Execute);
        }

        /// <inheritdoc />
        protected override List<string> GenerateArgs(IDirectoryInfo projectRoot, IDirectoryInfo workingDir, List<string> args)
        {
            var relativePathToRoot =
                this.fileSystem.Path.Join("..", //always one level deeper
                    this.fileSystem.Path.GetRelativePath(
                        workingDir.FullName,
                        projectRoot.FullName)
                ).Replace("\\", "/");

            args.AddRange(new[]
            {
                "--rootRelativePath", relativePathToRoot,
                "--ngxSchematicsVersion", this.schematicsVersion,
                "--npmRegistry", ExecutionContext.Instance.ProjectConfig.NPMRegistry.OriginalString,
                "--MESVersion", ExecutionContext.Instance.ProjectConfig.MESVersion.ToString(),
                "--nodeVersion", ExecutionContext.ServiceProvider.GetService<IDependencyVersionService>().Node(ExecutionContext.Instance.ProjectConfig.MESVersion),
                "--ngVersion", ExecutionContext.ServiceProvider.GetService<IDependencyVersionService>().Angular(ExecutionContext.Instance.ProjectConfig.MESVersion).CLI,
                "--zoneVersion", ExecutionContext.ServiceProvider.GetService<IDependencyVersionService>().Angular(ExecutionContext.Instance.ProjectConfig.MESVersion).Zone,
                "--tsVersion", ExecutionContext.ServiceProvider.GetService<IDependencyVersionService>().Angular(ExecutionContext.Instance.ProjectConfig.MESVersion).Typescript,
                "--esLintVersion", ExecutionContext.ServiceProvider.GetService<IDependencyVersionService>().Angular(ExecutionContext.Instance.ProjectConfig.MESVersion).ESLint,
                "--tsesVersion", ExecutionContext.ServiceProvider.GetService<IDependencyVersionService>().Angular(ExecutionContext.Instance.ProjectConfig.MESVersion).TSESLint,
                "--name", this.assetsPkgName,
                "--dfPackageNamePascalCase", this.dfPackageNamePascalCase
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
            if (ExecutionContext.Instance.ProjectConfig.MESVersion.Major > 9)
            {
                Log.Debug("Running v>=10 template");
                this.Execute(workingDir, version, ExecutionContext.Instance.ProjectConfig.MESVersion.Major);
            }
            else
            {
                Log.Debug("Running v<10 template");
                this.ExecuteV9(workingDir, version, documentationPackage);
            }
        }

        public void ExecuteV9(IDirectoryInfo workingDir, string version, IFileInfo documentationPackage)
        {
            if (documentationPackage == null)
            {
                throw new CliException("--docPkg option is required for MES versions up to 9.x");
            }
            if (!documentationPackage.Exists)
            {
                throw new CliException($"Cannot find Documentation package {documentationPackage.FullName}");
            }
            base.Execute(workingDir, version); // create package base - generate cmfpackage.json
            var nameIdx = Array.FindIndex(base.executedArgs, item => string.Equals(item, "--name"));
            var pkgName = base.executedArgs[nameIdx + 1];
            var htmlStarterVersion = ExecutionContext.Instance.ProjectConfig.HTMLStarterVersion;
            // clone HTMLStarter content in the folder
            var pkgFolder = workingDir.GetDirectories(pkgName).FirstOrDefault();
            if (!pkgFolder?.Exists ?? false)
            {
                throw new CliException($"Package folder {pkgFolder.Name} does not exist. This is a template error. Please open an issue on GitHub.");
            }
            this.CloneHTMLStarter(htmlStarterVersion, pkgFolder);

            // root package.json
            var rootPkgJsonPath = this.fileSystem.Path.Join(pkgFolder.FullName, "package.json");
            var json = fileSystem.File.ReadAllText(rootPkgJsonPath);
            dynamic rootPkgJson = JsonConvert.DeserializeObject(json);
            if (rootPkgJson == null)
            {
                throw new CliException("Could not load package.json");
            }
            var devTasksVersion = ExecutionContext.Instance.ProjectConfig.DevTasksVersion;
            var yoGeneratorVersion = ExecutionContext.Instance.ProjectConfig.YoGeneratorVersion;
            var projectName = ExecutionContext.Instance.ProjectConfig.ProjectName;
            var repositoryURL = ExecutionContext.Instance.ProjectConfig.RepositoryURL;
            rootPkgJson.devDependencies["@criticalmanufacturing/dev-tasks"] = devTasksVersion.ToString();
            rootPkgJson.devDependencies["@criticalmanufacturing/generator-html"] = yoGeneratorVersion.ToString();
            rootPkgJson.devDependencies["@types/node"] = "^12.0.0";
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
                throw new CliException("Could not load .dev-tasks.json");
            }
            var npmRegistry = ExecutionContext.Instance.ProjectConfig.NPMRegistry;
            var mesVersion = ExecutionContext.Instance.ProjectConfig.MESVersion;
            devTasksJson.packagePrefix = "cmf.docs.area";
            devTasksJson.webAppPrefix = "cmf.docs.area";
            devTasksJson.registry = npmRegistry;
            devTasksJson.channel = $"release-{mesVersion.Major}{mesVersion.Minor}{mesVersion.Build}";
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
        ""channel"": ""release-{mesVersion.Major}{mesVersion.Minor}{mesVersion.Build}""
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
                Args = new[]
                {
                    "--name", "@criticalmanufacturing/html", "--config", helpDevTasksConfigPath
                }
            }).Exec();
            // npx yeoman-gen-run --name @criticalmanufacturing/html:application cmf.docs.area.web --config "$path"
            (new NPXCommand()
            {
                Command = "yeoman-gen-run",
                WorkingDirectory = pkgFolder,
                Args = new[]
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
                throw new CliException("Could not load config.json");
            }
            var restPort = ExecutionContext.Instance.ProjectConfig.RESTPort;
            configJsonJson.host.rest.enableSsl = false;
            configJsonJson.host.rest.address = "localhost";
            configJsonJson.host.rest.port = restPort;
            configJsonJson.host.isLoadBalancerEnabled = false;
            configJsonJson.host.tenant.name = ExecutionContext.Instance.ProjectConfig.Tenant;
            configJsonJson.general.defaultDomain = ExecutionContext.Instance.ProjectConfig.DefaultDomain;
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
            var tenant = ExecutionContext.Instance.ProjectConfig.Tenant;
            var assetsPkgName = $"cmf.docs.area.{pkgName.ToLowerInvariant()}";
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
                Args = new[]
                {
                    // the package name must be in the same argument as the generator name: this is a yeoman-gen-run limitation:
                    // when yeoman-gen-run runs env.run(genName, doneFunc), genName gets split into [name, ...args] which causes args to be empty
                    // the actual invocation should be env.run([genName, ...config.cli.args], doneFunc) 
                    "--name", $@"""@criticalmanufacturing/html:package {assetsPkgName}""", "--config", helpPkgConfigPath
                }
            }).Exec();
            Log.Verbose("Generated documentation package");

            Log.Verbose("Generating assets...");
            base.ExecuteTemplate("helpSrcPkg", new[]
            {
                "--output", this.fileSystem.Path.Join(pkgFolder.FullName, "src", "packages"),
                "--name", assetsPkgName,
                "--dfPackageName", pkgName.ToLowerInvariant(),
                "--Tenant", tenant
            });

            Log.Verbose("Changing web app port and package type...");
            // replace type of package gulpfile
            var assetsPkgGulpFilePath = this.fileSystem.Path.Join(pkgFolder.FullName, "src",
                this.fileSystem.Path.Join("packages", assetsPkgName, "gulpfile.js"));
            var assetsPkgGulpFile = fileSystem.File.ReadAllText(assetsPkgGulpFilePath);
            this.fileSystem.File.WriteAllText(assetsPkgGulpFilePath, assetsPkgGulpFile.Replace("type: 'module'", "type: 'documentation'"));
            // replace port of webapp gulpfile
            var webAppGulpFilePath = this.fileSystem.Path.Join(pkgFolder.FullName,
                this.fileSystem.Path.Join("apps", "cmf.docs.area.web", "gulpfile.js"));
            var webAppGulpFile = fileSystem.File.ReadAllText(webAppGulpFilePath);
            this.fileSystem.File.WriteAllText(webAppGulpFilePath, webAppGulpFile.Replace("defaultPort: 7000", "defaultPort: 7001"));
            Log.Information("Help package generated");
        }

        public void Execute(IDirectoryInfo workingDir, string version, int majorVersion)
        {
            var ngxSchematicsVersion = ExecutionContext.Instance.ProjectConfig.NGXSchematicsVersion;
            if (ngxSchematicsVersion == null)
            {
                throw new CliException("Seems like the repository scaffolding was run on a previous version of MES. Please re-init for versions 10+.");
            }

            var mesVersion = ExecutionContext.Instance.ProjectConfig.MESVersion;

            this.schematicsVersion = ngxSchematicsVersion.ToString() ?? $"@release-{mesVersion.Major}{mesVersion.Minor}{mesVersion.Build}";
            //Switch between v10 and v11 template 
            switch (majorVersion)
            {
                case 10: 
                    this.CommandName = "help10";
                    break;
                case 11:
                    this.CommandName = "help11";
                    break;
            }
            

            var ngCliVersion = ExecutionContext.ServiceProvider.GetService<IDependencyVersionService>().AngularCLI(ExecutionContext.Instance.ProjectConfig.MESVersion);
            var nameIdx = Array.FindIndex(base.executedArgs, item => string.Equals(item, "--name"));
            var packageName = base.executedArgs[nameIdx + 1];
            var projectName = packageName.Replace(".", "-").ToLowerInvariant();
            this.assetsPkgName = $"cmf-docs-area-{projectName.ToLowerInvariant()}";
            this.dfPackageNamePascalCase = string.Join("", projectName.Split("-").Select(seg => Regex.Replace(seg, @"\b(\w)", m => m.Value.ToUpper())));
            
            base.Execute(workingDir, version); // create package base and web application
            // this won't return null because it has to success on the base.Execute call

            // ng generate library <docPackage>
            var pkgFolder = workingDir.GetDirectories(packageName).FirstOrDefault();
            if (!pkgFolder?.Exists ?? false)
            {
                throw new CliException($"Package folder {pkgFolder.Name} does not exist. This is a template error. Please open an issue on GitHub.");
            }

            Log.Verbose("Executing npm install, this will take a while...");
            (new NPMCommand() { Command = "install", WorkingDirectory = pkgFolder }).Exec();
            
            new NPXCommand()
            {
                Command = $"@angular/cli@{ngCliVersion}",
                Args = new[] { "generate", "library", assetsPkgName },
                WorkingDirectory = pkgFolder,
                ForceColorOutput = false
            }.Exec();
           
            // generate the assets structure in projects/<docPackage>/assets
            var tenant = ExecutionContext.Instance.ProjectConfig.Tenant;
            Log.Verbose("Generating assets...");
            base.ExecuteTemplate("helpSrcPkg", new[]
            {
                "--output", this.fileSystem.Path.Join(pkgFolder!.FullName, "projects"),
                "--name", assetsPkgName,
                "--dfPackageName", projectName,
                "--Tenant", tenant,
                "--v10metadata", true.ToString(),
                "--dfPackageNamePascalCase", dfPackageNamePascalCase
            });

            // register assets in angular.json
            var angularJsonFile =
                this.fileSystem.FileInfo.New(this.fileSystem.Path.Join(pkgFolder!.FullName, CliConstants.AngularJson));
            var content = fileSystem.File.ReadAllText(angularJsonFile.FullName);
            dynamic json = JsonConvert.DeserializeObject(content);
            var assets = json!.projects.DocumentationPortal.architect.build.options.assets;
            (assets as JArray)!.Add(new JObject() { { "glob", "**/*" }, { "input", $"projects/{assetsPkgName}/assets" }, { "output", $"assets/{assetsPkgName}" } });
            fileSystem.File.WriteAllText(angularJsonFile.FullName, JsonConvert.SerializeObject(json, Formatting.Indented));
        }
    }
}
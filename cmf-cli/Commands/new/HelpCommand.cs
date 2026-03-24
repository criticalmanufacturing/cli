using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
            var (workingDirArg, versionOpt) = base.GetBaseCommandConfig(cmd);

            cmd.SetAction((parseResult, cancellationToken) =>
            {
                var workingDir = parseResult.GetValue(workingDirArg);
                var version = parseResult.GetValue(versionOpt);

                Execute(workingDir, version);
                return Task.FromResult(0);
            });
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

            var requireModuleSES =
                ExecutionContext.Instance.ProjectConfig.MESVersion >= new Version(11, 0, 0) &&
                ExecutionContext.Instance.ProjectConfig.MESVersion < new Version(11, 2, 0);

            var angularDeps = ExecutionContext.ServiceProvider.GetService<IDependencyVersionService>().Angular(ExecutionContext.Instance.ProjectConfig.MESVersion);

            args.AddRange(new[]
            {
                "--rootRelativePath", relativePathToRoot,
                "--ngxSchematicsVersion", this.schematicsVersion,
                "--npmRegistry", ExecutionContext.Instance.ProjectConfig.NPMRegistry.OriginalString,
                "--MESVersion", ExecutionContext.Instance.ProjectConfig.MESVersion.ToString(),
                "--nodeVersion", ExecutionContext.ServiceProvider.GetService<IDependencyVersionService>().Node(ExecutionContext.Instance.ProjectConfig.MESVersion),
                "--ngVersion", angularDeps.CLI.Major.ToString(),
                "--zoneVersion", angularDeps.Zone,
                "--tsVersion", angularDeps.Typescript,
                "--esLintVersion", angularDeps.ESLint,
                "--tsesVersion", angularDeps.TSESLint,
                "--assetsPkgName", this.assetsPkgName,
                "--requireModuleSES", requireModuleSES.ToString(),
                "--dfPackageNamePascalCase", this.dfPackageNamePascalCase
            });

            return args;
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="workingDir">nearest root package</param>
        /// <param name="version">package version</param>
        public void Execute(IDirectoryInfo workingDir, string version)
        {
            int majorVersion = ExecutionContext.Instance.ProjectConfig.MESVersion.Major;
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
            var packageName = base.GeneratePackageName(workingDir)!.Value.Item1;
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
                "--dfPackageNamePascalCase", string.Join("", projectName.Split("-").Select(seg => Regex.Replace(seg, @"\b(\w)", m => m.Value.ToUpper())))
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
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cmf.CLI.Builders;
using Cmf.CLI.Commands.html;
using Cmf.CLI.Commands.restore;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cmf.CLI.Handlers
{
    /// <summary>
    /// Handler for packages managed with @angular/cli
    /// </summary>
    /// <seealso cref="PackageTypeHandler" />
    public class HtmlNgCliPackageTypeHandler : PackageTypeHandler
    {
        protected AngularWorkspace Workspace;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlNgCliPackageTypeHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage"></param>
        public HtmlNgCliPackageTypeHandler(CmfPackage cmfPackage) : base(cmfPackage)
        {
            cmfPackage.SetDefaultValues
            (
                targetDirectory:
                    "UI/Html",
                targetLayer:
                    "ui",
                steps:
                    new List<Step>
                    {
                        new Step(StepType.DeployFiles)
                        {
                            ContentPath = "**" // TODO: remove assets/config.json from the deployed files, should only apply transform
                        },
                        new Step(StepType.TaggedFile)
                        {
                            ContentPath = "index.html",
                            TagFile = true
                        },
                        new Step(StepType.TaggedFile)
                        {
                            ContentPath = "ngsw.json",
                            TagFile = true
                        },
                        new Step(StepType.TaggedFile)
                        {
                            ContentPath = "assets/config.json",
                            TagFile = true
                        }
                    }
            );

            var projectRoot = FileSystemUtilities.GetProjectRoot(this.fileSystem);
            var tsLBOsPath = this.fileSystem.Path.Join(projectRoot.FullName, "Libs", "LBOs", "TypeScript");
            var tsLBOsDir = this.fileSystem.DirectoryInfo.New(tsLBOsPath);

            BuildSteps = new IBuildCommand[]
            {
                new ExecuteCommand<RestoreCommand>()
                {
                    Command = new RestoreCommand(),
                    DisplayName = "cmf restore",
                    Execute = command =>
                    {
                        command.Execute(cmfPackage.GetFileInfo().Directory, null);
                    }
                },
                new NPMCommand()
                {
                    DisplayName = "NPM Install",
                    Command  = "install",
                    Args = new []{ "--force" },
                    WorkingDirectory = cmfPackage.GetFileInfo().Directory
                },
                new NPMCommand()
                {
                    DisplayName = "NPM Install LBOs",
                    Command  = "install",
                    Args = new []{ "--force" },
                    WorkingDirectory = tsLBOsDir
                },
                new NPMCommand()
                {
                    DisplayName = "Build LBOs",
                    Command  = "run",
                    Args = new []{ "build" },
                    WorkingDirectory = tsLBOsDir
                },
                new ExecuteCommand<LinkLBOsCommand>()
                {
                    DisplayName = "Link LBOs",
                    Command = new LinkLBOsCommand(),
                    Execute = command => command.Execute(cmfPackage.GetFileInfo().Directory)
                }
            };
            cmfPackage.DFPackageType = PackageType.Presentation;

            // Projects BuildSteps
            Workspace = new AngularWorkspace(cmfPackage.GetFileInfo().Directory);
            var apps = Workspace.Projects.Where(lib => lib.Type == "application").Select(p => (p.PackageJson.Content.name as Newtonsoft.Json.Linq.JValue)?.Value as string ?? p.Name).ToList();
            foreach (var package in Workspace.PackagesToBuild)
            {
                BuildSteps = BuildSteps.Concat(new IBuildCommand[]
                {
                    new NgCommand()
                    {
                        DisplayName = $"ng build {package.Key}",
                        Command = "build",
                        Args = apps.Contains(package.Key) ? new []{ "--base-href", "$(APPLICATION_BASE_HREF)" } : Array.Empty<string>(),
                        WorkingDirectory = package.Value
                    }
                }).ToArray();
            }

            BuildSteps = BuildSteps.Append(new NgBuildFileTokenReplacerCommand(this.fileSystem, apps, cmfPackage)).ToArray();
        }

        /// <summary>
        /// Bumps package.json and package-lock.json
        /// of each project to the specified version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="buildNr">The version for build Nr.</param>
        /// <param name="bumpInformation">The bump information.</param>
        public override void Bump(string version, string buildNr, Dictionary<string, object> bumpInformation = null)
        {
            base.Bump(version, buildNr, bumpInformation);

            foreach (var project in Workspace.Projects)
            {
                if (project.PackageJson.Content.version == null)
                {
                    throw new CliException(string.Format(CoreMessages.MissingMandatoryPropertyInFile, "version", project.PackageJson.File.FullName));
                }
                project.PackageJson.Content.version = GenericUtilities.RetrieveNewPresentationVersion(project.PackageJson.Content.version.ToString(), version, buildNr);

                // write package.json
                fileSystem.File.WriteAllText(project.PackageJson.File.FullName, JsonConvert.SerializeObject(project.PackageJson.Content, Formatting.Indented));

                // In some cases we don't have the package-lock
                if (project.PackageLock != null)
                {
                    if (project.PackageLock.Content.version == null)
                    {
                        throw new CliException(string.Format(CoreMessages.MissingMandatoryPropertyInFile, "version", project.PackageLock.File.FullName));
                    }

                    project.PackageLock.Content.version = project.PackageJson.Content.version;

                    // if package-lock also refer the package in the packages list
                    Dictionary<string, dynamic> packages = project.PackageLock.Content.packages.ToObject<Dictionary<string, dynamic>>();
                    if(packages.ContainsKey("") && packages[""].name == project.Name)
                    {
                        packages[""].version = project.PackageJson.Content.version;
                        project.PackageLock.Content.packages = JObject.FromObject(packages);
                    }


                    // write package-lock.json
                    fileSystem.File.WriteAllText(project.PackageLock.File.FullName, JsonConvert.SerializeObject(project.PackageLock.Content, Formatting.Indented));
                }

            }
        }

        // TODO: enable this when we can transform config.json
        // public override void Pack(IDirectoryInfo packageOutputDir, IDirectoryInfo outputDir)
        // {
        //     var filesToPack = GetContentToPack(packageOutputDir);
        //     if (CmfPackage.ContentToPack.HasAny() && !filesToPack.HasAny())
        //     {
        //         throw new CliException(string.Format(CoreMessages.ContentToPackNotFound, CmfPackage.PackageId, CmfPackage.Version));
        //     }
        //
        //     if (filesToPack != null)
        //     {
        //         FilesToPack.AddRange(filesToPack);
        //
        //         FilesToPack.ForEach(fileToPack =>
        //         {
        //             Log.Debug($"Packing '{fileToPack.Source.FullName} to {fileToPack.Target.FullName} by contentToPack rule (Action: {fileToPack.ContentToPack.Action.ToString()}, Source: {fileToPack.ContentToPack.Source}, Target: {fileToPack.ContentToPack.Target})");
        //             IDirectoryInfo _targetFolder = this.fileSystem.DirectoryInfo.New(fileToPack.Target.Directory.FullName);
        //             if (!_targetFolder.Exists)
        //             {
        //                 _targetFolder.Create();
        //             }
        //         });
        //     }
        //
        //     // TODO: replace this with an ignore entry in the ContentToPack of the template cmfpackage.json
        //     FilesToPack = FilesToPack.Where(ftp => !ftp.Source.FullName.EndsWith($"assets{this.fileSystem.Path.DirectorySeparatorChar}config.json")).ToList();
        //     GeneratePresentationConfigFile(packageOutputDir);
        //
        //     GenerateDeploymentFrameworkManifest(packageOutputDir);
        //
        //     FinalArchive(packageOutputDir, outputDir);
        //
        //     Log.Information($"{outputDir.FullName}/{CmfPackage.ZipPackageName} created");
        //     
        // }
        
        /// <summary>
        /// Generates the presentation configuration file.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        private void GeneratePresentationConfigFile(IDirectoryInfo packageOutputDir)
        {
            Log.Debug("Generating Presentation config.json");
            string path = $"{packageOutputDir.FullName}/assets/{CliConstants.CmfPackagePresentationConfig}";
            
            List<string> transformInjections = new();

            IDirectoryInfo cmfPackageDirectory = CmfPackage.GetFileInfo().Directory;

            foreach (ContentToPack contentToPack in CmfPackage.ContentToPack)
            {
                if (contentToPack.Action == PackAction.Transform)
                {
                    transformInjections.Add(contentToPack.Source);
                }
            }

            // Get Template
            string fileContent = ResourceUtilities.GetEmbeddedResourceContent($"{CliConstants.FolderTemplates}/{CmfPackage.PackageType}/config.ng.json");

            fileContent = fileContent.Replace(CliConstants.TokenVersion, CmfPackage.Version);

            string injection = string.Empty;
            if (transformInjections.HasAny())
            {
                // we actually want a trailing comma here, because the inject token is in the middle of the document. If this changes we need to put more logic here.
                var injections = transformInjections.Select(injection => this.fileSystem.File.ReadAllText($"{cmfPackageDirectory}/{injection}") + ",");
                injection = string.Join(System.Environment.NewLine, injections);
            }
            fileContent = fileContent.Replace(CliConstants.TokenJDTInjection, injection);
            fileContent = fileContent.Replace(CliConstants.CacheId, DateTime.Now.ToString("yyyyMMddHHmmss"));

            this.fileSystem.File.WriteAllText(path, fileContent);
        }
    }
}
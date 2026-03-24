using Cmf.CLI.Builders;
using Cmf.CLI.Commands.New;
using Cmf.CLI.Commands.restore;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cmf.CLI.Handlers
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="PresentationPackageTypeHandler" />
    public class IoTPackageTypeHandler : PackageTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IoTPackageTypeHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        public IoTPackageTypeHandler(CmfPackage cmfPackage) : base(cmfPackage)
        {
            var targetVersion = ExecutionContext.Instance.ProjectConfig.MESVersion;
            IBuildCommand[] buildCommands = Array.Empty<IBuildCommand>();
            var defaultSteps = new List<Step>();

            var packageLocation = "projects";

            if (!this.IsAngularProject(cmfPackage.GetFileInfo().Directory.FullName))
            {
                defaultSteps = this.AddAutomationTaskLibrariesStep(targetVersion, CmfPackage, defaultSteps);
                defaultSteps = this.AddAutomationBusinessScenarioStep(targetVersion, CmfPackage, defaultSteps);
            }

            buildCommands = new IBuildCommand[]
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
                    Command = "install",
                    Args = new[] {"--force"},
                    WorkingDirectory = cmfPackage.GetFileInfo().Directory
                },
                new NPMCommand()
                {
                    DisplayName = "NPM Lint",
                    Command = "run lint",
                    WorkingDirectory = cmfPackage.GetFileInfo().Directory,
                    ConditionForExecute = () =>
                    {
                        var packageJsons = this.GetPackageJsons(cmfPackage, packageLocation);

                        if(packageJsons == null && !packageJsons.Any())
                        {
                            return false;
                        }
                        foreach (var packageJson in packageJsons )
                        {
                            var json = fileSystem.File.ReadAllText(packageJson.FullName);
                            dynamic packageJsonContent = JsonConvert.DeserializeObject(json);

                            if(packageJsonContent?["scripts"] == null || packageJsonContent?["scripts"]?["lint"] == null)
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                },
                new NPMCommand()
                {
                    DisplayName = "NPM Build",
                    Command = "run build -ws",
                    Args = new[] {"--force"},
                    WorkingDirectory = cmfPackage.GetFileInfo().Directory
                },
                new NPMCommand()
                {
                    DisplayName = "NPM Test",
                    Command = "run test -ws",
                    Args = new[] {"--force"},
                    WorkingDirectory = cmfPackage.GetFileInfo().Directory
                },
                new ExecuteCommand<IoTLibCommand>()
                {
                    Command = new IoTLibCommand(),
                    DisplayName = "cmf iot lib command",
                    ConditionForExecute = () =>
                    {
                        if(cmfPackage.GetFileInfo().Directory.EnumerateDirectories().Any(dir => dir.Name == "dist"))
                        {
                            return true;
                        }
                        return false;
                    },
                    Execute = command =>
                    {
                        command.Execute(cmfPackage.GetFileInfo().Directory);
                    }
                },
            };

            // Add Steps
            defaultSteps.AddRange(new List<Step>()
            {
                new Step(StepType.DeployFiles)
                {
                    ContentPath = "runtimePackages/**"
                },
                new Step(StepType.DeployFiles)
                {
                    ContentPath = "*.ps1"
                },
                new Step(StepType.DeployFiles)
                {
                    ContentPath = "*.bat"
                },
                new Step(StepType.Generic)
                {
                    OnExecute = $"$(Package[{cmfPackage.PackageId}].TargetDirectory)/runtimePackages/ValidateIoTInstall.ps1"
                },
                new Step(StepType.Generic)
                {
                    OnExecute = $"$(Package[{cmfPackage.PackageId}].TargetDirectory)/runtimePackages/PublishToRepository.ps1"
                },
                new Step(StepType.Generic)
                {
                    OnExecute = $"$(Package[{cmfPackage.PackageId}].TargetDirectory)/runtimePackages/PublishToDirectory.ps1"
                },
                new Step(StepType.DeployRepositoryFiles)
                {
                    ContentPath = "runtimePackages/**"
                },
                new Step(StepType.GenerateRepositoryIndex)
            });

            // Validate Steps
            defaultSteps = defaultSteps.Where(step =>
                step.Type != StepType.DeployRepositoryFiles && step.Type != StepType.GenerateRepositoryIndex
            ).ToList();

            cmfPackage.SetDefaultValues
            (
                targetDirectory:
                    "UI/Html",
                targetLayer:
                    "ui",
                steps: defaultSteps
            );

            DefaultContentToIgnore.AddRange(new List<string>()
            {
                "gulpfile.js",
                "package-lock.json",
                "package.json",
                "packConfig.json",
                "README.md"
            });

            BuildSteps = buildCommands;

            cmfPackage.DFPackageType = PackageType.Presentation;
        }

        /// <summary>
        /// Bumps the specified CMF package.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="buildNr">The version for build Nr.</param>
        /// <param name="bumpInformation">The bump information.</param>
        /// <exception cref="CliException"></exception>
        public override void Bump(string version, string buildNr, Dictionary<string, object> bumpInformation = null)
        {
            base.Bump(version, buildNr, bumpInformation);

            string parentDirectory = CmfPackage.GetFileInfo().DirectoryName;
            string[] filesToUpdate = this.fileSystem.Directory.GetFiles(parentDirectory, "package.json", SearchOption.AllDirectories);
            foreach (var fileName in filesToUpdate)
            {
                if (fileName.Contains("node_modules"))
                {
                    continue;
                }
                string json = this.fileSystem.File.ReadAllText(fileName);
                dynamic jsonObj = JsonConvert.DeserializeObject(json);

                if (jsonObj["version"] == null)
                {
                    throw new CliException(string.Format(CoreMessages.MissingMandatoryPropertyInFile, "version", fileName));
                }

                jsonObj["version"] = GenericUtilities.RetrieveNewPresentationVersion(jsonObj["version"].ToString(), version, buildNr);

                this.fileSystem.File.WriteAllText(fileName, JsonConvert.SerializeObject(jsonObj, Formatting.Indented));
            }

            filesToUpdate = this.fileSystem.Directory.GetFiles(parentDirectory, "*metadata.ts", SearchOption.AllDirectories);
            foreach (var fileName in filesToUpdate)
            {
                if (fileName.Contains("node_modules")
                    || fileName.Contains("\\src\\style")) // prevent metadata.ts in the \src\style from being taken into account
                {
                    continue;
                }
                string metadataFile = this.fileSystem.File.ReadAllText(fileName);

                // take in consideration double quotes and single quotes
                string[] quotes = { "\"", "'" };
                string regex = @$"version: ({quotes[0]}|{quotes[1]})[0-9.-]*({quotes[0]}|{quotes[1]})";

                var regexMatch = Regex.Match(metadataFile, regex, RegexOptions.Singleline)?.Value?.Split(quotes, StringSplitOptions.TrimEntries);
                if (regexMatch?.Length <= 1)
                {
                    continue; // in case that version is not found on metadata.ts skip this
                }

                var metadataVersion = GenericUtilities.RetrieveNewPresentationVersion(regexMatch[1], version, buildNr);
                metadataFile = Regex.Replace(metadataFile, regex, string.Format("version: \"{0}\"", metadataVersion));
                this.fileSystem.File.WriteAllText(fileName, metadataFile);
            }
        }

        /// <summary>
        /// Bumps the Base version of the package
        /// </summary>
        /// <param name="version">The new Base version.</param>
        public override void UpgradeBase(string version, string iotVersion, List<string> iotPackagesToIgnore)
        {
            base.UpgradeBase(version, iotVersion, iotPackagesToIgnore);
            UpgradeBaseUtilities.UpdateNPMProject(this.fileSystem, this.CmfPackage, version);
        }

        /// <summary>
        /// Packs the specified package output dir.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        /// <param name="outputDir">The output dir.</param>
        /// <param name="dryRun">if set to <c>true</c> list the package structure without creating files.</param>
        public override void Pack(IDirectoryInfo packageOutputDir, IDirectoryInfo outputDir, bool dryRun = false)
        {
            foreach (ContentToPack contentToPack in CmfPackage.ContentToPack)
            {
                if (contentToPack.Action == null || contentToPack.Action == PackAction.Pack)
                {
                    IDirectoryInfo[] packDirectories = CmfPackage.GetFileInfo().Directory.GetDirectories(contentToPack.Source);

                    foreach (IDirectoryInfo packDirectory in packDirectories)
                    {
                        string inputDirPath = packDirectory.FullName;
                        IFileInfo packConfig = this.fileSystem.FileInfo.New($"{inputDirPath}/packConfig.json");
                        if (!packConfig.Exists)
                        {
                            Log.Warning("packConfig.json doesn't exist! packagePacker will not run.");
                            continue;
                        }
                        Log.Debug("Running Package Packer");

                        string outputDirPath = $"{packageOutputDir}/runtimePackages";

                        NPMCommand npmCommand = new NPMCommand()
                        {
                            DisplayName = "npm shrinkwrap",
                            Args = new string[] { "shrinkwrap" },
                            WorkingDirectory = packDirectory
                        };

                        npmCommand.Exec();
                        
                        if (ExecutionContext.Instance.ProjectConfig.MESVersion.Major < 11)
                        {
                            IFileInfo yo = this.fileSystem.FileInfo.New($"{AppContext.BaseDirectory}resources/vendors/yo/node_modules/.bin/yo");
                            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                            {
                                Log.Debug("Setting execute permissions on yo binary");
                                yo.UnixFileMode = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute | UnixFileMode.GroupRead | UnixFileMode.GroupExecute | UnixFileMode.OtherRead | UnixFileMode.OtherExecute;
                                fileSystem.File.SetUnixFileMode(yo.FullName, yo.UnixFileMode);
                            }
                            
                            CmdCommand cmdCommand = new CmdCommand()
                            {
                                DisplayName = "yo @criticalmanufacturing/iot:packagePacker",
                                Args = new string[] { "\"", $"{yo} @criticalmanufacturing/iot:packagePacker", $"-i \"{inputDirPath}\"", $"-o \"{outputDirPath}\"", "\"" },
                                WorkingDirectory = packDirectory
                            };

                            cmdCommand.Exec();
                        }
                        else
                        {
                            NPXCommand cmdCommand = new NPXCommand()
                            {
                                DisplayName = "npx @criticalmanufacturing/node-package-bundler",
                                Args = new string[] { "@criticalmanufacturing/node-package-bundler", $"-i \"{inputDirPath}\"", $"-o \"{outputDirPath}\"" },
                                WorkingDirectory = packDirectory
                            };

                            cmdCommand.Exec();
                        }
                    }
                }
                else if (contentToPack.Action == PackAction.Untar)
                {
                    IFileInfo tgzFile = this.fileSystem.FileInfo.New($"{CmfPackage.GetFileInfo().Directory.FullName}/{contentToPack.Source}");
                    CmdCommand cmdCommand = new CmdCommand()
                    {
                        DisplayName = "tar -xzvf",
                        Command = "tar",
                        Args = new string[] { $"-xzvf {tgzFile.FullName}" },
                        WorkingDirectory = packageOutputDir
                    };

                    cmdCommand.Exec();

                    dynamic packageJson = tgzFile.Directory.GetFile(CoreConstants.PackageJson);

                    string packDirectoryName = packageJson == null ? tgzFile.Directory.Name : packageJson.name;

                    IDirectoryInfo packageDirectory = this.fileSystem.DirectoryInfo.New($"{packageOutputDir}/package");
                    IDirectoryInfo destinationDirectory = this.fileSystem.DirectoryInfo.New($"{packageOutputDir}/{contentToPack.Target}/{packDirectoryName}");
                    destinationDirectory.Parent.Create();
                    packageDirectory.MoveTo(destinationDirectory.FullName);
                }
            }

            GeneratePresentationConfigFile(packageOutputDir);
            base.Pack(packageOutputDir, outputDir, dryRun);
        }

        private List<string> GetPackagesWithBusinessScenarios(List<IFileInfo> packageJsons)
        {
            List<string> packagesWVersion = new List<string>();
            foreach (var packageJson in packageJsons)
            {
                var json = fileSystem.File.ReadAllText(packageJson.FullName);
                dynamic packageJsonContent = JsonConvert.DeserializeObject(json);

                if (packageJsonContent?["criticalManufacturing"] == null || packageJsonContent?["criticalManufacturing"]?["businessScenarios"] == null)
                {
                    Log.Debug($"Package has no business scenario '{packageJson.FullName}' as no businessScenarios array was not found in the pre cmf pack package.json");
                }
                else
                {
                    if (packageJsonContent?["name"] == null || packageJsonContent?["version"] == null)
                    {
                        throw new CliException($"Invalid package '{packageJson.FullName}' has an invalid name or version");
                    }

                    string packageName = packageJsonContent["name"].ToString();

                    var package = $"{packageJsonContent["name"].ToString()}@{packageJsonContent["version"].ToString()}";
                    packagesWVersion.Add(package);
                }
            }

            return packagesWVersion;
        }

        private List<string> GetPackagesWithTaskLibraries(List<IFileInfo> packageJsons)
        {
            List<string> packagesWVersion = new List<string>();
            foreach (var packageJson in packageJsons)
            {
                var json = fileSystem.File.ReadAllText(packageJson.FullName);
                dynamic packageJsonContent = JsonConvert.DeserializeObject(json);

                if (packageJsonContent?["criticalManufacturing"] == null || packageJsonContent?["criticalManufacturing"]?["tasksLibrary"] == null)
                {
                    Log.Debug($"Package has no task library '{packageJson.FullName}' as no taskLibrary object was not found in the pre cmf pack package.json");
                }
                else
                {
                    if (packageJsonContent?["name"] == null || packageJsonContent?["version"] == null)
                    {
                        throw new CliException($"Invalid package '{packageJson.FullName}' has an invalid name or version");
                    }

                    string packageName = packageJsonContent["name"].ToString();

                    var package = $"{packageJsonContent["name"].ToString()}@{packageJsonContent["version"].ToString()}";
                    packagesWVersion.Add(package);
                }
            }

            return packagesWVersion;
        }

        private List<string> BuildPackageNames(List<IFileInfo> packageJsons)
        {
            List<string> packagesWVersion = new List<string>();
            foreach (var packageJson in packageJsons)
            {
                var json = fileSystem.File.ReadAllText(packageJson.FullName);
                dynamic packageJsonContent = JsonConvert.DeserializeObject(json);

                if (packageJsonContent?["name"] == null || packageJsonContent?["version"] == null)
                {
                    throw new CliException($"Invalid package '{packageJson.FullName}' has an invalid name or version");
                }

                string packageName = packageJsonContent["name"].ToString();

                var package = $"{packageJsonContent["name"].ToString()}@{packageJsonContent["version"].ToString()}";
                packagesWVersion.Add(package);
            }

            return packagesWVersion;
        }

        private List<IFileInfo> GetPackageJsons(CmfPackage cmfPackage, string projectDir = "projects")
        {
            var dirLoc = this.fileSystem.Path.Join(cmfPackage.GetFileInfo().Directory.FullName, projectDir);
            dirLoc = this.fileSystem.Directory.Exists(dirLoc) ? dirLoc : this.fileSystem.Path.Join(cmfPackage.GetFileInfo().Directory.FullName, "src");
            var directory = this.fileSystem.DirectoryInfo.New(dirLoc);
            var srcCodeDirs = directory.GetDirectories("", SearchOption.TopDirectoryOnly);

            var packageJsons = new List<IFileInfo>();
            foreach (var srcDir in srcCodeDirs)
            {
                var packageJson = srcDir.GetFiles($"package.json", SearchOption.TopDirectoryOnly)?.FirstOrDefault();
                if (packageJson != null)
                {
                    packageJsons.Add(packageJson);
                }
            }

            return packageJsons;
        }

        private bool IsAngularProject(string path)
        {
            return this.fileSystem.File.Exists(
                this.fileSystem.Path.Join(
                    path,
                    "angular.json"));
        }

        private List<Step> AddAutomationTaskLibrariesStep(Version targetVersion, CmfPackage cmfPackage, List<Step> defaultSteps, string packageLocation = "src")
        {
            // Introduced in version 10.2.x
            if ((targetVersion.Major > 10 || (targetVersion.Major == 10 &&
                    targetVersion.Minor >= 2 &&
                    targetVersion.Build >= 7)) && !this.IsAngularProject(cmfPackage.GetFileInfo().Directory.FullName))
            {
                var packages = string.Join(",", this.GetPackagesWithTaskLibraries(this.GetPackageJsons(cmfPackage, packageLocation)));

                if (!string.IsNullOrEmpty(packages))
                {
                    defaultSteps.Add(new Step(StepType.IoTAutomationTaskLibrariesSync)
                    {
                        Content = packages
                    });
                }
            }
            return defaultSteps;
        }

        private List<Step> AddAutomationBusinessScenarioStep(Version targetVersion, CmfPackage cmfPackage, List<Step> defaultSteps, string packageLocation = "src")
        {
            // Introduced in version 11.1.x
            if ((targetVersion.Major > 11 || (targetVersion.Major == 11 && targetVersion.Minor >= 1)))
            {
                var packages = string.Join(",", this.GetPackagesWithBusinessScenarios(this.GetPackageJsons(cmfPackage, packageLocation)));

                if (!string.IsNullOrEmpty(packages))
                {
                    defaultSteps.Add(new Step(StepType.AutomationBusinessScenariosSync)
                    {
                        Content = packages
                    });
                }
            }
            return defaultSteps;
        }

        public void GeneratePresentationConfigFile(IDirectoryInfo packageOutputDir)
        {
            Log.Debug("Generating Presentation config.json");
            string path = $"{packageOutputDir.FullName}/{CliConstants.CmfPackagePresentationConfig}";

            List<string> packageList = new();
            List<string> transformInjections = new();

            IDirectoryInfo cmfPackageDirectory = CmfPackage.GetFileInfo().Directory;

            foreach (ContentToPack contentToPack in CmfPackage.ContentToPack)
            {
                if (contentToPack.Action == null || contentToPack.Action == PackAction.Pack)
                {
                    // TODO: Validate if contentToPack.Source exists before
                    IDirectoryInfo[] packDirectories = cmfPackageDirectory.GetDirectories(contentToPack.Source);

                    foreach (IDirectoryInfo packDirectory in packDirectories)
                    {
                        dynamic packageJson = packDirectory.GetFile(CoreConstants.PackageJson);
                        if (packageJson != null)
                        {
                            string packageName = packageJson.name;

                            // For IoT Packages we should ignore the driver packages
                            if (CmfPackage.PackageType == PackageType.IoT && packageName.Contains(CliConstants.Driver, System.StringComparison.InvariantCultureIgnoreCase))
                            {
                                continue;
                            }

                            packageList.Add($"'{packageName}'");
                        }
                    }
                }
                else if (contentToPack.Action == PackAction.Transform)
                {
                    transformInjections.Add(contentToPack.Source);
                }
            }

            if (packageList.HasAny())
            {
                // Get Template
                string fileContent = ResourceUtilities.GetEmbeddedResourceContent($"{CliConstants.FolderTemplates}/{CmfPackage.PackageType}/{CliConstants.CmfPackagePresentationConfig}");

                string packagesToRemove = string.Empty;
                List<string> packagesToAdd = new();

                for (int i = 0; i < packageList.Count; i++)
                {
                    if (CmfPackage.PackageType == PackageType.IoT)
                    {
                        packagesToRemove += $"@.path=={packageList[i]}";
                    }
                    else
                    {
                        packagesToRemove += $"@=={packageList[i]}";
                    }

                    if (packageList.Count > 1 &&
                        i != packageList.Count - 1)
                    {
                        packagesToRemove += " || ";
                    }

                    string packageToAdd = packageList[i].Replace("'", "\"");
                    if (CmfPackage.PackageType == PackageType.IoT)
                    {
                        packageToAdd = string.Format("{{\"path\": {0} }}", packageToAdd);
                    }

                    packagesToAdd.Add(packageToAdd);
                }

                fileContent = fileContent.Replace(CliConstants.TokenPackagesToRemove, packagesToRemove);
                fileContent = fileContent.Replace(CliConstants.TokenPackagesToAdd, string.Join(",", packagesToAdd));
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
            else
            {
                Log.Debug("Could not find UI packages, so skipping generating config.json transform");
                this.CmfPackage.Steps = this.CmfPackage.Steps
                    .Where(step => step.Type != StepType.TransformFile && step.File != "config.json").ToList();
            }
        }
    }
}
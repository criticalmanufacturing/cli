
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Handlers
{
    /// <summary>
    /// Base handler for UI packages
    /// </summary>
    /// <seealso cref="PackageTypeHandler" />
    public class PresentationPackageTypeHandler : PackageTypeHandler
    {
        #region Private Methods

        /// <summary>
        /// Generates the presentation configuration file.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
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

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="PresentationPackageTypeHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        public PresentationPackageTypeHandler(CmfPackage cmfPackage) : base(cmfPackage)
        {
            if (ExecutionContext.Instance.ProjectConfig.MESVersion.Major < 10)
            {
                cmfPackage.SetDefaultValues
                (
                    steps:
                    new List<Step>
                    {
                        new Step(StepType.DeployFiles)
                        {
                            ContentPath = "node_modules/**"
                        },
                        new Step(StepType.TransformFile)
                        {
                            File = "config.json",
                            TagFile = true
                        }
                    }

                );
            }

            DefaultContentToIgnore.AddRange(new List<string>
            {
                "node_modules",
                "test",
                "*.ts",
                "node.exe",
                "CompileProject.ps1",
                "node_modules_cache.zip"
            });
        }

        /// <summary>
        /// Bumps the specified version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="buildNr">The version for build Nr.</param>
        /// <param name="bumpInformation">The bump information.</param>
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
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                if (jsonObj["version"] == null)
                {
                    throw new CliException(string.Format(CoreMessages.MissingMandatoryPropertyInFile, "version", fileName));
                }

                jsonObj["version"] = GenericUtilities.RetrieveNewPresentationVersion(jsonObj["version"].ToString(), version, buildNr);

                this.fileSystem.File.WriteAllText(fileName, Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented));
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
        /// Packs the specified package output dir.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        /// <param name="outputDir">The output dir.</param>
        /// <param name="dryRun">if set to <c>true</c> list the package structure without creating files.</param>
        public override void Pack(IDirectoryInfo packageOutputDir, IDirectoryInfo outputDir, bool dryRun = false)
        {
            GeneratePresentationConfigFile(packageOutputDir);

            base.Pack(packageOutputDir, outputDir, dryRun);
        }

        #endregion Public Methods
    }
}
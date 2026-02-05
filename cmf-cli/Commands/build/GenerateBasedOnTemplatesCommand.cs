using System;
using System.CommandLine;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Commands
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="PowershellCommand" />
    [CmfCommand(name: "generateBasedOnTemplates", Parent = "help", ParentId = "build_help")]
    public class GenerateBasedOnTemplatesCommand : BaseCommand
    {
        /// <summary>
        /// Configure command
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            var packageRoot = FileSystemUtilities.GetPackageRoot(this.fileSystem);
            var packagePath = ".";
            if (packageRoot != null)
            {
                packagePath = this.fileSystem.Path.GetRelativePath(this.fileSystem.Directory.GetCurrentDirectory(), packageRoot.FullName);
            }

            var packagePathArgument = new Argument<IDirectoryInfo>("packagePath")
            {
                Description = "Package Path"
            };
            packagePathArgument.CustomParser = argResult => Parse<IDirectoryInfo>(argResult, packagePath);
            packagePathArgument.DefaultValueFactory = _ => Parse<IDirectoryInfo>(null, packagePath);
            cmd.Arguments.Add(packagePathArgument);

            cmd.SetAction((parseResult, cancellationToken) =>
            {
                var pkgPath = parseResult.GetValue(packagePathArgument);
                Execute(pkgPath);
                return Task.FromResult(0);
            });
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="packagePath">The path of the current package folder</param>
        public void Execute(IDirectoryInfo packagePath)
        {
            var regex = new Regex("\"?id\"?:\\s+[\"'](.*)[\"']"); // match for menu item IDs
            
            var mesVersion = ExecutionContext.Instance.ProjectConfig.MESVersion;
            Log.Debug($"Generating markdown for a help package for base version {mesVersion}");

            var helpRoot = FileSystemUtilities.GetPackageRootByType(packagePath.FullName, PackageType.Help, this.fileSystem).FullName;
            var project = ExecutionContext.Instance.ProjectConfig.Tenant;
            var helpPackagesRoot = (mesVersion.Major > 9) ? this.fileSystem.Path.Join(helpRoot, "projects") : this.fileSystem.Path.Join(helpRoot, "src", "packages");
            var helpPackages = this.fileSystem.Directory.GetDirectories(helpPackagesRoot);
            var pkgName = CmfPackage.Load(this.fileSystem.FileInfo.New(this.fileSystem.Path.Join(helpRoot, CliConstants.CmfPackageFileName))).PackageId.ToLowerInvariant();
            foreach (var helpPackagePath in helpPackages)
            {
                var helpPackage = this.fileSystem.DirectoryInfo.New(helpPackagePath);
                var metadataFile = helpPackage.GetFiles("src/*.metadata.ts").FirstOrDefault();
                var useLegacyFormat = false;
                if (metadataFile?.Exists ?? false)
                {
                    var metadataContent = metadataFile.ReadToString();
                    var matchedIds = regex.Matches(metadataContent);
                    if (matchedIds.Any(m => m.Captures.Any(id => !id.Value.Contains("."))))
                    {
                        Log.Warning(
                            $"Using legacy menu item IDs! This package will not be deployable with other packages using legacy IDs, as collisions will happen!");
                        useLegacyFormat = true;
                    }
                }

                Generate(helpPackagePath, useLegacyFormat ? project : ( (mesVersion.Major > 9) ? pkgName.Replace(".", "-").ToLowerInvariant() : pkgName));
            }
        }

        private void Generate(string path, string project)
        {
            string templateSuffix = "_template";

            string GetTitle(string content)
            {
                string title = content;
                int indexOfNewLine = title.IndexOf("\n");
                if (indexOfNewLine > 0)
                {
                    title = title.Substring(0, indexOfNewLine);
                }

                title = title.Replace("#", "");
                title = title.Trim(); 
                // Log.Debug($"Got title: '{title}'");
                return title;
            }

            string GetDescription(string content)
            {
                string description = "-";
                string overviewHeader = "## Overview";
                int indexOfOverview = content.IndexOf(overviewHeader);
                if (indexOfOverview > 0)
                {
                    description = content;
                    indexOfOverview += overviewHeader.Length;
                    description = description.Substring(indexOfOverview, description.Length - indexOfOverview);
                    int indexOfNextSection = description.IndexOf("##");
                    if (indexOfNextSection > 0)
                    {
                        description = description.Substring(0, indexOfNextSection);
                    }

                    description = description.Trim();
                }

                return description;
            }

            var newLineRegex = new Regex(@"(\r?\n)( *)");

            // Replaces newlines "\r\n" and "\n" with <br/> and spaces " " with &nbsp;
            string TransformNewLines(string description)
            {
                return newLineRegex.Replace(description, match =>
                {
                    return "<br/>" + string.Join("", Enumerable.Repeat("&nbsp;", match.Groups[2].Value.Length));
                });
            }

            var directory = this.fileSystem.DirectoryInfo.New(path);
            var templateFiles = directory.GetFiles($"*{templateSuffix}", SearchOption.AllDirectories);
            foreach (IFileInfo templateFile in templateFiles)
            {
                if (templateFile.Name == templateSuffix)
                {
                    continue;
                }

                string template = templateFile.Name;
                string name = template.Replace(templateSuffix, "");
                string outputFile = $"{name}.md";
                Log.Debug($"template: {template}");
                Log.Debug($"outputFile: {outputFile}");
                string baseFolder = templateFile.FullName.Replace(templateFile.Name, "");
                Log.Debug($"BaseFolder: {baseFolder}");
                string filesPath = this.fileSystem.Path.GetFullPath(this.fileSystem.Path.Combine(baseFolder, name));
                IFileInfo[] files = this.fileSystem.DirectoryInfo.New(filesPath).GetFiles("*.md").OrderBy(f => f.FullName).ToArray();
                Log.Debug($"Number of files: {files.Length}");
                string output = "";
                string templateContent = this.fileSystem.File.ReadAllText(templateFile.FullName, Encoding.UTF8);
                bool isTableMode = templateContent.Contains("@TableData@");
                bool isIndexMode = templateContent.Contains("@IndexData@");
                string toReplaceToken = isTableMode ? "@TableData@" : "@IndexData@";
                foreach (IFileInfo file in files)
                {
                    string fileContent = this.fileSystem.File.ReadAllText(file.FullName);
                    string fileName = this.fileSystem.Path.GetFileNameWithoutExtension(file.FullName);

                    // Title
                    // Log.Debug($"Getting title for file {file.FullName}");
                    string title = GetTitle(fileContent);
                    string folderPath =
                        file.FullName.Substring(0, file.FullName.LastIndexOf(this.fileSystem.Path.DirectorySeparatorChar));
                    folderPath = folderPath.Substring(folderPath.IndexOf($"assets{this.fileSystem.Path.DirectorySeparatorChar}") +
                                                      ("assets" + this.fileSystem.Path.DirectorySeparatorChar).Length);
                    folderPath = folderPath.Replace(this.fileSystem.Path.DirectorySeparatorChar.ToString(), ">");

                    string link = $"/{project}/{folderPath}>{fileName}";
                    if (isIndexMode)
                    {
                        // Index Mode
                        output += $"* [{title}]({link}){Environment.NewLine}";
                    }
                    else
                    {
                        if (isTableMode)
                        {
                            // Table Mode
                            string description = GetDescription(fileContent);
                            output += $"| [{title}]({link}) | {TransformNewLines(description)} |{Environment.NewLine}";
                        }
                    }
                }

                string newContent = templateContent.Replace(toReplaceToken, output);
                this.fileSystem.File.WriteAllText(this.fileSystem.Path.Combine(baseFolder, outputFile), newContent, Encoding.UTF8);
            }
        }
    }
}
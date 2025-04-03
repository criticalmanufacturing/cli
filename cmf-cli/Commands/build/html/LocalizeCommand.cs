using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using Cmf.CLI.Builders;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;

namespace Cmf.CLI.Commands.html;

/// <summary>
/// Generates messages files in .json format containing UI localized messages
/// </summary>
[CmfCommand("localize", Id = "build_html_localize", ParentId = "build_html")]
public class LocalizeCommand : BaseCommand
{
    public override void Configure(Command cmd)
    {
        var packageRoot = FileSystemUtilities.GetPackageRoot(this.fileSystem);
        var packagePath = ".";
        if (packageRoot != null)
        {
            packagePath = this.fileSystem.Path.GetRelativePath(this.fileSystem.Directory.GetCurrentDirectory(), packageRoot.FullName);
        }
        var arg = new Argument<IDirectoryInfo>(
            name: "packagePath",
            parse: (argResult) => Parse<IDirectoryInfo>(argResult, packagePath),
            isDefault: true)
        {
            Description = "Package Path"
        };

        cmd.AddArgument(arg);
        cmd.Handler = CommandHandler.Create(this.Execute);
    }

    /// <summary>
    /// Executes this instance.
    /// </summary>
    public void Execute(IDirectoryInfo packagePath)
    {
        var cmfPackageFile = this.fileSystem.FileInfo.New(this.fileSystem.Path.Join(packagePath.FullName, CliConstants.CmfPackageFileName));
        if (!cmfPackageFile.Exists)
        {
            throw new CliException($"Cannot find a package at {cmfPackageFile.FullName}");
        }

        var cmfPackage = CmfPackage.Load(cmfPackageFile, true, this.fileSystem);

        var projectRoot = FileSystemUtilities.GetProjectRoot(this.fileSystem);
        var packageDirectory = cmfPackage.GetFileInfo().Directory;
        Debug.Assert(projectRoot != null, "Invalid repository! Run this command inside a project repository.");

        if (cmfPackage.BaseLocalizationFiles?.Any() != true)
        {
            throw new CliException($"Please define the baseLocalizationFiles setting.");
        }

        foreach (string localizationFolder in cmfPackage.BaseLocalizationFiles)
        {
            if (!this.fileSystem.Path.Exists(localizationFolder))
            {
                throw new CliException($"Could not find {localizationFolder}.");
            }
        }

        // TODO: set build target?
        new NgCommand()
        {
            DisplayName = $"ng extract-i18n",
            Command = "extract-i18n",
            Args = new[] { "--format", "json", "--output-path", "./src/assets/i18n" },
            WorkingDirectory = packageDirectory
        }.Exec()?.Wait();

        // merge xlfs
        string tempPath = this.fileSystem.Path.Join("translations", "temp");
        MergeLocalizationFiles(cmfPackage, tempPath);

        // localize
        new NPXCommand()
        {
            DisplayName = $"ui-i18n localize",
            Command = "ui-i18n",
            Args = new[] { "localize", "./src/assets/i18n/messages.json", "--destination", "./src/assets/i18n", "--translations", "./translations/temp" },
            WorkingDirectory = packageDirectory,
            ForceColorOutput = false
        }.Exec()?.Wait();

        this.fileSystem.Directory.Delete(tempPath, true);
    }

    private void MergeLocalizationFiles(CmfPackage cmfPackage, string tempPath)
    {
        if (fileSystem.Path.Exists(tempPath))
        {
            fileSystem.Directory.Delete(tempPath, recursive: true);
        }
        this.fileSystem.Directory.CreateDirectory(tempPath);

        // The files should be named like this: messages.[language].xlf
        var filesPerLanguage = cmfPackage.BaseLocalizationFiles
            .SelectMany(folder => this.fileSystem.Directory.EnumerateFiles(folder))
            .Where(fileName =>
        {
            string[] splitFileName = this.fileSystem.Path.GetFileNameWithoutExtension(fileName).Split(".");
            return splitFileName.Length == 2 && this.fileSystem.Path.GetExtension(fileName).Equals(".xlf", System.StringComparison.InvariantCultureIgnoreCase);
        })
            .GroupBy(fileName =>
        {
            string[] splitFileName = this.fileSystem.Path.GetFileNameWithoutExtension(fileName).Split(".");
            return splitFileName[1];
        });

        foreach (var group in filesPerLanguage)
        {
            string language = group.Key;
            string mainFileName = group.First();

            var mainFile = XDocument.Load(mainFileName);
            var mainTransUnits = mainFile.Descendants(mainFile.Root.Name.Namespace + "trans-unit");

            // merge with other base messages
            foreach (string fileName in group.Skip(1))
            {
                MergeFiles(fileName);
            }

            // merge with project messages
            string projectMessagesPath = this.fileSystem.Path.Join("translations", $"messages.{language}.xlf");
            MergeFiles(projectMessagesPath);

            mainFile.Save(this.fileSystem.Path.Join(tempPath, this.fileSystem.Path.GetFileName(mainFileName)));

            void MergeFiles(string path)
            {
                if (!this.fileSystem.Path.Exists(path))
                {
                    Log.Warning($"Could not find {path}. Make sure the names of your translation files match this pattern.");
                    return;
                }
                var file = XDocument.Load(path);

                // trans-units elements from the project have priority over the base ones
                // meaning the project can overwrite translations
                var unitsToAdd = file.Descendants(file.Root.Name.Namespace + "trans-unit").ExceptBy(mainTransUnits.Select(unit => unit.Attribute("id").Value), unit => unit.Attribute("id").Value);
                mainFile.Descendants(file.Root.Name.Namespace + "body").First().Add(unitsToAdd);
            }
        }
    }
}
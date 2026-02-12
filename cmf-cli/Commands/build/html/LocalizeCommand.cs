using System.CommandLine;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Cmf.CLI.Builders;
using Cmf.CLI.Constants;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using System.Linq;
using System;

namespace Cmf.CLI.Commands.html;

/// <summary>
/// Generates messages files in .json format containing UI localized messages
/// </summary>
[CmfCommand("localize", Id = "build_html_localize", ParentId = "build_html")]
public class LocalizeCommand : BaseCommand
{
    /// <summary>
    /// The minimum MES Version that supports this command
    /// </summary>
    private readonly Version MIN_MES_VERSION = new Version(11, 2, 0);

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
    public void Execute(IDirectoryInfo packagePath)
    {
        if (ExecutionContext.Instance.ProjectConfig.MESVersion < MIN_MES_VERSION)
        {
            throw new CliException(string.Format(CliMessages.InvalidVersionForCommand, MIN_MES_VERSION.ToString()));
        }

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

        new NgCommand()
        {
            DisplayName = $"ng extract-i18n",
            Command = "extract-i18n",
            Args = new[] { "--format", "json", "--output-path", "./src/assets/i18n" },
            WorkingDirectory = packageDirectory
        }.Exec()?.Wait();

        // localize
        new NPXCommand()
        {
            DisplayName = $"ui-i18n localize",
            Command = "ui-i18n",
            Args = new[] { "localize", "./src/assets/i18n/messages.json", "--destination", "./src/assets/i18n", "--translations", string.Join(" ", cmfPackage.BaseLocalizationFiles), "translations" },
            WorkingDirectory = packageDirectory,
            ForceColorOutput = false
        }.Exec()?.Wait();
    }
}
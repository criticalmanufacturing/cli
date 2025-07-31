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
using System;

namespace Cmf.CLI.Commands.html;

/// <summary>
/// Extract HTML i18n messages
/// </summary>
[CmfCommand("extract-i18n", Id = "build_html_extract-i18n", ParentId = "build_html")]
public class ExtractI18nCommand : BaseCommand
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

        new NgCommand()
        {
            DisplayName = $"ng extract-i18n",
            Command = "extract-i18n",
            Args = new[] { "--format", "xlf" },
            WorkingDirectory = packageDirectory
        }.Exec()?.Wait();

        RemoveInheritedMessages(packageDirectory);
    }

    /// <summary>
    /// The extract-i18n command scans the whole bundle, meaning that it generates a file containing messages from the project, but also
    /// from its dependencies. To make it easier to maintain the translations, this command will erase the messages that do not belong to this project.
    /// </summary>
    private void RemoveInheritedMessages(IDirectoryInfo packageDirectory)
    {
        Log.Debug("Removing inherited messages from messages.xlf");

        string messagesFilePath = this.fileSystem.Path.Join(packageDirectory.FullName, "messages.xlf");

        if (!this.fileSystem.Path.Exists(messagesFilePath))
        {
            throw new CliException($"Cannot find the messages.xlf file on {messagesFilePath}");
        }

        var messagesXlf = XDocument.Load(messagesFilePath);
        var transUnits = messagesXlf.Root?.Descendants(messagesXlf.Root.Name.Namespace + "trans-unit");

        // Erase elements that reference files that do not exist in this project
        transUnits.Where(unit =>
        {
            string sourceFile = unit.Descendants(messagesXlf.Root.Name.Namespace + "context").FirstOrDefault(element => element.Attribute("context-type").Value.Equals("sourcefile"))?.Value;

            return !this.fileSystem.Path.Exists(this.fileSystem.Path.Join(packageDirectory.FullName, sourceFile));
        }).Remove();

        messagesXlf.Save(messagesFilePath);
    }
}
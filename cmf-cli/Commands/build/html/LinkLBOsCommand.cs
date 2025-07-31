using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Diagnostics;
using System.IO.Abstractions;
using Cmf.CLI.Builders;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Commands.html;

/// <summary>
/// "link lbos in ui" command
/// </summary>
[CmfCommand("linkLBOs", Id = "build_html_linkLBOs", ParentId = "build_html")]
public class LinkLBOsCommand : BaseCommand
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
        Debug.Assert(projectRoot != null, "Invalid repository! Run this command inside a project repository.");

        var tsLBOsPath = this.fileSystem.Path.Join(projectRoot.FullName, "Libs", "LBOs", "TypeScript");
        Log.Debug($"Checking if {tsLBOsPath} exists...");
        if (fileSystem.Directory.Exists(tsLBOsPath))
        {
            var tsLBOsDir = fileSystem.DirectoryInfo.New(tsLBOsPath);
            new NPMCommand()
            {
                DisplayName = "NPM Install LBOs",
                Command = "install",
                Args = new[] { "--force" },
                WorkingDirectory = tsLBOsDir
            }.Exec()?.Wait();
            new NPMCommand()
            {
                DisplayName = "Build LBOs",
                Command = "run",
                Args = new[] { "build" },
                WorkingDirectory = tsLBOsDir
            }.Exec()?.Wait();

            var lbosNodeModules = this.fileSystem.Path.Join(tsLBOsDir.FullName, "node_modules");
            this.fileSystem.Directory.Delete(lbosNodeModules, true);

            var packageRoot = cmfPackage.GetFileInfo().DirectoryName;
            var linkTargetPath = this.fileSystem.Path.Join(packageRoot, "node_modules", "cmf-lbos");
            var linkTarget = this.fileSystem.DirectoryInfo.New(linkTargetPath);
            if (linkTarget.Exists)
            {
                Log.Debug($"Deleting directory {linkTarget.FullName}");
                linkTarget.Delete(true);

                Log.Debug($"Creating link from {tsLBOsPath} to {linkTarget.FullName}");
                this.fileSystem.Directory.CreateSymbolicLink(linkTarget.FullName, tsLBOsPath);
            }
        }
        else
        {
            // if there are no LBOs we don't need to do anything
            Log.Verbose($"Path '{tsLBOsPath}' does not exist, not doing anything.");
        }
    }
}
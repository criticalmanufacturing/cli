using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Cmf.CLI.Builders;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Commands
{
    /// <summary>
    /// UI Layer Template Abstract Command
    /// provides arguments and execution flow common to all UI layer templates
    /// </summary>
    public abstract class UILayerTemplateCommand : LayerTemplateCommand
    {
        /// <inheritdoc />
        protected UILayerTemplateCommand(string commandName, PackageType packageType) : base(commandName, packageType)
        {
        }

        /// <inheritdoc />
        protected UILayerTemplateCommand(string commandName, PackageType packageType, IFileSystem fileSystem) : base(commandName, packageType, fileSystem)
        {
        }

        /// <summary>
        /// Clones HTML Starter from github, checkout desired target version
        /// </summary>
        /// <param name="versionTag"></param>
        /// <param name="target"></param>
        protected void CloneHTMLStarter(Version versionTag, IDirectoryInfo target)
        {
            if (target?.Exists != true)
            {
                throw new CliException("Target clone directory doesn't exist");
            }

            // delete .gitkeep if it exists
            target.GetFiles(".gitkeep").FirstOrDefault()?.Delete();
            Log.Verbose("cloning html starter");
            // git init
            (new GitCommand() { Command = "init", WorkingDirectory = target }).Exec();
            // git remote add origin https://github.com/criticalmanufacturing/html-starter
            (new GitCommand() { Command = "remote", WorkingDirectory = target, Args = new[] { "add", "origin", "https://github.com/criticalmanufacturing/html-starter" } }).Exec();
            // git fetch
            (new GitCommand() { Command = "fetch", WorkingDirectory = target }).Exec();
            // git pull origin $vars['HTMLStarterVersion']
            (new GitCommand() { Command = "pull", WorkingDirectory = target, Args = new[] { "origin", versionTag.ToString() } }).Exec();
            Log.Debug("delete .git folder");
            this.DeleteFolderWithReadOnlyFiles(target.GetDirectories(".git").FirstOrDefault());
            // delete apps/.gitkeep
            target.GetDirectories("apps").FirstOrDefault()?.GetFiles(".gitkeep").FirstOrDefault()?.Delete();
        }

        private void DeleteFolderWithReadOnlyFiles(IDirectoryInfo folder)
        {
            var tries = 10;
            while (tries > 0)
            {
                try
                {
                    this.fileSystem.Directory.GetFiles(folder?.FullName,
                        "*.*",
                        SearchOption.AllDirectories).ToList().ForEach(file =>
                    {
                        this.fileSystem.File.SetAttributes(file, FileAttributes.Normal); // remove read-only bits
                        this.fileSystem.File.Delete(file);
                    });
                    folder?.Delete(true);
                    break;
                }
                catch (Exception ex)
                {
                    tries--;
                    System.Threading.Thread.Sleep(1000);
                    Log.Debug(ex.Message);
                    Log.Debug($"Retrying (remaining tries: {tries})...");
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Commands 
{
    /// <summary>
    /// This command generates the menu metadata for the documentation packages
    /// This algo is based in an internal PowerShell implementation
    /// </summary>
    /// <seealso cref="PowershellCommand" />
    [CmfCommand(name: "generateMenuItems", Parent = "help", ParentId = "build_help")]
    public class GenerateMenuItemsCommand : BaseCommand
    {
        public GenerateMenuItemsCommand()
        {
        }
        
        public GenerateMenuItemsCommand(IFileSystem fileSystem) : base(fileSystem)
        {
        }
        
        /// <summary>
        /// Configure command
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            cmd.Handler = CommandHandler.Create(this.Execute);
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            var helpRoot = FileSystemUtilities.GetPackageRootByType(Environment.CurrentDirectory, PackageType.Help, this.fileSystem).FullName;
            var project = FileSystemUtilities.ReadProjectConfig(this.fileSystem).RootElement.GetProperty("Tenant").GetString();
            
            var mesVersionStr = FileSystemUtilities.ReadProjectConfig(this.fileSystem).RootElement.GetProperty("MESVersion").GetString();
            Log.Debug($"Generating menu items database for a help package for base version {mesVersionStr}");
            var mesVersion = Version.Parse(mesVersionStr!);

            if (project == null)
            {
                throw new ArgumentException("Can't find project name, is your repository correctly configured?");
            }
            if (helpRoot == null)
            {
                throw new CliException("Can't find Help package root, please run this command inside a Help package");
            }

            var regex = new Regex("\"?id\"?:\\s+[\"'](.*)[\"']"); // match for menu item IDs

            var packagesDir = (mesVersion.Major > 9) ? this.fileSystem.DirectoryInfo.New(this.fileSystem.Path.Join(helpRoot, "projects")) : this.fileSystem.DirectoryInfo.New(this.fileSystem.Path.Join(helpRoot, "src", "packages"));
            var helpPackages = packagesDir.GetDirectories("cmf.docs.area.*".Replace(".", (mesVersion.Major > 9) ? "-" : "."));

            void GetMetadataFromFolder(IDirectoryInfo current, List<object> metadata, IDirectoryInfo parent = null)
            {
                if (parent != null)
                {
                    Log.Verbose($"Searching folder: {current.FullName}");
                    var files = current.GetFiles("*.md", SearchOption.TopDirectoryOnly);

                    foreach (var file in files)
                    {
                        Log.Verbose($"File: {file.Name}");
                        // get document title
                        var title = file.ReadToStringList()?.FirstOrDefault()?.TrimStart('#').Trim();

                        var basename = this.fileSystem.Path.GetFileNameWithoutExtension(file.FullName).ToLowerInvariant();
                        metadata.Add(new
                        {
                            id = basename,
                            menuGroupId = parent.Name.ToLowerInvariant(),
                            title = title,
                            actionId = ""
                        });
                    }
                }

                var folders = current.GetDirectories("*", SearchOption.TopDirectoryOnly)
                    .Where(d => d.Name != "images");
                foreach (var folder in folders)
                {
                    Log.Verbose($"Getting metadata from folder: {folder.FullName}");
                    GetMetadataFromFolder(folder, metadata, folder);
                }
            }

            foreach (var helpPackage in helpPackages)
            {
                var helpPackageMetadata = new List<object>();

                var assetsPath = helpPackage.GetDirectories("assets").FirstOrDefault();
                if (assetsPath == null)
                {
                    Log.Warning($"Help package {helpPackage.Name} does not contain an assets folder, which means it is not correctly formatted. Skipping...");
                    continue;
                }
                
                // check metadata file for menuGroupIds, to see if they are fully qualified or not
                var metadataFile = helpPackage.GetFiles("src/*.metadata.ts").FirstOrDefault();
                if (metadataFile?.Exists ?? false)
                {
                    var metadataContent = metadataFile.ReadToString();
                    var matchedIds = regex.Matches(metadataContent);
                    if (matchedIds.Any(m => m.Captures.Any(id => !id.Value.Contains("."))))
                    {
                        Log.Warning(
                            $"Using legacy menu item IDs! This package will not be deployable with other packages using legacy IDs, as collisions will happen!");
                    }
                }

                GetMetadataFromFolder(assetsPath, helpPackageMetadata);

                var menuItemsJson = this.fileSystem.Path.Join(assetsPath.FullName, "__generatedMenuItems.json");
                var jsonOut = System.Text.Json.JsonSerializer.Serialize(helpPackageMetadata, new JsonSerializerOptions() { WriteIndented = true });
                this.fileSystem.File.WriteAllText(menuItemsJson, jsonOut);
                Log.Verbose($"File '{menuItemsJson}' updated");
            }
        }
    }
}
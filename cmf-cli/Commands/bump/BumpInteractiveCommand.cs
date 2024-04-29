using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace Cmf.CLI.Commands.Bump
{
    /// <summary>
    ///     BumpInteractiveCommand
    /// </summary>
    /// <seealso cref="BaseCommand"/>
    [CmfCommand("interactive", Id = "bump_interactive", ParentId = "bump")]
    public class BumpInteractiveCommand : BaseCommand
    {
        /// <summary>
        ///     Configure command
        /// </summary>
        /// <param name="cmd">
        ///     Command
        /// </param>
        public override void Configure(Command cmd)
        {
            cmd.Handler = CommandHandler.Create(Execute);
        }

        /// <summary>
        ///     Executes the command
        /// </summary>
        public void Execute()
        {
            CmfPackageCollection allPackages = FileSystemUtilities.GetAllPackages(fileSystem);

            foreach (CmfPackage package in allPackages)
            {
                Log.Verbose("");
                Log.Verbose($"{package.PackageId}");
                Log.Verbose($"Current version: {package.Version}");
                string option = GenericUtilities.ReadStringValueFromConsole(prompt: "Bump? (Y/n):", allowEmptyValueInput: true, allowedValues: new string[] { "y", "n" });

                if (string.IsNullOrEmpty(option) || option.IgnoreCaseEquals("y"))
                {
                    Version version = new Version(package.Version);
                    string newVersion = string.Empty;
                    switch (GenericUtilities.ReadStringValueFromConsole(prompt: "Version to bump (a - Major) (i - Minor) (p - Patch):", allowedValues: new string[] { "a", "i", "p" }))
                    {
                        case "a":
                            {
                                newVersion = $"{version.Major + 1}.0.0";
                                break;
                            }
                        case "i":
                            {
                                newVersion = $"{version.Major}.{version.Minor + 1}.0";
                                break;
                            }
                        case "p":
                            {
                                newVersion = $"{version.Major}.{version.Minor}.{version.Build + 1}";
                                break;
                            }
                    }

                    string updateDepInParOption = GenericUtilities.ReadStringValueFromConsole(prompt: "Update in Parent Packages? (Y/n):", allowEmptyValueInput: true, allowedValues: new string[] { "y", "n" });
                    string renameVersionFoldersOption = GenericUtilities.ReadStringValueFromConsole(prompt: "Rename Version folders? (Y/n):", allowEmptyValueInput: true, allowedValues: new string[] { "y", "n" });

                    // Change Current Directory because BumpCommand uses it
                    Environment.CurrentDirectory = package.GetFileInfo().Directory.FullName;

                    new BumpCommand().Execute(
                        cmfPackage: package,
                        version: newVersion,
                        buildNr: null,
                        root: null,
                        packagesToUpdateDep: (string.IsNullOrEmpty(updateDepInParOption) || updateDepInParOption.IgnoreCaseEquals("y")) ? allPackages : null,
                        renameVersionFolders: string.IsNullOrEmpty(renameVersionFoldersOption) || renameVersionFoldersOption.IgnoreCaseEquals("y")
                    );
                }
            }
        }
    }
}
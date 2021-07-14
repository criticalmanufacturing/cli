using Cmf.Common.Cli.Attributes;
using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cmf.Common.Cli.Commands
{
    /// <summary>
    /// List dependencies command
    /// </summary>
    [CmfCommand("ls")]
    public class ListDependenciesCommand : BaseCommand
    {
        /// <summary>
        /// constructor
        /// </summary>
        public ListDependenciesCommand() : base()
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem"></param>
        public ListDependenciesCommand(IFileSystem fileSystem) : base(fileSystem)
        {
        }

        /// <summary>
        /// configure command signature
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            cmd.AddArgument(new Argument<IDirectoryInfo>(
                name: "workingDir",
                parse: (argResult) => Parse<IDirectoryInfo>(argResult, "."),
                isDefault: true
            )
            {
                Description = "Working Directory"
            });

            cmd.AddOption(new Option<string>(
                aliases: new string[] { "-r", "--repo" },
                description: "Repository where dependencies are located (url or folder)"));

            // Add the handler
            cmd.Handler = CommandHandler.Create<IDirectoryInfo, string>(Execute);
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="workingDir"></param>
        /// <param name="repo"></param>
        public void Execute(IDirectoryInfo workingDir, string repo)
        {
            IFileInfo cmfpackageFile = this.fileSystem.FileInfo.FromFileName($"{workingDir}/{CliConstants.CmfPackageFileName}");

            // Reading cmfPackage
            CmfPackage cmfPackage = CmfPackage.Load(cmfpackageFile, setDefaultValues: true);
            Log.Progress("Starting ls...");
            var loadedPackage = cmfPackage.LoadDependencies(repo, true);
            Log.Progress("Finished ls", true);
            DisplayTree(loadedPackage);
        }
        
        private string PrintBranch(List<bool> levels, bool isLast = false) {
            var sb = new StringBuilder();
            while (levels.Count > 0)
            {
                var level = levels[0];
                if (levels.Count > 1)
                {
                    sb.Append(level ? "  " : "| ");
                }
                else
                {
                    sb.Append(isLast ? "`-- " : "+-- ");
                }
                levels.RemoveAt(0);
            }
            return sb.ToString();
        }



        private void DisplayTree(CmfPackage pkg, List<bool> levels = null, bool isLast = false)
        {
            levels ??= new();

            Log.Information($"{this.PrintBranch(levels.ToList(), isLast)}{pkg.PackageId}@{pkg.Version} [{pkg.Location.ToString()}]");
            if (pkg.Dependencies.HasAny()) {
                for (int i = 0; i < pkg.Dependencies.Count; i++)
                {
                    Dependency dep = pkg.Dependencies[i];
                    var isDepLast = (i == (pkg.Dependencies.Count - 1));
                    var l = levels.Append(isDepLast).ToList();
                    if (dep.IsMissing)
                    {
                        if (dep.Mandatory)
                        {
                            Log.Error($"{this.PrintBranch(l, isDepLast)} MISSING {dep.Id}@{dep.Version}");
                        }
                        else
                        {
                            Log.Warning($"{this.PrintBranch(l, isDepLast)} MISSING {dep.Id}@{dep.Version}");
                        }
                    }
                    else
                    {
                        DisplayTree(dep.CmfPackage, l, isDepLast);
                    }
                }
            }
        }
    }
}

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

            cmd.AddOption(new Option<Uri[]>(
                aliases: new string[] { "-r", "--repos", "--repo" },
                description: "Repositories where dependencies are located (folder)"));

            // Add the handler
            cmd.Handler = CommandHandler.Create<IDirectoryInfo, Uri[]>(Execute);
        }

        /// <summary>
        /// Determine and print a package dependency tree
        /// </summary>
        /// <param name="workingDir">the path of the package which dependency tree we want to obtain</param>
        /// <param name="repos">a set of repositories for remote packages</param>
        public void Execute(IDirectoryInfo workingDir, Uri[] repos)
        {
            IFileInfo cmfpackageFile = this.fileSystem.FileInfo.FromFileName($"{workingDir}/{CliConstants.CmfPackageFileName}");

            // Reading cmfPackage
            CmfPackage cmfPackage = CmfPackage.Load(cmfpackageFile, setDefaultValues: true, this.fileSystem);
            
            if (repos != null)
            {
                ExecutionContext.Instance.RepositoriesConfig.Repositories.InsertRange(0, repos);
            }
            
            Log.Progress("Starting ls...");
            cmfPackage.LoadDependencies(ExecutionContext.Instance.RepositoriesConfig.Repositories.ToArray(), true);
            Log.Progress("Finished ls", true);
            DisplayTree(cmfPackage);
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

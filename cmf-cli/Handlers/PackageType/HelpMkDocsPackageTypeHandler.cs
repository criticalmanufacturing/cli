using System;
using System.IO;
using System.Linq;
using Cmf.CLI.Builders;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Handlers
{
    /// <summary>
    /// Handler for MkDocs-based Help packages.
    /// </summary>
    public class HelpMkDocsPackageTypeHandler : PackageTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HelpMkDocsPackageTypeHandler" /> class.
        /// </summary>
        public HelpMkDocsPackageTypeHandler(CmfPackage cmfPackage) : base(cmfPackage)
        {
            var packageDirectory = CmfPackage.GetFileInfo().DirectoryName;

            // MkDocs build must run where requirements.txt is located.
            var requirementsFile = this.fileSystem.Directory
                .GetFiles(packageDirectory, "requirements.txt", SearchOption.AllDirectories)
                .FirstOrDefault();

            if (requirementsFile == null)
            {
                throw new CliException($"Could not find requirements.txt under {packageDirectory}. Cannot build Help package.");
            }

            var workingDirectory = this.fileSystem.DirectoryInfo.New(this.fileSystem.Path.GetDirectoryName(requirementsFile));

            BuildSteps =
            [
                new CmdCommand()
                {
                    DisplayName = "mkdocs build",
                    Command = "\"python3 -m venv .venv && . .venv/bin/activate && python3 -m pip install mkdocs && pip install -r requirements.txt && mkdocs build\"",
                    Args = Array.Empty<string>(),
                    WorkingDirectory = workingDirectory
                }
            ];
        }
    }
}
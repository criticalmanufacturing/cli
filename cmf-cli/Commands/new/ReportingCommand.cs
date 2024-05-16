using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;

namespace Cmf.CLI.Commands.New
{
    /// <summary>
    /// Generates Reporting package structure
    /// </summary>
    [CmfCommand("reporting", ParentId = "new")]
    public class ReportingCommand : LayerTemplateCommand
    {
        /// <inheritdoc />
        public ReportingCommand() : base("reporting", PackageType.Reporting)
        {
            this.registerInParent = true;
        }

        /// <inheritdoc />
        public ReportingCommand(IFileSystem fileSystem) : base("reporting", PackageType.Reporting, fileSystem)
        {
            this.registerInParent = true;
        }

        /// <inheritdoc />
        protected override List<string> GenerateArgs(IDirectoryInfo projectRoot, IDirectoryInfo workingDir, List<string> args)
        {
            var relativePathToRoot =
                this.fileSystem.Path.Join("..", "..", //always two levels deeper
                    this.fileSystem.Path.GetRelativePath(
                        workingDir.FullName,
                        projectRoot.FullName)
                ).Replace("\\", "/");

            args.AddRange(new[]
            {
                "--rootRelativePath", relativePathToRoot
            });

            return args;
        }
    }
}

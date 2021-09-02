using Cmf.Common.Cli.Attributes;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cmf.Common.Cli.Commands.New
{
    /// <summary>
    /// Generates Database package structure
    /// </summary>
    [CmfCommand("database", Parent = "new")]
    public class DatabaseCommand : LayerTemplateCommand
    {
        /// <inheritdoc />
        public DatabaseCommand() : base("database", "Cmf.Custom.Database")
        {
        }

        /// <inheritdoc />
        public DatabaseCommand(IFileSystem fileSystem) : base("database", "Cmf.Custom.Database", fileSystem)
        {
        }

        /// <inheritdoc />
        protected override List<string> GenerateArgs(IDirectoryInfo projectRoot, IDirectoryInfo workingDir, List<string> args, JsonDocument projectConfig)
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

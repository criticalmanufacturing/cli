using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using System.Text.Json;
using Cmf.Common.Cli.Attributes;
using Cmf.Common.Cli.Enums;
using Cmf.Common.Cli.Utilities;

namespace Cmf.Common.Cli.Commands.New
{
    /// <summary>
    /// Generates IoT package structure
    /// </summary>
    [CmfCommand("iot", Parent = "new")]
    public class IoTCommand : LayerTemplateCommand
    {
        /// <summary>
        /// constructor
        /// </summary>
        public IoTCommand() : base("iot", "Cmf.Custom.IoT")
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem">the filesystem implementation</param>
        public IoTCommand(IFileSystem fileSystem) : base("iot", "Cmf.Custom.IoT", fileSystem)
        {
        }


        /// <inheritdoc />
        protected override List<string> GenerateArgs(IDirectoryInfo projectRoot, IDirectoryInfo workingDir, List<string> args, JsonDocument projectConfig)
        {
            var npmRegistry = projectConfig.RootElement.GetProperty("NPMRegistry").GetString();
            var devTasksVersion = projectConfig.RootElement.GetProperty("DevTasksVersion").GetString();
            
            // calculate relative path to local environment and create a new symbol for it
            var relativePathToRoot =
                this.fileSystem.Path.Join("..", "..", //always two levels deeper, because we are targeting the inner cmfpackage.json, which is one level down from the IoT root
                    this.fileSystem.Path.GetRelativePath(
                        workingDir.FullName,
                        projectRoot.FullName)
                ).Replace("\\", "/");
            
            args.AddRange(new []
            {
                "--rootInnerRelativePath", relativePathToRoot,
                "--DevTasksVersion", devTasksVersion,
                "--npmRegistry", npmRegistry
            });

            return args;
        }
    }
}
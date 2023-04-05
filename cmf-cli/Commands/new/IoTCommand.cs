using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text.Json;
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;

namespace Cmf.CLI.Commands.New
{
    /// <summary>
    /// Generates IoT package structure
    /// </summary>
    [CmfCommand("iot", ParentId = "new", Id = "new_iot")]
    public class IoTCommand : LayerTemplateCommand
    {
        /// <summary>
        /// constructor
        /// </summary>
        public IoTCommand() : base("iot", PackageType.IoT)
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem">the filesystem implementation</param>
        public IoTCommand(IFileSystem fileSystem) : base("iot", PackageType.IoT, fileSystem)
        {
        }


        /// <inheritdoc />
        protected override List<string> GenerateArgs(IDirectoryInfo projectRoot, IDirectoryInfo workingDir, List<string> args)
        {
            var npmRegistry = ExecutionContext.Instance.ProjectConfig.NPMRegistry;
            var devTasksVersion = ExecutionContext.Instance.ProjectConfig.DevTasksVersion;
            var repoType = ExecutionContext.Instance.ProjectConfig.RepositoryType ?? CliConstants.DefaultRepositoryType;
            Log.Debug($"Creating IoT Package at {workingDir} for repo type {repoType} using registry {npmRegistry}");

            // calculate relative path to local environment and create a new symbol for it
            var relativePathToRoot =
                this.fileSystem.Path.Join("..", "..", //always two levels deeper, because we are targeting the inner cmfpackage.json, which is one level down from the IoT root
                    this.fileSystem.Path.GetRelativePath(
                        workingDir.FullName,
                        projectRoot.FullName)
                ).Replace("\\", "/");

            args.AddRange(new[]
            {
                "--rootInnerRelativePath", relativePathToRoot,
                "--DevTasksVersion", devTasksVersion.ToString(),
                "--npmRegistry", npmRegistry.OriginalString,
                "--repositoryType", repoType.ToString()
            });

            return args;
        }
    }
}
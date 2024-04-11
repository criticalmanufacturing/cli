using System.Collections.Generic;
using System.IO.Abstractions;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Commands.New
{
    /// <summary>
    /// Generates the Grafana folder structure
    /// </summary>
    [CmfCommand(Name = "grafana", Id = "new_grafana", ParentId = "new")]
    public class GrafanaCommand : LayerTemplateCommand
    {
        /// <summary>
        /// constructor
        /// </summary>
        public GrafanaCommand() : base("grafana", PackageType.Grafana)
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem"></param>
        public GrafanaCommand(IFileSystem fileSystem) : base("grafana", PackageType.Grafana, fileSystem)
        {
        }

        /// <inheritdoc />
        protected override List<string> GenerateArgs(IDirectoryInfo projectRoot, IDirectoryInfo workingDir, List<string> args)
        {
            if (ExecutionContext.Instance.ProjectConfig.MESVersion.Major < 10)
            {
                throw new CliException("Version unsupported, available in MES >= 10");
            }

            var isRepositoryType = ExecutionContext.Instance.ProjectConfig.RepositoryType == RepositoryType.App;

            args.AddRange(new[]
            {
                "--app", isRepositoryType.ToString(),
            });
            return args;
        }
    }
}

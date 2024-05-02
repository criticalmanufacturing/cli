using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Cmf.CLI.Builders;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;

namespace Cmf.CLI.Handlers
{
    /// <summary>
    /// Grafana package type handler.
    /// </summary>
    /// <seealso cref="PackageTypeHandler" />
    public class GrafanaPackageTypeHandler : PackageTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GrafanaPackageTypeHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        public GrafanaPackageTypeHandler(CmfPackage cmfPackage) : base(cmfPackage)
        {
            cmfPackage.SetDefaultValues
            (
                targetLayer: "Grafana",
                isInstallable: true,
                isUniqueInstall: true,
                steps: new List<Step>()
                {
                    new(StepType.DeployFiles)
                    {
                        ContentPath = "**/**"
                    },
                }
            );

            IEnumerable<IBuildCommand> buildSteps = cmfPackage.BuildSteps?.Select(pbs => new SingleStepCommand() { BuildStep = pbs } as IBuildCommand);

            if (buildSteps != null && buildSteps.Any()) {
                BuildSteps = buildSteps.ToArray();
            }
        }
    }
}

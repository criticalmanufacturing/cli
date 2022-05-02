using System.Linq;
using Cmf.CLI.Builders;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Objects;

namespace Cmf.CLI.Handlers
{
    /// <summary>
    /// A Generic package manifest.
    /// Generic packages have no intrinsic behavior so build, pack and installation steps must be specified if needed.
    /// </summary>
    /// <seealso cref="PackageTypeHandler" />
    public class GenericPackageTypeHandler : PackageTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericPackageTypeHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        public GenericPackageTypeHandler(CmfPackage cmfPackage) : base(cmfPackage)
        {
            cmfPackage.SetDefaultValues
            (
                isInstallable: false
            );

            BuildSteps = cmfPackage.BuildSteps.Select(pbs => new SingleStepCommand() { BuildStep = pbs} as IBuildCommand).ToArray();
        }
    }
}
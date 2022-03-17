using Cmf.CLI.Constants;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Objects;

namespace Cmf.CLI.Handlers
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="PackageTypeHandler" />
    public class RootPackageTypeHandler : PackageTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RootPackageTypeHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        public RootPackageTypeHandler(CmfPackage cmfPackage) : base(cmfPackage)
        {
            cmfPackage.SetDefaultValues
            (
                name:
                    $"{cmfPackage.PackageId.Replace(".", " ")} (All)",
                keywords:
                    CliConstants.RootPackageDefaultKeyword
            );

            cmfPackage.DFPackageType = PackageType.Generic;
        }
    }
}
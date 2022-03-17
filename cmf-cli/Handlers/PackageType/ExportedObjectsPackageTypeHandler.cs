using Cmf.Common.Cli.Objects;
using System.Collections.Generic;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;

namespace Cmf.Common.Cli.Handlers
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Cmf.Common.Cli.Handlers.PackageTypeHandler" />
    public class ExportedObjectsPackageTypeHandler : PackageTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExportedObjectsPackageTypeHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        public ExportedObjectsPackageTypeHandler(CmfPackage cmfPackage) : base(cmfPackage)
        {
            cmfPackage.SetDefaultValues
            (
                steps:
                    new List<Step>()
                    {
                        new Step(StepType.CreateIntegrationEntries)
                        {
                            ContentPath = "ExportedObjects/**.xml",
                            MessageType = MessageType.ImportObject
                        }
                    }
            );

            cmfPackage.DFPackageType = PackageType.Generic;
        }
    }
}
using Cmf.Common.Cli.Enums;
using Cmf.Common.Cli.Objects;
using System.Collections.Generic;

namespace Cmf.Common.Cli.Handlers
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Cmf.Common.Cli.Handlers.PackageTypeHandler" />
    public class DatabasePackageTypeHandler : PackageTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabasePackageTypeHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        public DatabasePackageTypeHandler(CmfPackage cmfPackage) : base(cmfPackage)
        {
            cmfPackage.SetDefaultValues
            (
                isUniqueInstall:
                    true,
                steps:
                    new List<Step>()
                    {
                        new Step(StepType.RunSql)
                        {
                            ContentPath = "Online/*.sql",
                            TargetDatabase = "$(Product.Database.Online)"
                        },
                        new Step(StepType.EnqueueSql)
                        {
                            ContentPath = "ODS/*.sql",
                            TargetDatabase = "$(Product.Database.Ods)"
                        },
                        new Step(StepType.EnqueueSql)
                        {
                            ContentPath = "DWH/*.sql",
                            TargetDatabase = "$(Product.Database.Dwh)"
                        }
                    }
            );
        }
    }
}
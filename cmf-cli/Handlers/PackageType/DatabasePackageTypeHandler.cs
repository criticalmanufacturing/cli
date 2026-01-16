
using System.Collections.Generic;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;

namespace Cmf.CLI.Handlers
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="PackageTypeHandler" />
    public class DatabasePackageTypeHandler : PackageTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabasePackageTypeHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        public DatabasePackageTypeHandler(CmfPackage cmfPackage) : base(cmfPackage)
        {
            // Determine which database types are referenced in contentToPack by iterating once
            bool hasOnline = false;
            bool hasODS = false;
            bool hasDWH = false;

            if (cmfPackage.ContentToPack != null)
            {
                foreach (var content in cmfPackage.ContentToPack)
                {
                    if (content.Target != null)
                    {
                        var target = content.Target.TrimEnd('/');
                        
                        if (!hasOnline && target.Equals("Online", System.StringComparison.OrdinalIgnoreCase))
                        {
                            hasOnline = true;
                        }
                        if (!hasODS && target.Equals("ODS", System.StringComparison.OrdinalIgnoreCase))
                        {
                            hasODS = true;
                        }
                        if (!hasDWH && target.Equals("DWH", System.StringComparison.OrdinalIgnoreCase))
                        {
                            hasDWH = true;
                        }
                        
                        // Early exit if all database types have been found
                        if (hasOnline && hasODS && hasDWH)
                        {
                            break;
                        }
                    }
                }
            }

            // Only add steps for database types that are actually referenced
            var steps = new List<Step>();
            
            if (hasOnline)
            {
                steps.Add(new Step(StepType.RunSql)
                {
                    ContentPath = "Online/*.sql",
                    TargetDatabase = "$(Product.Database.Online)"
                });
            }
            
            if (hasODS)
            {
                steps.Add(new Step(StepType.EnqueueSql)
                {
                    ContentPath = "ODS/*.sql",
                    TargetDatabase = "$(Product.Database.Ods)"
                });
            }
            
            if (hasDWH)
            {
                steps.Add(new Step(StepType.EnqueueSql)
                {
                    ContentPath = "DWH/*.sql",
                    TargetDatabase = "$(Product.Database.Dwh)"
                });
            }

            cmfPackage.SetDefaultValues
            (
                isUniqueInstall:
                    true,
                steps:
                    steps
            );
        }
    }
}
using Cmf.Common.Cli.Enums;
using Cmf.Common.Cli.Handlers;
using Cmf.Common.Cli.Interfaces;
using Cmf.Common.Cli.Objects;
using System;
using System.IO;
using System.IO.Abstractions;

namespace Cmf.Common.Cli.Factories
{
    /// <summary>
    ///
    /// </summary>
    public static class PackageTypeFactory
    {
        /// <summary>
        /// Gets the package type handler.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="setDefaultValues"></param>
        /// <returns></returns>
        public static IPackageTypeHandler GetPackageTypeHandler(IFileInfo file, bool setDefaultValues = false)
        {
            // Load cmfPackage
            CmfPackage cmfPackage = CmfPackage.Load(file, setDefaultValues);

            return GetPackageTypeHandler(cmfPackage, setDefaultValues);
        }

        /// <summary>
        /// Gets the package type handler.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        /// <param name="setDefaultValues">if set to <c>true</c> [set default values].</param>
        /// <returns></returns>
        /// <exception cref="Cmf.Common.Cli.Utilities.CliException">
        /// </exception>
        public static IPackageTypeHandler GetPackageTypeHandler(CmfPackage cmfPackage, bool setDefaultValues = false)
        {
            PackageTypeHandler packageTypeHandler;
            packageTypeHandler = cmfPackage.PackageType switch
            {
                PackageType.Root => new RootPackageTypeHandler(cmfPackage),
                PackageType.Generic => new GenericPackageTypeHandler(cmfPackage),
                PackageType.Business => new BusinessPackageTypeHandler(cmfPackage),
                PackageType.Html => new HtmlPackageTypeHandler(cmfPackage),
                PackageType.Help => new HelpPackageTypeHandler(cmfPackage),
                PackageType.IoT => new IoTPackageTypeHandler(cmfPackage),
                PackageType.IoTData => cmfPackage.HandlerVersion switch
                {
                    2 => new IoTDataPackageTypeHandlerV2(cmfPackage),
                    1 => new IoTDataPackageTypeHandler(cmfPackage),
                    _ => new IoTDataPackageTypeHandlerV2(cmfPackage)
                },
                PackageType.Data => cmfPackage.HandlerVersion switch
                {
                    2 => new DataPackageTypeHandlerV2(cmfPackage),
                    1 => new DataPackageTypeHandler(cmfPackage),
                    _ => new DataPackageTypeHandlerV2(cmfPackage)
                },
                PackageType.Reporting => new ReportingPackageTypeHandler(cmfPackage),
                PackageType.ExportedObjects => new ExportedObjectsPackageTypeHandler(cmfPackage),
                PackageType.Database => new DatabasePackageTypeHandler(cmfPackage),
                PackageType.Test => new TestPackageTypeHandler(cmfPackage),
                _ => throw new NotImplementedException()
            };

            return packageTypeHandler;
        }
    }
}
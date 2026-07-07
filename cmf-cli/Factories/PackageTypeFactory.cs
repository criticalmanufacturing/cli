
using Cmf.CLI.Core;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Handlers;
using Cmf.CLI.Utilities;
using System;
using System.IO.Abstractions;

namespace Cmf.CLI.Factories
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
        /// <exception cref="CliException">
        /// </exception>
        public static IPackageTypeHandler GetPackageTypeHandler(CmfPackage cmfPackage, bool setDefaultValues = false)
        {
            IPackageTypeHandler packageTypeHandler;
            packageTypeHandler = cmfPackage.PackageType switch
            {
                PackageType.Root => new RootPackageTypeHandler(cmfPackage),
                PackageType.Generic => new GenericPackageTypeHandler(cmfPackage),
                PackageType.Business => new BusinessPackageTypeHandler(cmfPackage),
                PackageType.HTML => new HtmlNgCliPackageTypeHandler(cmfPackage),
                PackageType.Help => new HelpNgCliPackageTypeHandler(cmfPackage),
                PackageType.IoT => new IoTPackageTypeHandler(cmfPackage),
                PackageType.IoTData => cmfPackage.HandlerVersion switch
                {
                    1 => throw new CliException("Support for V1 Data packages was removed in CLI 6.0.0"),
                    _ => new IoTDataPackageTypeHandlerV2(cmfPackage)
                },
                PackageType.Data => cmfPackage.HandlerVersion switch
                {
                    1 => throw new CliException("Support for V1 Data packages was removed in CLI 6.0.0"),
                    _ => new DataPackageTypeHandlerV2(cmfPackage)
                },
                PackageType.Reporting => new ReportingPackageTypeHandler(cmfPackage),
                PackageType.ExportedObjects => new ExportedObjectsPackageTypeHandler(cmfPackage),
                PackageType.Database => new DatabasePackageTypeHandler(cmfPackage),
                PackageType.Tests => new TestPackageTypeHandler(cmfPackage),
                PackageType.SecurityPortal => new SecurityPortalPackageTypeHandlerV2(cmfPackage),
                PackageType.Grafana => new GrafanaPackageTypeHandler(cmfPackage),
                _ => throw new CliException(string.Format(CoreMessages.PackageTypeHandlerNotImplemented, cmfPackage.PackageType.ToString()))
            };

            return packageTypeHandler;
        }
    }
}


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
                PackageType.HTML => HtmlHandler(cmfPackage),
                PackageType.Help => HelpHandler(cmfPackage),
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
                PackageType.Tests => new TestPackageTypeHandler(cmfPackage),
                PackageType.SecurityPortal => SecurityPortalHandler(cmfPackage),
                _ => throw new CliException(string.Format(CoreMessages.PackageTypeHandlerNotImplemented, cmfPackage.PackageType.ToString()))
            };

            return packageTypeHandler;
        }

        private static IPackageTypeHandler HelpHandler(CmfPackage cmfPackage)
        {
            var targetVersion = ExecutionContext.Instance.ProjectConfig.MESVersion;
            var minimumVersion = new Version("10.0.0");
            if (targetVersion.CompareTo(minimumVersion) < 0)
            {
                return new HelpGulpPackageTypeHandler(cmfPackage);
            }

            return new HelpNgCliPackageTypeHandler(cmfPackage);
        }

        private static IPackageTypeHandler HtmlHandler(CmfPackage cmfPackage)
        {
            var targetVersion = ExecutionContext.Instance.ProjectConfig.MESVersion;
            var minimumVersion = new Version("10.0.0");
            if (targetVersion.CompareTo(minimumVersion) < 0)
            {
                return new HtmlGulpPackageTypeHandler(cmfPackage);
            }

            return new HtmlNgCliPackageTypeHandler(cmfPackage);
        }

        /// <summary>
        /// Creates the specific Security Portal package handler.
        /// 
        /// If the ProjectConfig's MESVersion is less than 10.0.0, a <seealso cref="SecurityPortalPackageTypeHandler"/> is created and returned.
        /// Otherwise, a <seealso cref="SecurityPortalPackageTypeHandlerV2"/> is created and returned.
        /// </summary>
        /// <param name="cmfPackage"></param>
        /// <returns></returns>
        private static IPackageTypeHandler SecurityPortalHandler(CmfPackage cmfPackage)
        {
            var targetVersion = ExecutionContext.Instance.ProjectConfig.MESVersion;
            var minimumVersion = new Version("10.0.0");
            if (targetVersion.CompareTo(minimumVersion) < 0)
            {
                return new SecurityPortalPackageTypeHandler(cmfPackage);
            }

            return new SecurityPortalPackageTypeHandlerV2(cmfPackage);
        }
    }
}

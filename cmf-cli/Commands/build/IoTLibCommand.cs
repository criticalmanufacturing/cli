using Cmf.CLI.Builders;
using Cmf.CLI.Constants;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO.Abstractions;
using System.Linq;
using Cmf.CLI.Core.Constants;

namespace Cmf.CLI.Commands.New
{
    /// <summary>
    /// Links all Related Packages to IoT Package
    /// </summary>
    [CmfCommand("lib", ParentId = "build_iot", Id = "build_iot_lib", Description = "Links all Related Packages to IoT Package")]
    public class IoTLibCommand : BaseCommand
    {
        /// <summary>
        /// constructor
        /// </summary>
        public IoTLibCommand() : base()
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem">the filesystem implementation</param>
        public IoTLibCommand(IFileSystem fileSystem) : base(fileSystem)
        {
        }

        public override void Configure(Command cmd)
        {
            var nearestRootPackage = FileSystemUtilities.GetPackageRootByType(
                this.fileSystem.Directory.GetCurrentDirectory(),
                PackageType.IoT,
                this.fileSystem
            );

            cmd.AddArgument(new Argument<IDirectoryInfo>(
                name: "workingDir",
                parse: (argResult) => Parse<IDirectoryInfo>(argResult, nearestRootPackage?.FullName),
                isDefault: true
            )
            {
                Description = "Working Directory"
            });

            cmd.Handler = CommandHandler.Create<IDirectoryInfo>(this.Execute);
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="workingDir">nearest root package</param>
        /// <param name="version">package version</param>
        public void Execute(IDirectoryInfo workingDir)
        {
            if (ExecutionContext.Instance.ProjectConfig.MESVersion.Major > 9)
            {
                IFileInfo cmfpackageFile = this.fileSystem.FileInfo.New($"{workingDir}/{CoreConstants.CmfPackageFileName}");
                var cmfPackage = CmfPackage.Load(cmfpackageFile, setDefaultValues: true, this.fileSystem);

                if (cmfPackage.PackageType != PackageType.IoT)
                {
                    throw new CliException(CliMessages.CommandIsOnlyValidForPackageOfTypeIoT);
                }

                var listOfLibs = workingDir.EnumerateDirectories().FirstOrDefault(dir => dir.Name == "dist").EnumerateDirectories();
                cmfPackage.RelatedPackages?.ForEach(relatedPackage =>
                {
                    if (relatedPackage.CmfPackage.GetFileInfo().Directory.GetFile(CliConstants.AngularJson) != null)
                    {
                        foreach (var lib in listOfLibs)
                        {
                            new NPMCommand()
                            {
                                DisplayName = "npm link dist",
                                Args = new string[] { "link", lib.FullName },
                                WorkingDirectory = relatedPackage.CmfPackage.GetFileInfo().Directory
                            }.Exec();
                        }
                    }
                });
            }
            else
            {
                throw new CliException(string.Format(CliMessages.InvalidVersionForCommand, "10"));
            }
        }
    }
}

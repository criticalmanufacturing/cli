using Cmf.CLI.Builders;
using Cmf.CLI.Constants;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

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

            var workingDirArgument = new Argument<IDirectoryInfo>("workingDir")
            {
                Description = "Working Directory"
            };
            workingDirArgument.CustomParser = argResult => Parse<IDirectoryInfo>(argResult, nearestRootPackage?.FullName);
            workingDirArgument.DefaultValueFactory = _ => Parse<IDirectoryInfo>(null, nearestRootPackage?.FullName);
            cmd.Arguments.Add(workingDirArgument);

            cmd.SetAction((parseResult, cancellationToken) =>
            {
                var workingDir = parseResult.GetValue(workingDirArgument);
                Execute(workingDir);
                return Task.FromResult(0);
            });
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="workingDir">nearest root package</param>
        /// <param name="version">package version</param>
        public void Execute(IDirectoryInfo workingDir)
        {
            if (Core.Objects.ExecutionContext.Instance.ProjectConfig.MESVersion.Major > 9)
            {
                IFileInfo cmfpackageFile = this.fileSystem.FileInfo.New($"{workingDir}/{CliConstants.CmfPackageFileName}");
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
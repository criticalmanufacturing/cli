using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using System.Text.Json;
using Cmf.Common.Cli.Attributes;
using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;

namespace Cmf.Common.Cli.Commands.New
{
    /// <summary>
    /// Generator for Security Portal package
    /// </summary>
    [CmfCommand("securityPortal", Parent = "new")]
    public class SecurityPortalCommand : LayerTemplateCommand
    {
        /// <summary>
        /// constructor
        /// </summary>
        public SecurityPortalCommand() : base("securityPortal", Enums.PackageType.SecurityPortal)
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem"></param>
        public SecurityPortalCommand(IFileSystem fileSystem) : base("securityPortal", Enums.PackageType.SecurityPortal, fileSystem)
        {
        }

        /// <inheritdoc />
        protected override List<string> GenerateArgs(IDirectoryInfo projectRoot, IDirectoryInfo workingDir, List<string> args, JsonDocument projectConfig)
        {
            return args;
        }
    }
}
using Cmf.CLI.Constants;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Factories;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Cmf.CLI.Commands
{
    /// <summary>
    /// Dummy upgrade command (only here to make "cmf upgrade base" work)
    /// </summary>
    /// <seealso cref="BaseCommand" />
    [CmfCommand("upgrade", Id = "upgrade", Description = "Project upgrade utilities")]
    public class UpgradeCommand : BaseCommand
    {
        
        /// <summary>
        /// constructor for System.IO filesystem
        /// </summary>
        public UpgradeCommand() : base()
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem"></param>
        public UpgradeCommand(IFileSystem fileSystem) : base(fileSystem)
        {
        }

        /// <summary>
        /// Configure command
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            // Add the handler using SetAction
            cmd.SetAction((parseResult, cancellationToken) =>
            {
                // If you have a packagePath argument/option defined, get it from parseResult
                // For now, assuming no arguments are needed for this dummy command
                Execute(null);
                return Task.FromResult(0);
            });
        }

        public void Execute(IDirectoryInfo packagePath) {}
    }
}
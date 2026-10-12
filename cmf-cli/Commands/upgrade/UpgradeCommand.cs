using Cmf.CLI.Core.Attributes;
using System.CommandLine;
using System.IO.Abstractions;

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
        }
    }
}

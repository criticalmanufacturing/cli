using System.CommandLine;
using Cmf.Common.Cli.Attributes;

namespace Cmf.Common.Cli.Commands
{
    /// <summary>
    /// "new" command group
    /// </summary>
    [CmfCommand("new")]
    public class NewCommand : BaseCommand
    {
        /// <summary>
        /// Configure command
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
        }
    }
}
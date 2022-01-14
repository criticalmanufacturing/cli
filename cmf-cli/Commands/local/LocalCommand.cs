using Cmf.Common.Cli.Attributes;
using System.CommandLine;

namespace Cmf.Common.Cli.Commands
{
    /// <summary>
    /// Local commands. TODO: migrate this to the dev plugin.
    /// </summary>
    /// <seealso cref="Cmf.Common.Cli.Commands.BaseCommand" />
    [CmfCommand("local", IsHidden = true)]
    public class LocalCommand : BaseCommand
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
using System.CommandLine;
using Cmf.CLI.Core.Attributes;

namespace Cmf.CLI.Commands
{
    /// <summary>
    /// Local commands. TODO: migrate this to the dev plugin.
    /// </summary>
    /// <seealso cref="BaseCommand" />
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
using Cmf.CLI.Core.Attributes;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace Cmf.CLI.Commands
{
    /// <summary>
    ///     DbManagerCommand
    /// </summary>
    [CmfCommand("dbmanager", Id = "dbmanager")]
    public class DbManagerCommand : BaseCommand
    {
        /// <summary>
        ///     Configure command
        /// </summary>
        /// <param name="cmd">
        ///     Command
        /// </param>
        public override void Configure(Command cmd)
        {
            cmd.Handler = CommandHandler.Create(Execute);
        }

        /// <summary>
        ///     Executes the command
        /// </summary>
        public void Execute()
        {
        }
    }
}
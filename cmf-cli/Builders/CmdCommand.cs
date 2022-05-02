using System.Linq;
using System.Runtime.InteropServices;
using Cmf.CLI.Core.Objects;

namespace Cmf.CLI.Builders
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="ProcessCommand" />
    /// <seealso cref="IBuildCommand" />
    public class CmdCommand : ProcessCommand, IBuildCommand
    {
        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public string Command { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the arguments.
        /// </summary>
        /// <value>
        /// The arguments.
        /// </value>
        public string[] Args { get; set; }

        /// <summary>
        /// Gets the steps.
        /// </summary>
        /// <returns></returns>
        public override ProcessBuildStep[] GetSteps()
        {
            var args = this.Args.ToList();

            args.Insert(0, (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "/c" : "-c"));
            args.Insert(1, this.Command);

            return new[]
            {
                new ProcessBuildStep()
                {
                    Command = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd" : "sh"),
                    Args = args.ToArray()
                }
            };
        }
    }
}
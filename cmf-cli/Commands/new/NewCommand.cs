using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Commands;

namespace Cmf.CLI.Commands
{
    /// <summary>
    /// "new" command group
    /// </summary>
    [CmfCommand("new", Id = "new")]
    public class NewCommand : TemplateCommand
    {
        private Command _cmd;

        /// <summary>
        /// Configure command
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            this._cmd = cmd;

            var resetOption = new Option<bool>("--reset")
            {
                Description = "Reset template engine. Use this if after an upgrade the templates are not working correctly."
            };
            cmd.Options.Add(resetOption);

            cmd.SetAction((parseResult, cancellationToken) =>
            {
                var reset = parseResult.GetValue(resetOption);
                Execute(reset);
                return Task.FromResult(0);
            });
        }

        /// <summary>
        /// Execute command
        /// </summary>
        /// <param name="reset"></param>
        public void Execute(bool reset)
        {
            if (reset)
            {
                RunCommand(new[] { "--debug:reinit" });
            }
            else
            {
                // In beta5, use Parse and Invoke pattern instead of direct Invoke
                var parseResult = this._cmd.Parse(new[] { "-h" });
                parseResult.Invoke();
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        public NewCommand() : base("new")
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem"></param>
        public NewCommand(IFileSystem fileSystem) : base("new", fileSystem)
        {
        }
    }
}
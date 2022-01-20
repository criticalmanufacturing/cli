using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using Cmf.Common.Cli.Attributes;

namespace Cmf.Common.Cli.Commands
{
    /// <summary>
    /// "new" command group
    /// </summary>
    [CmfCommand("new")]
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
            cmd.AddOption(new Option<bool>(
                aliases: new[] { "--reset" },
                description: "Reset template engine. Use this if after an upgrade the templates are not working correctly."
            ));
            cmd.Handler = CommandHandler.Create<bool>(Execute);
        }

        /// <summary>
        /// Execute command
        /// </summary>
        /// <param name="reset"></param>
        public void Execute(bool reset)
        {
            if (reset)
            {
                RunCommand(new []{ "--debug:reinit" });
            }
            else
            {
                this._cmd.Invoke(new [] { "-h" });
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        public NewCommand() : base("new")
        {
        }

    }
}
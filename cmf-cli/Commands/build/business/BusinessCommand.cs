using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO.Abstractions;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Commands;

namespace Cmf.CLI.Commands.build.business
{
	/// <summary>
	/// "new" command group
	/// </summary>
	[CmfCommand("business", Id = "build_business", ParentId = "build")]
	public class BusinessCommand : TemplateCommand
	{
		private Command _cmd;

		/// <summary>
		/// constructor
		/// </summary>
		public BusinessCommand() : base("business")
		{
		}

		/// <summary>
		/// Configure command
		/// </summary>
		/// <param name="cmd"></param>
		public override void Configure(Command cmd)
		{
			_cmd = cmd;
			cmd.Handler = CommandHandler.Create(Execute);
		}

		/// <summary>
		/// Execute command
		/// </summary>
		/// <param name="reset"></param>
		public void Execute()
		{
		}
	}
}

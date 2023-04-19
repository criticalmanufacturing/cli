using Cmf.CLI.Core.Attributes;
using System.CommandLine;

namespace Cmf.CLI.Commands.build.business
{
	/// <summary>
	/// "new" command group
	/// </summary>
	[CmfCommand("business", Id = "build_business", ParentId = "build")]
	public class BusinessCommand : BaseCommand
	{
		/// <summary>
		/// constructor
		/// </summary>
		public BusinessCommand()
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
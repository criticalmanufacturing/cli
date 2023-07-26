using Cmf.CLI.Commands.build.business.ValidateStartEndMethods;
using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Abstractions.Processors;
using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Extensions;
using Cmf.CLI.Core.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO.Abstractions;
using System.Text;

namespace Cmf.CLI.Commands.build.business;

/// <summary>
/// This command validates StartMethods and EndMethods
/// </summary>
/// <seealso cref="PowershellCommand" />
[CmfCommand(name: "validateStartAndEndMethods", ParentId = "build_business", Id = "build_business_validateStartAndEndMethods")]
public class ValidateStartAndEndMethodsCommand : BaseCommand
{
	public ValidateStartAndEndMethodsCommand()
	{
	}

	public ValidateStartAndEndMethodsCommand(IFileSystem fileSystem) : base(fileSystem)
	{
	}

	/// <summary>
	/// configure the command
	/// </summary>
	/// <param name="cmd"></param>
	public override void Configure(Command cmd)
	{
		cmd.AddArgument(new Argument<string>(
			name: "solutionPath",
			description: "The solution path"
		));

		var filesArgument = new Argument<string[]>(
			name: "files",
			description: "The files to validate"
		)
		{
			Arity = ArgumentArity.ZeroOrMore
		};

		cmd.AddArgument(filesArgument);

		cmd.Handler = CommandHandler.Create<string, string[]>(Execute);
	}

	/// <summary>
	/// Executes this instance.
	/// </summary>
	public void Execute(string solutionPath, string[] files)
	{
		if (string.IsNullOrEmpty(solutionPath))
		{
			return;
		}

		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

		// TODO: Can be replaced when cli implements Dependency injection
		var services = new ServiceCollection();
		services.AddProcessors();
		var serviceProvider = services.BuildServiceProvider();

		var solutionValidator = new SolutionValidator(serviceProvider.GetService<IProcessorFactory>(), solutionPath, files);
		solutionValidator.ValidateSolution().Wait();
	}
}
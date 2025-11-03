using Cmf.CLI.Commands.build.business.BusinessLinter;
using Cmf.CLI.Commands.build.business.BusinessLinter.Abstractions;
using Cmf.CLI.Commands.build.business.BusinessLinter.Extensions;
using Cmf.CLI.Core.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO.Abstractions;
using System.Text;

namespace Cmf.CLI.Commands.build.business;

/// <summary>
/// This command lints Business package code files
/// </summary>
[CmfCommand(name: "lint", ParentId = "build_business", Id = "build_business_lint")]
public class LintCommand : BaseCommand
{
	public LintCommand()
	{
	}

	public LintCommand(IFileSystem fileSystem) : base(fileSystem)
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
			description: "The files to lint"
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
			Console.Error.WriteLine("Error: Solution path is required.");
			return;
		}

		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

		// TODO: Can be replaced when cli implements Dependency injection
		var services = new ServiceCollection();
		services.AddLinterServices();
		var serviceProvider = services.BuildServiceProvider();

		var solutionLinter = new SolutionLinter(serviceProvider.GetService<IRuleFactory>(), solutionPath, files);
		solutionLinter.LintSolution().Wait();
	}
}

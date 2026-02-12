using Cmf.CLI.Commands.build.business.BusinessLinter.Abstractions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cmf.CLI.Commands.build.business.BusinessLinter.Rules;

internal abstract class BaseLintRule : ILintRule
{
	protected readonly ILintLogger _logger;

	protected BaseLintRule(ILintLogger logger)
	{
		_logger = logger;
	}

	public abstract string RuleName { get; }
	public abstract string RuleDescription { get; }
	public bool IsEnabled { get; set; } = true;

	public abstract void Analyze(MethodDeclarationSyntax methodNode, string filePath, string className);
}

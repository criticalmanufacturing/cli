using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cmf.CLI.Commands.build.business.BusinessLinter.Abstractions;

internal interface ILintRule
{
	string RuleName { get; }
	string RuleDescription { get; }
	bool IsEnabled { get; set; }
	void Analyze(MethodDeclarationSyntax methodNode, string filePath, string className);
}

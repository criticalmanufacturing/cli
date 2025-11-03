using Cmf.CLI.Commands.build.business.BusinessLinter.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Cmf.CLI.Commands.build.business.BusinessLinter.Rules;

internal class NoLoadInForeachRule : BaseLintRule
{
	public NoLoadInForeachRule(ILintLogger logger) : base(logger)
	{
	}

	public override string RuleName => "NoLoadInForeach";

	public override string RuleDescription => "Load() method should not be called inside foreach loops";

	public override void Analyze(MethodDeclarationSyntax methodNode, string filePath, string className)
	{
		if (methodNode.Body == null)
		{
			return;
		}

		// Find all foreach statements in the method
		var foreachStatements = methodNode.Body.DescendantNodes()
			.OfType<ForEachStatementSyntax>();

		foreach (var foreachStatement in foreachStatements)
		{
			// Find all invocation expressions within the foreach
			var invocations = foreachStatement.DescendantNodes()
				.OfType<InvocationExpressionSyntax>();

			foreach (var invocation in invocations)
			{
				// Check if this is a Load() method call
				if (IsLoadMethodCall(invocation))
				{
					var lineSpan = invocation.GetLocation().GetLineSpan();
					var line = lineSpan.StartLinePosition.Line + 1; // Line numbers are 0-based
					
					_logger.Warning($"[{RuleName}] {filePath}:{line} - {RuleDescription}. Found in class '{className}', method '{methodNode.Identifier.Text}'.");
				}
			}
		}
	}

	private bool IsLoadMethodCall(InvocationExpressionSyntax invocation)
	{
		// Check if the invocation is for a method named "Load"
		// Handle both simple calls like "obj.Load()" and chained calls like "material.Facility.Load()"
		
		if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
		{
			return memberAccess.Name.Identifier.ValueText == "Load";
		}
		
		if (invocation.Expression is IdentifierNameSyntax identifier)
		{
			return identifier.Identifier.ValueText == "Load";
		}

		return false;
	}
}

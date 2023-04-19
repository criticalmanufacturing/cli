using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Extensions;

internal static class ChildSyntaxListExtensions
{
	internal static KeyValuePair<string, string> GetParameterNameValuePair(this ParameterSyntax parameterSyntax, bool isEntityType)
	{
		string key;
		string value;

		if (isEntityType)
		{
			value = parameterSyntax.ChildTokens().Where(y => y.IsKind(SyntaxKind.IdentifierToken)).FirstOrDefault().Text;
			key = value.ToUpper(1);
		}
		else
		{
			key = parameterSyntax.ChildNodes()?.Where(x => x.IsKind(SyntaxKind.IdentifierName)).FirstOrDefault()?.ChildTokens().FirstOrDefault().Text;
			value = parameterSyntax.ChildTokens().Where(y => y.IsKind(SyntaxKind.IdentifierToken)).FirstOrDefault().Text;

			// In this case the type of the parameter is a int, long, etc.
			key ??= value.ToUpper(1);
		}

		return new KeyValuePair<string, string>(key, value);
	}
}
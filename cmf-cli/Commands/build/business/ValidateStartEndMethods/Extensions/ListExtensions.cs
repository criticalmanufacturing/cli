using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Extensions;

internal static class ListExtensions
{
    internal static bool ContainsKeyValue<TKey, TValue>(this IList<KeyValuePair<TKey, TValue>> list, TKey? key, TValue? value)
    {
        if (key is null || value is null)
            return false;

        return list.Contains(new KeyValuePair<TKey, TValue>(key, value));
    }

	internal static bool ContainsInputObject(this SeparatedSyntaxList<ParameterSyntax> list)
	{
		foreach (var parameter in list)
		{
			var identifierToken = parameter.ChildNodes()?.Where(x => x.IsKind(SyntaxKind.IdentifierName)).FirstOrDefault()?.ChildTokens().FirstOrDefault().Text;

			if (identifierToken != null && identifierToken.ToString().EndsWith("Input"))
				return true;
		}
		
		return false;		
	}
}
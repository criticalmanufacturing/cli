using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Extensions;

internal static class ChildSyntaxListExtensions
{
    internal static KeyValuePair<string, string> GetParameterNameValuePair(this ParameterSyntax parameterSyntax)
    {
        var key = parameterSyntax.ChildNodes()?.Where(x => x.IsKind(SyntaxKind.IdentifierName) || x.IsKind(SyntaxKind.PredefinedType)).FirstOrDefault()?.ChildTokens().FirstOrDefault().Text;
        var value = parameterSyntax.ChildTokens().Where(y => y.IsKind(SyntaxKind.IdentifierToken)).FirstOrDefault().Text;

        key ??= string.Empty;

        return new KeyValuePair<string, string>(key, value);
    }
}
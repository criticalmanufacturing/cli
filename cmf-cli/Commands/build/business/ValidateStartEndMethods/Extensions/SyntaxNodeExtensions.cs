using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Extensions;

internal static class SyntaxNodeExtensions
{
    internal static bool IsStartMethod(this SyntaxNode statement)
    {
        return statement.IsKind(SyntaxKind.ExpressionStatement) &&
            statement.ToString().Contains("StartMethod");
    }

    internal static bool IsEndMethod(this SyntaxNode statement)
    {
        return statement.IsKind(SyntaxKind.ExpressionStatement) &&
            statement.ToString().Contains("EndMethod");
    }

    internal static bool IsTryStatement(this SyntaxNode statement)
    {
        return statement.IsKind(SyntaxKind.TryStatement);
    }

    internal static SyntaxNode GetEndMethodExpression(this SyntaxNode statement)
    {
        return statement.ChildNodes()
            .Where(x => x.IsKind(SyntaxKind.Block))
            .SelectMany(x => x.ChildNodes())
            .Where(x => x.IsKind(SyntaxKind.ExpressionStatement) && x.ToString().Contains("EndMethod"))
            .FirstOrDefault();
    }
}
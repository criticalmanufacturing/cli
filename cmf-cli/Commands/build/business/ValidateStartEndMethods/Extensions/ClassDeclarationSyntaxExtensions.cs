using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Enums;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Extensions;

internal static class ClassDeclarationSyntaxExtensions
{
    public static ClassType GetClassType(this ClassDeclarationSyntax classDeclaration)
    {
        var name = classDeclaration.Identifier.ToString();

        if (name.EndsWith("Controller") &&
            classDeclaration.BaseList != null &&
            classDeclaration.BaseList.Types.Count > 0 &&
            classDeclaration.BaseList.Types[0].Type.ToString().Equals("ControllerBase"))
        {
            return ClassType.Controller;
        }

        if (name.EndsWith("Orchestration"))
        {
            return ClassType.Orchestration;
        }

        if (classDeclaration.BaseList != null && classDeclaration.BaseList.Types.Count > 0)
        {
            // TODO: EntityTypeCollection
            // TODO: EntityType
        }

        return ClassType.Other;
    }
}
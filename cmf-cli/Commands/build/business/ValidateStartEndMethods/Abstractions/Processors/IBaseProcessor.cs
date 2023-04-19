using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Abstractions.Processors;

internal interface IBaseProcessor
{
    bool IsStartMethod { get; }

    string StartEndMethodString { get; }

    void SetValues(string namespaceName, string className, string methodName, IEnumerable<ParameterSyntax> parameters, string outputType, ClassType classType);

    void ValidateParameterCount(ArgumentListSyntax statementArguments);

    void Process(SyntaxNode statement);
}
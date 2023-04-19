using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Abstractions.Processors;

internal interface IBaseProcessor
{
	bool IsStartMethod { get; }

	string StartEndMethodString { get; }

	void SetValues(ClassType classType, string namespaceName, string className, string methodName, string outputType, IEnumerable<ParameterSyntax> parameters, SyntaxNode statementToProcess);

	void ValidateParameterCount(ArgumentListSyntax statementArguments);

	void Process();
}
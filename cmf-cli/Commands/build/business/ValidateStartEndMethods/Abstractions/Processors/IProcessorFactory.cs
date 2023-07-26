using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Abstractions.Processors
{
	internal interface IProcessorFactory
	{
		public T Create<T>() where T : IBaseProcessor;

		public IBaseProcessor Create(ClassType classType, string namespaceName, string className, string methodName, string outputType, SeparatedSyntaxList<ParameterSyntax> parameters, SyntaxNode statement);
	}
}
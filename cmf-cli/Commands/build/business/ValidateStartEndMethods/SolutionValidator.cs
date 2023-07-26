using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Abstractions.Processors;
using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Enums;
using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cmf.CLI.Commands.build.business.ValidateStartEndMethods;

internal class SolutionValidator
{
	private readonly IProcessorFactory _processorFactory;
	private readonly string _solutionPath;
	private readonly IList<string> _files;

	public SolutionValidator(IProcessorFactory processorFactory, string solutionPath, IEnumerable<string> files)
	{
		_processorFactory = processorFactory;
		_solutionPath = solutionPath;
		_files = files.ToList();
	}

	public async Task ValidateSolution()
	{
		var solutionLoader = new SolutionLoader(_solutionPath);
		var workspace = solutionLoader.ReLoadSolution();
		var projects = workspace.CurrentSolution.Projects;

		foreach (var project in projects)
		{
			foreach (var document in project.Documents)
			{
				if (!_files.Any() || _files.Contains(document.Name))
				{
					var syntaxTree = await document.GetSyntaxTreeAsync();

					if (syntaxTree is null)
					{
						continue;
					}

					var namespaceNodes = syntaxTree.GetRoot().DescendantNodes().OfType<NamespaceDeclarationSyntax>();

					foreach (var namespaceNode in namespaceNodes)
					{
						var classDeclarations = namespaceNode.ChildNodes().OfType<ClassDeclarationSyntax>();

						foreach (var classDeclaration in classDeclarations)
						{
							var methodNodes = classDeclaration.ChildNodes().OfType<MethodDeclarationSyntax>();
							var classIdentifierToken = classDeclaration.Identifier;
							var classType = classDeclaration.GetClassType();

							if (classType != ClassType.Other)
							{
								foreach (MethodDeclarationSyntax methodNode in methodNodes)
								{
									ProcessMethod(namespaceNode, classType, methodNode, classIdentifierToken);
								}
							}
						}
					}
				}
			}
		}
	}

	public void ProcessMethod(NamespaceDeclarationSyntax namespaceNode, ClassType classType, MethodDeclarationSyntax methodNode, SyntaxToken classIdentifierToken)
	{
		var block = methodNode.Body;

		if (block is null)
		{
			return;
		}

		var methodName = methodNode.Identifier.ValueText;
		var outputType = methodNode.ReturnType is null ? string.Empty : methodNode.ReturnType.ToString();
		var parameters = methodNode.ParameterList.Parameters;
		var namespaceName = namespaceNode.Name.ToString();
		var className = classIdentifierToken.ToString();

		foreach (var statement in block.DescendantNodes().Where(x => x.IsKind(SyntaxKind.ExpressionStatement) && (x.IsStartMethod() || x.IsEndMethod())))
		{
			IBaseProcessor processor = _processorFactory.Create(classType, namespaceName, className, methodName, outputType, parameters, statement);
			processor?.Process();
		}
	}
}
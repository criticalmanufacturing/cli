using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Abstractions.Processors;
using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Enums;
using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cmf.CLI.Commands.build.business.ValidateStartEndMethods;

internal class SolutionValidator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string _solutionPath;
    private readonly IList<string> _files;

    public SolutionValidator(IServiceProvider serviceProvider, string solutionPath, IEnumerable<string> files)
    {
        _serviceProvider = serviceProvider;
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
                                    ProcessMethod(methodNode, namespaceNode, classType, classIdentifierToken);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void ProcessMethod(MethodDeclarationSyntax methodNode, NamespaceDeclarationSyntax namespaceNode, ClassType classType, SyntaxToken classIdentifierToken)
    {
        var block = methodNode.Body;
        var methodName = methodNode.Identifier.ValueText;
        var outputType = methodNode.ReturnType is null ? string.Empty : methodNode.ReturnType.ToString();
        var methodParameterList = methodNode.ParameterList.Parameters;
        var namespaceName = namespaceNode.Name.ToString();
        var className = classIdentifierToken.ToString();

        if (block is null)
        {
            return;
        }

        foreach (var statement in block.DescendantNodes())
        {
            var statementToProcess = statement;

            if (statement.IsTryStatement())
            {
                statementToProcess = statement.GetEndMethodExpression();
            }

            if (statementToProcess != null)
            {
                IBaseProcessor processor = null;

                if (statementToProcess.IsStartMethod())
                {
                    switch (classType)
                    {
                        case ClassType.Controller:
                            processor = _serviceProvider.GetRequiredService<IOrchestrationStartMethodProcessor>();
                            break;

                        case ClassType.Orchestration:
                            processor = _serviceProvider.GetRequiredService<IOrchestrationStartMethodProcessor>();
                            break;

                        case ClassType.EntityType:
                            // TODO: EntityType
                            break;

                        case ClassType.EntityTypeCollection:
                            // TODO: EntityTypeCollection
                            break;

                        default:
                            break;
                    }
                }
                else if (statementToProcess.IsEndMethod())
                {
                    switch (classType)
                    {
                        case ClassType.Controller:
                            processor = _serviceProvider.GetRequiredService<IOrchestrationEndMethodProcessor>();
                            break;

                        case ClassType.Orchestration:
                            processor = _serviceProvider.GetRequiredService<IOrchestrationEndMethodProcessor>();
                            break;

                        case ClassType.EntityType:
                            // TODO: EntityType
                            break;

                        case ClassType.EntityTypeCollection:
                            // TODO: EntityTypeCollection
                            break;

                        default:
                            break;
                    }
                }

                if (processor != null)
                {
                    processor.SetValues(namespaceName, className, methodName, methodParameterList, outputType);
                    processor.Process(statementToProcess);
                }
            }
        }
    }
}
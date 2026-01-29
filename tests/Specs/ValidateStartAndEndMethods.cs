using Autofac.Extras.Moq;
using Cmf.CLI.Commands.build.business.ValidateStartEndMethods;
using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Abstractions;
using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Enums;
using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Processors;
using Grpc.Net.Client.Configuration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace tests.Specs;

public class ValidateStartAndEndMethods
{
	private static string _baseNamespaceName = "Cmf.Foundation.BusinessOrchestration.SecurityManagement";
	private static string _baseClassName = "SecurityOrchestration";
	private static string _baseMethodName = "RemoveUsersFromRole";
	private static string _baseOutputType = "RemoveUsersFromRoleOutput";

	[Fact]
	public void Process_EverythingOk_ShouldNotLogAnything()
	{
		// Arrange
		using var mock = AutoMock.GetLoose();
		var logger = mock.Mock<IValidateLogger>();

		PrepareScenario(out var parameters, out var statement);

		var processor = mock.Create<OrchestrationStartMethodProcessor>();
		processor.SetValues(ClassType.ControllerOrOrchestration,
							_baseNamespaceName,
							_baseClassName,
							_baseMethodName,
							_baseOutputType,
							parameters,
							statement);

		// Act
		processor.Process();

		// Assert
		logger.Verify(x => x.Warning(It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public void Process_MethodNameWrong_ShouldLogMethodNameIsWrong()
	{
		// Arrange
		using var mock = AutoMock.GetLoose();
		var logger = mock.Mock<IValidateLogger>();
		var wrongMethodName = "WrongMethodName";
		PrepareScenario(out var parameters, out var statement);

		var processor = mock.Create<OrchestrationStartMethodProcessor>();
		processor.SetValues(ClassType.ControllerOrOrchestration,
							_baseNamespaceName,
							_baseClassName,
							wrongMethodName,
							_baseOutputType,
							parameters,
							statement);

		// Act
		processor.Process();

		// Assert
		logger.Verify(x => x.Warning(string.Format(ErrorMessages.StartMethodMethodNameIsIncorrect, _baseClassName, wrongMethodName)), Times.Once);
		logger.Verify(x => x.Warning(It.IsAny<string>()), Times.Once);
	}

	[Fact]
	public void Process_MissingArgument_ShouldLogMissingParameterAndIncorrectNumberOfParameters()
	{
		// Arrange
		using var mock = AutoMock.GetLoose();
		var logger = mock.Mock<IValidateLogger>();

		var extraParameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier("newParameter"))
									.WithType(SyntaxFactory.ParseTypeName("NewParameterType"));

		PrepareScenario(out var parameters, out var statement, aditionalParameters: new List<ParameterSyntax>() { extraParameter });

		var processor = mock.Create<OrchestrationStartMethodProcessor>();
		processor.SetValues(ClassType.ControllerOrOrchestration,
							_baseNamespaceName,
							_baseClassName,
							_baseMethodName,
							_baseOutputType,
							parameters,
							statement);

		// Act
		processor.Process();

		// Assert
		var missingParamMessage = string.Format(
			ErrorMessages.MethodParameterMissingOrIncorrectlyNamed,
			"StartMethod",
			_baseClassName,
			_baseMethodName,
			"NewParameterType");

		var incorrectCountMessage = string.Format(
			ErrorMessages.MethodHasIncorrectNumberOfParameters,
			"StartMethod",
			_baseClassName,
			_baseMethodName,
			3,
			4);

		logger.Verify(x => x.Warning(missingParamMessage), Times.Once);
		logger.Verify(x => x.Warning(incorrectCountMessage), Times.Once);
		logger.Verify(x => x.Warning(It.IsAny<string>()), Times.Exactly(2));
	}

	[Fact]
	public void Process_MethodNameWrongAndOutputNameWrong_ShouldLogInputObjectOutputObjectMethodNameDifferentAndOutputNameWrongOrMissing()
	{
		// Arrange
		using var mock = AutoMock.GetLoose();
		var logger = mock.Mock<IValidateLogger>();
		var wrongMethodName = "WrongMethodName";
		PrepareScenario(out var parameters, out var statement, new List<ArgumentSyntax>() { SyntaxFactory.Argument(GetObjectCreationExpressionSyntax("RemoveUsersFromRoleInput", "input")) });
		
		var processor = mock.Create<OrchestrationEndMethodProcessor>();
		processor.SetValues(ClassType.ControllerOrOrchestration,
							_baseNamespaceName,
							_baseClassName,
							wrongMethodName,
							_baseOutputType,
							parameters,
							statement);

		// Act
		processor.Process();

		// Assert
		var methodDontCoincideMessage = string.Format(
			ErrorMessages.InputOutputMethodDoNotCoincide, 
			_baseClassName, 
			wrongMethodName);
		
		var missingParamMessage = string.Format(
			ErrorMessages.MethodParameterMissingOrIncorrectlyNamed, 
			"EndMethod", 
			_baseClassName, 
			wrongMethodName, 
			_baseOutputType);

		logger.Verify(x => x.Warning(methodDontCoincideMessage), Times.Once);
		logger.Verify(x => x.Warning(missingParamMessage), Times.Once);
		logger.Verify(x => x.Warning(It.IsAny<string>()), Times.Exactly(2));
	}

	[Fact]
	public void Process_MethodNameWrong_ShouldLogInputObjectOutputObjectMethodNameDifferent()
	{
		// Arrange
		using var mock = AutoMock.GetLoose();
		var logger = mock.Mock<IValidateLogger>();
		var wrongMethodName = "WrongMethodName";
		PrepareScenario(out var parameters, out var statement, new List<ArgumentSyntax>() { SyntaxFactory.Argument(GetObjectCreationExpressionSyntax(_baseOutputType, "output")) });

		var processor = mock.Create<OrchestrationEndMethodProcessor>();
		processor.SetValues(ClassType.ControllerOrOrchestration,
							_baseNamespaceName,
							_baseClassName,
							wrongMethodName,
							_baseOutputType,
							parameters,
							statement);

		// Act
		processor.Process();

		// Assert
		logger.Verify(x => x.Warning(string.Format(ErrorMessages.InputOutputMethodDoNotCoincide, _baseClassName, wrongMethodName)), Times.Once);
		logger.Verify(x => x.Warning(It.IsAny<string>()), Times.Once);
	}

	[Fact]
	public void Process_OutputTypeWrong_ShouldLogMethodNameIsWrong()
	{
		// Arrange
		using var mock = AutoMock.GetLoose();
		var logger = mock.Mock<IValidateLogger>();
		PrepareScenario(out var parameters, out var statement);

		var processor = mock.Create<OrchestrationEndMethodProcessor>();
		processor.SetValues(ClassType.ControllerOrOrchestration,
							_baseNamespaceName,
							_baseClassName,
							_baseMethodName,
							"WrongOutput",
							parameters,
							statement);

		// Act
		processor.Process();

		// Assert
		logger.Verify(x => x.Warning(string.Format(ErrorMessages.InputOutputMethodDoNotCoincide, _baseClassName, _baseMethodName)), Times.Once);
	}

	private void PrepareScenario(out List<ParameterSyntax> parameters, out InvocationExpressionSyntax statement, List<ArgumentSyntax> aditionalArguments = null, List<ParameterSyntax> aditionalParameters = null)
	{
		var statementArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] {
																								SyntaxFactory.Argument(SyntaxFactory.IdentifierName("objectTypeName")),
																								SyntaxFactory.Argument(SyntaxFactory.IdentifierName("RemoveUsersFromRole")),
																								SyntaxFactory.Argument(GetObjectCreationExpressionSyntax("RemoveUsersFromRoleInput", "input"))
																							   }));
		if(aditionalArguments != null)
			statementArguments = statementArguments.AddArguments(aditionalArguments.ToArray());

		parameters = new List<ParameterSyntax>();
		var parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier("input"))
									.WithType(SyntaxFactory.ParseTypeName("RemoveUsersFromRoleInput"));
		parameters.Add(parameter);

		if (aditionalParameters != null)
			parameters.AddRange(aditionalParameters);

		var descendentNodes = new List<SyntaxNode>();
		var argumentListSyntax = SyntaxFactory.ArgumentList();
		descendentNodes.Add(argumentListSyntax);

		statement = SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("RemoveUsersFromRole"), statementArguments);
	}

	private ObjectCreationExpressionSyntax GetObjectCreationExpressionSyntax(string key, string value)
		=> SyntaxFactory.ObjectCreationExpression(
				SyntaxFactory.GenericName(SyntaxFactory.Identifier("KeyValuePair"))
					.WithTypeArgumentList(
						SyntaxFactory.TypeArgumentList(
							SyntaxFactory.SeparatedList<TypeSyntax>(
								new[]
								{
									SyntaxFactory.ParseTypeName("String"),
									SyntaxFactory.ParseTypeName("Object")
								}
							)
						)
					)
				)
				.WithArgumentList(
					SyntaxFactory.ArgumentList(
						SyntaxFactory.SeparatedList<ArgumentSyntax>(
							new[]
							{
								SyntaxFactory.Argument(
									SyntaxFactory.LiteralExpression(
										SyntaxKind.StringLiteralExpression,
										SyntaxFactory.Literal(key)
									)
								),
								SyntaxFactory.Argument(
									SyntaxFactory.IdentifierName(value)
								)
							}
						)
					)
				);
}

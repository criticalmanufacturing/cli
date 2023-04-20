using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Abstractions.Processors;
using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Enums;
using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Processors
{
	internal class ProcessorFactory : IProcessorFactory
	{
		private IServiceProvider _serviceProvider;

		public ProcessorFactory(IServiceProvider serviceProvider)
			=> _serviceProvider = serviceProvider;

		public T Create<T>() where T : IBaseProcessor
		{
			return _serviceProvider.GetService<T>();
		}

		public IBaseProcessor Create(ClassType classType, string namespaceName, string className, string methodName, string outputType, SeparatedSyntaxList<ParameterSyntax> parameters, SyntaxNode statement)
		{
			IBaseProcessor processor = null;

			if (statement.IsStartMethod())
			{
				switch (classType)
				{
					case ClassType.ControllerOrOrchestration:
						if (parameters.ContainsInputObject())
						{
							processor = Create<IOrchestrationStartMethodProcessor>();
							processor.SetValues(classType, namespaceName, className, methodName, outputType, parameters, statement);
						}

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
			else if (statement.IsEndMethod())
			{
				switch (classType)
				{
					case ClassType.ControllerOrOrchestration:
						if (parameters.ContainsInputObject())
						{
							processor = Create<IOrchestrationEndMethodProcessor>();
							processor.SetValues(classType, namespaceName, className, methodName, outputType, parameters, statement);
						}
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

			return processor;
		}
	}
}
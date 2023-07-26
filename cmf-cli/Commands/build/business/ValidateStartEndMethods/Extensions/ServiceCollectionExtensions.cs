using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Abstractions;
using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Abstractions.Processors;
using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Processors;
using Microsoft.Extensions.DependencyInjection;

namespace Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Extensions;

internal static class ServiceCollectionExtensions
{
	public static void AddProcessors(this ServiceCollection services)
	{
		// Add factories
		services.AddSingleton<IProcessorFactory, ProcessorFactory>();

		// Add classes
		services.AddTransient<IOrchestrationStartMethodProcessor, OrchestrationStartMethodProcessor>();
		services.AddTransient<IOrchestrationEndMethodProcessor, OrchestrationEndMethodProcessor>();

		// Add logger
		services.AddSingleton<IValidateLogger, ValidateLogger>();
	}
}
using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Abstractions.Processors;
using Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Processors;
using Microsoft.Extensions.DependencyInjection;

namespace Cmf.CLI.Commands.build.business.ValidateStartEndMethods.Extensions;

internal static class ServiceCollectionExtensions
{
	public static void AddProcessors(this ServiceCollection services)
	{
		// Add factories
		services.AddScoped<IProcessorFactory, ProcessorFactory>();

		// Add classes
		services.AddScoped<IOrchestrationStartMethodProcessor, OrchestrationStartMethodProcessor>();
		services.AddScoped<IOrchestrationEndMethodProcessor, OrchestrationEndMethodProcessor>();
	}
}
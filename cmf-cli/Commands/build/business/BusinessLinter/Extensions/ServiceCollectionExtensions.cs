using Cmf.CLI.Commands.build.business.BusinessLinter.Abstractions;
using Cmf.CLI.Commands.build.business.BusinessLinter.Rules;
using Microsoft.Extensions.DependencyInjection;

namespace Cmf.CLI.Commands.build.business.BusinessLinter.Extensions;

internal static class ServiceCollectionExtensions
{
	public static void AddLinterServices(this ServiceCollection services)
	{
		// Add factory
		services.AddSingleton<IRuleFactory, RuleFactory>();

		// Add logger
		services.AddSingleton<ILintLogger, LintLogger>();

		// Add rules
		services.AddTransient<ILintRule, NoLoadInForeachRule>();
	}
}

namespace Cmf.CLI.Commands.build.business.BusinessLinter.Abstractions;

internal interface IRuleFactory
{
	IEnumerable<ILintRule> CreateEnabledRules();
}

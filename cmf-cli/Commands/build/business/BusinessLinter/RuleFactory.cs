using Cmf.CLI.Commands.build.business.BusinessLinter.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cmf.CLI.Commands.build.business.BusinessLinter;

internal class RuleFactory : IRuleFactory
{
	private readonly IServiceProvider _serviceProvider;

	public RuleFactory(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	public IEnumerable<ILintRule> CreateEnabledRules()
	{
		// Get all registered rules from DI container
		var rules = _serviceProvider.GetServices<ILintRule>();
		
		// Return only enabled rules
		return rules.Where(r => r.IsEnabled);
	}
}

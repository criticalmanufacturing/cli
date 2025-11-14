using Cmf.CLI.Commands.build.business.BusinessLinter.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cmf.CLI.Commands.build.business.BusinessLinter;

internal class SolutionLinter
{
	private readonly IRuleFactory _ruleFactory;
	private readonly string _solutionPath;
	private readonly IList<string> _files;

	public SolutionLinter(IRuleFactory ruleFactory, string solutionPath, IEnumerable<string> files)
	{
		_ruleFactory = ruleFactory;
		_solutionPath = solutionPath;
		_files = files.ToList();
	}

	public async Task LintSolution()
	{
		var solutionLoader = new SolutionLoader(_solutionPath);
		var workspace = solutionLoader.ReLoadSolution();
		var projects = workspace.CurrentSolution.Projects;
		var rules = _ruleFactory.CreateEnabledRules().ToList();

		if (!rules.Any())
		{
			Console.WriteLine("No linting rules are enabled.");
			return;
		}

		Console.WriteLine($"Running {rules.Count} linting rule(s)...");

		foreach (var project in projects)
		{
			foreach (var document in project.Documents)
			{
				// Only lint specified files, or all files if none specified
				if (_files.Any() && !_files.Contains(document.Name))
				{
					continue;
				}

				var syntaxTree = await document.GetSyntaxTreeAsync();

				if (syntaxTree is null)
				{
					continue;
				}

				var root = syntaxTree.GetRoot();
				var filePath = document.FilePath ?? document.Name;

				// Find all class declarations
				var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

				foreach (var classDeclaration in classDeclarations)
				{
					var className = classDeclaration.Identifier.Text;
					var methodNodes = classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>();

					foreach (var methodNode in methodNodes)
					{
						// Apply all enabled rules to each method
						foreach (var rule in rules)
						{
							rule.Analyze(methodNode, filePath, className);
						}
					}
				}
			}
		}

		Console.WriteLine("Linting complete.");
	}
}

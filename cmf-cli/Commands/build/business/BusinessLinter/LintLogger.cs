using Cmf.CLI.Commands.build.business.BusinessLinter.Abstractions;
using System;

namespace Cmf.CLI.Commands.build.business.BusinessLinter;

internal class LintLogger : ILintLogger
{
	public void Warning(string message)
	{
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.WriteLine($"Warning: {message}");
		Console.ResetColor();
	}

	public void Error(string message)
	{
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine($"Error: {message}");
		Console.ResetColor();
	}
}

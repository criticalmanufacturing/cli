namespace Cmf.CLI.Commands.build.business.BusinessLinter.Abstractions;

internal interface ILintLogger
{
	void Warning(string message);
	void Error(string message);
}

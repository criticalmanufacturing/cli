
using Cmf.CLI.Core.Objects;

namespace Cmf.CLI.Utilities;

public class CommandUtilities
{
    public static void ThrowIfNoProjectConfig(ExecutionContext context)
    {
        if (context.ProjectConfig == null)
        {
            throw new CliException(@"No project config found. Please make sure you're executing this command inside a valid CMF repository.
You can init a repository with 'cmf init'.");
        }
    }
}
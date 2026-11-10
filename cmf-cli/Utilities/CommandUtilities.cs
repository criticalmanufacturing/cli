
using Cmf.CLI.Core.Objects;

namespace Cmf.CLI.Utilities;

public class CommandUtilities
{
    public static void ThrowIfNoProjectConfig(ExecutionContext context)
    {
        if (context?.ProjectConfig == null)
        {
            throw new CliException(CliMessages.ProjectContextRequired);
        }
    }
}

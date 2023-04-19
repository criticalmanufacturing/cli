using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

namespace Cmf.CLI.Commands.build.business.ValidateStartEndMethods;

/// <summary>
/// Loads a visual studio solution and make it ready to use by Roslyn
/// </summary>
public class SolutionLoader
{
    private readonly string _solutionPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="SolutionLoader"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="solutionPath">The solution path.</param>
    public SolutionLoader(string solutionPath)
    {
        _solutionPath = solutionPath;
    }

    /// <summary>
    /// LoadSolution from path
    /// </summary>
    /// <returns></returns>
    public MSBuildWorkspace ReLoadSolution()
    {
        if (!MSBuildLocator.IsRegistered)
        {
            MSBuildLocator.RegisterDefaults();
        }

        var _ = typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions);

        var workspace = MSBuildWorkspace.Create();

        workspace.OpenSolutionAsync(_solutionPath).Wait();

        return workspace;
    }
}
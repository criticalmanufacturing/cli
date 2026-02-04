using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

namespace Cmf.CLI.Commands.build.business.BusinessLinter;

/// <summary>
/// Loads a visual studio solution and make it ready to use by Roslyn
/// </summary>
internal class SolutionLoader
{
	private readonly string _solutionPath;

	/// <summary>
	/// Initializes a new instance of the <see cref="SolutionLoader"/> class.
	/// </summary>
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

		// Force load of the CSharp formatting assembly to prevent runtime errors
		// See: https://github.com/dotnet/roslyn/issues/48083
		var _ = typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions);

		var workspace = MSBuildWorkspace.Create();

		workspace.OpenSolutionAsync(_solutionPath).Wait();

		return workspace;
	}
}

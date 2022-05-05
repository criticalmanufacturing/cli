using System.Linq;
using Cmf.CLI.Core.Objects;

namespace Cmf.CLI.Builders;

/// <summary>
/// An un-opinionated command with a single execution step
/// Used to (de)serialize build steps for generic packages
/// </summary>
internal class SingleStepCommand : ProcessCommand, IBuildCommand
{
    /// <summary>
    /// The build step executed by this command
    /// </summary>
    public ProcessBuildStep BuildStep { get; init; }
    
    /// <inheritdoc />
    public override ProcessBuildStep[] GetSteps()
    {
        return new[] { this.BuildStep };
    }

    /// <inheritdoc />
    public string DisplayName
    {
        get => $"{BuildStep.Command} {string.Join(' ', BuildStep.Args)}";
        set { }
    }
    
    /// <summary>
    /// Only Executes on Test (--test)
    /// </summary>
    /// <value>
    /// boolean if to execute on Test should be true
    /// </value>
    public bool Test { get; set; } = false;

}
using System;
using System.Collections.Generic;
using System.Linq;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cmf.CLI.Builders;

public class NgCommand : ProcessCommand, IBuildCommand
{
    public string Command { get; set; }

    public string[] Args { get; set; }

    public string[] Projects { get; set; }

    public override ProcessBuildStep[] GetSteps()
    {
        var npx = new NPXCommand()
        {
            Command = $"@angular/cli@{ExecutionContext.ServiceProvider.GetService<IDependencyVersionService>().AngularCLI(ExecutionContext.Instance.ProjectConfig.MESVersion)}",
            Args = new List<string> { this.Command }.Concat(this.Args ?? Array.Empty<string>()).ToArray(),
            ForceColorOutput = false
        };
        var steps = npx.GetSteps();
        return steps.Select(s =>
        {
            s.EnvironmentVariables = new() { { "NODE_OPTIONS", "--max-old-space-size=8192" } };
            return s;
        }).ToArray();
    }

    public string DisplayName { get; set; }
    public bool Test { get; set; }
}

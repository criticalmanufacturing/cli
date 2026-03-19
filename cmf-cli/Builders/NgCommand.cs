using System;
using System.Collections.Generic;
using System.Linq;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Services;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Cmf.CLI.Builders;

public class NgCommand : ProcessCommand, IBuildCommand
{
    public required string Command { get; set; }

    public string[] Args { get; set; } = [];

    public string[] Projects { get; set; } = [];

    public override ProcessBuildStep[] GetSteps()
    {
        Version mesVersion = ExecutionContext.VerifyIsInsideProject().MESVersion;
        var npxCommand = $"@angular/cli@{ExecutionContext.ServiceProvider.GetRequiredService<IDependencyVersionService>().AngularCLI(mesVersion)}";
        var npx = new NPXCommand()
        {
            DisplayName = $"npx {npxCommand} {this.Command}",
            Command = npxCommand,
            Args = new List<string> { this.Command }.Concat(this.Args).ToArray(),
            ForceColorOutput = false
        };
        var steps = npx.GetSteps();
        return steps.Select(s =>
        {
            s.EnvironmentVariables = new() { { "NODE_OPTIONS", "--max-old-space-size=8192" } };
            return s;
        }).ToArray();
    }

    public required string DisplayName { get; set; }
    public bool Test { get; set; }
}

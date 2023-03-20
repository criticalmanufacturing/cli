using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Cmf.CLI.Core.Objects;
using Newtonsoft.Json;

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
            Command = "@angular/cli@15", // TODO: for future MES versions, we should determine the ng version automatically
            Args = new List<string> { this.Command }.Concat(this.Args ?? Array.Empty<string>()).ToArray()
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

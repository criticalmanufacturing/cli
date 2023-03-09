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
        var args = new List<string>
        {
            this.Command
        };

        return new[]
        {
            new ProcessBuildStep()
            {
                Command = "ng" + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".cmd" : ""),
                Args = args.Concat(this.Args ?? Array.Empty<string>()).ToArray(),
                WorkingDirectory = this.WorkingDirectory,
                EnvironmentVariables = new() { { "NODE_OPTIONS", "--max-old-space-size=8192" } }
            }
        };
    }

    public string DisplayName { get; set; }
    public bool Test { get; set; }
}

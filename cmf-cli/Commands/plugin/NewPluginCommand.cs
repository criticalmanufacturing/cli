using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Objects;
using Microsoft.Extensions.DependencyInjection;

namespace Cmf.CLI.Commands;

[CmfCommand("new", Parent = "plugins", Description = "Scaffold a new plugin")]
public class NewPluginCommand : TemplateCommand
{
    public NewPluginCommand() : base("plugin")
    {
    }

    public NewPluginCommand(IFileSystem fileSystem) : base("plugin", fileSystem)
    {
    }

    public override void Configure(Command cmd)
    {
        cmd.AddArgument(new Argument<string>(
            name: "name",
            description: "The name of the plugin. Will also be used as NPM package id, so make sure it is valid for this purpose. If you are scoping the package, you need to escape the organization name, e.g. \\@org/package."
        ));
        cmd.AddArgument(new Argument<string>(
            name: "binary",
            description: "The name of the plugin binary. This name will be prefixed with 'cmf-' to be handled by the CLI, e.g. examplePlugin will generate the cmf-examplePlugin binary in the path"
        ));
        cmd.AddArgument(new Argument<string>(
            name: "description",
             description: "The command description, e.g. \"My amazing plugin\""
        ));
        cmd.AddArgument(new Argument<IDirectoryInfo>(
            name: "workingDir",
            parse: (argResult) => Parse<IDirectoryInfo>(argResult, "."),
            isDefault: true
        )
        {
            Description = "Working Directory"
        });
        cmd.Handler = CommandHandler.Create<string, string, string, IDirectoryInfo>(this.Execute);
    }

    public void Execute(string name, string binary, string description, IDirectoryInfo workingDir)
    {
        using var activity = ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);

        if (name.StartsWith("\\@"))
        {
            Log.Debug($"Unescaping the package name {name}, removing the \"\\\"");
            name = name.Substring(1); // if we are scoping the package, we need to escape the @
        }
         
        
        var args = new List<string>()
        {
            // engine options
            "--output", workingDir.FullName,

            // template symbols
            "--pluginName", name,
            "--name", $"cmf-{binary}",
            "--binary", binary,
            "--description", description
        };
        
        this.RunCommand(args);
    }
}
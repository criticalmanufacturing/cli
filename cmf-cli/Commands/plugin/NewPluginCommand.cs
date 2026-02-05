using System.Collections.Generic;
using System.CommandLine;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Commands;
using Cmf.CLI.Core.Objects;
using Microsoft.Extensions.DependencyInjection;

namespace Cmf.CLI.Commands;

[CmfCommand("new", Id = "plugins_new", ParentId = "plugins", Description = "Scaffold a new plugin")]
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
        var nameArgument = new Argument<string>("name")
        {
            Description = "The name of the plugin. Will also be used as NPM package id, so make sure it is valid for this purpose. If you are scoping the package, you need to escape the organization name, e.g. \\@org/package."
        };
        cmd.Add(nameArgument);

        var binaryArgument = new Argument<string>("binary")
        {
            Description = "The name of the plugin binary. This name will be prefixed with 'cmf-' to be handled by the CLI, e.g. examplePlugin will generate the cmf-examplePlugin binary in the path"
        };
        cmd.Add(binaryArgument);

        var descriptionArgument = new Argument<string>("description")
        {
            Description = "The command description, e.g. \"My amazing plugin\""
        };
        cmd.Add(descriptionArgument);

        var workingDirArgument = new Argument<IDirectoryInfo>("workingDir")
        {
            Description = "Working Directory",
            CustomParser = argResult => Parse<IDirectoryInfo>(argResult, "."),
            DefaultValueFactory = _ => Parse<IDirectoryInfo>(null, ".")
        };
        cmd.Add(workingDirArgument);

        cmd.SetAction((parseResult, cancellationToken) =>
        {
            var name = parseResult.GetValue(nameArgument);
            var binary = parseResult.GetValue(binaryArgument);
            var description = parseResult.GetValue(descriptionArgument);
            var workingDir = parseResult.GetValue(workingDirArgument);

            Execute(name, binary, description, workingDir);
            return Task.FromResult(0);
        });
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
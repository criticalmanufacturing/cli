using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Objects;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Cmf.CLI.Commands;

[CmfCommand("plugins", Id="plugins", Description = "Search for plugins for the CLI")]
public class PluginsCommand : BaseCommand
{
    public override void Configure(Command cmd)
    {
        cmd.AddOption(new Option<Uri[]>(
            aliases: new [] { "-r", "--registry" },
            description: "Registries to query for plugins. If unspecified, uses the default NPMJS registry."));
        cmd.Handler = CommandHandler.Create<Uri[]>(Execute);
    }

    public void Execute(Uri[] registry)
    {
        var npmClient = ExecutionContext.ServiceProvider.GetService<INPMClient>();
        var packages = npmClient.FindPlugins(registry);
        foreach (var package in packages)
        {
            Log.Render(new Markup($"[bold deepskyblue1] {package.Name}[/]   {(package.IsOfficial ? "[default on green3] :check_mark:  Official Plugin [/]" : "")}"));
            Log.AnsiConsole.WriteLine();
            Log.Render(new Markup($"\t[grey]Package info:[/] {package.Link.AbsoluteUri}"));
            Log.AnsiConsole.WriteLine();
            Log.Render(new Markup($"\t[grey]Install with:[/] npm install --global {package.Name}{(registry?.Length > 0 ? $" --registry {package.Registry}" : "")}"));
            Log.AnsiConsole.WriteLine();
            Log.AnsiConsole.WriteLine();
        }
    }
}
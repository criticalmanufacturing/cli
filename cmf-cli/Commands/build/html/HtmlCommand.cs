using System.CommandLine;
using Cmf.CLI.Core.Attributes;

namespace Cmf.CLI.Commands.build.html;
/// <summary>
/// "build html" command group
/// </summary>
[CmfCommand("html", Id = "build_html", ParentId = "build")]
public class HtmlCommand : BaseCommand
{
    public override void Configure(Command cmd)
    {
    }
}
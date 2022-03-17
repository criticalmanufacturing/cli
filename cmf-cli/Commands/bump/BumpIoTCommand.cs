using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cmf.CLI.Core.Attributes;

namespace Cmf.Common.Cli.Commands
{
    /// <summary>
    /// iot command group
    /// </summary>
    [CmfCommand(name: "iot", Parent = "bump")]
    public class BumpIoTCommand : BaseCommand
    {
        /// <summary>
        /// Configure command (no-op, command is a group only)
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
        }
    }
}

using Cmf.CLI.Core.Attributes;
using System.CommandLine;

namespace Cmf.CLI.Commands
{
    /// <summary>
    /// Parent Command For IoT
    /// </summary>
    /// <seealso cref="BaseCommand" />
    [CmfCommand(name: "iot", ParentId = "build", Id = "build_iot", Description = "Parent Command For IoT")]
    public class IotCommand : BaseCommand
    {
        /// <summary>
        /// Configure command
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
        }
    }
}
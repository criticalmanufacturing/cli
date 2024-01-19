using System.CommandLine;

namespace Cmf.CLI.Core.Commands
{
    /// <summary>
    ///
    /// </summary>
    public interface IBaseCommand
    {
        public void Configure(Command cmd);
    }
}
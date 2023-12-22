using Cmf.CLI.Core.Objects;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace Cmf.CLI.Builders
{
    /// <summary>
    /// Interface for Process command
    /// </summary>
    public interface IProcessCommand
    {
        /// <summary>
        /// Gets or sets the working directory.
        /// </summary>
        /// <value>
        /// The working directory.
        /// </value>
        public IDirectoryInfo WorkingDirectory { get; set; }

        /// <summary>
        /// This method will be used to do a run check before the Exec() is able to run.
        /// If Condition() is false, the Exec() will not be able to run
        /// If Condition() is true, the Exec() will run
        /// </summary>
        /// <returns></returns>
        public bool Condition();

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <returns></returns>
        public Task Exec();

        /// <summary>
        /// Gets the steps.
        /// </summary>
        /// <returns></returns>
        public abstract ProcessBuildStep[] GetSteps();
    }
}
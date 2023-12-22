using System.Threading.Tasks;

namespace Cmf.CLI.Builders
{
    /// <summary>
    ///
    /// </summary>
    public interface IBuildCommand
    {
        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Only Executes on Test (--test)
        /// </summary>
        /// <value>
        /// boolean if to execute on Test should be true
        /// </value>
        public bool Test { get; set; }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <returns></returns>
        public Task Exec();

        /// <summary>
        /// This method will be used to do a run check before the Exec() is able to run.
        /// If Condition() is false, the Exec() will not be able to run
        /// If Condition() is true, the Exec() will run
        /// </summary>
        /// <returns></returns>
        public bool Condition();
    }
}
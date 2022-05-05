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
    }
}
using System.Threading.Tasks;

namespace Cmf.Common.Cli.Builders
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
        /// Executes this instance.
        /// </summary>
        /// <returns></returns>
        public Task Exec();
    }
}
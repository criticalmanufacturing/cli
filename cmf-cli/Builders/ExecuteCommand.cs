using System;
using System.Threading.Tasks;
using Cmf.CLI.Commands;

namespace Cmf.CLI.Builders
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="IBuildCommand" />
    public class ExecuteCommand<T> : IBuildCommand where T : BaseCommand
    {
        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public T Command { get; set; }

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
        public bool Test { get; set; } = false;

        /// <summary>
        /// Gets or sets the execute.
        /// </summary>
        /// <value>
        /// The execute.
        /// </value>
        public Action<T> Execute { get; set; }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <returns></returns>
        public Task Exec()
        {
            dynamic command = this.Command;
            if (this.Execute != null)
            {
                this.Execute(this.Command);
            }
            else
            {
                command.Execute();
            }

            return null;
        }
    }
}
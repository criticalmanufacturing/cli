using Cmf.CLI.Commands;
using Cmf.CLI.Core;
using System;
using System.Threading.Tasks;

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
        /// Gets or sets the condition. This will impact the Exec
        /// </summary>
        /// <value>
        /// The execute.
        /// </value>
        public Func<bool> ConditionForExecute = () => { return true; };

        /// <summary>
        /// Gets or sets the execute.
        /// </summary>
        /// <value>
        /// The execute.
        /// </value>
        public Action<T> Execute { get; set; }

        /// <summary>
        /// Condition if true. You can execute this instance.
        /// </summary>
        /// <returns></returns>
        public bool Condition()
        {
            return this.ConditionForExecute();
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <returns></returns>
        public Task Exec()
        {
            if (this.Condition())
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
            }
            else
            {
                Log.Debug($"Command: {this.DisplayName} will not be executed as its condition was not met");
            }
            return null;
        }
    }
}
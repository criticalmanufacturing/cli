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
    public class ExecuteCommand<T> : IBuildCommand where T : Cmf.CLI.Core.Commands.IBaseCommand
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
        /// Gets or sets the condition. 
        /// This will impact the Condition(), the Condtion will run the Func to determine if it should reply with true or false
        /// By Default it will return true
        /// </summary>
        /// <value>
        /// A Func that if it returns true it will allow the Execute to run.
        /// </value>
        /// <returns>Func<bool></returns>
        public Func<bool> ConditionForExecute = () => { return true; };

        /// <summary>
        /// Gets or sets the execute.
        /// </summary>
        /// <value>
        /// The execute.
        /// </value>
        public Action<T> Execute { get; set; }

        /// <summary>
        /// This method will be used to do a run check before the Exec() is able to run.
        /// If Condition() is false, the Exec() will not be able to run
        /// If Condition() is true, the Exec() will run
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
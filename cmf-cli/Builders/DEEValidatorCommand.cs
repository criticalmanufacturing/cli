using Cmf.CLI.Core;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cmf.CLI.Builders
{
    /// <summary>
    /// Validator for DEE files
    /// </summary>
    /// <seealso cref="ProcessCommand" />
    /// <seealso cref="IBuildCommand" />
    public class DEEValidatorCommand : IBuildCommand
    {
        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public required List<FileToPack> FilesToValidate { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public required string DisplayName { get; set; }

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
        /// This method will be used to do a run check before the Exec() is able to run.
        /// If Condition() is false, the Exec() will not be able to run
        /// If Condition() is true, the Exec() will run
        /// </summary>
        /// <returns></returns>
        public bool Condition()
        {
            return ConditionForExecute();
        }
        /// <summary>
        /// Search all the cs DEE files and validate them
        /// </summary>
        /// <returns></returns>
        public Task Exec()
        {
            if (Condition())
            {
                var errors = new List<string>();
                if (FilesToValidate is null)
                    throw new CliException("FilesToValidate cannot be null.");
                foreach (var file in FilesToValidate.Where(file =>
                                    file.Source!.FullName.Contains(".cs") &&
                                    !file.Source!.FullName.Contains(".csproj") &&
                                    !file.Source!.FullName.Contains("DeeDevBase.cs") &&
                                    !file.Source!.FullName.Replace("\\", "/").Contains("/Properties") &&
                                    !file.Source!.FullName.Replace("\\", "/").Contains("/bin") &&
                                    !file.Source!.FullName.Replace("\\", "/").Contains("/obj")))
                {
                    if (file.Source is null)
                        throw new CliException($"FileToPack.Source cannot be null for file: {file}");
                    Stream fs = file.Source.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
                    StreamReader sr = new StreamReader(fs);
                    string fileContent = sr.ReadToEnd();
                    // ...validation logic...
                    // (unchanged)
                }
                if (errors.Count > 0)
                {
                    throw new CliException($"DEE Validation failed with the following errors:{Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
                }
            }
            else
            {
                Log.Debug($"Command: {(DisplayName ?? "<null>")} will not be executed as its condition was not met");
            }
            return Task.CompletedTask;
        }
    }
}
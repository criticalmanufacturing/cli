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
        public List<FileToPack> FilesToValidate { get; set; }

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
                foreach (var file in FilesToValidate.Where(file =>
                                                    file.Source.FullName.Contains(".cs") &&
                                                    !file.Source.FullName.Contains(".csproj") &&
                                                    !file.Source.FullName.Contains("DeeDevBase.cs") &&
                                                    !file.Source.FullName.Replace("\\", "/").Contains("/Properties") &&
                                                    !file.Source.FullName.Replace("\\", "/").Contains("/bin") &&
                                                    !file.Source.FullName.Replace("\\", "/").Contains("/obj")))
                {
                    //Open file for Read\Write
                    Stream fs = file.Source.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);

                    //Create object of StreamReader by passing FileStream object on which it needs to operates on
                    StreamReader sr = new StreamReader(fs);

                    //Use ReadToEnd method to read all the content from file
                    string fileContent = sr.ReadToEnd();

                    #region Validate DEE Indicators

                    if (!fileContent.Contains("//---Start DEE Condition Code---") ||
                        !fileContent.Contains("//---End DEE Condition Code---") ||
                        !fileContent.Contains("//---Start DEE Code---") ||
                        !fileContent.Contains("//---End DEE Code---"))
                    {
                        throw new CliException($"DEE File {file.Source.FullName} is not a valid. It does not have all the valid indicators");
                    }

                    #endregion

                    #region Validate UseReference

                    var matches = Regex.Matches(fileContent, @"UseReference\((.*)\);");

                    foreach (Group match in matches)
                    {
                        // Expected UseReference("x.dll", "y");
                        if (match.Value.Length != Regex.Replace(match.Value, @"\s+", "").Length + 1)
                        {
                            throw new CliException($"DEE File {file.Source.FullName} is not a valid on '{match.Value}'. UseReference contains a whitespace, please refer to the valid format UseReference(\"x.dll\", \"y\");");
                        }
                    }
                    #endregion
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
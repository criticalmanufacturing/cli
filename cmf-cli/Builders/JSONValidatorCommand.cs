using Cmf.CLI.Core;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using NuGet.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cmf.CLI.Builders
{
    /// <summary>
    /// Validator for json files
    /// </summary>
    /// <seealso cref="ProcessCommand" />
    /// <seealso cref="IBuildCommand" />
    public class JSONValidatorCommand : IBuildCommand
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
        /// Search all the json files and validate them
        /// </summary>
        /// <returns></returns>
        public Task Exec()
        {
            if (Condition())
            {
                foreach (var file in FilesToValidate.Where(file => file.Source.FullName.Contains(".json")))
                {
                    //Open file for Read\Write
                    Stream fs = file.Source.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);

                    //Create object of StreamReader by passing FileStream object on which it needs to operates on
                    StreamReader sr = new StreamReader(fs);

                    //Use ReadToEnd method to read all the content from file
                    string fileContent = sr.ReadToEnd();

                    try
                    {
                        var json = JsonDocument.Parse(fileContent);
                    }
                    catch (Exception)
                    {
                        throw new CliException($"File {file.Source.FullName} is not a valid json");
                    }

                    #region Validate IoT Workflow Paths

                    var matchCheckWorlflowFilePath = Regex.Matches(fileContent, @"Workflow"": ""(.*?)\\(.*?).json");

                    if (matchCheckWorlflowFilePath.Any())
                    {
                        throw new CliException($"JSON File {file.Source.FullName} is not a valid on '{matchCheckWorlflowFilePath.Select(x => x.Value + "/r/n").ToJson()}'. Please normalize all slashes to be forward slashes /");
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
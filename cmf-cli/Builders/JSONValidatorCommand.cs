using Cmf.Common.Cli.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cmf.Common.Cli.Builders
{
    /// <summary>
    /// Validator for json files
    /// </summary>
    /// <seealso cref="Cmf.Common.Cli.Builders.ProcessCommand" />
    /// <seealso cref="Cmf.Common.Cli.Builders.IBuildCommand" />
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
        /// Search all the json files and validate them
        /// </summary>
        /// <returns></returns>
        public Task Exec()
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
                catch(Exception)
                {
                    throw new Exception($"File {file.Source.FullName} is not a valid json");
                }
            }
            return null;
        }
    }
}
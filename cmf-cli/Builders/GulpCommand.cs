using System.Collections.Generic;

namespace Cmf.CLI.Builders
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="ProcessCommand" />
    /// <seealso cref="IBuildCommand" />
    public class GulpCommand : ProcessCommand, IBuildCommand
    {
        /*
         *  - task: Gulp@1
              displayName: 'gulp install'
              inputs:
                gulpFile: UI/HTML/gulpfile.js
                targets: install
                gulpjs: node_modules/gulp/bin/gulp.js
                arguments: '--update'
                workingDirectory: UI/HTML
         */

        /// <summary>
        /// Gets or sets the gulp file.
        /// </summary>
        /// <value>
        /// The gulp file.
        /// </value>
        public string GulpFile { get; set; }

        /// <summary>
        /// Gets or sets the task.
        /// </summary>
        /// <value>
        /// The task.
        /// </value>
        public string Task { get; set; }

        /// <summary>
        /// Gets or sets the gulp js.
        /// </summary>
        /// <value>
        /// The gulp js.
        /// </value>
        public string GulpJS { get; set; }

        /// <summary>
        /// Gets or sets the arguments.
        /// </summary>
        /// <value>
        /// The arguments.
        /// </value>
        public string[] Args { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets the steps.
        /// </summary>
        /// <returns></returns>
        public override ProcessBuildStep[] GetSteps()
        {
            var args = new List<string>
            {
                this.GulpJS, // TODO: support global gulp if we have no gulpJS declared
            };
            if (this.GulpFile != null)
            {
                args.AddRange(new[]
                {
                    "--gulpfile",
                    this.GulpFile
                });
            }
            args.Add(this.Task);
            if (this.Args != null)
            {
                args.AddRange(this.Args);
            }
            args.Add("--color");

            return new[]
            {
                new ProcessBuildStep()
                {
                    Command = "node",
                    Args = args.ToArray(),
                    WorkingDirectory = this.WorkingDirectory
                }
            };
        }
    }
}
using Cmf.CLI.Builders;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace Cmf.CLI.Commands.Run
{
    /// <summary>
    ///     RunHelpCommand
    /// </summary>
    /// <seealso cref="BaseCommand"/>
    [CmfCommand("help", Id = "run_help", ParentId = "run")]
    public class RunHelpCommand : BaseCommand
    {
        /// <summary>
        ///     Configure command
        /// </summary>
        /// <param name="cmd">
        ///     Command
        /// </param>
        public override void Configure(Command cmd)
        {
            cmd.Handler = CommandHandler.Create(Execute);
        }

        /// <summary>
        ///     Executes the command
        /// </summary>
        public void Execute()
        {
            CmfPackage helpPackage = GenericUtilities.SelectPackage(fileSystem, packageType: PackageType.Help);

            // Build Help
            new GulpCommand()
            {
                GulpFile = "gulpfile.js",
                Task = "build",
                DisplayName = "Gulp Build",
                GulpJS = "node_modules/gulp/bin/gulp.js",
                Args = new[] { "--production" },
                WorkingDirectory = helpPackage.GetFileInfo().Directory
            }.Exec();

            // Start and Watch Help (allows Rebuild on changes)
            new GulpCommand()
            {
                GulpFile = "gulpfile.js",
                Task = "start watch",
                DisplayName = "Gulp Start Watch",
                GulpJS = "node_modules/gulp/bin/gulp.js",
                WorkingDirectory = helpPackage.GetFileInfo().Directory
            }.Exec();
        }
    }
}
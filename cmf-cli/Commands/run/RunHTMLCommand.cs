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
    ///     RunHTMLCommand
    /// </summary>
    /// <seealso cref="BaseCommand"/>
    [CmfCommand("html", Id = "run_html", ParentId = "run")]
    public class RunHTMLCommand : BaseCommand
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
        ///     <para>Runs the HTML</para>
        /// </summary>
        public void Execute()
        {
            CmfPackage htmlPackage = GenericUtilities.SelectPackage(fileSystem, packageType: PackageType.HTML);

            // Build HTML
            new GulpCommand()
            {
                GulpFile = "gulpfile.js",
                Task = "build",
                DisplayName = "Gulp Build",
                GulpJS = "node_modules/gulp/bin/gulp.js",
                WorkingDirectory = htmlPackage.GetFileInfo().Directory
            }.Exec();

            // Start and Watch HTML (allows Rebuild on changes)
            new GulpCommand()
            {
                GulpFile = "gulpfile.js",
                Task = "start watch",
                DisplayName = "Gulp Start Watch",
                GulpJS = "node_modules/gulp/bin/gulp.js",
                WorkingDirectory = htmlPackage.GetFileInfo().Directory
            }.Exec();
        }
    }
}
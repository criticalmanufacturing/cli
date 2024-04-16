using Cmf.CLI.Builders;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Utilities;
using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO.Abstractions;

namespace Cmf.CLI.Commands.Generate
{
    /// <summary>
    ///     GenerateDocumentationCommand
    /// </summary>
    [CmfCommand("documentation", Id = "generate_documentation", ParentId = "generate")]
    public class GenerateDocumentationCommand : BaseCommand
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
            IDirectoryInfo helpDirectory = GenericUtilities.SelectPackage(fileSystem, packageType: PackageType.Help).GetFileInfo().Directory;

            Environment.CurrentDirectory = helpDirectory.FullName;

            new GenerateBasedOnTemplatesCommand().Execute();
            new GenerateMenuItemsCommand().Execute();

            new GulpCommand()
            {
                GulpFile = "gulpfile.js",
                Task = "build",
                DisplayName = "Build Help",
                GulpJS = "node_modules/gulp/bin/gulp.js",
                Args = new[] { "--production" },
                WorkingDirectory = helpDirectory
            }.Exec();
        }
    }
}
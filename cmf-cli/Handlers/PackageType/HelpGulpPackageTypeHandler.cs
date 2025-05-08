using Cmf.CLI.Builders;
using Cmf.CLI.Commands;
using Cmf.CLI.Commands.restore;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;


namespace Cmf.CLI.Handlers
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="PresentationPackageTypeHandler" />
    public class HelpGulpPackageTypeHandler : PresentationPackageTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HelpGulpPackageTypeHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage"></param>
        public HelpGulpPackageTypeHandler(CmfPackage cmfPackage) : base(cmfPackage)
        {
            cmfPackage.SetDefaultValues
            (
                targetDirectory:
                    "UI/Help",
                targetLayer:
                    "help"
            );

            BuildSteps = new IBuildCommand[]
            {
                new ExecuteCommand<RestoreCommand>()
                {
                    Command = new RestoreCommand(),
                    DisplayName = "cmf restore",
                    Execute = command =>
                    {
                        command.Execute(cmfPackage.GetFileInfo().Directory, null);
                    }
                },
                new NPMCommand()
                {
                    DisplayName = "NPM Install",
                    Command  = "install",
                    Args = new []{ "--force" },
                    WorkingDirectory = cmfPackage.GetFileInfo().Directory
                },
                new GulpCommand()
                {
                    GulpFile = "gulpfile.js",
                    Task = "install",
                    DisplayName = "Gulp Install",
                    GulpJS = "node_modules/gulp/bin/gulp.js",
                    Args = new [] { "--update" },
                    WorkingDirectory = cmfPackage.GetFileInfo().Directory
                },
                // generate based on templates
                new ExecuteCommand<GenerateBasedOnTemplatesCommand>()
                {
                    DisplayName = "Generate help pages based on templates",
                    Command = new GenerateBasedOnTemplatesCommand(),
                    Execute = command =>
                    {
                        command.Execute(cmfPackage.GetFileInfo().Directory);
                    }
                },
                // generate menu items
                new ExecuteCommand<GenerateMenuItemsCommand>()
                {
                    DisplayName = "Generate menu items",
                    Command = new GenerateMenuItemsCommand(),
                    Execute = command =>
                    {
                        command.Execute(cmfPackage.GetFileInfo().Directory);
                    }
                },
                new GulpCommand()
                {
                    GulpFile = "gulpfile.js",
                    Task = "build",
                    DisplayName = "Gulp Build",
                    GulpJS = "node_modules/gulp/bin/gulp.js",
                    Args = new [] { "--production" },
                    WorkingDirectory = cmfPackage.GetFileInfo().Directory
                },
            };

            cmfPackage.DFPackageType = PackageType.Presentation;
        }
    }
}
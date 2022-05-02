using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using Cmf.CLI.Core.Objects;

namespace Cmf.CLI.Builders
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="ProcessCommand" />
    /// <seealso cref="IBuildCommand" />
    public class DotnetCommand : ProcessCommand, IBuildCommand
    {
        /*
         *  - task: DotNetCoreCLI@2
              displayName: 'Build Business Solution'
              inputs:
                command: 'build'
                projects: 'Business/**//*.csproj'
                    arguments: '--configuration $(BuildConfiguration) --output $(Build.BinariesDirectory)/Business'
         */

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public string Command { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the arguments.
        /// </summary>
        /// <value>
        /// The arguments.
        /// </value>
        public string[] Args { get; set; }

        /// <summary>
        /// Gets or sets the nu get configuration.
        /// </summary>
        /// <value>
        /// The nu get configuration.
        /// </value>
        public IFileInfo NuGetConfig { get; set; }

        /// <summary>
        /// Gets or sets the solution.
        /// </summary>
        /// <value>
        /// The solution.
        /// </value>
        public IFileInfo Solution { get; set; }

        /// <summary>
        /// Gets or sets the output directory.
        /// </summary>
        /// <value>
        /// The output directory.
        /// </value>
        public IDirectoryInfo OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public string Configuration { get; set; }

        /// <summary>
        /// Gets the steps.
        /// </summary>
        /// <returns></returns>
        public override ProcessBuildStep[] GetSteps()
        {
            // /usr/bin/dotnet restore /home/vsts/work/1/s/Business/Cmf.Custom.Actions/Cmf.Custom.DevOps.Actions.csproj --configfile /home/vsts/work/1/Nuget/tempNuGet_101.config --verbosity Detailed
            // /usr/bin/dotnet build /home/vsts/work/1/s/Business/Cmf.Custom.Actions/Cmf.Custom.DevOps.Actions.csproj -dl:CentralLogger,"/home/vsts/work/_tasks/DotNetCoreCLI_5541a522-603c-47ad-91fc-a4b1d163081b/2.181.0/dotnet-build-helpers/Microsoft.TeamFoundation.DistributedTask.MSBuild.Logger.dll"*ForwardingLogger,"/home/vsts/work/_tasks/DotNetCoreCLI_5541a522-603c-47ad-91fc-a4b1d163081b/2.181.0/dotnet-build-helpers/Microsoft.TeamFoundation.DistributedTask.MSBuild.Logger.dll" --configuration release --output /home/vsts/work/1/b/Business
            var args = new List<string>
            {
                this.Command,
                this.Solution.FullName
            };
            if (this.NuGetConfig != null)
            {
                args.AddRange(new[] { "--configfile", this.NuGetConfig.FullName });
            }

            if (this.Configuration != null)
            {
                args.AddRange(new[] { "--configuration", this.Configuration });
            }

            if (this.OutputDirectory != null)
            {
                args.AddRange(new[] { "--output", this.OutputDirectory.FullName });
            }

            if (this.Args != null)
            {
                args.AddRange(this.Args);
            }
            // args.AddRange(new []{"--verbosity", "Detailed"}); // TODO: make this a command option
            return new[]
            {
                new ProcessBuildStep()
                {
                    Command = "dotnet",
                    Args = args.ToArray(),
                    WorkingDirectory = this.WorkingDirectory
                }
            };
        }
    }
}
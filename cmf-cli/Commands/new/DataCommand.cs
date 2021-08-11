using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using System.Text.Json;
using Cmf.Common.Cli.Attributes;
using Cmf.Common.Cli.Builders;

namespace Cmf.Common.Cli.Commands.New
{
    /// <summary>
    /// Generates Data package structure
    /// </summary>
    [CmfCommand("data", Parent = "new")]
    public class DataCommand : LayerTemplateCommand
    {
        /// <inheritdoc />
        public DataCommand() : base("data", "Cmf.Custom.Data")
        {
        }

        /// <inheritdoc />
        public DataCommand(IFileSystem fileSystem) : base("data", "Cmf.Custom.Data", fileSystem)
        {
        }

        /// <inheritdoc />
        public override void Configure(Command cmd)
        {
            base.GetBaseCommandConfig(cmd);
            cmd.AddOption(new Option<IDirectoryInfo>(
                aliases: new[] { "--businessPackage" },
                parseArgument: argResult => Parse<IDirectoryInfo>(argResult),
                isDefault: true,
                description: "Business package where the Process Rules project should be built"
            ));
            cmd.Handler = CommandHandler.Create<IDirectoryInfo, string, IDirectoryInfo>(this.Execute);
        }

        /// <inheritdoc />
        protected override List<string> GenerateArgs(IDirectoryInfo projectRoot, IDirectoryInfo workingDir, List<string> args, JsonDocument projectConfig)
        {
            var relativePathToRoot =
                this.fileSystem.Path.Join("..", //always one level deeper
                    this.fileSystem.Path.GetRelativePath(
                        workingDir.FullName,
                        projectRoot.FullName)
                ).Replace("\\", "/");
            
            args.AddRange(new []
            {
                "--rootRelativePath", relativePathToRoot 
            });

            return args;
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="workingDir">the nearest root package</param>
        /// <param name="version">the package version</param>
        /// <param name="businessPackage">business package where the process rules should be built.</param>
        public void Execute(IDirectoryInfo workingDir, string version, IDirectoryInfo businessPackage)
        {
            if (businessPackage != null && !businessPackage.Exists)
            {
                Log.Error($"Target business package does not exist");
                return;
            }
            
            base.Execute(workingDir, version);
            if (businessPackage != null)
            {
                // add the new project to the business solution
                var businessSlnPath = this.fileSystem.Path.Join(businessPackage.FullName, "Business.sln");
                if (this.fileSystem.File.Exists(businessSlnPath))
                {
                    var nameIdx = Array.FindIndex(base.executedArgs, item => string.Equals(item, "--name"));
                    var tenantIdx = Array.FindIndex(base.executedArgs, item => string.Equals(item, "--Tenant"));
                    var csproj = this.fileSystem.Path.Join(
                        workingDir.FullName, 
                        base.executedArgs[nameIdx + 1],
                        this.fileSystem.Path.Join(
                            "DEEs",
                            $"Cmf.Custom.{base.executedArgs[tenantIdx + 1]}.Actions.csproj"));
                    if (!this.fileSystem.File.Exists(csproj))
                    {
                        Log.Warning($"Tried to inject project {csproj} into solution {businessSlnPath} but failed, project does not exist.");
                        return;
                    }
                    // run dotnet sln add
                    // dotnet sln <sln> add <dataPkg>/DEEs/Cmf.Custom.<tenant>.Actions.csproj
                    var slnAddCmd = new DotnetCommand()
                    {
                        Command = "sln",
                        Solution = this.fileSystem.FileInfo.FromFileName(businessSlnPath),
                        Args = new[]
                        {
                            "add",
                            csproj
                        },
                        WorkingDirectory = workingDir
                    };

                    Log.Information($"Adding new project {csproj} to solution {businessSlnPath}");
                    slnAddCmd.Exec();
                }
            }
        }
        
    }
}
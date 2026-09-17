using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cmf.CLI.Builders;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Handlers
{
    /// <summary>
    /// Handler for MkDocs-based Help packages.
    /// </summary>
    public class HelpMkDocsPackageTypeHandler : PackageTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HelpMkDocsPackageTypeHandler" /> class.
        /// </summary>
        public HelpMkDocsPackageTypeHandler(CmfPackage cmfPackage) : base(cmfPackage)
        {
            var packageDirectory = CmfPackage.GetFileInfo().DirectoryName;

            // MkDocs build must run where requirements.txt is located.
            var requirementsFile = this.fileSystem.Directory
                .GetFiles(packageDirectory, "requirements.txt", SearchOption.AllDirectories)
                .FirstOrDefault();

            if (requirementsFile == null)
            {
                throw new CliException($"Could not find requirements.txt under {packageDirectory}. Cannot build Help package.");
            }

            var workingDirectory = this.fileSystem.DirectoryInfo.New(this.fileSystem.Path.GetDirectoryName(requirementsFile));

            BuildSteps = new IBuildCommand[]
            {
                new PythonCommand
                {
                    DisplayName = "Create Python virtual environment",
                    Module = "venv",
                    Args = new[] { ".venv" },
                    WorkingDirectory = workingDirectory
                },
                new PythonCommand
                {
                    DisplayName = "Install MkDocs",
                    Module = "pip",
                    Args = new[] { "install", "mkdocs" },
                    VirtualEnvironment = ".venv",
                    EnvironmentVariables = new Dictionary<string, string>
                    {
                        ["REQUESTS_CA_BUNDLE"] = null
                    },
                    WorkingDirectory = workingDirectory
                },
                new PythonCommand
                {
                    DisplayName = "Install Python requirements",
                    Module = "pip",
                    Args = new[] { "install", "-r", "requirements.txt" },
                    VirtualEnvironment = ".venv",
                    EnvironmentVariables = new Dictionary<string, string>
                    {
                        ["REQUESTS_CA_BUNDLE"] = null
                    },
                    WorkingDirectory = workingDirectory
                },
                new MkDocsCommand
                {
                    DisplayName = "MkDocs build",
                    Command = "build",
                    VirtualEnvironment = ".venv",
                    WorkingDirectory = workingDirectory
                }
            };
        }
    }
}

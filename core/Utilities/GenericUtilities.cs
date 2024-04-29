using Cmf.CLI.Core;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Principal;

namespace Cmf.CLI.Utilities
{
    /// <summary>
    /// </summary>
    public static class GenericUtilities
    {
        #region Public Methods

        /// <summary>
        ///     Will create a new version based on the old and new inputs
        /// </summary>
        /// <param name="currentVersion">
        /// </param>
        /// <param name="version">
        /// </param>
        /// <param name="buildNr">
        /// </param>
        /// <returns>
        ///     the new version
        /// </returns>
        public static string RetrieveNewVersion(string currentVersion, string version, string buildNr)
        {
            if (!string.IsNullOrEmpty(version))
            {
                currentVersion = version;
            }
            if (!string.IsNullOrEmpty(buildNr))
            {
                currentVersion += "-" + buildNr;
            }

            return currentVersion;
        }

        /// <summary>
        ///     Will create a new version based on the old and new inputs
        /// </summary>
        /// <param name="currentVersion">
        /// </param>
        /// <param name="version">
        /// </param>
        /// <param name="buildNr">
        /// </param>
        /// <returns>
        ///     the new version
        /// </returns>
        public static string RetrieveNewPresentationVersion(string currentVersion, string version, string buildNr)
        {
            GenericUtilities.GetCurrentPresentationVersion(currentVersion, out string originalVersion, out string originalBuildNumber);

            string newVersion = !string.IsNullOrEmpty(version) ? version : originalVersion;
            if (!string.IsNullOrEmpty(buildNr))
            {
                newVersion += "-" + buildNr;
            }

            return newVersion;
        }

        /// <summary>
        ///     Get current version based on string, for the format 1.0.0-1234 where 1.0.0 will be the version and the 1234 will be the build number
        /// </summary>
        /// <param name="source">
        ///     Source information to be parsed
        /// </param>
        /// <param name="version">
        ///     Version Number
        /// </param>
        /// <param name="buildNr">
        ///     Build Number
        /// </param>
        public static void GetCurrentPresentationVersion(string source, out string version, out string buildNr)
        {
            version = string.Empty;
            buildNr = string.Empty;

            if (!string.IsNullOrWhiteSpace(source))
            {
                string[] sourceInfo = source.Split('-');
                version = sourceInfo[0];
                if (sourceInfo.Length > 1)
                {
                    buildNr = sourceInfo[1];
                }
            }
        }

        /// <summary>
        ///     Get Package from Repository
        /// </summary>
        /// <param name="outputDir">
        ///     Target directory for the package
        /// </param>
        /// <param name="repoUri">
        ///     Repository Uri
        /// </param>
        /// <param name="force">
        /// </param>
        /// <param name="packageId">
        ///     Package Identifier
        /// </param>
        /// <param name="packageVersion">
        ///     Package Version
        /// </param>
        /// <param name="fileSystem">
        ///     the underlying file system
        /// </param>
        /// <returns>
        /// </returns>
        public static bool GetPackageFromRepository(IDirectoryInfo outputDir, Uri repoUri, bool force, string packageId, string packageVersion, IFileSystem fileSystem)
        {
            bool packageFound = false;

            // TODO: Support for nexus repository

            if (repoUri != null)
            {
                // If other repository types are supported they will be added here.

                if (repoUri.IsDirectory())
                {
                    // Create expected file name for the package to get
                    string _packageFileName = $"{packageId}.{packageVersion}.zip";
                    IDirectoryInfo repoDirectory = fileSystem.DirectoryInfo.New(repoUri.OriginalString);

                    if (repoDirectory.Exists)
                    {
                        // Search by Packages already Packed
                        IFileInfo[] dependencyFiles = repoDirectory.GetFiles(_packageFileName);
                        packageFound = dependencyFiles.HasAny();

                        if (packageFound)
                        {
                            foreach (IFileInfo dependencyFile in dependencyFiles)
                            {
                                string destDependencyFile = $"{outputDir.FullName}/{dependencyFile.Name}";
                                if (force && fileSystem.File.Exists(destDependencyFile))
                                {
                                    fileSystem.File.Delete(destDependencyFile);
                                }

                                dependencyFile.CopyTo(destDependencyFile);
                            }
                        }
                    }
                }
                else
                {
                    throw new CliException(CoreMessages.UrlsNotSupported);
                }
            }

            return packageFound;
        }

        /// <summary>
        ///     Flatten a tree
        /// </summary>
        /// <param name="items">
        ///     The top level tree items
        /// </param>
        /// <param name="getChildren">
        ///     a function that for each tree node returns its children
        /// </param>
        /// <typeparam name="T">
        ///     The tree node type
        /// </typeparam>
        /// <returns>
        /// </returns>
        public static IEnumerable<T> Flatten<T>(
            this IEnumerable<T> items,
            Func<T, IEnumerable<T>> getChildren)
        {
            var stack = new Stack<T>();
            foreach (var item in items)
                stack.Push(item);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current;

                var children = getChildren(current);
                if (children == null) continue;

                foreach (var child in children)
                    stack.Push(child);
            }
        }

        /// <summary>
        ///     Converts a JsonObject to an Uri
        /// </summary>
        /// <param name="value">
        /// </param>
        /// <returns>
        /// </returns>
#nullable enable

        public static Uri? JsonObjectToUri(dynamic value)
        {
            return string.IsNullOrEmpty(value?.Value) ? null : new Uri(value?.Value);
        }

        /// <summary>
        ///     Builds a tree representation of a CmfPackage dependency tree
        /// </summary>
        /// <param name="pkg">
        ///     the root package
        /// </param>
        public static Tree BuildTree(CmfPackage pkg)
        {
            var tree = new Tree($"{pkg.PackageId}@{pkg.Version} [[{pkg.Location.ToString()}]]");
            if (pkg.Dependencies.HasAny())
            {
                for (int i = 0; i < pkg.Dependencies.Count; i++)
                {
                    Dependency dep = pkg.Dependencies[i];

                    if (!dep.IsMissing)
                    {
                        var curNode = tree.AddNode($"{dep.CmfPackage.PackageId}@{dep.CmfPackage.Version} [[{dep.CmfPackage.Location.ToString()}]]");
                        BuildTreeNodes(dep.CmfPackage, curNode);
                    }
                    else if (dep.IsMissing)
                    {
                        if (dep.Mandatory)
                        {
                            tree.AddNode($"[red]MISSING {dep.Id}@{dep.Version}[/]");
                        }
                        else
                        {
                            tree.AddNode($"[yellow]MISSING {dep.Id}@{dep.Version}[/]");
                        }
                    }
                }
            }
            return tree;
        }

        /// <summary>
        /// </summary>
        /// <param name="envVarName">
        /// </param>
        /// <returns>
        /// </returns>
        public static bool IsEnvVarTruthy(string envVarName)
        {
            var enableConsoleExporter = System.Environment.GetEnvironmentVariable(envVarName);
            return enableConsoleExporter is "1" or "true" or "TRUE" or "True";
        }

        /// <summary>
        ///     Gets all packages of a specified type and allows the user to choose from multiple packages if more than one is found.
        /// </summary>
        /// <param name="fileSystem">
        ///     FileSystem
        /// </param>
        /// <param name="packageType">
        ///     <see cref="PackageType"/> to select
        /// </param>
        /// <returns>
        ///     Selected <see cref="CmfPackage"/>
        /// </returns>
        public static CmfPackage SelectPackage(IFileSystem fileSystem, PackageType packageType)
        {
            CmfPackageCollection packages = new CmfPackageCollection();

            Log.Status($"Loading {packageType} CmfPackages", (_) =>
            {
                IDirectoryInfo projectRoot = FileSystemUtilities.GetProjectRoot(fileSystem);

                // Get Packages of given Type
                packages = projectRoot.LoadCmfPackagesFromSubDirectories(packageType);
            });

            int packageIndex = 0;

            // If there are more than 1 package
            if (packages.Count > 1)
            {
                Log.Verbose($"Multiple {packageType} Packages found.");
                Log.Verbose("Select the Package:");
                int index = 0;
                foreach (CmfPackage package in packages)
                {
                    Log.Verbose($"{index + 1} - {package.PackageId}");
                    index++;
                }
                int option = ReadIntValueFromConsole(prompt: "Option:", minValue: 1, maxValue: packages.Count);
                packageIndex = option - 1;
            }

            return packages[packageIndex];
        }

        /// <summary>
        ///     Clears the console screen and removes all previous output.
        /// </summary>
        public static void FullConsoleClear()
        {
            Console.Clear();
            Console.WriteLine("\x1b[3J");
            Console.Clear();
        }

        /// <summary>
        ///     Reads a value of type T from the console until the value is valid (not null or of type T).
        /// </summary>
        /// <param name="prompt">
        ///     Message or question that will be displayed to the user when they are prompted to enter a value in the console.
        /// </param>
        /// <returns>
        ///     Value of type `T` that is read from the console input after converting it from a string. If the input is null, the method recursively calls
        ///     itself until a non-null input is provided. If the input cannot be converted to type `T` due to a <see cref="FormatException"/>, an error message
        ///     is logged, and the method recursively calls itself in order to read a new value from the console.
        /// </returns>
        public static T? ReadValueFromConsole<T>(string prompt, bool allowEmptyValueInput = false)
        {
            Console.Write($"  {prompt} ");
            string? input = Console.ReadLine();

            try
            {
                // If empty value is received but not allowed
                if (!allowEmptyValueInput && input?.Length == 0)
                {
                    throw new CliException();
                }
                else if (allowEmptyValueInput && input?.Length == 0)
                {
                    return default;
                }

                return (T)Convert.ChangeType(input, typeof(T));
            }
            catch (Exception ex) when (ex is CliException || ex is FormatException)
            {
                Log.Error("Invalid input. Try again.");
                return ReadValueFromConsole<T>(prompt); // Recursively call until valid input
            }
        }

        /// <summary>
        ///     Reads an integer value from the console with optional minimum and maximum value constraints.
        /// </summary>
        /// <param name="prompt">
        ///     Message or question that will be displayed to the user when they are prompted to enter a value in the console.
        /// </param>
        /// <param name="minValue">
        ///     Minimum value that the user input must be greater than or equal to in order to be considered valid. If a `minValue` is not specified, the user
        ///     input will not be validated in this scenario.
        /// </param>
        /// <param name="maxValue">
        ///     Maximum value that the user input must be less than or equal to in order to be considered valid. If a `maxValue` is not specified, the user
        ///     input will not be validated in this scenario.
        /// </param>
        /// <returns>
        ///     An integer value that is read from the console input, after validating it against optional minimum and maximum values.
        /// </returns>
        public static int ReadIntValueFromConsole(string prompt, int? minValue = null, int? maxValue = null, bool allowEmptyValueInput = false)
        {
            string errorMessage = string.Empty;
            int value = ReadValueFromConsole<int>(prompt: prompt, allowEmptyValueInput: allowEmptyValueInput);

            if (allowEmptyValueInput && value == 0)
            {
                return value;
            }

            bool hasMinAndMaxValue = !minValue.IsNullOrEmpty() && !maxValue.IsNullOrEmpty();
            bool hasMinValue = !minValue.IsNullOrEmpty() && maxValue.IsNullOrEmpty();
            bool hasMaxValue = minValue.IsNullOrEmpty() && !maxValue.IsNullOrEmpty();

            // If we have a `minValue` and the `value` is greater than or equal to the `minValue`
            bool isValueGraterThanOrEqualToMinValue = !minValue.IsNullOrEmpty() && value >= minValue;
            // If we have a `maxValue` and the `value` is less than or equal to the `maxValue`
            bool isValueLessThanOrEqualToMaxValue = !maxValue.IsNullOrEmpty() && value <= maxValue;

            if (hasMinAndMaxValue && isValueGraterThanOrEqualToMinValue && isValueLessThanOrEqualToMaxValue)
            {
                return value;
            }
            else if (hasMinValue && isValueGraterThanOrEqualToMinValue)
            {
                return value;
            }
            else if (hasMaxValue && isValueLessThanOrEqualToMaxValue)
            {
                return value;
            }
            else if (!hasMinAndMaxValue)
            {
                return value;
            }

            // Build error messages
            if (hasMinAndMaxValue)
            {
                errorMessage = $"The value must be between {minValue} and {maxValue}. Try again.";
            }
            else if (hasMinValue)
            {
                errorMessage = $"The value must be grater than or equal to {minValue}. Try again.";
            }
            else if (hasMaxValue)
            {
                errorMessage = $"The value must be less than or equal to {maxValue}. Try again.";
            }

            Log.Error(errorMessage);
            return ReadIntValueFromConsole(prompt, minValue, maxValue);
        }

        /// <summary>
        ///     Reads a string value from the console with validation for allowed values and empty input.
        /// </summary>
        /// <param name="prompt">
        ///     Message or question that will be displayed to the user when they are prompted to enter a value in the console.
        /// </param>
        /// <param name="allowEmptyValueInput">
        ///     Determines whether an empty value input is allowed or not. If set to `true`, the user can input an empty string as a value. If set to `false`,
        ///     the user must provide a non-empty string.
        /// </param>
        /// <returns>
        ///     A string value that is read from the console input.
        /// </returns>
        public static string? ReadStringValueFromConsole(string prompt, bool allowEmptyValueInput = false, params string[] allowedValues)
        {
            string errorMessage = string.Empty;
            string? value = ReadValueFromConsole<string>(prompt: prompt, allowEmptyValueInput: allowEmptyValueInput);

            bool isValueAllowed = allowedValues.Length > 0 && allowedValues.Contains(value, StringComparer.OrdinalIgnoreCase);
            bool isEmptyAndAllowedEmptyValue = allowEmptyValueInput && string.IsNullOrEmpty(value);

            if (
                (allowedValues.Length == 0 && isEmptyAndAllowedEmptyValue)
                || (allowedValues.Length > 0 && isEmptyAndAllowedEmptyValue)
                || allowedValues.Length == 0
                || isValueAllowed
                || (isValueAllowed && isEmptyAndAllowedEmptyValue)
            )
            {
                return value;
            }

            if (!isValueAllowed)
            {
                errorMessage = "The value is not allowed. Try again.";
            }

            Log.Error(errorMessage);
            return ReadStringValueFromConsole(prompt, allowEmptyValueInput, allowedValues);
        }

        /// <summary>
        ///     Determines the appropriate PowerShell executable based on the operating system platform.
        /// </summary>
        /// <returns>
        ///     <para>Path to the PowerShell executable based on the operating system.</para>
        ///     <para>On Windows, it checks if `pwsh.exe` exists in the default path, and if not, it returns `powershell.exe`.</para>
        ///     <para>On Unix-like systems (Linux, macOS), it returns `/usr/bin/pwsh`.</para>
        /// </returns>
        public static string GetPowerShellExecutable()
        {
            // Check if pwsh is installed on Windows
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "PowerShell", "7", "pwsh.exe")))
                {
                    return "pwsh.exe";
                }
                else
                {
                    return "powershell.exe";
                }
            }
            else // Assume Unix-like systems (Linux, macOS)
            {
                return "/usr/bin/pwsh";
            }
        }

        /// <summary>
        ///     <para>Executes a PowerShell command with specified arguments and outputs the result.</para>
        ///     <para>
        ///         This function was created because, for some reason, the "CmdCommand" doesn't always work as expected. E.g.: when trying to open a tab in
        ///         WindowsTerminal, it opens 2 tabs, one of those not executing any command.
        ///     </para>
        /// </summary>
        /// <param name="command">
        ///     PowerShell command to execute.
        /// </param>
        /// <param name="arguments">
        ///     OPTIONAL. Arguments to add to the comment.
        /// </param>
        public static void ExecutePowerShellCommand(string command, params string[] arguments)
        {
            try
            {
                // Create a ProcessStartInfo object to configure the process to start
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = GetPowerShellExecutable(),
                    Arguments = $"-NoProfile -ExecutionPolicy unrestricted -Command \"{command} {string.Join(' ', arguments)}\";",
                    UseShellExecute = false, // Don't use the system shell to execute the command
                    RedirectStandardOutput = true // Redirect the standard output of the command
                };

                // Create and start the process
                Process process = new Process
                {
                    StartInfo = psi
                };
                process.Start();

                // Read the output of the command and display it
                string output = process.StandardOutput.ReadToEnd();
                Console.WriteLine(output);

                // Wait for the process to exit
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error executing PowerShell command: " + ex.Message);
            }
        }

        /// <summary>
        ///     Checks if Windows Terminal is installed and throws an exception if it is not.
        /// </summary>
        public static void EnsureWindowsTerminalIsInstalled()
        {
            if (!File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "WindowsApps", "wt.exe")))
            {
                throw new CliException("Windows Terminal is not installed");
            }
        }

        /// <summary>
        ///     Checks if the CLI is running as an administrator on a Windows platform and throws an exception if it is not.
        /// </summary>
        public static void EnsureIsRunningAsAdmin()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT && !new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                throw new CliException("CLI not running as Administrator.");
            }
        }

        /// <summary>
        ///     Write DB Name to "CurrentDB" file
        /// </summary>
        /// <param name="fileSystem">
        ///     FileSystem
        /// </param>
        /// <param name="dbToSet">
        ///     Name of the database to be set as the current database.
        /// </param>
        public static void SetCurrentDb(IFileSystem fileSystem, string dbToSet)
        {
            File.WriteAllText(fileSystem.Path.Combine(FileSystemUtilities.GetProjectRoot(fileSystem).FullName, "LocalEnvironment", "DBDumps", "CurrentDB"), dbToSet);
        }

        /// <summary>
        ///     Gets the DB Name from "CurrentDB" file
        /// </summary>
        /// <param name="fileSystem">
        ///     FileSystem
        /// </param>
        /// <returns>
        ///     Current DB Name
        /// </returns>
        public static string GetCurrentDb(IFileSystem fileSystem)
        {
            string curentDbFilePath = fileSystem.Path.Combine(FileSystemUtilities.GetProjectRoot(fileSystem).FullName, "LocalEnvironment", "DBDumps", "CurrentDB");

            if (!File.Exists(curentDbFilePath))
            {
                SetCurrentDb(fileSystem, $"{ExecutionContext.Instance.ProjectConfig.EnvironmentName}");
                return $"{ExecutionContext.Instance.ProjectConfig.EnvironmentName}";
            }
            else
            {
                return File.ReadAllText(curentDbFilePath);
            }
        }

        /// <summary>
        ///     Gets existing/available DB Backups from `DBDumps` folder
        /// </summary>
        /// <param name="fileSystem">
        ///     FileSystem
        /// </param>
        /// <returns>
        ///     File paths to backup files
        /// </returns>
        public static string[] GetBdBackups(IFileSystem fileSystem)
        {
            string dbDumpsDirectoryPath = fileSystem.Path.Combine(FileSystemUtilities.GetProjectRoot(fileSystem).FullName, "LocalEnvironment", "DBDumps");

            return Directory.GetFiles(dbDumpsDirectoryPath, "*.bak").Where(file => Path.GetFileNameWithoutExtension(file).StartsWith("Custom-")).ToArray();
        }

        /// <summary>
        ///     Executes a SQL command
        /// </summary>
        /// <param name="connectionString">
        ///     String that contains information needed to establish a connection to a database.
        ///     <code>
        ///Data Source=*IP*,*PORT*;Initial Catalog=master;User ID=*USERNAME*;Password=*PASSWORD*
        ///     </code>
        /// </param>
        /// <param name="sqlCommand">
        ///     SQL query or command that you want to execute
        /// </param>
        public static void ExecuteSqlCommand(string connectionString, string sqlCommand)
        {
            // Creating a SqlConnection object to connect to the database
            using SqlConnection connection = new SqlConnection(connectionString);

            // Opening the connection
            connection.Open();

            // Creating a SqlCommand object with the SQL command and SqlConnection
            using SqlCommand command = new SqlCommand(sqlCommand, connection);

            // Executing the SQL command
            command.ExecuteNonQuery();
        }

        #endregion Public Methods

        #region Private Methods

        private static void BuildTreeNodes(CmfPackage pkg, TreeNode node)
        {
            if (pkg.Dependencies.HasAny())
            {
                for (int i = 0; i < pkg.Dependencies.Count; i++)
                {
                    Dependency dep = pkg.Dependencies[i];

                    if (!dep.IsMissing)
                    {
                        var curNode = node.AddNode($"{dep.CmfPackage.PackageId}@{dep.CmfPackage.Version} [[{dep.CmfPackage.Location.ToString()}]]");
                        BuildTreeNodes(dep.CmfPackage, curNode);
                    }
                    else if (dep.IsMissing)
                    {
                        if (dep.Mandatory)
                        {
                            node.AddNode($"[red]MISSING {dep.Id}@{dep.Version}[/]");
                        }
                        else
                        {
                            node.AddNode($"[yellow]MISSING {dep.Id}@{dep.Version}[/]");
                        }
                    }
                }
            }
        }

        #endregion Private Methods
    }
}
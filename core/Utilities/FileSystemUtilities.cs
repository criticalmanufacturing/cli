using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Objects;

namespace Cmf.CLI.Utilities
{
    /// <summary>
    ///
    /// </summary>
    public static class FileSystemUtilities
    {
        #region Public Methods

        /// <summary>
        /// Gets the files to pack.
        /// </summary>
        /// <param name="contentToPack">The content to pack.</param>
        /// <param name="sourceDirName">Name of the source dir.</param>
        /// <param name="destDirName">Name of the dest dir.</param>
        /// <param name="contentToIgnore">The content to ignore.</param>
        /// <param name="copySubDirs">if set to <c>true</c> [copy sub dirs].</param>
        /// <param name="isCopyDependencies">if set to <c>true</c> [is copy dependencies].</param>
        /// <param name="filesToPack">The files to pack.</param>
        /// <param name="fileSystem">the underlying file system</param>
        /// <returns></returns>
        /// <exception cref="DirectoryNotFoundException">$"Source directory does not exist or could not be found: {sourceDirName}</exception>
        public static List<FileToPack> GetFilesToPack(ContentToPack contentToPack, string sourceDirName, string destDirName, IFileSystem fileSystem, List<string> contentToIgnore = null, bool copySubDirs = true, bool isCopyDependencies = false, List<FileToPack> filesToPack = null)
        {
            if (filesToPack == null)
            {
                filesToPack = new();
            }

            // Get the subdirectories for the specified directory.
            IDirectoryInfo dir = fileSystem.DirectoryInfo.New(sourceDirName);

            if (!dir.Exists)
            {
                if (!isCopyDependencies)
                {
                    throw new DirectoryNotFoundException($"Source directory does not exist or could not be found: {sourceDirName}");
                }
                else
                {
                    return new();
                }
            }

            // skip if is to ignore folder
            if (contentToIgnore.Has(dir.Name))
            {
                return new();
            }

            IDirectoryInfo[] dirs = dir.GetDirectories();

            // Get the files in the directory and copy them to the new location.
            IFileInfo[] files = dir.GetFiles();
            foreach (IFileInfo file in files)
            {
                if (contentToIgnore.Has(file.Name))
                {
                    continue;
                }

                string tempPath = fileSystem.Path.Combine(destDirName, file.Name);

                filesToPack.Add(new()
                {
                    ContentToPack = contentToPack,
                    Source = file,
                    Target = fileSystem.FileInfo.New(tempPath)
                });
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (IDirectoryInfo subdir in dirs)
                {
                    if (contentToIgnore.Has(subdir.Name))
                    {
                        continue;
                    }

                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    GetFilesToPack(contentToPack, subdir.FullName, tempPath, fileSystem, contentToIgnore, copySubDirs, isCopyDependencies, filesToPack);
                }
            }

            return filesToPack;
        }

        /// <summary>
        /// Directories copy.
        /// </summary>
        /// <param name="sourceDirName">Name of the source dir.</param>
        /// <param name="destDirName">Name of the dest dir.</param>
        /// <param name="contentToIgnore">The exclusions.</param>
        /// <param name="copySubDirs">if set to <c>true</c> [copy sub dirs].</param>
        /// <param name="isCopyDependencies">if set to <c>true</c> [is copy dependencies].</param>
        /// <param name="fileSystem">the underlying file system</param>
        /// <exception cref="DirectoryNotFoundException">Source directory does not exist or could not be found: "
        /// + sourceDirName</exception>
        public static void CopyDirectory(string sourceDirName, string destDirName, IFileSystem fileSystem, List<string> contentToIgnore = null, bool copySubDirs = true, bool isCopyDependencies = false)
        {
            // Get the subdirectories for the specified directory.
            IDirectoryInfo dir = fileSystem.DirectoryInfo.New(sourceDirName);

            if (!dir.Exists)
            {
                if (!isCopyDependencies)
                {
                    throw new DirectoryNotFoundException($"Source directory does not exist or could not be found: {sourceDirName}");
                }
                else
                {
                    return;
                }
            }

            // skip if is to ignore folder
            if (contentToIgnore.Has(dir.Name)) return;

            IDirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.
            fileSystem.Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            IFileInfo[] files = dir.GetFiles();
            foreach (IFileInfo file in files)
            {
                string _fileName = isCopyDependencies && file.Extension.Equals(".dep") ? file.Name.Replace(".dep", "") : file.Name;

                if (contentToIgnore.Has(_fileName)) continue;

                string tempPath = fileSystem.Path.Combine(destDirName, _fileName);
                file.CopyTo(tempPath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (IDirectoryInfo subdir in dirs)
                {
                    if (contentToIgnore.Has(subdir.Name)) continue;

                    string tempPath = fileSystem.Path.Combine(destDirName, subdir.Name);
                    CopyDirectory(subdir.FullName, tempPath, fileSystem, contentToIgnore, copySubDirs, isCopyDependencies);
                }
            }
        }

        /// <summary>
        /// Reads to string.
        /// </summary>
        /// <param name="fi">The fi.</param>
        /// <returns></returns>
        public static string ReadToString(this IFileInfo fi)
        {
            //open for read operation
            Stream fsToRead = fi.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

            //get the StreamReader
            StreamReader sr = new(fsToRead);
            //read all texts using StreamReader object
            string fileContent = sr.ReadToEnd();
            sr.Close();

            //close Stream objects
            fsToRead.Close();

            return fileContent;
        }

        /// <summary>
        /// Reads to string list.
        /// </summary>
        /// <param name="fi">The fi.</param>
        /// <returns></returns>
        public static List<string> ReadToStringList(this IFileInfo fi)
        {
            List<string> result = new();

            if (fi != null)
            {
                //open for read operation
                FileStream fsToRead = (FileStream)fi.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

                //get the StreamReader
                StreamReader sr = new(fsToRead);
                //read all texts using StreamReader object
                string fileContent = sr.ReadToEnd();
                sr.Close();

                //close Stream objects
                fsToRead.Close();

                string[] lines = fileContent.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    result.Add(line);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the package root.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="CliException">Cannot find package root. Are you in a valid package directory?</exception>
        public static IDirectoryInfo GetPackageRoot(IFileSystem fileSystem, string workingDir = null)
        {
            var cwd = fileSystem.DirectoryInfo.New(workingDir ?? fileSystem.Directory.GetCurrentDirectory());
            var cur = cwd;
            while (cur != null && !fileSystem.File.Exists(fileSystem.Path.Join(cur.FullName, CoreConstants.CmfPackageFileName)))
            {
                cur = cur.Parent;
            }

            return cur;
        }

        /// <summary>
        /// Gets the project root.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="CliException">Cannot find project root. Are you in a valid project directory?</exception>
        /// <exception cref="CliException">Cannot find project root. Are you in a valid project directory?</exception>
        public static IDirectoryInfo GetProjectRoot(IFileSystem fileSystem, bool throwException = false)
        {
            var cwd = fileSystem.DirectoryInfo.New(fileSystem.Directory.GetCurrentDirectory());
            var cur = cwd;
            while (cur != null && !fileSystem.File.Exists(fileSystem.Path.Join(cur.FullName, CoreConstants.ProjectConfigFileName)))
            {
                cur = cur.Parent;
            }

            if (cur == null && throwException)
            {
                throw new CliException(CoreMessages.PackageRootNotFound);
            }

            return cur;
        }

        /// <summary>
        /// Gets the package root of type package root.
        /// </summary>
        /// <param name="directoryName">The current working directory</param>
        /// <param name="packageType">Type of the package.</param>
        /// <param name="fileSystem">the underlying file system</param>
        /// <returns></returns>
        /// <exception cref="CliException">Cannot find project root. Are you in a valid project directory?</exception>
        public static IDirectoryInfo GetPackageRootByType(string directoryName, PackageType packageType, IFileSystem fileSystem)
        {
            var cwd = fileSystem.DirectoryInfo.New(directoryName);
            var cur = cwd;
            while (cur != null)
            {
                if (fileSystem.File.Exists(fileSystem.Path.Join(cur.FullName, CoreConstants.CmfPackageFileName)))
                {
                    IFileInfo cmfpackageFile = fileSystem.FileInfo.New(Path.Join(cur.FullName, CoreConstants.CmfPackageFileName));
                    CmfPackage cmfPackage = CmfPackage.Load(cmfpackageFile);

                    if (cmfPackage.PackageType == packageType)
                    {
                        break;
                    }
                }
                cur = cur.Parent;
            }

            return cur;
        }

        /// <summary>
        /// Reads the environment configuration.
        /// </summary>
        /// <param name="envConfigName">Name of the env configuration.</param>
        /// <param name="fileSystem">the underlying file system</param>
        /// <returns></returns>
        public static JsonDocument ReadEnvironmentConfig(string envConfigName, IFileSystem fileSystem)
        {
            if (!envConfigName.EndsWith(".json"))
            {
                envConfigName += ".json";
            }

            envConfigName = envConfigName.Contains(fileSystem.Path.PathSeparator)
                ? envConfigName
                : fileSystem.Path.Join(GetProjectRoot(fileSystem).FullName, "EnvironmentConfigs", envConfigName);

            var json = fileSystem.File.ReadAllText(envConfigName);

            return JsonDocument.Parse(json);
        }

        /// <summary>
        /// Reads the project configuration.
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use ExecutionContext.Instance.ProjectConfig")]
        public static JsonDocument ReadProjectConfig(IFileSystem fileSystem)
        {
            Log.Debug("Loading .project-config.json (legacy mode)");
            var projectCfg = fileSystem.Path.Join(GetProjectRoot(fileSystem)?.FullName, CoreConstants.ProjectConfigFileName);
            var json = fileSystem.File.ReadAllText(projectCfg);
            Log.Debug("Loaded .project-config.json (legacy mode)");
            return JsonDocument.Parse(json);
        }

        /// <summary>
        /// Read DF repositories config from filesystem
        /// </summary>
        /// <param name="fileSystem">The filesystem object</param>
        /// <returns>a RepositoriesConfig object</returns>
        public static RepositoriesConfig ReadRepositoriesConfig(IFileSystem fileSystem)
        {
            var cwd = fileSystem.DirectoryInfo.New(fileSystem.Directory.GetCurrentDirectory());
            var cur = cwd;
            string repoConfigPath = null;
            var repoConfig = new RepositoriesConfig();
            while (cur != null)
            {
                if (fileSystem.File.Exists(fileSystem.Path.Join(cur.FullName, CoreConstants.RepositoriesConfigFileName)))
                {
                    repoConfigPath = fileSystem.Path.Join(cur.FullName, CoreConstants.RepositoriesConfigFileName);
                    break;
                }
                if (fileSystem.File.Exists(fileSystem.Path.Join(cur.FullName, CoreConstants.ProjectConfigFileName)))
                {
                    // we're at the repository root, quit
                    break;
                }
                cur = cur.Parent;
            }

            if (repoConfigPath != null)
            {
                var json = fileSystem.File.ReadAllText(repoConfigPath);
                repoConfig = JsonConvert.DeserializeObject<RepositoriesConfig>(json);
            }

            return repoConfig;
        }

        /// <summary>
        /// Copies the contents of input to output. Doesn't close either stream.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

        /// <summary>
        /// Copies the install dependencies.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        /// <param name="packageType">Type of the package.</param>
        /// <param name="fileSystem">the underlying file system</param>
        public static void CopyInstallDependenciesFiles(IDirectoryInfo packageOutputDir, PackageType packageType, IFileSystem fileSystem)
        {
            string sourceDirectory = fileSystem.Path.Join(AppDomain.CurrentDomain.BaseDirectory, CoreConstants.FolderInstallDependencies, packageType.ToString());
            CopyDirectory(sourceDirectory, packageOutputDir.FullName, fileSystem, isCopyDependencies: true);
        }

        /// <summary>
        /// Gets the output dir.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        /// <param name="outputDir">The output dir.</param>
        /// <param name="force">if set to <c>true</c> [force].</param>
        /// <returns></returns>
        public static IDirectoryInfo GetOutputDir(CmfPackage cmfPackage, IDirectoryInfo outputDir, bool force)
        {
            // Create OutputDir
            if (!outputDir.Exists)
            {
                Log.Information($"Creating {outputDir.Name} folder");
                outputDir.Create();
            }
            else
            {
                if (outputDir.GetFiles(cmfPackage.ZipPackageName).HasAny())
                {
                    if (force)
                    {
                        outputDir.GetFiles(cmfPackage.ZipPackageName)[0].Delete();
                    }
                    else
                    {
                        Log.Information($"Skipping {cmfPackage.ZipPackageName}. Already packed in Output Directory.");
                        return null;
                    }
                }
            }

            return outputDir;
        }

        /// <summary>
        /// Gets the package output dir.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        /// <param name="packageDirectory">The package directory.</param>
        /// <param name="fileSystem">the underlying file system</param>
        /// <returns></returns>
        public static IDirectoryInfo GetPackageOutputDir(CmfPackage cmfPackage, IDirectoryInfo packageDirectory, IFileSystem fileSystem)
        {
            // Clear and Create packageOutputDir
            IDirectoryInfo packageOutputDir = fileSystem.DirectoryInfo.New($"{packageDirectory}/{cmfPackage.PackageName}");
            if (packageOutputDir.Exists)
            {
                packageOutputDir.Delete(true);
                packageOutputDir.Refresh();
            }

            Log.Debug($"Generating output folder {cmfPackage.PackageName}");
            packageOutputDir.Create();

            return packageOutputDir;
        }

        /// <summary>
        /// Get Manifest File From package
        /// </summary>
        /// <param name="packageFile"></param>
        /// <returns></returns>
        public static XDocument GetManifestFromPackage(string packageFile, IFileSystem fileSystem = null)
        {
            fileSystem ??= new FileSystem();
            XDocument dFManifest = null;

            using (var zipToOpen = fileSystem.FileStream.New(packageFile, FileMode.Open))
            {
                using (ZipArchive zip = new(zipToOpen, ZipArchiveMode.Read))
                {
                    var manifest = zip.GetEntry(CoreConstants.DeploymentFrameworkManifestFileName);
                    if (manifest != null)
                    {
                        using var stream = manifest.Open();
                        using var reader = new StreamReader(stream);
                        XmlDocument contentXml = new XmlDocument();
                        contentXml.Load(reader);
                        dFManifest = XDocument.Parse(contentXml.OuterXml);
                    }
                }
            }

            return dFManifest;
        }

        /// <summary>
        /// Get File Content From package
        /// </summary>
        /// <param name="packageFile"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string GetFileContentFromPackage(string packageFile, string filename)
        {
            using (FileStream zipToOpen = new FileInfo(packageFile).OpenRead())
            {
                using (ZipArchive zip = new(zipToOpen, ZipArchiveMode.Read))
                {
                    var manifest = zip.GetEntry(filename);
                    if (manifest != null)
                    {
                        using var stream = manifest.Open();
                        using var reader = new StreamReader(stream);
                        return reader.ReadToEnd();
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Create a Zip file at filePath from the directory content
        /// </summary>
        /// <param name="fileSystem">The file system implementation</param>
        /// <param name="filePath">The path of the resulting zip file</param>
        /// <param name="directory">The directory to zip</param>
        /// <returns></returns>
        public static void ZipDirectory(IFileSystem fileSystem, string filePath, IDirectoryInfo directory)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (ZipArchive zipArchive = new(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (IFileInfo file in directory.AllFilesAndFolders().Where(o => o is IFileInfo).Cast<IFileInfo>())
                    {
                        var relPath = file.FullName.Substring(directory.FullName.Length + 1).Replace("\\", "/");
                        var archive = zipArchive.CreateEntry(relPath);

                        using (var entryStream = archive.Open())
                        {
                            entryStream.Write(fileSystem.File.ReadAllBytes(file.FullName));
                        }
                    }
                }

                using (Stream zipToOpen = fileSystem.FileStream.New(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 0x1000, useAsync: false))
                {
                    memoryStream.Position = 0;
                    memoryStream.WriteTo(zipToOpen);
                    memoryStream.Flush();
                }
            }
        }

        /// <summary>
        /// Get Directory from a FileSystem dependending if Uri is UNC
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IDirectoryInfo GetDirectory(this Uri uri)
        {
            string path = uri.IsUnc ? uri.OriginalString : uri.LocalPath;

            return ExecutionContext.Instance.FileSystem.DirectoryInfo.New(path);
        }

        /// <summary>
        /// Get Directory Name from a FileSystem dependending if Uri is UNC
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string GetDirectoryName(this Uri uri)
        {
            return GetDirectory(uri).FullName;
        }

        /// <summary>
        /// Get File from a FileSystem dependending if Uri is UNC
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IFileInfo GetFile(this Uri uri)
        {
            string path = uri.IsUnc ? uri.OriginalString : uri.LocalPath;

            return ExecutionContext.Instance.FileSystem.FileInfo.New(path);
        }

        /// <summary>
        /// Get Directory Name from a FileSystem dependending if Uri is UNC
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string GetFileName(this Uri uri)
        {
            return GetFile(uri).FullName;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get all files and folders from a directory
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        private static IEnumerable<IFileSystemInfo> AllFilesAndFolders(this IDirectoryInfo dir)
        {
            foreach (var f in dir.GetFiles())
                yield return f;
            foreach (var d in dir.GetDirectories())
            {
                yield return d;
                foreach (var o in AllFilesAndFolders(d))
                    yield return o;
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Services;
using Cmf.CLI.Utilities;
using Core.Objects;

namespace Cmf.CLI.Core.Repository;

public class CIFSRepositoryClient : ICIFSRepositoryClient
{
    private ICIFSClient client = null;
    private Uri root = null;

    public CIFSRepositoryClient(string rootPath, IFileSystem fileSystem)
    {
        this.root = new Uri(rootPath, UriKind.Absolute);
        var host = this.root.Host;
        this.client = new CIFSClient(host, [this.root]);
    }
    
    public Task<CmfPackageV1> Find(string packageId, string version)
    {
        string dependencyFileName = $"{packageId}.{version}.*";
        return GetFromRepository(dependencyFileName, true);
    }

    public Task<CmfPackageV1Collection> List()
    {
        throw new NotSupportedException("Cannot list packages from CIFS share!");
    }

    public Task Put(CmfPackageV1 package)
    {
        throw new NotSupportedException("Cannot publish packages to CIFS share!");
    }

    public Task<IFileInfo> Get(CmfPackageV1 package, IDirectoryInfo targetDirectory)
    {
        var targetFile =
            targetDirectory.FileSystem.FileInfo.New(
                targetDirectory.FileSystem.Path.Join(targetDirectory.FullName,
                    $"{package.PackageId}.{package.Version}.zip"));
        
        string dependencyFileName = $"{package.PackageId}.{package.Version}.*";
        this.DownloadFile(dependencyFileName, targetFile);
        return Task.FromResult(targetFile);
    }

    public async Task<IDirectoryInfo> Extract(CmfPackageV1 package, IDirectoryInfo targetDirectory)
    {
        IDirectoryInfo depPkgDir = targetDirectory;
        var fileSystem = targetDirectory.FileSystem;
        var pkgFile = await this.Get(package, fileSystem.DirectoryInfo.New(fileSystem.Path.GetTempPath()));
        using (Stream zipToOpen = pkgFile.OpenRead())
        {
            using (ZipArchive zip = new(zipToOpen, ZipArchiveMode.Read))
            {
                // these tuples allow us to rewrite entry paths
                var entriesToExtract = new List<Tuple<ZipArchiveEntry, string>>();
                entriesToExtract.AddRange(zip.Entries.Select(entry => new Tuple<ZipArchiveEntry, string>(entry, entry.FullName)));
    
                foreach (var entry in entriesToExtract)
                {
                    var target = fileSystem.Path.Join(targetDirectory.FullName, entry.Item2);
                    var targetDir = fileSystem.Path.GetDirectoryName(target);
                    if (target.EndsWith("/"))
                    {
                        // this a dotnet bug: if a folder contains a ., the library assumes it's a file and adds it as an entry
                        // however, afterwards all folder contents are separate entries, so we can just skip these
                        continue;
                    }
    
                    if (!fileSystem.File.Exists(target)) // TODO: support overwriting if requested
                    {
                        var overwrite = false;
                        Log.Debug($"Extracting {entry.Item1.FullName} to {target}");
                        if (!string.IsNullOrEmpty(targetDir))
                        {
                            fileSystem.Directory.CreateDirectory(targetDir);
                        }
    
                        entry.Item1.ExtractToFile(target, overwrite, fileSystem);
                    }
                    else
                    {
                        Log.Debug($"Skipping {target}, file exists");
                    }
                }
            }
        }

        return depPkgDir;
    }

    private Stream GetFileStream(string file)
    {
        var (_, stream) = this.client?.SharedFolders?.FirstOrDefault(sf => sf.Exists)?.GetFile(file) ?? new Tuple<Uri, Stream>(null, null);
        return stream;
    }
    
    private Task<CmfPackageV1> GetFromRepository(string dependencyFileName, bool fromManifest)
    {
        var stream = this.GetFileStream(dependencyFileName);
        if(stream != null)
        {                   
            if(fromManifest)
            {
                using (ZipArchive zip = new(stream, ZipArchiveMode.Read))
                {
                    var manifest = zip.GetEntry(CoreConstants.DeploymentFrameworkManifestFileName);
                    if (manifest != null)
                    {
                        using var manifStream = manifest.Open();
                        using var reader = new StreamReader(manifStream);
                        var pkg = CmfPackageController.FromXml(XDocument.Parse(reader.ReadToEnd()));
                        return Task.FromResult(pkg);
                    }
                }
            }
            else
            {
                // cmfPackage = new CmfPackage(packageId, version, file.Item1);
                // cmfPackage.SharedFolder = share;
            }
        }
        return null;
    }

    private IFileInfo DownloadFile(string file, IFileInfo output)
    {
        if (!output.Exists)
        {
            output.Create();
        }
        using var fileStream = output.OpenWrite();
        var stream = this.GetFileStream(file);
        Log.Debug("Saving to temp file");
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        // Stream the content directly into the file
        stream.CopyTo(fileStream);
        Log.Debug($"Saving to file {output.FullName} finished, took {stopwatch.ElapsedMilliseconds}ms");
        return output;
    }

    public string RepositoryRoot => this.root?.OriginalString;
    public bool Unreacheable => this.client?.IsConnected ?? false;
}
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Services;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Core.Repository;

public class ArchiveRepositoryClient : ICIFSRepositoryClient
{
    private IDirectoryInfo? root;
    private IFileInfo? file;

    public ArchiveRepositoryClient(string rootPath) : this(rootPath, new FileSystem())
    {
    }
    
    public ArchiveRepositoryClient(string rootPath, IFileSystem fileSystem)
    {
        root = fileSystem.DirectoryInfo.New(rootPath.Replace("file:", ""));
        var checkFile = fileSystem.FileInfo.New(rootPath.Replace("file:", ""));
        if (!root.Exists && checkFile.Exists)
        {
            file = checkFile;
            root = checkFile.Directory;
        }
    }
    
    public async Task<CmfPackageV1?> Find(string packageId, string version)
    {
        ArgumentNullException.ThrowIfNull(version);

        string dependencyFileName = $"{packageId}.{version}.*";
        return (await GetPackages(dependencyFileName)).FirstOrDefault();
    }

    public async Task<CmfPackageV1Collection> List()
    {
        return await GetPackages("*");
    }

    public Task Put(CmfPackageV1 package)
    {
        var r = root ?? throw new InvalidOperationException("Repository root is not set.");
        var targetFilePath = r.FileSystem.Path.Join(r.FullName, $"{package.PackageDotRef}.zip");
        // using var fileStream = r.FileSystem.FileInfo.New(filePath).Create();
        // package.Stream.Seek(0, SeekOrigin.Begin);
        // package.Stream.CopyTo(fileStream);
        var originFile = (package.Client ?? throw new CliException($"Package {package.PackageAtRef} has no associated client")).Get(package, r);
        return Task.CompletedTask;
    }

    public Task<IFileInfo?> Get(CmfPackageV1 package, IDirectoryInfo targetDirectory)
    {
        var files = this.Unreacheable ? [] : (this.file != null ? [file] : root?.GetFiles($"{package.PackageDotRef}.*", SearchOption.TopDirectoryOnly));
        return Task.FromResult(files?.Where(f => f.Extension is ".tgz" or ".zip").FirstOrDefault());
    }

    public async Task<IDirectoryInfo> Extract(CmfPackageV1 package, IDirectoryInfo targetDirectory)
    {
        IDirectoryInfo? depPkgDir = null;
        var fileSystem = targetDirectory.FileSystem;
        var pkgFile = await this.Get(package, fileSystem.DirectoryInfo.New(fileSystem.Path.GetTempPath()))
            ?? throw new CliException($"Could not find package {package.PackageAtRef}");
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
                            depPkgDir = fileSystem.Directory.CreateDirectory(targetDir);
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

        return depPkgDir ?? targetDirectory;
    }

    public string RepositoryRoot => this.root?.FullName ?? string.Empty;
    public bool Unreacheable => !(this.root?.Exists ?? false);

    private Task<CmfPackageV1Collection> GetPackages(string dependencyFileName)
    {
        CmfPackageV1Collection cmfPackages = [];
        
        var files = this.Unreacheable ? [] : (this.file != null ? [file] : root?.GetFiles(dependencyFileName));

        foreach (var file in files ?? [])
        {
            var ctrlr = new CmfPackageController(file);
            ctrlr.CmfPackage.Client = this;
            cmfPackages.Add(ctrlr.CmfPackage);
        }
        return Task.FromResult(cmfPackages);
    }
}
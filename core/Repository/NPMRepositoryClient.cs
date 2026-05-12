using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Services;
using Cmf.CLI.Utilities;

namespace Cmf.CLI.Core.Repository;

public class NPMRepositoryClient : IRepositoryClient
{
    private IFileSystem fileSystem;
    private Uri registryUrl;
    private INPMClientEx client;
    public NPMRepositoryClient(string registryUrl, IFileSystem fileSystem = null, INPMClientEx client = null)
    {
        this.registryUrl = new Uri(registryUrl);
        this.client = client ?? new NPMClient(registryUrl);
        this.fileSystem = fileSystem ?? new FileSystem();
    }

    public async Task<CmfPackageV1> Find(string packageId, string version)
    {
        try
        {
            var pkg = await client.FetchPackageVersion(packageId.ToLowerInvariant(), version);

            if (pkg != null)
            {
                pkg.Client = this;
            }

            return pkg;
        }
        catch (Exception ex)
        {
            // TODO Should we remove this catch? The handling for "not finding" a package seems to
            //      be done by returning null. Handling any other failures (network, etc...)
            //      should be done higher in the code, not hidden from the user, maybe?
            Log.Debug($"Error finding package {packageId}@{version}: {ex.Message}");
            return null;
        }
    }

    public async Task<CmfPackageV1Collection> List()
    {
        // not all feeds support searching for keywords
        var pkgs = await client.SearchPackages("keywords:cmf-deployment-package");
        return new CmfPackageV1Collection();
        // throw new System.NotImplementedException();
    }

    public async Task Put(CmfPackageV1 package)
    {
        var toLowerCase = true; // TODO: complete implementation
        var addManifestVersion = true; // TODO: complete implementation

        var tmp = this.fileSystem.DirectoryInfo.New(this.fileSystem.Path.GetTempPath());
        Log.Debug($"Downloading package {package.PackageAtRef} to {tmp.FullName}...");
        var file = await package.Client.Get(package, tmp);
        Log.Debug("Done!");
        if (file.Extension == ".zip")
        {
            var zipFile = file;
            file = this.fileSystem.FileInfo.New(this.fileSystem.Path.ChangeExtension(zipFile.FullName, "tgz"));
            Log.Debug($"Package at {zipFile.FullName} is in Zip format, converting to tgz at {file.FullName}...");
            CmfPackageController.ConvertZipToTarGz(zipFile, file, toLowerCase, addManifestVersion);
            Log.Debug("Done!");
        }
        else if (file.Extension == ".json")
        {
            // TODO: pack
            throw new CliException("Please pack before publishing and publish the packed file.");
        }
        Log.Debug("Publishing via NPMClient...");
        await this.client.PublishPackage(file);
        Log.Debug("Publishing via NPMClient completed!");
    }

    public async Task<IFileInfo> Get(CmfPackageV1 package, IDirectoryInfo targetDirectory)
    {
        var tmp = targetDirectory.FileSystem.Path.Combine(targetDirectory.FileSystem.Path.GetTempPath(), targetDirectory.FileSystem.Path.GetRandomFileName()).Replace(".tmp", ".tgz");
        var targetFile =
            targetDirectory.FileSystem.FileInfo.New(
                targetDirectory.FileSystem.Path.Join(targetDirectory.FullName,
                $"{package.PackageId}.{package.Version}.zip"));
        var tgzPkg = await client.DownloadPackage(package.PackageId.ToLowerInvariant(), package.Version, targetDirectory.FileSystem.FileInfo.New(tmp));
        if (tgzPkg == null)
        {
            throw new CliException($"Could not find package {package.PackageAtRef} at {registryUrl.OriginalString}");
        }
        CmfPackageController.ConvertTarGzToZip(tgzPkg, targetFile);
        return targetFile;
    }

    public async Task<IDirectoryInfo> Extract(CmfPackageV1 package, IDirectoryInfo targetDirectory)
    {
        IDirectoryInfo depPkgDir = targetDirectory;
        var fileSystem = targetDirectory.FileSystem;
        var pkgFile = await this.Get(package, fileSystem.DirectoryInfo.New(fileSystem.Path.GetTempPath()));
        // TODO: extract to utility
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

    public string RepositoryRoot => this.registryUrl.AbsoluteUri;
    public bool Unreacheable => this.client == null;
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using TarWriter = System.Formats.Tar.TarWriter;

namespace Cmf.CLI.Core.Services;

public class CmfPackageController
{
    
    private const string NPMAliasPrefix = "npm:"; 
        
    private static List<CmfPackageV1> loadedPackages = new();
    private CmfPackageV1 package;
    private IFileSystem fileSystem;

    public CmfPackageV1 CmfPackage => this.package; 
    
    public CmfPackageController(CmfPackageV1 package, IFileSystem fileSystem)
    {
        this.package = package;
        this.fileSystem = fileSystem;
    }
    public CmfPackageController(IFileInfo file, IFileSystem fileSystem = null, bool setDefaultValues = false)
    {
        Log.Debug("Spinning up a controller for a CmfPackage file info: " + file.FullName);
        #if DEBUG
        // TODO: this is dumb
        var stackTrace = new StackTrace();
        var callingMethod = stackTrace.GetFrame(1).GetMethod(); // Get the calling method
        var callerType = callingMethod.DeclaringType; // Get the caller's type

        if (callerType.GetInterface(nameof(IRepositoryClient)) == null)
        {
            Log.Warning("This constructor can only be invoked from RepositoryClients!");
        }
        #endif
        
        this.fileSystem = fileSystem ?? file.FileSystem;
        if (!file.Exists)
        {
            throw new CliException(string.Format(CoreMessages.NotFound, file.FullName));
        }

        if (file.Extension == ".json")
        {
            Log.Debug("File is a source package");
            // source package
            var cmfPackage = CmfPackageController.FromSourceManifest(file);
            cmfPackage.Client = ExecutionContext.ServiceProvider.GetService<IRepositoryLocator>()
                .GetRepositoryClient(new Uri(file.FullName), file.FileSystem); // this is a hack to avoid awaiting for the LocalRepositoryClient as it is async
            // string fileContent = file.ReadToString();
            // CmfPackage cmfPackage = JsonConvert.DeserializeObject<CmfPackage>(fileContent);
            cmfPackage.IsToSetDefaultValues = setDefaultValues;
            // cmfPackage.FileInfo = file;
            // cmfPackage.Location = PackageLocation.Local;
            // cmfPackage.fileSystem = fileSystem;

            // TODO: restore this
            // cmfPackage.RelatedPackages?.Load(cmfPackage);

            this.package = cmfPackage;
        }
        else if (file.Extension == ".zip")
        {
            Log.Debug("File is a DF package in ZIP format");
            using Stream zipToOpen = file.OpenRead();
            using ZipArchive zip = new(zipToOpen, ZipArchiveMode.Read);
            var manifest = zip.GetEntry(CoreConstants.DeploymentFrameworkManifestFileName);
            if (manifest != null)
            {
                using var stream = manifest.Open();
                using var reader = new StreamReader(stream);
                // TODO: make sure this is ok
                // this.package = CmfPackageController.FromXmlManifest(reader.ReadToEnd(), setDefaultValues: true);
                this.package = CmfPackageController.FromXml(XDocument.Parse(reader.ReadToEnd()));
                // cmfPackage.Stream = zipToOpen;
                // if (cmfPackage != null)
                // {
                //     cmfPackage.Uri = new(dependencyFile.FullName);
                // }
            }
            else
            {
                var jsonManifest = zip.GetEntry(CoreConstants.PackageJson);
                if (jsonManifest == null)
                {
                    throw new CliException($"Zip file {file.FullName} does not contain a valid manifest!");
                }

                using var stream = jsonManifest.Open();
                using var reader = new StreamReader(stream);
                // TODO: make sure this is ok
                // this.package = CmfPackageController.FromXmlManifest(reader.ReadToEnd(), setDefaultValues: true);
                this.package = CmfPackageController.FromJson(reader.ReadToEnd());
            }
        }
        else if (file.Extension is ".gz" or ".tgz")
        {
            Log.Debug("File is a DF package in TAR.GZ format");
            using Stream zipToOpen = file.OpenRead();
            using GZipStream gzipStream = new GZipStream(zipToOpen, CompressionMode.Decompress);
            var foundManifest = false;

            using var tarReader = SharpCompress.Readers.Tar.TarReader.Open(gzipStream, new SharpCompress.Readers.ReaderOptions { });
            while (tarReader.MoveToNextEntry())
            {
                var entry = tarReader.Entry;

                // Check if this is the file you're looking for
                if ((entry.Key == CoreConstants.DeploymentFrameworkManifestFileName || entry.Key == $"package/{CoreConstants.DeploymentFrameworkManifestFileName}") && !entry.IsDirectory)
                {
                    foundManifest = true;
                    // Read the content of the file inside the TAR
                    using var reader = new StreamReader(tarReader.OpenEntryStream());

                    // TODO: make sure this is ok
                    // this.package = CmfPackageController.FromXmlManifest(reader.ReadToEnd(), setDefaultValues: true);
                    this.package = CmfPackageController.FromXml(XDocument.Parse(reader.ReadToEnd()));
                    break;
                }
            }

            if (!foundManifest)
            {
                using Stream zipToOpen2 = file.OpenRead();
                using GZipStream gzipStream2 = new GZipStream(zipToOpen2, CompressionMode.Decompress);
                using var tarReader2 = SharpCompress.Readers.Tar.TarReader.Open(gzipStream2, new SharpCompress.Readers.ReaderOptions { });

                while (tarReader2.MoveToNextEntry())
                {
                    var entry = tarReader2.Entry;

                    // Check if this is the file you're looking for
                    if ((entry.Key == CoreConstants.PackageJson || entry.Key == $"package/{CoreConstants.PackageJson}" ) && !entry.IsDirectory)
                    {
                        foundManifest = true;
                        // Read the content of the file inside the TAR
                        using var reader = new StreamReader(tarReader2.OpenEntryStream());
                   
                        // TODO: make sure this is ok
                        // this.package = CmfPackageController.FromXmlManifest(reader.ReadToEnd(), setDefaultValues: true);
                        this.package = CmfPackageController.FromJson(reader.ReadToEnd());
                        break;
                    }
                }
            }

            if (!foundManifest)
            {
                throw new CliException($"Tgz file {file.FullName} does not contain a valid manifest!");
            }
        }

        
    }
    
    public async Task LoadDependencies(IEnumerable<Uri> repoUris, StatusContext ctx, bool recurse = false)
    {
        using var activity = ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity("CmfPackageController LoadDependencies");
        activity?.SetTag("cmfPackage", $"{package.PackageId}.{package.Version}");
        loadedPackages.Add(package);
        ctx?.Status($"Working on {package.Name ?? (package.PackageId + "@" + package.Version)}");
        
        if (package.Dependencies.HasAny())
        {
            // IDirectoryInfo[] repoDirectories = repoUris?.Select(r => r.GetDirectory()).ToArray();
            // var missingRepoDirectories = repoDirectories?.Where(r => r.Exists == false).ToArray();
            // if (missingRepoDirectories.HasAny())
            // {
            //     throw new CliException($"Some of the provided repositories do not exist: {string.Join(", ", missingRepoDirectories.Select(d => d.FullName))}");
            // }
            foreach (var dependency in package.Dependencies)
            {
                ctx?.Status($"Working on dependency {dependency.Id}@{dependency.Version}");
                Log.Debug($"Working on dependency {dependency.Id}@{dependency.Version}");

                #region Get Dependencies from Dependencies Directory

                // 1) check if we have found this package before
                var dependencyPackage = loadedPackages.FirstOrDefault(x => x.PackageId.IgnoreCaseEquals(dependency.Id) && x.Version.IgnoreCaseEquals(dependency.Version));

                // 2) check if package is in repository
                if (dependencyPackage == null)
                {
                    // dependencyPackage = LoadFromRepo(repoDirectories, dependency.Id, dependency.Version);
                    dependencyPackage = await ExecutionContext.ServiceProvider.GetService<IRepositoryLocator>()
                        .FindPackage(dependency.Id, dependency.Version);
                }

                // 3) search in the source code repository (only if this is a local package)
                if (dependencyPackage == null && (package.SourceManifestFile?.Exists ?? false))
                {
                    // dependencyPackage = FileInfo.Directory.LoadCmfPackagesFromSubDirectories(setDefaultValues: true).GetDependency(dependency);
                    // if (dependencyPackage != null)
                    // {
                    //     dependencyPackage.Uri = new Uri(dependencyPackage.FileInfo.FullName);
                    // }
                    dependencyPackage = await ExecutionContext.ServiceProvider.GetService<IRepositoryLocator>().GetSourceClient(this.fileSystem)
                        .Find(dependency.Id, dependency.Version);
                }

                if (dependencyPackage != null)
                {
                    loadedPackages.Add(dependencyPackage);
                    dependency.CmfPackageV1 = dependencyPackage;
                    if (recurse)
                    {
                        var ctrlr = new CmfPackageController(dependencyPackage, fileSystem);
                        await ctrlr.LoadDependencies(repoUris, ctx, recurse);
                    }
                }
            }

            #endregion Get Dependencies from Dependencies Directory
        }
    }
    
    
    
    private static CmfPackageV1 FromXmlManifest(string manifest, bool setDefaultValues = false)
    {
        StringReader dFManifestReader = new(manifest);
        XDocument dFManifestTemplate = XDocument.Load(dFManifestReader);
        var tokens = new Dictionary<string, string>();

        XElement rootNode = dFManifestTemplate.Element("deploymentPackage", true);
        if (rootNode == null)
        {
            throw new CliException(string.Format(CoreMessages.InvalidManifestFile));
        }
        DependencyCollection deps = new();
        DependencyCollection testPackages = new();
        foreach (XElement element in rootNode.Elements())
        {
            // Get the Property Value based on the Token name
            string token = element.Value.Trim();

            if (element.Name.LocalName == "dependencies")
            {
                var deplist = element.Elements().Select(depEl => new Dependency(depEl.Attribute("id").Value, depEl.Attribute("version").Value));
                deps.AddRange(deplist);
            }

            if (element.Name.LocalName == "testPackages")
            {
                var testPackagesList = element.Elements().Select(depEl => new Dependency(depEl.Attribute("id").Value, depEl.Attribute("version").Value));
                testPackages.AddRange(testPackagesList);
            }

            if (string.IsNullOrEmpty(token))
            {
                continue;
            }

            tokens.Add(element.Name.LocalName.ToLowerInvariant(), token);
        }

        PackageType cliPackageType = PackageType.Generic;
        if (tokens.ContainsKey("clipackagetype"))
        {
            Enum.TryParse(tokens["clipackagetype"], out cliPackageType);
        }

        // NOTE: we're extracting only the essentials here for `cmf ls` but we can get extra data from the manifests
        var cmfPackage = new CmfPackageV1(
            tokens.ContainsKey("name") ? tokens["name"] : null,
            tokens["packageid"],
            tokens["version"],
            tokens.ContainsKey("description") ? tokens["description"] : null,
            cliPackageType,
            "",
            "",
            false,
            false,
            tokens.ContainsKey("keywords") ? tokens["keywords"] : null,
            true,
            deps,
            null,
            null,
            null,
            waitForIntegrationEntries: false,
            testPackages
            );

        // cmfPackage.Location = PackageLocation.Repository;
        // cmfPackage.fileSystem = fileSystem;

        return cmfPackage;
    }
    
    /// <summary>
        /// Froms the XML.
        /// </summary>
        /// <param name="xml">The XML.</param>
        /// <returns></returns>
        /// <exception cref="PackageReadingException">
        /// </exception>
        public static CmfPackageV1 FromXml(XDocument xml)
        {
            // NOTE: We don't use an automatic serializer because we want full control
            // on how the file is parsed in order to be able to version it

            var rootNode = xml.Element("deploymentPackage", true);
            if (rootNode == null)
            {
                throw new CliException("Invalid manifest");
            }

            // if (rootNode.Element("systemName", true) != null)
            // {
            //     package.AddMetadata(MetadataKey.ApplicationName, rootNode.Element("systemName", true).Value);
            //
            //     var targetSystemVersionText = rootNode.Element("systemVersion", true)?.Value;
            //     if (!string.IsNullOrWhiteSpace(targetSystemVersionText))
            //     {
            //         package.AddMetadata(MetadataKey.ApplicationVersion, targetSystemVersionText);
            //     }
            // }

            PackageType cliPackageType = PackageType.Generic;
            Enum.TryParse(rootNode.Element("clipackagetype", true)?.Value, out cliPackageType);

            var steps = new List<Step>();
            var stepsElements = rootNode.Element("steps", true)?.Elements("step", true);
            if (stepsElements != null)
            {
                foreach (var element in stepsElements)
                {
                    Step step = new Step(
                        type: Enum.Parse(typeof(StepType), element.Attribute("type")?.Value) is StepType
                            ? (StepType)Enum.Parse(typeof(StepType), element.Attribute("type")?.Value)
                            : StepType.Generic,
                        title: element.Attribute("title")?.Value,
                        onExecute: element.Attribute("onExecute")?.Value,
                        contentPath: element.Attribute("contentPath")?.Value,
                        file: null,
                        tagFile: element.Attribute("tagFile")?.Value != null ? bool.Parse(element.Attribute("tagFile")?.Value) : null,
                        targetDatabase: element.Attribute("targetDatabase")?.Value,
                        messageType: MessageType.ImportObject, // TODO: get value
                        relativePath: null,
                        filePath: element.Attribute("filePath")?.Value
                    );
                    
                    // // Create an XmlSerializer for the Person type
                    // XmlSerializer serializer = new XmlSerializer(typeof(Step));
                    //
                    // // Use StringReader to read the XML string
                    // using var reader = element.CreateReader();
                    // // Deserialize the XML string into a Person object
                    // Step s = (Step)serializer.Deserialize(reader);
                    steps.Add(step);
                }
            }

            // #region PackageDemands Parsing
            //
            // var packageDemands = rootNode.Element("packageDemands", true)?.Elements("packageDemand", true);
            // if (packageDemands != null)
            // {
            //     foreach (var item in packageDemands)
            //     {
            //         PackageDemand packageDemand = ParseDemand(item);
            //
            //         package.AddDemand(packageDemand);
            //     }
            // }
            //
            // #endregion

            // var variables = rootNode.Element("variables", true)?.Elements("variable", true);
            // if (variables != null)
            // {
            //     foreach (var element in variables)
            //     {
            //         var variable = ParseVariable(element);
            //         package.AddVariable(variable);
            //     }
            // }

            DependencyCollection deps = new();
            DependencyCollection testPackages = new();
            var dependenciesElements = rootNode.Element("dependencies", true)?.Elements("dependency", true);
            if (dependenciesElements != null)
            {
                foreach (var dependenciesElement in dependenciesElements)
                {
                    var id = dependenciesElement.Attribute("id")?.Value;
                    var versionRange = dependenciesElement.Attribute("version")?.Value;
                    var mandatory = dependenciesElement.Attribute("mandatory");
                    var conditional = dependenciesElement.Attribute("conditional");

                    bool isMandatory = false;
                    bool isConditional = false;

                    if (mandatory != null)
                    {
                        isMandatory = (bool)mandatory;
                    }

                    if (conditional != null)
                    {
                        isConditional = (bool)conditional;
                    }

                    Dependency result = null;

                    if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(versionRange))
                    {
                        result = new Dependency(id, versionRange) { Mandatory = isMandatory/*, isConditional*/};
                    }
                    // else if (!string.IsNullOrWhiteSpace(id))
                    // {
                    //     result = new Dependency(id) { Mandatory = isMandatory/*, isConditional*/};
                    // }
                    else
                    {
                        throw new CliException($"Could not load package {id}@{versionRange} dependencies");
                    }

                    // foreach (var attr in dependenciesElement.Attributes())
                    // {
                    //     if (attr.Name == "id" || attr.Name == "version")
                    //     {
                    //         continue;
                    //     }
                    //
                    //     result.AddMetadata(attr.Name.LocalName, attr.Value);
                    // }

                    deps.Add(result);
                }
            }
            
            var testPksElements = rootNode.Element("testPackages", true)?.Elements("testPackages", true);
            if (testPksElements != null)
            {
                foreach (var element in testPksElements)
                {
                    var testPackagesList = element.Elements().Select(depEl =>
                        new Dependency(depEl.Attribute("id").Value, depEl.Attribute("version").Value));
                    testPackages.AddRange(testPackagesList);
                }
            }

            // var uiElements = rootNode.Element("ui", true)?.Elements("wizardStep", true);
            // if (uiElements != null)
            // {
            //     foreach (var uiElement in uiElements)
            //     {
            //         var step = (PackageWizardStep)ParseGroup(uiElement, new PackageWizardStep());
            //         step.Id = uiElement.Attribute("id")?.Value;
            //         package.UserInterface.AddStep(step);
            //     }
            // }

            // var metadataElements = rootNode.Element("metadata", true)?.Elements();
            // if (metadataElements != null)
            // {
            //     foreach (var element in metadataElements)
            //     {
            //         package.AddMetadata(element.Name.LocalName, element.Value);
            //     }
            // }

            // string manifestVersionString = rootNode.Element("manifestVersion", true)?.Value;
            // int manifestVersion = -1;
            // Int32.TryParse(manifestVersionString, out manifestVersion);
            // package.ManifestVersion = manifestVersion;
            //
            // string minSqlCompatibilityString = rootNode.Element("minSqlCompatibility", true)?.Value;
            // int minSqlCompatibility = -1;
            // Int32.TryParse(minSqlCompatibilityString, out minSqlCompatibility);
            // package.MinSqlCompatibility = minSqlCompatibility;
            // package.TargetLayerDirectory = rootNode.Element("targetLayerDirectory", true)?.Value;
            // package.IsToForceInstall = rootNode.Element("IsToForceInstall", true)?.Value != null ? bool.Parse(rootNode.Element("IsToForceInstall", true).Value) : false;
            // package.BuildDate = rootNode.Element("buildDate", true)?.Value != null ? DateTime.ParseExact(rootNode.Element("buildDate", true).Value, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture) : (DateTime?)null;
            // package.IsRootPackage = (rootNode.Element("keywords", true)?.Value.Split(',').Any(k => k.Contains("cmf-root-package")) ?? false);
            // package.ForceRerunAfterDatabaseRestore = bool.Parse(rootNode.Element("forceRerunAfterDatabaseRestore", true)?.Value ?? "false");
            var cmfPackage = new CmfPackageV1(
                rootNode.Element("name", true)?.Value,
                rootNode.Element("packageId", true)?.Value,
                rootNode.Element("version", true)?.Value,
                rootNode.Element("description", true)?.Value,
                cliPackageType,
                rootNode.Element("targetDirectory", true)?.Value,
                rootNode.Element("targetLayer", true)?.Value,
                bool.Parse(rootNode.Element("isInstallable", true)?.Value ?? "false"),
                rootNode.Element("isUniqueInstall", true)?.Value != null ? bool.Parse(rootNode.Element("isUniqueInstall", true).Value) : false,
                rootNode.Element("keywords", true)?.Value,
                true,
                deps,
                steps,
                null,
                null,
                waitForIntegrationEntries: false,
                testPackages
            );
            
            return cmfPackage;
        }

    public static CmfPackageV1 FromJson(string manifest)
    {
        return FromJson(JsonConvert.DeserializeObject<JObject>(manifest));
    }
    
    /// <summary>
    /// Forms the PackageManifest from the json object.
    /// </summary>
    /// <param name="json">The JSON.</param>
    /// <returns></returns>
    /// <exception cref="PackageReadingException">
    /// </exception>
    public static CmfPackageV1 FromJson(JObject json)
    {
        // Confirm if it is a standard deployment package
        var keywords = new List<string>();
        
        if (json.Property("keywords")?.Value != null && json.Property("keywords")?.Value.Type == JTokenType.Array)
        {
            keywords = JsonConvert.DeserializeObject<List<string>>(json.Property("keywords")!.Value.ToString());
        }
        
        bool isDeploymentPackage = keywords.Any(k => ((JToken)k).ToString() == JSONPackageKeyword);
        bool isTestPackage = keywords.Any(k => ((JToken)k).ToString() == JSONTestsPackageKeyword);

        var rootNode = json.Property("deployment");

        // due to a bug (not returning the keywords) in the npm registry that we are using
        // we need to disable these checks for some cases
        string disableKeywordsEnvVar = Environment.GetEnvironmentVariable("cmf_cli_disable_keywords_check");
        bool disableKeywords = disableKeywordsEnvVar?.ToLowerInvariant() == "true" || disableKeywordsEnvVar == "1";
        if (!disableKeywords)
        {
            // If none of the keywords are found, this package is not valid and cannot be accepted by cmf cli 
            if (!isTestPackage && !isDeploymentPackage)
            {
                throw new CliException($"Invalid manifest file: one of the following keywords must be present: {JSONPackageKeyword} or {JSONTestsPackageKeyword}.");
            }
            else if (isTestPackage && isDeploymentPackage)
            {
                throw new CliException($"Invalid manifest file: only one of the following keywords can be present at the same time: {JSONPackageKeyword} and {JSONTestsPackageKeyword}.");
            }
            else if (isDeploymentPackage && rootNode == null)
            {
                throw new CliException("Invalid manifest file: missing \"deployment\" property.");
            }
        }

        // Some packages (only the Test ones right now) do not have a manifest.xml, because they are not
            // Deployment Framework packages. Nonetheless, we want the CLI to be able to, amongst other things,
            // publish those packages to the repositories alongside the other ones. So, for those packages only, we 
            // allow not having a "deployment" property.
            if (isTestPackage)
            {
                return new CmfPackageV1(
                    json.Property("name")?.Value.ToString(),
                    json.Property("name")?.Value.ToString(),
                    json.Property("version")?.Value.ToString(),
                    json.Property("description")?.Value?.ToString(),
                    PackageType.Tests,
                    null,
                    null,
                    false,
                    false,
                    keywords: string.Join(", ", keywords),
                    true,
                    [],
                    [],
                    null,
                    null,
                    waitForIntegrationEntries: false,
                    []
                );
            }
        // if (!string.IsNullOrEmpty(json.Property("systemName")?.Value.ToString()))
        // {
        //     package.AddMetadata(PackageManifestReader.MetadataKey.ApplicationName, json.Property("systemName").Value.ToString());
        //
        //     var targetSystemVersionText = json.Property("systemVersion")?.Value.ToString();
        //     if (!string.IsNullOrWhiteSpace(targetSystemVersionText))
        //     {
        //         package.AddMetadata(PackageManifestReader.MetadataKey.ApplicationVersion, targetSystemVersionText);
        //     }
        // }

        var deploymentVariables = rootNode.Children<JObject>();
        string packageType = null;

        foreach (var entry in deploymentVariables)
        {
            // string manifestVersionString = entry.Property("manifestVersion")?.Value.ToString();
            // int manifestVersion = -1;
            // Int32.TryParse(manifestVersionString, out manifestVersion);
            // package.ManifestVersion = manifestVersion;

            // string minSqlCompatibilityString = entry.Property("minSqlCompatibility")?.Value.ToString();
            // int minSqlCompatibility = -1;
            // Int32.TryParse(minSqlCompatibilityString, out minSqlCompatibility);
            // package.MinSqlCompatibility = minSqlCompatibility;

            if (!string.IsNullOrEmpty(entry.Property("packageType")?.Value.ToString()))
            {
                packageType = entry.Property("packageType")?.Value.ToString();
            }

            // if (!string.IsNullOrEmpty(entry.Property("targetDirectory")?.Value.ToString()))
            // {
            //     targetDirectory = entry.Property("targetDirectory")?.Value.ToString();
            // }
            //
            // if (!string.IsNullOrEmpty(entry.Property("targetLayerDirectory")?.Value.ToString()))
            // {
            //     package.TargetLayerDirectory = entry.Property("targetLayerDirectory")?.Value.ToString();
            // }
            //
            // if (!string.IsNullOrEmpty(entry.Property("targetLayer")?.Value.ToString()))
            // {
            //     package.TargetLayer = entry.Property("targetLayer")?.Value.ToString();
            // }

            // if (!string.IsNullOrEmpty(entry.Property("buildDate")?.Value.ToString()))
            // {
            //     DateTime dt;
            //     if (DateTime.TryParse(entry.Property("buildDate").Value.ToString(), out dt))
            //         package.BuildDate = dt;
            // }
            // else
            // {
            //     package.BuildDate = (DateTime?)null;
            // }
            // package.IsInstallable = bool.Parse(entry.Property("isInstallable")?.Value.ToString() ?? "false");
        }

        var auxArr = (JObject)rootNode.Value;
        var steps = new List<Step>();
        if (auxArr.Property("steps").Value.Type == JTokenType.Array)
        {
            var stepsEl = (JArray)auxArr.Property("steps").Value;

            if (stepsEl != null)
            {
                foreach (var element in stepsEl)
                {
                    if (element.Type == JTokenType.Object)
                    {
                        var elem = (JObject)element;

                        Step step = new Step(
                            type: Enum.Parse(typeof(StepType), elem.Property("type")?.Value.ToString()) is StepType
                                ? (StepType)Enum.Parse(typeof(StepType), elem.Property("type")?.Value.ToString())
                                : StepType.Generic,
                            title: elem.Property("title")?.Value.ToString(),
                            onExecute: elem.Property("onExecute")?.Value.ToString(),
                            contentPath: elem.Property("contentPath")?.Value.ToString(),
                            file: null,
                            tagFile: elem.Property("tagFile")?.Value.ToString() != null ? bool.Parse(elem.Property("tagFile")?.Value.ToString()) : null,
                            targetDatabase: elem.Property("targetDatabase")?.Value.ToString(),
                            messageType: MessageType.ImportObject, // TODO: get value
                            relativePath: null,
                            filePath: elem.Property("filePath")?.Value.ToString()
                        );
                        steps.Add(step);
                    }
                }
            }
        }

        #region PackageDemands Parsing

        // var demandsAuxArr = (JObject)rootNode.Value;
        // if (demandsAuxArr.Property("packageDemands")?.Value.Type == JTokenType.Array)
        // {
        //     var packageDemands = (JArray)demandsAuxArr.Property("packageDemands").Value;
        //
        //     if (packageDemands != null)
        //     {
        //         foreach (var item in packageDemands)
        //         {
        //             if (item.Type == JTokenType.Object)
        //             {
        //                 var element = (JObject)item;
        //
        //                 PackageDemand demand = ParseDemand(element);
        //                 package.AddDemand(demand);
        //             }
        //         }
        //     }
        // }

        #endregion

        // var varsAuxArray = (JObject)rootNode.Value;
        // if (varsAuxArray.Property("variables")?.Value != null)
        // {
        //     if (varsAuxArray.Property("variables").Value.Type == JTokenType.Array)
        //     {
        //         var variablesElements = (JArray)varsAuxArray.Property("variables").Value;
        //         if (variablesElements != null)
        //         {
        //             foreach (JObject variableElement in variablesElements)
        //             {
        //                 var variable = ParseVariable(variableElement);
        //                 package.AddVariable(variable);
        //             }
        //         }
        //     }
        // }

        DependencyCollection deps = new();
        DependencyCollection testPackages = new();
        var dependenciesElements = json.Property("dependencies")?.Children<JObject>().Properties();

        var mandatoryDependencies = ConvertFromJsonToDependencies(json.Property("mandatoryDependencies")?.Children<JObject>().Properties());
        var conditionalDependencies = ConvertFromJsonToDependencies(json.Property("conditionalDependencies")?.Children<JObject>().Properties());

        if (dependenciesElements != null)
        {
            foreach (var dependency in dependenciesElements)
            {
                var version = dependency.Value.ToString();
                var id = dependency.Name;
                
                if (version.StartsWith(NPMAliasPrefix))
                {
                    var parts = version.Substring(NPMAliasPrefix.Length).Split("@");
                    id = parts[0];
                    version = parts[1];
                }

                bool isDependencyMandatory = mandatoryDependencies.Any(item => item.Id.Equals(id) && item.Version.Equals(version));
                bool isDependencyConditional = conditionalDependencies.Any(item => item.Id.Equals(id) && item.Version.Equals(version));

                if (!string.IsNullOrWhiteSpace(id) && version != null)
                {
                    deps.Add(new Dependency(id, version) { Mandatory = isDependencyMandatory/*, isConditional*/});
                }
                // else if (!string.IsNullOrWhiteSpace(id))
                // {
                //     package.AddDependency(new PackageDependency(id, isDependencyMandatory, isDependencyConditional));
                // }
                else
                {
                    throw new CliException("Invalid manifest");
                }
            }
        }

        // var uiAuxArray = (JObject)rootNode.Value;
        // if (uiAuxArray.Property("ui")?.Value.Type == JTokenType.Array)
        // {
        //     var uiElements = (JArray)varsAuxArray.Property("ui").Value;
        //     if (uiElements != null)
        //     {
        //         foreach (JObject uiElement in uiElements)
        //         {
        //             var step = (PackageWizardStep)ParseGroup(uiElement, new PackageWizardStep());
        //             step.Id = uiElement.Property("id")?.Value.ToString();
        //             package.UserInterface.AddStep(step);
        //         }
        //     }
        // }

        // var metadata = (JObject)rootNode.Value;
        // var metadataElements = metadata.Property("metadata")?.Children<JObject>().Properties();
        // var aux = json.ToString();
        // if (metadataElements != null)
        // {
        //     foreach (var element in metadataElements)
        //     {
        //         package.AddMetadata(element.Name, element.Value.ToString());
        //     }
        // }
        
        PackageType cliPackageType = PackageType.Generic;
        Enum.TryParse(packageType, out cliPackageType);
        
        // package.IsRootPackage = keywords.Any(s => s.Equals(JSONPackageKeywordIsRootPackage)) ? true : false;
        //
        //
        // package.PackageId = json.Property("name")?.Value.ToString();
        // package.TargetDirectory = json.Property("targetDirectory")?.Value.ToString();
        // package.TargetLayer = json.Property("targetLayer")?.Value.ToString();
        // package.IsUniqueInstall = bool.Parse(json.Property("isUniqueInstall")?.Value.ToString() ?? "false");
        // package.IsToForceInstall = bool.Parse(json.Property("isToForceInstall")?.Value.ToString() ?? "false");
        // if (bool.TryParse(json.Property("forceRerunAfterDatabaseRestore")?.Value.ToString(), out bool forceRerunAfterDatabaseRestore))
        // {
        //     package.ForceRerunAfterDatabaseRestore = forceRerunAfterDatabaseRestore;
        // }
        // package.Version = Cmf.Core.Versioning.Version.Parse(json.Property("version")?.Value.ToString());
        // string description = json.Property("description")?.Value.ToString();
        // package.Description = string.IsNullOrEmpty(description) ? null : description;
        // package.Name = json.Property("packageName")?.Value.ToString();
        var cmfPackage = new CmfPackageV1(
            json.Property("packageName")?.Value.ToString(),
            json.Property("name")?.Value.ToString(),
            json.Property("version")?.Value.ToString(),
            json.Property("description")?.Value?.ToString(),
            cliPackageType,
            json.Property("targetDirectory")?.Value.ToString(),
            json.Property("targetLayer")?.Value.ToString(),
            bool.Parse(json.Property("isInstallable")?.Value.ToString() ?? "false"),
            bool.Parse(json.Property("isUniqueInstall")?.Value.ToString() ?? "false"),
            keywords: string.Join(", ", keywords),
            true,
            deps,
            steps,
            null,
            null,
            waitForIntegrationEntries: false,
            testPackages
        );

        return cmfPackage;
    }

    public static string JSONPackageKeyword = "cmf-deployment-package";
    public static string JSONPackageKeywordIsInstallable = "cmf-deployment-installable";
    public static string JSONPackageKeywordIsRootPackage = "cmf-deployment-rootPackage";
    
    public static string JSONTestsPackageKeyword = "cmf-tests-package";

    public string ToJson(bool lowercase = false, bool addManifestVersion = false)
    {
        #region assemble steps
        JArray stepsArray = new JArray();

        var jsonSerializerSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            PreserveReferencesHandling = PreserveReferencesHandling.None,
        };

        var jsonSerializer = JsonSerializer.Create(jsonSerializerSettings);

        foreach (var step in package.Steps ?? Array.Empty<Step>().ToList())
        {
            var stepObject = JObject.FromObject(step, jsonSerializer);
            // foreach (var attributeName in step.AttributeNames)
            // {
            //     if (stepObject[attributeName] == null)
            //     {
            //         stepObject.Add(attributeName, step.GetAttribute(attributeName));
            //     }
            // }
            //
            // var elementTypes = step.Elements.ToLookup(x => x.Name.LocalName);
            //
            // foreach (var element in elementTypes)
            // {
            //     stepObject.Add(element.Key, JArray.FromObject(element.ToArray(), jsonSerializer));
            // }

            stepsArray.Add(stepObject);
        }


        JObject jObject = new JObject();
        foreach (JObject desc in stepsArray)
        {
            var children = desc.Value<JObject>();

            if (children.Properties() != null)
            {
                var values = children.Value<JObject>();
                foreach (var value in values)
                {
                    if (value.Value.Type == JTokenType.Array)
                    {
                        var entityTypes = value.Value;
                        var entities = entityTypes.Values<JObject>();
                        foreach (var entity in entities)
                        {
                            var x = entity.Value<JObject>();
                            var xx = x.Value<JObject>();
                            foreach (var xxx in xx)
                            {
                                var final = xxx.Value.Value<JObject>();
                                foreach (var finalElement in final)
                                {
                                    if (finalElement.Key.StartsWith("@"))
                                    {
                                        var newValue = finalElement.Key.Replace("@", "");
                                        JProperty newProperty = new JProperty(newValue, finalElement.Value);
                                        jObject.Add(newProperty);
                                    }
                                }
                                final.RemoveAll();
                                final.Add(jObject.Properties());
                                jObject.RemoveAll();
                            }
                        }
                    }
                }
            }

        }

        JObject j = new JObject(new JProperty("steps", stepsArray));
        #endregion

        #region assemble wizardSteps
        JArray uiArray = new JArray();
        JArray variablesArray = new JArray();

        var jsonSerializerSettingsUI = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            PreserveReferencesHandling = PreserveReferencesHandling.None,
        };

        // var jsonSerializerUI = JsonSerializer.Create(jsonSerializerSettingsUI);
        //
        // foreach (var wizardStep in package.UserInterface.Steps)
        // {
        //     var wizardStepObject = JObject.FromObject(wizardStep, jsonSerializerUI);
        //     uiArray.Add(wizardStepObject);
        // }
        //
        // foreach (var variable in package.Variables)
        // {
        //     var variablesObject = JObject.FromObject(variable, jsonSerializerUI);
        //     variablesArray.Add(variablesObject);
        // }

        #endregion

        #region assemble dependencies
        var dependecies = new JObject();
        var mandatoryDependecies = new JObject();
        var conditionalDependencies = new JObject();

        // List of dependencies following the format "<Id>@<Version>" that have already been added to the dependency list.
        // Used to detect duplicates (Id and Version are the same) and show an error message in such scenarios
        var duplicateDependencies = package.Dependencies
            .GroupBy(dep => $"{dep.Id}@{dep.Version}")
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateDependencies.Count > 0)
        {
            throw new Exception($"Invalid Package {package.PackageAtRef}: the following dependencies are declared more than once - {string.Join(", ", duplicateDependencies)}");
        }

        // Collection of PackageIds that appear more than once in this list of dependencies (with different versions)
        // These packages will need to use the npm alias functionality
        var duplicateDependencyIds = package.Dependencies
            .GroupBy(dep => dep.Id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        var dependencyAliases = new Dictionary<string, int>();

        string getUniquePackageAlias(string packageName)
        {
            if (!dependencyAliases.TryGetValue(packageName, out int counter))
            {
                counter = 0;
            }

            string aliasName;
            do
            {
                counter++;

                aliasName = $"{packageName}__{counter}";
            } while (package.Dependencies.Any(dep => dep.Id.IgnoreCaseEquals(aliasName)));

            dependencyAliases[packageName] = counter;

            return aliasName;
        }

        foreach (var dependency in package.Dependencies)
        {
            // If this dependency is declared more than once on this package (usually for different versions)
            // We need to register as an alias with a different JS key
            var property = duplicateDependencyIds.Contains(dependency.Id)
                ? new JProperty(getUniquePackageAlias(dependency.Id), $"{NPMAliasPrefix}{dependency.Id}@{dependency.Version}")
                : new JProperty(dependency.Id, dependency.Version);

            dependecies.Add(property);

            if (dependency.Mandatory)
            {
                mandatoryDependecies.Add(property);
            }
            //
            // if (dependency.IsConditional)
            // {
            //     conditionalDependencies.Add(property);
            // }
        }

        #endregion

        #region assemble metadata
        var metadata = new JObject();
        // foreach (var meta in package.extendedMetadata)
        // {
        //     var property = new JProperty(meta.Key, meta.Value);
        //     metadata.Add(property);
        // }
        #endregion

        #region assemble keywords 

        // Add keywords to enable searching for packages on an package registry
        var keywordsList = new List<string>();

        // Default DF package keyword
        keywordsList.Add(JSONPackageKeyword);

        if (package.IsInstallable ?? false)
        {
            keywordsList.Add(JSONPackageKeywordIsInstallable);
        }
        // if (package.IsRootPackage)
        // {
        //     keywordsList.Add(JSONPackageKeywordIsRootPackage);
        // }

        var keywords = JArray.FromObject(keywordsList);

        #endregion

        #region assemble package demands

        JArray demandsArray = new JArray();

        // foreach (var demand in package.Demands)
        // {
        //     var demandObject = JObject.FromObject(demand, jsonSerializer);
        //     foreach (var attributeName in demand.AttributeNames)
        //     {
        //         demandObject.Add(attributeName, demand.GetAttribute(attributeName));
        //     }
        //
        //     var elementTypes = demand.Elements.ToLookup(x => x.Name.LocalName);
        //
        //     foreach (var element in elementTypes)
        //     {
        //         demandObject.Add(element.Key, JArray.FromObject(element.ToArray(), jsonSerializer));
        //     }
        //
        //     demandsArray.Add(demandObject);
        // }


        foreach (JObject desc in demandsArray)
        {
            var children = desc.Value<JObject>();

            if (children.Properties() != null)
            {
                var values = children.Value<JObject>();
                foreach (var value in values)
                {
                    if (value.Value.Type == JTokenType.Array)
                    {
                        var entityTypes = value.Value;
                        var entities = entityTypes.Values<JObject>();
                        foreach (var entity in entities)
                        {
                            var entityValueAux = entity.Value<JObject>();
                            var entityValues = entityValueAux.Value<JObject>();
                            foreach (var entityValue in entityValues)
                            {
                                var final = entityValue.Value.Value<JObject>();
                                foreach (var finalElement in final)
                                {
                                    if (finalElement.Key.StartsWith("@"))
                                    {
                                        var newValue = finalElement.Key.Replace("@", "");
                                        JProperty newProperty = new JProperty(newValue, finalElement.Value);
                                        jObject.Add(newProperty);
                                    }
                                }
                                final.RemoveAll();
                                final.Add(jObject.Properties());
                                jObject.RemoveAll();
                            }
                        }
                    }
                }
            }

        }

        #endregion

        JObject jsonObject = new JObject(
                                new JProperty("name", lowercase ? package.PackageId.ToLowerInvariant() : package.PackageId),
                                new JProperty("description", package.Description),
                                new JProperty("packageName", package.Name != null ? package.Name : package.PackageId),
                                new JProperty("version", package.Version?.ToString()),
                                new JProperty("author", "Critical Manufacturing"),
                                new JProperty("keywords", keywords),
                                // new JProperty("isToForceInstall", package.IsToForceInstall),
                                new JProperty("isUniqueInstall", package.IsUniqueInstall),
                                // new JProperty("forceRerunAfterDatabaseRestore", package.ForceRerunAfterDatabaseRestore),
                                new JProperty("deployment", new JObject(
                                                                // new JProperty("manifestVersion", package.ManifestVersion),
                                                                new JProperty("isInstallable", package.IsInstallable),
                                                                new JProperty("packageType", package.PackageType),
                                                                new JProperty("targetDirectory", !String.IsNullOrEmpty(package.TargetDirectory) ? package.TargetDirectory : ""),
                                                                // new JProperty("targetLayerDirectory", !String.IsNullOrEmpty(package.TargetLayerDirectory) ? package.TargetLayerDirectory : ""),
                                                                new JProperty("targetLayer", !String.IsNullOrEmpty(package.TargetLayer) ? package.TargetLayer : ""),
                                                                // new JProperty("buildDate", package.BuildDate?.ToString()),
                                                                new JProperty("steps", stepsArray),
                                                                new JProperty("packageDemands", demandsArray))),
                                new JProperty("dependencies", dependecies),
                                new JProperty("mandatoryDependencies", mandatoryDependecies),
                                new JProperty("conditionalDependencies", conditionalDependencies),
                                new JProperty("_originalPackageId", package.PackageId));

        if (addManifestVersion)
        {
            jsonObject.SelectToken("deployment")?.Value<JObject>()?.AddFirst(new JProperty("manifestVersion", CoreConstants.ManifestVersion));
        }

        // if (package.MinSqlCompatibility > 0)
        // {
        //     jsonObject["deployment"]["minSqlCompatibility"] = package.MinSqlCompatibility;
        // }

        if (uiArray.Count > 0)
        {
            var uiProperty = new JProperty("ui", uiArray);
            var jsonFinal = (JObject)jsonObject.Property("deployment").Value;
            jsonFinal.Add(uiProperty);
            jsonObject.Property("deployment").Value = jsonFinal;


        }
        if (variablesArray.Count > 0)
        {
            var varProperty = new JProperty("variables", variablesArray);
            var jsonFinal = (JObject)jsonObject.Property("deployment").Value;
            jsonFinal.Add(varProperty);
            jsonObject.Property("deployment").Value = jsonFinal;
        }
        if (metadata.HasValues)
        {
            var metadataProperty = new JProperty("metadata", metadata);
            var jsonFinal = (JObject)jsonObject.Property("deployment").Value;
            jsonFinal.Add(metadataProperty);
            jsonObject.Property("deployment").Value = jsonFinal;
        }

        return JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
    }
    
    #region utils
    // private static PackageWizardStepGroup ParseGroup(JObject uiElement, PackageWizardStepGroup result = null)
    //     {
    //         result = result ?? new PackageWizardStepGroup();
    //
    //         result.Id = uiElement.Property("id")?.Value.ToString();
    //         result.Order = int.Parse(uiElement.Property("order")?.Value.ToString() ?? "0");
    //         result.Title = uiElement.Property("title")?.Value.ToString();
    //         result.Type = uiElement.Property("type")?.Value.ToString();
    //         result.RequiresValidation = bool.Parse(uiElement.Property("requiresValidation")?.Value.ToString() ?? "false");
    //         result.IsAdvanced = bool.Parse(uiElement.Property("isAdvanced")?.Value.ToString() ?? "false");
    //         result.Condition = uiElement.Property("condition")?.Value.ToString();
    //
    //         var groupElements = uiElement.Property("groups")?.Value;
    //         if (groupElements?.Type == JTokenType.Array)
    //         {
    //             var groupArray = (JArray)groupElements;
    //             if (groupArray != null)
    //             {
    //                 foreach (JObject groupElement in groupArray)
    //                 {
    //                     var group = ParseGroup(groupElement);
    //                     result.Groups.Add(group);
    //                 }
    //             }
    //         }
    //
    //         var variableElements = uiElement.Property("variables")?.Value;
    //         if (variableElements?.Type == JTokenType.Array)
    //         {
    //             var variablesArray = (JArray)variableElements;
    //             if (variablesArray != null)
    //             {
    //                 foreach (JObject variableElement in variablesArray)
    //                 {
    //                     var variable = ParseVariable(variableElement);
    //                     result.Variables.Add(variable);
    //                 }
    //             }
    //         }
    //
    //         return result;
    //     }
    //
    //     private static VariableDefinition ParseVariable(XElement element)
    //     {
    //         var result = new VariableDefinition
    //         {
    //             Name = element.Attribute("name")?.Value,
    //             ValueType = element.Attribute("valueType")?.Value,
    //             IsRequired = bool.Parse(element.Attribute("isRequired")?.Value ?? "false"),
    //             GroupName = element.Attribute("groupName")?.Value,
    //             Label = element.Attribute("label")?.Value,
    //             ReadOnly = bool.Parse(element.Attribute("readOnly")?.Value ?? "false"),
    //             ValidationConfiguration = element.Attribute("validationConfiguration")?.Value,
    //             Placeholder = element.Attribute("placeholder")?.Value,
    //             Default = element.Attribute("default")?.Value,
    //             IsToValidate = bool.Parse(element.Attribute("isToValidate")?.Value ?? "true"),
    //         };
    //
    //         return result;
    //     }
    //
    //     private static VariableDefinition ParseVariable(JObject element)
    //     {
    //         var result = new VariableDefinition
    //         {
    //             Name = element.Property("name")?.Value.ToString(),
    //             ValueType = element.Property("valueType")?.Value.ToString(),
    //             IsRequired = bool.Parse(element.Property("isRequired")?.Value.ToString() ?? "false"),
    //             GroupName = element.Property("groupName")?.Value.ToString(),
    //             Label = element.Property("label")?.Value.ToString(),
    //             ReadOnly = bool.Parse(element.Property("readOnly")?.Value.ToString() ?? "false"),
    //             ValidationConfiguration = element.Property("validationConfiguration")?.Value.ToString(),
    //             Placeholder = element.Property("placeholder")?.Value.ToString(),
    //             Default = element.Property("default")?.Value.ToString(),
    //             IsToValidate = bool.Parse(element.Property("isToValidate")?.Value.ToString() ?? "true"),
    //         };
    //
    //         return result;
    //     }
    //
    //
    //     private static PackageStep ParseStep(JObject element)
    //     {
    //         var coreAttributes = new string[]
    //         {
    //             "type",
    //             "id",
    //             "title",
    //             "onInitialize",
    //             "onAquire",
    //             "onValidate",
    //             "onPrepare",
    //             "onExecute",
    //             "onComplete",
    //             "onCleanup",
    //             "packageIds",
    //             "contentPath"
    //         };
    //
    //         PackageStep step;
    //
    //         if (element.Property("reevaluatePlan") != null || element.Property("packageId") != null)
    //         {
    //             var packageDeploymentStep = new PackageDeploymentStep();
    //
    //             packageDeploymentStep.ReevaluatePlan = bool.Parse(element.Property("reevaluatePlan")?.Value.ToString() ?? "false");
    //             packageDeploymentStep.PackageId = element.Property("packageId")?.Value.ToString();
    //             step = packageDeploymentStep;
    //         }
    //         else
    //         {
    //             step = new PackageStep();
    //         }
    //
    //         step.Type = element.Property("type")?.Value.ToString();
    //         step.Id = element.Property("id")?.Value.ToString();
    //         step.Title = element.Property("title")?.Value.ToString();
    //         step.ContentPath = element.Property("contentPath")?.Value.ToString();
    //         step.OnInitialize = element.Property("onInitialize")?.Value.ToString();
    //         step.OnAquire = element.Property("onAquire")?.Value.ToString();
    //         step.OnValidate = element.Property("onValidate")?.Value.ToString();
    //         step.OnPrepare = element.Property("onPrepare")?.Value.ToString();
    //         step.OnExecute = element.Property("onExecute")?.Value.ToString();
    //         step.OnComplete = element.Property("onComplete")?.Value.ToString();
    //         step.OnCleanup = element.Property("onCleanup")?.Value.ToString();
    //
    //         var extraAttributes = element.Properties().Where(a => !coreAttributes.Contains(a.Name)).ToList();
    //         foreach (var a in extraAttributes)
    //         {
    //             step.AddAttribute(a.Name, a.Value.ToString());
    //         }
    //
    //         foreach (var innerElement in element.Children())
    //         {
    //             if (innerElement.Type == JTokenType.Object)
    //             {
    //                 var elem = (JObject)innerElement;
    //                 step.AddElement(elem);
    //             }
    //         }
    //
    //         return step;
    //     }
        
        
        // /// <summary>
        // /// Parse PackageDemand elements from a XElement.
        // /// </summary>
        // /// <param name="element">Element to be parsed.</param>
        // /// <returns></returns>
        // private static PackageDemand ParseDemand(XElement element)
        // {
        //     var coreAttributes = new string[]
        //     {
        //         "type",
        //         "id",
        //         "title",
        //         "value",
        //         "onInitialize",
        //         "onAquire",
        //         "onValidate",
        //         "onPrepare",
        //         "onExecute",
        //         "onComplete",
        //         "onCleanup",
        //         "packageIds"
        //     };
        //
        //     PackageDemand demand = new PackageDemand();
        //
        //     demand.Type = (PackageDemandType)Enum.Parse(typeof(PackageDemandType), element.Attribute("type")?.Value);
        //     demand.Id = element.Attribute("id")?.Value;
        //     demand.Title = element.Attribute("title")?.Value;
        //     demand.Value = element.Attribute("value")?.Value;
        //
        //     var extraAttributes = element.Attributes().Where(a => !coreAttributes.Contains(a.Name.LocalName)).ToList();
        //     foreach (var a in extraAttributes)
        //     {
        //         demand.AddAttribute(a.Name.LocalName, a.Value);
        //     }
        //
        //     foreach (var innerElement in element.Elements())
        //     {
        //         demand.AddElement(innerElement);
        //     }
        //
        //     return demand;
        // }
        //
        // /// <summary>
        // /// Parse PackageDemand elements from a JObject.
        // /// </summary>
        // /// <param name="element">Element to be parsed.</param>
        // /// <returns></returns>
        // private static PackageDemand ParseDemand(JObject element)
        // {
        //     var coreAttributes = new string[]
        //     {
        //         "type",
        //         "id",
        //         "title",
        //         "value",
        //         "onInitialize",
        //         "onAquire",
        //         "onValidate",
        //         "onPrepare",
        //         "onExecute",
        //         "onComplete",
        //         "onCleanup",
        //         "packageIds"
        //     };
        //
        //     PackageDemand demand = new PackageDemand();
        //
        //     demand.Type = (PackageDemandType)Enum.Parse(typeof(PackageDemandType), element.Property("type")?.Value.ToString());
        //     demand.Id = element.Property("id")?.Value.ToString();
        //     demand.Title = element.Property("title")?.Value.ToString();
        //     demand.Value = element.Property("value")?.Value.ToString();
        //
        //     var extraAttributes = element.Properties().Where(a => !coreAttributes.Contains(a.Name)).ToList();
        //     foreach (var a in extraAttributes)
        //     {
        //         demand.AddAttribute(a.Name, a.Value.ToString());
        //     }
        //
        //     foreach (var innerElement in element.Children())
        //     {
        //         if (innerElement.Type == JTokenType.Object)
        //         {
        //             var elem = (JObject)innerElement;
        //             demand.AddElement(elem);
        //         }
        //     }
        //
        //     return demand;
        // }
        
        /// <summary>
        /// Convert json array of dependecies to list
        /// </summary>
        /// <param name="dependenciesJson">Dependecies json</param>
        /// <returns>Array of dependencies</returns>
        private static List<Dependency> ConvertFromJsonToDependencies(IJEnumerable<JProperty> dependenciesJson)
        {
            List<Dependency> dependencies = new List<Dependency>();
        
            if(dependenciesJson != null && dependenciesJson.Count() > 0)
            {
                foreach (JProperty item in dependenciesJson)
                {
                    var packageAux = item.Value.ToString();
                    var id = item.Name;
                    
                    if (packageAux.StartsWith(NPMAliasPrefix))
                    {
                        var parts = packageAux.Substring(NPMAliasPrefix.Length).Split("@");
                        id = parts[0];
                        packageAux = parts[1];
                    }

                    string range;
                    if (packageAux.Contains("~") || packageAux.Contains("^") || packageAux.Contains("*"))
                    {
                        continue;
                    }
                    try
                    {
                        range = packageAux;
                    }
                    catch
                    {
                        //explicity silenced
                        continue;
                    }
        
                    if (!string.IsNullOrWhiteSpace(id) && range != null)
                    {
                        dependencies.Add(new Dependency(id, range));
                    }
                    else if (!string.IsNullOrWhiteSpace(id))
                    {
                        dependencies.Add(new Dependency(id, null));
                    }
                    else
                    {
                        throw new CliException("Invalid manifest");
                    }
                }
            }
        
            return dependencies;
        }
    #endregion
    
    public static CmfPackageV1 FromSourceManifest(IFileInfo cmfPackageFile)
    {
        string fileContent = cmfPackageFile.ReadToString();
        var cmfPackage = JsonConvert.DeserializeObject<CmfPackageV1>(fileContent);
        cmfPackage.IsToSetDefaultValues = true;
        cmfPackage.SourceManifestFile = cmfPackageFile;
        return cmfPackage;
    }

    // this does not work if the archive contains an entry with a name with more than 100 characters
    public static void ConvertZipToTarGz_NET(IFileInfo package)
    {
        using Stream zipToOpen = package.OpenRead();
        using ZipArchive zip = new(zipToOpen, ZipArchiveMode.Read);
        var tgz = package.FileSystem.FileInfo.New(package.FullName.Replace(".zip", ".tgz"));
        using var tarWriter = new TarWriter(tgz.OpenWrite());
        foreach (var zipEntry in zip.Entries)
        {
            using var entryStream = zipEntry.Open();
            // Create the tar entry and copy the content from the zip entry
            var tarEntry = new V7TarEntry(TarEntryType.V7RegularFile, zipEntry.FullName);
            // tarEntry.DataStream.Seek(0, SeekOrigin.Begin);
            tarEntry.DataStream = entryStream;
            tarWriter.WriteEntry(tarEntry);
        }
    }

    public static void ConvertZipToTarGz(IFileInfo zipFile, IFileInfo tarGzFile, bool lowercase = false, bool addManifestVersion = false)
    {
        // Open the Zip file
        using var zipStream = zipFile.OpenRead();
        using (var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read))
        {
            // Create a TarGz file stream
            using var tarStream = tarGzFile.Create();
            // use SharpCompress for compatibility with Product Deployment packages which are created with the same library
            using (var tarWriter = new SharpCompress.Writers.Tar.TarWriter(tarStream, new SharpCompress.Writers.Tar.TarWriterOptions(SharpCompress.Common.CompressionType.GZip, finalizeArchiveOnClose: true)))
            {
                foreach (ZipArchiveEntry zipEntry in zipArchive.Entries)
                {
                    var entryName = zipEntry.FullName;

                    // The ZIP spec does not have a specific flag for determining if an entry is a file or folder
                    // Taking as an example the SharpZipLib, the way they validate that is by checking he last character
                    // of the full name, and validating if it is a path separator
                    if (entryName.EndsWith("\\") || entryName.EndsWith("/"))
                    {
                        continue;
                    }

                    // Open the entry stream from the Zip archive
                    using (Stream zipEntryStream = zipEntry.Open())
                    {
                        // if we found the XML manifest and do not have a JSON manifest, convert it now
                        if (entryName == CoreConstants.DeploymentFrameworkManifestFileName && !zipArchive.Entries.Any(ze => ze.FullName == CoreConstants.PackageJson))
                        {
                            using (StreamReader reader = new StreamReader(zipEntryStream, Encoding.UTF8))
                            {
                                var manifest = reader.ReadToEnd();
                                var pkg = FromXml(XDocument.Parse(manifest));
                                var ctrlr = new CmfPackageController(pkg, zipFile.FileSystem);
                                var jsonManifest = ctrlr.ToJson(lowercase, addManifestVersion);
                                using (MemoryStream tarEntryStream = new MemoryStream())
                                {
                                    using var streamWriter = new StreamWriter(tarEntryStream);
                                    streamWriter.Write(jsonManifest);
                                    streamWriter.Flush();
                                    tarEntryStream.Seek(0, SeekOrigin.Begin);
                                    tarWriter.Write($"package/{CoreConstants.PackageJson}", tarEntryStream, DateTime.Now);
                                }
                                using (MemoryStream tarEntryStream = new MemoryStream())
                                {
                                    using var streamWriter = new StreamWriter(tarEntryStream);
                                    streamWriter.Write(manifest);
                                    streamWriter.Flush();
                                    tarEntryStream.Seek(0, SeekOrigin.Begin);
                                    tarWriter.Write($"package/{CoreConstants.DeploymentFrameworkManifestFileName}", tarEntryStream, zipEntry.LastWriteTime.DateTime);
                                }
                            }
                        }
                        else if (lowercase && entryName == CoreConstants.PackageJson)
                        {
                            using (StreamReader reader = new StreamReader(zipEntryStream, Encoding.UTF8))
                            {
                                var manifest = reader.ReadToEnd();
                                // this should be handled by deserializing to CmfPackageJson and serializing again, but currently many fields are not supported
                                // so we'll change just the necessary entries
                                var json = JsonConvert.DeserializeObject<JObject>(manifest);
                                json["_originalPackageId"] = json[json.ContainsKey("_originalPackageId") ? "_originalPackageId" : "name"]!.Value<string>();
                                json["name"] = json["name"]!.Value<string>().ToLowerInvariant();
                                manifest = JsonConvert.SerializeObject(json, Formatting.Indented);
                                
                                using (MemoryStream tarEntryStream = new MemoryStream())
                                {
                                    using var streamWriter = new StreamWriter(tarEntryStream);
                                    streamWriter.Write(manifest);
                                    streamWriter.Flush();
                                    tarEntryStream.Seek(0, SeekOrigin.Begin);
                                    tarWriter.Write($"package/{CoreConstants.PackageJson}", tarEntryStream, DateTime.Now);
                                }
                            }
                        }
                        else
                        {
                            // Add the file content to the Tar archive
                            using (MemoryStream tarEntryStream = new MemoryStream())
                            {
                                zipEntryStream.CopyTo(tarEntryStream);
                                tarEntryStream.Seek(0, SeekOrigin.Begin);

                                // Create a tar entry in the Tar archive
                                tarWriter.Write("package/" + entryName, tarEntryStream, zipEntry.LastWriteTime.DateTime);
                            }
                        }
                    }
                }
            }
        }
    }
    
    public static void ConvertTarGzToZip(IFileInfo package, IFileInfo zipFile)
    {
        // byte[] tarGzFileContent = package.FileSystem.File.ReadAllBytes(package.FullName);
        // using var tarGzStream = new MemoryStream(tarGzFileContent);
        // using var gzipStream = new GZipStream(tarGzStream, CompressionMode.Decompress);
        using GZipStream gzipStream = new GZipStream(package.OpenRead(), CompressionMode.Decompress);
        using TarReader tarReader = new(gzipStream);
        // Prepare the output ZIP stream
        using var zipStream = zipFile.OpenWrite();
        using var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, false);
        while (tarReader.GetNextEntry() is { } entry)
        {
            
            // Check for the "package" folder and strip it out
            string packageFolderName = "package/";

            // If the tar file has an entry just for the "package/" folder, we simply skip that entry,
            // because the folder itself will never be a part of the converted zip, only the files/sub-folders
            // inside of it
            if (entry.Name == packageFolderName)
            {
                continue;
            }

            // Iterate through the files in the TAR archive
            // If the file is in the "package" folder, strip it
            var entryName = entry.Name.StartsWith(packageFolderName) ? entry.Name.Substring(packageFolderName.Length) : entry.Name;
            
            // NOTE: we're believing the archive always contains a manifest.xml here, so we are not doing any conversion. 
            if (entryName == CoreConstants.PackageJson && entry.EntryType == TarEntryType.V7RegularFile)
            {
                // // Read the content of the file inside the TAR
                // using var reader = new StreamReader(entry.DataStream,leaveOpen: true);
                // var jsonManifest = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                // var manifest = FromJson(jsonManifest);
                // var xmlManifestEntry = zipArchive.CreateEntry(CoreConstants.DeploymentFrameworkManifestFileName);
                // using (var xmlManifestStream = xmlManifestEntry.Open())
                // {
                //     using var streamWriter = new StreamWriter(xmlManifestStream);
                //     streamWriter.Write(manifest.ToXml());
                //     streamWriter.Flush();
                //     if (xmlManifestStream.CanSeek)
                //     {
                //         xmlManifestStream.Seek(0, SeekOrigin.Begin);
                //     }
                // }
                //
                // var jsonManifestEntry = zipArchive.CreateEntry(CoreConstants.PackageJson);
                // using (var jsonManifestStream = jsonManifestEntry.Open())
                // {
                //     using var jsonStreamWriter = new StreamWriter(jsonManifestStream);
                //     jsonStreamWriter.Write(jsonManifest);
                //     jsonStreamWriter.Flush();
                //     if (jsonManifestStream.CanSeek)
                //     {
                //         jsonManifestStream.Seek(0, SeekOrigin.Begin);
                //     }
                // }
            }
            
            
            // Add the entry as is if it's not in the "package" folder
            var zipEntry = zipArchive.CreateEntry(entryName);
            zipEntry.LastWriteTime = entry.ModificationTime;

            using (var zipEntryStream = zipEntry.Open())
            {
                // if (entry.DataStream != null)
                // {
                (entry.DataStream ?? MemoryStream.Null).CopyTo(zipEntryStream);    
                // }
                // else
                // {
                //     var sw = new StreamWriter(zipEntryStream);
                //     sw.Write(String.Empty);
                //     sw.Flush();
                // }
            }
        }
        // zipArchive.Dispose();
    }
}
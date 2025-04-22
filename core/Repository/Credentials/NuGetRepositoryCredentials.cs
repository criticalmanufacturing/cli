using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Utilities;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cmf.CLI.Core.Repository.Credentials
{
    public class NuGetRepositoryCredentials : IRepositoryCredentials
    {
        #region Constants

        private const string ConfigurationTag = "configuration";
        private const string PackageSourcesTag = "packageSources";
        private const string PackageSourceCredentialsTag = "packageSourceCredentials";
        private const string AddTag = "add";
        private const string ValueAttr = "value";
        private const string KeyAttr = "key";
        private const string ProtocolVersionAttr = "protocolVersion";
        private const string UsernameKey = "Username";
        private const string ClearTextPasswordKey = "ClearTextPassword";

        #endregion Constants

        protected IFileSystem _fileSystem;

        public NuGetRepositoryCredentials(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public AuthType[] SupportedAuthTypes => [AuthType.Basic];

        public RepositoryCredentialsType RepositoryType => RepositoryCredentialsType.NuGet;

        public PropertyRequirement KeyPropertyRequirement => PropertyRequirement.Mandatory;

        public PropertyRequirement DomainPropertyRequirement => PropertyRequirement.Ignored;

        public void ValidateCredentials(IList<ICredential> credentials)
        {
            foreach (var credential in credentials)
            {
                if (string.IsNullOrEmpty(credential.Key))
                {
                    throw new Exception($"Missing mandatory \"key\" value for {RepositoryType} repository \"{credential.Repository}\"");
                }

                if (credential is not BasicCredential)
                {
                    throw new InvalidAuthTypeException(credential);
                }
            }
        }

        public async Task SyncCredentials(IList<ICredential> credentials)
        {

            IFileInfo nugetConfigFile = null;
            try
            {
                nugetConfigFile = GetConfigFile();

                var config = await LoadConfig(nugetConfigFile);

                if (config.Root == null)
                {
                    config = new XDocument(
                        new XDeclaration("1.0", "utf-8", null),
                        new XElement(ConfigurationTag));
                }

                var root = config.Root!;

                var packageSources = root.Element(PackageSourcesTag);

                if (packageSources == null)
                {
                    packageSources = new XElement(PackageSourcesTag);
                    root.Add(packageSources);
                }

                var packageSourceCredentials = root.Element(PackageSourceCredentialsTag);

                if (packageSourceCredentials == null)
                {
                    packageSourceCredentials = new XElement(PackageSourceCredentialsTag);
                    root.Add(packageSourceCredentials);
                }

                foreach (var cred in credentials)
                {
                    if (cred is BasicCredential basicCred)
                    {
                        // First thing to do, is try to find an existing source for this repository in the PackageSources
                        var sourceElem = packageSources.Elements(AddTag)
                            .Where(elem => elem.Attribute(KeyAttr)?.Value == basicCred.Key)
                            .LastOrDefault();

                        // We create the source, and give it a key/name, because the credentials are stored in a different XML element,
                        // and are associated with the repository by that key
                        if (sourceElem == null)
                        {
                            sourceElem = new XElement(AddTag,
                                new XAttribute(KeyAttr, basicCred.Key),
                                new XAttribute(ValueAttr, basicCred.Repository),
                                // TODO Can we check this by querying the repository URL somehow?
                                new XAttribute(ProtocolVersionAttr, "3"));

                            packageSources.Add(sourceElem);
                        }
                        else if (sourceElem.Attribute(KeyAttr) == null)
                        {
                            sourceElem.SetAttributeValue(ValueAttr, basicCred.Repository);
                        }

                        // Per the Microsoft documentation, any keys with a space must be replaced by the character sequence "_x0020_"
                        // to be used as element names in the credentials section
                        var normalizedKey = basicCred.Key.Replace(" ", "_x0020_");

                        // Now that we have a source that exists and we know has a key, we need to create the credentials for said repository
                        var credElem = packageSourceCredentials.Elements(normalizedKey).LastOrDefault();

                        if (credElem == null)
                        {
                            credElem = new XElement(normalizedKey);

                            packageSourceCredentials.Add(credElem);
                        }

                        // Set the username
                        var usernameElem = credElem.Elements(AddTag).Where(elem => elem.Attribute(KeyAttr)?.Value == UsernameKey).LastOrDefault();
                        if (usernameElem == null)
                        {
                            usernameElem = new XElement(AddTag,
                                new XAttribute(KeyAttr, UsernameKey),
                                new XAttribute(ValueAttr, basicCred.Username));
                            credElem.Add(usernameElem);
                        }
                        else
                        {
                            usernameElem.SetAttributeValue(ValueAttr, basicCred.Username);
                        }

                        // Set the password
                        var passwordElem = credElem.Elements(AddTag).Where(elem => elem.Attribute(KeyAttr)?.Value == ClearTextPasswordKey).LastOrDefault();
                        if (passwordElem == null)
                        {
                            passwordElem = new XElement(AddTag,
                                new XAttribute(KeyAttr, ClearTextPasswordKey),
                                new XAttribute(ValueAttr, basicCred.Password));
                            credElem.Add(passwordElem);
                        }
                        else
                        {
                            passwordElem.SetAttributeValue(ValueAttr, basicCred.Password);
                        }
                    }
                    else
                    {
                        throw new InvalidAuthTypeException(cred);
                    }
                }

                await SaveConfig(nugetConfigFile, config);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to sync credentials into NuGet config file: {nugetConfigFile?.FullName}", ex);
            }
        }

        public string GetEnvironmentVariablePrefix(string repository)
        {
            var uri = new Uri(repository);
            return GenericUtilities.BuildEnvVarPrefix(RepositoryType, $"{uri.Host}{uri.PathAndQuery.TrimEnd('/')}");
        }

        #region Protected Methods

        protected IFileInfo GetConfigFile()
        {
            // NuGet stores the per-user NuGet.config file on the AppData directory
            //      in Linux, it is stored in "~/.config", which is the path returned for ApplicationData in Linux too. Consistency, yay
            var home = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // The official Microsoft documentation has references to both `NuGet.config` and `nuget.config` explicitly in some parts of their documentation.
            // Because of that, and since Linux is case sensitive, we do our best effort to support both. In case none exists on the filesystem yet, the last one
            // we looked for (in this case, NuGet.config) will be used
            var nugetConfig = _fileSystem.FileInfo.New(_fileSystem.Path.Join(home, "NuGet", "nuget.config"));

            if (!nugetConfig.Exists)
            {
                nugetConfig = _fileSystem.FileInfo.New(_fileSystem.Path.Join(home, "NuGet", "NuGet.Config"));
            }

            return nugetConfig;
        }

        protected async Task<XDocument> LoadConfig(IFileInfo nugetConfigFile)
        {
            nugetConfigFile = GetConfigFile();

            if (!nugetConfigFile.Exists)
            {
                return new XDocument();
            }

            var contents = await _fileSystem.File.ReadAllTextAsync(nugetConfigFile.FullName);

            return XDocument.Parse(contents);
        }

        protected async Task SaveConfig(IFileInfo nugetConfigFile, XDocument document)
        {
            nugetConfigFile = GetConfigFile();

            var contents = document.ToString();

            if (nugetConfigFile.DirectoryName != null)
            {
                _fileSystem.Directory.CreateDirectory(nugetConfigFile.DirectoryName);
            }

            // Write the XML back to the NuGet.Config file
            using (var stream = _fileSystem.File.OpenWrite(nugetConfigFile.FullName))
            {
                await document.SaveAsync(stream, SaveOptions.None, default);
                stream.SetLength(stream.Position);
            }
        }

        #endregion Protected Methods
    }
}

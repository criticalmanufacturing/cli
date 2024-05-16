using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Xml.Serialization;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Utilities;
using Newtonsoft.Json;

namespace Cmf.CLI.Core.Objects.CmfApp;

/// <summary>
/// CMF App data
/// </summary>
public record AppData
{
    public string id { get; set; }
    public string name { get; set; }
    public string author { get; set; }
    public string description { get; set; }
    public string targetFramework { get; set; }
    public string licensedApplication { get; set; }
    public string icon { get; set; }
}

/// <summary>
/// Target framework of the app
/// </summary>
public record Framework
{
    [XmlAttribute("version")]
    public string Version { get; set; }
}

/// <summary>
/// App license related data
/// </summary>
public record LicensedApplication
{
    [XmlAttribute("name")]
    public string Name { get; set; }
}

/// <summary>
/// App icon file information
/// </summary>
public record Image
{
    [XmlAttribute("file")]
    public string File { get; set; }
}

/// <summary>
/// CMF App data object for serialization Version 1
/// </summary>
[XmlRoot("App")]
public record CmfAppV1
{
    [XmlAttribute("id")] public string Id { get; set; }
    [XmlAttribute("author")] public string Author { get; set; }
    [XmlAttribute("name")] public string Name { get; set; }
    [XmlAttribute("description")] public string Description { get; set; }
    [XmlElement(Namespace = "urn:cmf:dp:xml:ns:app-metadata-v1")] public Framework Framework { get; set; }
    [XmlElement(Namespace = "urn:cmf:dp:xml:ns:app-metadata-v1")] public LicensedApplication LicensedApplication { get; set; }
    [XmlElement(Namespace = "urn:cmf:dp:xml:ns:app-metadata-v1")] public Image Image { get; set; }
}

/// <summary>
/// CMF App container for data and metadata
/// </summary>
public record AppContainer
{
    public record HeaderRec
    {
        public string Version => "1.0";
        public string Encoding => "utf-8";
    }

    [JsonProperty("?xml")] public HeaderRec Header => new HeaderRec();

    [XmlElement("App")]
    [JsonProperty("App")] public CmfAppV1 App { get; set; }
}

/// <summary>
/// CMF App content data object handler class
/// </summary>
public class CmfApp
{
    public IFileInfo FileInfo { get; private set; }
    public PackageLocation Location { get; private set; }
    public IFileSystem FileSystem { get; private set; }
    public AppContainer Content { get; private set; }
    public string PackageName => Content.App.Id;

    /// <summary>
    /// Loads CmfApp data object from a specified file using file system from the execution context.
    /// </summary>
    /// <param name="file">The file to load the CmfApp from.</param>
    /// <param name="fileSystem">Optional parameter specifying the file system to use for file operations.</param>
    /// <returns>The loaded CmfApp instance.</returns>
    public static CmfApp Load(IFileInfo file, IFileSystem fileSystem = null)
    {
        fileSystem ??= ExecutionContext.Instance.FileSystem;
        if (!file.Exists)
        {
            throw new CliException(string.Format(CoreMessages.NotFound, file.FullName));
        }

        string fileContent = file.ReadToString();

        var appData = JsonConvert.DeserializeObject<AppData>(fileContent);

        CmfAppV1 cmfAppData = new()
        {
            Id = appData.id,
            Author = appData.author,
            Name = appData.name,
            Description = appData.description,
            Framework = new Framework { Version = appData.targetFramework },
            LicensedApplication = new LicensedApplication { Name = appData.licensedApplication },
            Image = new Image { File = appData.icon }
        };

        CmfApp cmfApp = new()
        {
            Content = new AppContainer { App = cmfAppData },
            FileInfo = file,
            Location = PackageLocation.Local,
            FileSystem = fileSystem,
        };

        return cmfApp;
    }

    /// <summary>
    /// Serializes the content of the CmfApp instance and saves it to the specified path as an XML file.
    /// </summary>
    /// <param name="path">The path where the XML file will be saved.</param>
    public void Save(string path)
    {
        XmlSerializer serializer = new(typeof(AppContainer));
        using StringWriter writer = new();

        serializer.Serialize(writer, Content);
        string xmlString = writer.ToString();
        FileSystem.File.WriteAllText(path, xmlString);
    }

    /// <summary>
    /// Saves the icon associated with the CmfApp instance to the specified destination.
    /// </summary>
    /// <param name="iconDestination">The destination path where the icon will be saved.</param>
    public void SaveIcon(string path)
    {
        string iconSource = Content.App.Image.File;

        byte[] iconBytes = FileSystem.File.ReadAllBytes(iconSource);

        FileSystem.File.WriteAllBytes(path, iconBytes);
    }
}

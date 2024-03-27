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
    public string version { get; set; }
    public string name { get; set; }
    public string author { get; set; }
    public string description { get; set; }
    public string targetFramework { get; set; }
    public string licensedApplication { get; set; }
    public string icon { get; set; }
}

public record Framework
{
    [XmlAttribute("version")]
    public string Version { get; set; }
}

public record LicensedApplication
{
    [XmlAttribute("name")]
    public string Name { get; set; }
}

public record Image
{
    [XmlAttribute("file")]
    public string File { get; set; }
}

[XmlRoot("App")]
public record CmfAppV1
{
    [XmlAttribute("id")] public string Id { get; set; }
    [XmlAttribute("version")] public string Version { get; set; }
    [XmlAttribute("author")] public string Author { get; set; }
    [XmlAttribute("name")] public string Name { get; set; }
    [XmlAttribute("description")] public string Description { get; set; }
    [XmlElement(Namespace = "urn:cmf:dp:xml:ns:app-metadata-v1")] public Framework Framework { get; set; }
    [XmlElement(Namespace = "urn:cmf:dp:xml:ns:app-metadata-v1")] public LicensedApplication LicensedApplication { get; set; }
    [XmlElement(Namespace = "urn:cmf:dp:xml:ns:app-metadata-v1")] public Image Image { get; set; }
}

public record CmfAppV1_XmlContainer
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

public class CmfApp
{
    public IFileInfo FileInfo { get; private set; }
    public PackageLocation Location { get; private set; }
    public IFileSystem FileSystem { get; private set; }
    public CmfAppV1_XmlContainer Content { get; private set; }

    /// <summary>
    /// Loads the specified file.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="fileSystem">the underlying file system</param>
    /// <returns></returns>
    /// <exception cref="Cmf.CLI.Utilities.CliException">
    /// </exception>
    /// <exception cref="CliException"></exception>
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
            Version = appData.version,
            Author = appData.author,
            Name = appData.name,
            Description = appData.description,
            Framework = new Framework { Version = appData.targetFramework },
            LicensedApplication = new LicensedApplication { Name = appData.licensedApplication },
            Image = new Image { File = appData.icon }
        };

        CmfApp cmfApp = new()
        {
            Content = new CmfAppV1_XmlContainer { App = cmfAppData },
            FileInfo = file,
            Location = PackageLocation.Local,
            FileSystem = fileSystem,
        };

        return cmfApp;
    }


    public static void Save(CmfApp cmfApp, string path)
    {
        XmlSerializer serializer = new(typeof(CmfAppV1_XmlContainer));
        using StringWriter writer = new();

        serializer.Serialize(writer, cmfApp.Content);
        string xmlString = writer.ToString();
        cmfApp.FileSystem.File.WriteAllText(path, xmlString);
    }

    public static void SaveIcon(CmfApp cmfApp, string iconDestination)
    {
        string iconSource = cmfApp.Content.App.Image.File;

        byte[] iconBytes = cmfApp.FileSystem.File.ReadAllBytes(iconSource);

        cmfApp.FileSystem.File.WriteAllBytes(iconDestination, iconBytes);
    }
}

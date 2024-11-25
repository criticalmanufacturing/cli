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
    public string licensedApplication { get; set; }
    public string icon { get; set; }
}
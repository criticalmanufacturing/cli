using System;
using System.IO.Abstractions;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Utilities;
using NuGet.Versioning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cmf.CLI.Core.Objects;

public interface IProjectConfigService
{
    public ProjectConfig ProjectConfig { get; }
    public ProjectConfig Load(IFileSystem fileSystem);
}

public class ProjectConfigService : IProjectConfigService
{
    private bool? isInsideProject = null;
    public ProjectConfig ProjectConfig { get; private set; }
    public ProjectConfig Load(IFileSystem fileSystem)
    {
        if (System.Environment.GetEnvironmentVariable("cmf_cli_internal_disable_projectconfig_cache") != null || ProjectConfig == null)
        {
            if (System.Environment.GetEnvironmentVariable("cmf_cli_internal_disable_projectconfig_cache") != null || isInsideProject == null)
            {
                var projectCfg = fileSystem.Path.Join(FileSystemUtilities.GetProjectRoot(fileSystem)?.FullName,
                    CoreConstants.ProjectConfigFileName);
                if (!fileSystem.FileInfo.New(projectCfg).Exists)
                {
                    Log.Debug("Running outside a project repository");
                    isInsideProject = false;
                    return null;
                }
                Log.Debug($"Loading .project-config.json");
                this.ProjectConfig = Load(fileSystem.FileInfo.New(projectCfg));
                Log.Debug($"Loaded .project-config.json");
                isInsideProject = true;
            }
        }
        return this.ProjectConfig;
    }

    /// <summary>Loads and validates a configuration file without using the current project cache.</summary>
    /// <param name="projectConfigFile">The configuration file to load.</param>
    /// <returns>The validated project configuration.</returns>
    public ProjectConfig Load(IFileInfo projectConfigFile)
    {
        using var reader = projectConfigFile.OpenText();
        var json = reader.ReadToEnd();
        ProjectConfig projectConfig;
        string mesVersionText = null;
        try
        {
            var jsonObject = JObject.Parse(json);
            var mesVersionProperty = jsonObject.Property(nameof(ProjectConfig.MESVersion), StringComparison.OrdinalIgnoreCase);
            mesVersionText = mesVersionProperty?.Value.ToString();
            var mesVersion = string.IsNullOrWhiteSpace(mesVersionText)
                ? null
                : NuGetVersion.Parse(mesVersionText);
            var projectConfigType = mesVersion?.Major >= 12
                ? typeof(ProjectConfigV2)
                : typeof(ProjectConfigV1);

            // Reuse the version parsed for model selection instead of parsing it again during deserialization.
            mesVersionProperty?.Remove();
            projectConfig = (ProjectConfig)jsonObject.ToObject(projectConfigType);
            projectConfig.MESVersion = mesVersion;
        }
        catch (ArgumentException exception)
        {
            var message = string.Format(CoreMessages.InvalidVersionInput, nameof(ProjectConfig.MESVersion),
                string.Empty, mesVersionText, CoreMessages.NuGetVersionExpectedFormat);
            throw new CliException(string.Format(CoreMessages.InvalidProjectConfiguration, projectConfigFile.FullName, message), exception);
        }
        catch (JsonException exception)
        {
            throw new CliException(string.Format(CoreMessages.InvalidProjectConfiguration, projectConfigFile.FullName, exception.Message), exception);
        }
        return projectConfig;
    }
}

using System.IO.Abstractions;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Utilities;
using Newtonsoft.Json;

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
                var json = fileSystem.File.ReadAllText(projectCfg);
                this.ProjectConfig = JsonConvert.DeserializeObject<ProjectConfig>(json);
                Log.Debug($"Loaded .project-config.json");
                isInsideProject = true;
            }
        }
        return this.ProjectConfig;
    }
}
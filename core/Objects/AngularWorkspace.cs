using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Newtonsoft.Json;

namespace Cmf.CLI.Core.Objects
{
    public class AngularWorkspace
    {
        public Dictionary<string, IDirectoryInfo> PackagesToBuild { get; protected set; }

        private IDirectoryInfo WorkingDirectory;

        private IEnumerable<Project> Libraries;
        private IEnumerable<Project> Applications;

        public AngularWorkspace(IDirectoryInfo cwd)
        {
            WorkingDirectory = cwd;
            LoadProjects();
            PackagesToBuild = GetPackagesToBuild();
        }

        private void LoadProjects()
        {
            var angularjsonStr = File.ReadAllText(WorkingDirectory.GetFiles("angular.json")[0].FullName);
            var angularjson = JsonConvert.DeserializeObject<dynamic>(angularjsonStr);
            var libs = new List<Project>();

            foreach (var project in angularjson.projects)
            {
                libs.Add(new Project(project, WorkingDirectory));
            }
            Libraries = libs.Where(lib => lib.Type == "library");
            Applications = libs.Where(lib => lib.Type == "application");
        }

        private Dictionary<string, IDirectoryInfo> GetPackagesToBuild()
        {
            var projectDeps = Libraries.Select(lib =>
            {
                return new
                {
                    Name = lib.Name,
                    Pkg = lib.PackageJson.name,
                    Deps = lib.Dependencies
                };
            }).ToList();

            var libDeps = new Dictionary<string, List<string>>();
            projectDeps.ForEach(project =>
            {
                libDeps.Add(project.Name, project.Deps.Select(dep =>
                {
                    var projDep = projectDeps.Find(_p => _p.Pkg == dep);
                    return projDep?.Name;
                }).Where(dep => dep != null).ToList());
            });

            // reorder libraries to make sure that are built by the correct order
            var packagesToBuild = new Dictionary<string, IDirectoryInfo>();

            while (libDeps.Count > 0)
            {
                foreach (var (lib, deps) in libDeps)
                {
                    deps.RemoveAll(dep => packagesToBuild.ContainsKey(dep));

                    if (deps.Count == 0)
                    {
                        packagesToBuild[lib] = Libraries.FirstOrDefault(l => l.Name.Equals(lib)).Directory;
                        libDeps.Remove(lib);
                    }
                }
            }

            // add application packages
            foreach (var app in Applications)
            {
                packagesToBuild[app.Name] = app.Directory;
            }

            return packagesToBuild;
        }
    }

    public class Project
    {
        public string Name { get; protected set; }
        public string Type { get; protected set; }

        public IDirectoryInfo Directory { get; protected set; }

        public dynamic PackageJson;

        public List<string> Dependencies { get; protected set; }

        public Project(dynamic project, IDirectoryInfo cwd)
        {
            Name = project.Name;
            Type = project.Value.projectType?.ToString();
            GetPackageJson();
            GetDependencies();
            Log.Debug($"Loaded package {Name}");
        }

        private void LoadPackageJson(dynamic project, IDirectoryInfo cwd)
        {
            string packageJsonPath = "package.json";
            if (!string.IsNullOrEmpty(project.Value.root?.ToString()))
            {
                packageJsonPath = $"{project.Value.root?.ToString()}/{packageJsonPath}";
            }

            var packageJsonFile = cwd.GetFiles(packageJsonPath).FirstOrDefault();
            var packagejsonStr = File.ReadAllText(packageJsonFile?.FullName);
            PackageJson = JsonConvert.DeserializeObject<dynamic>(packagejsonStr);

            Directory = packageJsonFile?.Directory;
        }

        private void LoadDependencies()
        {
            var peerDependencies = PackageJson.peerDependencies?.ToObject<Dictionary<string, string>>().Keys;
            var dependencies = PackageJson.dependencies.ToObject<Dictionary<string, string>>().Keys;
            Dependencies = new List<string>();
            Dependencies.AddRange(dependencies);
            Dependencies.AddRange(peerDependencies ?? new List<string>());

            Log.Debug($"Loaded dependencies from package {Name}:\n{string.Join("\n", Dependencies)}");
        }
    }

    public class PackageJson
    {
        public IFileInfo File { get; protected set; }

        public dynamic Content { get; protected set; }

        public PackageJson(IFileInfo file)
        {
            File = file;
            var packagejsonStr = System.IO.File.ReadAllText(File?.FullName);
            Log.Debug($"Loading file {File?.FullName}");
            Content = JsonConvert.DeserializeObject<dynamic>(packagejsonStr);
        }
    }
}
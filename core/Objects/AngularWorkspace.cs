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
        public IEnumerable<Project> Projects;

        private IDirectoryInfo WorkingDirectory;
        private IEnumerable<Project> Libraries;
        private IEnumerable<Project> Applications;

        public AngularWorkspace(IDirectoryInfo cwd)
        {
            WorkingDirectory = cwd;
            Projects = GetProjects();
            PackagesToBuild = GetPackagesToBuild();
        }

        private IEnumerable<Project> GetProjects()
        {
            var angularjsonStr = File.ReadAllText(WorkingDirectory.GetFiles("angular.json")[0].FullName);
            var angularjson = JsonConvert.DeserializeObject<dynamic>(angularjsonStr);
            var projects = new List<Project>();

            foreach (var project in angularjson.projects)
            {
                projects.Add(new Project(project, WorkingDirectory));
            }

            Libraries = projects.Where(lib => lib.Type == "library");
            Applications = projects.Where(lib => lib.Type == "application");

            return projects;
        }

        private Dictionary<string, IDirectoryInfo> GetPackagesToBuild()
        {
            var projectDeps = Libraries.Select(lib =>
            {
                return new
                {
                    Name = lib.Name,
                    Pkg = lib.PackageJson.Content.name,
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
                        packagesToBuild[lib] = Libraries.FirstOrDefault(l => l.Name.Equals(lib)).PackageJson.File.Directory;
                        libDeps.Remove(lib);
                    }
                }
            }

            // add application packages
            foreach (var app in Applications)
            {
                packagesToBuild[app.Name] = app.PackageJson.File.Directory;
            }

            return packagesToBuild;
        }
    }

    public class Project
    {
        public string Name { get; protected set; }
        public string Type { get; protected set; }
        public PackageJson PackageJson { get; protected set; }
        public PackageJson PackageLock { get; protected set; }
        public List<string> Dependencies { get; protected set; }

        protected dynamic _Project;
        protected IDirectoryInfo _Cwd;

        public Project(dynamic project, IDirectoryInfo cwd)
        {
            _Project = project;
            _Cwd = cwd;
            Name = project.Name;
            Type = project.Value.projectType?.ToString();
            GetPackageJson();
            GetDependencies();
            Log.Debug($"Loaded package {Name}");
        }

        private void GetPackageJson()
        {
            // package.json
            string packageJsonPath = "package.json";
            if (!string.IsNullOrEmpty(_Project.Value.root?.ToString()))
            {
                packageJsonPath = $"{_Project.Value.root?.ToString()}/{packageJsonPath}";
            }
            var packageJsonFile = _Cwd.GetFiles(packageJsonPath).FirstOrDefault();
            PackageJson = new PackageJson(packageJsonFile);

            // package-lock.json
            string packageLockPath = "package-lock.json";
            if (!string.IsNullOrEmpty(_Project.Value.root?.ToString()))
            {
                packageLockPath = $"{_Project.Value.root?.ToString()}/{packageLockPath}";
            }
            var packageLockFile = _Cwd.GetFiles(packageLockPath).FirstOrDefault();

            // In some cases we don't have the package-lockW
            if(packageLockFile != null)
            {
                PackageLock = new PackageJson(packageLockFile);
            }         
        }

        private void GetDependencies()
        {
            var peerDependencies = PackageJson.Content.peerDependencies?.ToObject<Dictionary<string, string>>().Keys;
            var dependencies = PackageJson.Content.dependencies.ToObject<Dictionary<string, string>>().Keys;
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
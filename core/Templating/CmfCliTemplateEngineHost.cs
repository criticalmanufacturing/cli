using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Abstractions.PhysicalFileSystem;
using Microsoft.TemplateEngine.Utils;
using System;
using System.Collections.Generic;

namespace Cmf.CLI.Core.Templating
{
    /// <summary>
    /// Implementation of <see cref="ITemplateEngineHost"/>, based on the 
    /// <see cref="Microsoft.TemplateEngine.Edge.DefaultTemplateEngineHost"/>, with the difference that this 
    /// one allows passing an initial file system (that does not have to be the real <see cref="PhysicalFileSystem"/>).
    /// </summary>
    public class CmfCliTemplateEngineHost : ITemplateEngineHost
    {
        private static readonly IReadOnlyList<(Type, IIdentifiedComponent)> NoComponents = Array.Empty<(Type, IIdentifiedComponent)>();
        private readonly IReadOnlyDictionary<string, string> _hostDefaults;
        private readonly IReadOnlyList<(Type InterfaceType, IIdentifiedComponent Instance)> _hostBuiltInComponents;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        public CmfCliTemplateEngineHost(
            IPhysicalFileSystem fileSystem,
            string hostIdentifier,
            string version,
            Dictionary<string, string> defaults = null,
            IReadOnlyList<(Type InterfaceType, IIdentifiedComponent Instance)> builtIns = null,
            IReadOnlyList<string> fallbackHostTemplateConfigNames = null,
            ILoggerFactory loggerFactory = null)
        {
            HostIdentifier = hostIdentifier;
            Version = version;
            _hostDefaults = defaults ?? new Dictionary<string, string>();
            FileSystem = fileSystem;
            _hostBuiltInComponents = builtIns ?? NoComponents;
            FallbackHostTemplateConfigNames = fallbackHostTemplateConfigNames ?? new List<string>();

            loggerFactory ??= NullLoggerFactory.Instance;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger("Template Engine") ?? NullLogger.Instance;
        }

        public IPhysicalFileSystem FileSystem { get; private set; }

        public string HostIdentifier { get; }

        public IReadOnlyList<string> FallbackHostTemplateConfigNames { get; }

        public string Version { get; }

        public virtual IReadOnlyList<(Type InterfaceType, IIdentifiedComponent Instance)> BuiltInComponents => _hostBuiltInComponents;

        public ILogger Logger => _logger;

        public ILoggerFactory LoggerFactory => _loggerFactory;

        // stub that will be built out soon.
        public virtual bool TryGetHostParamDefault(string paramName, out string value)
        {
            switch (paramName)
            {
                case "HostIdentifier":
                    value = HostIdentifier;
                    return true;
            }

            return _hostDefaults.TryGetValue(paramName, out value);
        }

        public void VirtualizeDirectory(string path)
        {
            FileSystem = new InMemoryFileSystem(path, FileSystem);
        }

        public void Dispose()
        {
            _loggerFactory?.Dispose();
        }

        #region Obsoleted
        [Obsolete]
        bool ITemplateEngineHost.OnPotentiallyDestructiveChangesDetected(IReadOnlyList<IFileChange> changes, IReadOnlyList<IFileChange> destructiveChanges)
        {
            return true;
        }
        #endregion
    }
}

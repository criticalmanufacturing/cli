using Cmf.CLI.Core;
using Cmf.CLI.Core.Attributes;
using Cmf.CLI.Core.Commands;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Enums;
using Cmf.CLI.Core.Interfaces;
using Cmf.CLI.Core.Objects;
using Cmf.CLI.Core.Repository.Credentials;
using Cmf.CLI.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace Cmf.CLI.Commands
{
    /// <summary>
    /// Synchronizes the credentials that are stored on the .cmf-auth.json file into the respective tools' configuration files
    /// </summary>
    [CmfCommand("sync", Id = "login_sync", ParentId = "login", Description = "Sync credentials from the .cmf-auth.json file into each specific tool (npm, nuget, docker, etc...) configuration files.")]
    public class SyncCommand : BaseCommand
    {
        /// <summary>
        /// constructor
        /// </summary>
        public SyncCommand() : base()
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem"></param>
        public SyncCommand(IFileSystem fileSystem) : base(fileSystem)
        {
        }

        /// <summary>
        /// configure command signature
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            // Add the handler
            cmd.Handler = CommandHandler.Create(Execute);
        }

        /// <summary>
        /// Synchronous wrapper for the command
        /// </summary>
        internal void Execute(RepositoryCredentialsType? repositoryType, string repository)
        {
            ExecuteAsync(repositoryType, repository).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        internal async Task ExecuteAsync(RepositoryCredentialsType? repositoryType, string repository)
        {
            using var activity = ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);

            var authStore = ExecutionContext.ServiceProvider.GetService<IRepositoryAuthStore>();

            var authFile = await authStore.Load();
            authStore.AddDerivedCredentials(authFile);

            RepositoryCredentialsType[] resolvedRepositoryTypes;

            if (repositoryType == null)
            {
                Log.Debug($"No repository type provided, will use all repositories defined in our .cmf-auth.json file (and derived credentials)");
                resolvedRepositoryTypes = authFile.Repositories.Keys.ToArray();
            }
            else
            {
                resolvedRepositoryTypes = [repositoryType.Value];
            }

            Log.Debug($" > Syncing repository types: {string.Join(", ", resolvedRepositoryTypes)}");

            foreach (var repoType in resolvedRepositoryTypes)
            {
                if (!authFile.Repositories.TryGetValue(repoType, out var repoData) || repoData.Credentials.Count <= 0)
                {
                    Log.Warning($"File .cmf-auth.json does not contain any credentials for repository {repoType}");
                    continue;
                }

                var repo = authStore.GetRepositoryType(repoType);

                var credentials = new List<ICredential>();

                // If a specific repository URL was given, make sure to only sync the credentials for that repo
                if (repository != null)
                {
                    credentials.AddRange(repoData.Credentials.Where(repoCred => repoCred.Repository == repository));
                }
                else
                {
                    credentials.AddRange(repoData.Credentials);
                }

                Log.Debug($"Syncing {credentials.Count} credentials for repository type {repoType}");

                if (credentials.Any())
                {
                    await repo.SyncCredentials(credentials);
                }
            }
        }
    }
}

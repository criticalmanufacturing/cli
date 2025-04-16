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
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace Cmf.CLI.Commands
{
    /// <summary>
    /// Saves the credentials for access into one or more package repositories. If no arguments are passed, uses `cmf portal login` to get a Customer Portal token,
    /// and uses that to algo login into Critical Manufacturing's NPM, NuGet and Docker registries.
    /// </summary>
    [CmfCommand("login", Id = "login", Description = "Login into one or more package respositories")]
    public class LoginCommand : BaseCommand
    {
        /// <summary>
        /// constructor
        /// </summary>
        public LoginCommand() : base()
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileSystem"></param>
        public LoginCommand(IFileSystem fileSystem) : base(fileSystem)
        {
        }

        /// <summary>
        /// configure command signature
        /// </summary>
        /// <param name="cmd"></param>
        public override void Configure(Command cmd)
        {
            cmd.AddArgument(new Argument<RepositoryCredentialsType?>(
                name: "repositoryType", description: "Type of repository for login (values: portal, docker, npm, nuget, cifs)"
            ) { Arity = ArgumentArity.ZeroOrOne });
            cmd.AddArgument(new Argument<string>(
                name: "repository", description: "URL of repository for login"
            ) { Arity = ArgumentArity.ZeroOrOne });

            cmd.AddOption(new Option<AuthType>(
                aliases: new[] { "-T", "--auth-type" },
                description: "Type of authentication type to use (only needed if the repository type supports more than one)"
            ));
            cmd.AddOption(new Option<string>(
                aliases: new[] { "-t", "--token" },
                description: "Token used for this, used when the auth type is Bearer"
            ));
            cmd.AddOption(new Option<string>(
                aliases: new[] { "-u", "--username" },
                description: "Account username, used when the auth type is Basic"
            ));
            cmd.AddOption(new Option<string>(
                aliases: new[] { "-p", "--password" },
                description: "Account password, used when the auth type is Basic"
            ));
            cmd.AddOption(new Option<string>(
                aliases: new[] { "-d", "--domain" },
                description: "For repositories that support it, the domain to use when logging in."
            ));
            cmd.AddOption(new Option<string>(
                aliases: new[] { "-k", "--key" },
                description: "For repositories that support it, the key under which we should store the credential"
            ));
            cmd.AddOption(new Option<bool>(
                aliases: new[] { "--store-only" },
                description: "If true, the credentials are stord on the .cmf-auth.json file, but are not applied to the credentials file of the tool (NPM, NuGet, Docker, etc...)"
            ));
            cmd.AddOption(new Option<bool>(
                aliases: new[] { "--no-prompt" },
                description: "Do not display any interactive prompts. If a prompt was needed, an error will be raised instead"
            ));

            // Add the handler
            cmd.Handler = CommandHandler.Create(Execute);
        }

        /// <summary>
        /// Synchronous wrapper for the command
        /// </summary>
        internal void Execute(RepositoryCredentialsType? repositoryType, string repository, AuthType? authType, string token, string username, string password, string domain, string key, bool storeOnly, bool noPrompt)
        {
            ExecuteAsync(repositoryType, repository, authType, token, username, password, domain, key, storeOnly, noPrompt).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        internal async Task ExecuteAsync(RepositoryCredentialsType? repositoryType, string repository, AuthType? authType, string token, string username, string password, string domain, string key, bool storeOnly, bool noPrompt)
        {
            using var activity = ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);

            ICredential credentials;

            var authStore = ExecutionContext.ServiceProvider.GetService<IRepositoryAuthStore>();

            if (repositoryType == null)
            {
                repositoryType = RepositoryCredentialsType.Portal;
            }

            if (repositoryType == RepositoryCredentialsType.Portal &&
                string.IsNullOrEmpty(repository))
            {
                repository = CmfAuthConstants.PortalRepository;
            }

            // We find the repository type implementation for the repo type
            var repositoryCredentials = authStore.GetRepositoryType(repositoryType.Value);

            if (repositoryCredentials is IRepositoryAutomaticLogin automaticLoginRepo
                && authType == null && username == null && password == null && token == null)
            {
                credentials = await automaticLoginRepo.AutomaticLogin();
            }
            else
            {
                var supportedAuthTypes = repositoryCredentials.SupportedAuthTypes;

                // This should never really happen, if it does, it is most likely an implementation error. We still give out a nice exception message
                // to make it easier if users need to report this issue
                if (supportedAuthTypes == null || !supportedAuthTypes.Any())
                {
                    throw new CliException($"Repository type \"{repositoryCredentials.RepositoryType}\" does not support any authentication method, please report this issue.");
                }

                // This can happen, if the repo supports multiple auth types, and the user did not provide one
                if (supportedAuthTypes.Length > 1 && authType == null)
                {
                    var supportedAuthTypeNames = string.Join(", ", supportedAuthTypes);

                    throw new CliException($"Missing mandatory auth type for repository type \"{repositoryCredentials.RepositoryType}\", supported values are: {supportedAuthTypeNames}.", ErrorCode.InvalidArgument);
                }

                if (authType != null && !supportedAuthTypes.Contains(authType.Value))
                {
                    var supportedAuthTypeNames = string.Join(", ", supportedAuthTypes);

                    throw new CliException($"Invalid auth type \"{authType.Value}\" for repository type \"{repositoryCredentials.RepositoryType}\", supported values are: {supportedAuthTypeNames}.", ErrorCode.InvalidArgument);
                }

                if (authType == null)
                {
                    // We must only reach this line if we validated before that `supportedAuthTypes` has 1 and only 1 auth type
                    authType = supportedAuthTypes.Single();
                }
                
                GenericUtilities.ValidatePropertyRequirement($"Option \"key\"", key, repositoryCredentials.KeyPropertyRequirement);

                if (authType.Value == AuthType.Basic)
                {
                    if (username == null)
                    {
                        username = Prompt("Username", noPrompt);
                    }

                    if (password == null)
                    {
                        password = Prompt("Username", noPrompt);
                    }

                    GenericUtilities.ValidatePropertyRequirement($"Option \"domain\"", domain, repositoryCredentials.DomainPropertyRequirement);

                    credentials = new BasicCredential(repositoryCredentials.RepositoryType, repository, key, domain, username, password);
                }
                else if (authType.Value == AuthType.Bearer)
                {
                    if (token == null)
                    {
                        token = Prompt("Token", noPrompt);
                    }

                    credentials = new BearerCredential(repositoryCredentials.RepositoryType, repository, key, token);
                }
                else
                {
                    throw new CliException($"Unhandled auth type \"{authType}\", please report this issue.");
                }
            }

            // Store the credentials on the auth
            await authStore.Save([credentials], sync: !storeOnly);
        }

        internal string Prompt(string label, bool noPrompt)
        {
            if (noPrompt)
            {
                throw new Exception($"Missing command argument for \"{label}\"");
            }

            Console.Write(label + ": ");

            return Console.ReadLine();
        }
    }
}

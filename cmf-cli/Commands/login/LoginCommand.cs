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
using System.CommandLine.Parsing;
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
            var repositoryTypeArgument = new Argument<RepositoryCredentialsType?>("repositoryType")
            {
                Description = "Type of repository for login",
                Arity = ArgumentArity.ZeroOrOne
            };
            cmd.Arguments.Add(repositoryTypeArgument);

            var repositoryArgument = new Argument<string>("repository")
            {
                Description = "URL of repository for login",
                Arity = ArgumentArity.ZeroOrOne
            };
            cmd.Arguments.Add(repositoryArgument);

            var authTypeOption = new Option<AuthType?>("--auth-type", "-T")
            {
                Description = "Type of authentication type to use (only needed if the repository type supports more than one)"
            };
            cmd.Options.Add(authTypeOption);

            var tokenOption = new Option<string>("--token", "-t")
            {
                Description = "Token used for this, used when the auth type is Bearer"
            };
            cmd.Options.Add(tokenOption);

            var usernameOption = new Option<string>("--username", "-u")
            {
                Description = "Account username, used when the auth type is Basic"
            };
            cmd.Options.Add(usernameOption);

            var passwordOption = new Option<string>("--password", "-p")
            {
                Description = "Account password, used when the auth type is Basic"
            };
            cmd.Options.Add(passwordOption);

            var domainOption = new Option<string>("--domain", "-d")
            {
                Description = "For repositories that support it, the domain to use when logging in."
            };
            cmd.Options.Add(domainOption);

            var keyOption = new Option<string>("--key", "-k")
            {
                Description = "For repositories that support it, the key under which we should store the credential"
            };
            cmd.Options.Add(keyOption);

            var storeOnlyOption = new Option<bool>("--store-only")
            {
                Description = "If true, the credentials are stord on the .cmf-auth.json file, but are not applied to the credentials file of the tool (NPM, NuGet, Docker, etc...)"
            };
            cmd.Options.Add(storeOnlyOption);

            var noPromptOption = new Option<bool>("--no-prompt")
            {
                Description = "Do not display any interactive prompts. If a prompt was needed, an error will be raised instead"
            };
            cmd.Options.Add(noPromptOption);

            // Add the handler
            cmd.SetAction((parseResult, cancellationToken) =>
            {
                var repositoryType = parseResult.GetValue(repositoryTypeArgument);
                var repository = parseResult.GetValue(repositoryArgument);
                var authType = parseResult.GetValue(authTypeOption);
                var token = parseResult.GetValue(tokenOption);
                var username = parseResult.GetValue(usernameOption);
                var password = parseResult.GetValue(passwordOption);
                var domain = parseResult.GetValue(domainOption);
                var key = parseResult.GetValue(keyOption);
                var storeOnly = parseResult.GetValue(storeOnlyOption);
                var noPrompt = parseResult.GetValue(noPromptOption);

                Execute(repositoryType, repository, authType, token, username, password, domain, key, storeOnly, noPrompt);
                return Task.FromResult(0);
            });
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
            using var activity = Core.Objects.ExecutionContext.ServiceProvider?.GetService<ITelemetryService>()?.StartExtendedActivity(this.GetType().Name);

            ICredential credentials;

            var authStore = Core.Objects.ExecutionContext.ServiceProvider.GetService<IRepositoryAuthStore>();

            if (repositoryType == null)
            {
                repositoryType = RepositoryCredentialsType.Portal;
            }

            if (repositoryType == RepositoryCredentialsType.Portal &&
                string.IsNullOrEmpty(repository))
            {
                repository = CmfAuthConstants.PortalRepository;
            }
            else if (string.IsNullOrEmpty(repository))
            {
                throw new CliException($"Missing mandatory {repositoryType} repository URL.");
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
                        password = Prompt("Password", noPrompt);
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
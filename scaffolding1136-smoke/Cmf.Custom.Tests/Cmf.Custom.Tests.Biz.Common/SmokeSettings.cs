using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using Cmf.LightBusinessObjects.Infrastructure;
using Cmf.LightBusinessObjects.Infrastructure.Security.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cmf.Custom.Tests.Biz.Common
{
    /// <summary>Settings for read-only smoke tests against the nightly environment.</summary>
    public static class SmokeSettings
    {
        /// <summary>The default environment under test.</summary>
        public const string DefaultEnvironment = "https://nightly-dev-113-mes.apps.rhos.cm-mes.dev/";

        /// <summary>Gets a secret or setting from the test process environment.</summary>
        /// <param name="name">Environment variable name.</param>
        /// <returns>The configured value.</returns>
        public static string Required(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(value))
            {
                Assert.Inconclusive($"Set {name} before running this authenticated smoke test.");
            }
            return value;
        }

        /// <summary>Gets the website address, allowing an explicit environment override.</summary>
        public static Uri Website => new Uri(
            Environment.GetEnvironmentVariable("CMF_TEST_URL") ?? DefaultEnvironment);

        /// <summary>Gets the MES user account to retrieve through the Host.</summary>
        public static string UserAccount =>
            Environment.GetEnvironmentVariable("CMF_TEST_USER_ACCOUNT") ?? Required("CMF_TEST_USERNAME");

        /// <summary>Configures the MES 11 LBO client using an authenticated browser token.</summary>
        public static void ConfigureLboClient()
        {
            _ = Required("CMF_TEST_USERNAME");
            var tenant = Required("CMF_TEST_TENANT");
            var accessToken = Required("CMF_TEST_ACCESS_TOKEN");
            var host = new Uri(Environment.GetEnvironmentVariable("CMF_TEST_HOST_URL") ?? Website.AbsoluteUri);
            if (host.Scheme != Uri.UriSchemeHttps)
            {
                throw new InvalidOperationException("CMF_TEST_HOST_URL must use HTTPS.");
            }

            // Configure the Host separately from the browser's authentication flow.
            // Legacy UserName/Password fields do not select an auth provider in this LBO.
            var configuration = new ClientConfiguration
            {
                HostAddress = host.Authority + host.AbsolutePath.TrimEnd('/'),
                UseSsl = true,
                IsUsingLoadBalancer = false,
                ClientTenantName = tenant,
                ApplicationName = "NightlyReadOnlySmoke",
                DelegatingHandler = new BrowserGatewayHandler(accessToken),
                RequestTimeout = "00:01:00"
            };
            ClientConfigurationProvider.ConfigurationFactory = () => configuration;
            ClientConfigurationProvider.AuthProviderFactory =
                () => new BrowserSessionAuthProvider(accessToken);
        }

        // The runner obtains this token through the real username/password browser
        // sign-in. It exists only in process memory/environment, never a test file.
        private sealed class BrowserSessionAuthProvider(string token) : IAuthProvider
        {
            public bool IsAuthenticated => true;
            public string AccessToken => token;
            public string RefreshToken => null;
            public string Type => "Bearer";
            public Task Authenticate() => Task.CompletedTask;
            public Task RenewAuthentication() => Task.FromException(
                new InvalidOperationException(
                    "The Host rejected the browser token. Check the Host URL, tenant and token audience; automatic renewal is not configured."));
        }

        private sealed class BrowserGatewayHandler(string token) : DelegatingHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // This environment exposes the Host through the authenticated website
                // gateway. Reuse only the approved access_token, not other browser cookies.
                if (request.RequestUri.Host.Equals(Website.Host, StringComparison.OrdinalIgnoreCase))
                {
                    request.Headers.TryAddWithoutValidation("Cookie",
                        "access_token=" + Uri.EscapeDataString(token));
                }
                Console.WriteLine($"Host request: {request.Method} {request.RequestUri.GetLeftPart(UriPartial.Path)}");
                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                Console.WriteLine($"Host response: {(int)response.StatusCode}");
                return response;
            }
        }
    }
}

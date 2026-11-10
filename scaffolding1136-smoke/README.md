# MES 11.3.6 nightly smoke tests

Default target: https://nightly-dev-113-mes.apps.rhos.cm-mes.dev/

For the implementation report, browser setup, recorded results and step-by-step
commands, see [TESTING.md](TESTING.md).

This separate cmf-new-test scaffold contains real MSTest methods. CMF test
packages are pinned to exactly [11.3.6] so restore cannot silently select MES 12.
Local LBO inputs come from the existing MES 11.3.6 scaffold.

## Tests

| Project | Test | Behavior |
| --- | --- | --- |
| Biz | GetUserByAccount_ReturnsRequestedUser | Calls the Host through GetUserByUserAccountInput and checks the returned account and ID. |
| Biz | GetUserById_MatchesUserRetrievedByAccount | Reads the account, then calls GetUserByIdInput and compares the returned identity. |
| GUI | LoginPage_ShowsEnabledAccountForm | Opens the environment and checks the Security Portal account form through a CMF page object. No credentials needed. |
| GUI | SignIn_OpensMesShell | Submits the configured account/password and checks that navigation reaches the MES shell. |
| GUI | Refresh_PreservesAuthenticatedSession | Signs in, refreshes the MES page, and checks that the shell remains available. |

The tests do not create/update/delete MES entities or assign roles. The generated
assembly initializers were adapted to avoid the CMF default user/role setup.
Sign-in creates an ordinary browser/authentication session.

## Configure authentication

Set credentials in the terminal/process that runs dotnet test. No secret is read
from the original env.json or embedded in the test files.

    export CMF_TEST_USERNAME='your-test-account'
    read -rsp 'MES password: ' CMF_TEST_PASSWORD
    echo
    export CMF_TEST_PASSWORD
    export CMF_TEST_TENANT='the-actual-tenant-name'

CMF_TEST_TENANT is required for Host tests; the local scaffold's original tenant
is not assumed to describe the remote nightly environment.

The authenticated retry identified the Security Portal tenant as MesDevelopment.
It also showed that this LBO build needs a bearer token: username/password fields
alone do not select an authentication provider. SmokeRunner signs
in through the browser and hands its session token to the Host test process in
memory. With the explicit token-reuse approval, both Host tests passed. The
same-origin website gateway also requires the access_token cookie; the handler
supplies only that approved token, alongside bearer authentication.

| Optional variable | Meaning |
| --- | --- |
| CMF_TEST_URL | Website override; defaults to the URL above. |
| CMF_TEST_HOST_URL | Actual externally reachable Host/REST base address if it differs from the website origin. HTTPS is required. Confirm this route for authenticated LBO execution. |
| CMF_TEST_USER_ACCOUNT | MES user account to retrieve if it differs from the login name. |
| CMF_TEST_ACCESS_TOKEN | Bearer token supplied to Host tests by SmokeRunner, only in the child process environment. |
| CMF_CHROME_BINARY | Full path to Chrome when not installed in the default location. |
| CMF_CHROMEDRIVER_DIRECTORY | Directory containing a compatible ChromeDriver; otherwise the generated driver is used. |
| CMF_WEBDRIVER_URL | Optional Selenium Grid/remote WebDriver endpoint. |
| CMF_TEST_SHELL_SELECTOR | Authenticated MES shell selector, if this environment differs from cmf-core-shell-shell. |

The login page object uses the observed username input and subsequent password
form. It refuses to submit credentials after navigation to a different hostname.

## Build and run

From this directory:

    cmf build Cmf.Custom.Tests

Public browser check without credentials:

    dotnet test Cmf.Custom.Tests/Cmf.Custom.Tests.GUI/Cmf.Custom.Tests.GUI.csproj --configuration Release --no-build --no-restore --settings Cmf.Custom.Tests/smoke.runsettings --filter 'TestCategory=UiPublic' --results-directory Cmf.Custom.Tests/TestResults --logger 'trx;LogFileName=ui-public.trx'

Authenticated Host requests:

Set CMF_TEST_TENANT=MesDevelopment and run the tested browser-to-Host flow:

    dotnet run --project SmokeRunner/SmokeRunner.csproj --configuration Release

All three browser checks:

    dotnet test Cmf.Custom.Tests/Cmf.Custom.Tests.GUI/Cmf.Custom.Tests.GUI.csproj --configuration Release --no-build --no-restore --settings Cmf.Custom.Tests/smoke.runsettings --filter 'TestCategory=NightlySmoke' --results-directory Cmf.Custom.Tests/TestResults --logger 'trx;LogFileName=ui-smoke.trx'

Authenticated tests report inconclusive/skipped rather than passed when required
environment variables are absent. Build/discovery do not prove an authenticated
request succeeded. Use smoke.runsettings; the original generated runsettings
contain local-environment defaults.

## Validation performed

- Solution build: passed with zero errors.
- Discovery: two Host tests and three UI tests.
- Public login-page test: passed against the configured nightly URL using
  Chrome/ChromeDriver 147.0.7727.56 and the CMF page-object library.
- Authenticated UI tests: both passed after handling the optional device prompt
  by selecting "No, thanks".
- Authenticated Host tests: both passed through SmokeRunner, using the approved
  browser token as bearer authentication and the gateway access_token cookie.
- Host requests returned HTTP 200 for boot information, user-by-account and user-by-ID.
- The live page title advertises MES 11.3.7; client/test packages remain 11.3.6.

TRX files are under Cmf.Custom.Tests/TestResults. The test browser downloaded
for this devcontainer is /tmp/cmf-browser/chrome-linux64/chrome; set
CMF_CHROME_BINARY to that path to repeat the public browser check here.
Use an installed compatible browser or a remote WebDriver elsewhere.

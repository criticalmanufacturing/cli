# MES 11.3.6 smoke tests: work performed and commands

## Target and result

Target environment: https://nightly-dev-113-mes.apps.rhos.cm-mes.dev/

The requested version was corrected from 11.3.7 to **11.3.6**. The scaffold
is under /workspaces/cli/scaffolding1136-smoke.

| Check | Actual result |
| --- | --- |
| Build the complete generated test solution | Passed, zero errors |
| Discover Host tests | Two tests discovered |
| Discover UI tests | Three tests discovered |
| Open the nightly login page in Chrome using a CMF page object | Passed |
| Authenticated Host requests | Both passed using the approved browser-session token and gateway cookie |
| Authenticated UI sign-in and refresh | Both passed after declining the optional device-registration prompt |

**All four authenticated checks have passed.** The live UI title advertises
MES 11.3.7 while client/test
packages remain pinned to the requested 11.3.6.

## What was created

The starting point was normal CLI initialization and test scaffolding. Custom
MSTest methods were then added; cmf new test alone does not create these tests.

| File, relative to Cmf.Custom.Tests | Purpose |
| --- | --- |
| Cmf.Custom.Tests.Biz.Common/SmokeSettings.cs | Reads environment variables and configures the MES 11 LBO client. |
| Cmf.Custom.Tests.Biz/Tests/HostSmokeTests.cs | Makes read-only Host calls using LBO orchestration inputs. |
| Cmf.Custom.Tests.GUI.PageObjects/NightlyLoginPage.cs | Uses CMF BasePageObject for the observed Security Portal login form. |
| Cmf.Custom.Tests.GUI/Tests/NightlyUiSmokeTests.cs | Implements public login-page, authenticated sign-in and session-refresh checks. |
| Cmf.Custom.Tests.Biz/Context.cs and Cmf.Custom.Tests.GUI/Context.cs | Initialize culture without invoking the generated default login/role setup. |
| smoke.runsettings | Sets one test process at a time and a ten-minute session limit. |
| ../SmokeRunner/Program.cs | Browser sign-in and in-memory bearer-token handoff to the Host tests. |

The five methods are:

| Method | What it checks |
| --- | --- |
| GetUserByAccount_ReturnsRequestedUser | Calls GetUserByUserAccountInput.GetUserByUserAccountAsync and checks the returned account and persisted ID. |
| GetUserById_MatchesUserRetrievedByAccount | Reads the account, then calls GetUserByIdInput.GetUserByIdAsync and compares identities. |
| LoginPage_ShowsEnabledAccountForm | Checks the visible account field and submit button. No credentials needed. |
| SignIn_OpensMesShell | Submits the configured credentials and waits for the MES shell. |
| Refresh_PreservesAuthenticatedSession | Signs in, refreshes the page and checks that the MES shell remains available. |

The tests do not explicitly create/update/delete MES entities or assign roles.
Sign-in creates normal sessions and may produce normal authentication/audit records.

## How I got it running

1. Used the locally built CLI at /workspaces/cli/cmf-cli/bin/Release/cmf.
2. Set MES and TestScenarios to 11.3.6 and pinned the generated CMF package
   references to [11.3.6]. The brackets enforce an exact NuGet version and prevent
   fallback to MES 12 packages.
3. Copied the retained LBO inputs from the existing MES 11.3.6 scaffold. They
   were existing matching LBOs, not newly generated from the nightly environment.
4. Adapted the generated assembly initializers to avoid CMF's default user/role
   setup, and added real MSTest methods.
5. Used environment variables for credentials. Missing required values produce
   inconclusive/skipped results instead of a false pass.
6. Downloaded Chrome for Testing 147.0.7727.56, matching the generated ChromeDriver.
   The devcontainer originally had no browser or its Linux runtime libraries, so
   those libraries were installed.
7. Built the solution, verified discovery, and ran the public browser check
   against the actual nightly URL.

On the authenticated retry, the supplied account reached a device-registration
prompt. The page object now selects "No, thanks", then both authenticated UI
checks pass. The observed Security Portal tenant is MesDevelopment.

The LBO Host retry exposed a separate harness problem: the legacy username and
password configuration properties do not select an auth provider in this LBO
build. With explicit authorization, SmokeRunner now supplies the browser's
authenticated token to the Host tests in memory. The website gateway requires
the access_token cookie as well as bearer authentication. The same-origin handler
supplies that one approved token and does not copy other browser cookies.
After this adjustment, both read-only Host tests passed.

The retained local DLL inputs are:

```text
Libs/LBOs/NetStandard/Cmf.LightBusinessObjects.dll
Libs/LBOs/NetStandard/Libs/Cmf.LoadBalancing.dll
Libs/LBOs/NetStandard/Libs/Cmf.MessageBus.Client.dll
Libs/LBOs/NetStandard/Libs/System.Net.Http.Formatting.dll
```

No local Newtonsoft.Json DLL was added: the CMF package dependencies supply it.
GUI.PageObjects retains its local Http.Formatting input.

The public test exercised the real browser and the CMF page-object library.
No username or password was submitted during that passing check.

## Commands to run the existing tests

Commands below use **Bash inside the repository's devcontainer**. Start Bash
first if your terminal uses another shell.

### 1. Open the test scaffold and check SDKs

```bash
bash
cd /workspaces/cli/scaffolding1136-smoke
test -f /.dockerenv
dotnet --list-sdks
dotnet --list-runtimes
dotnet --version
```

The tests target net8.0. The successful run used SDK 8.0.427 and runtime 8.0.33.
Building the CLI itself requires .NET 10; SDK 10.0.403 was available.
Run these commands in the devcontainer rather than installing SDKs on the host.

### 2. Build the local CLI only if its executable is missing

If /workspaces/cli/cmf-cli/bin/Release/cmf exists, skip this step.

```bash
cd /workspaces/cli
dotnet restore cmf-cli/cmf.csproj
dotnet build cmf-cli/cmf.csproj --configuration Release --no-restore
cd /workspaces/cli/scaffolding1136-smoke
```

### 3. Build the test solution

```bash
/workspaces/cli/cmf-cli/bin/Release/cmf build Cmf.Custom.Tests
```

This runs the normal CLI restore/build flow. The successful build had zero errors;
existing IoT package/framework compatibility warnings remain.

Use the existing custom test files to run the tests. Initialization and
cmf new test do not need to be rerun over this directory.

### 4. Select the browser already downloaded in this devcontainer

```bash
export CMF_CHROME_BINARY=/tmp/cmf-browser/chrome-linux64/chrome
"$CMF_CHROME_BINARY" --version
./Cmf.Custom.Tests/Release/chromedriver --version
```

Both should report 147.0.7727.56. The browser under /tmp needs to be downloaded
again in a fresh container.

To recreate the browser setup in a fresh Ubuntu 24.04 devcontainer:

```bash
sudo apt-get update
sudo apt-get install -y --no-install-recommends \
  libglib2.0-0t64 libnss3 libnspr4 libatk1.0-0t64 libatk-bridge2.0-0t64 \
  libdbus-1-3 libcups2t64 libdrm2 libxkbcommon0 libxcomposite1 \
  libxdamage1 libxfixes3 libxrandr2 libgbm1 libasound2t64 \
  libpango-1.0-0 libcairo2 libatspi2.0-0t64

curl --fail --location \
  https://storage.googleapis.com/chrome-for-testing-public/147.0.7727.56/linux64/chrome-linux64.zip \
  --output /tmp/cmf-chrome147-linux64.zip
mkdir -p /tmp/cmf-browser
unzip -q -o /tmp/cmf-chrome147-linux64.zip -d /tmp/cmf-browser
export CMF_CHROME_BINARY=/tmp/cmf-browser/chrome-linux64/chrome
"$CMF_CHROME_BINARY" --version
./Cmf.Custom.Tests/Release/chromedriver --version
```

Alternatively, select an installed compatible Chrome through CMF_CHROME_BINARY.
For remote WebDriver, unset that local binary override and set CMF_WEBDRIVER_URL.

### 5. Verify discovery

```bash
dotnet test Cmf.Custom.Tests/Cmf.Custom.Tests.Biz/Cmf.Custom.Tests.Biz.csproj \
  --configuration Release --no-build --no-restore --list-tests

dotnet test Cmf.Custom.Tests/Cmf.Custom.Tests.GUI/Cmf.Custom.Tests.GUI.csproj \
  --configuration Release --no-build --no-restore --list-tests
```

Expect two Host methods and three UI methods matching the names above.

### 6. Run the public UI check first

This requires no account, password or tenant:

```bash
dotnet test Cmf.Custom.Tests/Cmf.Custom.Tests.GUI/Cmf.Custom.Tests.GUI.csproj \
  --configuration Release --no-build --no-restore \
  --settings Cmf.Custom.Tests/smoke.runsettings \
  --filter 'TestCategory=UiPublic' \
  --results-directory Cmf.Custom.Tests/TestResults \
  --logger 'trx;LogFileName=ui-public.trx'
```

Expected: **one passed test**. This check already passed against the nightly site.

### 7. Configure authentication in the same Bash session

```bash
read -r -p 'MES login account: ' CMF_TEST_USERNAME
read -r -s -p 'MES password: ' CMF_TEST_PASSWORD
printf '\n'
read -r -p 'Actual MES tenant name: ' CMF_TEST_TENANT
export CMF_TEST_USERNAME CMF_TEST_PASSWORD CMF_TEST_TENANT
```

The password is entered without being displayed or written into this guide.
The tests do not read credentials from the scaffold's original env.json.

The authenticated retry confirmed the Security Portal tenant as MesDevelopment
and successful Host requests through the supplied website origin under /api.
The website origin is the default Host address. Override it
if Host/REST uses another hostname, port or base path:

```bash
# Optional: replace with this environment's actual HTTPS Host/REST address.
# export CMF_TEST_HOST_URL='https://actual-host-route/'

# Optional: MES account to retrieve if different from the login account.
# export CMF_TEST_USER_ACCOUNT='the-MES-user-account'
```

Use the observed remote tenant rather than the original local scaffold tenant:

```bash
export CMF_TEST_TENANT=MesDevelopment
```

For the supplied nightly environment no Host URL override was needed.

### 8. Run the read-only LBO Host requests

The runner authenticates through the observed username/password browser
flow, reads the session's access_token cookie, and supplies it only to the child
Host test process. It does not print or write the token to a file.

This token reuse was explicitly authorized and the following command passed:

```bash
dotnet run --project SmokeRunner/SmokeRunner.csproj --configuration Release
```

Expected result: two passed Host tests. Successful requests returned HTTP 200 for
/api/ApplicationSetting/GetApplicationBootInformation,
/api/Security/GetUserByUserAccount and /api/Security/GetUserById.
The direct test command reports inconclusive if CMF_TEST_ACCESS_TOKEN is absent;
SmokeRunner obtains and supplies that value without a token file.

### 9. Run authenticated UI sign-in and session refresh

```bash
dotnet test Cmf.Custom.Tests/Cmf.Custom.Tests.GUI/Cmf.Custom.Tests.GUI.csproj \
  --configuration Release --no-build --no-restore \
  --settings Cmf.Custom.Tests/smoke.runsettings \
  --filter 'TestCategory=UiAuthenticated' \
  --results-directory Cmf.Custom.Tests/TestResults \
  --logger 'trx;LogFileName=ui-authenticated.trx'
```

The latest authenticated retry passed both tests. These assume the observed
username-then-password Security Portal flow and decline optional device enrollment.
MFA or another SSO flow may require adapting the login page object.

To run all three browser checks together:

```bash
dotnet test Cmf.Custom.Tests/Cmf.Custom.Tests.GUI/Cmf.Custom.Tests.GUI.csproj \
  --configuration Release --no-build --no-restore \
  --settings Cmf.Custom.Tests/smoke.runsettings \
  --filter 'TestCategory=NightlySmoke' \
  --results-directory Cmf.Custom.Tests/TestResults \
  --logger 'trx;LogFileName=ui-smoke.trx'
```

### 10. Inspect results and clear the password

```bash
ls -lh Cmf.Custom.Tests/TestResults/*.trx
unset CMF_TEST_PASSWORD
```

Repeated runs with the same LogFileName overwrite that TRX file. Change the
filename when you want to retain multiple runs.

## Settings reference

| Variable | Required for | Meaning |
| --- | --- | --- |
| CMF_TEST_USERNAME | Authenticated Host/UI | Login account. |
| CMF_TEST_PASSWORD | Authenticated Host/UI | Password supplied through the process environment. |
| CMF_TEST_TENANT | Host | Actual remote MES tenant. |
| CMF_TEST_URL | Optional | Website override; defaults to the supplied nightly URL. |
| CMF_TEST_HOST_URL | Optional | HTTPS Host/REST base if different from the website. |
| CMF_TEST_USER_ACCOUNT | Optional | Account to retrieve; defaults to CMF_TEST_USERNAME. |
| CMF_TEST_ACCESS_TOKEN | Host | Bearer token passed in memory by SmokeRunner; also used for the same-origin gateway cookie. |
| CMF_CHROME_BINARY | Local UI | Browser binary when not installed in the default location. |
| CMF_CHROMEDRIVER_DIRECTORY | Optional | Directory containing a compatible driver. |
| CMF_WEBDRIVER_URL | Optional | Remote Selenium endpoint. |
| CMF_TEST_SHELL_SELECTOR | Optional | Shell selector; defaults to cmf-core-shell-shell. |

## Troubleshooting and limits

- **Zero discovered tests:** compare with step 5. An empty run is not successful
  runtime validation.
- **Skipped tests:** the inconclusive message names a missing environment
  variable. Set it in the same terminal/process and rerun.
- **Browser cannot start:** check browser/driver versions, binary path and
  required Linux libraries.
- **Host 401/403, redirect or timeout:** confirm the actual Host route, tenant,
  credentials and account read permissions. A passing public page does not
  validate those settings.
- **Authenticated shell timeout:** check the login flow and shell selector.
  The page object refuses to submit credentials to another host or an HTTP page.

## Evidence from the run performed here

The public test previously passed. The authenticated retry passed both UI tests
and both Host tests: five smoke checks passed across these runs. Host authentication
needed the browser-session bearer token and the website gateway's access_token
cookie; no passwords or tokens were written into test source or token files.
These results cover the 11.3.6 client/LBO inputs against the nightly environment,
whose UI reports 11.3.7. They are not a full MES regression suite.

- Cmf.Custom.Tests/TestResults/ui-public.trx
- Cmf.Custom.Tests/TestResults/ui-smoke.trx
- Cmf.Custom.Tests/TestResults/host-smoke.trx
- Cmf.Custom.Tests/TestResults/ui-authenticated.trx
- Cmf.Custom.Tests/TestResults/host-authenticated.trx
- /tmp/cmf-1136-smoke-build.log
- /tmp/cmf-1136-host-discovery.log
- /tmp/cmf-1136-ui-discovery.log
- /tmp/cmf-1136-ui-smoke-tests.log
- /tmp/cmf-1136-host-tests.log
- /tmp/cmf-1136-ui-authenticated-tests.log
- /tmp/cmf-1136-host-authenticated-tests.log
- /tmp/cmf-1136-auth-helper-build.log
- /tmp/cmf-1136-host-browser-auth-tests.log

Build output, test results, local LBO binaries and supplied environment
configuration are local/generated inputs ignored by the scaffold's .gitignore.

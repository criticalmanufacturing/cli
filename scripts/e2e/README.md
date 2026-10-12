# CLI end-to-end developer flow

This opt-in helper runs the real user workflow against a fresh MES customization
project. It is separate from PR CI and does not replace
`.github/workflows/scaffolding-validation.yml`.

Use Node 20 or later on Linux and Docker. The helper installs the
[devcontainer CLI](https://github.com/devcontainers/cli) as a local dependency. Run from the
CLI development container if your host has no SDKs/tooling. Docker must be
accessible there, and the generated project must be visible to its daemon.
The self-contained CLI bundle must match the generated Linux container's architecture.
Portal, private feeds, Docker registry access, sufficient resources for the MES
stack, and a valid license for the selected MES major are required.
The generated devcontainer must provide Node/npm, its MES SDKs, Docker access,
and the compatible `cmf-dev` plugin. Its normal setup hooks must complete.

## Setup and run

From the repository root **inside the CLI devcontainer**:

```sh
sudo apt update && sudo apt install -y xdg-utils && sudo apt clean
nvm use 20
npm install -g @criticalmanufacturing/portal
npm run build:bundle:linux
npm --prefix scripts/e2e ci
npm --prefix scripts/e2e run browser:install
cp scripts/e2e/config.example.json scripts/e2e/config.local.mes11.json
```

The `browser:install` step installs Chromium and its required Linux libraries
in the runner's container; installing system libraries requires sudo access.
`xdg-utils` supports CLI login but does not supply Chromium's dependencies.
If Chromium reports missing Linux libraries, rerun
`npm --prefix scripts/e2e run browser:install` inside the CLI devcontainer.
Playwright supplies the real browser needed to check authentication and the
rendered homepage. Both Playwright and the devcontainer CLI are isolated from
the CLI build and published npm package. The runner requires Node 20; the
generated MES project's container keeps its own Node version (for example, 18 for MES 10).

Keep a separate configuration for each MES version: set a full `mesVersion`, its
single `licenseId`, and a fresh `workspace`. Paths are relative to the current working
directory. Set `ciRepo` to the CI repository URL and `releaseRepos` to a nonempty
array of release repository URLs, matching the `cmf init` option names. The example
URLs are intentionally not real publishing destinations; packages are only packed locally.

```sh
node scripts/e2e/run.mjs --config scripts/e2e/config.local.mes11.json
```

For example, copy the example to `config.local.mes11.json` and
`config.local.mes12.json`, giving each file its own MES version, license, and workspace.
Pass the appropriate file with `--config` when running that version's flow.

A missing `licenseId` is prompted for in an interactive terminal. For unattended runs,
set `licenseId` in that version's configuration or override it with `CMF_E2E_LICENSE_ID`.
`cmf login` keeps its normal interactive behavior and does not record terminal
input. Install the portal plugin with `npm install -g @criticalmanufacturing/portal`
in the runner's Node environment for interactive login. For an unattended run,
set `CMF_E2E_PORTAL_TOKEN` to an existing valid portal bearer token; the helper uses
`cmf login Portal --auth-type Bearer --token ... --no-prompt` without logging the token.
Existing portal/registry credentials must also be available to the
generated devcontainer through your usual credential setup. The helper does not
copy credential stores between containers.

If you launch from the host without tooling, use
`devcontainer exec --workspace-folder . node scripts/e2e/run.mjs --config scripts/e2e/config.local.mes11.json`
after completing setup inside that container. When the generated project's path
differs between the runner and Docker host, set `devcontainerWorkspace` to the
host-visible path. Both paths must refer to the same directory.

## What it verifies

1. Logs in and runs `cmf init test --tenant test --baseVersion ... --ciRepo ... --releaseRepos ...`.
   Requires root `cmfpackage.json`, `repositories.json`, and devcontainer configuration.
   The project is scaffolded into the working directory, matching `cmf init` behavior.
2. Starts the generated devcontainer and checks the CLI and local stack plugin.
   Copies the entire candidate `cliBundle` into `.e2e/cli` and uses it for every
   subsequent `cmf` command, including subprocesses via PATH. The generated
   container's preinstalled CLI is not the candidate under test.
3. Runs `cmf login sync` explicitly inside the generated devcontainer before
   scaffolding. VS Code's folder-open login task does not execute during a headless
   `devcontainer up`, so the helper synchronizes registry credentials itself.
   Creates business, HTML, help, data, test, and IoT packages; checks each package's
   manifest and scaffolded content. Passes business/HTML locations for related packages.
4. Initializes the local stack, requires `compose.local.yaml`, then starts it with
   the selected license and `--nobackup`.
5. Polls `/deployment/status`: HTTP 202 means installation continues, HTTP 400
   fails immediately, and HTTP 200 with EnvManager's success message completes it.
   Startup connection errors are retried until the installation timeout.
6. Runs `cmf dev local lbos` and checks the generated .NET and TypeScript libraries.
   These libraries are required by the test scaffold. Then runs `cmf build` from
   each of the six package directories.
7. Starts `npm run serve` in the background inside the generated devcontainer,
   waits for its HTTP endpoint, opens `localhost:80` in headless Chromium, logs in
   with `admin`/`admin`, and requires visible “Administration” and “Business Data”
   entries in the MES sidebar. If the fresh stack requires an initial password reset,
   the helper confirms the configured password (admin by default) through the UI
   signs in again if required, and then verifies the homepage.
8. Stops the serve process and runs `cmf pack` for all six packages, requiring
   nonempty ZIP archives in `.e2e/packages/<type>`.

The generated `serve` script listens on port **7000**; the local stack routes
the browser through port **80**. Set `serveUrl`, `uiUrl`, and `deploymentUrl` if
your runner reaches those ports through a different address. Port 7000 must be
reachable from the runner; use your devcontainer's normal forwarding/network
setup. For localhost URLs, the helper writes `.e2e/devcontainer.json` with Docker
port mappings for 80 and 7000 and uses it as an override. This makes the ports
reachable without VS Code's port-forwarding UI and preserves the generated
configuration. An already responding development server is rejected.
Each run labels its container with the run ID in `report.json`, so a stopped
container from an earlier run is not silently reused.

The runner uses its own Docker credential directory at
`~/.config/cmf-cli-e2e/docker`, avoiding VS Code's interactive credential helper.
An explicit `DOCKER_CONFIG` environment variable overrides that location.

For different login markup, override `login.usernameSelector`, `passwordSelector`,
`submitSelector`, and `sidebarSelector`. The optional reset form uses
`resetPasswordSelector`, `resetConfirmationSelector`, and `resetSubmitSelector`. Defaults match the local security login
and MES sidebar. Use `CMF_E2E_UI_USER`/`CMF_E2E_UI_PASSWORD` to override admin credentials.
The default sidebar selector is `.menu-sub-menu`; both required labels must be
visible inside it. Existing configurations should use this selector for MES 10.
`initArgs` is an optional array of extra `cmf init` arguments. Set `interactiveInit`
to true if your template requires interactive answers; that step then has no log.
All commands have `commandTimeoutSeconds`; installation and browser readiness
have separate `installationTimeoutSeconds` and `uiTimeoutSeconds` limits.

## Results and cleanup

Prerequisites are checked before creating the workspace. A setup failure leaves
no new project directory and can be retried after fixing the reported issue.
An existing workspace is preserved and rejected with an actionable message;
choose a fresh workspace or move the previous run aside. To continue a failed
run with the same config and candidate version, append `--resume`:

```sh
node scripts/e2e/run.mjs --config scripts/e2e/config.local.10.json --resume
```

Resume repeats prerequisites, login synchronization, installation readiness, and
browser checks, and skips completed scaffolds, builds, and packs. It verifies
retained scaffold content before continuing. Use a fresh workspace when testing
a changed candidate.

The first failed step stops the flow and returns exit code 1. A complete successful
flow returns 0. `.e2e/report.json` records step results, timings, the candidate
version, container ID, and commands. Every command is printed with its working
directory and execution context (runner or generated devcontainer). Credential
arguments are redacted. `.e2e/commands.log` collects these command headers;
individual logs also contain the header and command output. `serve.log` records the
background server, and `homepage.png` or `ui-failure.png` captures browser evidence.
Treat these artifacts as local development data: command output and screenshots
can contain environment details. Login input is not logged.

The helper always attempts to stop its background serve process, including on
failure or interruption. It retains the project, devcontainer, and local stack
for diagnosis. To stop the stack afterwards, enter the generated devcontainer
and run `cmf dev local stack down`; stop the generated development container with
your normal Docker/devcontainer tools. A fresh run needs a new workspace directory; `--resume` continues a failed run.

## Login failure diagnostics

A scope rejection is a failed workflow, even when installation and builds pass.
The fresh 10.2.16 stack tested here created admin in Administrators but had no
`MES` OAuth scope role; SecurityPortal reported "The user doesn't have all the
required scopes." The helper records that rejection and a screenshot. Correct
the baseline's scope setup before using `--resume` to retry the homepage check.

## Helper tests

```sh
npm --prefix scripts/e2e test
npm --prefix scripts/e2e run test:smoke
```

These offline tests check installation success/failure/timeout, server failures,
and scaffold validation. The smoke test also runs the complete orchestrator with
simulated CLI/Docker commands and a real Chromium login, checks package working
directories and server cleanup after both success and a failed UI check, and
verifies that a failed build stops the flow.
It requires the browser installation from setup. The actual MES workflow requires the authenticated
environment and license above; the offline tests do not certify a real MES installation.

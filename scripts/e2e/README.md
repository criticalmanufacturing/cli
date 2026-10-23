# CLI end-to-end developer flow

This opt-in helper runs the real user workflow against a fresh MES customization
project. It is separate from PR CI and does not replace
`.github/workflows/scaffolding-validation.yml`.

Use Node 20 or later on Linux x64 and Docker. The helper installs the
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
nvm use 20
npm run build:bundle:linux
npm --prefix scripts/e2e ci
cp scripts/e2e/config.example.json scripts/e2e/config.local.json
```

`npm ci` installs the devcontainer CLI, the portal SDK plugin, Chromium, and
Chromium's Linux libraries. System library installation requires root or sudo
access. No global portal install or separate browser setup is needed. To repair
an existing browser installation, run `npm --prefix scripts/e2e run browser:install`.
These dependencies are isolated from the CLI build and published npm package.
The generated MES container keeps its own Node version (for example, 18 for MES 10).

Set `mesVersion`, `licenseId`, and `portalPat` in the local config, then run:

```sh
node scripts/e2e/run.mjs --config scripts/e2e/config.local.json
```

The runner starts a local live dashboard and prints its URL, for example
`Dashboard: http://127.0.0.1:8787`. Open it while the flow is running to see
the setup checks individually, then follow the package, installation, and
browser branches as they update in real time. The output panel follows the
active step until you select another one. The dashboard uses Critical
Manufacturing UI theme colors from
`Platform/UI/projects/cmf-core/assets/style/common/_colors-theme.less`, shows
recent command output and browser captures, and redacts configured credentials.
If the default port is busy, it automatically picks an available local port.
Use `--dashboard-port <port>` to choose one explicitly
or `--no-dashboard` to disable it. A final, offline copy is written to
`.e2e/dashboard.html` when a workspace exists. After an interactive local
failure, the live dashboard stays open until you press Enter or Ctrl+C so you
can inspect the final state and browser capture.

You can keep one config per MES version, each with a single `licenseId`.
For CI, supply credentials using `CMF_E2E_PORTAL_PAT` or `CMF_E2E_PORTAL_TOKEN`
and the license using `CMF_E2E_LICENSE_ID` rather than committing secrets.
Locally, omitting both PAT and token retains interactive `cmf login`; on Linux
that browser-opening flow requires `xdg-utils`.

## CI behavior

Use `--ci` or `CI=true` to enable unattended validation:

```sh
node scripts/e2e/run.mjs --config scripts/e2e/config.local.11.json --ci
```

CI requires a fresh workspace, a configured license, unattended portal
credentials, and noninteractive initialization. `--resume` is local only.
The initial password confirmation is part of both flows. If it leaves an empty
root page for 30 seconds (or the shorter configured UI timeout), CI fails and
captures diagnostics. Locally, the helper reopens the configured UI URL once
and records that recovery in `report.json`; the normal homepage checks still
must pass. Other login failures do not use this retry.

## Authentication and configuration

The [portal SDK login](https://github.com/criticalmanufacturing/portal-sdk#login)
supports `--pat`. The helper runs `cmf portal login --pat ...`, reads the cached
token, and runs `cmf login Portal --auth-type Bearer --token ... --no-prompt`
to synchronize registry credentials. Override `portalPat` with
`CMF_E2E_PORTAL_PAT`, or supply an existing bearer token via `CMF_E2E_PORTAL_TOKEN`.
Credentials are redacted from command headers and reports. Local config files
are ignored by Git. Existing portal/registry credentials must also be available
to the generated devcontainer through your usual credential setup.

A missing license is prompted for in an interactive terminal.
`CMF_E2E_LICENSE_ID` overrides configured licenses. To share one config across
MES majors, use `licenseIds` instead of `licenseId`:

```json
{
  "mesVersion": "11.3.6",
  "licenseIds": { "10": "YOUR_MES_10_LICENSE", "11": "YOUR_MES_11_LICENSE" },
  "portalPat": "YOUR_PORTAL_PAT"
}
```

```sh
node scripts/e2e/run.mjs --config scripts/e2e/config.local.json --mes-version 10.2.16
```

Explicit settings in existing configs still work. Defaults cover shared settings:

| Setting | Default |
| --- | --- |
| `ciRepo` | `https://dev.criticalmanufacturing.io` |
| `releaseRepos` | `[ciRepo]` |
| `workspace` | `./artifacts/user-flow/mes-<normalized-version>` |
| `cliBundle` | `./npm/dist/linux-x64` |
| `projectName`, `tenant` | `test` |
| `deploymentUrl` | `http://localhost:80/deployment/status` |
| `uiUrl`, `serveUrl` | `http://localhost:80`, `http://localhost:7000` |
| Command / installation / UI timeouts | 1800 / 3600 / 600 seconds |
| UI credentials | `admin` / `admin` |

Paths are relative to the current working directory. A normal run requires a fresh
workspace. Set `workspace` for another run of the same MES version or use `--resume`
to continue a failed run. Packages are packed locally; nothing is published.
Default folder names are lowercase and replace dots with underscores, for example
`12.0.0-beta.2` uses `mes-12_0_0-beta_2`. The MES version passed to the CLI stays
unchanged. Explicit workspace paths are preserved; choose a final folder name
containing only lowercase letters, digits, hyphens, or underscores and starting
with a letter or digit, because `cmf-dev` uses it in the Compose project name.

If you launch from the host without tooling, use
`devcontainer exec --workspace-folder . node scripts/e2e/run.mjs --config scripts/e2e/config.local.json`
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
4. Scaffolds and builds business first. After the business build succeeds,
   initializes the local stack, requires `compose.local.yaml`, then starts it with
   the selected license and `--nobackup`, alongside the remaining package work.
5. Polls `/deployment/status`: HTTP 202 means installation continues, HTTP 400
   fails immediately, and HTTP 200 with EnvManager's success message completes it.
   Startup connection errors are retried until the installation timeout.
6. After both scaffolding and stack installation complete, runs `cmf dev local lbos` and checks the generated .NET and TypeScript libraries.
   These libraries are required by the test package build. Other package builds
   and packs overlap stack installation; package commands remain sequential to
   preserve shared manifest updates and business/HTML scaffold dependencies.
7. After HTML build, starts `npm run serve` in the background inside the generated devcontainer,
   waits for its HTTP endpoint, opens `localhost:80` in headless Chromium, logs in
   with `admin`/`admin`, and requires visible “Administration” and “Business Data”
   entries in the MES sidebar. If the fresh stack requires an initial password reset,
   the helper confirms the configured password (admin by default) through the UI
   signs in again if required, and then verifies the homepage.
   Browser verification starts as soon as the HTML server responds, independently
   of the installation branch. The complete run still requires installation success.
8. Stops the serve process and waits for all six builds and packs, requiring
   nonempty ZIP archives in `.e2e/packages/<type>`.
9. Runs `cmf dev local stack down --remove-volumes` as the final test. A teardown
   failure fails the run; successful runs remove stack containers and volumes.

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
node scripts/e2e/run.mjs --config scripts/e2e/config.local.json --mes-version 10.2.16 --resume
```

Resume repeats prerequisites, login synchronization, stack startup, installation readiness, and
browser checks, and skips completed scaffolds, builds, and packs. It verifies
retained scaffold content before continuing. Use a fresh workspace when testing
a changed candidate.

The first failed step cancels concurrent work, waits for it to finish, and returns exit code 1. A complete successful
flow returns 0. `.e2e/report.json` records step results, timings, the candidate
version, container ID, and commands. Every command is printed with its working
directory and execution context (runner or generated devcontainer). Credential
arguments are redacted. `.e2e/commands.log` collects these command headers;
individual logs also contain the header and command output. `serve.log` records the
background server, and `homepage.png` or `ui-failure.png` captures browser evidence.
Treat these artifacts as local development data: command output and screenshots
can contain environment details. Login input is not logged.

The helper always attempts to stop its background serve process, including on
failure or interruption. It retains the project and devcontainer. Failed runs retain the local stack
for diagnosis; successful runs remove it and its volumes. To stop a retained
stack afterwards, enter the generated devcontainer and run
`cmf dev local stack down --remove-volumes`; stop the generated development container with
your normal Docker/devcontainer tools. A fresh run needs a new workspace directory; `--resume` continues a failed run.

## Login failure diagnostics

During browser checks, `.e2e/ui-progress.json` and `.e2e/ui-progress.png` are
updated every 30 seconds and immediately after password confirmation. The
current URL is also logged without its query parameters or fragment.

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
scaffold validation, configuration defaults, parallel execution, and readiness
dependencies. The smoke test also runs the complete orchestrator with
simulated CLI/Docker commands and a real Chromium login, checks package working
directories and server cleanup after both success and a failed UI check, and
verifies PAT redaction, that a failed build prevents login, and final teardown
success and failure.
It requires the browser installation from setup. The actual MES workflow requires the authenticated
environment and license above; the offline tests do not certify a real MES installation.

## Maintaining the helper

Start with `flow.mjs`: it lists the user steps, their order, and the three
concurrent branches. Package commands are sequential because they update shared
project manifests. `steps.mjs` contains each step's commands and success checks.

`run.mjs` only reads options and starts the flow. Browser authentication lives
in `browser.mjs`. Configuration, reports/resume, command processes, and
devcontainers each have a separate module; `runtime.mjs` connects them and owns
cancellation and cleanup. Commands receive their step identity explicitly.
`exec.mjs` and `serve.mjs` are the small helpers copied into the generated container.

### Adding a validation step

1. Add an async function inside `createSteps` in `steps.mjs`, then include its
   name in the returned actions object. Use `commands.cli` for the candidate CLI
   inside the generated container, or `commands.run` for runner commands.
   Add assertions for the expected result; throw an error when validation fails.

   ```js
   async function validateCli(commands) {
     await commands.cli(['--version']); // Replace with your command and assertions.
   }
   // Include validateCli in createSteps' returned object.
   ```

2. Register it in `executeWorkflow` in `flow.mjs` with a unique, stable name:

   ```js
   await step('validate-cli', actions.validateCli);
   ```

   Place it after its prerequisites. A check that needs the installed stack and
   all completed package/UI checks belongs after `await runFlow(...)` and before
   `stack-down`. To add a check to an existing parallel branch, place it in that
   branch's callback in `executeWorkflow`; preserve the dependencies in `runFlow`.

3. Successful steps are skipped on local resume by default. For a check that must
   run on every attempt, add its name to `repeatOnResume` in `report.mjs`.
4. Add a focused success/failure test. Update `flow.test.mjs` if dependencies
   change, and `smoke.mjs` if the new step changes the complete workflow.

The step wrapper handles reporting and timings. Scoped commands handle logging,
timeouts, and cancellation; a failed validation fails the run and triggers cleanup.

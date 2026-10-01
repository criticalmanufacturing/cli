# Contributing

This guide covers setting up a development environment for the CLI, building, running and testing it, the contribution flow, and how to fix the most common environment problems.

For the release flow, see the [publishing guide](./PUBLISHING.MD).

## Repository layout

| Folder     | Contents                                                                        |
| ---------- | ------------------------------------------------------------------------------- |
| `core/`    | Shared CLI infrastructure (commands base, services, templating, utilities)      |
| `cmf-cli/` | The main executable (`cmf`), its commands and the `dotnet new` templates        |
| `tests/`   | xUnit test project for `core` and `cmf-cli`                                     |
| `npm/`     | The npm package (`@criticalmanufacturing/cli`) that downloads and runs the CLI  |
| `features/`| Dev container features (e.g. `install`) and their tests                         |
| `docs/`    | Documentation site sources (MkDocs / mdBook) and the command help generator     |

## Development environment

The recommended setup is the dev container defined in [`.devcontainer/devcontainer.json`](./.devcontainer/devcontainer.json). It provides an Ubuntu image with:

- the .NET SDKs needed to build and test the CLI
- Node.js
- GitHub CLI
- zsh
- the recommended VS Code extensions

Check `devcontainer.json` for the exact image and tool versions, and the `.csproj` files for the target frameworks.

### Prerequisites

- [VS Code](https://code.visualstudio.com/) with the [Dev Containers](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers) extension
- A Docker engine that VS Code can reach. On Windows, either:
  - **Docker Engine installed inside a WSL 2 distribution** (e.g. Ubuntu). Install [WSL 2](https://learn.microsoft.com/windows/wsl/install), [install Docker Engine](https://docs.docker.com/engine/install/ubuntu/) in the distribution, and add the [WSL](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-wsl) extension to VS Code. Then open the repository from WSL, or enable the **Dev › Containers: Execute In WSL** setting.
  - **[Docker Desktop](https://www.docker.com/products/docker-desktop/)** with the WSL 2 backend. Check its licensing terms for company use.

WSL 2 is not a container runtime by itself. It provides the Linux environment, and Docker runs inside it.

### Starting the container

1. Clone the repository. If you don't have push access, fork it and clone your fork.
2. Open the folder in VS Code.
3. Run **Dev Containers: Reopen in Container** from the command palette.

The first build takes a few minutes. When it finishes, `postCreateCommand` runs `npm ci` at the repository root, which installs the commit tooling (husky, commitlint, commitizen) and sets up the git hooks.

The repository is mounted at `/workspaces/cli`, and the container user is `vscode`.

> **Windows tip:** a repository cloned on the Windows file system (e.g. `C:\...`) and bind-mounted into the container is noticeably slower and more prone to permission issues (see [Troubleshooting](#troubleshooting)). For the best experience, clone it inside WSL (e.g. `~/src/cli`) or use **Dev Containers: Clone Repository in Container Volume**.

### Package age policy

npm is configured with a minimum release age (`min-release-age` in `.npmrc` and `NPM_CONFIG_MIN_RELEASE_AGE` in `devcontainer.json`), so freshly published packages are not installed. CI applies the same rule to NuGet packages. Before opening a PR that adds or updates NuGet dependencies, run:

```bash
npm run check:nuget:min-age
```

## Building and running

```bash
dotnet build
```

In VS Code you can also press **Ctrl+Shift+B** (**Cmd+Shift+B** on macOS) to run a build task. If no default build task is set, VS Code asks you to pick one. Choose **build**, which builds `cmf-cli` using [`.vscode/tasks.json`](./.vscode/tasks.json).

The executable is written to `cmf-cli/bin/Debug/cmf`. To run the local build as `cmf`, add an alias to your shell:

```bash
alias cmf=/workspaces/cli/cmf-cli/bin/Debug/cmf
```

The `postCreateCommand` adds this alias to `~/.bashrc`, but the container's default shell is zsh. To use it in zsh, add it to `~/.zshrc` as well:

```bash
echo "alias cmf=/workspaces/cli/cmf-cli/bin/Debug/cmf" >> ~/.zshrc
source ~/.zshrc
```

## Debugging

### Debugging a CLI command in VS Code

Most CLI commands act on the project in the current directory, so debugging means running the CLI from inside a test project. The repository includes a launch configuration for this, **debug-command** ([`.vscode/launch.json`](./.vscode/launch.json)). It:

- builds `cmf-cli` first (the `build` task in [`.vscode/tasks.json`](./.vscode/tasks.json)),
- starts `cmf-cli/bin/Debug/cmf` with the arguments in `args`,
- uses a `repo/` folder at the repository root as the working directory.

**One-time setup:** create the `repo/` folder, put a project in it for the commands to act on, and keep it out of git:

```bash
mkdir /workspaces/cli/repo && cd /workspaces/cli/repo
# scaffold a test project here with `cmf init` (see `cmf init --help`), or copy an existing one
echo "repo/" >> /workspaces/cli/.git/info/exclude
```

**Each debugging session:**

1. Set breakpoints in `cmf-cli/` or `core/`.
2. Edit `args` in `launch.json` to the command you want to run. Each argument is a separate array item:

   ```jsonc
   "args": ["new", "iot", "converter", "--help"],
   ```

3. Open **Run and Debug**, select **debug-command** and press **F5**.

Tips:

- **Commands that ask for input:** the configuration uses `"console": "internalConsole"`, which cannot read keyboard input. For interactive commands, change it to `"integratedTerminal"`.
- **Local changes to `launch.json`:** the file is tracked. Don't commit your personal `args`, or run `git update-index --skip-worktree .vscode/launch.json` to make git ignore your local edits.

### Attaching to a running CLI

To debug a command started from the terminal (for example one launched by a script), use the **.NET Core Attach** configuration and pick the `cmf` process. Most commands finish too quickly to attach to, so this is mainly useful for:

- **Commands that ask for input.** The process pauses at the prompt, which gives you time to attach. Start the command in a terminal, attach from VS Code while it waits, set your breakpoints, then answer the prompt. This is often simpler than switching `launch.json` to `integratedTerminal`.
- **Long-running commands**, such as builds or installs.

### Debugging templates (`cmf new`)

The `cmf new` templates live in `cmf-cli/resources/template_feed/` and are **copied to `cmf-cli/bin/Debug/resources/template_feed/` on build**. The CLI reads them from the build output, not from the source folder, so rebuild after changing a template. The **debug-command** configuration does this automatically.

To check the generated output, run the `new` command against the `repo/` folder, inspect the files, then discard them with `git clean` (if `repo/` is its own git repository) or delete and recreate the folder.

### Logging

Log output is often faster than stepping through code. The log level (`Debug`, `Verbose`, `Information`, `Warning`, `Error`; default `Verbose`) can be set in three ways:

```bash
cmf build --loglevel debug      # or -l debug, for a single command
export cmf_cli_loglevel=debug   # for every command in this shell
export SYSTEM_DEBUG=true        # forces debug level (same variable Azure DevOps uses)
```

To set it in the debugger, add it to `env` in `launch.json`.

### Useful environment variables

| Variable                                       | Effect                                                               |
| ---------------------------------------------- | -------------------------------------------------------------------- |
| `cmf_cli_loglevel`                             | Default log level (see above)                                        |
| `cmf_cli_skip_version_check=true`              | Skips the check for newer CLI versions at startup                    |
| `cmf_cli_feature__use_repository_clients=true` | Enables the repository clients feature (set by default in `launch.json`) |
| `cmf_cli_feature__use_streaming_publish=true`  | Enables streaming publish                                            |
| `cmf_cli_internal_disable_projectconfig_cache=1` | Re-reads the project configuration on every access instead of caching it |
| `cmf_cli_enable_telemetry` / `cmf_cli_enable_extended_telemetry` | Turn telemetry on or off                          |

Feature flags follow the pattern `cmf_cli_feature__<name>`. See [`core/Services/FeaturesService.cs`](./core/Services/FeaturesService.cs) for the current list.

### Debugging tests

**From VS Code:** with the C# Dev Kit extension, open the **Testing** view, right-click a test and choose **Debug Test**. You can also use the **Debug Test** link above each test method.

**From the terminal:** run a single test or class with a filter:

```bash
dotnet test --filter "FullyQualifiedName~tests.Specs.New"
```

To debug it, make the test host wait for a debugger, then attach with **.NET Core Attach** to the process ID it prints:

```bash
VSTEST_HOST_DEBUG=1 dotnet test --filter "FullyQualifiedName~tests.Specs.New"
```

**npm package tests:** open a **JavaScript Debug Terminal** in VS Code and run the tests in-process, so breakpoints in `npm/*.js` are hit:

```bash
cd npm && npx jest --runInBand
```

## Testing

The .NET tests use xUnit and are grouped with `TestCategory` traits:

| Category      | Meaning                                                  |
| ------------- | -------------------------------------------------------- |
| `Integration` | Runs real CLI flows (e.g. scaffolding); slower           |
| `LongRunning` | Takes a long time (e.g. installs npm dependencies)       |
| `Node12`      | Requires Node.js 12                                      |
| `Node18`      | Requires Node.js 18                                      |

```bash
# Fast suite (the same filter the pre-commit hook uses)
dotnet test --filter "(TestCategory!=LongRunning)&(TestCategory!=Node12)"

# Same filter as the PR pipeline
dotnet test --filter "TestCategory!=Integration"

# Everything except the tests that need a Node.js version you don't have installed (check with `node -v`)
dotnet test --filter "TestCategory!=Node18"   # or TestCategory!=Node12
```

The npm package has its own Jest tests (see [`npm/TESTING.md`](./npm/TESTING.md)):

```bash
cd npm && npm install --ignore-scripts && npm test
```

Changes to dev container features under `features/` are tested with the [Dev Container CLI](https://github.com/devcontainers/cli):

```bash
npm install -g @devcontainers/cli
devcontainer features test -f install --skip-autogenerated --skip-duplicated ./features/
```

## Contribution flow

1. Create a branch from `development`.
2. Make your changes, adding or updating tests under `tests/` for any functional change.
3. Commit using [Conventional Commits](https://www.conventionalcommits.org/) (see below).
4. Open a pull request to `development` and fill in the [PR template](./.github/pull_request_template.md).

`main` only receives release merges from `development`. Do not target it directly.

### Commits

Commit messages are validated by commitlint, both locally and in the PR pipeline.

- Write the message as `<type>: <description>`, e.g. `fix: resolve IoT test utilities path`. Common types are `feat`, `fix`, `docs`, `test`, `chore` and `ci`.
- The **pre-commit** hook runs the fast test suite whenever staged changes touch `cmf-cli/`, `core/` or `tests/`.
- The **commit-msg** hook rejects messages that do not follow the convention.

### What CI checks

On every pull request (except changes only under `features/`), the [PR tests workflow](./.github/workflows/pr-tests.yml):

- validates all commit messages with commitlint
- checks the NuGet minimum package age
- builds in Release and runs `dotnet test --filter "TestCategory!=Integration"`
- runs the npm package tests
- fails if line coverage drops below the threshold set in the workflow

## Documentation

The documentation site is published from this repository. The command reference pages are generated from the CLI itself:

```bash
cd docs
node gen-cmds.js /workspaces/cli/cmf-cli/bin/Debug/cmf.dll
```

See [`docs/README.md`](./docs/README.md) for how to render the site locally with MkDocs or mdBook.

## Troubleshooting

### File ownership and permission problems

**Symptoms:**

- CLI commands or builds fail with *Permission denied*
- `chmod` fails with *Operation not permitted*
- every file shows as modified in `git status`, and `git diff` shows only `old mode 100644` / `new mode 100755`
- commits go through without running the git hooks

**Cause:** files owned by a different user than the one in the container, usually because the repository was cloned or built by another container, image or user. Working around it with `chmod -R 777` causes the `git status` problem: git tracks the executable bit, so every file that became executable shows as modified.

**1. Fix the ownership.** Compare your user ID with the owner of the files:

```bash
id                      # e.g. uid=1000(vscode) gid=1000(vscode)
ls -ln /workspaces/cli  # the 3rd and 4th columns are the owner uid and gid
```

If they differ, take ownership back:

```bash
sudo chown -R $(id -u):$(id -g) /workspaces/cli
```

This alone fixes *Permission denied*. If it comes back after rebuilding the container, add `"updateRemoteUserUID": true` to `devcontainer.json` so that the container user's ID matches the owner of the mounted files.

**2. Reset the permissions** (only needed if permissions were changed, e.g. by `chmod -R 777`). This sets normal permissions and restores the executable bit on the files git records as executable, such as the `.husky` hooks. It never touches file contents:

```bash
cd /workspaces/cli
find . -path ./.git -prune -o -type d -exec chmod 755 {} +
find . -path ./.git -prune -o -type f -exec chmod 644 {} +
git ls-files -s -z | grep -z '^100755' | cut -z -f2 | xargs -0 -r chmod +x
git status   # only your real changes should remain
npm ci       # reinstall node_modules, whose executables are not tracked by git
```

The last step matters: the reset also removes the executable bit from the tools in `node_modules` (such as husky and commitlint), and git cannot restore it because `node_modules` is not tracked. Without it, the git hooks and npm scripts fail with *Permission denied*. Run `npm ci` in any other folder with a `node_modules` you use, such as `npm/`.

> **Do not use `chmod -R 777`.** Fixing the ownership is enough, and 777 leaves the whole repository writable by any user.

If permissions cannot be changed at all (common when the repository is bind-mounted from a Windows drive), make git ignore them for this clone instead. This only hides the issue, and git will also stop recording legitimate executable-bit changes to scripts:

```bash
git config core.fileMode false
```

### Extra folders inside the repository show as untracked

If you keep other repositories or scratch folders (such as the `repo/` debugging folder) inside the repository, you can hide them from `git status` without changing the tracked `.gitignore`:

```bash
echo "repo/" >> .git/info/exclude
```

`.git/info/exclude` works like `.gitignore`, but it is local to your clone and never committed.

Alternatively, move the folders out of the repository. A cloned repository is self-contained, so moving it does not break it. Before moving it, check that nothing references its current path, and keep in mind that folders outside the mounted workspace may be lost when the container is rebuilt.

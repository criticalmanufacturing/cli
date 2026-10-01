# AGENTS.md

.NET 10 C# CLI (`cmf`). Solution: `cmf-cli.slnx` → `cmf-cli/` (exe, entry `cmf-cli/Program.cs`), `core/` (shared infra), `tests/` (`tests/tests.csproj`).

Existing agent playbooks: `.github/agents/cmf-cli-implementer.agent.md`, `.github/agents/cmf-cli-issue-planner.agent.md`. Copilot rules in `.github/copilot-instructions.md` also apply (minimal diffs, backward compat, no breaking CLI changes without migration, no new external deps without justification, no licensing/publishing changes).

## Build / test

- SDKs: CI installs .NET 8 + 10 (`pr-tests.yml`); `ci-tests.yml` also uses 6. Target framework is `net10.0`.
- Build: `dotnet build --configuration Release` (or `dotnet build cmf-cli/cmf.csproj`). Always `dotnet restore` first in clean envs.
- Fast unit tests (what PR CI runs): `dotnet test --configuration Release --filter "TestCategory!=Integration"`
- Single test: `dotnet test tests/tests.csproj --filter "FullyQualifiedName~<ClassOrMethod>"`
- Full suite runs `TestCategory=Integration|LongRunning|Node18|Node20` — slow, needs network/registries/node; excluded from PRs. `features/**` changes skip PR tests entirely.
- `npm` dir tests: `cd npm && npm install --ignore-scripts && npm test`
- Pre-push check: `npm run check:nuget:min-age` (CI enforces NuGet package age).

## Commands — how they wire up

- New command = class extending `Core.Commands.BaseCommand` (or `cmf-cli/Commands/BaseCommand.cs`) with `[CmfCommand(Name, Parent/ParentId, Description, ...)]`. `ParentId` takes precedence over `Parent`. Registration is reflection-based (`BaseCommand.AddChildCommands`); hierarchy is tested by `tests/Specs/CommandHierarchy.cs`.
- Implement `Configure(Command cmd)` with `System.CommandLine` options/arguments, validation, helpful errors, examples.
- Per-command MES gating: `[CmfCommand(MinimumMESVersion = "11.0.0")]`. Global gate in `Program.ValidateMesVersion` rejects MES major `< 10`.
- User strings go in `CliMessages.resx` / `CoreMessages.resx` (+ `.Designer.cs`); templates/embedded assets live in `cmf-cli/resources/` (note `template_feed/` and `vendors/` are `CopyToOutputDirectory`, not embedded).
- Always use `IFileSystem` (`System.IO.Abstractions`) via constructor injection, never static `System.IO` directly — tests use `TestingHelpers` fakes. Access shared state via `ExecutionContext`.
- Plugins: `Program.cs` loads plugin commands via `BaseCommand.AddPluginCommands`; `plugin` command group in `cmf-cli/Commands/plugin/`.

## Tests / docs conventions

- Stack is **xunit** + FluentAssertions + Moq + `Spectre.Console.Testing` (not MSTest). Specs in `tests/Specs/`, fixtures in `tests/Fixtures/`.
- Docs are MkDocs under `docs/src/` (`01-install`, `02-learn`, `03-explore`). Update docs + `CHANGES.md` when adding/changing commands, options, config files, or plugin behavior. `docs/gen-cmds.js` regenerates command pages from `cmf -h` output — don't hand-edit the `<!-- BEGIN USAGE -->` blocks.

## Commits / releases

- Conventional Commits enforced by commitlint (`header-max-length: 200`); PR CI lints every commit.
- Version lives in `cmf-cli/cmf.csproj` (`<Version>`). Bump via root `npm run bump:*` scripts only. Release flow (`PUBLISHING.MD`): `development` → PR `Release X.Y.Z` (merge, don't rebase) → `main` → GitHub Release (`@latest` from `main`, `@next` pre-release from `development`).

# Critical Manufacturing CLI - Copilot Instructions

## Repository Overview

This is the **Critical Manufacturing CLI** (`cmf-cli`), a command-line tool for scaffolding, building, and managing Critical Manufacturing projects. The CLI is written in **C# (.NET 8.0)** and distributed as an npm package with platform-specific binaries for Windows, Linux, and macOS.

**Repository Stats:**
- Primary language: C# (.NET 8.0)
- Secondary: Node.js/npm (for packaging and distribution)
- Test framework: xUnit
- Lines of code: ~50k+
- Main projects: `cmf-cli` (CLI tool), `core` (shared library), `tests` (unit tests)

## Project Structure

```
.
├── cmf-cli/              # Main CLI application (C#, .NET 8.0)
│   ├── cmf.csproj        # Project file, Version: 5.5.0
│   ├── Program.cs        # Entry point
│   ├── Commands/         # CLI command implementations
│   ├── Handlers/         # Command handlers
│   ├── Builders/         # Build orchestration
│   ├── Utilities/        # Helper utilities
│   └── resources/        # Embedded templates and resources
├── core/                 # Core shared library (C#, .NET 8.0)
│   ├── core.csproj       # Published as NuGet: CriticalManufacturing.CLI.Core
│   ├── Commands/         # Base command infrastructure
│   ├── Services/         # Core services
│   ├── Objects/          # Data models
│   └── Utilities/        # Shared utilities
├── tests/                # xUnit test suite
│   ├── tests.csproj
│   ├── Specs/            # Test specifications
│   └── Fixtures/         # Test data and fixtures
├── npm/                  # npm package wrapper
│   ├── package.json      # Published as @criticalmanufacturing/cli
│   ├── run.js            # Node.js launcher script
│   └── postinstall.js    # Downloads platform-specific binaries
├── docs/                 # Documentation site (MkDocs)
│   ├── mkdocs.yml
│   └── src/              # Documentation markdown files
├── features/             # DevContainer features
├── package.json          # Root build scripts (npm)
└── cmf-cli.sln           # Visual Studio solution file
```

## Build and Development Workflow

### Prerequisites

- **.NET SDK 8.0** (primary) and **6.0** (required for some dependencies)
- **Node.js 12+** (Node.js 20 recommended for publishing)
- **npm** (comes with Node.js)

### Environment Setup

**ALWAYS run these commands in order before any build or test operations:**

```bash
# 1. Install npm dependencies (for build tooling)
npm install

# 2. Restore .NET dependencies
# IMPORTANT: Use only nuget.org source if Azure DevOps feeds are unreachable
dotnet restore
# OR if Azure feeds fail:
dotnet restore --source https://api.nuget.org/v3/index.json
```

### Build Commands

**Development build (Debug):**
```bash
dotnet build --configuration Debug
# Output: cmf-cli/bin/Debug/cmf.dll
```

**Release build (without restore):**
```bash
dotnet build --configuration Release --no-restore
# Takes ~15 seconds
# Output: cmf-cli/bin/Release/cmf.dll
```

**Production cross-platform build (via npm scripts):**
```bash
# Clean previous builds first
npm run build:clean

# Build for all platforms (Windows, Linux, macOS)
npm run build:prod
# Takes ~3-5 minutes total
# Creates dist/win-x64/, dist/linux-x64/, dist/osx-x64/
# Each contains self-contained binaries

# Build single platform (faster for testing):
npm run build:prod:win    # Windows only (~60s)
npm run build:prod:linux  # Linux only (~60s)
npm run build:prod:osx    # macOS only (~60s)
```

**IMPORTANT BUILD NOTES:**
- Always run `npm run build:clean` before production builds to avoid stale artifacts
- The NuGet.Config includes Azure DevOps feeds which may be unreachable in some environments
- If `dotnet restore` fails with network errors, use `--source https://api.nuget.org/v3/index.json`
- Production builds use `--self-contained` and include the .NET runtime
- Do NOT commit files in `dist/`, `bin/`, or `obj/` directories

### Testing

**CRITICAL: Tests include categories that control execution:**
- `TestCategory=Internal` - Tests requiring internal infrastructure (excluded in CI)
- `TestCategory=LongRunning` - Tests that take several minutes
- `TestCategory=Node12` - Tests requiring Node.js 12
- `TestCategory=Node18` - Tests requiring Node.js 18

**Run tests (standard, used in CI):**
```bash
dotnet test --no-restore --verbosity normal --filter "TestCategory!=Internal"
# Excludes Internal tests
# May take 5-10 minutes depending on test selection
```

**Run tests (pre-commit hook):**
```bash
dotnet test --filter "(TestCategory!=LongRunning)&(TestCategory!=Node12)"
# Used by .husky/pre-commit hook
# Faster, skips long-running tests
```

**Run specific test:**
```bash
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"
```

**KNOWN ISSUE:** Some tests may hang indefinitely. If `dotnet test` doesn't complete after 5 minutes, stop it and run with more specific filters.

### Linting and Code Quality

**Commit message linting:**
- Uses **commitlint** with conventional commits format
- Enforced by `.husky/commit-msg` hook
- Format: `type(scope): message` (e.g., `feat(cli): add new command`)
- Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

**Code style:**
- Uses `.editorconfig` for C# formatting rules
- Suppresses CS1658 and CS1584 XML comment warnings (see `.editorconfig`)

### Git Hooks (Husky)

**Pre-commit hook (`.husky/pre-commit`):**
```bash
dotnet test --filter "(TestCategory!=LongRunning)&(TestCategory!=Node12)"
```
- Runs fast tests before commit
- Can be skipped with `git commit --no-verify` if needed for urgent fixes

**Commit-msg hook (`.husky/commit-msg`):**
```bash
node_modules/.bin/commitlint --edit "$1"
```
- Validates commit message format
- Failure blocks the commit

## GitHub Actions CI/CD

### PR Tests Workflow (`.github/workflows/pr-tests.yml`)

Triggered on: PRs (opened, edited, synchronize), ignores `features/**` changes

**Jobs:**
1. **commitlint** - Validates commit messages
   ```bash
   npm install -D @commitlint/cli @commitlint/config-conventional
   npx commitlint --from <base> --to <head> --verbose
   ```

2. **build** - Builds and tests
   - Setup: .NET 6.0 + 8.0, Node.js 12
   - Steps:
     ```bash
     dotnet restore
     dotnet build --configuration Release --no-restore
     dotnet test --no-restore --verbosity normal --filter "TestCategory!=Internal"
     ```

**If PR tests fail:**
- Check commitlint errors first (most common)
- Then check build errors
- Finally check test failures (exclude Internal tests)

### NPM Publish Workflow (`.github/workflows/npm-publish.yml`)

Triggered on: Release published (tag-based)

**Process:**
1. Install dependencies: `npm install`
2. Build: `npm run build:prod`
3. Create platform-specific zip archives
4. Upload to GitHub release
5. Publish to npm (@next for pre-release, @latest for release)
6. Publish Core NuGet package

### Documentation Workflow (`.github/workflows/gh-pages.yml`)

Triggered on: Push to main, PRs affecting `docs/**`

Uses **Earthly** to build MkDocs documentation site.

### DevContainer Features Tests (`.github/workflows/devcontainer-pr-tests.yml`)

Triggered on: PRs affecting `features/**`

Tests DevContainer features using `@devcontainers/cli`.

## Publishing and Versioning

**Version bumping:**
```bash
# Bump pre-release version (e.g., 5.5.0 -> 5.5.1-0)
npm run bump:pre

# Bump patch version (e.g., 5.5.0 -> 5.5.1)
npm run bump:patch

# Bump minor version (e.g., 5.5.0 -> 5.6.0)
npm run bump:feature

# Bump major version (e.g., 5.5.0 -> 6.0.0)
npm run bump:breaking
```
- Bumps version in `npm/package.json` AND `cmf-cli/cmf.csproj` AND `core/core.csproj`
- Uses `dotnet-bump` tool for .csproj files

**Local publishing (to test registry):**
```bash
npm run build:clean
npm run bump:pre
npm run build:prod
npm run publish  # Publishes to @next tag
```

**DO NOT:**
- Run `npm run publish:live` unless you intend to publish to npm @latest
- Commit package.json files before testing the build
- Use `npm version` directly (use `npm run bump:*` instead)

## Common Tasks and Pitfalls

### Making Code Changes

1. **Always restore and build first to check baseline:**
   ```bash
   dotnet restore --source https://api.nuget.org/v3/index.json
   dotnet build --configuration Release --no-restore
   ```

2. **Make your changes** to `.cs` files in `cmf-cli/`, `core/`, or `tests/`

3. **Build incrementally:**
   ```bash
   dotnet build --configuration Release --no-restore
   ```

4. **Run targeted tests:**
   ```bash
   dotnet test --filter "FullyQualifiedName~YourTestClass"
   ```

5. **Before committing:**
   - Ensure all tests pass: `dotnet test --filter "(TestCategory!=LongRunning)&(TestCategory!=Node12)"`
   - Verify commit message format: `type(scope): message`

### Common Errors and Solutions

**Error: "Name or service not known" during `dotnet restore`**
- **Cause:** Azure DevOps NuGet feeds are unreachable
- **Solution:** Use `--source https://api.nuget.org/v3/index.json`

**Error: `dotnet test` hangs indefinitely**
- **Cause:** Some tests may have infinite loops or network timeouts
- **Solution:** Use more specific test filters or exclude LongRunning tests

**Error: Commitlint fails with "subject may not be empty"**
- **Cause:** Commit message doesn't follow conventional format
- **Solution:** Use format `type(scope): message`, e.g., `fix(cli): resolve build issue`

**Error: Husky hook fails on commit**
- **Cause:** Pre-commit tests fail or commitlint validation fails
- **Solution:** Fix tests or commit message. Use `--no-verify` only for urgent fixes.

**Error: `dist/` directory committed**
- **Cause:** Build artifacts were not in .gitignore
- **Solution:** `dist/` is already in .gitignore; run `npm run build:clean` and check your git status

## Key Dependencies and Configuration Files

- **NuGet.Config**: Defines package sources (nuget.org + Azure DevOps feeds)
- **.editorconfig**: C# code style rules
- **.gitignore**: Excludes build artifacts (`dist/`, `bin/`, `obj/`, `node_modules/`, etc.)
- **.commitlintrc.json**: Commit message validation rules
- **.versionrc.json**: Version bumping configuration
- **package.json** (root): Build scripts and tooling dependencies
- **npm/package.json**: Published npm package definition
- **testenvironments.json**: Test environment configuration

## Documentation

- **Main docs:** [https://criticalmanufacturing.github.io/cli](https://criticalmanufacturing.github.io/cli)
- **Publishing guide:** [PUBLISHING.MD](../PUBLISHING.MD)
- **Command reference:** Generated in `docs/src/03-explore/commands/`

## Important Notes for Agents

1. **Trust these instructions**: Only search for additional information if these instructions are incomplete or incorrect.
2. **Test incrementally**: Always build and test after each significant change.
3. **Respect test categories**: Don't run Internal tests without proper setup.
4. **Use npm scripts**: Prefer `npm run build:*` over direct dotnet commands for production builds.
5. **Clean before building**: Always run `npm run build:clean` before production builds.
6. **Check commit messages**: Ensure conventional commit format to pass CI.
7. **Don't force-push**: The repository doesn't support force pushing.
8. **Restore first**: Always run `dotnet restore` before building after pulling changes.

## Quick Reference

| Task | Command | Time | Notes |
|------|---------|------|-------|
| Install deps | `npm install && dotnet restore --source https://api.nuget.org/v3/index.json` | ~30s | Always run first |
| Dev build | `dotnet build --configuration Debug` | ~15s | For quick iteration |
| Release build | `dotnet build --configuration Release --no-restore` | ~15s | After restore |
| Run tests | `dotnet test --filter "(TestCategory!=LongRunning)&(TestCategory!=Node12)"` | ~2-5min | Pre-commit check |
| Clean | `npm run build:clean` | <1s | Before prod builds |
| Prod build | `npm run build:prod` | ~3-5min | Cross-platform |
| Bump version | `npm run bump:patch` | <1s | Updates all projects |

---

Last updated: 2025-10-23 | CLI Version: 5.5.0

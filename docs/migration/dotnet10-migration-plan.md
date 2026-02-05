# .NET 8 to .NET 10 Migration Plan
## Critical Manufacturing CLI

**Document Version:** 1.0  
**Date:** January 2026  
**Status:** Production-Ready Migration Plan  
**Criticality:** Business-Critical System

---

## Executive Summary

This document provides a comprehensive, production-ready migration plan for upgrading the Critical Manufacturing CLI from .NET 8 to .NET 10. The CLI is a cross-platform console application distributed as an npm package with platform-specific binaries (Windows, Linux, macOS) and is business-critical for Critical Manufacturing's development operations.

**Current State:**
- Framework: .NET 8.0
- Projects: 3 main projects (cmf-cli, core, tests)
- Distribution: npm package (@criticalmanufacturing/cli) with self-contained binaries
- CI/CD: GitHub Actions
- Test Framework: xUnit
- Key Dependencies: ~30+ NuGet packages

**Target State:**
- Framework: .NET 10.0
- Maintain backward compatibility where possible
- Leverage new .NET 10 features for performance and maintainability

---

## 1. Breaking Changes Analysis

### 1.1 Known .NET 10 Breaking Changes

#### High Priority Breaking Changes

**1.1.1 Runtime and BCL Changes**
- **String comparison changes:** .NET 10 updates Unicode and globalization behavior. Impact: Minimal for CLI tool.
- **Obsolete API removals:** Several APIs marked obsolete in .NET 8 are removed in .NET 10.
- **DateTimeOffset parsing:** Stricter parsing rules may affect date/time handling.

**Action Items:**
- ✅ Review all `DateTime` and `DateTimeOffset` usage
- ✅ Search for usage of obsolete APIs
- ✅ Test string comparison logic, especially for file paths

**1.1.2 SDK and MSBuild Changes**
- **Build property changes:** Some MSBuild properties have new defaults
- **Publish profiles:** Self-contained publish behavior may have subtle changes
- **Target pack changes:** Updates to runtime packs and framework references

**Action Items:**
- ✅ Test self-contained publish for all platforms (win-x64, linux-x64, osx-x64)
- ✅ Verify `PublishSingleFile` behavior
- ✅ Check binary size changes

**1.1.3 ASP.NET Core (Not Applicable)**
- This is a console application, so ASP.NET Core breaking changes don't apply

### 1.2 Dependency-Specific Breaking Changes

#### Critical Dependencies to Verify

| Package | Current Version | .NET 10 Compatibility | Risk Level |
|---------|----------------|----------------------|------------|
| Microsoft.Extensions.DependencyInjection | 8.0.0 | ✅ Upgrade to 10.x | Low |
| Newtonsoft.Json | 13.0.3 | ✅ Compatible | Low |
| Spectre.Console | 0.49.1 | ⚠️ Check latest | Medium |
| System.CommandLine | 2.0.0-beta4 | ⚠️ Still beta | Medium |
| Microsoft.CodeAnalysis.* | 4.9.2 | ⚠️ Check compatibility | High |
| Microsoft.TemplateEngine.* | 8.0.300 | ⚠️ Upgrade to 10.x | Medium |
| System.IO.Abstractions | 21.0.2 | ✅ Compatible | Low |
| SixLabors.ImageSharp | 3.1.11 | ✅ Compatible | Low |
| SharpCompress | 0.39.0 | ✅ Compatible | Low |
| OpenTelemetry.* | 1.8.1 | ✅ Check latest | Low |

**High-Risk Dependencies:**
1. **Microsoft.CodeAnalysis.CSharp.Workspaces (4.9.2)**
   - Used for code analysis and manipulation
   - May have breaking changes with new C# 13 features
   - **Mitigation:** Test all code generation and analysis features thoroughly

2. **System.CommandLine (2.0.0-beta4)**
   - Still in beta; API may change
   - Core to CLI functionality
   - **Mitigation:** Check for stable release or lock to compatible beta version

3. **Microsoft.TemplateEngine.* (8.0.300)**
   - Version tied to .NET SDK version
   - Must upgrade to 10.x series
   - **Mitigation:** Test all template scaffolding functionality

### 1.3 Language Version Changes

**.NET 10 includes C# 13:**
- New language features (pattern matching enhancements, collection expressions improvements)
- Compiler changes may affect existing code
- **Action:** Update `<LangVersion>` if needed, default is `latest` for .NET 10

---

## 2. Step-by-Step Migration Strategy

### 2.1 Pre-Migration Phase (1-2 weeks)

#### 2.1.1 Environment Setup
```bash
# Install .NET 10 SDK
winget install Microsoft.DotNet.SDK.10  # Windows
# or
sudo apt-get install dotnet-sdk-10.0    # Linux
# or
brew install --cask dotnet-sdk          # macOS

# Verify installation
dotnet --list-sdks
# Should show: 10.0.xxx

# Keep .NET 8 SDK for rollback
dotnet --list-sdks
# Should show both: 8.0.xxx and 10.0.xxx
```

#### 2.1.2 Documentation Review
- [ ] Read [.NET 10 breaking changes documentation](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0)
- [ ] Review [.NET 10 what's new](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10)
- [ ] Check release notes for all major dependencies
- [ ] Document current baseline metrics (binary size, build time, test execution time)

#### 2.1.3 Baseline Measurements
```bash
# Capture baseline metrics
npm run build:prod
# Record:
# - Build time
# - Binary sizes (dist/win-x64/, dist/linux-x64/, dist/osx-x64/)
# - Package sizes

# Run full test suite
dotnet test --no-restore --verbosity normal
# Record:
# - Total test count
# - Execution time
# - Any flaky tests
```

**Baseline Metrics Template:**
```
.NET 8 Baseline:
- Build time (prod): ___ minutes
- Windows binary size: ___ MB
- Linux binary size: ___ MB
- macOS binary size: ___ MB
- npm package size: ___ MB
- Test execution time: ___ minutes
- Test count: ___
```

### 2.2 Migration Phase - Stage 1: Framework Update (Week 1)

#### 2.2.1 Update Project Files
**Strategy:** Incremental updates with verification at each step

**Step 1: Update cmf-cli project**
```xml
<!-- cmf-cli/cmf.csproj -->
<PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <!-- ... rest unchanged ... -->
</PropertyGroup>
```

**Step 2: Update core project**
```xml
<!-- core/core.csproj -->
<PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <!-- ... rest unchanged ... -->
</PropertyGroup>
```

**Step 3: Update tests project**
```xml
<!-- tests/tests.csproj -->
<PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
</PropertyGroup>
```

**Checkpoint:** After each update, run:
```bash
dotnet restore --source https://api.nuget.org/v3/index.json
dotnet build --configuration Debug --no-restore
```

#### 2.2.2 Update GitHub Actions Workflows
**File:** `.github/workflows/pr-tests.yml`
```yaml
- name: Setup .NET Core SDKs
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: |
      6.0.x
      8.0.x
      10.0.x  # Add .NET 10
```

**File:** `.github/workflows/npm-publish.yml`
```yaml
- uses: actions/setup-dotnet@v1.7.2
  with:
    dotnet-version: '10.0.x'  # Update to .NET 10
```

**Checkpoint:** Verify CI runs successfully on test branch

#### 2.2.3 Update DevContainer Configuration
**File:** `.devcontainer/devcontainer.json`
```json
{
    "features": {
        "ghcr.io/devcontainers/features/dotnet": {
            "version": "10.0",
            "additionalVersions": ["8.0"]  # Keep 8.0 for compatibility testing
        }
    }
}
```

### 2.3 Migration Phase - Stage 2: Dependency Updates (Week 1-2)

#### 2.3.1 Update Microsoft.Extensions.* Packages
```xml
<!-- Update in both cmf-cli/cmf.csproj and core/core.csproj -->
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.0" />
<PackageReference Include="System.Configuration.ConfigurationManager" Version="10.0.0" />
```

**Verification:**
```bash
dotnet restore --source https://api.nuget.org/v3/index.json
dotnet build --configuration Debug
dotnet test --filter "TestCategory!=Internal&TestCategory!=LongRunning"
```

#### 2.3.2 Update Template Engine Packages
```xml
<!-- core/core.csproj -->
<PackageReference Include="Microsoft.TemplateEngine.Edge" Version="10.0.100" />
<PackageReference Include="Microsoft.TemplateEngine.IDE" Version="10.0.100" />
<PackageReference Include="Microsoft.TemplateEngine.Orchestrator.RunnableProjects" Version="10.0.100" />
```

**Critical Testing:**
```bash
# Test template scaffolding commands
cmf init --help
cmf new --help
# Verify template generation works
```

#### 2.3.3 Update Roslyn/CodeAnalysis Packages
```xml
<!-- cmf-cli/cmf.csproj -->
<!-- Check latest compatible version for .NET 10 -->
<PackageReference Include="Microsoft.CodeAnalysis.CSharp.Workspaces" Version="4.12.0" />
<PackageReference Include="Microsoft.CodeAnalysis.Workspaces.MSBuild" Version="4.12.0" />
```

**Verification:**
```bash
# Test code analysis features
# Run any commands that use MSBuild integration or code generation
```

#### 2.3.4 Update Test Framework Packages
```xml
<!-- tests/tests.csproj -->
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
<PackageReference Include="coverlet.collector" Version="6.0.2" />
<PackageReference Include="xunit" Version="2.9.0" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
```

#### 2.3.5 Audit All Other Dependencies
```bash
# Check for newer versions
dotnet list cmf-cli/cmf.csproj package --outdated
dotnet list core/core.csproj package --outdated
dotnet list tests/tests.csproj package --outdated

# Update conservatively
# - Spectre.Console: Check for updates
# - Newtonsoft.Json: Keep at 13.0.3 unless issues
# - System.IO.Abstractions: Update to latest
```

**Decision Matrix for Dependency Updates:**
| Update Strategy | When to Use |
|----------------|-------------|
| Major update | If required for .NET 10 compatibility |
| Minor update | If bug fixes or security patches available |
| Keep current | If working and no security issues |

### 2.4 Migration Phase - Stage 3: Validation (Week 2)

#### 2.4.1 Build Validation
```bash
# Clean build
npm run build:clean

# Debug build
dotnet build --configuration Debug

# Release build
dotnet build --configuration Release --no-restore

# Production cross-platform build
npm run build:prod

# Verify outputs
ls -lh dist/win-x64/
ls -lh dist/linux-x64/
ls -lh dist/osx-x64/
```

**Success Criteria:**
- ✅ All builds complete without errors
- ✅ Binary sizes within 5% of baseline (some increase expected)
- ✅ All platforms build successfully

#### 2.4.2 Test Suite Validation
```bash
# Run fast tests (pre-commit hook tests)
dotnet test --filter "(TestCategory!=LongRunning)&(TestCategory!=Node12)"

# Run full test suite (excluding Internal)
dotnet test --no-restore --verbosity normal --filter "TestCategory!=Internal"

# Check for new warnings or errors
dotnet build --configuration Release /warnaserror
```

**Success Criteria:**
- ✅ All tests pass
- ✅ No new warnings introduced
- ✅ Test execution time similar to baseline

#### 2.4.3 Integration Testing
**Manual Test Checklist:**
```bash
# Install locally
cd npm
npm pack
npm install -g ./criticalmanufacturing-cli-5.8.0-3.tgz

# Test core commands
cmf --version
cmf --help
cmf init --help
cmf new --help
cmf build --help
cmf pack --help

# Test actual workflows
# TODO: Add specific integration tests for your workflows
```

#### 2.4.4 Performance Validation
```bash
# Run performance-critical operations
# Compare execution times with baseline

# Build performance
time npm run build:prod

# Test performance
time dotnet test --no-restore

# CLI command performance
time cmf <critical-command>
```

**Performance Acceptance Criteria:**
- Build time: Within 10% of baseline
- Test time: Within 10% of baseline
- CLI startup time: Improved or same
- Memory usage: Within 15% of baseline

### 2.5 Migration Phase - Stage 4: Pre-Production Testing (Week 3)

#### 2.5.1 Staging Deployment
```bash
# Publish to dev/staging environment
npm run publish:dev
# This publishes to dev registry with branch name as tag

# Install from dev registry
npm install @criticalmanufacturing/cli@$(git branch --show-current) --registry https://dev.criticalmanufacturing.io/repository/npm-releases

# Run smoke tests in staging environment
```

#### 2.5.2 Cross-Platform Testing
**Test on all target platforms:**
- [ ] Windows 10/11 (x64)
- [ ] Ubuntu 20.04/22.04/24.04 (x64)
- [ ] macOS 12+ (x64, including Rosetta 2 on ARM)

**Test Scenarios per Platform:**
1. Install via npm
2. Run `cmf --version`
3. Create new project from template
4. Build a sample project
5. Run tests on sample project
6. Pack a package

#### 2.5.3 Backward Compatibility Testing
**Test with existing projects:**
- [ ] Clone existing customer projects
- [ ] Run `cmf build` on .NET 8 projects
- [ ] Verify no breaking changes in CLI behavior
- [ ] Test upgrade path for customer projects

### 2.6 Rollback Strategy

**If critical issues are discovered:**

#### Option 1: Quick Rollback (Minutes)
```bash
# Revert .csproj files
git checkout HEAD~1 -- cmf-cli/cmf.csproj core/core.csproj tests/tests.csproj

# Rebuild with .NET 8
dotnet build --configuration Release
npm run build:prod

# Republish
npm run publish
```

#### Option 2: Branch Rollback (Minutes)
```bash
# Create rollback branch from last stable release
git checkout v5.8.0
git checkout -b hotfix/rollback-net10

# Publish from stable tag
# Follow emergency release process
```

#### Option 3: npm Tag Manipulation (Seconds)
```bash
# Point @latest back to previous version
npm dist-tag add @criticalmanufacturing/cli@5.8.0 latest

# Users get previous version on install
npm install @criticalmanufacturing/cli
```

**Rollback Decision Tree:**
```
Issue Severity?
├─ Critical (Production Down): Option 3 immediately, then Option 2
├─ Major (Breaking Change): Option 2 for hotfix
└─ Minor (Cosmetic/Non-blocking): Fix forward in new release
```

---

## 3. Code Modernization Opportunities

### 3.1 C# 13 Language Features

#### 3.1.1 Collection Expressions Enhancements
**Before (.NET 8):**
```csharp
var list = new List<string> { "item1", "item2" };
var array = new[] { 1, 2, 3 };
```

**After (.NET 10 with C# 13):**
```csharp
// Collection expressions (introduced in C# 12, enhanced in C# 13)
List<string> list = ["item1", "item2"];
int[] array = [1, 2, 3];

// Spread operator
int[] combined = [..array1, ..array2, newItem];
```

**Impact:** Cleaner, more concise collection initialization throughout codebase

**Where to Apply:**
- `cmf-cli/Handlers/` - Many list/array initializations
- `core/Utilities/` - Collection operations
- `tests/` - Test data setup

**Effort:** Low, can be done gradually with code analyzer

#### 3.1.2 Primary Constructors (C# 12+)
**Before:**
```csharp
public class PackageBuilder
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    
    public PackageBuilder(IFileSystem fileSystem, ILogger logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }
}
```

**After:**
```csharp
public class PackageBuilder(IFileSystem fileSystem, ILogger logger)
{
    // Use fileSystem and logger directly
    public void Build() => logger.LogInfo("Building...");
}
```

**Impact:** Reduce boilerplate in classes with dependency injection

**Where to Apply:**
- All service classes in `cmf-cli/Services/`
- All handler classes in `cmf-cli/Handlers/`
- All builder classes in `cmf-cli/Builders/`

**Effort:** Medium, requires careful refactoring with good test coverage

#### 3.1.3 Required Properties (C# 11+)
**Before:**
```csharp
public class PackageConfig
{
    public string Name { get; set; }
    public string Version { get; set; }
    
    public PackageConfig()
    {
        Name = string.Empty;
        Version = string.Empty;
    }
}
```

**After:**
```csharp
public class PackageConfig
{
    public required string Name { get; set; }
    public required string Version { get; set; }
}
```

**Impact:** Better null safety, clearer required properties

**Where to Apply:**
- DTOs in `core/Objects/`
- Configuration classes

**Effort:** Low, mainly for new code

### 3.2 .NET 10 Runtime Improvements

#### 3.2.1 Performance - Faster String Operations
- .NET 10 includes significant string performance improvements
- No code changes needed, automatic benefit

**Potential Impact:**
- Faster file path operations
- Faster JSON serialization/deserialization
- Improved template processing

**Measurement:**
```csharp
// Add benchmarks for critical string operations
BenchmarkDotNet.Attributes.Benchmark
public void ParsePackageJson()
{
    // Existing code
}
```

#### 3.2.2 Performance - Improved LINQ
- LINQ operations are faster in .NET 10
- No code changes needed

**Potential Impact:**
- Faster collection filtering in file operations
- Improved query performance in dependency resolution

#### 3.2.3 Performance - Enhanced RegEx
- Compiled regex performance improvements
- Source-generated regex for AOT compatibility

**Before:**
```csharp
private static readonly Regex VersionRegex = new Regex(@"^\d+\.\d+\.\d+$");
```

**After (.NET 10 with source generation):**
```csharp
[GeneratedRegex(@"^\d+\.\d+\.\d+$")]
private static partial Regex VersionRegex();
```

**Impact:** Better performance, AOT-friendly

**Where to Apply:**
- `core/Utilities/` - Version parsing, package name validation
- Any regex-heavy code paths

**Effort:** Low, mainly find-and-replace with testing

### 3.3 Dependency Injection Improvements

**Before (.NET 8):**
```csharp
services.AddSingleton<IFileSystem, FileSystem>();
services.AddSingleton<ILogger, ConsoleLogger>();
```

**After (.NET 10 - Keyed Services):**
```csharp
services.AddKeyedSingleton<ILogger, ConsoleLogger>("console");
services.AddKeyedSingleton<ILogger, FileLogger>("file");

// Usage
public class Service([FromKeyedServices("console")] ILogger logger)
{
    // Use console logger specifically
}
```

**Impact:** Better control over DI resolution, especially for multiple implementations

**Where to Apply:**
- If you have multiple logger implementations
- Multiple file system abstractions (real, mock, test)

**Effort:** Medium, requires architecture review

### 3.4 Observability and Telemetry

**Leverage .NET 10 Observability Features:**
```csharp
// .NET 10 enhanced metrics
using System.Diagnostics.Metrics;

private static readonly Meter s_meter = new("CriticalManufacturing.CLI", "5.8.0");
private static readonly Counter<int> s_commandExecutions = s_meter.CreateCounter<int>("command.executions");
private static readonly Histogram<double> s_commandDuration = s_meter.CreateHistogram<double>("command.duration", "ms");

public async Task ExecuteCommand()
{
    var stopwatch = Stopwatch.StartNew();
    try
    {
        // Execute command
        s_commandExecutions.Add(1, new KeyValuePair<string, object>("command", "build"));
    }
    finally
    {
        s_commandDuration.Record(stopwatch.ElapsedMilliseconds);
    }
}
```

**Impact:** Better insights into CLI usage and performance

**Where to Apply:**
- Wrap all command handlers
- Track build/pack/deploy operations

**Effort:** Medium, requires instrumentation

### 3.5 Configuration and Options Binding

**Leverage improved options binding:**
```csharp
// .NET 10 improved options validation
services.AddOptions<PackageOptions>()
    .BindConfiguration("Package")
    .ValidateDataAnnotations()
    .ValidateOnStart();  // New in .NET 8+

public class PackageOptions
{
    [Required]
    public string Name { get; set; }
    
    [Range(1, 100)]
    public int MaxPackages { get; set; }
}
```

---

## 4. Tooling and Ecosystem Readiness

### 4.1 SDK and Toolchain Updates

#### 4.1.1 .NET SDK
**Current:** .NET SDK 8.0.x
**Target:** .NET SDK 10.0.x

**Installation:**
```bash
# Windows
winget install Microsoft.DotNet.SDK.10

# Linux (Ubuntu/Debian)
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0

# macOS
brew install --cask dotnet-sdk

# Verify
dotnet --version
# Should show: 10.0.xxx
```

**Parallel Installation:**
- Keep .NET 8 SDK installed alongside .NET 10
- Allows building for both targets during transition
- Enables rollback capability

#### 4.1.2 Global Tools
```bash
# Update global tools
dotnet tool update -g dotnet-ef
dotnet tool update -g dotnet-outdated-tool
dotnet tool update -g dotnet-format

# Check compatibility
dotnet tool list -g
```

### 4.2 Docker Image Updates

#### 4.2.1 Development Dockerfile
**Current:** Uses .NET 8 SDK image
**Update needed:** Switch to .NET 10 SDK image

**Before:**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app
```

**After:**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app
```

#### 4.2.2 Documentation Build (MkDocs)
**File:** `docs/mkdocs/Dockerfile`
- No changes needed (Python-based)
- Verify build still works after SDK update

#### 4.2.3 DevContainer
**Already Updated in Stage 1**
```json
{
    "image": "mcr.microsoft.com/devcontainers/base:ubuntu-24.04",
    "features": {
        "ghcr.io/devcontainers/features/dotnet": {
            "version": "10.0",
            "additionalVersions": ["8.0"]
        }
    }
}
```

### 4.3 CI/CD Pipeline Changes

#### 4.3.1 GitHub Actions Updates

**PR Tests Workflow** (`.github/workflows/pr-tests.yml`):
```yaml
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET Core SDKs
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: |
            6.0.x
            8.0.x
            10.0.x  # Add .NET 10
      
      - name: Setup node versions
        uses: actions/setup-node@v4  # Update to v4
        with:
          node-version: '20'  # Update to Node 20 LTS
          cache: 'npm'
      
      - name: Install dependencies
        run: dotnet restore --source https://api.nuget.org/v3/index.json
      
      - name: Build
        run: dotnet build --configuration Release --no-restore
      
      - name: Test
        run: dotnet test --no-restore --verbosity normal --filter "TestCategory!=Internal"
```

**NPM Publish Workflow** (`.github/workflows/npm-publish.yml`):
```yaml
jobs:
  build-and-publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4  # Update to v4
      
      - uses: actions/setup-node@v4  # Update to v4
        with:
          node-version: 22  # Update to Node 22 LTS
          registry-url: https://registry.npmjs.org/
      
      - uses: actions/setup-dotnet@v4  # Update to v4
        with:
          dotnet-version: '10.0.x'  # Update to .NET 10
      
      # ... rest of workflow unchanged ...
```

**DevContainer Tests** (`.github/workflows/devcontainer-pr-tests.yml`):
- Should work automatically with devcontainer.json update
- Test locally first

#### 4.3.2 Build Scripts
**No changes needed** in `package.json` scripts:
- `npm run build:prod` - Will use installed .NET SDK
- `npm run build:prod:win|linux|osx` - Platform-specific builds
- Publishing scripts remain the same

#### 4.3.3 Build Time Optimization
**Enable parallel builds:**
```bash
# In CI, use all available cores
dotnet build --configuration Release -maxcpucount

# Or via environment variable
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
dotnet build --configuration Release
```

### 4.4 Test Framework Compatibility

#### 4.4.1 xUnit (Current Framework)
- xUnit 2.8.0+ is compatible with .NET 10
- xUnit.runner.visualstudio 2.8.0+ is compatible
- No breaking changes expected

**Update if needed:**
```xml
<PackageReference Include="xunit" Version="2.9.0" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
```

#### 4.4.2 Test Runners
- Visual Studio Test Explorer: Works with .NET 10
- `dotnet test`: Works with .NET 10
- GitHub Actions test runner: Works with .NET 10

#### 4.4.3 Code Coverage
**Coverlet** (current tool):
```xml
<PackageReference Include="coverlet.collector" Version="6.0.2" />
```
- Compatible with .NET 10
- No changes needed

**Optional: Upgrade to latest:**
```xml
<PackageReference Include="coverlet.collector" Version="6.0.3" />
```

### 4.5 IDE and Editor Support

#### 4.5.1 Visual Studio
- **Visual Studio 2022 17.12+** required for .NET 10
- Update via Visual Studio Installer
- All C# 13 features supported

#### 4.5.2 Visual Studio Code
- **C# Dev Kit extension** (recommended)
- **C# extension** by Microsoft
- Both support .NET 10 out of the box
- Update extensions to latest versions

#### 4.5.3 JetBrains Rider
- **Rider 2024.3+** for full .NET 10 support
- Excellent C# 13 language support
- ReSharper integration works

### 4.6 NuGet Feed Configuration

**Review NuGet.Config:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <!-- Azure DevOps feeds may need updates for .NET 10 packages -->
  </packageSources>
</configuration>
```

**Action Items:**
- ✅ Verify all feeds are accessible
- ✅ Check if Azure DevOps feeds have .NET 10 packages
- ✅ Use `--source https://api.nuget.org/v3/index.json` if feed issues

---

## 5. Risk Assessment

### 5.1 High-Risk Areas

#### 5.1.1 Code Analysis and MSBuild Integration
**Risk Level:** 🔴 HIGH

**Components:**
- Microsoft.CodeAnalysis.CSharp.Workspaces
- Microsoft.CodeAnalysis.Workspaces.MSBuild

**Why High Risk:**
- Deep integration with MSBuild
- C# 13 compiler changes
- Code generation logic may break

**Mitigation:**
1. Test all code generation features exhaustively
2. Have rollback plan ready
3. Update to latest Roslyn packages
4. Monitor for syntax errors in generated code

**Testing Strategy:**
```bash
# Test all commands that use code analysis
cmf init business
cmf new <various templates>
# Verify generated code compiles
```

#### 5.1.2 Template Engine
**Risk Level:** 🟡 MEDIUM-HIGH

**Components:**
- Microsoft.TemplateEngine.Edge
- Microsoft.TemplateEngine.IDE
- Microsoft.TemplateEngine.Orchestrator.RunnableProjects

**Why Medium-High Risk:**
- Version tied to SDK version
- Many embedded templates in `cmf-cli/resources/template_feed/`
- Template syntax may have subtle changes

**Mitigation:**
1. Update to .NET 10 compatible versions
2. Test every template type
3. Verify template variable substitution
4. Check conditional content generation

**Testing Strategy:**
```bash
# Test each template category
cmf new business --help
cmf new data --help
cmf new test --help
cmf new iot --help
cmf new plugin --help

# Generate samples from each
mkdir /tmp/template-tests
cd /tmp/template-tests
cmf new business -n TestBusiness
cmf new data -n TestData
# ... etc
# Build each generated project
```

#### 5.1.3 Self-Contained Publishing
**Risk Level:** 🟡 MEDIUM

**Components:**
- Self-contained publish for win-x64, linux-x64, osx-x64
- PublishSingleFile option

**Why Medium Risk:**
- .NET 10 runtime size changes
- Breaking changes in trim/publish behavior
- Binary size may increase significantly

**Mitigation:**
1. Test publish on all platforms
2. Monitor binary sizes (set alerts for >20% increase)
3. Test on clean VMs without .NET installed
4. Verify all embedded resources are included

**Testing Strategy:**
```bash
# Build for all platforms
npm run build:prod

# Check sizes
ls -lh dist/*/

# Test on clean systems
# - Windows 10 VM (no .NET installed)
# - Ubuntu 22.04 container
# - macOS 12 VM
```

#### 5.1.4 Cross-Platform Compatibility
**Risk Level:** 🟡 MEDIUM

**Why Medium Risk:**
- Different runtime behaviors on Linux/macOS/Windows
- File path handling differences
- Process execution differences

**Mitigation:**
1. Test on all target platforms
2. Use System.IO.Abstractions consistently
3. Test file path operations with edge cases
4. Verify process spawning works correctly

### 5.2 Medium-Risk Areas

#### 5.2.1 Dependency Compatibility
**Risk Level:** 🟡 MEDIUM

**Components:** All third-party NuGet packages

**Mitigation:**
1. Update packages incrementally
2. Test after each major update
3. Lock versions in .csproj files
4. Monitor breaking change notices

#### 5.2.2 Test Suite Stability
**Risk Level:** 🟡 MEDIUM

**Why:** Some tests may be flaky or depend on environment

**Mitigation:**
1. Run tests multiple times
2. Fix flaky tests before migration
3. Update test infrastructure if needed
4. Consider adding timeout protections

### 5.3 Low-Risk Areas

#### 5.3.1 Documentation
**Risk Level:** 🟢 LOW
- MkDocs build is Python-based
- No .NET dependency

#### 5.3.2 npm Package Wrapper
**Risk Level:** 🟢 LOW
- Node.js wrapper is stable
- No changes needed

#### 5.3.3 Core Business Logic
**Risk Level:** 🟢 LOW
- Most business logic is framework-agnostic
- Well-tested with unit tests

### 5.4 Regression Testing Strategy

#### 5.4.1 Automated Regression Tests
**Test Categories:**
```bash
# Fast tests (run frequently)
dotnet test --filter "(TestCategory!=LongRunning)&(TestCategory!=Node12)"

# Full suite (run daily during migration)
dotnet test --filter "TestCategory!=Internal"

# All tests (run before release)
dotnet test
```

**New Test Scenarios for .NET 10:**
1. **Template Generation Tests**
   - Generate project from each template
   - Verify generated code compiles with .NET 10
   - Check all file placeholders are replaced

2. **Build Tests**
   - Build sample projects
   - Pack sample projects
   - Deploy sample projects

3. **Cross-Platform Tests**
   - Run on Windows, Linux, macOS
   - Test file path operations
   - Test process execution

4. **Performance Regression Tests**
   - Benchmark critical operations
   - Compare with .NET 8 baseline
   - Set alerts for >10% degradation

#### 5.4.2 Manual Testing Checklist
**Critical User Workflows:**
- [ ] Install CLI from npm
- [ ] Create new project (`cmf init`)
- [ ] Generate components (`cmf new`)
- [ ] Build project (`cmf build`)
- [ ] Pack project (`cmf pack`)
- [ ] Run tests (`cmf test`)
- [ ] Deploy project (`cmf deploy`)
- [ ] Update CLI (`npm update @criticalmanufacturing/cli`)

**Edge Cases:**
- [ ] Large projects (>100 packages)
- [ ] Projects with external dependencies
- [ ] Projects with custom templates
- [ ] Projects in nested folder structures
- [ ] Projects with special characters in names
- [ ] Offline scenarios (no internet)

#### 5.4.3 Performance Baseline Comparison
**Key Metrics:**
| Metric | .NET 8 Baseline | .NET 10 Target | Alert Threshold |
|--------|-----------------|----------------|-----------------|
| Build time (small project) | X ms | ≤ X * 1.1 | 20% increase |
| Build time (large project) | Y ms | ≤ Y * 1.1 | 20% increase |
| CLI startup time | Z ms | ≤ Z * 0.9 | 10% increase |
| Memory usage (peak) | M MB | ≤ M * 1.15 | 25% increase |
| Binary size (win-x64) | B MB | ≤ B * 1.2 | 30% increase |

### 5.5 Monitoring After Deployment

#### 5.5.1 Key Metrics to Monitor
**Installation Metrics:**
- npm download count
- Installation errors (from npm logs)
- Version distribution (% on .NET 10 version)

**Runtime Metrics:**
- Command execution success rates
- Average command execution times
- Error rates by command type

**Crash/Error Metrics:**
- Unhandled exceptions
- Stack traces
- Platform distribution of errors

#### 5.5.2 Telemetry Collection
**Implement/Enhance OpenTelemetry:**
```csharp
// Already has OpenTelemetry packages
// Enhance for .NET 10 migration monitoring

private static readonly ActivitySource s_activitySource = new("CriticalManufacturing.CLI");

public async Task ExecuteCommand(string command)
{
    using var activity = s_activitySource.StartActivity("ExecuteCommand");
    activity?.SetTag("command", command);
    activity?.SetTag("dotnet.version", Environment.Version.ToString());
    activity?.SetTag("os.platform", Environment.OSVersion.Platform.ToString());
    
    try
    {
        // Execute
    }
    catch (Exception ex)
    {
        activity?.SetTag("error", true);
        activity?.SetTag("error.type", ex.GetType().Name);
        throw;
    }
}
```

#### 5.5.3 Canary Deployment Strategy
**Phased Rollout:**
1. **Week 1:** Internal testing only (devs + QA)
2. **Week 2:** Beta release (@next tag) - 10% of users
3. **Week 3:** Extended beta - 30% of users
4. **Week 4:** Full release (@latest tag) - all users

**Rollback Triggers:**
- Error rate > 5% increase
- Installation failure rate > 2%
- Performance degradation > 20%
- Critical bug reports > 3

---

## 6. Validation Checklist

### 6.1 Pre-Migration Checklist

#### 6.1.1 Environment Preparation
- [ ] .NET 10 SDK installed on development machines
- [ ] .NET 10 SDK installed on CI agents
- [ ] .NET 8 SDK kept alongside for rollback
- [ ] All developers have updated IDEs (VS 2022 17.12+, VS Code, Rider 2024.3+)
- [ ] Docker images with .NET 10 SDK available
- [ ] DevContainer updated and tested

#### 6.1.2 Documentation and Planning
- [ ] This migration plan reviewed by team
- [ ] Breaking changes documentation read
- [ ] Dependency compatibility verified
- [ ] Rollback plan understood and documented
- [ ] Team trained on .NET 10 features
- [ ] Migration schedule communicated to stakeholders

#### 6.1.3 Baseline Establishment
- [ ] Current .NET 8 metrics captured:
  - [ ] Build times (debug, release, production)
  - [ ] Binary sizes (win-x64, linux-x64, osx-x64)
  - [ ] Test execution times
  - [ ] npm package size
- [ ] Current test suite passing rate: ____%
- [ ] Known issues documented
- [ ] Flaky tests identified and marked

#### 6.1.4 Backup and Safety
- [ ] All changes committed to git
- [ ] Feature branch created: `feature/dotnet10-migration`
- [ ] Last stable release tagged
- [ ] Backup of production registry packages
- [ ] Rollback procedure tested on staging

### 6.2 Migration Execution Checklist

#### 6.2.1 Stage 1: Framework Update
- [ ] Updated `cmf-cli/cmf.csproj` to `<TargetFramework>net10.0</TargetFramework>`
- [ ] Updated `core/core.csproj` to `<TargetFramework>net10.0</TargetFramework>`
- [ ] Updated `tests/tests.csproj` to `<TargetFramework>net10.0</TargetFramework>`
- [ ] Ran `dotnet restore --source https://api.nuget.org/v3/index.json`
- [ ] Ran `dotnet build --configuration Debug` - SUCCESS
- [ ] Ran `dotnet build --configuration Release` - SUCCESS
- [ ] Git commit: "build: migrate to .NET 10.0 framework"

#### 6.2.2 Stage 2: CI/CD Updates
- [ ] Updated `.github/workflows/pr-tests.yml` to include .NET 10
- [ ] Updated `.github/workflows/npm-publish.yml` to use .NET 10
- [ ] Updated `.devcontainer/devcontainer.json` to use .NET 10
- [ ] Tested devcontainer locally
- [ ] Pushed changes and verified CI runs
- [ ] Git commit: "ci: update workflows for .NET 10"

#### 6.2.3 Stage 3: Dependency Updates
- [ ] Updated Microsoft.Extensions.* to 10.0.0
- [ ] Tested with `dotnet test --filter "(TestCategory!=LongRunning)"`
- [ ] Updated Microsoft.TemplateEngine.* to 10.0.100
- [ ] Tested template generation
- [ ] Updated Microsoft.CodeAnalysis.* to 4.12.0 (or latest compatible)
- [ ] Tested code analysis features
- [ ] Updated test framework packages
- [ ] Full test run successful
- [ ] Git commit: "deps: update packages for .NET 10"

#### 6.2.4 Stage 4: Build and Test
- [ ] `npm run build:clean` successful
- [ ] `npm run build:prod` successful
- [ ] Binary sizes verified (within acceptable range)
- [ ] All platforms built successfully (win-x64, linux-x64, osx-x64)
- [ ] Full test suite passing: `dotnet test --filter "TestCategory!=Internal"`
- [ ] Performance tests run and compared to baseline
- [ ] Git commit: "chore: verify .NET 10 migration builds and tests"

### 6.3 Post-Migration Verification Checklist

#### 6.3.1 Functional Testing
**Build and Pack:**
- [ ] `cmf build` works on sample project
- [ ] `cmf pack` works on sample project
- [ ] Generated packages are valid
- [ ] Dependencies are correctly resolved

**Template Generation:**
- [ ] `cmf init` creates new projects
- [ ] `cmf new business` generates business logic
- [ ] `cmf new data` generates data entities
- [ ] `cmf new test` generates test projects
- [ ] `cmf new iot` generates IoT drivers
- [ ] All generated code compiles
- [ ] All template variables are substituted

**Cross-Platform:**
- [ ] Tested on Windows 10/11
- [ ] Tested on Ubuntu 22.04/24.04
- [ ] Tested on macOS 12+
- [ ] All platforms show same behavior

#### 6.3.2 Performance Verification
- [ ] Build time within 10% of baseline
- [ ] Test execution time within 10% of baseline
- [ ] CLI startup time same or better
- [ ] Memory usage within 15% of baseline
- [ ] Binary sizes within 20% of baseline

**Results:**
```
.NET 10 Results:
- Build time (prod): ___ minutes (baseline: ___ minutes, delta: ___%)
- Windows binary size: ___ MB (baseline: ___ MB, delta: ___%)
- Linux binary size: ___ MB (baseline: ___ MB, delta: ___%)
- macOS binary size: ___ MB (baseline: ___ MB, delta: ___%)
- npm package size: ___ MB (baseline: ___ MB, delta: ___%)
- Test execution time: ___ minutes (baseline: ___ minutes, delta: ___%)
- Test count: ___ (all passing)
```

#### 6.3.3 Integration Testing
- [ ] Local install: `npm pack && npm install -g <tarball>`
- [ ] Global command works: `cmf --version`
- [ ] All CLI commands work
- [ ] Help text displays correctly
- [ ] Error handling works correctly

#### 6.3.4 Documentation Updates
- [ ] README.md updated (if framework mentioned)
- [ ] PUBLISHING.MD reviewed
- [ ] Migration notes added to CHANGES.md
- [ ] Documentation site rebuilt and checked
- [ ] DevContainer instructions updated

#### 6.3.5 Release Preparation
- [ ] Version bumped: `npm run bump:feature` (or appropriate)
- [ ] CHANGELOG generated: `npm run gen:changelog`
- [ ] PR opened to `development` branch
- [ ] PR description includes:
  - [ ] Summary of changes
  - [ ] Testing performed
  - [ ] Performance comparison
  - [ ] Breaking changes (if any)
  - [ ] Migration notes for users
- [ ] Code review completed
- [ ] All CI checks passing

#### 6.3.6 Deployment Verification
- [ ] Published to npm @next (pre-release)
- [ ] Smoke tested installation from @next
- [ ] Monitored for 1-2 days
- [ ] No major issues reported
- [ ] Published to npm @latest (stable)
- [ ] Smoke tested installation from @latest
- [ ] Monitoring dashboard shows healthy metrics

### 6.4 Post-Deployment Monitoring (First 30 Days)

#### Week 1: Intensive Monitoring
- [ ] Daily check of npm download stats
- [ ] Daily check of error metrics
- [ ] Review user feedback/issues
- [ ] Performance metrics reviewed
- [ ] No rollback triggered

#### Week 2-3: Regular Monitoring
- [ ] Every 2-3 days metrics review
- [ ] Weekly team sync on migration status
- [ ] Address any issues found
- [ ] Performance trending positive

#### Week 4: Migration Complete
- [ ] 30-day metrics comparison
- [ ] Migration retrospective held
- [ ] Lessons learned documented
- [ ] .NET 8 support deprecated (if appropriate)
- [ ] Update roadmap for future migrations

---

## 7. Timeline and Resources

### 7.1 Estimated Timeline

**Total Duration:** 3-4 weeks

| Phase | Duration | Activities |
|-------|----------|------------|
| **Pre-Migration** | 1-2 weeks | Environment setup, documentation review, baseline establishment |
| **Stage 1: Framework** | 2-3 days | Update .csproj files, CI/CD, initial build |
| **Stage 2: Dependencies** | 3-5 days | Update NuGet packages, test after each update |
| **Stage 3: Validation** | 3-5 days | Full testing, performance validation, cross-platform testing |
| **Stage 4: Staging** | 1 week | Staging deployment, beta testing, final validation |
| **Production Release** | 1 day | Final build, publish to npm @latest |
| **Post-Migration** | 30 days | Monitoring, issue resolution, documentation |

**Critical Path:**
```
Environment Setup → Framework Update → Dependency Updates → Testing → Staging → Production
```

**Parallel Tasks:**
- Documentation updates can happen in parallel with testing
- DevContainer updates can happen early
- CI/CD updates can happen early

### 7.2 Resource Requirements

**Development Team:**
- 1-2 Senior .NET Developers (full-time for 3 weeks)
- 1 DevOps Engineer (part-time for CI/CD updates)
- 1 QA Engineer (full-time for week 2-3)

**Infrastructure:**
- CI/CD runners with .NET 10 SDK
- Test VMs for each target OS
- Staging npm registry (or use @next tag on production)

**Tools and Licenses:**
- Visual Studio 2022 Professional or Enterprise
- JetBrains Rider (if used)
- GitHub Actions minutes (for CI/CD)

### 7.3 Communication Plan

**Stakeholder Updates:**
- **Weekly:** Progress email to stakeholders
- **Daily:** Stand-up with dev team during active migration
- **Immediate:** Any issues or blockers

**User Communication:**
- **Pre-release:** Blog post or docs page announcing upcoming migration
- **Beta release:** npm @next tag announcement
- **Stable release:** Release notes with migration details
- **Post-release:** Follow-up blog post with results

---

## 8. Success Criteria

### 8.1 Technical Success Criteria

✅ **Build Success:**
- All projects build with .NET 10
- All platforms build successfully (win-x64, linux-x64, osx-x64)
- No new warnings or errors
- Binary sizes within 20% of baseline

✅ **Test Success:**
- 100% of tests passing (excluding known flaky tests)
- Test execution time within 10% of baseline
- No new test failures introduced

✅ **Performance Success:**
- Build time within 10% of baseline
- CLI startup time same or better
- Memory usage within 15% of baseline

✅ **Functional Success:**
- All CLI commands work correctly
- All template generation works
- All code analysis features work
- Cross-platform compatibility maintained

### 8.2 Business Success Criteria

✅ **User Satisfaction:**
- No increase in support tickets
- Positive feedback on new version
- No critical bugs reported

✅ **Adoption:**
- >80% of users on .NET 10 version within 60 days
- No significant user resistance or rollbacks

✅ **Stability:**
- Error rate same or lower than .NET 8 version
- No production incidents related to migration
- Rollback not needed

### 8.3 Failure Criteria (Rollback Triggers)

🚫 **Critical Failures:**
- Production-breaking bug discovered
- Error rate increase >10%
- Performance degradation >25%
- Security vulnerability introduced

🚫 **Major Failures:**
- Test failure rate >5%
- Binary size increase >30%
- Build failures on any platform
- Critical dependency incompatibility

---

## 9. Additional Resources

### 9.1 Documentation Links

**Official Microsoft Docs:**
- [.NET 10 Release Notes](https://github.com/dotnet/core/tree/main/release-notes/10.0)
- [Breaking Changes in .NET 10](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0)
- [What's New in .NET 10](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10)
- [C# 13 What's New](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13)
- [Migration Guide](https://learn.microsoft.com/en-us/dotnet/core/migration/)

**Key Package Documentation:**
- [Microsoft.Extensions.DependencyInjection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [Microsoft.CodeAnalysis (Roslyn)](https://github.com/dotnet/roslyn)
- [System.CommandLine](https://github.com/dotnet/command-line-api)
- [Spectre.Console](https://spectreconsole.net/)

### 9.2 Internal Resources

**Repository:**
- [CLI Documentation](https://criticalmanufacturing.github.io/cli)
- [Publishing Guide](./PUBLISHING.MD)
- [Change Log](./CHANGES.md)

**CI/CD:**
- [GitHub Actions Workflows](./.github/workflows/)
- [PR Tests Workflow](./.github/workflows/pr-tests.yml)
- [NPM Publish Workflow](./.github/workflows/npm-publish.yml)

### 9.3 Support and Escalation

**Issue Tracking:**
- GitHub Issues for bug reports
- Internal team Slack/Teams channel for questions
- Weekly sync meetings during migration

**Escalation Path:**
1. Development Team Lead
2. Engineering Manager
3. CTO (for critical production issues)

---

## 10. Appendix

### 10.1 Glossary

- **AOT:** Ahead-of-Time compilation
- **BCL:** Base Class Library
- **CLI:** Command-Line Interface
- **LTS:** Long-Term Support
- **MSBuild:** Microsoft Build Engine
- **NuGet:** .NET package manager
- **RID:** Runtime Identifier (e.g., win-x64, linux-x64)
- **SDK:** Software Development Kit
- **TFM:** Target Framework Moniker (e.g., net10.0)

### 10.2 Common Commands Reference

```bash
# Build commands
dotnet restore --source https://api.nuget.org/v3/index.json
dotnet build --configuration Debug
dotnet build --configuration Release --no-restore
npm run build:prod

# Test commands
dotnet test --filter "(TestCategory!=LongRunning)&(TestCategory!=Node12)"
dotnet test --filter "TestCategory!=Internal"
dotnet test --verbosity detailed

# Package management
dotnet list package --outdated
dotnet add package <PackageName> --version <Version>

# Version bumping
npm run bump:feature
npm run bump:patch
npm run bump:breaking

# Publishing
npm run publish        # @next
npm run publish:live   # @latest
```

### 10.3 Troubleshooting Guide

#### Issue: Restore Fails with Azure DevOps Feed Errors
**Solution:**
```bash
dotnet restore --source https://api.nuget.org/v3/index.json
```

#### Issue: Tests Hang Indefinitely
**Solution:**
```bash
# Use more specific filters
dotnet test --filter "(TestCategory!=LongRunning)&(TestCategory!=Node12)"
# Or target specific test class
dotnet test --filter "FullyQualifiedName~YourTestClass"
```

#### Issue: Build Fails with Missing Reference
**Solution:**
```bash
# Clean and rebuild
npm run build:clean
dotnet clean
dotnet restore --source https://api.nuget.org/v3/index.json
dotnet build --configuration Release
```

#### Issue: Binary Size Too Large
**Solution:**
```xml
<!-- Add to .csproj -->
<PropertyGroup>
    <PublishTrimmed>true</PublishTrimmed>
    <TrimMode>partial</TrimMode>
</PropertyGroup>
```
**Warning:** Test thoroughly after enabling trimming

#### Issue: Runtime Error on macOS
**Solution:**
- Verify code signing (if applicable)
- Check for x64 vs ARM compatibility
- Test on Rosetta 2 environment

---

## Document Control

**Version History:**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-01-30 | Migration Team | Initial comprehensive migration plan |

**Approval:**

- [ ] Development Team Lead: _________________ Date: _______
- [ ] Engineering Manager: _________________ Date: _______
- [ ] QA Lead: _________________ Date: _______

**Next Review Date:** 2026-02-28 (or upon completion of migration)

---

**End of Migration Plan**

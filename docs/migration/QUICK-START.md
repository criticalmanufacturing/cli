# .NET 8 to .NET 10 Migration - Quick Start

> **Full Plan:** See [dotnet10-migration-plan.md](./dotnet10-migration-plan.md) for the complete 1,662-line production-ready migration guide.

## TL;DR - Migration Overview

**Timeline:** 3-4 weeks  
**Effort:** 1-2 Senior .NET Developers  
**Risk Level:** Medium (with mitigation strategies in place)

### Quick Decision Matrix

| If you need... | Use this section |
|----------------|------------------|
| High-level overview | This document |
| Breaking changes list | [Section 1](./dotnet10-migration-plan.md#1-breaking-changes-analysis) |
| Step-by-step instructions | [Section 2](./dotnet10-migration-plan.md#2-step-by-step-migration-strategy) |
| Code modernization ideas | [Section 3](./dotnet10-migration-plan.md#3-code-modernization-opportunities) |
| CI/CD updates | [Section 4.3](./dotnet10-migration-plan.md#43-cicd-pipeline-changes) |
| Risk analysis | [Section 5](./dotnet10-migration-plan.md#5-risk-assessment) |
| Checklists | [Section 6](./dotnet10-migration-plan.md#6-validation-checklist) |

---

## 5-Minute Summary

### What Changes?

**Project Files (.csproj):**
```xml
<!-- Change this -->
<TargetFramework>net8.0</TargetFramework>

<!-- To this -->
<TargetFramework>net10.0</TargetFramework>
```

**Files to Update:**
- `cmf-cli/cmf.csproj`
- `core/core.csproj`
- `tests/tests.csproj`
- `.github/workflows/pr-tests.yml`
- `.github/workflows/npm-publish.yml`
- `.devcontainer/devcontainer.json`

**Packages to Update:**
- Microsoft.Extensions.* → 10.0.0
- Microsoft.TemplateEngine.* → 10.0.100
- Test packages (xUnit, etc.) to latest

### Key Risks & Mitigation

| Risk | Level | Mitigation |
|------|-------|------------|
| Code Analysis breaking | 🔴 HIGH | Test all code gen features, update Roslyn packages |
| Template Engine breaking | 🟡 MEDIUM | Test all templates, verify variable substitution |
| Binary size increase | 🟡 MEDIUM | Monitor sizes, consider trimming if >20% increase |
| Cross-platform issues | 🟡 MEDIUM | Test on Windows, Linux, macOS before release |

### Critical Testing Areas

1. **Template Generation** - All `cmf new` commands
2. **Code Analysis** - MSBuild integration features
3. **Self-Contained Publish** - win-x64, linux-x64, osx-x64
4. **Cross-Platform** - Test on all target OSes

---

## 4-Stage Migration Process

### Stage 1: Framework Update (2-3 days)
```bash
# 1. Update .csproj files to net10.0
# 2. Update CI/CD workflows
# 3. Update DevContainer
# 4. Verify builds work

dotnet restore --source https://api.nuget.org/v3/index.json
dotnet build --configuration Release --no-restore
```

✅ **Checkpoint:** All projects build successfully

### Stage 2: Dependency Updates (3-5 days)
```bash
# Update packages one group at a time
# 1. Microsoft.Extensions.* packages
# 2. Microsoft.TemplateEngine.* packages
# 3. Microsoft.CodeAnalysis.* packages
# 4. Test framework packages

dotnet test --filter "TestCategory!=Internal&TestCategory!=LongRunning"
```

✅ **Checkpoint:** All tests pass after each package group update

### Stage 3: Validation (3-5 days)
```bash
# Full build and test
npm run build:prod
dotnet test --filter "TestCategory!=Internal"

# Performance comparison
# - Build times
# - Binary sizes
# - Test execution times
```

✅ **Checkpoint:** All builds, tests pass; performance within acceptable range

### Stage 4: Staging (1 week)
```bash
# Publish to dev/staging
npm run publish:dev

# Cross-platform testing
# - Windows 10/11
# - Ubuntu 22.04/24.04
# - macOS 12+
```

✅ **Checkpoint:** Smoke tests pass on all platforms

---

## Quick Commands Reference

### Pre-Migration
```bash
# Capture baseline
npm run build:prod
# Record build time, binary sizes

dotnet test --no-restore --verbosity normal
# Record test count, execution time
```

### Migration
```bash
# Install dependencies
npm install
dotnet restore --source https://api.nuget.org/v3/index.json

# Build
dotnet build --configuration Release --no-restore

# Test (fast)
dotnet test --filter "(TestCategory!=LongRunning)&(TestCategory!=Node12)"

# Test (full)
dotnet test --filter "TestCategory!=Internal"

# Production build
npm run build:prod
```

### Rollback (if needed)
```bash
# Option 1: Revert files
git checkout HEAD~1 -- cmf-cli/cmf.csproj core/core.csproj tests/tests.csproj

# Option 2: npm tag
npm dist-tag add @criticalmanufacturing/cli@5.8.0 latest
```

---

## Success Criteria (Quick Check)

✅ All builds complete without errors  
✅ All tests pass (excluding known flaky tests)  
✅ Binary sizes within 20% of baseline  
✅ Build time within 10% of baseline  
✅ All platforms work (win-x64, linux-x64, osx-x64)  
✅ Template generation works for all templates  
✅ Code analysis features work  
✅ No new warnings or errors  

---

## Red Flags (Stop and Review)

🚫 Test failure rate > 5%  
🚫 Binary size increase > 30%  
🚫 Build failures on any platform  
🚫 Performance degradation > 25%  
🚫 Template generation broken  
🚫 Code analysis features broken  

---

## Most Common Issues & Solutions

### Issue: NuGet restore fails with Azure DevOps errors
```bash
# Solution: Use nuget.org directly
dotnet restore --source https://api.nuget.org/v3/index.json
```

### Issue: Tests hang indefinitely
```bash
# Solution: Use more specific filters
dotnet test --filter "(TestCategory!=LongRunning)&(TestCategory!=Node12)"
```

### Issue: Binary size too large (>30% increase)
```xml
<!-- Solution: Enable trimming (test thoroughly!) -->
<PropertyGroup>
    <PublishTrimmed>true</PublishTrimmed>
    <TrimMode>partial</TrimMode>
</PropertyGroup>
```

---

## When to Read the Full Plan

You should read the [full migration plan](./dotnet10-migration-plan.md) if:

- You're the migration lead
- You need to understand all risks
- You need detailed CI/CD update instructions
- You need code modernization guidance (C# 13 features)
- You need the complete validation checklist (100+ items)
- You need performance optimization strategies
- You need the rollback procedures
- You're planning resources and timeline

---

## Need Help?

1. Read the [full migration plan](./dotnet10-migration-plan.md)
2. Check the [troubleshooting guide](./dotnet10-migration-plan.md#103-troubleshooting-guide)
3. Open a GitHub issue
4. Consult the [main documentation](https://criticalmanufacturing.github.io/cli)

---

**Remember:** This is a business-critical system. Take time to plan, test thoroughly, and have rollback strategies ready.

**Full Plan:** [dotnet10-migration-plan.md](./dotnet10-migration-plan.md) (1,662 lines, ~45 minutes read)

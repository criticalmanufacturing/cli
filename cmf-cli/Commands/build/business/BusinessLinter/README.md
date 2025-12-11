# Business Package Linter

## Overview

The Business Package Linter is a code analysis tool designed to detect problematic code patterns in Business packages. It uses Roslyn (Microsoft.CodeAnalysis) to parse and analyze C# code files.

## Usage

The linter can be invoked using the CLI:

```bash
cmf build business lint <solution-path> [files...]
```

### Parameters

- `solution-path` (required): Path to the solution file (.sln)
- `files` (optional): Specific files to lint. If not provided, all files in the solution will be linted.

### Example

```bash
# Lint all files in the solution
cmf build business lint ./MyProject/MyProject.sln

# Lint specific files
cmf build business lint ./MyProject/MyProject.sln MyOrchestration.cs MyService.cs
```

## Rules

The linter currently implements the following rules:

### NoLoadInForeach

**Description**: Detects calls to `Load()` methods inside `foreach` loops, which can lead to performance issues.

**Rationale**: Calling `Load()` for each item in a loop can result in unnecessary resource usage and poor performance. Collection loads should be used instead.

**Example of problematic code**:

```csharp
foreach (DataRow dataRow in results.Tables[0].Rows)
{
    IMaterial material = _entityFactory.Create<IMaterial>();
    material.Name = (string)dataRow["Name"];
    material.Load();            // Bad: Load() inside foreach
    material.Facility.Load();   // Bad: Load() inside foreach
    materials.Add(material);
}
```

**Recommended approach**:

```csharp
// Collect all materials first
var materials = new List<IMaterial>();
foreach (DataRow dataRow in results.Tables[0].Rows)
{
    IMaterial material = _entityFactory.Create<IMaterial>();
    material.Name = (string)dataRow["Name"];
    materials.Add(material);
}

// Load all at once using collection load
materials.LoadCollection();
```

## Architecture

The linter follows a modular, extensible architecture:

```
BusinessLinter/
├── Abstractions/          # Interfaces for core components
│   ├── ILintLogger.cs     # Logging interface
│   ├── ILintRule.cs       # Base interface for linting rules
│   └── IRuleFactory.cs    # Factory for creating rule instances
├── Rules/                 # Individual linting rules
│   ├── BaseLintRule.cs    # Base class for rules
│   └── NoLoadInForeachRule.cs
├── Extensions/            # Extension methods
│   └── ServiceCollectionExtensions.cs
├── LintLogger.cs          # Console logger implementation
├── RuleFactory.cs         # Rule factory implementation
├── SolutionLinter.cs      # Main orchestrator
└── SolutionLoader.cs      # Loads solutions using Roslyn
```

## Adding New Rules

To add a new linting rule:

1. Create a new class in the `Rules/` folder that inherits from `BaseLintRule`
2. Implement the required properties and methods:
   - `RuleName`: Unique identifier for the rule
   - `RuleDescription`: Human-readable description
   - `Analyze()`: The logic to detect the code pattern

3. Register the rule in `Extensions/ServiceCollectionExtensions.cs`:

```csharp
services.AddTransient<ILintRule, YourNewRule>();
```

### Example Rule Implementation

```csharp
internal class MyCustomRule : BaseLintRule
{
    public MyCustomRule(ILintLogger logger) : base(logger) { }

    public override string RuleName => "MyCustomRule";
    
    public override string RuleDescription => "Description of what this rule checks";

    public override void Analyze(MethodDeclarationSyntax methodNode, string filePath, string className)
    {
        // Your analysis logic here
        // Use _logger.Warning() or _logger.Error() to report issues
    }
}
```

## Configuration

Rules can be enabled or disabled using the `IsEnabled` property. By default, all rules are enabled. Future enhancements may include configuration file support for more granular control.

## Testing

Unit tests for the linter are located in `tests/Specs/BusinessLinter.cs`. Tests use Moq and Autofac for dependency injection and mocking.

## Future Enhancements

- Configuration file support for enabling/disabling rules
- Rule severity levels (Warning, Error, Info)
- Custom rule parameters via configuration
- Integration with build pipelines
- HTML/JSON report generation
- Additional rules for common anti-patterns

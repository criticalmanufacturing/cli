# Centralized MES Version Validation

## Overview

This feature provides a centralized way to validate MES version requirements for CLI commands. Instead of manually adding version checks in each command's execute method, you can now declaratively specify the minimum required MES version using the `MinimumMESVersion` property on the `CmfCommand` attribute.

## How to Use

### Adding Version Requirements to Commands

To specify a minimum MES version for a command, add the `MinimumMESVersion` property to the `CmfCommand` attribute:

```csharp
[CmfCommand("taskLibrary", ParentId = "new_iot", Id = "iot_tasklibrary", MinimumMESVersion = "11.0.0")]
public class GenerateTaskLibraryCommand : TemplateCommand
{
    // No need for manual version check in Execute method anymore!
    public void Execute(IDirectoryInfo workingDir)
    {
        // Command implementation
    }
}
```

### Version Format

The version string should follow semantic versioning format: `Major.Minor.Build` (e.g., "11.0.0", "11.1.5").

### What Happens

When a command with `MinimumMESVersion` is executed:

1. The `BaseCommand.FindChildCommands` method automatically adds a validator to the command
2. Before execution, the validator checks if the current project's MES version meets the minimum requirement
3. If the version is too low, a user-friendly error message is displayed
4. If the version meets the requirement (or no minimum is specified), the command executes normally

### Error Messages

Users will see a clear error message if their MES version is too low:

```
This command requires MES version 11.0.0 or higher. Current version: 10.5.0
```

## Migration from Manual Checks

### Before (Manual Validation)

```csharp
[CmfCommand("converter", ParentId = "new_iot", Id = "iot_converter")]
public class GenerateConverterCommand : TemplateCommand
{
    public void Execute(IDirectoryInfo workingDir)
    {
        if (ExecutionContext.Instance.ProjectConfig.MESVersion.Major < 11)
        {
            throw new CliException("This command is only valid for versions above 11.0.0");
        }
        
        // Rest of the command implementation
    }
}
```

### After (Declarative Validation)

```csharp
[CmfCommand("converter", ParentId = "new_iot", Id = "iot_converter", MinimumMESVersion = "11.0.0")]
public class GenerateConverterCommand : TemplateCommand
{
    public void Execute(IDirectoryInfo workingDir)
    {
        // Command implementation - no manual check needed!
    }
}
```

## Implementation Details

### Service

The `IMESVersionValidationService` provides two methods:

- `ValidateMinimumVersion(string minimumVersion)` - Throws `MESVersionValidationException` if version is too low
- `IsVersionCompatible(string minimumVersion)` - Returns boolean indicating compatibility

### Architecture

1. **CmfCommandAttribute** - Stores the minimum version requirement
2. **MESVersionValidationService** - Performs the actual version comparison
3. **BaseCommand.FindChildCommands** - Integrates validation into command lifecycle
4. **MESVersionValidationException** - Custom exception for version validation errors

## Benefits

1. **Consistency** - All commands validate versions the same way
2. **Maintainability** - Version requirements are declared in one place
3. **Clarity** - Easy to see which commands require which versions
4. **Testability** - Centralized service is easy to test
5. **User Experience** - Consistent error messages across all commands

## When Not to Use

The attribute-based validation is for **command availability**. Use manual checks when you need:

- Conditional logic within a command based on version (e.g., choosing between template versions)
- Complex version requirements that can't be expressed as a simple minimum version
- Version-dependent feature flags within a command's execution

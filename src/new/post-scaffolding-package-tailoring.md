# post scaffolding package tailoring

## Containers:
 - **Root Package**
   - Add to dependencies
        ```json
        "dependencies":
        [
            { "id": "Cmf.Environment", "version": "8.3.0" }
        ]
        ```

## Deployment Framework:
 - **Root Package**
   - Add to dependencies
        ```json
        "dependencies":
        [
            { "id": "CriticalManufacturing", "version": "8.3.0" }
        ]
        ```
 - **Business Package**
   - Add to steps:
        ```json
        "steps":
        [
            { "order": "1", "type": "Generic", "onExecute": "$(Agent.Root)/agent/scripts/stop_host.ps1" },
            { "order": "3", "onExecute": "$(Agent.Root)/agent/scripts/start_host.ps1" }
        ]
        ```
  - **Data Package**
    - Add to steps:
        ```json
        "steps":
        [
            { "order": "1", "type": "Generic", "onExecute": "$(Agent.Root)/agent/scripts/stop_host.ps1" },
            { "order": "2", "type": "TransformFile", "file": "Cmf.Foundation.Services.HostService.dll.config" },
            { "order": "3", "type": "Generic", "onExecute": "$(Agent.Root)/agent/scripts/start_host.ps1" },
            { "order": "4", "type": "Generic", "onExecute": "$(Package[Cmf.Custom.Package].TargetDirectory)/GenerateLBOs.ps1" }
        ]
        ```

**NOTE:** Make sure that the order of the steps is the same referenced on this document
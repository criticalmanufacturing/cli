# repositories.json
This document is all you need to know about what's required in your **repositories.json** file.

### CIRepository
Path that points to a folder that contain packages that are treat as Continuous Integration packages.

### Repositories
Array of paths that point to folders that contain package that are treat as official (i.e. upstream dependencies or already releases packages).

Example:
```json
{
    "CIRepository": "\\\\fileshare\\my-continuous-integration\\packages\\repository",
    "Repositories": [
        "\\\\fileshare\\my-official\\packages\\repository",
        "\\\\fileshare\\my-released\\packages\\repository"
    ]
}
```
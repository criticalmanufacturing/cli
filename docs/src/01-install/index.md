---
hide:
  - navigation
---

# CLI Installation

This tutorial guides you through installing `@criticalmanufacturing/cli`, including prerequisites, installation steps, and validation.

## 1. Prerequisites

### Node.js and NPM

Before you begin, ensure you have the following software installed:

* Node.js;
* NPM command line interface.

#### Node.js Version Compatibility

`@criticalmanufacturing/cli` works with the latest Node.js version. However, specific scaffolding commands require compatibility with your target CM MES version:

| CM MES Version | Required Node.js Version | Required NPM Version |
|---|---|---|
| v8.x or v9.x | v12.x (Erbium) | v6.x |
| v10.x | v18.x (Hydrogen) | v9.x |
| v11.x or later | v20.x (Iron) or later | v10.x or later |

#### Installing Node.js and NPM

!!! tip "Use NVM"

    **Use a Node version manager like [nvm](https://github.com/nvm-sh/nvm) to install Node.js and NPM.**

    Avoid the usage of Node installer, since it usually installs npm in
    a directory with local permissions, which may cause permissions
    errors when you run npm packages globally.

To perform the checks or install Node.js and NPM dependencies:

1. Adapt the following PowerShell script;
2. Run it on a command line with administration privileges.

    ```PowerShell
    #SET TARGET MES MAJOR VERSION (8, 9, 10 or 11)
    $mesMajorVersion = 11

    #Map MES to node version
    $nodeMajorVerion = switch ($mesMajorVersion)
    {
        8  { "12" }
        9  { "12" }
        10 { "18" }
        11 { "20" }
    }

    # To check installed versions of Node.js and NPM use:
    nvm list

    # If not found, install them
    nvm install $nodeMajorVerion 

    #Determine the installed version
    $newNodeVersion = nvm list 
    $newNodeVersion = $newNodeVersion | where-object { $_ -Match "\s$nodeMajorVersion\." } | Select-Object -First 1
    $newNodeVersion = ($newNodeVersion | Select-String -Pattern "\d+\.\d+\.\d+").Matches.Value

    # Set appropriate node version as active
    nvm use $newNodeVersion

    # Check the active node.js version
    node -v

    # Check the active NPM version
    npm -v
    ```

### IoT Driver Dependencies (Optional)

The development of IoT drivers on Critical Manufacturing MES has a dependency for the [node-gyp][Node-Gyp] NPM package. As so, if your project requires the development of custom driver, ensure that your dev/build environment has its dependencies previously installed (check its [GitHub Page][Node-Gyp] for details).

## 2. Install CLI

 1. Open a command line with administration privileges.
 2. Execute the command:

    ```PowerShell
    npm install -g @criticalmanufacturing/cli
    ```

## 3. Validate

1. Open a command prompt.
2. Run the following command to verify the installation:

    ```PowerShell
    cmf -v
    ```

This command should output the installed version of `@criticalmanufacturing/cli` if successful.

[Node-Gyp]: https://github.com/nodejs/node-gyp

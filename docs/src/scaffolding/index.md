# Scaffolding

## Pre-requisites

Though `@criticalmanufacturing/cli` runs with the latest `node` version, to run scaffolding commands the versions required by the target MES version are __mandatory__.

For **MES v10**, the recommended versions are:

- latest node 18 (Hydrogen)
- latest npm 9 (should come with node)

For MES v8 and v9, the recommended versions are:

- latest node 12 (Erbium)
- latest npm 6 (should come with node)

Apart from those, scaffolding also needs the following dependencies:
```
npm install -g windows-build-tools
npm install -g gulp@3.9.1
npm install -g yo@3.1.1
```
For **MES v10**, you will also need [angular cli](https://angular.io/cli)
```
npm install -g @angular/cli
```

### NuGet and NPM repositories
Rarely changing information, possibly sensitive, like NuGet or NPM repositories and respective access credentials are considered infrastructure. More information on how to set up your own is available at [Infrastructure](./infrastructure.md)

### Environment Config
A valid MES installation is required to initialize your repository, either installed via Setup or via DevOps Center.
For the Setup:
- in the final step of the Setup, click Export to obtain a JSON file with your environment settings
For DevOps Center:
- Open your environment and click Export Parameters

Both these files contain sensitive information, such as user accounts and authentication tokens. They need to be provided to the `init` command with:
```
cmf init --config <config file.json> --infra...
```

## Scaffolding a repository
Let's start by cloning the empty repository.

```
git clone https://git.example/repo.git
```

Move into the repository folder

```
cd repo
```

For a classic project example, check the [traditional](./Traditional) structure documentation.

For more advanced structures, you'll probably be using [Features](./Feature Package).

## Pipelines
By default, our scaffolding doesn't provide any built-in CI/CD pipelines, giving you the flexibility to choose any tool/platform that suits your needs.

However, we can share as a reference our internal process:

### Pull Requests (PRs)
For each changed package, we run the command `cmf build --test`, which compiles the package and runs unit tests if available, comparing with the target branch.
> We consider a package as "changed" when any file is modified inside a folder with a *cmfpackage.json* file.

An alternative is to run `cmf build --test` for all packages.

### Continuous Integration (CI)
After merging code into the main branch, we perform the following steps:

1. Run `cmf build --test` to ensure successful building of all packages and passing of unit tests.
2. Run `cmf pack` to generate a package that can be installed via DevOps Center or Critical Manufacturing Setup.

### Continuous Deployment (CD)
#### Traditional (Windows VMs)
1. Follow the instructions in the [documentation](https://help.criticalmanufacturing.com/9.1/InstallationGuide/Installation)
2. In the Package Sources step, add the path where your packages are located.

#### Containers
1. Follow the instructions in the [documentation](https://portal.criticalmanufacturing.com/Info/CustomerPortal.Support/devops_center%3Eguide%3Eupgrade_mes_customer_environment)
2. Copy the generated packages to the folder defined in volume **Boot Packages**.
3. In the Configuration > General Data step, set the Package to Install as `RootPackageId@PackageVersion`
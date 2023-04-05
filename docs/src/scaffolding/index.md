# Scaffolding

## Pre-requisites

Though `@criticalmanufacturing/cli` runs with the latest `node` version, to run scaffolding commands the versions required by the target MES version are __mandatory__.

For MES v8, the recommended versions are:

- latest node 12 (Erbium)
- latest npm 6 (should come with node)

Apart from those, scaffolding also needs the following dependencies:
```
npm install -g windows-build-tools
npm install -g gulp@3.9.1
npm install -g yo@3.1.1
```

### Infrastructure
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

For a classic project example, check the [traditional](./single.md) structure documentation.

For more advanced structures, you'll probably be using [Features](./features.md).



## Continuous Integration and Delivery
The scaffolding templates provide a few pipelines designed for [Azure DevOps](https://dev.azure.com/). They work both in [Azure DevOps Server](https://azure.microsoft.com/en-us/services/devops/server/) and [Azure DevOps Services](https://dev.azure.com/).

> **IMPORTANT**: Only the pipelines for Pull Request and for Package generation (CI-Changes and CI-Package) are designed to run outside of Critical Manufacturing infrastructure. We currently do not support running the Continuous Delivery part of the pipelines in a client infrastructure.

The YAML files are available in the Builds folder at the repository root. Next to them are some JSON files which contain the metadata for the pipelines in Azure DevOps format, which can be used by directly invoking the Azure DevOps API. These files are in git ignore and they should **not** be committed, as they can contain secrets in plain text, such as Nuget credentials.

> _For CMFers_: you can use an internal tool to import the pipeline metadata, as well as the branch policies. Check "How To Import Builds" in the COMMON wiki at Docs/Pipelines.

The **CD-Containers** pipeline requires a secret to be created into the Azure DevOps library. Check more details in the [pipeline import document](./pipeline_import.md#secrets).

## Manual Pipeline import
For non-CMFs, it's simple to import the pipelines. Check out [this document](./pipeline_import.md).

## External Users
There is more available information for non-CMFers at [External Users](./external.md).
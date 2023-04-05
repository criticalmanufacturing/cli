# init

<!-- BEGIN USAGE -->

Usage
-----

```
cmf init [options] <projectName> [<rootPackageName> [<workingDir>]]
```

### Arguments

Name | Description
---- | -----------
`<projectName>` |
`<rootPackageName>` | [default: Cmf.Custom.Package]
`<workingDir>` | Working Directory [default: .]

### Options

Name | Description
---- | -----------
`--version <version>` | Package Version [default: 1.0.0]
`-c, --config <config> (REQUIRED)` | Configuration file exported from Setup [default: ]
`--repositoryUrl <repositoryUrl> (REQUIRED)` | Git repository URL
`--deploymentDir <deploymentDir> (REQUIRED)` | Deployments directory
`--MESVersion <MESVersion> (REQUIRED)` | Target MES version
`--DevTasksVersion <DevTasksVersion> (REQUIRED)` | Critical Manufacturing dev-tasks version
`--HTMLStarterVersion <HTMLStarterVersion> (REQUIRED)` | HTML Starter version
`--yoGeneratorVersion <yoGeneratorVersion> (REQUIRED)` | @criticalmanufacturing/html Yeoman generator version
`--nugetVersion <nugetVersion> (REQUIRED)` | NuGet versions to target. This is usually the MES version
`--testScenariosNugetVersion <testScenariosNugetVersion> (REQUIRED)` | Test Scenarios Nuget Version
`--infra, --infrastructure <infrastructure>` | Infrastructure JSON file [default: ]
`--nugetRegistry <nugetRegistry>` | NuGet registry that contains the MES packages
`--npmRegistry <npmRegistry>` | NPM registry that contains the MES packages
`--azureDevOpsCollectionUrl <azureDevOpsCollectionUrl>` | The Azure DevOps collection address
`--agentPool <agentPool>` | Azure DevOps agent pool
`--agentType <Cloud|Hosted>` | Type of Azure DevOps agents: Cloud or Hosted
`--ISOLocation <ISOLocation>` | MES ISO file [default: ]
`--nugetRegistryUsername <nugetRegistryUsername>` | NuGet registry username
`--nugetRegistryPassword <nugetRegistryPassword>` | NuGet registry password
`--cmfCliRepository <cmfCliRepository>` | NPM registry that contains the CLI
`--cmfPipelineRepository <cmfPipelineRepository>` | NPM registry that contains the CLI Pipeline Plugin
`--releaseCustomerEnvironment <releaseCustomerEnvironment>` | Customer Environment Name defined in DevOpsCenter
`--releaseSite <releaseSite>` | Site defined in DevOpsCenter
`--releaseDeploymentPackage <releaseDeploymentPackage>` | DeploymentPackage defined in DevOpsCenter
`--releaseLicense <releaseLicense>` | License defined in DevOpsCenter
`--releaseDeploymentTarget <releaseDeploymentTarget>` | DeploymentTarget defined in DevOpsCenter
`-?, -h, --help` | Show help and usage information


<!-- END USAGE -->

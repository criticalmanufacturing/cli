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
`-c, --config <config>` | Configuration file exported from Setup [default: ]
`--repositoryUrl <repositoryUrl>` | Git repository URL
`--deploymentDir <deploymentDir>` | Deployments directory (for releases). Don't specify if not using CI-Release. [default: ]
`--MESVersion <MESVersion>` | Target MES version
`--DevTasksVersion <DevTasksVersion>` | Critical Manufacturing dev-tasks version
`--HTMLStarterVersion <HTMLStarterVersion>` | HTML Starter version
`--yoGeneratorVersion <yoGeneratorVersion>` | @criticalmanufacturing/html Yeoman generator version
`--nugetVersion <nugetVersion>` | NuGet versions to target. This is usually the MES version
`--testScenariosNugetVersion <testScenariosNugetVersion>` | Test Scenarios Nuget Version
`--infra, --infrastructure <infrastructure>` | Infrastructure JSON file [default: ]
`--nugetRegistry <nugetRegistry>` | NuGet registry that contains the MES packages
`--npmRegistry <npmRegistry>` | NPM registry that contains the MES packages
`--azureDevOpsCollectionUrl <azureDevOpsCollectionUrl>` | The Azure DevOps collection address
`--agentPool <agentPool>` | Azure DevOps agent pool
`--agentType <Cloud|Hosted>` | Type of Azure DevOps agents: Cloud or Hosted
`--ISOLocation <ISOLocation>` | MES ISO file [default: ]
`--nugetRegistryUsername <nugetRegistryUsername>` | NuGet registry username
`--nugetRegistryPassword <nugetRegistryPassword>` | NuGet registry password
`--releaseCustomerEnvironment <releaseCustomerEnvironment>` | Customer Environment Name defined in DevOpsCenter
`--releaseSite <releaseSite>` | Site defined in DevOpsCenter
`--releaseDeploymentPackage <releaseDeploymentPackage>` | DeploymentPackage defined in DevOpsCenter
`--releaseLicense <releaseLicense>` | License defined in DevOpsCenter
`--releaseDeploymentTarget <releaseDeploymentTarget>` | DeploymentTarget defined in DevOpsCenter
`-?, -h, --help` | Show help and usage information


<!-- END USAGE -->

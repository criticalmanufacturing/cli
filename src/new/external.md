# Scaffolding and Pipelines for non-CMF Users

This document details a base approach on how to use `@criticalmanufacturing/cli` to scaffold a Product Customization solution if you are not in the CMF infrastructure. This guide assumes it is being executed in a Windows machine, but can be run in Linux with PowerShell Core if needed. Adapt as required.

## Notes on the Local Repository
In this document, it is assumed that the user has got a copy of the necessary packages from CMF. However, it is also possible that the user simply has access to the needed repositories. If this is the case, skip the repository setup steps.

If the user is setting up their own repository, this guide is using the Sonatype Nexus v3 repo as an example. This repository provides hosting for NuGet, NPM and raw (i.e. Zip file) packages. Other repositories can be used, hosted together or not. However, it may be left to the user to determine the correct endpoints. In this document, it is assumed the local repository live at `example.local` and is an OSS Nexus v3.

For hosting Nexus, the easiest way is to use the official Docker image. The setup of the repository is not detailed in this guide but information can be found in Sonatype's help page.

The Nexus repository used in this guide needs to:

1. Have a NuGet repository. In this guide it is called `nuget-hosted`. We do not use the proxy or grouped repository types, as the public packages are directly referenced from nuget.org
1. Have an NPM repository. In this guide it is called `npm` and is of type `npm-grouped`. Unlike NuGet, here we do setup proxy and grouped repositories. The configuration is as follows:

    1. Hosted npm repository `npm-hosted`: this will contain the private CMF packages

    1. Proxy npm repository `npm-proxy`: this repository only proxies public package request to npmjs.org

    1. Grouped npm repository `npm`: this repository is a group of the `npm-hosted` and `npm-proxy` repositories.
1. In Settings > Realms, have NPM Bearer Token and NuGet API Key realms active. The API Key can be obtained from the user profile.

### Authentication
By default, NuGet and NPM repositories operate with anonymous access, requiring only credentials to publish. If this does not meet your requirements, you can opt to require authentication to all feeds.

#### NuGet
1. In Nexus, go to Settings > Security > Anonymous Access and disable anonymous access.
1. When running `init`, provide an infrastructure file that contains the credentials like so:

    "NuGetRegistryUsername": "<user>",
    "NuGetRegistryPassword": "<password>"

If provided, the NuGet restore calls will be authenticated. Note that currently there is no way to do the restore using NuGet ApiKeys.

#### <a id="npmauth"></a> NPM
1. login to your NPM repo with the build account (This is also done when publishing the packages but for the hosted repo. In this case the grouped repo must be used):

        npm adduser --registry=http://example.local/repository/npm/
    
1. in the .npmrc file in your home folder, a new line should be added, in the format `//example.local/repository/npm/:_authToken=NpmToken.<GUID>`

1. take this line and add it to the .npmrc in both the Help and HTML solutions. The files are always in the `apps/<web solution>` path

1. commit these files

> NOTE: this file will be visible to anyone with source code access! If push permissions are not desired here, use an account that can only download packages!

In some cases `npm adduser` may fail. A known case is when using Personal Access Tokens (PATs) instead of passwords in Windows systems. In these cases, it's possible to directly use the credentials without this authentication step:

1. edit the `.npmrc` file in the HTML and Help package directories with your text editor

1. fill in the repository: `registry=https://example.io/repository/npm/`

1. fill in the credentials. The `_auth` string is obtained by doing a Base 64 encode of `<user>:<password>` with the respective username and password of the repository account: `//example.io/repository/npm/:_auth="<base64 string>"`

1. tell NPM to always authenticate: `//example.io/repository/npm/:always_auth=true`

So if your username is `user1` and your password is `secret2` the .npmrc files would look like
```ini
registry=https://example.io/repository/npm/
//example.io/repository/npm/:_auth="dQBzAGUAcgAxADoAcwBlAGMAcgBlAHQAMgA=" 
//example.io/repository/npm/:always_auth=true
```

where the auth string can be generated in Powershell using
```powershell
[Convert]::ToBase64String([System.Text.Encoding]::Unicode.GetBytes("user1:secret2"))
```
## Process

### Host packages in local repository
If using a local repository, the dependency packages need to be hosted there. CMF will have provided a package with the NuGet packages. The NPM packages are extracted from the MES ISO file. The instructions are available either on your Help website or in CMF's general [Help repository](https://help.criticalmanufacturing.com/8.0/Development/Tutorial%3EPresentation%3Elocalrepository).

However, currently the script provided in the Help tutorial only uploads the HTML solution dependencies, not the Help solution ones. As such, an updated copy of the script can be provided if requested.

It is recommended that you follow the Help tutorial, but a few quick steps will be detailed here

1. Authenticate to your local repo: 

        npm adduser --registry=http://example.local/repository/npm-hosted/

    1. NOTE: If using Nexus OSS, you will have to upload to the `npm-hosted` repo, not to `npm`

1. Run

    *You only need to specify the Username if you don't have anonymous access to your repository* 

        .\LoadNPMPackagesToLocalRepository.ps1 -PathToISO "<ISO path>" "http://example.local/repository/npm-hosted/" -ExtractionFolder "C:\_CMTemp\packages" [-Username <User>]

    1.  the script will automatically mount the ISO, extract the packages and upload them to the repository

1. OPTIONAL: tag the loaded packages with the dist-tags used by the solutions. If this is not desired, the scaffolded solutions will have to be changed to point to the exact versions that are uploaded into the repository. Adding the dist-tags is generally easier. The dist-tags always follow the format `release-<version>` with no dots. As an example, for MES version 8.1.0 the dist-tag commands would be:

        npm dist-tag add @criticalmanufacturing/mes-ui-web@8.1.0-202103302 release-810 --registry=http://example.local/repository/npm-hosted/
	    
        npm dist-tag add cmf.docs.web@8.1.0-20210329.4 release-810 --registry=http://example.local/repository/npm-hosted/

## Loading the scaffolding into the target project

### Using a local repository
If using a local repository, the first step is to load the NuGet dependencies. Until this is done the Business and Test solutions will not compile. 

Place the packages in a folder. Open a powershell session in `Tools` and run the `Deploy_LoadAllNuGetPackages.ps1`script. Don't forget to authenticate first.

```
cd <folder>
nuget setapikey <nuget key> -source http://example.local/repository/nuget-hosted

# for each nuget file:
nuget push <file.nupkg> -source http://local.example/repository/nuget-hosted/
```

### Commiting the code
If you did not run `init` and `new` inside a cloned git repository folder, you can add it as a remote in place. For the second approach, open a PS session where you ran the `init` and run:

```
git remote add origin https://<user>@dev.azure.com/<organization>/<project>/_git/<repository>
git push -u origin --all
```

### Loading pipelines and policies
To run the pipelines, some tasks need to be installed in Azure DevOps. These are:

-	MSPremier.PostBuildCleanup.PostBuildCleanup-task.PostBuildCleanup
-	qetza.replacetokens

You can now load the Pipelines and Policies into your project.
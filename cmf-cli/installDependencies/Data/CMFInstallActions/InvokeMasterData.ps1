# Powershell script to load CM master data
# Author: Oscar Martins
# Last Modified by: Oscar Martins
# Last Modified on: 10/05/2017
# --------------------------------------------------------------------------
param(
	[string]$EnvironmentConfigName,
	[switch]$InteractiveMode = $False,
	[string]$MasterDataFilePath,
	[string]$MasterDataFolderPath,
	[string]$DeeRulesPath,
	[string]$AutomationWorkflowFilesPath,
	[string]$ChecklistImageFilesPath,
	[string]$ExportedObjectsFilesPath,
	[switch]$ExecuteAsSubScript
)

if ([string]::IsNullOrWhiteSpace($EnvironmentConfigName)) {
	throw "Missing mandatory parameter: EnvironmentConfigName"
}

################################################
# Variables

$ErrorActionPreference = "Stop"
$global:InteractiveMode = $InteractiveMode
$EXITCODE = 0


# Scripts & Configs
$scriptPath = Split-Path $MyInvocation.MyCommand.Path
$packageRootFolder = Resolve-Path (Join-Path $scriptPath '..') # change to match package root folder
$cmfInstallActionsFolder = "$packageRootFolder\CMFInstallActions"
$jsonConfigFilePath = "$packageRootFolder\EnvironmentConfigs\$EnvironmentConfigName.json"
$loadedModule = $false

# Load Module with CMF Install actions and Environment configuration
if (!(Get-Module CMFInstallActions)) {
	Import-Module $cmfInstallActionsFolder\CMFInstallActions.psd1
	LogWrite (" *  CMFInstallActions module imported") -ForegroundColor Green

	$loadedModule = $true
}

$env = (Get-CMFEnvironment -environmentConfigPath $jsonConfigFilePath -cmfInstallActionsPath $cmfInstallActionsFolder)

if ($loadedModule) {
	Get-MDLDependencies $env
}

try {
	if ( -not $ExecuteAsSubScript ) {
		LogWrite "Loading Master Data" -ForegroundColor Cyan
		LogWrite "------------------------------------"
		LogWrite "Script Input Parameters" -ForegroundColor Cyan
		LogWrite "------------------------------------"
		LogWrite ("MasterDataFilePath: $MasterDataFilePath" )
		LogWrite ("MasterDataFolderPath: $MasterDataFolderPath" )
		LogWrite ("DeeRulesPath: $DeeRulesPath" )
		LogWrite ("AutomationWorkflowFilesPath: $AutomationWorkflowFilesPath" )
		LogWrite ("ChecklistImageFilesPath: $ChecklistImageFilesPath" )
		LogWrite ("ExportedObjectsFilesPath: $ExportedObjectsFilesPath" )
		LogWrite ("EnvironmentConfigName: $EnvironmentConfigName")
		LogWrite ("InteractiveMode: $InteractiveMode")
		LogWrite ("Global.InteractiveMode: $($global:InteractiveMode)")
		LogWrite "------------------------------------"
	}

	if ($AutomationWorkflowFilesPath -and (Test-Path ("$AutomationWorkflowFilesPath"))) {
		$automationWorkflowFilesBasePath = "$AutomationWorkflowFilesPath"
	}
	
	if ($ChecklistImageFilesPath -and (Test-Path ("$ChecklistImageFilesPath"))) {
		$checklistImageFilesBasePath = "$ChecklistImageFilesPath"
	}

	if ($ExportedObjectsFilesPath -and (Test-Path ("$ExportedObjectsFilesPath"))) {
		$exportedObjectsFilesBasePath = "$ExportedObjectsFilesPath"
	}
	
	if ((-not $MasterDataFolderPath) -and (-not $MasterDataFilePath)) {
		throw 'One of the following parameters are mandatory: MasterDataFilePath or MasterDataFolderPath'
	}
	
	# Run master data by folder
	if ($MasterDataFolderPath) {
		# Run master data by folder
		$output = Invoke-MasterDataLoaderByFolder $env    `
			-excelFolder $MasterDataFolderPath    `
			-deeBasePath $DeeRulesPath    `
			-automationWorkflowFilesBasePath $automationWorkflowFilesBasePath    `
			-checklistImageBasePath $checklistImageFilesBasePath    `
			-importObjectBasePath $exportedObjectsFilesBasePath
	}
	else {	
		# Run master data by file
		$output = Invoke-MasterDataLoader $env    `
			-excelFile $MasterDataFilePath    `
			-deeBasePath $DeeRulesPath    `
			-automationWorkflowFilesBasePath $automationWorkflowFilesBasePath    `
			-checklistImageBasePath $checklistImageFilesBasePath    `
			-importObjectBasePath $exportedObjectsFilesBasePath
	}
}
catch {
	if ( $ExecuteAsSubScript ) {
		throw $_
	}
 else {
		$ErrorMessage = $_.Exception.Message
		LogWrite (" ! " + $ErrorMessage) -ForegroundColor Red
    
		LogWrite (" !! Installation Aborted!") -ForegroundColor Red -backgroundColor DarkRed
		LogWrite "`n * No rollback action taken..." -ForegroundColor Yellow
	}
}
finally {
	if ( $loadedModule ) {
		LogWrite " *  CMFInstallActions module removed" -ForegroundColor Green
		Remove-Module CMFInstallActions
	}
}
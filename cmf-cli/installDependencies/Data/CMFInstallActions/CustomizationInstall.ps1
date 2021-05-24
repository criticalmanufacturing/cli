# Powershell script to load CMF Customizations into a CM MES Installation
# Author: Luis Ponte
# Last Modified by: PI DevOps
# Last Modified on: 2020/07/07
# --------------------------------------------------------------------------
param(
    [string]$EnvironmentConfigName,
    [switch]$InteractiveMode = $false,
    [string]$PlantName = "",
    [switch]$BackupBefore = $true,
    [switch]$RunAppInstall = $true
)

if ([string]::IsNullOrWhiteSpace($EnvironmentConfigName))
{
    throw "Missing mandatory parameter: EnvironmentConfigName"
}

################################################
# Variables

$ErrorActionPreference = "Stop"
$global:InteractiveMode = $InteractiveMode
$EXITCODE = 0
$sortRegex = { [regex]::Replace($_, '\d+', { $args[0].Value.PadLeft(20) }) }


# Scripts & Configs
$scriptPath = Split-Path $MyInvocation.MyCommand.Path
$packageRootFolder = Resolve-Path (Join-Path $scriptPath '..') # change to match package root folder
$cmfInstallActionsFolder = "$packageRootFolder\CMFInstallActions"
$runMasterDataScript = "$cmfInstallActionsFolder\InvokeMasterData.ps1"
$runAppInstallScript = "$cmfInstallActionsFolder\AppInstall.ps1"
$jsonConfigFilePath = "$packageRootFolder\EnvironmentConfigs\$EnvironmentConfigName.json"

# Load Module with CMF Install Actions and Environment configuration
Import-Module $cmfInstallActionsFolder\CMFInstallActions.psd1
$env = (Get-CMFEnvironment -environmentConfigPath $jsonConfigFilePath -cmfInstallActionsPath $cmfInstallActionsFolder)

$customizationVersion = ParseYamlVariables -File "$packageRootFolder\EnvironmentConfigs\GlobalVariables.yml" -VariableName "CurrentPackage"
$releaseID = "Release $customizationVersion"
$backupIdentifier = 'BeforePackage_' + $customizationVersion

# Business
$businessFolder = "$packageRootFolder\Business"
$backupBusinessTier = ($RunAppInstall -and (Test-Path -Path "$businessFolder"))

# HTML
$htmlFolder = "$packageRootFolder\UI\HTML"
$backupUIHtml = ($RunAppInstall -and (Test-Path -Path "$htmlFolder"))

# Help
$helpFolder = "$packageRootFolder\UI\Help"
$backupUIHelp = ($RunAppInstall -and (Test-Path -Path "$helpFolder"))

# DeeRules
$deeRulesFolder = "$packageRootFolder\DeeRules"

# ProcessRules
$processRulesFolder = "$deeRulesFolder\ProcessRules"
$processRulesFolderEntityTypes = "$processRulesFolder\EntityTypes"

# Powershell
$powershellFolder = "$packageRootFolder\Powershell"

# MasterData
$masterDataFolder = "$packageRootFolder\MasterData"

# ExportedObjects
$exportedObjectsFolder = "$packageRootFolder\ExportedObjects"

# Database
$databaseFolder = "$packageRootFolder\Database"
$databaseFolderOnline = "$databaseFolder\Online";
$databaseFolderODS= "$databaseFolder\ODS";
$databaseFolderDWH= "$databaseFolder\DWH";

# Reports
$reportsFolder = "$packageRootFolder\Reports"

# MasterDataFiles
$automationWorkflowFilesFolder = "AutomationWorkflowFiles"
$checklistImageFilesFolder = "ChecklistImages"
$exportedObjectsMDFolder = "ExportedObjects"

# IoT
$automationCustomPackagesFolder = "$packageRootFolder\AutomationCustomPackages"

# Defaults Before & After
$before = "Before"
$after = "After"
$plantBefore ="$PlantName\$before"
$plantAfter = "$PlantName\$after"

try {
    LogWrite "CM MES Customization Installation" -ForegroundColor Cyan
    LogWrite "------------------------------------"
    LogWrite ("Release Package ID    > " + $releaseID)
    LogWrite ("Customization version > " + $customizationVersion)
    LogWrite ("Backup Identifier > " + $BackupIdentifier)
    LogWrite "------------------------------------"

    LogWrite "Script Input Parameters" -ForegroundColor Cyan
    LogWrite "------------------------------------"
    LogWrite ("EnvironmentConfigName: $EnvironmentConfigName")
    LogWrite ("InteractiveMode: $InteractiveMode")
    LogWrite ("Global.InteractiveMode: $global:InteractiveMode")
    LogWrite ("PlantName: $PlantName")
    LogWrite ("BackupBefore: $BackupBefore")
    LogWrite ("RunAppInstall: $RunAppInstall")
    LogWrite "------------------------------------"

    $DoBackup = $BackupBefore
    if ($env:IsBackupDone) {
        if ($env:IsBackupDone -eq $true) {
            $DoBackup = $false
            $BackupBefore = $false;
        }
    }

    if($DoBackup -eq $true)
    {
        $systemBackupPSExpression = "& '$packageRootFolder\DeploymentTools\SystemBackup.ps1' " +
            "-EnvironmentConfigName '$EnvironmentConfigName' " +
            "-backupIdentifier '$backupIdentifier' " +
            "-InteractiveMode:$" + $InteractiveMode + " " +
            "-FullBackup:$" + "$false " +
            "-BackupDBOnline " +
            "-BackupDBODS " +
            "-BackupDBDWH " +
            "-BackupBusinessTier:$" + "$BackupBusinessTier " +
            "-BackupUIHtml:$" + "$BackupUIHtml " +
            "-BackupUIHelp:$" + "$BackupUIHelp " +
            "-backupBasedOnDeliverables"

        Invoke-Expression $systemBackupPSExpression
    }
    else
    {
        LogWrite "Backup skipped as it was done in a previous step of the execution thread" -foregroundColor Yellow
    }

    Get-MDLDependencies $env

    # Disable perform transformations on host configuration files
    Install-CMFBusinessConfigTransformation $env -transformFile "$cmfInstallActionsFolder\DownTimeConfig.xdt"

    # Disabling ERP integration if needed
    $isERPEnabled = Get-Config $env -configPath '/Cmf/System/Configuration/ERP/IsActive/'
    if ($isERPEnabled)
    {
        $config = New-Config -parentPath '/Cmf/System/Configuration/ERP' -name 'IsActive' -Value 'FALSE' -ValueType 'Boolean'
        Set-Config $env $config
    }
    
    #########################################################################################################################
    # Execute load entity types process rules
    
    if (Test-Path "$processRulesFolderEntityTypes") 
    {
        LogWrite "Executing entity type process rules..." -foregroundColor Green
        LoadProcessRules $env "$processRulesFolderEntityTypes"
    }

    ################################################################################################################################
    # Execute pre process rules

    if (Test-Path "$processRulesFolder") 
    {   
        foreach ($folder in Get-ChildItem $processRulesFolder -Directory | Sort-Object $sortRegex) 
        {        
            $processRulesFolderBefore = "$processRulesFolder\" + $folder.Name + "\$before"
            if (Test-Path "$processRulesFolderBefore") 
            {
                LogWrite "Executing $folder process rules (pre-deployment)..." -foregroundColor Green            
                LoadProcessRules $env "$processRulesFolderBefore"
            }

            if($PlantName)
            {
                $processRulesFolderPlantBefore = "$processRulesFolder\" + $folder.Name + "\$plantBefore"
                if (Test-Path "$processRulesFolderPlantBefore") 
                {
                    LogWrite "Executing $folder process rules (pre-deployment)..." -foregroundColor Green            
                    LoadProcessRules $env "$processRulesFolderPlantBefore"
                }
            }            
        }
    }


    ##################################################################################################################################################
    # Execute pre powershell scripts

    if (Test-Path "$powershellFolder")
    {   
        foreach ($folder in Get-ChildItem $powershellFolder -Directory | Sort-Object $sortRegex) 
        {
            $powershellFolderBefore = "$powershellFolder\" + $folder.Name + "\$before"
            if (Test-Path "$powershellFolderBefore")
            {
                foreach ($file in Get-ChildItem $powershellFolderBefore -File -Filter '*.ps1') 
                {
                    Logwrite "Executing Powershell script $file" -foregroundColor Green
                    $fileFullPath = "$powershellFolderBefore\$file"
                    Invoke-Expression "& '$fileFullPath' -EnvironmentConfigName $EnvironmentConfigName"
                }
            }
            
            if($PlantName)
            {
                $powershellFolderPlantBefore = "$powershellFolder\" + $folder.Name + "\$plantBefore"
                if (Test-Path "$powershellFolderPlantBefore")
                {
                    foreach ($file in Get-ChildItem $powershellFolderPlantBefore -File -Filter '*.ps1') 
                    {
                        Logwrite "Executing Powershell script $file" -foregroundColor Green
                        $fileFullPath = "$powershellFolderPlantBefore\$file"
                        Invoke-Expression "& '$fileFullPath' -EnvironmentConfigName $EnvironmentConfigName"
                    }
                }
            }
        }
    }

  

   
    ######################################################################################################################################   
    # Run pre SQL Scripts
   
    if (Test-Path "$databaseFolder") 
    {
        if (Test-Path $databaseFolderOnline) 
        {
            foreach ($folder in Get-ChildItem $databaseFolderOnline -Directory | Sort-Object $sortRegex) 
            {
                $databaseFolderBefore = "$databaseFolderOnline\" + $folder.Name + "\$before"
                if(Test-Path $databaseFolderBefore)
                {
                    LogWrite ("Executing Online DB scripts from $databaseFolderBefore") -foregroundColor Green
                    Invoke-PackageSqlScripts $env -database 'Online' -folderPath $databaseFolderBefore
                }

                if($PlantName)
                {
                    $databaseFolderPlantBefore = "$databaseFolderOnline\" + $folder.Name + "\$plantBefore"
                    if(Test-Path $databaseFolderPlantBefore)
                    {
                        LogWrite ("Executing Online DB scripts from $databaseFolderPlantBefore") -foregroundColor Green
                        Invoke-PackageSqlScripts $env -database 'Online' -folderPath $databaseFolderPlantBefore
                    }
                }
            }
        }
    
        if (Test-Path "$databaseFolderODS") 
        {
            foreach ($folder in Get-ChildItem $databaseFolderODS -Directory | Sort-Object $sortRegex) 
            {
                $databaseFolderBefore = "$databaseFolderODS\" + $folder.Name + "\$before"
                if(Test-Path "$databaseFolderBefore")
                {
                    LogWrite ("Executing ODS DB scripts from $databaseFolderBefore") -foregroundColor Green
                    Invoke-PackageSqlScripts $env -database 'ODS' -folderPath $databaseFolderBefore
                }
                
                if($PlantName)
                {
                    $databaseFolderPlantBefore = "$databaseFolderODS\" + $folder.Name + "\$plantBefore"
                    if(Test-Path "$databaseFolderPlantBefore")
                    {
                        LogWrite ("Executing ODS DB scripts from $databaseFolderPlantBefore") -foregroundColor Green
                        Invoke-PackageSqlScripts $env -database 'ODS' -folderPath $databaseFolderPlantBefore
                    }
                }
            }
        }

        if (Test-Path "$databaseFolderDWH") 
        {
            foreach ($folder in Get-ChildItem $databaseFolderDWH -Directory | Sort-Object $sortRegex) 
            {
                $databaseFolderBefore = "$databaseFolderDWH\" + $folder.Name + "\$before"
                if(Test-Path "$databaseFolderBefore")
                {
                    LogWrite ("Executing DWH DB scripts from $databaseFolderBefore") -foregroundColor Green
                    Invoke-PackageSqlScripts $env -database 'DWH' -folderPath $databaseFolderBefore
                }

                if($PlantName)
                {
                    $databaseFolderPlantBefore = "$databaseFolderDWH\" + $folder.Name + "\$plantBefore"
                    if(Test-Path "$databaseFolderPlantBefore")
                    {
                        LogWrite ("Executing DWH DB scripts from $databaseFolderPlantBefore") -foregroundColor Green
                        Invoke-PackageSqlScripts $env -database 'DWH' -folderPath $databaseFolderPlantBefore
                    }
                }
            }
        }
    }

    ##########################################################################################################################
    # Run IoT Packages Deploy
    if (Test-Path "$automationCustomPackagesFolder")
    {
        IoTPackages-Deploy $env $packageRootFolder
    }
   
    #########################################################################################################################################
    #   Run Master Data

    if (Test-Path "$masterDataFolder") 
    {
        # If no DEERulesFolder exists the variable will be empty
        if(Test-Path "$deeRulesFolder")
        {
            $deeRulesFolderPath = "$deeRulesFolder";
        }
        
        foreach ($f in Get-ChildItem $masterDataFolder -Directory | Sort-Object $sortRegex) 
        {
            $folder = $f.FullName

            # If no AutomationWorkFlowFiles exists the variable will be empty
			if (Test-Path "$folder\$automationWorkflowFilesFolder") 
			{
				$automationWorkflowFilesBasePath = "$folder\$automationWorkflowFilesFolder"
			}

            # If no ChecklistImageFiles exists the variable will be empty
            if(Test-Path "$folder\$checklistImageFilesFolder")
            {
                $checklistImageFilesBasePath = "$folder\$checklistImageFilesFolder"
            }

            # If no ExportedObjects exists the variable will be empty
            if(Test-Path "$folder\$exportedObjectsMDFolder")
            {
                $exportedObjectsMDBasePath = "$folder\$exportedObjectsMDFolder"
            }

            $runMasterDataPSExpression = "& '$runMasterDataScript'" +
            " -EnvironmentConfigName '$EnvironmentConfigName'" +
            " -MasterDataFolderPath '$folder'" +
            " -DeeRulesPath '$deeRulesFolderPath'" +
            " -AutomationWorkflowFilesPath '$automationWorkflowFilesBasePath'" +
            " -ChecklistImageFilesPath '$checklistImageFilesBasePath'" +
            " -ExportedObjectsFilesPath '$exportedObjectsMDBasePath'" +
            " -ExecuteAsSubScript" +
            " -InteractiveMode:$" + $InteractiveMode
            Invoke-Expression $runMasterDataPSExpression

            $plantFolder = "$folder\$PlantName"
            if($PlantName -and (Test-Path $plantFolder))
            {
                # If no ChecklistImageFiles exists the variable will be empty
                if(Test-Path "$plantFolder\$checklistImageFilesFolder")
                {
                    $checklistImageFilesBasePath = "$plantFolder\$checklistImageFilesFolder"
                }

                # If no ExportedObjects exists the variable will be empty
                if(Test-Path "$plantFolder\$exportedObjectsMDFolder")
                {
                    $exportedObjectsMDBasePath = "$plantFolder\$exportedObjectsMDFolder"
                }

                $runMasterDataPSExpression = "& '$runMasterDataScript'" +
                " -EnvironmentConfigName '$EnvironmentConfigName'" +
                " -MasterDataFolderPath '$plantFolder'" +
                " -DeeRulesPath '$deeRulesFolderPath'" +
                " -AutomationWorkflowFilesPath '$automationWorkflowFilesBasePath'" +
                " -ChecklistImageFilesPath '$checklistImageFilesBasePath'" +
                " -ExportedObjectsFilesPath '$exportedObjectsMDBasePath'" +
                " -ExecuteAsSubScript" +
                " -InteractiveMode:$" + $InteractiveMode
                Invoke-Expression $runMasterDataPSExpression
            }
        }
    }

    ##########################################################################################################################
    # Run post SQL Scripts  
    
    if (Test-Path "$databaseFolder") 
    {
        if (Test-Path "$databaseFolderOnline") 
        {
            foreach ($folder in Get-ChildItem $databaseFolderOnline -Directory | Sort-Object $sortRegex) 
            {
                $databaseFolderAfter = "$databaseFolderOnline\" + $folder.Name + "\$after"
                if(Test-Path "$databaseFolderAfter")
                {
                    LogWrite ("Executing Online DB scripts from $databaseFolderAfter") -foregroundColor Green
                    Invoke-PackageSqlScripts $env -database 'Online' -folderPath $databaseFolderAfter
                }

                if($PlantName)
                {
                    $databaseFolderPlantAfter = "$databaseFolderOnline\" + $folder.Name + "\$plantAfter"
                    if(Test-Path "$databaseFolderPlantAfter")
                    {
                        LogWrite ("Executing Online DB scripts from $databaseFolderPlantAfter") -foregroundColor Green
                        Invoke-PackageSqlScripts $env -database 'Online' -folderPath $databaseFolderPlantAfter
                    }
                }
            }
        }
    
        if (Test-Path "$databaseFolderODS") 
        {
            foreach ($folder in Get-ChildItem $databaseFolderODS -Directory | Sort-Object $sortRegex) 
            {
                $databaseFolderAfter = "$databaseFolderODS\" + $folder.Name + "\$after"
                if(Test-Path "$databaseFolderAfter")
                {
                    LogWrite ("Executing ODS DB scripts from $databaseFolderAfter") -foregroundColor Green
                    Invoke-PackageSqlScripts $env -database 'ODS' -folderPath $databaseFolderAfter
                }

                if($PlantName)
                {
                    $databaseFolderPlantAfter = "$databaseFolderODS\" + $folder.Name + "\$plantAfter"
                    if(Test-Path "$databaseFolderPlantAfter")
                    {
                        LogWrite ("Executing ODS DB scripts from $databaseFolderPlantAfter") -foregroundColor Green
                        Invoke-PackageSqlScripts $env -database 'ODS' -folderPath $databaseFolderPlantAfter
                    }
                }
            }
        }

        if (Test-Path "$databaseFolderDWH") 
        {
            foreach ($folder in Get-ChildItem $databaseFolderDWH -Directory | Sort-Object $sortRegex) 
            {
                $databaseFolderAfter = "$databaseFolderDWH\" + $folder.Name + "\$after"
                if(Test-Path "$databaseFolderAfter")
                {
                    LogWrite ("Executing DWH DB scripts from $databaseFolderAfter") -foregroundColor Green
                    Invoke-PackageSqlScripts $env -database 'DWH' -folderPath $databaseFolderAfter
                }

                if($PlantName)
                {
                    $databaseFolderPlantAfter = "$databaseFolderDWH\" + $folder.Name + "\$plantAfter"
                    if(Test-Path "$databaseFolderPlantAfter")
                    {
                        LogWrite ("Executing DWH DB scripts from $databaseFolderAfter") -foregroundColor Green
                        Invoke-PackageSqlScripts $env -database 'DWH' -folderPath $databaseFolderAfter
                    }
                }
            }
        }

        # Restart host
        Stop-NavigoHosts $env -disableNLBManagement
        Start-NavigoHosts $env -disableNLBManagement
    }

    ##################################################################################################################################################
    # Execute post powershell scripts

    if (Test-Path "$powershellFolder")
    {
        foreach ($folder in Get-ChildItem $powershellFolder -Directory | Sort-Object $sortRegex) 
        {
            $powershellFolderAfter = "$powershellFolder\" + $folder.Name + "\$after"
            if (Test-Path "$powershellFolderAfter")
            {
                foreach ($file in Get-ChildItem $powershellFolderAfter -File -Filter '*.ps1') 
                {
                    Logwrite "Executing Powershell script $file" -foregroundColor Green
                    $fileFullPath = "$powershellFolderAfter\$file"
                    Invoke-Expression "& '$fileFullPath' -EnvironmentConfigName $EnvironmentConfigName"
                }
            }

            if($PlantName)
            {
                $powershellFolderPlantAfter = "$powershellFolder\" + $folder.Name + "\$plantAfter"
                if (Test-Path "$powershellFolderPlantAfter")
                {
                    foreach ($file in Get-ChildItem $powershellFolderPlantAfter -File -Filter '*.ps1') 
                    {
                        Logwrite "Executing Powershell script $file" -foregroundColor Green
                        $fileFullPath = "$powershellFolderPlantAfter\$file"
                        Invoke-Expression "& '$fileFullPath' -EnvironmentConfigName $EnvironmentConfigName"
                    }
                }
            }
        }
    }
   
    ################################################################################################################################
    # Execute post process rules

    if (Test-Path "$processRulesFolder") 
    {
        foreach ($folder in Get-ChildItem "$processRulesFolder" -Directory | Sort-Object $sortRegex) 
        {        
            $processRulesFolderAfter = "$processRulesFolder\" + $folder.Name + "\$after"
            if (Test-Path "$processRulesFolderAfter") 
            {
                LogWrite "Executing $folder process rules (post-deployment)..." -foregroundColor Green            
                LoadProcessRules $env "$processRulesFolderAfter"
            }

            if($PlantName)
            {
                $processRulesFolderPlantAfter = "$processRulesFolder\" + $folder.Name + "\$plantAfter"
                if (Test-Path "$processRulesFolderPlantAfter") 
                {
                    LogWrite "Executing $folder process rules (post-deployment)..." -foregroundColor Green            
                    LoadProcessRules $env "$processRulesFolderPlantAfter"
                }
            }
        }
    }
      
    ########################################################################################################################## 
    # Import Exported Objects

    if (Test-Path "$exportedObjectsFolder") 
    {
        foreach ($f in Get-ChildItem "$exportedObjectsFolder" -Directory | Sort-Object $sortRegex) 
        {
            $folder = $f.FullName
            Import-NavigoObjects $env "$folder" -securityToken "customImport"

            $plantFolder = "$folder\$PlantName"
            if($PlantName -and (Test-Path $plantFolder))
            {
                Import-NavigoObjects $env "$plantFolder" -securityToken "customImport"
            }
        }
    }
  
    ##########################################################################################################################
    # Deploy Reports & Resources in Report Server

    if(Test-Path $reportsFolder)
    {
        foreach ($f in Get-ChildItem "$reportsFolder" -Directory  | Sort-Object $sortRegex) 
        {
            $folder = $f.FullName
            Install-ReportsInFolder -env $env -folderPath "$folder"

            $plantFolder = "$folder\$PlantName"
            if($PlantName -and (Test-Path $plantFolder))
            {
                Install-ReportsInFolder -env $env -folderPath "$plantFolder"
            }
        }
    }

    
    ##########################################################################################################################

    LogWrite "Executing host configuration file transformation to $cmfInstallActionsFolder\UpTimeConfig.xdt" -foregroundColor Green
    Install-CMFBusinessConfigTransformation $env -transformFile "$cmfInstallActionsFolder\UpTimeConfig.xdt"

    # Enabling ERP integration
	if ($isERPEnabled)
    {
		$config = New-Config -parentPath '/Cmf/System/Configuration/ERP' -name 'IsActive' -Value 'TRUE' -ValueType 'Boolean'
		Set-Config $env $config
	}

    #LogWrite "Updating customization version"  
    #Set-CustomizationVersion $env  -version $customizationVersion

    #LogWrite "Starting and stopping hosts to ensure all configurations are reloaded"
    #Stop-NavigoHosts $env  -disableNLBManagement
    #Start-NavigoHosts $env -disableNLBManagement

    # Generate LBOs
    #LogWrite "Generating LBOs..." -foregroundColor Green
    #Update-LightBusinessObjects $env -isToGenerateWCF $false

    LogWrite "`n >> INSTALLATION COMPLETE`n" -foregroundColor Green -backgroundColor DarkGreen

    $EXITCODE = 0

}

catch 
{
    $ErrorMessage = $_.Exception.Message
    LogWrite (" ! " + $ErrorMessage) -ForegroundColor Red
	
    $restoreSystem = $true
    if ($global:InteractiveMode -eq "" -Or $global:InteractiveMode -eq $true) 
    {
        Write-host "    Rollback system to " $backupIdentifier "?"
        Write-Host "       Rollback [r]" -ForegroundColor Yellow -NoNewline
        Write-Host "       Bypass rollback [b]: " -NoNewline
		                
        $confirmation = Read-Host 

        $restoreSystem = ( $confirmation -ne 'b' )
    }
	
    if ( $BackupBefore -and $restoreSystem ) 
    {	
        LogWrite (" !! Installation Aborted!") -ForegroundColor Red -backgroundColor DarkRed
        LogWrite "`n * Initiating rollback..." -ForegroundColor Yellow

        Stop-NavigoHosts $env

        $systemRestorePSExpression = "& '$PackageRootFolder\DeploymentTools\SystemRestore.ps1' " +
        "-EnvironmentConfigName '$EnvironmentConfigName' " +
        "-backupIdentifier 'DoNotBackup' " +
        "-restoreIdentifier '$backupIdentifier' " +
        "-InteractiveMode:$" + $InteractiveMode + " " +
        "-FullRestore:$" + "$false " +
        "-RestoreDBOnline " +
        "-RestoreDBODS " +
        "-RestoreDBDWH " +
        "-RestoreBusinessTier:$" + "$backupBusinessTier " +
        "-RestoreUIHtml:$" + "$backupUIHtml " +
        "-RestoreUIHelp:$" + "$backupUIHelp "
        
        Invoke-Expression $systemRestorePSExpression

        Start-NavigoHosts $env

        LogWrite "`n >> Rollback complete!`n" -foregroundColor Black -backgroundColor Yellow
    }

    $EXITCODE = 1
}
finally 
{
    Remove-Module CMFInstallActions
}
Exit $EXITCODE
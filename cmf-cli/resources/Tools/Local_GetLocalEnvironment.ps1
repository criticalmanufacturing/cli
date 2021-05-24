# ************************************************************************************************
# Summary: Get Host files from remote virtual machine
# Author: Cesar Meira
# ************************************************************************************************

Param(
  [string]$ScriptPath,
  [string]$hostname,
  [string]$TARGET = ".\..\LocalEnvironment",
  [string]$SOURCE # installation path
)

$SOURCE = $SOURCE.Replace("`"","").Replace("\\", "\")
$hostname = $hostname.Replace("`"","")

$sourceDir = "\\${hostname}`\" + $SOURCE -replace ':', '$'
$targetDir = $TARGET
$BUSINESS_TIER = "$sourceDir\BusinessTier"
$BUSINESS_TIER_TARGET = "$targetDir\BusinessTier"
$MESSAGE_BUS = "$sourceDir\MessageBusGateway"
$MESSAGE_BUS_TARGET = "$targetDir\MessageBusGateway"
$MASTERDATA_LOADER = "$sourceDir\MasterDataLoader"
$MASTERDATA_LOADER_TARGET = "$targetDir\MasterDataLoader"
$LBO_GENERATOR = "$sourceDir\LBOGenerator"
$LBO_GENERATOR_TARGET = "$targetDir\LBOGenerator"
$PRINTABLEDOCUMENTS_RENDERER = "$sourceDir\PrintableDocumentsRenderer"
$PRINTABLEDOCUMENTS_RENDERER_TARGET = "$targetDir\PrintableDocumentsRenderer"
$SCHEDULING = "$sourceDir\Scheduling"
$SCHEDULING_TARGET = "$targetDir\Scheduling"

# Check if source exists
$sourceDir = "\\${hostname}\$($SOURCE -replace ':', '$')"
#$sourceDir = "\\vm-it.cmf.criticalmanufacturing.com\C$\Program Files\CriticalManufacturing\PIDevOps"
Write-Verbose $($sourceDir)
if(Test-Path -Path $($sourceDir))
{
#     Write-Verbose "Deal with host config file"
# 	$sess = New-PSSession -ComputerName $hostname
# 	Enter-PSSession -Session $sess
#     Write-Verbose "Entered PS Session"
# # 	Invoke-Command -Session $sess -Scriptblock {$protectUnprotectConfigFilePath = "${SOURCE}\ProtectUnprotectConfigFile"}
# # 	Invoke-Command -Session $sess -Scriptblock {$arguments = "/Mode:2 /InstallPath:""${SOURCE}\BusinessTier"""}
# # 	Invoke-Command -Session $sess -Scriptblock {Set-Location -Path $protectUnprotectConfigFilePath}
# # 	Invoke-Command -Session $sess -Scriptblock {Start-Process Cmf.Tools.ProtectUnprotectConfigFile.exe $arguments -Wait}
# 	$protectUnprotectConfigFilePath = "${SOURCE}\ProtectUnprotectConfigFile"
# 	$arguments = "/Mode:2 /InstallPath:""${SOURCE}\BusinessTier"""
# 	Set-Location -Path $protectUnprotectConfigFilePath
# 	Start-Process Cmf.Tools.ProtectUnprotectConfigFile.exe $arguments -Wait
# 	Exit-PSSession
#     Remove-PSSession -Session $sess
    
	New-Item $BUSINESS_TIER_TARGET -ItemType Directory -Force
    Remove-Item $BUSINESS_TIER_TARGET\* -Force -Recurse -Verbose
    Write-Verbose "$(Get-Date): Copying $BUSINESS_TIER to $BUSINESS_TIER_TARGET..."
    ROBOCOPY $BUSINESS_TIER $BUSINESS_TIER_TARGET /S /IS /purge /xf Cmf.Custom.*.* *.bin /XD "Log" "Manuals" "Release Notes"

    New-Item $MESSAGE_BUS_TARGET -ItemType Directory -Force
    Remove-Item $MESSAGE_BUS_TARGET\* -Force -Recurse -Verbose
    Write-Verbose "$(Get-Date): Copying $MESSAGE_BUS to $MESSAGE_BUS_TARGET..."
    ROBOCOPY $MESSAGE_BUS $MESSAGE_BUS_TARGET /S /IS /XD "log"

    New-Item $MASTERDATA_LOADER_TARGET -ItemType Directory -Force
    Remove-Item $MASTERDATA_LOADER_TARGET\* -Force -Recurse -Verbose
    Write-Verbose "$(Get-Date): Copying $MASTERDATA_LOADER to $MASTERDATA_LOADER_TARGET..."
    ROBOCOPY $MASTERDATA_LOADER $MASTERDATA_LOADER_TARGET /S /IS

    New-Item $LBO_GENERATOR_TARGET -ItemType Directory -Force
    Remove-Item $LBO_GENERATOR_TARGET\* -Force -Recurse -Verbose
    Write-Verbose "$(Get-Date): Copying $LBO_GENERATOR to $LBO_GENERATOR_TARGET..."
    ROBOCOPY $LBO_GENERATOR $LBO_GENERATOR_TARGET /S /IS /XD "out"

    New-Item $PRINTABLEDOCUMENTS_RENDERER_TARGET -ItemType Directory -Force
    Remove-Item $PRINTABLEDOCUMENTS_RENDERER_TARGET\* -Force -Recurse -Verbose
    Write-Verbose "$(Get-Date): Copying $PRINTABLEDOCUMENTS_RENDERER to $PRINTABLEDOCUMENTS_RENDERER_TARGET..."
    ROBOCOPY $PRINTABLEDOCUMENTS_RENDERER $PRINTABLEDOCUMENTS_RENDERER_TARGET /S /IS

    New-Item $SCHEDULING_TARGET -ItemType Directory -Force
    Remove-Item $SCHEDULING_TARGET\* -Force -Recurse -Verbose
    Write-Verbose "$(Get-Date): Copying $SCHEDULING to $SCHEDULING_TARGET..."
    ROBOCOPY $SCHEDULING $SCHEDULING_TARGET /S /IS

    $messagebusConfigPath = "$MESSAGE_BUS_TARGET\config\config.json"
    $json = Get-Content $messagebusConfigPath | ConvertFrom-Json
    $json.PSObject.Properties.Remove("cluster");
    $json.PSObject.Properties.Remove("loadBalancer");
    $mbPort = $json.port
    $json | ConvertTo-Json -Depth 10 | set-content $messagebusConfigPath

    # Disable loadbalancer
    Write-Verbose "$(Get-Date): Disabling Load Balancer"
    $loadBalancerSettingsPath = "$BUSINESS_TIER_TARGET\Settings\LoadBalancingConfig.json"
    $json = Get-Content $loadBalancerSettingsPath | ConvertFrom-Json
    $json.Instances[0].IsEnabled = $false
    $json.Instances[0].HostAddress = "localhost"
    $json.IsLoadBalancerEnabled = $false
    $json.LoadBalancerConfig.Address = "localhost"
    $json | ConvertTo-Json -Depth 10 | set-content $loadBalancerSettingsPath

    $transportConfigPath = "$BUSINESS_TIER_TARGET\Settings\TransportConfig.json"
    $json = Get-Content $transportConfigPath | ConvertFrom-Json
    $json.TransportConfig.UseLoadBalancing = $false
    $json.TransportConfig.GatewaysConfig[0].Address = "localhost"
    $json.TransportConfig.GatewaysConfig[0].ExternalAddress = "ws://localhost:"+ $mbPort
    $json.TransportConfig.LoadBalancerConfig.Address = "localhost"
    $json | ConvertTo-Json -Depth 10 | set-content $transportConfigPath

    $appConfigPath = "$BUSINESS_TIER_TARGET\Cmf.Foundation.Services.HostService.dll.config"
    $log4netConfigPath = "$BUSINESS_TIER_TARGET\log4net.config"
    Write-Verbose "$(Get-Date): Updating Settings folder ($appConfigPath)" 

    $configLog4Net = (Get-Content $log4netConfigPath) -as [Xml]
    $objLog4Net = $configLog4Net.log4net.appender.file
    $objLog4Net[0].value = ".\Log\LocalEnvironment.log"
    $objLog4Net[1].value = ".\Log\LocalEnvironment.log"

    $config = (Get-Content $appConfigPath) -as [Xml]
    $obj = $config.configuration.log4net.appender.file
    $obj = $config.configuration.SettingsManagement.add | where {$_.Key -eq 'TransportConfigDirectory'}
    $obj.value = ".\Settings\TransportConfig.json"
    $obj = $config.configuration.SettingsManagement.add | where {$_.Key -eq 'LoadBalancingDirectory'}
    $obj.value = ".\Settings\LoadBalancingConfig.json"
    $config.Save($appConfigPath)
    $configLog4Net.Save($configLog4Net)

    (Get-Content $appConfigPath) -replace([RegEx]::Escape('VM-PI-DB-DEV\ONLINE'), '127.0.0.1')  | Set-Content $appConfigPath

    $mdLoaderConfigPath = "$MASTERDATA_LOADER_TARGET\MasterData.exe.config"
    Write-Verbose "$(Get-Date): Updating Settings folder ($mdLoaderConfigPath)"
    $config = (Get-Content $mdLoaderConfigPath) -as [Xml]
    $obj = $config.configuration.appSettings.add | where {$_.Key -eq 'HostAddress'}
    $obj.value = "localhost"
    $config.Save($mdLoaderConfigPath)
}
else
{
	Write-Verbose " - Path $sourceDir does not exist" -BackgroundColor RED -ForegroundColor BLACK
}

Write-Verbose "$(Get-Date): Completed..."
if($PAUSE -eq 1)
{
	PAUSE
}

EXIT 0

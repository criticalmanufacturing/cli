########################################################################################
# this script uses the information available as deployment variables
# to install a customizationpackage
########################################################################################
param($installPath, $toolsPath, $step, $hostContext, $cmfExecutionContext, $workingDirectory)

$TargetDirectory = $cmfExecutionContext.Variables.GetValue("Package[$(packageId)].TargetDirectory")
$SystemName = $cmfExecutionContext.Variables.GetValue("Product.SystemName")
$AdminPass = $cmfExecutionContext.Variables.GetValue("Product.ApplicationServer.ServiceUser.Password")
$SQLOnlinePassword = $cmfExecutionContext.Variables.GetValue("Package[Product.Database.Online].Database.Password")
$SQLODSPassword = $cmfExecutionContext.Variables.GetValue("Package[Product.Database.Ods].Database.Password")
$SQLDWHPassword = $cmfExecutionContext.Variables.GetValue("Package[Product.Database.Dwh].Database.Password")
$NLBAddress = $cmfExecutionContext.Variables.GetValue("Product.ApplicationServer.Address")
$ServicePort = $cmfExecutionContext.Variables.GetValue("Product.ApplicationServer.Port")
$ClientTenantName = $cmfExecutionContext.Variables.GetValue("Product.Tenant.Name")
$BackupLocation = $cmfExecutionContext.Variables.GetValue("Product.Database.BackupShare").Replace("\", "\\")
$InstallationPath = $cmfExecutionContext.Variables.GetValue("Packages.Root.TargetDirectory").Replace("\", "\\")
$AdminUser = $cmfExecutionContext.Variables.GetValue("Product.ApplicationServer.ServiceUser.UserName").Replace("\", "\\")
$AlwaysOn = $cmfExecutionContext.Variables.GetValue("Product.Database.IsAlwaysOn")
$OnlineClusterInstance = $cmfExecutionContext.Variables.GetValue("Package[Product.Database.Online].Database.Server").Replace("\", "\\")
$ODSClusterInstance = $cmfExecutionContext.Variables.GetValue("Package[Product.Database.Ods].Database.Server").Replace("\", "\\")
$DWHClusterInstance = $cmfExecutionContext.Variables.GetValue("Package[Product.Database.Dwh].Database.Server").Replace("\", "\\")
$SQLOnlineUser = $cmfExecutionContext.Variables.GetValue("Package[Product.Database.Online].Database.User")
$SQLODSUser = $cmfExecutionContext.Variables.GetValue("Package[Product.Database.Ods].Database.User")
$SQLDWHUser = $cmfExecutionContext.Variables.GetValue("Package[Product.Database.Dwh].Database.User")

cd "$TargetDirectory"

$dfConfig = @"
{
"Product.SystemName": "$SystemName",
"Product.ApplicationServer.ServiceUser.Password": "$AdminPass",
"Package[Product.Database.Online].Database.Password": "$SQLOnlinePassword",
"Package[Product.Database.Ods].Database.Password": "$SQLODSPassword",
"Package[Product.Database.Dwh].Database.Password": "$SQLDWHPassword",
"Product.ApplicationServer.Address": "$NLBAddress",
"Product.Presentation.IisConfiguration.Binding.IsSslEnabled": false,
"Product.ApplicationServer.Port": "$ServicePort",
"Product.Tenant.Name": "$ClientTenantName",
"Product.Database.BackupShare": "$BackupLocation",
"Packages.Root.TargetDirectory": "$InstallationPath",
"Product.ApplicationServer.ServiceUser.UserName": "$AdminUser",
"Product.DocumentManagement.TemporaryFolder": "",
"Product.Database.IsAlwaysOn": "$AlwaysOn",
"Package[Product.Database.Online].Database.Server": "$OnlineClusterInstance",
"Package[Product.Database.Ods].Database.Server": "$ODSClusterInstance",
"Package[Product.Database.Dwh].Database.Server": "$DWHClusterInstance",
"Package[Product.Database.Online].Database.User": "$SQLOnlineUser",
"Package[Product.Database.Ods].Database.User": "$SQLODSUser",
"Package[Product.Database.Dwh].Database.User": "$SQLDWHUser"
}
"@ | Out-File "$TargetDirectory\EnvironmentConfigs\DF.json"


$EnvironmentConfigName = "DF"
$InteractiveMode = $false
$BaseScriptPath = $TargetDirectory
$RunAppInstall = $false
$BackupBefore = $false
$ScriptPath = "$BaseScriptPath\CMFInstallActions\CustomizationInstall.ps1";
$ScriptParameters = "-EnvironmentConfigName " + $EnvironmentConfigName +" -InteractiveMode:$" + $InteractiveMode + " -BackupBefore:$" + $BackupBefore + " -RunAppInstall:$" + $RunAppInstall;

if($PlantName)
{
	$ScriptParameters = $ScriptParameters + " -PlantName " + $PlantName
}

$command = ("& '$ScriptPath' $ScriptParameters")
Powershell $command -NoNewWindow | Out-Host

if ($LASTEXITCODE -ne 0) {
	  throw "Customization Installation Aborted!"
}

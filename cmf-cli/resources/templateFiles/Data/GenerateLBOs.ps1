param($installPath, $toolsPath, $step, $hostContext, $cmfExecutionContext, $workingDirectory)

Write-Host '    Starting LBO Generation ...'

$installationPath = $cmfExecutionContext.Variables.GetValue("Packages.Root.TargetDirectory").Replace("\", "\\")

$businessInstallationPath = "$installationPath\BusinessTier"
$protectUnprotectConfigFilePath = "$installationPath\ProtectUnprotectConfigFile"
$lboGeneratorPath = "$installationPath\LBOGenerator"

$arguments = "/Mode:2 /InstallPath:""$businessInstallationPath"""

#Unprotect HostConfigFile
Write-Host "Deal with host config file"
Set-Location -Path $protectUnprotectConfigFilePath
Start-Process Cmf.Tools.ProtectUnprotectConfigFile.exe $arguments -Wait

#REST LBOs
Set-Location -Path $lboGeneratorPath
.\LBOUpdater.ps1

$arguments = "/Mode:1 /InstallPath:""$businessInstallationPath"""

#Unprotect HostConfigFile
Write-Host "Deal with host config file"
Set-Location -Path $protectUnprotectConfigFilePath
Start-Process Cmf.Tools.ProtectUnprotectConfigFile.exe $arguments -Wait

$filePath = (Get-Item $myinvocation.mycommand.path).DirectoryName.ToString() + "\GenerateLBOs.ps1"

Remove-Item $filePath -Force -Recurse
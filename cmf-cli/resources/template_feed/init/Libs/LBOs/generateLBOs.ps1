$invocation = (Get-Variable MyInvocation).Value
$pwd = Split-Path $invocation.MyCommand.Path
$lbosPath = "$PSScriptRoot\..\LBOs"

# Create a new session remotely to wait for process
$sess = New-PSSession -ComputerName "<%= $PLASTER_PARAM_vmHostname %>"
Enter-PSSession -Session $sess
Write-Host "Deal with host config file"

Invoke-Command -Session $sess -Scriptblock {$protectUnprotectConfigFilePath = '<%= $PLASTER_PARAM_InstallationPath %>\ProtectUnprotectConfigFile'}
Invoke-Command -Session $sess -Scriptblock {$lboGeneratorPath = '<%= $PLASTER_PARAM_InstallationPath %>\LBOGenerator'}
Invoke-Command -Session $sess -Scriptblock {$pathBiz = '<%= $PLASTER_PARAM_InstallationPath %>\BusinessTier'}
Invoke-Command -Session $sess -Scriptblock {$arguments = "/Mode:2 /InstallPath:""$pathBiz"""}
Invoke-Command -Session $sess -Scriptblock {Set-Location -Path $protectUnprotectConfigFilePath}
Invoke-Command -Session $sess -Scriptblock {Start-Process Cmf.Tools.ProtectUnprotectConfigFile.exe $arguments -Wait}
Invoke-Command -Session $sess -Scriptblock {Set-Location -Path $lboGeneratorPath}
Invoke-Command -Session $sess -ScriptBlock {.\LBOUpdater.ps1}

Remove-PSSession -Session $sess
Exit-PSSession

if((Test-Path "$lbosPath\NetStandard" -PathType Container)) { 
    Remove-Item "$lbosPath\NetStandard" -Recurse -Force
}

if((Test-Path "$lbosPath\TypeScript" -PathType Container)) { 
    Remove-Item "$lbosPath\TypeScript" -Recurse -Force
}

$SOURCE = "<%= $PLASTER_PARAM_InstallationPath %>"
$installation = '\\<%= $PLASTER_PARAM_vmHostname %>\' + $SOURCE -replace ':', '$'
Remove-Item -Path $pwd\* -Recurse -Force -Exclude "generateLBOs.ps1"
Copy-Item "$installation\LBOGenerator\out\NetStandard" -Destination "$lbosPath" -Recurse -Force -Filter '*.dll'
Copy-Item "$installation\LBOGenerator\out\TypeScript" -Destination "$lbosPath" -Recurse -Force
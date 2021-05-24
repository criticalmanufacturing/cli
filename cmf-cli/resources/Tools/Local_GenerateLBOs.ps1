#Clear-Host
Param(
  [string]$PSScriptRoot
)

# Set Variables to later copy LBO's
$businessPath = "$PSScriptRoot\..\LocalEnvironment\BusinessTier"
$lbosPath = "$PSScriptRoot\..\Libs\LBOs"
$masterDataPath = "$PSScriptRoot\..\LocalEnvironment\MasterDataLoader"

# $SOURCE = "<%= $PLASTER_PARAM_InstallationPath %>"
# $installation = '\\<%= $PLASTER_PARAM_vmHostname %>\' + $SOURCE -replace ':', '$'
$installation = "$PSScriptRoot\..\LocalEnvironment"
$lboGeneratorExe = "$installation\LBOGenerator\LboGenerator.exe"

# Generate LBOs

Write-Verbose "Generate LBOs"

& $lboGeneratorExe --hostdir="$businessPath" --nobanner

# Copy LBOs to folders

Write-Verbose "Copy to master data loader"

Copy-Item "$installation\LBOGenerator\out\NetStandard\Cmf.LightBusinessObjects.*" -Destination $masterDataPath -force

Write-Verbose "Copy to LBOs Dir"

if((Test-Path "$lbosPath\NetStandard" -PathType Container)) { 
    Remove-Item "$lbosPath\NetStandard" -Recurse -Force
}

if((Test-Path "$lbosPath\TypeScript" -PathType Container)) { 
    Remove-Item "$lbosPath\TypeScript" -Recurse -Force
}

Copy-Item "$installation\LBOGenerator\out\NetStandard" -Destination "$lbosPath" -Recurse -Force -Filter '*.dll'
Copy-Item "$installation\LBOGenerator\out\TypeScript" -Destination "$lbosPath" -Recurse -Force
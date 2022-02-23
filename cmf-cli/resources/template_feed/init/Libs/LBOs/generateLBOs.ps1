Clear-Host

# Set Variables to later copy LBO's
$businessPath = "$PSScriptRoot\..\..\LocalEnvironment\BusinessTier"
$masterDataPath = "$PSScriptRoot\..\..\LocalEnvironment\MasterDataLoader"
$lbosPath = "$PSScriptRoot"



# Generate LBOs
Write-Host "Generate .Net LBOs"
$installation = "$PSScriptRoot\..\..\LocalEnvironment"
$lboGeneratorExe = "$installation\LBOGenerator\LboGenerator.exe"
& $lboGeneratorExe --hostdir="$businessPath" --nobanner

# Copy LBOs to folders
Write-Host "Copy to master data loader"
Copy-Item "$installation\LBOGenerator\out\NetStandard\Cmf.LightBusinessObjects.*" -Destination $masterDataPath -force
Write-Host "Copy to LBOs Dir"

if((Test-Path "$lbosPath\NetStandard" -PathType Container)) { 
    Remove-Item "$lbosPath\NetStandard" -Recurse -Force
}

if((Test-Path "$lbosPath\TypeScript" -PathType Container)) { 
    Remove-Item "$lbosPath\TypeScript" -Recurse -Force
}

Copy-Item "$installation\LBOGenerator\out\NetStandard" -Destination "$lbosPath" -Recurse -Force -Filter '*.dll'
Copy-Item "$installation\LBOGenerator\out\TypeScript" -Destination "$lbosPath" -Recurse -Force

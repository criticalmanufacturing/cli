########################################################################################
# this script uses the information available as deployment variables
# to publish connect IoMT packages to a directory (repository)
########################################################################################
param($installPath, $toolsPath, $step, $hostContext, $cmfExecutionContext, $workingDirectory)

$IsPublishToDirectoryEnabled = $cmfExecutionContext.Variables.GetValue("Local.Repository.Directory.Enabled")
if ($IsPublishToDirectoryEnabled -eq $true) {
	$SourceDirectory = $installPath
	$TargetDirectory = $cmfExecutionContext.Variables.GetValue("Local.Repository.Directory.Location")
	$repositoryBuilderScriptFileName = ".rebuildDatabase.ps1"
	
	# Test source directory
	If (-Not (Test-Path $SourceDirectory -pathType container)) { 
		Write-Error "Could not find packages source directory $($SourceDirectory)." -ErrorAction Stop
	}
	$packageCount =  (Get-ChildItem "$SourceDirectory/*" -Filter "*.tgz").Count;
	If ($packageCount -eq 0) { 
		Write-Error "Could not find packages to copy in the source directory $($SourceDirectory)." -ErrorAction Stop
	}

	# Test target directory
	If (-Not (Test-Path $TargetDirectory -pathType container)) { 
		New-Item -ItemType Directory -Force -Path $TargetDirectory
		Write-Verbose  -Verbose "Created target directory $($TargetDirectory)."
	}

	# Copy packages
	Copy-item -Force -Recurse -Verbose "$SourceDirectory\*.tgz" -Destination $TargetDirectory
	Write-Verbose -Verbose "Successfully copied $($packageCount) packages."
	
	# Rebuild database on TargetDirectory so that both newer and existing packages(if any) are merged together
	Write-Verbose -Verbose "Rebuilding directory content definition in target directory $($TargetDirectory)"
		& "$($TargetDirectory)\$($repositoryBuilderScriptFileName)"	
	
} else {
	Write-Verbose -Verbose "Publish to directory repository is not enabled"
}

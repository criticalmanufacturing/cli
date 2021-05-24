########################################################################################
# this script uses the information available as deployment variables
# to validate if at least one of the options is selected
########################################################################################
param($installPath, $toolsPath, $step, $hostContext, $cmfExecutionContext, $workingDirectory)

$IsPublishToDirectoryEnabled = $cmfExecutionContext.Variables.GetValue("Local.Repository.Directory.Enabled")
$IsPublishToRepositoryEnabled = $cmfExecutionContext.Variables.GetValue("Local.Repository.Server.Enabled")
if ($IsPublishToDirectoryEnabled -eq $false -and $IsPublishToRepositoryEnabled -eq $false) {
	
	throw "Publish to directory repository and Publish to directory repository are not enabled"
} 
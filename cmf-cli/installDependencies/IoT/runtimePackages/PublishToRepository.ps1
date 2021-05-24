########################################################################################
# this script uses the information available as deployment variables
# to publish connect IoT packages to local npm repository
########################################################################################
param($installPath, $toolsPath, $step, $hostContext, $cmfExecutionContext, $workingDirectory)

$IsPublishToRepositoryEnabled = $cmfExecutionContext.Variables.GetValue("Local.Repository.Server.Enabled")
if ($IsPublishToRepositoryEnabled -eq $true) {
	$TargetDirectory = $installPath
	$LocalRegistryAddress = $cmfExecutionContext.Variables.GetValue("Local.Repository.Server.Address")
	$Tag = $cmfExecutionContext.Variables.GetValue("Publish.Tag")
	$UserName = $cmfExecutionContext.Variables.GetValue("Local.Repository.Server.UserName")
	$UserPassword = $cmfExecutionContext.Variables.GetValue("Local.Repository.Server.UserPassword")
	$UserEmail = $cmfExecutionContext.Variables.GetValue("Local.Repository.Server.UserEmail")

	$NpmServerAddress = $LocalRegistryAddress
	$parts = $NpmServerAddress.Split(':')
	$machineName = $parts[1].Replace('//','')
	$port = $parts[2].Replace('/','')

	If (-Not (Test-Path $TargetDirectory -pathType container)) { 
		Write-Error "Could not find packages source directory $($TargetDirectory)." -ErrorAction Stop
	}

	if ((Test-NetConnection -ComputerName $machineName -Port $port).TcpTestSucceeded -eq $false) {
		Write-Error "Could not find $($NpmServerAddress)." -ErrorAction Stop
	}

	Write-Verbose -Verbose "Deploying $Tag to $NpmServerAddress..."

	$psi = New-Object System.Diagnostics.ProcessStartInfo;
	$psi.WindowStyle = [System.Diagnostics.ProcessWindowStyle]::Hidden;
	$psi.FileName = "cmd.exe"; #process file
	$psi.UseShellExecute = $false; #start the process from it's own executable file
	$psi.RedirectStandardInput = $true; #enable the process to read from standard input

	$p = [System.Diagnostics.Process]::Start($psi);

	Start-Sleep -s 2 #wait 2 seconds so that the process can be up and running

	$p.StandardInput.WriteLine("npm adduser --registry $NpmServerAddress"); #StandardInput property of the Process is a .NET StreamWriter object
	Start-Sleep -s 2 #wait for the stdin to be ready to receive input
	$p.StandardInput.WriteLine($UserName.ToLowerInvariant()); #StandardInput property of the Process is a .NET StreamWriter object
	Start-Sleep -s 2 #wait for the stdin to be ready to receive input
	$p.StandardInput.WriteLine($UserPassword); #StandardInput property of the Process is a .NET StreamWriter object
	Start-Sleep -s 2 #wait for the stdin to be ready to receive input
	$p.StandardInput.WriteLine($UserEmail); #StandardInput property of the Process is a .NET StreamWriter object
	Start-Sleep -s 2 #wait for adduser command to finish

	Stop-Process -Id $p.Id -Force
	$successfullyPublishedPackagesCounter = 0
	$unsuccessfullyPublishedPackagesCounter = 0
	& cd $TargetDirectory
	foreach ($package in Get-ChildItem "$TargetDirectory/*" -Filter "*.tgz") {
		$result = & npm publish $package --registry $NpmServerAddress --tag $Tag --force
		if($LASTEXITCODE -ge 0){
			$successfullyPublishedPackagesCounter++
		}else{
			$unsuccessfullyPublishedPackagesCounter++
			Write-Error "Could not publish package $($package)"
		}
		if(-not([string]::IsNullOrWhiteSpace($result))) {
			Write-Verbose -Verbose $result
		}
	}

	Write-Verbose -Verbose "Successfully published $($successfullyPublishedPackagesCounter) packages."
	Write-Verbose -Verbose "$($unsuccessfullyPublishedPackagesCounter) packages were not published successfully."
} else {
	Write-Verbose -Verbose "Publish to npm server repository is not enabled"
}

param(
	[Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [string]$ConfigFilePath,
	[string]$ReferencesDir = "..\CMFInstallActions\References\"
)
if(Test-Path $ConfigFilePath)
{
	$configJson = Get-Content $ConfigFilePath | ConvertFrom-Json

	$env = New-CMFEnvironment $configJson.'Product.SystemName'

	# Decrypt-Encrypt All Passwords
	# Load DLL that contains the methods to decrypt the passwords of the parameter file
	$ScriptPath = Split-Path $MyInvocation.MyCommand.Path
	[void][System.Reflection.Assembly]::LoadFile("$ReferencesDir\Cmf.Encrypt.Utils.dll")
	$encrypter = New-Object Cmf.Encrypt.Utils.EncryptionService
	
	$AdminPass = "";
	if ([string]::IsNullOrEmpty($AdminPass)) {
		$AdminPass = $configJson.'Product.ApplicationServer.ServiceUser.Password'
		# Some parameter files have the passwords already decrypted
		$AdminPass = $encrypter.DecryptIfNeeded($AdminPass)
	}

	$SQLOnlinePassword = "";
	if ([string]::IsNullOrEmpty($SQLOnlinePassword)) {
		$SQLOnlinePassword = $configJson.'Package[Product.Database.Online].Database.Password'
		# Some parameter files have the passwords already decrypted
		$SQLOnlinePassword = $encrypter.DecryptIfNeeded($SQLOnlinePassword)
	}

	$SQLODSPassword = "";
	if ([string]::IsNullOrEmpty($SQLODSPassword)) {
		$SQLODSPassword = $configJson.'Package[Product.Database.Ods].Database.Password'
		# Some parameter files have the passwords already decrypted
		$SQLODSPassword = $encrypter.DecryptIfNeeded($SQLODSPassword)
	}

	$SQLDWHPassword = "";
	if ([string]::IsNullOrEmpty($SQLDWHPassword)) {
		$SQLDWHPassword = $configJson.'Package[Product.Database.Dwh].Database.Password'
		# Some parameter files have the passwords already decrypted
		$SQLDWHPassword = $encrypter.DecryptIfNeeded($SQLDWHPassword)
	}

	$env.NLBAddress = $configJson.'Product.ApplicationServer.Address'
   	$env.UseSSL = $false # $configJson.'Product.Presentation.IisConfiguration.Binding.IsSslEnabled'
	
   	$env.ReadPasswordToSignGeneratedLBOs = $false # How to get this?
	$env.GenerateErpCustomManagement = $false # How to get this?
					
	$env.ServicePort = $configJson.'Product.ApplicationServer.Port'
	$env.ClientTenantName = $configJson.'Product.Tenant.Name'

	# Backups will use the following structure
	# BackupLocation \ BackupIdentifier \ Database-BackupIdentifier.bak
	# BackupLocation \ BackupIdentifier \ ServerName-BackupFolder-BackupIdentifier.zip
	$env.BackupLocation = $configJson.'Product.Database.BackupShare'

	#### #### #### #### #### ####
	#### Aplication Servers  ####
	#### #### #### #### #### ####

	# New-CMFServer 'ServerName' 'InstallationPath'
	# To Enable Autentication on the servers the following commands should be run on the servers
	#	This allows the server to delegate credentials to all domain PCs,
	#		Enable-WSManCredSSP -Role "Client" -DelegateComputer "*.CMF.CRITICALMANUFACTURING.COM"
	#	This allows the server to receive the credentials 
	#		Enable-WSManCredSSP -Role "Server"
	$InstallationPath = $configJson.'Packages.Root.TargetDirectory'
	$env.ApplicationServers += New-CMFServer $env.NLBAddress $InstallationPath

	$env.AdminUser = $configJson.'Product.ApplicationServer.ServiceUser.UserName'
	# To cypher the password, please run the following commands in powershell, Remove the break lines of the result
	# Import-Module <PackageLocation>\CMFInstallActions\CMFInstallActions.psm1
	# Get-EncryptedStringFromClearText "ClearTextPassword"
	$env.AdminPass = Get-EncryptedStringFromClearText $AdminPass

	$env.TemporaryFileShare = $configJson.'Product.DocumentManagement.TemporaryFolder'

	#### #### #### #### #### ####
	#### Database Servers    ####
	#### #### #### #### #### ####

	$alwaysOn = if($configJson.'Product.Database.IsAlwaysOn' -eq 'True') { $true } else { $false };
	$env.OnlineClusterInstance = $configJson.'Package[Product.Database.Online].Database.Server'
	$env.OnlineClusterAlwaysOn = $alwaysOn
	$env.ODSClusterInstance = $configJson.'Package[Product.Database.Ods].Database.Server'
	$env.ODSClusterAlwaysOn = $alwaysOn
	$env.DWHClusterInstance = $configJson.'Package[Product.Database.Dwh].Database.Server'
	$env.DWHClusterAlwaysOn = $alwaysOn

	# $true if windows authentication ; $false for not use
	$env.SQLOnlineWindowsAuthentication = $false 
	# The user to access to online DB. 
	$env.SQLOnlineUser = $configJson.'Package[Product.Database.Online].Database.User'
	# Read-SQLPassword $env.OnlineClusterInstance $env.SQLOnlineUser
	$env.SQLOnlinePassword = Get-EncryptedStringFromClearText $SQLOnlinePassword 
	# $true if windows authentication ; $false for not use
	$env.SQLODSWindowsAuthentication = $false 
	$env.SQLODSUser = $configJson.'Package[Product.Database.Ods].Database.User'
	$env.SQLODSPassword = Get-EncryptedStringFromClearText $SQLODSPassword
	# $true if windows authentication ; $false for not use
	$env.SQLDWHWindowsAuthentication = $false 
	$env.SQLDWHUser = $configJson.'Package[Product.Database.Dwh].Database.User'
	$env.SQLDWHPassword = Get-EncryptedStringFromClearText $SQLDWHPassword
	$env.SQLBackupCompression =  $true

	if($alwaysOn)
	{
		# Get Replicas

		[System.Reflection.Assembly]::LoadFrom((Resolve-Path $ReferencesDir\Microsoft.SqlServer.BatchParser.dll)) | Out-Null
		[System.Reflection.Assembly]::LoadFrom((Resolve-Path $ReferencesDir\Microsoft.SqlServer.SqlClrProvider.dll)) | Out-Null
		[System.Reflection.Assembly]::LoadFrom((Resolve-Path $ReferencesDir\Microsoft.SqlServer.Smo.dll)) | Out-Null
		[System.Reflection.Assembly]::LoadFrom((Resolve-Path $ReferencesDir\Microsoft.SqlServer.ConnectionInfo.dll)) | Out-Null
		[System.Reflection.Assembly]::LoadFrom((Resolve-Path $ReferencesDir\Microsoft.SqlServer.Management.Sdk.Sfc.dll)) | Out-Null
		
		$commandText += "SELECT DISTINCT r.replica_server_name as Name"
		$commandText += " FROM sys.availability_replicas r"
		$commandText += " INNER JOIN sys.availability_groups ags"
		$commandText += " ON ags.group_id = r.group_id"
		
		#Online
			
		$srv = New-Object -TypeName Microsoft.SqlServer.Management.Smo.Server -ArgumentList $env.OnlineClusterInstance
		$srv.ConnectionContext.DatabaseName = $env.SystemName
		$srv.ConnectionContext.LoginSecure = $False
		$srv.ConnectionContext.Login = $env.SQLOnlineUser
		$srv.ConnectionContext.Password = $SQLOnlinePassword

		$onlineResult = $srv.ConnectionContext.ExecuteWithResults($commandText)

		foreach ($row in $onlineResult.Tables[0].Rows) 
		{
			$env.OnlineReplicas += "$($row.Name)";
		}
		
		$srv.ConnectionContext.Disconnect()
		
		#ODS
			
		$srv = New-Object -TypeName Microsoft.SqlServer.Management.Smo.Server -ArgumentList $env.OdsClusterInstance
		$srv.ConnectionContext.DatabaseName = $env.SystemName + 'ODS'
		$srv.ConnectionContext.LoginSecure = $False
		$srv.ConnectionContext.Login = $env.SQLODSUser
		$srv.ConnectionContext.Password = $SQLODSPassword

		$odsResult = $srv.ConnectionContext.ExecuteWithResults($commandText)

		foreach ($row in $odsResult.Tables[0].Rows) 
		{
			$env.ODSReplicas += "$($row.Name)";
		}
		
		$srv.ConnectionContext.Disconnect()
			
		#DWH
			
		$srv = New-Object -TypeName Microsoft.SqlServer.Management.Smo.Server -ArgumentList $env.DwhClusterInstance
		$srv.ConnectionContext.DatabaseName = $env.SystemName + 'DWH'
		$srv.ConnectionContext.LoginSecure = $False
		$srv.ConnectionContext.Login = $env.SQLDWHUser
		$srv.ConnectionContext.Password = $SQLDWHPassword

		$dwhResult = $srv.ConnectionContext.ExecuteWithResults($commandText)

		foreach ($row in $dwhResult.Tables[0].Rows) 
		{
			$env.DWHReplicas += "$($row.Name)";
		}
		
		$srv.ConnectionContext.Disconnect()
	}
	

	# Define if different backup locations for the different databases are required.
	$env.OnlineBackupLocation = $env.BackupLocation
	$env.ODSBackupLocation = $env.BackupLocation
	$env.DWHBackupLocation = $env.BackupLocation

	# ReportServer Configuration
	$env.ReportServerUri = $configJson.'Package.ReportingServices.Address' #The Uri to the ReportServer
	$env.ReportUseDefaultCredential = $true #$true if windows authentication ; $false for not use
	# $env.ReportServerUser = '«CMFUser»' # The user to access to reporting services. 
	# $env.ReportServerPassword = '«CMFUser»' # The password to access to reporting services 

	return $env
}
else 
{
	throw "File $($ConfigFilePath) not found..."
}
#
# CMFDatabaseActions.psm1
#

$ModulePath = Split-Path $MyInvocation.MyCommand.Path

<#
    .SYNOPSIS
        Run an SQL command on any SQL Server database.

    .DESCRIPTION
        Executes an SQL command on an SQL Server database specified using its instance name and database name.

    .PARAMETER dataSource
        Data source name (i.e. instance name) to be used when connecting to the database.

    .PARAMETER database
        Default database name on which command will be ran.

    .PARAMETER sqlCommand
        String containing the SQL Command to be ran on the database.
    
    .PARAMETER username
        Username to use to connect to the database if SQL Authentication is used. If not specified, windows authentication will
        be used.

    .PARAMETER password
        Password to use to connect to the database if SQL Authentication is used.

	.PARAMETER timeOut
        the wait time before terminating the attempt to execute a command and generating an error.

    .EXAMPLE 
        Invoke-SQL -dataSource 'CMF-VM-TEST\ONLINE' -database 'cmNavigoODS' -sqlCommand $command

    .EXAMPLE 
        Invoke-SQL -dataSource 'CMF-VM-TEST\ONLINE' -database 'cmNavigoODS' -sqlCommand $command -username 'TestUser' -password 'TestPassword'
        
    .NOTES
        Query results are not returned.
        GO batch separators are supported, provided they are in separate lines on the sqlCommand string.
#>
function Invoke-SQL 
{
    param
    (
        [Parameter(Mandatory=$True)]
        [string] $dataSource,

        [Parameter(Mandatory=$True)]
        [string] $database,

        [Parameter(Mandatory=$True)]
        [string] $sqlCommand,

        [Parameter(Mandatory=$False)]
        [string] $username,

        [Parameter(Mandatory=$False)]
        [string] $password,

		[Parameter(Mandatory=$False)]
        $timeOut = $null,

		[Parameter(Mandatory=$False)]
		[switch]$returnScalar
    )

    [System.Reflection.Assembly]::LoadFrom((Resolve-Path $ModulePath\References\Microsoft.SqlServer.BatchParser.dll)) | Out-Null
    [System.Reflection.Assembly]::LoadFrom((Resolve-Path $ModulePath\References\Microsoft.SqlServer.SqlClrProvider.dll))  | Out-Null
    [System.Reflection.Assembly]::LoadFrom((Resolve-Path $ModulePath\References\Microsoft.SqlServer.Smo.dll)) | Out-Null
    [System.Reflection.Assembly]::LoadFrom((Resolve-Path $ModulePath\References\Microsoft.SqlServer.ConnectionInfo.dll)) | Out-Null
    [System.Reflection.Assembly]::LoadFrom((Resolve-Path $ModulePath\References\Microsoft.SqlServer.Management.Sdk.Sfc.dll)) | Out-Null
		
	$srv = New-Object -TypeName Microsoft.SqlServer.Management.Smo.Server -ArgumentList $dataSource
	$srv.ConnectionContext.DatabaseName = $database
	if ($username)
	{
		$srv.ConnectionContext.LoginSecure = $False
		$srv.ConnectionContext.Login = $username
		$srv.ConnectionContext.Password = $password
	}

    try 
    {
		if( $timeOut  )
		{
			LogWrite "    Timeout $timeOut secs"
			$srv.ConnectionContext.StatementTimeout = $timeOut
		}
		if ( $returnScalar )
		{
			$result = $srv.ConnectionContext.ExecuteScalar($sqlCommand)    
		}
        else 
		{
			$result = $srv.ConnectionContext.ExecuteNonQuery($sqlCommand)    
		}
    }
	catch [Exception]
    {
        throw $_.Exception.InnerException
    }
	finally 
	{
		$srv.ConnectionContext.Disconnect()
	}

    return $result
}

<#
    .SYNOPSIS
        Run an SQL Script file on a cmNavigo database.

    .DESCRIPTION
        Runs a set of SQL statements from a file on one of the environment databases.

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER database
        Database that the script should be ran on. Possible values include 'Online', 'ODS' and 'DWH'

    .PARAMETER scriptPath
        Absolute path to the script file to run
    
    .PARAMETER username
        Username to use to connect to the database if SQL Authentication is used. If not specified, windows authentication will
        be used.

    .PARAMETER password
        Password to use to connect to the database if SQL Authentication is used.

    .EXAMPLE 
        Invoke-SQLScript $env -database 'Online' -scriptPath '.\Analytics\Release 015.1.sql' -username 'CMFUser' -password 'CMFUser'
        
    .NOTES
        Assumes SQL Script file is saved with UTF8 encoding.
        GO batch separators are supported, assuming they are placed in separate lines.
#>
function Invoke-SQLScript 
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject] $env,

        # Database
        [Parameter(Mandatory=$True)]
        [CMFDatabase] $database,

        # Script path
        [Parameter(Mandatory=$True)]
        [string] $scriptPath,

		# TimeoutScript path
        [Parameter(Mandatory=$False)]
        $timeOut = $null
    )

    $username = ""
	$password = ""

    switch ($database) 
    {
        "Online" 
        {
            $databaseName = $env.OnlineDBName
            $databaseServer = $env.OnlineClusterInstance
			
			if ($env.SQLOnlineWindowsAuthentication -eq $false)
			{
				$username = $env.SQLOnlineUser
				$password = Get-ClearTextFromEncryptedString $env.SQLOnlinePassword
			}
        }
        "ODS" 
        {
            $databaseName = $env.ODSDBName
            $databaseServer = $env.ODSClusterInstance
			
			if ($env.SQLODSWindowsAuthentication -eq $false)
			{
				$username = $env.SQLODSUser
				$password = Get-ClearTextFromEncryptedString $env.SQLODSPassword
			}
        }
        "DWH" 
        {
		    $databaseName = $env.DWHDBName
            $databaseServer = $env.DWHClusterInstance
			
			if ($env.SQLDWHWindowsAuthentication -eq $false)
			{
				$username = $env.SQLDWHUser
				$password = Get-ClearTextFromEncryptedString $env.SQLDWHPassword
			}
        }
    }

	[string] $commandText = (Get-Content -Path $scriptPath -Encoding UTF8 | Out-String)
	$commandText = $commandText.replace('$(DatabaseName)', $databaseName);
	$commandText = $commandText.replace('$(OnlineDatabaseName)', $env.OnlineDBName);
	$commandText = $commandText.replace('$(ODSDatabaseName)', $env.ODSDBName);
	$commandText = $commandText.replace('$(DWHDatabaseName)', $env.DWHDBName);

	$scriptName = Split-Path $scriptPath -leaf
    LogWrite " >  Running SQL script $scriptName on $database database..."
    $result = Invoke-SQL $databaseServer $databaseName $commandText $username $password $timeOut
    LogWrite " *  Executed SQL script $scriptName" -foregroundColor Green 
}

<#
    .SYNOPSIS
        Backup cmNavigo databases

    .DESCRIPTION
        Backup one or all cmNavigo databases to a predefined folder (according to environment configuration). The backup folder is 
        created if it doesn't exist, as long as it is a network shared folder. 

        Online database backups are stored on the path specified on the $env.OnlineBackupLocation property.
        ODS database backups are stored on the path specified on the $env.ODSBackupLocation property.
        DWH database backups are stored on the path specified on the $env.DWHBackupLocation property.
        If the specific backup folder for a given database is null or not defined, the standard backup location will be used,
        specified on the $env.BackupLocation property.

        Backups are identified by a backupIdentifier token which can later be used to restore the backup.
        
    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER backupIdentifier
        Token used to identify the backups that will be taken. Backup files will be named according to this token.
        If not specified, the file will be named using the current date/time in the format "yyyy.mm.dd-HHmmss".

    .PARAMETER database
        Database to be backed up. Possible values include 'Online', 'ODS' and 'DWH'.
        If not specified, all databases are backed up.

    .PARAMETER username
        Username to use to connect to the database if SQL Authentication is used. If not specified, windows authentication will
        be used.

    .PARAMETER password
        Password to use to connect to the database if SQL Authentication is used.

	.PARAMETER useCompression
		Flag to enable compression of backups

	.PARAMETER useParallelJob
		Flag to enable parallel jobs. Useful when  backing up several large databases
		
    .EXAMPLE 
        Backup-CMFDatabase $env

    .EXAMPLE 
        Backup-CMFDatabase $env -backupIdentifier 'Before4.2'
    
	.EXAMPLE 
        Backup-CMFDatabase $env -backupIdentifier 'Before4.2' -useParallelJob	
		    
    .EXAMPLE 
        Backup-CMFDatabase $env -backupIdentifier 'Before4.2' -database 'Online'

    .EXAMPLE 
        Backup-CMFDatabase $env -backupIdentifier 'Before4.2' -username 'CMFTest' -password 'CMFPassword'    

    .NOTES
        Local backup folders are local on the DB server and are not recommended. If the local folder does not exist, an error will 
        be thrown.
        Backups are taken by SQL Server BACKUP DATABASE statement with the NOFORMAT, INIT, SKIP, NOREWIND, NOUNLOAD, COPY_ONLY
        and STATS = 10 options.
        Backup file names follow the naming convention: <databaseName>-<backupIdentifier>.bak

        Requires permissions to write and create directories on the specified backup locations.
#>
function Backup-CMFDatabase
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # BackupID
        [Parameter(Mandatory=$False)]
        $backupIdentifier =$null,

        # Database
        [Parameter(Mandatory=$False)]
        [CMFDatabase] $database,

        # Username
		[Parameter(Mandatory=$False)]
        [string] $username,

		# Password
        [Parameter(Mandatory=$False)]
        [string] $password,
		
		# UseCompression
		[Parameter(Mandatory=$False)]
		$useCompression = $null,
		
		# Parallel backups
		[Parameter(Mandatory=$False)]
		[switch]$useParallelJob,

		[Parameter(Mandatory=$False)]
		[int] $timeout = 600

    )

    LogWrite " >  Starting database backup..."

    [CMFDatabase[]] $dbsToBackup = @()
    if($database -ne $null)
    {
        $dbsToBackup += $database
    }
    else
    {
        $dbsTobackup = @('Online', 'ODS', 'DWH')
    }    
	
	$bgJobs = @()

    foreach($dbToBackup in $dbsToBackup) 
    {
		$activityDescription = "Backing up " + $dbToBackup + " database"
        LogWrite ("    " + $activityDescription)

		$username = ""
		$password = ""

        switch ($dbToBackup) 
        {
            "Online" 
            {
                $databaseName = $env.OnlineDBName
                $databaseServer = $env.OnlineClusterInstance
                $backupLocation = $env.OnlineBackupLocation
				if ($env.SQLOnlineWindowsAuthentication -eq $false)
				{
					 
					$username = $env.SQLOnlineUser
					$password = Get-ClearTextFromEncryptedString $env.SQLOnlinePassword
				}
            }
            "ODS" 
            {
                $databaseName = $env.ODSDBName
                $databaseServer = $env.ODSClusterInstance
                $backupLocation = $env.ODSBackupLocation
				if ($env.SQLODSWindowsAuthentication -eq $false)
				{
					$username = $env.SQLODSUser
					$password = Get-ClearTextFromEncryptedString $env.SQLODSPassword
				}
            }
            "DWH" 
            {
                $databaseName = $env.DWHDBName
                $databaseServer = $env.DWHClusterInstance
                $backupLocation = $env.DWHBackupLocation

				if ($env.SQLDWHWindowsAuthentication -eq $false)
				{
					$username = $env.SQLDWHUser
					$password = Get-ClearTextFromEncryptedString $env.SQLDWHPassword
				}
            }
        }

		if( $useCompression -eq $null )
		{
			$useCompression = $env.SQLBackupCompression
		}
		
		$compressionOption = ""
		if($useCompression)
		{
			$compressionOption = ", COMPRESSION "
		}		

        # Prepare scripts
        if ($backupIdentifier -eq $null) 
        {
            $backupIdentifier = (Get-Date -Format "yyyy.MM.dd-HHmmss")
        } 

        # Get default backup location if specific is null
        if ($backupLocation -eq $null)
        {
            $backupLocation = $env.BackupLocation
        }
 
		# Check if folder is a network path
        if ( ($backupLocation.StartsWith('\\')))
        {
			# create backup folder if it doesn't exist 
			if (-not (Test-Path $backupLocation))
			{
				$result = New-Item -ItemType Directory -Force -Path $backupLocation | Out-Null
			}
			
			$backupFileName = $backupLocation+'\'+$backupIdentifier+'\'+$databaseName+"-"+$backupIdentifier+".bak"
			$continueBackup = OverwriteBackupFile $backupFileName
		}
		else
		{
			# check if  folder exists inside  SQL Server instance
			$sqlScript = "DECLARE @BackupDestination nvarchar(500) = N'$backupLocation'; 
						  DECLARE @DirectoryExists int; EXEC master.dbo.xp_fileexist @BackupDestination, @DirectoryExists OUT; 
						  IF @DirectoryExists = 0 EXEC master.sys.xp_create_subdir @BackupDestination;"
			
			$result = Invoke-SQL $databaseServer $databaseName $sqlScript  -username $username -password $password
			$backupFileName = $backupLocation+'\'+$backupIdentifier+'\'+$databaseName+"-"+$backupIdentifier+".bak"
			$continueBackup = $true
		}
		
		# create backup folder if it doesn't exist
		New-Item -ItemType Directory -Force -Path (Split-Path -parent $backupFileName) | Out-Null

		if ($continueBackup -eq $True)
		{

			$backupScript = "BACKUP DATABASE ["+$databaseName+"] TO  DISK = N'"+$backupFileName+"' WITH COPY_ONLY, NOFORMAT, INIT,  NAME = N'"+$databaseName+"-Full Database Backup', SKIP, NOREWIND, NOUNLOAD "+ $compressionOption  +", STATS = 10"

			if( -not($useParallelJob)  )
			{
				Write-Progress -Activity ($activityDescription) -Status "Please wait..."
        		$result = Invoke-SQL $databaseServer $databaseName $backupScript -username $username -password $password -timeOut $timeout
				Write-Progress -Activity $activityDescription -Completed -Status ("    " + $dbToBackup  + " Backed Up")
				LogWrite ("    " + $dbToBackup  + " Backed Up")
			}
			else
			{
				LogWrite "$activityDescription started"
				$sc = {
					param($databaseServer, $databaseName, $backupScript,  $username, $password, $timeOut)
					$result = Invoke-SQL $databaseServer $databaseName $backupScript -username $username -password $password -timeOut $timeOut
				}
				
				$initscript= [scriptblock]::create(@"
					[Environment]::SetEnvironmentVariable("CMFInstallActionsPreventClear", "true")
import-module -name "$ModulePath\CMFInstallActions.psd1"
"@)

				$bgJobs += Start-Job -Name $activityDescription -ScriptBlock $sc -ArgumentList @($databaseServer, $databaseName, $backupScript,  $username, $password, $timeOut) -InitializationScript $initscript
			}
		}
		else
		{
			LogWrite ("    Backup of database " + $dbToBackup + " was bypassed due to previous backup file...") -foregroundColor Yellow 
		}
    }
	if( $useParallelJob ){
		Wait-Job -Job $bgJobs | Out-Null

		foreach ($job in $bgJobs) {
			$RemoteErr = $null
			$output = Receive-Job $job -Keep -ErrorVariable RemoteErr
			if( $RemoteErr -ne  $null )
			{
				throw $RemoteErr
			} else {
				LogWrite ("    " + $job.Name  + " finished") 
			}
		}
	}

    LogWrite " *  Finished database backup!" -foregroundColor Green 
}

<#
    .SYNOPSIS
        Restore cmNavigo databases

    .DESCRIPTION
        Restore one or all cmNavigo databases from a predefined folder (according to environment configuration). 

        Online database backups are stored on the path specified on the $env.OnlineBackupLocation property.
        ODS database backups are stored on the path specified on the $env.ODSBackupLocation property.
        DWH database backups are stored on the path specified on the $env.DWHBackupLocation property.
        If the specific backup folder for a given database is null or not defined, the standard backup location will be used,
        specified on the $env.BackupLocation property.

        Backups are identified by a backupIdentifier token which should correspond to a previously taken backup.

		AlwaysOn High-Availability Groups are supported. Make sure to set the $env.OnlineClusterAlwaysOn, 
		$env.ODSClusterAlwaysOn and $env.DWHClusterAlwaysOn to $true.
		The definition of the replicas included in the HADR cluster is also required, using:

		$env.OnlineReplicas += 'CMF-VM-CLT-DB1\ONLINE2014'
		$env.OnlineReplicas += 'CMF-VM-CLT-DB2\ONLINE2014'
		$env.ODSReplicas = $env.OnlineReplicas
		$env.DWHReplicas = $env.OnlineReplicas
        
    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER backupIdentifier
        Token used to identify the backup files to be restored.

    .PARAMETER database
        Database to be restored. Possible values include 'Online', 'ODS' and 'DWH'.
        If not specified, all databases are restored, in the following order: Online, ODS, DWH.

	.PARAMETER requireConfirmation
		Set this flag to force an interactive confirmation from the user before restoring the database.

    .EXAMPLE 
        Restore-CMFDatabase $env

    .EXAMPLE 
        Restore-CMFDatabase $env -backupIdentifier 'Before4.2'
        
    .EXAMPLE 
        Restore-CMFDatabase $env -backupIdentifier 'Before4.2' -database 'Online'   

    .NOTES
        Backup file names are assumed to follow the naming convention: <databaseName>-<backupIdentifier>.bak
        Before restore, existing database is set to single user mode with immediate rollback. Database is restored to multi-user mode after
        restore is complete.
        After completion of the restore procedure, database is set as TRUSTWORTHY and the dbowner is changed to 'sa'.

        cmNavigo application hosts must be stopped during the restore procedure.
#>
function Restore-CMFDatabase
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # BackupID
        [Parameter(Mandatory=$True)]
        [string]$backupIdentifier,

        # Database
        [Parameter(Mandatory=$False)]
        $database,

		# Require Confirmation
        [Parameter(Mandatory=$False)]
		[switch]$requireConfirmation,
		
		[Parameter(Mandatory=$False)]
        [int]$timeout = 600
    )

    LogWrite " >  Starting database restore..."

    [CMFDatabase[]] $dbsToBackup = @()
    if($database)
    {
        $dbsToBackup += $database
    }
    else
    {
        $dbsTobackup = @('Online', 'ODS', 'DWH')
    }    

    foreach($dbToBackup in $dbsToBackup) 
    {
		$activityDescription = "Restoring $dbToBackup database"
		LogWrite ("    " + $activityDescription)
		Write-Progress -Activity ($activityDescription) -Status "Please wait..."	

		$username = ""
		$password = ""

        switch ($dbToBackup) 
        {
            "Online" 
            {
                $databaseName = $env.OnlineDBName
                $databaseServer = $env.OnlineClusterInstance
                $backupLocation = $env.OnlineBackupLocation
				$replicas = $env.OnlineReplicas

				if ($env.SQLOnlineWindowsAuthentication -eq $false)
				{
					$username = $env.SQLOnlineUser
					$password = Get-ClearTextFromEncryptedString $env.SQLOnlinePassword
				}

				$isAlwaysOn = $env.OnlineClusterAlwaysOn
            }
            "ODS" 
            {
                $databaseName = $env.ODSDBName
                $databaseServer = $env.ODSClusterInstance
                $backupLocation = $env.ODSBackupLocation
				$replicas = $env.ODSReplicas
				
				if ($env.SQLODSWindowsAuthentication -eq $false)
				{
					$username = $env.SQLODSUser
					$password = Get-ClearTextFromEncryptedString $env.SQLODSPassword
				}

				$isAlwaysOn = $env.ODSClusterAlwaysOn
            }
            "DWH" 
            {
                $databaseName = $env.DWHDBName
                $databaseServer = $env.DWHClusterInstance
                $backupLocation = $env.DWHBackupLocation
				$replicas = $env.DWHReplicas

				if ($env.SQLDWHWindowsAuthentication -eq $false)
				{
					$username = $env.SQLDWHUser
					$password = Get-ClearTextFromEncryptedString $env.SQLDWHPassword
				}

				$isAlwaysOn = $env.DWHClusterAlwaysOn
            }
        }

		if ($requireConfirmation) 
		{
			if($global:InteractiveMode -eq "" -Or $global:InteractiveMode -eq $true){
				Write-host "    Please confirm if you want to proceed with Restore operation for $dbToBackup database"
				Write-Host "       Do not restore [n]" -ForegroundColor Yellow -NoNewline
				Write-Host "       Proceed with restore [y]: " -NoNewline
			}
                
			$confirmation = Read-Host 
			if ($confirmation -ne 'y')
			{
				# If the user does not confirm the restore, proceed to the next database
				LogWrite ("    Restore of database $dbToBackup was not performed due to user selection.") -foregroundColor Yellow 
				continue
			}
		}

        # Get default backup location if specific is null
        if ($null -eq $backupLocation)
        {
            $backupLocation = $env.BackupLocation
        }
		
        # Prepare scripts
        if ($null -eq $backupIdentifier) 
        {
            $backupIdentifier = (Get-Date -Format "yyyy.MM.dd-HHmmss")
        } 
	
        $backupFileName = $backupLocation + "\" + $backupIdentifier + "\" + $databaseName + "-" + $backupIdentifier + ".bak"

		if ($isAlwaysOn -ne $true) 
		{
			# No AlwaysOn - single server or windows failover clustering - use regular restore scripts
			Restore-SqlDatabase -instanceName $databaseServer `
								-databaseName $databaseName `
								-backupFilename $backupFileName `
								-username $username `
								-password $password `
								-timeout $timeout
		}
        else 
		{
			# AlwaysOn Logic
			$availabilityGroupName = Get-AvailabilityGroupNameFromListenerAddress 	-instanceName $databaseServer `
																					-username $username `
																					-password $password

			# Determine what server is acting as primary
			$primaryServer = Get-PrimaryForAvailabilityGroup `
				-replicas $replicas `
				-database $databaseName `
				-username $username -password $password

			# Remove DB from AG and drop from secondary
			Write-Progress -Activity $activityDescription -Status "Removing $dbtoBackup database from Availability Group..."
			Remove-DatabaseFromAvailabilityGroup `
				-database $databaseName `
				-availabilityGroup $availabilityGroupName `
				-replicas $replicas `
				-primary $primaryServer `
				-DropFromSecondary `
				-username $username -password $password

			# Restore on primary server
			Write-Progress -Activity $activityDescription -Status "Restoring $dbtoBackup database on primary replica..."
			Restore-SqlDatabase `
				-databaseName $databaseName `
				-instanceName $primaryServer `
				-backupFilename $backupFileName `
				-username $username -password $password `
				-timeout $timeout

			# Re-add to availability group
			$backupLocation = $env.BackupLocation
			Write-Progress -Activity $activityDescription -Status "Joining $dbtoBackup database to Availability Group..."
			Add-DatabaseToAvailabilityGroup `
				-database $databaseName `
				-availabilityGroupInstanceName $databaseServer `
				-primary $primaryServer `
				-replicas $replicas `
				-backupLocation $backupLocation `
				-username $username -password $password

		}

		Write-Progress -Activity $activityDescription -Completed -Status "Restore of $dbToBackup database has been completed."
    }

    LogWrite " *  Finished database restore!" -foregroundColor Green 
}

<#
	.SYNOPSIS
        Restores an SQL database

    .DESCRIPTION
        Executes a simple restore command for an SQL database

	.PARAMETER instanceName
        SQL Server instance name against which the RESTORE statement will be ran

    .PARAMETER databaseName
        Name of the database to be backed up

	.PARAMETER backupFilename
		Path to the backup file to restore

	.PARAMETER username
		Username to be used to connect to the database. If $null, Windows Authentication will be used.

	.PARAMETER password 
		Password used to connect to the database. If $username is not set, Windows Authentication will be used.

	.PARAMETER restoreOnly
		If set, no operation will be performed on the existing database before restoring (not set to single_user)

	.PARAMETER norecovery
		If set, the NORECOVERY option is used. Required for restoring database replicas for AlwaysOn.
    
    .EXAMPLE 
        Restore-SQLDatabase 'MSSQL\ONLINE' 'Development' 'C:\Backup.bak'
#>
function Restore-SQLDatabase
{
	param
	(
		# Instance name
		[Parameter(Mandatory=$True)]
		[string] $instanceName,

		# Database Name
        [Parameter(Mandatory=$True)]
        $databaseName,

		# Backup file name
		[Parameter(Mandatory=$True)]
		[string] $backupFilename,

        # Username
		[Parameter(Mandatory=$False)]
        [string] $username,

		# Password
        [Parameter(Mandatory=$False)]
        [string] $password,

		# Restore only
		[Parameter(Mandatory=$False)]
		[switch] $restoreOnly,

		# Use NORECOVERY option
		[Parameter(Mandatory=$False)]
		[switch] $norecovery,

		[Parameter(Mandatory=$False)]
		[string] $TargetLocation,

		[Parameter(Mandatory=$False)]
		[int] $timeout = 600

	)

	$restoreScript = ""
	if (!$restoreOnly) 
	{
		$restoreScript += "ALTER DATABASE [" + $databaseName + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE `n"
	}
	if ($norecovery) 
	{
		$restoreScript += "RESTORE DATABASE [" + $databaseName + "] FROM  DISK = N'" + $backupFileName + "' WITH NORECOVERY, FILE = 1,  NOUNLOAD,  REPLACE,  STATS = 5 `n"
	}
	else 
	{
		$restoreScript += "RESTORE DATABASE [" + $databaseName + "] FROM  DISK = N'" + $backupFileName + "' WITH  FILE = 1,  NOUNLOAD,  REPLACE,  STATS = 5 `n"
		$restoreScript += "ALTER DATABASE [" + $databaseName + "] SET MULTI_USER `n"
		$restoreScript += "GO `n"
		$restoreScript += "USE [" + $databaseName + "] `n"
		$restoreScript += "GO `n"
		$restoreScript += "ALTER DATABASE CURRENT SET TRUSTWORTHY ON `n"
		$restoreScript += "EXEC sp_changedbowner 'sa'"
	}
	$result = Invoke-SQL $instanceName 'master' $restoreScript -username $username -password $password -timeout $timeout
	
}

<#
	.SYNOPSIS
        Executes  scripts  of a entire folder

    .DESCRIPTION
        Executes all scripts ( *.sql ) against a database 

    .PARAMETER dataSource
        Data source name (i.e. instance name) to be used when connecting to the database.
    
    .PARAMETER folderPath
        Folder containing the scripts to be executed

    .EXAMPLE 
        Invoke-PackageSqlScripts $env 'Online' 'c:\package\database\online'
#>
function Invoke-PackageSqlScripts
{
    param
	(
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # CMFDataBase
		[Parameter(Mandatory=$True)]
        $dataBase,

        #directory  to enumerate the files
        [Parameter(Mandatory=$True)]
        $folderPath,
		
		[Parameter(Mandatory=$False)]
        $timeOut = $null
    )

    if( Test-Path $folderPath )
    {
       foreach($file in ( Get-ChildItem $folderPath -Filter '*.sql') )
       {
             Invoke-SQLScript $env -database $database -scriptPath $file.FullName -timeout $timeOut
        }
    }
}

<#
	.SYNOPSIS
        Removes a database from an availability group

    .DESCRIPTION
        Removes a given system database from the respective HADR availability group.
		Optionally, the database can be dropped from all secondary replicas, which is useful for a restore operation.

	.PARAMETER availabilityGroup
        Instance name for the Availability Group

	.PARAMETER replicas
		List of replicas in the availability group

    .PARAMETER database
        database to be removed from the availability group
    
	.PARAMETER primary
		Primary server for the availability group

	.PARAMETER username
		Username to be used to connect to the database. If $null, Windows Authentication will be used.

	.PARAMETER password 
		Password used to connect to the database. If $username is not set, Windows Authentication will be used.

	.PARAMETER DropFromSecondary
		Set to true to enable dropping the database from the secondary servers

    .EXAMPLE 
		$replicas += 'CMF-VM-CLT-DB1\ONLINE2012'
		$replicas += 'CMF-VM-CLT-DB2\ONLINE2012'
        Remove-DatabaseFromAvailabilityGroup -availability 'AG2012\ONLINE' -replicas $replicas -database 'Development'

	.EXAMPLE 
		$replicas += 'CMF-VM-CLT-DB1\ONLINE2012'
		$replicas += 'CMF-VM-CLT-DB2\ONLINE2012'
		$primary = 'CMF-VM-CLT-DB2\ONLINE2012'
        Remove-DatabaseFromAvailabilityGroup -availability 'AG2012\ONLINE' -replicas $replicas -database 'Development' -primary $primary -DropFromSecondary
#>
function Remove-DatabaseFromAvailabilityGroup 
{
	param
	(
		# availability Group name
        [Parameter(Mandatory=$True)]
        [string] $availabilityGroup,
		
        # Availability group replicas
		[Parameter(Mandatory=$True)]
        $replicas,

		# Database
		[Parameter(Mandatory=$True)]
		[string] $database,

		# primary server
		[Parameter(Mandatory=$False)]
		$primary,
		
		# Username
		[Parameter(Mandatory=$False)]
        [string] $username,

		# Password
        [Parameter(Mandatory=$False)]
        [string] $password,

		# Drop from secondary servers
		[Parameter(Mandatory=$False)]
		[switch] $DropFromSecondary = $False
	)

	# Check which server is primary
	if ($null -eq $primary)
	{
		$primary = Get-PrimaryForAvailabilityGroup -replicas $replicas -database $database -username $username -password $password
	}
	

	# Remove database from availability group
	$sql = "ALTER AVAILABILITY GROUP [$availabilityGroup] REMOVE DATABASE [$database];  "
	$result = Invoke-SQL $primary 'master' $sql -username $username -password $password
	
	# Drop database from all secondary servers
	if ($DropFromSecondary) 
	{
		foreach($server in $replicas)
		{
			if ($server -ne $primary)
			{
				# Wait for database to be in restoring state before continuing -- otherwise DROP DATABASE will fail
				$sql = "WHILE ((select DATABASEPROPERTYEX('$database', 'Status')) <> 'RESTORING') WAITFOR DELAY '00:00:01'"
				$result = Invoke-SQL $server 'master' $sql -username $username -password $password -timeOut 120

				# Drop it.
				$sql = "DROP DATABASE [$database];"
				$result = Invoke-SQL $server 'master' $sql -username $username -password $password
			}
		}
	}
	
}

<#
	.SYNOPSIS
        Adds a database to an availability group

    .DESCRIPTION
        Adds a given system database the an respective availability group.
		
		Assumes that the secondary replica does not contain a database with the same name. Script will perform full 
		backups and log backups from the primary replica and apply them to the secondary replicas before joining the availability group.

	.PARAMETER availabilityGroupInstanceName
        Instance name for the Availability Group

	.PARAMETER replicas
		List of replicas in availability group in the availability group

    .PARAMETER database
        Database to be added to the availability group
    
	.PARAMETER backupLocation
		Path to store temporary database and log backups required for adding databases to HADR cluster

	.PARAMETER primary
		Primary server for the availability group

	.PARAMETER username
		Username to be used to connect to the database. If $null, Windows Authentication will be used.

	.PARAMETER password 
		Password used to connect to the database. If $username is not set, Windows Authentication will be used.

    .EXAMPLE 
		$replicas += 'CMF-VM-CLT-DB1\ONLINE2012'
		$replicas += 'CMF-VM-CLT-DB2\ONLINE2012'
		$primary = 'CMF-VM-CLT-DB2\ONLINE2012'
        Remove-DatabaseFromAvailabilityGroup 'AG2012\ONLINE' -replicas $replicas -database 'Development' -primary $primary
#>
function Add-DatabaseToAvailabilityGroup 
{
	param
	(
		# availability Group instance name
        [Parameter(Mandatory=$True)]
        [string] $availabilityGroupInstanceName,
		
        # Availability group nodes
		[Parameter(Mandatory=$True)]
        $replicas,

		# Database
		[Parameter(Mandatory=$True)]
		[string] $database,

		# Backup location
		[Parameter(Mandatory=$True)]
		[string] $backupLocation,

		# primary server
		[Parameter(Mandatory=$True)]
		$primary,
		
		# Username
		[Parameter(Mandatory=$False)]
        [string] $username,

		# Password
        [Parameter(Mandatory=$False)]
        [string] $password
	)
	
	$availabilityGroup = $availabilityGroupInstanceName.Split("\")[0]

	# Add database to availability group
	$sql = "ALTER AVAILABILITY GROUP [" + $availabilityGroup + "] ADD DATABASE ["+ $database + "]"
	$result = Invoke-SQL $availabilityGroupInstanceName 'master' $sql -username $username -password $password

	# Create primary full backup and log backup
	$timestamp = Get-Date -format yyyymmddhhmmss
	$fullbackupPath = Join-Path $backupLocation "$databaseName_$timestamp.bak"
	$logbackupPath = Join-Path $backupLocation "$databaseName_$timestamp.trn"

	$sql = "BACKUP DATABASE [$database] TO  DISK = N'$fullbackupPath' WITH  COPY_ONLY, FORMAT, INIT, SKIP, REWIND, NOUNLOAD, COMPRESSION,  STATS = 5"
	$result = Invoke-SQL $availabilityGroupInstanceName 'master' $sql -username $username -password $password
	
	$sql = "BACKUP LOG [$database] TO  DISK = N'$logbackupPath' WITH COPY_ONLY, NOFORMAT, NOINIT, NOSKIP, REWIND, NOUNLOAD, COMPRESSION,  STATS = 5"
	$result = Invoke-SQL $availabilityGroupInstanceName 'master' $sql -username $username -password $password

	# Restore database on secondary replicas
	foreach($server in $replicas) 
	{
		if ($server -ne $primary)
		{
			# Ensure replica has privileges to create databases
			$sql = "ALTER AVAILABILITY GROUP $availabilityGroup GRANT CREATE ANY DATABASE;"
			$result = Invoke-SQL $server 'master' $sql -username $username -password $password

			# Get seeding mode to determine if restore is to be made or not...
			$sql = 	"DECLARE @ReturnValue NVARCHAR(512) = N'MANUAL'
					DECLARE @DBVersion INT
					DECLARE @StringVersion	NVARCHAR(512)	= CONVERT(NVARCHAR, SERVERPROPERTY('productversion'))
					DECLARE @FinalChar		INTEGER			= CHARINDEX('.', @StringVersion, 1)

					IF( @FinalChar > 1)
					BEGIN
						SET @StringVersion = SUBSTRING(@StringVersion, 1, @FinalChar - 1)
					END

					SET @DBVersion = CONVERT(INTEGER, @StringVersion)
					IF(@DBVersion >= 13)
					BEGIN

						SELECT @ReturnValue = AR.[seeding_mode_desc]
						from [sys].[availability_replicas] AR
						INNER JOIN [sys].[availability_groups] AG ON AG.[group_id] = AR.[group_id]
						WHERE AG.[name] = '$availabilityGroup' AND AR.[replica_server_name] = '$server'

					END

					SELECT @ReturnValue SeedingMode"
			$seedingMode = Invoke-SQL $server 'master' $sql -username $username -password $password -returnScalar

			# If we're dealing with a manual seeding mode, restore database in secondary replica along with the log
			if($seedingMode -eq "MANUAL") {

				# Restore Full backup
				$sql = "RESTORE DATABASE [$database] FROM  DISK = N'$fullbackupPath' WITH  NORECOVERY,  NOUNLOAD,  STATS = 5"
				$result = Invoke-SQL $server 'master' $sql -username $username -password $password

				# Restore log			
				$sql = "RESTORE LOG [$database] FROM  DISK = N'$logbackupPath' WITH  NORECOVERY,  NOUNLOAD,  STATS = 5"
				$result = Invoke-SQL $server 'master' $sql -username $username -password $password

				# Wait for replica to become online and add to availability group
				$sql = @'
				begin try
				declare @conn bit
				declare @count int
				declare @replica_id uniqueidentifier 
				declare @group_id uniqueidentifier
				set @conn = 0
				set @count = 30 -- wait for 5 minutes 

				if (serverproperty('IsHadrEnabled') = 1)
					and (isnull((select member_state from master.sys.dm_hadr_cluster_members where upper(member_name) = upper(cast(serverproperty('ComputerNamePhysicalNetBIOS') as nvarchar(256)))), 0) <> 0)
					and (isnull((select state from master.sys.database_mirroring_endpoints), 1) = 0)
				begin
					select @group_id = ags.group_id from master.sys.availability_groups as ags where name = N'$(AVAILABILITYGROUPNAME)'
					select @replica_id = replicas.replica_id from master.sys.availability_replicas as replicas where upper(replicas.replica_server_name) = upper(@@SERVERNAME) and group_id = @group_id
					while @conn <> 1 and @count > 0
					begin
						set @conn = isnull((select connected_state from master.sys.dm_hadr_availability_replica_states as states where states.replica_id = @replica_id), 1)
						if @conn = 1
						begin
							-- exit loop when the replica is connected, or if the query cannot find the replica status
							break
						end
						waitfor delay '00:00:10'
						set @count = @count - 1
					end
				end
				end try
				begin catch
					-- If the wait loop fails, do not stop execution of the alter database statement
				end catch
				ALTER DATABASE [$(DATABASE)] SET HADR AVAILABILITY GROUP = [$(AVAILABILITYGROUPNAME)];
'@

			$sql = $sql.Replace('$(AVAILABILITYGROUPNAME)', $availabilityGroup)
			$sql = $sql.Replace('$(DATABASE)', $database)

			$result = Invoke-SQL $server 'master' $sql -username $username -password $password

			}
		}
	}

	try {
		Remove-Item $fullbackupPath -ErrorAction Stop
	}
	catch {
		LogWrite " > Error while removing $fullbackupPath" -foreGroundColor Red
	}

	try {
		Remove-Item $logbackupPath -ErrorAction Stop
	}
	catch {
		LogWrite " > Error while removing $logbackupPath" -foreGroundColor Red
	}
}

<#
	.SYNOPSIS
        Returns the primary server for an availability group

    .DESCRIPTION
        Returns the name of the primary SQL server instance on a given availability group

	.PARAMETER Replicas
		List of all replicas in availability group

    .PARAMETER database
        Name of the database to be checked

	.PARAMETER username
		Username to be used to connect to the database. If $null, Windows Authentication will be used.

	.PARAMETER password 
		Password used to connect to the database. If $username is not set, Windows Authentication will be used.
    
    .EXAMPLE 
        Get-PrimaryForAvailabilityGroup -replicas $nodes -database $databaseName -username $username -password $password
#>
function Get-PrimaryForAvailabilityGroup
{
	param
	(		
        # Availability group nodes
		[Parameter(Mandatory=$True)]
        $replicas,

		# Database
		[Parameter(Mandatory=$True)]
		[string] $database,

        # Username
		[Parameter(Mandatory=$False)]
        [string] $username,

		# Password
        [Parameter(Mandatory=$False)]
        [string] $password
	)

	$sql = "select role_desc "
	$sql += "from sys.dm_hadr_availability_replica_states states "
	$sql += "inner join sys.databases dbs on dbs.replica_id = states.replica_id "
	$sql += "where is_local = 1 and name = '" + $database + "'"
	foreach($server in $replicas) 
	{
		$result = Invoke-SQL $server 'master' $sql -username $username -password $password -returnScalar
		if ($result -eq "PRIMARY")
		{
			$primaryServer = $server
			break
		}
	}

    if ($null -eq $primaryServer)
    {
        throw "Database is not part of any availability group"
    }

	return $primaryServer
}

<#
	.SYNOPSIS
        Returns the availability group name from the listener address

    .DESCRIPTION
        Returns the availability group name from the listener address

	.PARAMETER instanceName
		Address of the availability group instance

	.PARAMETER username
		Username to be used to connect to the database. If $null, Windows Authentication will be used.

	.PARAMETER password 
		Password used to connect to the database. If $username is not set, Windows Authentication will be used.
    
    .EXAMPLE 
        Get-AvailabilityGroupNameFromListenerAddress -instanceName 'AG2016' -database $databaseName -username $username -password $password
#>
function Get-AvailabilityGroupNameFromListenerAddress
{
	param
	(		
        # Availability group instance
		[Parameter(Mandatory=$True)]
        $instanceName,

        # Username
		[Parameter(Mandatory=$False)]
        [string] $username,

		# Password
        [Parameter(Mandatory=$False)]
        [string] $password
	)

	$listenerAddress = $instanceName.Split("\")[0]

	$sql = "SELECT ags.name FROM sys.availability_groups ags "
	$sql += "INNER JOIN sys.availability_group_listeners agsl "
	$sql += "ON ags.group_id = agsl.group_id AND agsl.dns_name = '" + $listenerAddress + "'"

	$agName = Invoke-SQL $instanceName 'master' $sql -username $username -password $password -returnScalar

    if ($null -eq $agName)
    {
        throw "Could not determine Availability Group name for listener address " + $listenerAddress
    }

	return $agName
}

<#
    .SYNOPSIS
        Prompts the interactive user for a password and test against a data source.

    .DESCRIPTION
        Prompts the interactive user for a password and test against a data source.

    .PARAMETER dataSource
        Data source name (i.e. instance name) to be used when connecting to the database.
    
    .PARAMETER username
        Username to use to connect to the sql server if SQL Authentication is used.
        be used.

	 .OUTPUTS
        A password, in CLEAR TEXT, validated against the datasource specified

    .EXAMPLE 
        Read-SQLPassword -dataSource 'CMF-VM-TEST\ONLINE' -username  SQLUser
#>
function Read-SQLPassword
{
	param
	(
		[Parameter(Mandatory=$True)]
        [string] $dataSource,       

        [Parameter(Mandatory=$True)]
        [string] $username
	)

	$securePassword = Read-Host "Enter password for user $username at $dataSource"  -AsSecureString
    $psCred = New-Object System.Management.Automation.PSCredential $username, $securePassword
    [string] $clearPassword = $psCred.GetNetworkCredential().Password


	try
	{
		$dummy = Invoke-SQL -dataSource $dataSource -database 'master' -sqlCommand "SELECT  GETUTCDATE()" -username $username -password $clearPassword
	}
	catch [Exception]
	{
		LogWrite $_.Exception.Message  -ForegroundColor Red
		throw "Invalid password for $username"
	}

	return $clearPassword
}

<#
    .SYNOPSIS
        Prompt message asking if should overwrite a existing backup

    .DESCRIPTION
		Returns the result if should overwrite a existing backup
        
    .PARAMETER $backupPath
        The location of the backup path

    .EXAMPLE 
        OverwriteBackupFile 'c:\filename.zip'


#>
function OverwriteBackupFile
{
	Param
	(
	   [string]$backupPath
	)

	if (Test-Path $backupPath) 
	{ 
		if($global:InteractiveMode -eq "" -Or $global:InteractiveMode -eq $true){
			Write-host "    Backup file " $backupPath "already exists!!"
			Write-Host "       Ignore Backup [i]" -ForegroundColor Yellow -NoNewline
			Write-Host "       or Overwrite [o]: " -NoNewline
		
                
            $confirmation = Read-Host 
        }
        else
        {
            $confirmation = 'o'
        }

        if ($confirmation -ne 'o')
		{
			return $false
		}
	}

	return $true

}
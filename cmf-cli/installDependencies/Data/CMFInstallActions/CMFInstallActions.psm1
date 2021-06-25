$ModulePath = Split-Path $MyInvocation.MyCommand.Path
$Logfile = "$(gc env:computername)" + "_" + (Get-Date).ToString("yyyyMMddHH.mm.ss") + ".log"
$GenericLogFile = $true
$ShowProgressBar = $false

Add-Type -TypeDefinition "public enum CMFDatabase { Online, ODS, DWH, Replication }"

#clear screen when this module is called

if($global:InteractiveMode -eq "" -Or $global:InteractiveMode -eq $true){
    if( -not($env:CMFInstallActionsPreventClear ))
    {
        clear
    }
}

# Set Global Variables for CMF Folders
if( !($CMFFoldersLoaded))
{
    Set-Variable CMFFoldersLoaded -Value $true -scope global
	if( !($CMFBusinessTier))
	{
		Set-Variable CMFBusinessTier -Value 'BusinessTier' -scope global
    }
	if( !($CMFDiscoveryService))
	{
		Set-Variable CMFDiscoveryService -Value 'DiscoveryService' -scope global
    }
	if( !($CMFHelp))
	{
		Set-Variable CMFHelp -Value 'UI\Help' -scope global
    }
	if( !($CMFHTML))
	{
		Set-Variable CMFHTML -Value 'UI\Html' -scope global
    }
	if( !($CMFLBOGenerator))
	{
		Set-Variable CMFLBOGenerator -Value 'LBOGenerator' -scope global
    }
	if( !($CMFLBOGeneratorWCF))
	{
		Set-Variable CMFLBOGeneratorWCF -Value 'LBOGeneratorWCF' -scope global
    }
	if( !($CMFMasterDataLoader))
	{
		Set-Variable CMFMasterDataLoader -Value 'MasterDataLoader' -scope global
    }
	if( !($CMFMessageBusGateway))
	{
		Set-Variable CMFMessageBusGateway -Value 'MessageBusGateway' -scope global
    }
	if( !($CMFPresentationTier))
	{
		Set-Variable CMFPresentationTier -Value 'UI\Silverlight' -scope global
    }
	if( !($CMFProtectUnprotectConfigFile))
	{
		Set-Variable CMFProtectUnprotectConfigFile -Value 'ProtectUnprotectConfigFile' -scope global
    }
}

<#
    .SYNOPSIS
        Creates a new object to represent a cmNavigo environment

    .DESCRIPTION
        Returns a new custom object which contains all the required information to run the installation scripts.

    .PARAMETER SystemName
        cmNavigo environment name selected during installation. This is usually part of the website address.

    .EXAMPLE
        $env = New-CMFEnvironment 'Staging'
        $env.NLBAddress = 'cmf-vm-test'
        $env.ServicePort = 9050
        $env.ReadPasswordToSignGeneratedLBOs = $true
        $env.ApplicationServers += New-CMFServer 'cmf-vm-test'
        $env.OnlineClusterInstance = 'cmf-vm-test\ONLINE'
        $env.ODSClusterInstance = 'cmf-vm-test\ONLINE'
        $env.DWHClusterInstance = 'cmf-vm-test\ONLINE'
        $env.NavigoServiceName = 'cmFoundationSrv' + $env.SystemName + 'Host'
        $env.BackupLocation = '\\CMF-VM-test\'+$env.SystemName+'Shares\Backups\Test'
        $env.OnlineBackupLocation = '\\CMF-VM-test\'+$env.SystemName+'Shares\Backups\Test\Online'
        $env.ODSBackupLocation = '\\CMF-VM-test\'+$env.SystemName+'Shares\Backups\Test\ODS'
        $env.DWHBackupLocation = '\\CMF-VM-test\'+$env.SystemName+'Shares\Backups\Test\DWH'
        $env.SQLOnlineWindowsAuthentication = $true
        $env.SQLOnlineUser = 'user'
        $env.SQLOnlinePassword = 'pass'   #Read-SQLPassword '«DataSource»' '«UserName»'
        $env.SQLODSWindowsAuthentication = $true
        $env.SQLODSUser = 'user'
        $env.SQLODSPassword = 'pass' #Read-SQLPassword '«DataSource»' '«UserName»'
        $env.SQLDWHWindowsAuthentication = $false
        $env.SQLDWHUser = 'user'
        $env.SQLDWHPassword = 'pass' #Read-SQLPassword '«DataSource»' '«UserName»'
        $env.SQLBackupCompression =  $true
        $env.ReportServerUri = 'http://cmf-vm-atsh/ReportServer_Online'
        $env.ReportUseDefaultCredential = $true
        $env.ReportServerUser = '«CMFUser»'
        $env.ReportServerPassword = '«CMFUser»'
        $env.ReportsOnlineDataSource = '/Datasources/StagingAS'
        $env.ReportsODSDataSouce = '/Datasources/StagingODS'
        $env.ReportsDWHDataSource = '/Datasources/StagingDWH'

    .OUTPUTS
        A custom object representing the cmNavigo environment. The object contains the following properties:
        - NLBAddress:           address of the NLB cluster in use (or the single app server if NLB is not in use)
        - ServicePort:          port used for WCF services exposed by the application host
        - ReadPasswordToSignGeneratedLBOs: Flag to read password to GenerateLBOs.  Required when certificate is encrypted with password.
        - ApplicationServers:   list of application servers. Should be objects created with the New-CMFServer function.
        - OnlineClusterInstance:instance name for the ONLINE database
        - ODSClusterInstance:   instance name for the ODS database
        - DWHClusterInstance:   instance name for the DWH database
		- OnlineClusterAlwaysOn set to $true if ONLINE cluster is an AlwaysOn Availability Group
		- ODSClusterInstanceAlwaysOn	 set to $true if ODS cluster is an AlwaysOn Availability Group
		- DWHClusterInstanceAlwaysOn	 set to $true if DWH cluster is an AlwaysOn Availability Group
		- OnlineReplicas		list of database replicas used in HADR cluster for Online database
		- ODSReplicas			list of database replicas used in HADR cluster for ODS database
		- DWHReplicas			list of database replicas used in HADR cluster for DWH database
        - NavigoServiceName     name of the windows service running the application hosts in each app server
        - BackupLocation        location of the database backups (can be overriden by specific locations per database)
        - OnlineBackupLocation: location of the database backups for the online database
        - ODSBackupLocation:    location of the ODS database backups
        - DWHBackupLocation:    location of the DWH database backups
		- SQLOnlineWindowsAuthentication:	Set if the SQL Connection to Online Database is using Windows Authentication
		- SQLOnlineUser:		The SQL Online Username
		- SQLOnlinePassword:	The SQL Online Password for selected user
		- SQLODSWindowsAuthentication:	Set if the SQL Connection to ODS Database is using Windows Authentication
		- SQLODSUser:			The SQL ODS Username
		- SQLODSPassword:		The SQL ODS Password for selected user
		- SQLDWHWindowsAuthentication:	Set if the SQL Connection to DataWareHouse Database is using Windows Authentication
		- SQLDWHUser:			The SQL DataWareHouse Username
		- SQLDWHPassword:		The SQL DataWareHouse Password for selected user
		- SQLBackupCompression  Flag to compress  database backups
		- ReportServerUri:		The ReportServer Uri of Reports
		- ReportUseDefaultCredential:	Set if the connection to the Report Server is using the default credentails, or if need to set credentials
		- ReportServerUser:		 The Username to access to reportserver
		- ReportServerPassword:	 The password to access to reportserver for selected user
		- ReportsOnlineDataSource:	The name (and path) of the ReportServer Datasource to database Online 
		- ReportsODSDataSouce:	The name (and path) of the ReportServer Datasource to database ODS 
		- ReportsDWHDataSource:	The name (and path) of the ReportServer Datasource to database DataWareHouse

    .NOTES
        The ApplicationServers properties of the returned object are lists which should be filled
        with objects created using the New-CMFServer function. Check the corresponding help entry for additional
        information.
#>
function New-CMFEnvironment()
{
    param
    (
        [string] $SystemName
    )

    $NavigoServiceName = 'cmHostService' + $SystemName
    $ODSDBName = $SystemName + 'ODS'
    $DWHDBName = $SystemName + 'DWH'
    $ReportsOnlineDataSource = '/Datasources/'+$SystemName
    $ReportsASDataSource = '/Datasources/'+$SystemName+'AS'
    $ReportsODSDataSouce = '/Datasources/'+$SystemName+'ODS'
    $ReportsDWHDataSource = '/Datasources/'+$SystemName+'DWH'

    $environment = new-object PSObject

    $environment | add-member -type NoteProperty -Name SystemName -Value $SystemName

    $environment | add-member -type NoteProperty -Name NavigoServiceName -Value $NavigoServiceName

    # derived database names
    $environment | add-member -type NoteProperty -Name OnlineDBName -Value $SystemName
    $environment | add-member -type NoteProperty -Name ODSDBName -Value $ODSDBName
    $environment | add-member -type NoteProperty -Name DWHDBName -Value $DWHDBName

    # database instances
    $environment | add-member -type NoteProperty -Name OnlineClusterInstance -Value $null
    $environment | add-member -type NoteProperty -Name ODSClusterInstance -Value $null
    $environment | add-member -type NoteProperty -Name DWHClusterInstance -Value $null
    $environment | add-member -type NoteProperty -Name OnlineClusterAlwaysOn -Value $false
    $environment | add-member -type NoteProperty -Name ODSClusterAlwaysOn -Value $false
    $environment | add-member -type NoteProperty -Name DWHClusterAlwaysOn -Value $false
    $environment | add-member -type NoteProperty -Name OnlineReplicas -Value @()
    $environment | add-member -type NoteProperty -Name ODSReplicas -Value @()
    $environment | add-member -type NoteProperty -Name DWHReplicas -Value @()

    # server names
    $environment | add-member -type NoteProperty -Name ApplicationServers -Value @()
    $environment | add-member -type NoteProperty -Name NLBAddress -Value $null
    $environment | add-member -type NoteProperty -Name ServicePort -Value $null
    $environment | add-member -type NoteProperty -Name ReadPasswordToSignGeneratedLBOs -Value $false

    # backup locations
    $environment | add-member -type NoteProperty -Name BackupLocation -Value $null
    $environment | add-member -type NoteProperty -Name OnlineBackupLocation -Value $null
    $environment | add-member -type NoteProperty -Name ODSBackupLocation -Value $null
    $environment | add-member -type NoteProperty -Name DWHBackupLocation -Value $null

    #backup option
    $environment | add-member -type NoteProperty -Name SQLBackupCompression -Value $null


    # user / password for SQL Online
    $environment | add-member -type NoteProperty -Name SQLOnlineWindowsAuthentication -Value $null
    $environment | add-member -type NoteProperty -Name SQLOnlineUser -Value $null
    $environment | add-member -type NoteProperty -Name SQLOnlinePassword -Value $null

    # user / password for SQL ODS
    $environment | add-member -type NoteProperty -Name SQLODSWindowsAuthentication -Value $null
    $environment | add-member -type NoteProperty -Name SQLODSUser -Value $null
    $environment | add-member -type NoteProperty -Name SQLODSPassword -Value $null

    # user / password for SQL DWH
    $environment | add-member -type NoteProperty -Name SQLDWHWindowsAuthentication -Value $null
    $environment | add-member -type NoteProperty -Name SQLDWHUser -Value $null
    $environment | add-member -type NoteProperty -Name SQLDWHPassword -Value $null

    #report server configuration
    $environment | add-member -type NoteProperty -Name ReportServerUri -Value $null
    $environment | add-member -type NoteProperty -Name ReportUseDefaultCredential -Value $null
    $environment | add-member -type NoteProperty -Name ReportServerUser -Value $null
    $environment | add-member -type NoteProperty -Name ReportServerPassword -Value $null
    $environment | add-member -type NoteProperty -Name ReportsOnlineDataSource -Value $ReportsOnlineDataSource
    $environment | add-member -type NoteProperty -Name ReportsODSDataSouce -Value $ReportsODSDataSouce
    $environment | add-member -type NoteProperty -Name ReportsDWHDataSource -Value $ReportsDWHDataSource
    $environment | add-member -type NoteProperty -Name ReportsASDataSource -Value $ReportsASDataSource

    #erp generation
    $environment | add-member -type NoteProperty -Name GenerateErpCustomManagement -Value $true

    # TemporaryFileShare information
    $environment | add-member -type NoteProperty -Name TemporaryFileShare -Value $null

    # user / password for HOST
    $environment | add-member -type NoteProperty -Name AdminUser -Value $null
    $environment | add-member -type NoteProperty -Name AdminPass -Value $null

    # tenant information
    $environment | add-member -type NoteProperty -Name ClientTenantName -Value $null

    # UseSSL information
    $environment | add-member -type NoteProperty -Name UseSSL -Value $null

    return $environment
}

<#
    .SYNOPSIS
        Creates a new object to represent a server that's part of a cmNavigo installation.

    .DESCRIPTION
        Returns a new custom object which contains all the required configuration properties for a server that's part of
        a cmNavigo installation. Some of the properties on the returned object are only applicable to application servers.

    .PARAMETER ServerName
        Machine name used to identify the server.

    .EXAMPLE
        $appServer = New-CMFServer 'cmf-vm-test'
        $appServer.BusinessInstallationPath = 'C:\'+$env.SystemName+'\Business Tier'
        $appServer.GUIInstallationPath = 'C:\inetpub\wwwroot\' + $env.SystemName
        $appServer.XAPUpdaterPath = 'C:\'+$env.SystemName+'\cmNavigo Tools\XapUpdater'

    .EXAMPLE
        $env = New-CMFEnvironment 'Staging'
        $env.ApplicationServers += New-CMFServer 'cmf-vm-test'

        # Installation Paths on App Servers
        foreach($appServer in $env.ApplicationServers)
        {
            $appServer.BusinessInstallationPath = 'C:\'+$env.SystemName+'\Business Tier'
            $appServer.GUIInstallationPath = 'C:\inetpub\wwwroot\' + $env.SystemName
            $appServer.XAPUpdaterPath = 'C:\'+$env.SystemName+'\cmNavigo Tools\XapUpdater'
        }

    .OUTPUTS
        A custom object representing the cmNavigo server. The object contains the following properties:
        - BusinessInstallationPath: Local path to the business tier installation folder
        - GUIInstallationPath:      Local path to the presentation tier installation folder (website)
        - XAPUpdaterPath:           Local path to the XAPUpdated tool, used to generate LBOs on the server
#>
function New-CMFServer()
{
    param
    (
        [Parameter(Mandatory=$True)]
        [string] $ServerName,
        [Parameter(Mandatory=$False)]
        [string] $InstallationPath = $null
    )

    # Version dependent
    $appServerBusinessInstallationPath = $InstallationPath + "\$CMFBusinessTier"
    $appServerXAPUpdaterPath = $InstallationPath + "\$CMFLBOGeneratorWCF\XapUpdater"
    $appServerGUIInstallationPath = $InstallationPath + "\$CMFPresentationTier"
    $appServerHTMLPath = $InstallationPath + "\$CMFHTML"
    $appServerHelpPath = $InstallationPath + "\$CMFHelp"
    $appServerLBOGeneratorPath = $InstallationPath + "\$CMFLBOGenerator"
    $appServerMasterDataLoaderPath = $InstallationPath + "\$CMFMasterDataLoader"
    $appServerProtectUnprotectConfigFilePath = $InstallationPath + "\$CMFProtectUnprotectConfigFile"

    $server = new-object PSObject

    # Main Properties
    $server | add-member -type NoteProperty -Name ServerName -Value $ServerName
    $server | add-member -type NoteProperty -Name InstallationPath -Value $InstallationPath

    # Business and GUIs
    $server | add-member -type NoteProperty -Name BusinessInstallationPath -Value $appServerBusinessInstallationPath
    $server | add-member -type NoteProperty -Name GUIInstallationPath -Value $appServerGUIInstallationPath
    $server | add-member -type NoteProperty -Name HTMLPath -Value $appServerHTMLPath
    $server | add-member -type NoteProperty -Name HelpPath -Value $appServerHelpPath

    # Tools
    $server | add-member -type NoteProperty -Name XAPUpdaterPath -Value $appServerXAPUpdaterPath
    $server | add-member -type NoteProperty -Name LBOGeneratorPath -Value $appServerLBOGeneratorPath
    $server | add-member -type NoteProperty -Name MasterDataLoaderPath -Value $appServerMasterDataLoaderPath
    $server | add-member -type NoteProperty -Name ProtectUnprotectConfigFilePath -Value $appServerProtectUnprotectConfigFilePath

    return $server
}

<#
    .SYNOPSIS
        Creates a new object to represent a cmNavigo custom service.

    .DESCRIPTION
        Returns a new custom object which contains all the required configuration properties to load a custom service assembly to
        cmNavigo. Should be used in conjunction with Install-CMFCustomServices function.

    .PARAMETER AssemblyName
        Assembly file name to be loaded

    .PARAMETER ServiceName
        Name of the service to be registered in cmNavigo

    .PARAMETER InterfaceName
        Interface name to be loaded

    .EXAMPLE
        New-CMFService -AssemblyName 'Cmf.Custom.Services.BackEnd.BackEndManagement.dll' -ServiceName 'BEBurnInResultsManagement'     -InterfaceName 'IBEBurnInResultsManagement'

    .OUTPUTS
        A custom object representing the cmNavigo service. The object contains the following properties:
        - AssemblyName:     Assembly file name to be loaded
        - ServiceName:      Name of the service to be registered in cmNavigo
        - InterfaceName:    Interface name to be loaded
#>
function New-CMFService()
{
    param
    (
        [Parameter(Mandatory=$True)]
        [string] $AssemblyName,

        [Parameter(Mandatory=$True)]
        [string] $ServiceName,

        [Parameter(Mandatory=$True)]
        [string] $InterfaceName
    )

    $service = new-object PSObject

    $service | add-member -type NoteProperty -Name AssemblyName  -Value $AssemblyName
    $service | add-member -type NoteProperty -Name ServiceName   -Value $ServiceName
    $service | add-member -type NoteProperty -Name InterfaceName -Value $InterfaceName

    return $service
}

<#
    .SYNOPSIS
        Prepares the current powershell session to call cmNavigo WCF services using LBOs

    .DESCRIPTION
        Configures and loads the app.config file and Light Business Objects (LBOs) assembly to allow calling cmNavigo WCF services.

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .EXAMPLE
        Use-LightBusinessObjects $env

    .NOTES
        The LBOs assembly loaded should be placed on the References folder, with the standard name
        (Cmf.Proxy.LightBusinessObjects.Assembly.dll).
        All service calls are synchronous.
#>
function Use-LightBusinessObjects()
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env
    )

    $config_path = (Resolve-Path $ModulePath'\References\MasterData.Exe.config').Path

    sp $config_path IsReadOnly $false

    # Adjust endpoints
    Add-Type -AssemblyName ('System.configuration, version=4.0.0.0, '+ 'Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a')#System.Configuration

    [Configuration.ConfigurationManager].GetField("s_initState", "NonPublic, Static").SetValue($null, 0)
    [Configuration.ConfigurationManager].GetField("s_configSystem", "NonPublic, Static").SetValue($null, $null)
    ([Configuration.ConfigurationManager].Assembly.GetTypes() | where {$_.FullName -eq "System.Configuration.ClientConfigPaths"})[0].GetField("s_current", "NonPublic, Static").SetValue($null, $null)

    [System.AppDomain]::CurrentDomain.SetData("APP_CONFIG_FILE", $null)
    [System.AppDomain]::CurrentDomain.SetData("APP_CONFIG_FILE", $config_path)
    #endregion

    # Load LBOs assembly
	$candidateAssembly =  (Resolve-Path "$ModulePath\References\Cmf.LightBusinessObjects.dll")

	# Load your target version of the assembly
	[System.Reflection.Assembly]::LoadFrom((Resolve-Path "$ModulePath\References\Cmf.LoadBalancing.dll")) | Out-Null
    [System.Reflection.Assembly]::LoadFrom((Resolve-Path "$ModulePath\References\Cmf.MessageBus.Client.dll")) | Out-Null
    if ( Test-Path ("$ModulePath\References\System.Net.Http.dll") ) {
		[System.Reflection.Assembly]::LoadFrom((Resolve-Path "$ModulePath\References\System.Net.Http.dll")) | Out-Null
	}

	# Method to intercept resolution of binaries
	$onAssemblyResolveEventHandler = [System.ResolveEventHandler] {
		param($sender, $e)

		# Write-Host "ResolveEventHandler: Attempting FullName resolution of $($e.Name)"
		foreach($assembly in [System.AppDomain]::CurrentDomain.GetAssemblies()) {
			if ($assembly.FullName -eq $e.Name) {
				# Write-Host "Successful FullName resolution of $($e.Name)"
				return $assembly
			}
		}

		# Write-Host "ResolveEventHandler: Attempting name-only resolution of $($e.Name)"
		foreach($assembly in [System.AppDomain]::CurrentDomain.GetAssemblies()) {
			# Get just the name from the FullName (no version)
			$assemblyName = $assembly.FullName.Substring(0, $assembly.FullName.IndexOf(", "))

			if ($e.Name.StartsWith($($assemblyName + ","))) {

				# Write-Host "Successful name-only (no version) resolution of $assemblyName"
				return $assembly
			}
		}

		# Write-Host "Unable to resolve $($e.Name)"
		return $null
	}

	# Wire-up event handler
	[System.AppDomain]::CurrentDomain.add_AssemblyResolve($onAssemblyResolveEventHandler)

	# Load into app domain
	$assembly = [System.Reflection.Assembly]::LoadFrom($candidateAssembly) | Out-Null
}

<#
    .SYNOPSIS
        Installs customization assemblies into the business tier directory of application servers.

    .DESCRIPTION
        Copies customization assemblies to the business tier folder on one or all application servers.
        All copied files will be overwritten if already existing.

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER filesPath
        Absolute Path to the items to copy. Should be a string ending in \* to copy all files in a given directory or
        \*.dll to copy only assemblies.

    .PARAMETER applicationServer
        If defined, files will only be copied to the specified application server.

    .EXAMPLE
        Install-CMFBusinessAssemblies $env -filesPath .\Business\*

    .NOTES
        Application hosts should be stopped for the copy to succeed.
        Requires administrative privileges on the application servers.
#>
function Install-CMFBusinessAssemblies()
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # Path of the business assemblies to install
        [Parameter(Mandatory=$True)]
        [string]$filesPath,

        # Application Server
        [Parameter(Mandatory=$False)]
        [PSObject]$applicationServer
    )

    # Get servers to use
    $serversToUse = @()
    if ($applicationServer)
    {
        $serversToUse += $applicationServer
    }
    else
    {
        $serversToUse = $env.ApplicationServers
    }

    LogWrite (" >  Starting installation of business assemblies...")

    $source = $filesPath
    foreach($appServer in $serversToUse)
    {
        LogWrite ("    Copying assemblies to " + $appServer.ServerName + " ...")
        $target = '\\' + $appServer.ServerName + '\' + $appServer.BusinessInstallationPath -replace ':', '$'

        try
        {
            Copy-Item $source $target -recurse -force
        }
        catch [Exception]
        {
            # locked files?
            $randomDir = [System.IO.Path]::GetRandomFileName();
            $currentDate = (Get-Date).AddDays(-1).ToString('yyyy-MM-dd')
            $lockedPath = Join-Path -Path (Split-Path -parent $target) -ChildPath ("\Backups\Locked-$currentDate-$randomDir")

            LogWrite ("Error Copying "+ $_.Exception.Message) -ForeGroundColor Red
            if($global:InteractiveMode -eq "" -Or $global:InteractiveMode -eq $true){
                Write-Host $_.Exception -ForegroundColor Red|format-list -force
                }

            LogWrite "Creating locked folder $lockedPath" -ForeGroundColor Yellow
            mkdir $lockedPath |out-null
            gci -path $source -filter *.dll | ForEach-Object {
                    $fi =  join-path  $target $_.Name;
                    if (Test-Path $fi ){
                        move -path $fi -destination $lockedPath
                    }
            }
            LogWrite "Trying to copy again... " -ForeGroundColor Yellow
            Copy-Item $source $target -recurse -force
        }

    }

    LogWrite (" *  Completed installation of business assemblies!") -foregroundColor Green
}

<#
    .SYNOPSIS


<#
    .SYNOPSIS
        Copies files to a CMF Folder [on testings]

    .DESCRIPTION
        Copies files to a CMF Folder on one or all application servers.
        All copied files will be overwritten if already existing.

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER folder
        Destination folder inside the CMF environment.

    .PARAMETER filesPath
        Absolute Path to the items to copy. Should be a string ending in \* to copy all files in a given directory or
        \*.dll to copy only assemblies.

    .PARAMETER applicationServer
        If defined, files will only be copied to the specified application server.

    .EXAMPLE
        Publish-CMFFolder $env -folder BusinessTier -filesPath .\Business\*

    .NOTES
        Application hosts should be stopped for the copy to succeed.
        Requires administrative privileges on the application servers.
#>
function Publish-CMFFolder()
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # Folder
        [Parameter(Mandatory=$True)]
        [string] $folder,

        # Path of the business assemblies to install
        [Parameter(Mandatory=$True)]
        [string]$filesPath,

        # Application Server
        [Parameter(Mandatory=$False)]
        [PSObject]$applicationServer
    )

    # Get servers to use
    $serversToUse = @()
    if ($applicationServer)
    {
        $serversToUse += $applicationServer
    }
    else
    {
        $serversToUse = $env.ApplicationServers
    }

    LogWrite (" >  Copying files to $folder...")

    $source = $filesPath
    foreach($appServer in $serversToUse)
    {
        LogWrite ("    Copying files to " + $appServer.ServerName + " ...")
        $target = '\\' + $appServer.ServerName + '\' + ($appServer.InstallationPath -replace ':', '$')
        $target = (Join-Path -Path $target -ChildPath ("\" + $folder))

        try
        {
            Copy-Item $source $target -recurse -force
        }
        catch [Exception]
        {
            # locked fuiles?
            $randomDir = [System.IO.Path]::GetRandomFileName();
            $currentDate = (Get-Date).AddDays(-1).ToString('yyyy-MM-dd')
            $lockedPath = Join-Path -Path (Split-Path -parent $target) -ChildPath ("\Backups\Locked-$currentDate-$randomDir")

            LogWrite ("Error Copying "+ $_.Exception.Message) -ForeGroundColor Red
            if($global:InteractiveMode -eq "" -Or $global:InteractiveMode -eq $true){
                Write-Host $_.Exception -ForegroundColor Red|format-list -force
                }

            LogWrite "Creating locked folder $lockedPath" -ForeGroundColor Yellow
            mkdir $lockedPath |out-nll
            gci -path $source -filter *.dll | ForEach-Object {
                    $fi =  join-path  $target $_.Name;
                    if (Test-Path $fi ){
                        move -path $fi -destination $lockedPath
                    }
            }
            LogWrite "Trying to copy again... " -ForeGroundColor Yellow
            Copy-Item $source $target -recurse -force
        }

    }

    LogWrite (" *  Completed copying files to $folder!") -foregroundColor Green
}

<#
    .SYNOPSIS
    Installs customization assemblies and addins into the presentation tier directory of application servers.

    .DESCRIPTION
        Copies customization assemblies and addin files to the presentation tier folder on one or all application servers.
        All copied files will be overwritten if already existing.
        All files are copied to the ClientBin subfolder.

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER filesPath
        Absolute Path to the items to copy. Should be a string ending in \* to copy all files in a given directory or
        \*.dll to copy only assemblies.

    .PARAMETER applicationServer
        If defined, files will only be copied to the specified application server.

    .PARAMETER copyToWebRoot
        If defined, copy to web root folder (GUIInstallationPath) instead of ClientBin

    .EXAMPLE
        Install-CMFPresentationAssemblies $env -filesPath .\Presentation\*

    .NOTES
        Requires administrative privileges on the application servers.
#>
function Install-CMFPresentationAssemblies()
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # Path of the presentation assemblies to install
        [Parameter(Mandatory=$True)]
        [string]$filesPath,

        # Application Server
        [Parameter(Mandatory=$False)]
        [PSObject]$applicationServer,

        # switch to copy to webroot
        [Parameter(Mandatory=$False)]
        [switch] $copyToWebRoot

    )

    # Get servers to use
    $serversToUse = @()
    if ($applicationServer)
    {
        $serversToUse += $applicationServer
    }
    else
    {
        $serversToUse = $env.ApplicationServers
    }

   LogWrite (" >  Starting installation of presentation assemblies...")

    $source = $filesPath
    foreach($appServer in $serversToUse)
    {
        LogWrite ("    Copying assemblies to " + $appServer.ServerName + " ...")

        if( $copyToWebRoot)
        {
            $target = '\\' + $appServer.ServerName + '\' + $appServer.GUIInstallationPath  -replace ':', '$'
        }
        else
        {
            $target = '\\' + $appServer.ServerName + '\' + $appServer.GUIInstallationPath + '\ClientBin' -replace ':', '$'
        }

        Copy-Item $source $target -recurse -force
    }
    LogWrite ' *  Completed installation of presentation assemblies!' -foregroundColor Green
}

<#
    .SYNOPSIS
        Register new addin extensions on the ApplicationRoster.xml file

    .DESCRIPTION
        Register a specified addin on the ApplicationRoster.xml file on one or all application servers.

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER addinsToRegister
        Array containing the names of all addins to be registered.

    .PARAMETER applicationServer
        If defined, addins will only be registered on the specified application server.

    .EXAMPLE
        Register-CMFAddins $env -addinsToRegister @('MyCustomGUIS')

    .NOTES
        If the addins are already registed, they won't be added again.
        Requires administrative privileges on the application servers.
#>
function Register-CMFAddins()
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # List of addins to register
        [Parameter(Mandatory=$True)]
        [string[]]$addinsToRegister,

        # Application Server
        [Parameter(Mandatory=$False)]
        [PSObject]$applicationServer

    )

    # Get servers to use
    $serversToUse = @()
    if ($applicationServer)
    {
        $serversToUse += $applicationServer
    }
    else
    {
        $serversToUse = $env.ApplicationServers
    }

    $password = Get-SecureStringFromEncryptedString $env.AdminPass
    $cred = new-object -typename System.Management.Automation.PSCredential -argumentlist $env.AdminUser, $password

    LogWrite(' >  Registering addins in application servers...')
    # Prepare script to run on each app server
    $sc =
    {
        param($appServer, $addinsToRegister)

        # get xml content
        $applicationRosterPath = $appServer.GUIInstallationPath + '\ApplicationRoster.xml'
        [xml] $appRosterXml = Get-Content $applicationRosterPath

        # register each addin
        foreach($addin in $addinsToRegister)
        {
            $node = $appRosterXml.App.Addins.Addin | where {$_.name -eq $addin}
            if ($node -eq $null)
            {
                # node does not exist, we need to add it
                $node = $appRosterXml.App.Addins.Addin[0].Clone()
                $node.name = $addin
                $node.source = $addin + '.addin'
                $node = $appRosterXml.App.Addins.AppendChild($node)
                if($global:InteractiveMode -eq "" -Or $global:InteractiveMode -eq $true){
                    Write-Host '    Registering addin' + $addin + 'on app server ' + $appserver.ServerName + '...'
                }
            }
            else
            {
                if($global:InteractiveMode -eq "" -Or $global:InteractiveMode -eq $true){
                    Write-Host '    Addin was already registered on app server ' + $appserver.ServerName -foregroundColor Yellow
                }
            }
        }

        $appRosterXml.Save($applicationRosterPath)
    }

    # Execute script on each app server
    foreach($appServer in $serversToUse)
    {
        Invoke-Command -ComputerName $appServer.ServerName -ScriptBlock $sc -ArgumentList $appServer,$addinsToRegister  -Credential $cred
        LogWrite ("    Addins inserted/updated in server " +  $appserver.ServerName)
    }
    LogWrite ' *  Addins registered!' -foregroundColor Green
}

<#
    .SYNOPSIS
        Stop cmNavigo application hosts.

    .DESCRIPTION
        Stops cmNavigo application hosts on one or all application servers and manages their inclusion on the NLB cluster

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER applicationServer
        If defined, addins will only be registered on the specified application server.

    .PARAMETER disableNLBManagement
        If set, hosts will not be removed from the NLB cluster. To be used when third-party NLB solutions are in use.

    .EXAMPLE
        Stop-NavigoHosts $env

    .EXAMPLE
        Stop-NavigoHosts $env -applicationServer $env.ApplicationServers[0] -disableNLBManagement

    .NOTES
        Host will only be removed from the NLB cluster if there is more than one application server registered on the $env object
        passed as argument. Connections to the host are drained for a maximum of 2 minutes before the host is stopped and removed
        from the NLB cluster (drainstop).
        Requires administrative privileges on the application servers.
#>
function Stop-NavigoHosts()
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # Application Server
        [Parameter(Mandatory=$False)]
        [PSObject]$applicationServer,

        # Disable manage NLBs
        [Parameter(Mandatory=$False)]
        [switch]$disableNLBManagement
    )

    # Get servers to use
    $serversToUse = @()
    if ($applicationServer)
    {
        $serversToUse += $applicationServer
    }
    else
    {
        $serversToUse = $env.ApplicationServers
    }

    $password = Get-SecureStringFromEncryptedString $env.AdminPass
    $cred = new-object -typename System.Management.Automation.PSCredential -argumentlist $env.AdminUser, $password

    LogWrite ' >  Stopping application hosts...'
    foreach($appServer in $serversToUse)
    {
        if (-not ($disableNLBManagement))
        {
            # before stopping, remove host from NLB, if NLB is in use
            if ($env.ApplicationServers.Length -gt 1)
            {
               LogWrite ("    Removing host " + $appServer.ServerName + " from NLB...")
               Invoke-Command -ComputerName $appServer.ServerName  -Credential $cred -ScriptBlock { Stop-NlbClusterNode -Drain -Timeout 2 }
            }
        }

        # stop service
        # Create a new session remotely to wait for process
        $sess = New-PSSession -ComputerName $appServer.ServerName -Credential $cred
        Enter-PSSession -Session $sess

        try
        {
            # request stop service
            $service = Get-Service -computername $appServer.ServerName -Name $env.NavigoServiceName
            if ($service.Status -eq 'Stopped')
            {
                 LogWrite ("    Service was already stopped on " + $appServer.ServerName) -foregroundColor Yellow
            }
            else
            {
                 LogWrite ("    Stopping MES host on " + $appServer.ServerName + "...")

                $service.Stop()

                $service.WaitForStatus('Stopped','00:01:00')

                if ($service.Status -ne 'Stopped')
                {
                    throw 'Service termination timed out!'
                }
            }

            $processId = gwmi Win32_Service -Filter ("Name LIKE '" + $env.NavigoServiceName + "'")  | select -expand ProcessId

            if ($processId -and $processId  -ne "0")
            {
                # Wait 10 seconds to process die, if not process continue but notification is done
                $retries = 11

                $activityDescription = "Killing process"
                Write-Progress -Activity ($activityDescription) -Status "Please wait..."
                $processKilled = $False
                for($i=1; $i -le $retries ; $i++)
                {
                    $processActive = Get-Process -Id $processId -ErrorAction SilentlyContinue
                    if($processActive -eq $null)
                    {
                        $processKilled = $True
                        break
                    }
                    else
                    {
                         kill $processId -Force
                    }

                    SLEEP 1
                    Write-Progress -Activity ($activityDescription) -CurrentOperation ("Tentative n " + $i ) -Status ("Please wait...")
                }

                if($processKilled -eq $True)
                {
                    Write-Progress -Activity $activityDescription -Completed -Status "Process was killed with success."
                }
                else
                {
                    Write-Progress -Activity $activityDescription -Completed -Status "Killing process failed."
                    LogWrite ("    Killing process failed") -ForeGroundColor Yellow
                }
            }
            else
            {
                #hack sometimes process id is not found in the process.
                #because of that we force a sleep.

                $totalTimeToWait = 5
                LogWrite ("    No process id found. Wait " + $totalTimeToWait.ToString() + " seconds to process end")

                $processStopDescription = "Wait " + $totalTimeToWait.ToString() + " seconds to process stop"
                Write-Progress -Activity ($processStopDescription) -Status "Please wait..."
                for($i=0; $i -le $totalTimeToWait ; $i++)
                {
                    SLEEP 1
                    Write-Progress -Activity ($processStopDescription) -CurrentOperation (($i + 1).ToString() + " of " + $totalTimeToWait.ToString()) -Status ("Please wait...")
                }

                Write-Progress -Activity $processStopDescription -Completed -Status "Sleep time for Process stop ended."
                LogWrite ("    End of wait till the process end")

            }
        }
        finally
        {
            Remove-PSSession -Session $sess
            Exit-PSSession
        }
    }
    LogWrite ' *  Application services stopped!' -foregroundColor Green
}

<#
    .SYNOPSIS
        Start cmNavigo application hosts.

    .DESCRIPTION
        Starts cmNavigo application hosts on one or all application servers and manages their inclusion on the NLB cluster

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER applicationServer
        If defined, addins will only be registered on the specified application server.

    .PARAMETER disableNLBManagement
        If set, hosts will not be added to the NLB cluster. To be used when third-party NLB solutions are in use.

    .EXAMPLE
        Start-NavigoHosts $env

    .EXAMPLE
        Start-NavigoHosts $env -applicationServer $env.ApplicationServers[0] -disableNLBManagement

    .NOTES
        Host will only be added to the NLB cluster if there is more than one application server registered on the $env object
        passed as argument.
        Requires administrative privileges on the application servers.
#>
function Start-NavigoHosts()
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # Application Server
        [Parameter(Mandatory=$False)]
        [PSObject]$applicationServer,

        # Disable manage NLBs
        [Parameter(Mandatory=$False)]
        [switch]$disableNLBManagement
    )

    # Get servers to use
    $serversToUse = @()
    if ($applicationServer)
    {
        $serversToUse += $applicationServer
    }
    else
    {
        $serversToUse = $env.ApplicationServers
    }

    LogWrite ' >  Starting application hosts...'
    foreach($appServer in $env.ApplicationServers)
    {
        $service = Get-Service -computername $appServer.ServerName -Name $env.NavigoServiceName
        if ($service.Status -eq 'Running')
        {
            LogWrite ("    Service is already started on " + $appServer.ServerName) -foregroundColor Yellow
        }
        else
        {
            LogWrite ("    Starting MES host on " + $appServer.ServerName + " ...")
            $service.Start()
            $service.WaitForStatus('Running', '00:02:00')
        }

        if (-not ($disableNLBManagement))
        {
            # after starting, start host in NLB, if NLB is in use
            if ($env.ApplicationServers.Length -gt 1)
            {
                Invoke-Command -ComputerName $appServer.ServerName -ScriptBlock  { Start-NlbClusterNode }
            }
        }
    }
    LogWrite ' *  Application services started!' -foregroundColor Green
}


<#
    .SYNOPSIS
        Stop a specific service on all application services.

    .DESCRIPTION
        Stops any service on one or all application servers

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER applicationServer
        If defined, addins will only be registered on the specified application server.

    .PARAMETER serviceName
        Service name to be stopped

    .EXAMPLE
        Stop-SpecificService $env -serviceName $env.NavigoRemoteImportExportGatewayServiceName

    .EXAMPLE
        Stop-SpecificService $env -applicationServer $env.ApplicationServers[0] -serviceName $env.NavigoRemoteImportExportGatewayServiceName

    .NOTES
        Requires administrative privileges on the application servers.
#>
function Stop-SpecificService()
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # Application Server
        [Parameter(Mandatory=$False)]
        [PSObject]$applicationServer,

        #Service Name
        [Parameter(Mandatory=$True)]
        [string]$serviceName
    )

    # Get servers to use
    $serversToUse = @()
    if ($applicationServer)
    {
        $serversToUse += $applicationServer
    }
    else
    {
        $serversToUse = $env.ApplicationServers
    }


    LogWrite " >  Stopping service $serviceName..."
    foreach($appServer in $serversToUse)
    {
        # stop service
        # Create a new session remotely to wait for process
        $password = Get-SecureStringFromEncryptedString $env.AdminPass
        $cred = new-object -typename System.Management.Automation.PSCredential -argumentlist $env.AdminUser, $password

        $sess = New-PSSession -ComputerName $appServer.ServerName -Credential $cred
        Enter-PSSession -Session $sess

        try
        {
            # request stop service
            $service = Get-Service -computername $appServer.ServerName -Name $serviceName
            if ($service)
            {
                if ($service.Status -eq 'Stopped')
                {
                    LogWrite ("    Service was already stopped on " + $appServer.ServerName) -foregroundColor Yellow
                }
                else
                {
                    LogWrite ("    Stopping service on " + $appServer.ServerName + "...")

                    $service.Stop()

                    $service.WaitForStatus('Stopped','00:01:00')

                    if ($service.Status -ne 'Stopped')
                    {
                        throw 'Service termination timed out!'
                    }
                }
            }

            $processId = gwmi Win32_Service -Filter ("Name LIKE '" + $service + "'")  | select -expand ProcessId

            if ($processId -and $processId  -ne "0")
            {
                # Wait 10 seconds to process die, if not process continue but notification is done
                $retries = 11

                $activityDescription = "Killing process"
                Write-Progress -Activity ($activityDescription) -Status "Please wait..."
                $processKilled = $False
                for($i=1; $i -le $retries ; $i++)
                {
                    $processActive = Get-Process -Id $processId -ErrorAction SilentlyContinue
                    if($processActive -eq $null)
                    {
                        $processKilled = $True
                        break
                    }
                    else
                    {
                         kill $processId -Force
                    }

                    SLEEP 1
                    Write-Progress -Activity ($activityDescription) -CurrentOperation ("Tentative n� " + $i ) -Status ("Please wait...")
                }

                if($processKilled -eq $True)
                {
                    Write-Progress -Activity $activityDescription -Completed -Status "Process was killed with success."
                }
                else
                {
                    Write-Progress -Activity $activityDescription -Completed -Status "Killing process failed."
                    LogWrite ("    Killing process failed") -ForeGroundColor Yellow
                }
            }
            else
            {
                #hack sometimes process id is not found in the process.
                #because of that we force a sleep.

                $totalTimeToWait = 5
                LogWrite ("    No process id found. Wait " + $totalTimeToWait.ToString() + " seconds to process end")

                $processStopDescription = "Wait " + $totalTimeToWait.ToString() + " seconds to process stop"
                Write-Progress -Activity ($processStopDescription) -Status "Please wait..."
                for($i=0; $i -le $totalTimeToWait ; $i++)
                {
                    SLEEP 1
                    Write-Progress -Activity ($processStopDescription) -CurrentOperation (($i + 1).ToString() + " of " + $totalTimeToWait.ToString()) -Status ("Please wait...")
                }

                Write-Progress -Activity $processStopDescription -Completed -Status "Sleep time for Process stop ended."
                LogWrite ("    End of wait till the process end")

            }
        }
        finally
        {
            Remove-PSSession -Session $sess
            Exit-PSSession
        }
    }
    LogWrite " *  Service $serviceName stopped!" -foregroundColor Green
}

<#
    .SYNOPSIS
        Start a specific service on all application services.

    .DESCRIPTION
        Starts any service on one or all application servers

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER applicationServer
        If defined, addins will only be registered on the specified application server.

    .PARAMETER serviceName
        Service name to be started

    .EXAMPLE
        Start-SpecificService $env -serviceName $env.NavigoRemoteImportExportGatewayServiceName

    .EXAMPLE
        Start-SpecificService $env -applicationServer $env.ApplicationServers[0] -serviceName $env.NavigoRemoteImportExportGatewayServiceName

    .NOTES
        Requires administrative privileges on the application servers.
#>
function Start-SpecificService()
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # Application Server
        [Parameter(Mandatory=$False)]
        [PSObject]$applicationServer,

        #Service Name
        [Parameter(Mandatory=$True)]
        [string]$serviceName
    )

    # Get servers to use
    $serversToUse = @()
    if ($applicationServer)
    {
        $serversToUse += $applicationServer
    }
    else
    {
        $serversToUse = $env.ApplicationServers
    }

    LogWrite " >  Starting service $serviceName..."
    foreach($appServer in $env.ApplicationServers)
    {
        $service = Get-Service -computername $appServer.ServerName -Name $serviceName
        if ($service)
        {
            if ($service.Status -eq 'Running')
            {
                LogWrite ("    Service is already started on " + $appServer.ServerName) -foregroundColor Yellow
            }
            else
            {
                LogWrite ("    Starting service on " + $appServer.ServerName + " ...")
                $service.Start()
                $service.WaitForStatus('Running', '00:02:00')
            }
        }
    }
    LogWrite " *  Service $serviceName started!" -foregroundColor Green
}

<#
    .SYNOPSIS
        Installs cmNavigo custom services

    .DESCRIPTION
        Install one or more cmNavigo custom services. If already existing, the services are removed and re-added.

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER services
        Array containing the services to be added. Each service should be a custom object created with the New-CMFService
        function.
        Each individual interface must be registered as a different service.

    .PARAMETER assembliesPath
        Absolute path to the directory containing the specified service assemblies.

    .EXAMPLE
        Invoke-SQLScript $env -database 'Online' -scriptPath '.\Analytics\Release 015.1.sql' -username 'CMFUser' -password 'CMFUser'

    .NOTES
        Assumes SQL Script file is saved with UTF8 encoding.
        GO batch separators are supported, assuming they are placed in separate lines.
#>
function Install-CMFCustomServices()
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # Services to Install
        [Parameter(Mandatory=$True)]
        [Object[]]$services,

        [Parameter(Mandatory=$True)]
        [string] $assembliesPath
    )

    LogWrite " >  Starting deploy of custom services to database..."

    Use-LightBusinessObjects $env

    #region Try to remove the service if it already exists
    try
    {
        [Cmf.Foundation.BusinessOrchestration.WCFServiceManagement.InputObjects.GetAllServicesInput] $getServiceInput = New-Object Cmf.Foundation.BusinessOrchestration.WCFServiceManagement.InputObjects.GetAllServicesInput
        $getServiceInput.Filter = New-Object Cmf.Foundation.BusinessObjects.QueryObject.FilterCollection
        [Cmf.Foundation.BusinessObjects.ServiceIntegrator.ServiceContractCollection] $existingServices = $getServiceInput.GetAllServicesSync().Services
        #Write-Host $existingServices.Count
        foreach($service in $services)
        {
            $existingService = $existingServices | Where-Object { $service.ServiceName -eq $_.Name }# | Select -First 1
            if($existingService)
            {
                LogWrite ("    Service " + $service.InterfaceName + " already exists and will be removed...") -foregroundColor Yellow

                [Cmf.Foundation.BusinessOrchestration.WCFServiceManagement.InputObjects.RemoveServiceInput] $removeServiceInput = New-Object Cmf.Foundation.BusinessOrchestration.WCFServiceManagement.InputObjects.RemoveServiceInput
                $removeServiceInput.Service = $existingService
                $removeServiceInput.RemoveServiceSync() | Out-Null

            }


        }
    }
    catch [Exception]
    {
        LogWrite $_.Exception.Message
    }
    #endregion

    foreach($service in $services)
    {
        #region Read assembly and compute checksum
        LogWrite ("    Adding service " + $service.InterfaceName + " ...")

        $sha = New-Object System.Security.Cryptography.SHA256CryptoServiceProvider
        $assemblyPath = Join-Path $assembliesPath $service.AssemblyName
        [byte[]] $assemblyBytes = [System.IO.File]::ReadAllBytes($assemblyPath)
        [byte[]] $checksum = $sha.ComputeHash($assemblyBytes)

        [Cmf.Foundation.BusinessOrchestration.WCFServiceManagement.InputObjects.GetExportedTypesForAssemblyInput] $getTypesInput = New-Object Cmf.Foundation.BusinessOrchestration.WCFServiceManagement.InputObjects.GetExportedTypesForAssemblyInput
        $getTypesInput.Assembly = New-Object Cmf.Foundation.BusinessObjects.ServiceIntegrator.ServiceAssembly
        $getTypesInput.Assembly.Assembly = $assemblyBytes
        $fullAssemblyName = $getTypesInput.GetExportedTypesForAssemblySync().FullAssemblyName
        #endregion

        #region Prepare Service Contract
        [Cmf.Foundation.BusinessObjects.ServiceIntegrator.ServiceContract] $serviceContract = New-Object Cmf.Foundation.BusinessObjects.ServiceIntegrator.ServiceContract
        $serviceContract.Name = $service.ServiceName
        $serviceContract.IsSystem = $false
        $serviceContract.Active = $true
        $serviceContract.Description = $service.ServiceName
        $serviceContract.ServiceInterface = $service.InterfaceName
        $serviceContract.ServiceAssembly = New-Object Cmf.Foundation.BusinessObjects.ServiceIntegrator.ServiceAssembly
        $serviceContract.ServiceAssembly.IsSystem = $false
        $serviceContract.ServiceAssembly.Name = $fullAssemblyName
        $serviceContract.ServiceAssembly.Assembly = $assemblyBytes
        $serviceContract.ServiceAssembly.Checksum = $checksum
        #endregion

        # Call Service
        [Cmf.Foundation.BusinessOrchestration.WCFServiceManagement.InputObjects.CreateServiceInput] $createServiceInput = New-Object Cmf.Foundation.BusinessOrchestration.WCFServiceManagement.InputObjects.CreateServiceInput
        $createServiceInput.Service = $serviceContract
        $return = $createServiceInput.CreateServiceSync()
    }

    LogWrite " *  Finished deploy of custom services to database!" -foregroundColor Green
}

<#
    .SYNOPSIS
        Generates LBOs

    .DESCRIPTION
        Generate LBOs and update XAP files on one or all application servers. Assumes LBO generator is correctly configured on
        each application server.

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER applicationServer
        If defined, LBOs will be generated only in the specified application server.

    .EXAMPLE
        Update-LightBusinessObjects $env

    .NOTES
        Requires admininstrative permissions on the application servers.
#>
function Update-LightBusinessObjects()
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # Application Server
        [Parameter(Mandatory=$False)]
        [PSObject]$applicationServer,

        # Application Server
        [Parameter(Mandatory=$False)]
        [PSObject]$isToGenerateWCF = $false

    )

    # Get servers to use
    $serversToUse = @()
    if ($applicationServer)
    {
        $serversToUse += $applicationServer
    }
    else
    {
        $serversToUse = $env.ApplicationServers
    }

    LogWrite ' >  Generating LBOs in application servers...'


    $certPassword = $null

    $password = Get-SecureStringFromEncryptedString $env.AdminPass
    $cred = new-object -typename System.Management.Automation.PSCredential -argumentlist $env.AdminUser, $password

    if( $env.ReadPasswordToSignGeneratedLBOs -eq $true )
    {
        $secureCertPassword = Read-Host "Enter  certificate password:" -AsSecureString
        $psCred = New-Object System.Management.Automation.PSCredential ("dummyUser", $secureCertPassword)
        $certPassword = $psCred.GetNetworkCredential().Password;
    }


    # Prepare script to runon each app server
    $sc =
    {
        param($XAPUpdaterPath, $certPassword)

        if( -not($certPassword -eq $null))
        {
            $tmpFile =  [System.IO.Path]::GetTempFileName()+'.cmd'

            try
            {
                Set-Content  $tmpFile "echo $certPassword | ""$XAPUpdaterPath\GenerateLBOsAndUpdateXapFile.bat"""

                Start-Process $tmpFile -Wait -WorkingDirectory $XAPUpdaterPath  | Out-Host
            }
            finally
            {
                if( Test-Path $tmpFile ){
                    Set-Content  $tmpFile ' '  # clear the contents
                    Remove-Item $tmpFile
                }
            }
        }
        else
        {
            Set-Location -Path $XAPUpdaterPath
            .\GenerateLBOsAndUpdateXapFile.bat
        }
    }
     # Prepare script to runon each app server
    $puhfc =
    {
        param($protectUnprotectConfigFilePath, $mode, $businessInstallationPath)

        $arguments = "/Mode:$mode /InstallPath:""$businessInstallationPath"""

        Write-Host "Deal with host config file"
        Set-Location -Path $protectUnprotectConfigFilePath
        Start-Process Cmf.Tools.ProtectUnprotectConfigFile.exe $arguments -Wait
    }

    # Prepare script to runon each app server
    $restlboc =
    {
        param($lboGeneratorPath)

        Set-Location -Path $lboGeneratorPath
		.\LBOUpdater.ps1        
    }

    # Execute batch script on each app server
    foreach($appServer in $serversToUse)
    {
        LogWrite( '    Starting LBO Generation on '+ $appserver.ServerName +' ...')

        if($isToGenerateWCF)
        {
            #WCF LBOs
            Invoke-Command -ScriptBlock $sc -ArgumentList @($appServer.XAPUpdaterPath, $certPassword) -ComputerName $appServer.ServerName -Credential $cred
        }      

        #Unprotect HostConfigFile
        Invoke-Command -ScriptBlock $puhfc -ArgumentList @($appServer.ProtectUnprotectConfigFilePath, 2, $appServer.BusinessInstallationPath) -ComputerName $appServer.ServerName -Credential $cred

        #REST LBOs
        Invoke-Command -ScriptBlock $restlboc -ArgumentList @($appServer.LBOGeneratorPath) -ComputerName $appServer.ServerName -Credential $cred

        #Protect HostConfigFile
        Invoke-Command -ScriptBlock $puhfc -ArgumentList @($appServer.ProtectUnprotectConfigFilePath, 1, $appServer.BusinessInstallationPath) -ComputerName $appServer.ServerName -Credential $cred

        LogWrite ('    Finished LBO Generation on ' +$appserver.ServerName +' ...')
    }

    LogWrite ' *  LBOs generated!' -foregroundColor Green
}

<#
    .SYNOPSIS
        Gets the Available Types of a given Master Data file

    .DESCRIPTION
        Uses the master data loader application logic to get the available types of the master data file.
        This function uses the same logic as the standard Master Data Loader in Navigo.

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER excelFile
        Absolute to the Excel file to be loaded.

    .PARAMETER TypesToExclude
        String array containing the types to be excluded.
        If TypesToLoad is defined, this parameter is ignored.

    .EXAMPLE
        Get-MasterDataAvailableTypes $env -excelFile 'C:\MasterData.xlsx'
		
    .EXAMPLE
        Get-MasterDataAvailableTypes $env -excelFile 'C:\MasterData.xlsx' -typesToExclude @('Document')

    .NOTES
        Master data logic uses LBOs to load data into cmNavigo. The LBOs assenmbly used is located on the References folder.
#>
function Get-MasterDataAvailableTypes()
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        #Excel file
        [Parameter(Mandatory=$True)]
        [string] $excelFile,
		
        #Exclude Types
        [Parameter(Mandatory=$False)]
        [string[]] $TypesToExclude
    )

    Use-LightBusinessObjects $env

    # Prepare config arguments
    # Load master data logic assembly
    [Reflection.Assembly]::LoadFrom((Resolve-Path "$ModulePath\References\MasterData.Logic.dll")) | Out-Null

    $configArguments = [MasterData.Logic.ConfigArguments]::Singleton
    $configArguments.UploadedFile = $excelFile

    # Get available types
    [MasterData.Logic.Runner]::GetAvailableTypes()
	
    # Prepare types to load
	$TypesToLoad = @()
	foreach($typeToLoad in $configArguments.AvailableTypes)
	{
		if (-not ($TypesToExclude -ne $null -and $TypesToExclude.Contains($typeToLoad.UserObjectType)))
		{
			$TypesToLoad += $typeToLoad.UserObjectType
		}
	}

    return $TypesToLoad
}

<#
    .SYNOPSIS
        Load Master Data file

    .DESCRIPTION
        Uses the master data loader application logic to load a master data file into cmNavigo.
        This function uses the same logic as the standard Master Data Loader in Navigo.

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER excelFile
        Absolute to the Excel file to be loaded.

    .PARAMETER typesToLoad
        String array containing the types to be loaded.
        If not defined, all types on the excel file will be loaded.

    .PARAMETER deeBasePath
        Absolute path to the folder containing the code files for DEE rules to be imported.
        Required if DEE rules are to be imported.

    .PARAMETER documentsBasePath
        Absolute path to the folder containint the documents to be uploaded.
        Required if Documents are to be imported.

    .PARAMETER createInCollection
        Set this flag if objects are to be loaded in collection. If not set, objects are loaded individually.

    .PARAMETER TypesToExclude
        String array containing the types to be excluded.
        If TypesToLoad is defined, this parameter is ignored.

    .EXAMPLE
        Invoke-MasterDataLoader $env -excelFile 'C:\MasterData.xlsx'

    .EXAMPLE
        Invoke-MasterDataLoader $env -excelFile 'C:\MasterData.xlsx' -typesToLoad @('Parameter', 'DataCollection')

    .EXAMPLE
        Invoke-MasterDataLoader $env -excelFile 'C:\MasterData.xlsx' -typesToLoad @('DEEAction') -deeBasePath 'C:\DEE'

    .EXAMPLE
        Invoke-MasterDataLoader $env -excelFile 'C:\MasterData.xlsx' -typesToLoad @('Document') -documentsBasePath 'C:\Documents'

    .EXAMPLE
        Invoke-MasterDataLoader $env -excelFile 'C:\MasterData.xlsx' -deeBasePath 'C:\DEE' -createInCollection

    .NOTES
        Master data logic uses LBOs to load data into cmNavigo. The LBOs assenmbly used is located on the References folder.
        Master data loading errors are shown on the console but not actually treated as execution errors or exceptions,
        since they are a common occurence in deployment procedures.
#>
function Invoke-MasterDataLoader()
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        #Excel file
        [Parameter(Mandatory=$True)]
        [string] $excelFile,

        #Types to load
        [Parameter(Mandatory=$False)]
        [string[]] $TypesToLoad,

        #Dee base path
        [Parameter(Mandatory=$False)]
        [string] $deeBasePath,

        #Document base path
        [Parameter(Mandatory=$False)]
        [string] $documentsBasePath,

        #Mapping Files base path
        [Parameter(Mandatory=$False)]
        [string] $mappingFilesBasePath,

        #Automation Workflow Files base path
        [Parameter(Mandatory=$False)]
        [string] $automationWorkflowFilesBasePath,

        #Checklist Image base path
        [Parameter(Mandatory=$False)]
        [string] $checklistImageBasePath,

        #ImportObjects base path
        [Parameter(Mandatory=$False)]
        [string] $importObjectBasePath,

        #create in collection
        [Parameter(Mandatory=$False)]
        [switch] $createInCollection,

        #Exclude Types
        [Parameter(Mandatory=$False)]
        [string[]] $TypesToExclude
    )

    LogWrite " >  Starting master data installation..."
    LogWrite "    $excelFile"

    Use-LightBusinessObjects $env

    # Prepare config arguments
    # Load master data logic assembly
    [Reflection.Assembly]::LoadFrom((Resolve-Path "$ModulePath\References\MasterData.Logic.dll")) | Out-Null

    $configArguments = [MasterData.Logic.ConfigArguments]::Singleton
    $configArguments.UploadedFile = $excelFile
    $configArguments.CreateInCollection = $createInCollection
    if ($deeBasePath) { $configArguments.DeeActionBasePath = (Resolve-Path $deeBasePath).Path }
    if ($documentsBasePath) { $configArguments.DocumentFilesBasePath = (Resolve-Path $documentsBasePath).Path }
    if ($automationWorkflowFilesBasePath) { $configArguments.AutomationWorkflowFilesBasePath = (Resolve-Path $automationWorkflowFilesBasePath).Path }
    if ($mappingFilesBasePath) { $configArguments.MappingFilesBasePath = (Resolve-Path $mappingFilesBasePath).Path }
    if ($checklistImageBasePath) { $configArguments.ChecklistImagePath = (Resolve-Path $checklistImageBasePath).Path }
    if ($importObjectBasePath) { $configArguments.ImportObjectBasePath = (Resolve-Path $importObjectBasePath).Path }

    # Get available types
    [MasterData.Logic.Runner]::GetAvailableTypes()
    # Prepare types to load
    if($TypesToLoad -eq $null)
    {
        $TypesToLoad = @()
        foreach($typeToLoad in $configArguments.AvailableTypes)
        {
            if (-not ($TypesToExclude -ne $null -and $TypesToExclude.Contains($typeToLoad.UserObjectType)))
            {
                $TypesToLoad += $typeToLoad.UserObjectType
            }
        }
    }

	if ( $TypesToLoad.length -gt 0 ) {
		LogWrite "    Loading types: "
		foreach($type in $TypesToLoad)
		{
			LogWrite ("      - " + $type)
		}		
		$TypesToLoad = $TypesToLoad | % { $_.ToLower() }
		$configArguments.ObjectsToLoad = $TypesToLoad

		$global:hasErrors = 0

		# register for logger events
		$logger = [MasterData.Logic.Logger]
		$eventJob = Register-ObjectEvent -InputObject $logger -EventName OnLog -Action {
			if($event.SourceArgs[0].LogType -eq [MasterData.Logic.LogType]::Error)
			{
				LogWrite ("   " + $event.SourceArgs[0].LogMessage) -ForegroundColor Red

				$global:hasErrors = 1
			
				if($global:InteractiveMode -eq $false -and $setPartiallySucceeded )
				{
					# Assuming this is running as part of a TFS build, set the build task to partially succeeded
					Write-Host "##vso[task.complete result=SucceededWithIssues;] There was at least one error during master data loading. Please check the master data log for details."
				}
			}
			else
			{
				LogWrite ("   "+$event.SourceArgs[0].LogMessage)
			}
		}

		# Run master data
		try
		{
			$activityDescription = "Uploading master data to MES..."
			Write-Progress -Activity ($activityDescription) -Status "Please wait..."
			[MasterData.Logic.Runner]::Run([MasterData.Logic.Runner+RunnerMode]::Upload)
			Write-Progress -Activity $activityDescription -Completed -Status "Master data has finished."
		}
		finally
		{
			[System.AppDomain]::CurrentDomain.SetData("APP_CONFIG_FILE", $null)
			Unregister-Event -SourceIdentifier $eventJob.Name
		}
		
		LogWrite " *  Finished running master data ($excelFile)!" -foregroundColor Green
	}
	else {
		LogWrite " *  No types to load on master data ($excelFile)!" -foregroundColor Green
	}

    return $global:hasErrors
}

<#
    .SYNOPSIS
        Load Master Data files by folder

    .DESCRIPTION
        Uses the master data loader application logic to load master data files within a folder into cmNavigo.
        This function uses the same logic as the standard Master Data Loader in Navigo.

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER excelFolder
        Absolute to the Excel file to be loaded.

    .PARAMETER typesToLoad
        String array containing the types to be loaded.
        If not defined, all types on the excel file will be loaded.

    .PARAMETER deeBasePath
        Absolute path to the folder containing the code files for DEE rules to be imported.
        Required if DEE rules are to be imported.

    .PARAMETER documentsBasePath
        Absolute path to the folder containint the documents to be uploaded.
        Required if Documents are to be imported.

    .PARAMETER createInCollection
        Set this flag if objects are to be loaded in collection. If not set, objects are loaded individually.

    .PARAMETER TypesToExclude
        String array containing the types to be excluded.
        If TypesToLoad is defined, this parameter is ignored.

    .PARAMETER stopOnError
        Stop loading if it finds error in loading onne master data

    .EXAMPLE
        Invoke-MasterDataLoaderByFolder $env -excelFolder 'C:\MasterData'

    .EXAMPLE
        Invoke-MasterDataLoaderByFolder $env -excelFolder 'C:\MasterData' -typesToLoad @('Parameter', 'DataCollection')

    .EXAMPLE
        Invoke-MasterDataLoaderByFolder $env -excelFolder 'C:\MasterData' -typesToLoad @('DEEAction') -deeBasePath 'C:\DEE'

    .EXAMPLE
        Invoke-MasterDataLoaderByFolder $env -excelFolder 'C:\MasterData' -typesToLoad @('Document') -documentsBasePath 'C:\Documents'

    .EXAMPLE
        Invoke-MasterDataLoaderByFolder $env -excelFolder 'C:\MasterData' -deeBasePath 'C:\DEE' -createInCollection

    .NOTES
        Master data logic uses LBOs to load data into cmNavigo. The LBOs assenmbly used is located on the References folder.
        Master data loading errors are shown on the console but not actually treated as execution errors or exceptions,
        since they are a common occurence in deployment procedures.
#>
function Invoke-MasterDataLoaderByFolder()
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        #Excel file
        [Parameter(Mandatory=$True)]
        [string] $excelFolder,

        #Types to load
        [Parameter(Mandatory=$False)]
        [string[]] $TypesToLoad,

        #Dee base path
        [Parameter(Mandatory=$False)]
        [string] $deeBasePath,

        #Document base path
        [Parameter(Mandatory=$False)]
        [string] $documentsBasePath,

        #Mapping Files base path
        [Parameter(Mandatory=$False)]
        [string] $mappingFilesBasePath,

        #Automation Workflow Files base path
        [Parameter(Mandatory=$False)]
        [string] $automationWorkflowFilesBasePath,

        #Checklist Image base path
        [Parameter(Mandatory=$False)]
        [string] $checklistImageBasePath,

        #ImportObjects base path
        [Parameter(Mandatory=$False)]
        [string] $importObjectBasePath,

        #create in collection
        [Parameter(Mandatory=$False)]
        [switch] $createInCollection,

        #Exclude Types
        [Parameter(Mandatory=$False)]
        [string[]] $TypesToExclude,

        #Stop on Error
        [Parameter(Mandatory=$False)]
        [bool] $stopOnError
    )

    Use-LightBusinessObjects $env
    # Prepare config arguments

	if (Test-Path $excelFolder)
	{
        $excelFolder = "$(Resolve-Path $excelFolder)"
		# Master Data Files in folder
		$excelFiles = Get-ChildItem ($excelFolder) -File -Include '*.xlsx','*.xml','*.json' -recurse | Where {$_.FullName -notlike "*\AutomationWorkflowFiles\*"} | Sort-Object Name
        LogWrite (" *  Loading $($excelFiles.Count) MasterData files from folder ($excelFolder)...")
        $OriginalTypesToLoad = $TypesToLoad
        $OriginalTypesToExclude = $TypesToExclude
		
		foreach ($file in $excelFiles )
		{
			# Get available types
			# Prepare types to load
			if ( $TypesToLoad -eq $null )
			{
				$TypesToLoad = Get-MasterDataAvailableTypes $env $file $TypesToExclude
			}

         	if (!($TypesToExclude -contains 'LookupTableValues') -and
				(!$TypesToLoad -or ($TypesToLoad -contains 'LookupTableValues')))
			{			
         		for ($i=0; $i -lt 10; $i++) {
					$hasErrors =  Invoke-MasterDataLoader $env    `
					-excelFile $file    `
					-typesToLoad @('LookupTableValues')
					if(! $hasErrors)
					{
						break
					}
				}
			}
		
			if (!($TypesToExclude -contains 'EntityTypeProperty') -and
				(!$TypesToLoad -or ($TypesToLoad -contains 'EntityTypeProperty')))
			{			
				for ($i=0; $i -lt 10; $i++) {
					$hasErrors =  Invoke-MasterDataLoader $env    `
					-excelFile $file    `
					-typesToLoad @('EntityTypeProperty')
					if(! $hasErrors)
					{
						break
					}
				}
			}
		
			if (!($TypesToExclude -contains 'Config') -and
				(!$TypesToLoad -or ($TypesToLoad -contains 'Config')))
			{			
				for ($i=0; $i -lt 10; $i++) {
					$hasErrors =  Invoke-MasterDataLoader $env    `
					-excelFile $file    `
					-typesToLoad @('Config')
					if(! $hasErrors)
					{
						break
					}
				}
			}
		
			$TypesToExclude += 'Config'
			$TypesToExclude += 'EntityTypeProperty'
			$TypesToExclude += 'LookupTableValues'

			# Identify the Actual Types to be loaded (TypesToLoad - TypesToExclude)
			$ActualTypesToLoad = @()			
			foreach ( $typeToLoad in $TypesToLoad ) {
				if (-not ( $TypesToExclude.Contains($typeToLoad) ))
				{
					$ActualTypesToLoad += $typeToLoad
				}
			}
			
			if ( $ActualTypesToLoad.length -gt 0 ) {
				$hasErrors = Invoke-MasterDataLoader $env	`
					-excelFile $file	`
					-deeBasePath $deeBasePath	`
					-documentsBasePath $documentsBasePath	`
					-mappingFilesBasePath $mappingFilesBasePath	`
					-automationWorkflowFilesBasePath $automationWorkflowFilesBasePath	`
                    -checklistImageBasePath $checklistImageBasePath	`
                    -importObjectBasePath $importObjectBasePath	`
					-createInCollection:$createInCollection	`
					-typesToLoad $ActualTypesToLoad	`
					-TypesToExclude $TypesToExclude
			}			
            
            $TypesToLoad = $OriginalTypesToLoad
            $TypesToExclude = $OriginalTypesToExclude

			if($stopOnError -and $hasErrors )
			{
				break
			}
		}
	}
	else {
		LogWrite ("No MasterData found in folder ($excelFolder)...") -foregroundColor Yellow
	}

    return $global:hasErrors
}

<#
    .SYNOPSIS
        Import cmNavigo objects

    .DESCRIPTION
        Imports objects to cmNavigo exported to xml using the native Export functionality.
        Supports versioned and non-versioned entity types. For versioned objects, a new change set is created and approved. The imported
        object is set effective.
        Static model objects that have import/export support in cmNavigo are also supported.

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER fileName
        Path to the xml file to be imported.

    .EXAMPLE
        Import-NavigoObject $env 'C:\DataCollection1.xml'
#>
function Import-NavigoObject
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # file
        [Parameter(Mandatory=$True)]
        [string]$filename
    )

    LogWrite " >  Starting object import..."

    Use-LightBusinessObjects $env

    #region Get entity type to find if it's versionable
    [xml] $xml = Get-Content $filename
    [string] $fullTypeName = $xml.'CMF.ExportFile'.Object.type
    $entityTypeName = $fullTypeName.Split(", ")[0].Split(".")[-1]

    # use try block to deal with objects from static model (not entity types)
    try
    {
        [Cmf.Foundation.BusinessOrchestration.EntityTypeManagement.InputObjects.GetEntityTypeByNameInput]$getETinput = New-Object Cmf.Foundation.BusinessOrchestration.EntityTypeManagement.InputObjects.GetEntityTypeByNameInput
        $getETinput.Name = $entityTypeName
        $isVersioned = $getETinput.GetEntityTypeByNameSync().EntityType.IsVersioned
    }
    catch [Exception] { $isVersioned = $false }
    #endregion

    # create change set
    if ($isVersioned)
    {
        [Cmf.Foundation.BusinessOrchestration.ChangeSetManagement.InputObjects.CreateChangeSetInput] $createCSInput = New-Object Cmf.Foundation.BusinessOrchestration.ChangeSetManagement.InputObjects.CreateChangeSetInput
        [Cmf.Foundation.BusinessObjects.ChangeSet] $changeSet = New-Object Cmf.Foundation.BusinessObjects.ChangeSet
        $changeSet.Name = [guid]::NewGuid()
        $createCSInput.ChangeSet = $changeSet
        $changeSet.Type = 'General'
        $changeSet.MakeEffectiveOnApproval = $true
        $changeSet = $createCSInput.CreateChangeSetSync().ChangeSet
    }

    # Call service for import
    [Cmf.Foundation.BusinessOrchestration.ImportExportManagement.InputObjects.ImportObjectsInput] $inputObj = New-Object Cmf.Foundation.BusinessOrchestration.ImportExportManagement.InputObjects.ImportObjectsInput
    $inputObj.Xml = Get-Content $filename -Encoding UTF8
    $inputObj.ChangeSet = $changeSet

    $return = $inputObj.ImportObjectsSync()

    $object = $return.Objects[0]

    if ($isVersioned)
    {
        # Approve Change set
        [Cmf.Foundation.BusinessOrchestration.ChangeSetManagement.InputObjects.RequestChangeSetApprovalInput] $requestApprovalInput = New-Object Cmf.Foundation.BusinessOrchestration.ChangeSetManagement.InputObjects.RequestChangeSetApprovalInput
        $requestApprovalInput.ChangeSet = $changeSet
        $requestApprovalInput.IgnoreLastServiceId = $true
        $return = $requestApprovalInput.RequestChangeSetApprovalSync()
    }

    LogWrite ("    Imported object " + $object.Name + "of type " + $object.GetType().Name + " ...")

    LogWrite " *  Finished importing object!" -foregroundColor Green
}

<#
    .SYNOPSIS
        Import cmNavigo objects

    .DESCRIPTION
        Imports objects to cmNavigo exported to xml using the native Export functionality.
        Supports versioned and non-versioned entity types. For versioned objects, a new change set is created and approved. The imported
        object is set effective.
        Static model objects that have import/export support in cmNavigo are also supported.

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER objectsPath
       Path of XML files to be imported.

    .EXAMPLE
        Import-NavigoObjects $env $listObjectsToImport -securityToken "uipagesimportdf"
#>
function Import-NavigoObjects
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # objectsPath
        [Parameter(Mandatory=$True)]
        [string]$objectsPath,

        # securityToken
        [Parameter(Mandatory=$False)]
        [string]$securityToken

    )

	LogWrite " >  Importing objects from folder $($objectsPath)..."
    if (Test-Path ($objectsPath)) {
        Use-LightBusinessObjects $env

        if ($securityToken)
        {
            #Insert import token in table
            $InsertQuery = "INSERT INTO [Control].[T_ImportToken] (SecurityToken, IsTokenActive)
                        VALUES ('$securityToken', 1);"

            $filePath =  '.\InsertImportToken.sql';

            Set-Content -path $filePath  -value $InsertQuery
            try{
                Invoke-SQLScript $env -database 'Online' -scriptPath $filePath
            }finally{
                Remove-Item $filePath
            }
        }

		# Extract ZIP files to be imported
        $archivesToDecompress = Get-ChildItem ("$objectsPath\*") -Include *.zip| Sort-Object Name
		foreach($archive in $archivesToDecompress)
		{
			LogWrite (" >  Extracting archive " + (Resolve-Path $archive | Split-Path -Leaf) + "...")
			$filesExpanded = Expand-Archive $archive -Destination (Resolve-Path $archive | Split-Path -Parent) -Force
		}
		
        $filesToImport = Get-ChildItem ("$objectsPath\*") -Include *.xml,*.cmf| Sort-Object Name
		
        if($filesToImport.Count -gt 0){
			
            foreach($filename in $filesToImport)
            {
				LogWrite (" >  Importing file " + (Resolve-Path $filename | Split-Path -Leaf) + "...")
				
				#region Get entity type to find if it's versionable
				[xml] $xml = Get-Content $filename
				[string] $fullTypeName = $xml.'CMF.ExportFile'.Object.type
				$entityTypeName = $fullTypeName.Split(", ")[0].Split(".")[-1]

				# use try block to deal with objects from static model (not entity types)
				try
				{
					[Cmf.Foundation.BusinessOrchestration.EntityTypeManagement.InputObjects.GetEntityTypeByNameInput]$getETinput = New-Object Cmf.Foundation.BusinessOrchestration.EntityTypeManagement.InputObjects.GetEntityTypeByNameInput
					$getETinput.Name = $entityTypeName
					$isVersioned = $getETinput.GetEntityTypeByNameSync().EntityType.IsVersioned
				}
				catch [Exception] { $isVersioned = $false }
				#endregion


				# create change set
				if ($isVersioned)
				{
					[Cmf.Foundation.BusinessOrchestration.ChangeSetManagement.InputObjects.CreateChangeSetInput] $createCSInput = New-Object Cmf.Foundation.BusinessOrchestration.ChangeSetManagement.InputObjects.CreateChangeSetInput
					[Cmf.Foundation.BusinessObjects.ChangeSet] $changeSet = New-Object Cmf.Foundation.BusinessObjects.ChangeSet
					$changeSet.Name = [guid]::NewGuid()
					$createCSInput.ChangeSet = $changeSet
					$changeSet.Type = 'General'
					$changeSet.MakeEffectiveOnApproval = $true
					$changeSet = $createCSInput.CreateChangeSetSync().ChangeSet
				}


				# Call service for import
				[Cmf.Foundation.BusinessOrchestration.ImportExportManagement.InputObjects.ImportObjectsInput] $inputObj = New-Object Cmf.Foundation.BusinessOrchestration.ImportExportManagement.InputObjects.ImportObjectsInput
				$inputObj.Xml = Get-Content $filename -Encoding UTF8
				$inputObj.ChangeSet = $changeSet

				if ($securityToken)
				{
					$inputObj.SecurityToken = "$securityToken"
					$inputObj.EntityTypeNamesReplace = New-Object System.Collections.ObjectModel.Collection -TypeName String
					$inputObj.EntityTypeNamesReplace[0] = $entityTypeName
				}

				$return = $inputObj.ImportObjectsSync()

				$objects = $return.Objects

				if ($isVersioned)
				{
					# Approve Change set
					[Cmf.Foundation.BusinessOrchestration.ChangeSetManagement.InputObjects.RequestChangeSetApprovalInput] $requestApprovalInput = New-Object Cmf.Foundation.BusinessOrchestration.ChangeSetManagement.InputObjects.RequestChangeSetApprovalInput
					$requestApprovalInput.ChangeSet = $changeSet
					$requestApprovalInput.IgnoreLastServiceId = $true
					$return = $requestApprovalInput.RequestChangeSetApprovalSync()
				}

				foreach ($object in $objects) {
					LogWrite ("    Imported object " + $object.Name + " of type " + $object.GetType().Name + " ...")
				}
			}
        }

        #Delete import token in table
        if ($securityToken)
        {
            $DELQuery = "DELETE From [Control].[T_ImportToken] Where SecurityToken='$securityToken'"
            $filePath =  '.\DeleteImportToken.sql';

            Set-Content -path $filePath  -value $DELQuery
            try{
                Invoke-SQLScript $env -database 'Online' -scriptPath $filePath
            }finally{
                Remove-Item $filePath
            }
        }

        LogWrite " *  Finished importing objects!" -foregroundColor Green
    } else {
        LogWrite " !  Folder not found!" -foregroundColor Yellow
    }
}

<#
    .SYNOPSIS
        Creates a new object to represent a cmNavigo ERP Action.

    .DESCRIPTION
        Returns a new custom object which contains all the required configuration properties to load a custom ERP action to
        cmNavigo. Should be used in conjunction with Install-ERPActions function.

     .PARAMETER ActionName
        Name of the action

    .PARAMETER Description
        The description of the ERP action

    .PARAMETER SourceName
        The Name of the IDOC to use

    .PARAMETER TargetName
        The Name of the DEE Action to use

    .EXAMPLE
        $erpActions += New-ERPAction -actionName 'Employee01' -description 'Creates employees from SAP' -sourceName 'HRMD_A09' -targetName 'ZHRMDIDocReceiverAction'

    .OUTPUTS
        A custom object representing the cmNavigo service. The object contains the following properties:
        - ActionName:     The name of ERP action
        - Description:    The description of the action
        - SourceName:    The name of the IDOC to use
        - TargetName:    The name of the DEE to use
#>
function New-ERPAction()
{
    param
    (
        [Parameter(Mandatory=$True)]
        [string] $actionName,

        [Parameter(Mandatory=$True)]
        [string] $description,

        [Parameter(Mandatory=$True)]
        [string] $sourceName,

        [Parameter(Mandatory=$True)]
        [string] $targetName
    )

    $erpAction = new-object PSObject

    $erpAction | add-member -type NoteProperty -Name ActionName -Value $actionName
    $erpAction | add-member -type NoteProperty -Name Description -Value $description
    $erpAction | add-member -type NoteProperty -Name SourceName -Value $sourceName
    $erpAction | add-member -type NoteProperty -Name TargetName -Value $targetName

    return $erpAction
}

<#
    .SYNOPSIS
        Installs cmNavigo ERP Actions

    .DESCRIPTION
        Install one or more cmNavigo ERP Actions. If already existing, the ERP action will be ignore and nothing will be done.

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER erpActions
        Array containing the ERP actions to be added. Each action should be a custom object created with the New-ERPAction
        function.


    .EXAMPLE
        Install-ERPActions $env -erpActions $erpActions

#>
function Install-ERPActions()
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # Services to Install
        [Parameter(Mandatory=$True)]
        [Object[]]$erpActions
    )

    LogWrite " >  Starting deploy of ERP action to database..."

    Use-LightBusinessObjects $env

    foreach($service in $erpActions)
    {
        [Cmf.Foundation.BusinessOrchestration.ErpManagement.InputObjects.GetErpTriggerByNameInput] $getErpTrigger = New-Object Cmf.Foundation.BusinessOrchestration.ErpManagement.InputObjects.GetErpTriggerByNameInput
        $getErpTrigger.Name = $service.ActionName

        $continue = $true

        try
        {
            $result = $getErpTrigger.GetErpTriggerByNameSync()
            if ($result -or $result.ErpTrigger -or $result.ErpTrigger.Name)
            {
                LogWrite ("    Trigger " + $service.ActionName + " already in server ") $appserver.ServerName -foregroundColor Yellow
                continue = $false
            }
        }
        catch{}



        if ($continue)
        {
             LogWrite ("    Adding ERP Action " + $service.ActionName + " ...")
            [Cmf.Foundation.BusinessOrchestration.ErpManagement.InputObjects.CreateErpTriggerInput] $createServiceInput = New-Object  Cmf.Foundation.BusinessOrchestration.ErpManagement.InputObjects.CreateErpTriggerInput

             [Cmf.Foundation.Erp.ErpTrigger] $inputObj  = New-Object  Cmf.Foundation.Erp.ErpTrigger
            $inputObj.Name = $service.ActionName
            $inputObj.Description = $service.Description
            $inputObj.SourceName = $service.SourceName
            $inputObj.Targetname = $service.Targetname
            $inputObj.Type = [Cmf.Foundation.Erp.ErpTriggerType]::DeeRule

            $createServiceInput.ErpTrigger = $inputObj;
        $return = $createServiceInput.CreateErpTriggerSync()

        }

    }

    LogWrite " *  Finished deploy of ERP actions to database!" -foregroundColor Green
}

<#
    .SYNOPSIS
        Creates a new object to represent a cmNavigo ERP Function.

    .DESCRIPTION
        Returns a new custom object which contains all the required configuration properties to load a custom ERP function to
        cmNavigo. Should be used in conjunction with Install-ERPFunctions function.

    .PARAMETER FunctionName
        The Name of the function

    .EXAMPLE
        New-ERPFunction -functionName 'BAPI_TRANSACTION_COMMIT'

    .OUTPUTS
        A custom object representing the cmNavigo ERP Function. The object contains the following properties:
        - FunctionName:     Name of the function to import
#>
function New-ERPFunction()
{
    param
    (
        [Parameter(Mandatory=$True)]
        [string] $functionName
    )

    $erpFunction = new-object PSObject

    $erpFunction | add-member -type NoteProperty -Name FunctionName -Value $functionName

    return $erpFunction
}

<#
    .SYNOPSIS
        Installs cmNavigo ERP Functions

    .DESCRIPTION
        Install one or more cmNavigo ERP Functions. If already exists, the ERP function will be ignore and nothing will be done.

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER erpActions
        Array containing the ERP functions to be added. Each function should be a custom object created with the New-ERPFunction
        function.

    .EXAMPLE
       Install-ERPFunctions $env -erpFunctions $erpFunctions -createServices $false


#>
function Install-ERPFunctions()
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # Services to Install
        [Parameter(Mandatory=$True)]
        [Object[]]$erpFunctions,

        # If Create Services
        [Parameter(Mandatory=$True)]
        [bool]$createServices
    )

    LogWrite " >  Starting deploy of ERP function to database..."

    $functionsAdded = 0;
    Use-LightBusinessObjects $env

    foreach($service in $erpFunctions)
    {
        try
        {
            [Cmf.Foundation.BusinessOrchestration.ErpManagement.OutputObjects.CreateErpFunctionInput] $getErpTrigger = New-Object Cmf.Foundation.BusinessOrchestration.ErpManagement.OutputObjects.CreateErpFunctionInput

            [Cmf.Foundation.Erp.ErpFunction] $inputObj = New-Object Cmf.Foundation.Erp.ErpFunction

            $inputObj.Name = $service.FunctionName
            $getErpTrigger.ErpFunction = $inputObj;

            $result = $getErpTrigger.CreateErpFunctionSync()

            LogWrite ("    Function " + $service.FunctionName + " was added.")
            $functionsAdded = $functionsAdded  + 1;
        }
        catch [Exception]
        {
            LogWrite $_.Exception.Message
        }
    }



    if ($functionsAdded -gt 0)
    {
        LogWrite ("    ERP Functions added " + $functionsAdded + ".") -foregroundColor Green
        [Cmf.Foundation.BusinessOrchestration.ErpManagement.InputObjects.GenerateSourceCodeInput] $generateServices =  New-Object Cmf.Foundation.BusinessOrchestration.ErpManagement.InputObjects.GenerateSourceCodeInput
        $generateServices.CreateServices = $createServices
        $result = $generateServices.ErpGenerateSourceCodeSync();
        LogWrite ('    Code was generated successfully.')
    }
    else
    {
        LogWrite ("    ERP Functions added " + $functionsAdded +"." ) -foregroundColor Yellow
    }

    LogWrite (" *  Finished deploy of ERP Functions to database!") -foregroundColor Green

}


<#
    .SYNOPSIS
        Creates a new object to represent a cmNavigo DEE Action.

    .DESCRIPTION
        Returns a new custom object which contains all the required configuration properties to load a DEE Action to
        cmNavigo. Should be used in conjunction with Execute-DEEAction or Delete-DEEAction function.

    .PARAMETER FunctionName
        The Name of the DEE action

    .EXAMPLE
        New-DEEAction -deeName 'ATSHTBRevertResourceState'

    .OUTPUTS
        A custom object representing the cmNavigo DEE Action. The object contains the following properties:
        - DEEName:     Name of the DEE Action
#>
function New-DEEAction()
{
    param
    (
        [Parameter(Mandatory=$True)]
        [string] $deeName
    )

    $deeAction = new-object PSObject

    $deeAction | add-member -type NoteProperty -Name DEEACtionName -Value $deeName

    return $deeAction
}

<#
    .SYNOPSIS
        Execute cmNavigo DEE Actions

    .DESCRIPTION
        Execute one or more cmNavigo DEE Actions. If the execution fails, a message will be sent to user, but the process continue.

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER deeActions
        Array containing the DEE Actions to be executed. Each DEE Action should be a custom object created with the New-DEEAction
        function.

    .EXAMPLE
       Execute-DEEAction $env -deeActions $deeActions

#>
function Invoke-DEEAction()
{

      param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # Actions to execute
        [Parameter(Mandatory=$True)]
        [Object[]]$deeActions
    )

    LogWrite " >  Starting execution of DEE actions..."

    $totalActionsToExecute = $deeActions.Count
    $executedActions = 0;
    $activityDescription = 'Execution DEE Actions'

    Write-Progress -Activity ($activityDescription) -PercentComplete 0 -CurrentOperation    "0% complete" -Status "Please wait..."


    Use-LightBusinessObjects $env
    $index = 0

    foreach($dee in $deeActions)
    {
        $percentage = [int](($index / $totalActionsToExecute) * 100)
        Write-Progress -Activity ($activityDescription) -PercentComplete $percentage -CurrentOperation  "$percentage% complete" -Status (($index + 1).ToString() + " of " + $totalActionsToExecute.ToString() + ". Execution action " + $dee.DEEACtionName + "...    Please wait...")

        try
        {
            [Cmf.Foundation.BusinessOrchestration.DynamicExecutionEngineManagement.InputObjects.ExecuteActionInput] $inputAction = New-Object Cmf.Foundation.BusinessOrchestration.DynamicExecutionEngineManagement.InputObjects.ExecuteActionInput

            [Cmf.Foundation.Common.DynamicExecutionEngine.Action] $action = New-Object Cmf.Foundation.Common.DynamicExecutionEngine.Action
            $action.Name = $dee.DEEACtionName

            $inputAction.Action = $action

            $result = $inputAction.ExecuteActionSync()

            LogWrite ("    DEE Action " + $dee.DEEACtionName + " was executed correctly.")
            $executedActions = $executedActions + 1;
        }
        catch [Exception]
        {
            LogWrite $_.Exception.Message
        }
        finally
        {
            $index = $index  + 1
            $percentage = [int](($index / $totalActionsToExecute) * 100)
            Write-Progress -Activity ($activityDescription) -PercentComplete $percentage -CurrentOperation    "$percentage% complete" -Status "Please wait..."
        }
    }

    Write-Progress -Activity $activityDescription -Completed -Status "Actions has been executed."

    if ($executedActions -gt 0)
    {
        if ($executedActions -eq $totalActionsToExecute)
        {
            LogWrite ("    DEE actions executed with success.") -foregroundColor Green
        }
        else
        {
            LogWrite ("    Not all DEE actions executed with success.") -foregroundColor Yellow
        }
    }
    else
    {
        LogWrite ("    DEE ACtions did not execute with success." ) -foregroundColor Yellow
    }

    LogWrite (" *  Finished Execution DEE Actions!") -foregroundColor Green
}

<#
    .SYNOPSIS
        Delete cmNavigo DEE Actions

    .DESCRIPTION
        Delete one or more cmNavigo DEE Actions. If the delete fails, a message will be sent to user, but the process continue.

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER deeActions
        Array containing the DEE Actions to be executed. Each DEE Action should be a custom object created with the New-DEEAction
        function.

    .EXAMPLE
       Delete-DEEAction $env -deeActions $deeActions

#>
function Remove-DEEAction()
{

    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # Actions to execute
        [Parameter(Mandatory=$True)]
        [Object[]]$deeActions
    )

    LogWrite " >  Starting execution of DEE actions..."

    $totalActionsDelete = $deeActions.Count
    $deletedActions = 0;
    $activityDescription = 'Deleting DEE Actions'

    Write-Progress -Activity ($activityDescription) -PercentComplete 0 -CurrentOperation    "0% complete" -Status "Please wait..."


    Use-LightBusinessObjects $env
    $index = 0

    foreach($dee in $deeActions)
    {
        $percentage = [int](($index / $totalActionsDelete) * 100)
        Write-Progress -Activity ($activityDescription) -PercentComplete $percentage -CurrentOperation  "$percentage% complete" -Status (($index + 1).ToString() + " of " + $totalActionsDelete.ToString() + ". Delete action " + $dee.DEEACtionName + "...    Please wait...")

        try
        {

            [Cmf.Foundation.Common.DynamicExecutionEngine.Action] $action = New-Object Cmf.Foundation.Common.DynamicExecutionEngine.Action
            [Cmf.Foundation.BusinessOrchestration.DynamicExecutionEngineManagement.InputObjects.RemoveActionInput] $removeAction = New-Object Cmf.Foundation.BusinessOrchestration.DynamicExecutionEngineManagement.InputObjects.RemoveActionInput
            [Cmf.Foundation.BusinessOrchestration.DynamicExecutionEngineManagement.InputObjects] $getAction = New-Object  Cmf.Foundation.BusinessOrchestration.DynamicExecutionEngineManagement.InputObjects.GetActionByNameInput

            $getAction.Name = $dee.DEEACtionName

            $action  = $getAction.GetActionByNameSync().Action
            $removeAction.Action = $action;
            $result = $removeAction.RemoveActionSync();


            LogWrite ("    DEE Action " + $dee.DEEACtionName + " was deleted correctly.")
            $deletedActions = $deletedActions + 1;
        }
        catch [Exception]
        {
            LogWrite $_.Exception.Message
        }
        finally
        {
            $index = $index  + 1
            $percentage = [int](($index / $totalActionsDelete) * 100)
            Write-Progress -Activity ($activityDescription) -PercentComplete $percentage -CurrentOperation    "$percentage% complete" -Status "Please wait..."
        }
    }

    Write-Progress -Activity $activityDescription -Completed -Status "Actions has been deleted."

    if ($deletedActions -gt 0)
    {
        if ($deletedActions -eq $totalActionsDelete)
        {
            LogWrite ("    DEE actions deleted with success.") -foregroundColor Green
        }
        else
        {
            LogWrite ("    Not all DEE actions deleted with success.") -foregroundColor Yellow
        }
    }
    else
    {
        LogWrite ("    DEE Actions did not delete with success." ) -foregroundColor Yellow
    }

    LogWrite (" *  Finished delete DEE Actions!") -foregroundColor Green
}



<#
    .SYNOPSIS
        Remove cmNavigo DEE Actions Triggers

    .DESCRIPTION
        Delete one, or all, DEE Action triggers from action. If no trigger defined, all triggers for that DEE action will be deleted

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER $deeActionName
        The Dee action name that contains the trigger to be deleted

    .PARAMETER $triggerName
        The name of the trigger to be deleted

    .EXAMPLE
       Remove-DEEActionTrigger $env -deeActionName 'ATSHTBRevertResourceState' -triggerName 'BusinessObjects.ChecklistInstance.Terminate.Post'

    .EXAMPLE
        Remove-DEEActionTrigger $env -deeActionName 'ATSHTBRevertResourceState'

#>

function Remove-DEEActionTrigger()
{

    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # DEE Action
        [Parameter(Mandatory=$True)]
        [string]$deeActionName,

        [Parameter(Mandatory=$False)]
        [string]$triggerName
    )
        Use-LightBusinessObjects $env

        [Cmf.Foundation.Common.DynamicExecutionEngine.Action] $action = New-Object Cmf.Foundation.Common.DynamicExecutionEngine.Action
        [Cmf.Foundation.BusinessOrchestration.DynamicExecutionEngineManagement.InputObjects] $getAction = New-Object  Cmf.Foundation.BusinessOrchestration.DynamicExecutionEngineManagement.InputObjects.GetActionByNameInput
        $getAction.Name = $deeActionName
        $action  = $getAction.GetActionByNameSync().Action


          [Cmf.Foundation.Common.DynamicExecutionEngine.ActionGroupActionCollection] $actionGroups =  new-object Cmf.Foundation.Common.DynamicExecutionEngine.ActionGroupActionCollection
          [Cmf.Foundation.BusinessOrchestration.DynamicExecutionEngineManagement.InputObjects.RemoveActionGroupActionsInput] $removeActionGroup = new-object Cmf.Foundation.BusinessOrchestration.DynamicExecutionEngineManagement.InputObjects.RemoveActionGroupActionsInput

        if ($triggerName)
        {
            LogWrite (" >  Deleting Action trigger " + $triggerName + " on DEE Action " + $deeActionName + "...")
            $found = $false

            for($i=0; $i -le $action.ActionGroupActions.Count ; $i++)
            {
                $actionGroup  = $action.ActionGroupActions[$i]
                 if ($actionGroup.ActionGroup -ne $null -and $actionGroup.ActionGroup.Name.toLower().equals($triggerName.ToLower()))
                {
                    $removeActionGroup = new-object Cmf.Foundation.BusinessOrchestration.DynamicExecutionEngineManagement.InputObjects.RemoveActionGroupActionsInput
                    $removeActionGroup.ActionGroupActions = new-object Cmf.Foundation.Common.DynamicExecutionEngine.ActionGroupActionCollection
                    $removeActionGroup.ActionGroup = $actionGroup.ActionGroup;
                    $removeActionGroup.ActionGroupActions.Add($actionGroup);
                    $result = $removeActionGroup.RemoveActionGroupActionsSync();
                    $found = $true
                    break
                }
            }

            if ($found -eq $true)
            {
                LogWrite (" >  Action trigger " + $triggerName + " on DEE Action " + $deeActionName + "deleted with success...") -foregroundColor Green
            }
            else
            {
                LogWrite (" >  Action trigger " + $triggerName + " on DEE Action " + $deeActionName + " not found ...") -ForeGroundColor Yellow
            }

        }
        else
        {
            LogWrite (" >  Deleting all actions trigger from DEE Action " + $deeActionName + "...")

            for($i=0; $i -le $action.ActionGroupActions.Count ; $i++)
            {
                $actionGroup  = $action.ActionGroupActions[$i]
                 if ($actionGroup.ActionGroup -ne $null)
                {
                    LogWrite (" >  Deleting Action trigger " + $actionGroup.ActionGroup.Name + "...")

                    $removeActionGroup = new-object Cmf.Foundation.BusinessOrchestration.DynamicExecutionEngineManagement.InputObjects.RemoveActionGroupActionsInput
                    $removeActionGroup.ActionGroupActions = new-object Cmf.Foundation.Common.DynamicExecutionEngine.ActionGroupActionCollection
                    $removeActionGroup.ActionGroup = $actionGroup.ActionGroup;
                    $removeActionGroup.ActionGroupActions.Add($actionGroup);
                    $result = $removeActionGroup.RemoveActionGroupActionsSync();
                }

            }
            LogWrite (" >  All actions trigger was deleted from DEE Action " + $deeActionName + "...") -ForeGroundColor Green
        }
}


<#
    .SYNOPSIS
        Generic function to write display all messages, and write them to a log file

    .DESCRIPTION
        Writes log message to display. Called like the Write-Host, but with the feature to write all display to log file.
        If there is the need to not write the log file in certain transaction, a global variable ($GenericLogFile) is inside of this file,
        and we can set it to not write log file.
        By default, always write file with this criteria: [computername]_[currentdate (yyyyMMddHH.mm.ss)].log


    .PARAMETER $logstring
        The string to display and write to log file

    .PARAMETER $ForeGroundColor
        Possiblity to define a foreground color for the string to display.
        The ForeGroundColor also defines to level of information (Red = Error; YEllow = Warning; Other = Info)

    .PARAMETER $BackgroundColor
        Possiblity to define a background color for the string to display

    .PARAMETER $LogToFile
        If want to display a certain message, but not want to write in log file

    .EXAMPLE
       LogWrite  'The message to display'

    .EXAMPLE
       LogWrite  ("The message to display with parameters " + paramter[0] + "_" + parameter[b] + ".")

    .EXAMPLE
       LogWrite  'The message to display' -foregroundColor Green

    .EXAMPLE
       LogWrite  'The message to display' -foregroundColor Green -logToFile $false


    .OUTPUTS
        A custom object representing the cmNavigo ERP Function. The object contains the following properties:
        - FunctionName:     Name of the function to import
#>
Function LogWrite
{
   Param (
       [string]$logstring

       ,[Parameter(Mandatory=$False)]
        [string]$ForeGroundColor

        ,[Parameter(Mandatory=$False)]
        [string]$BackgroundColor

       ,[Parameter(Mandatory=$False)]
        [bool]$LogToFile = $true
     )

    $messageType = "[INFO]"

    if($global:InteractiveMode -eq "")
    {
        $global:InteractiveMode = $true
    }


    if ($ForeGroundColor -eq '')
    {
        if ($global:InteractiveMode -eq $true)
        {
            $ForeGroundColor = (get-host).ui.rawui.ForegroundColor
        }
    }
    else
    {
        if ($ForeGroundColor.ToLower() -eq "red")
        {
            $messageType = "[ERROR]"
        }
        elseif ($ForeGroundColor.ToLower() -eq "yellow")
        {
            $messageType = "[WARNING]"
        }
    }

    if ($BackgroundColor -eq '')
    {
        if ($global:InteractiveMode -eq $true)
        {
            $BackgroundColor = (get-host).ui.rawui.BackgroundColor
        }
    }

    if($global:InteractiveMode -eq $true){
        Write-Host $logstring -ForegroundColor $ForeGroundColor -BackgroundColor $BackgroundColor
    }
    else {
        Write-Verbose "$messageType $logstring" -Verbose
    }

    $logTime = "[" + (Get-Date).ToString("yyyy-MM-dd HH:mm:ss") + "]"
    if ($GenericLogFile -eq $true -and $LogToFile -eq $true)
    {
        Add-content $Logfile -value ($logTime + $messageType + $logstring)
    }
}



<#
    .SYNOPSIS
        Create a command to extract files from a zipped file

    .DESCRIPTION
        Create a command to extract files from a zipped file. This command intends to be called with 3 parameters
        This function, by it self, does not execute anything. Must be called inside a Invoke-Command action


    .PARAMETER $targetdir
        The location where to extract the files


    .PARAMETER $zipfilename
        The location,and the name, of the zip file to extact

    .PARAMETER $activityDescription
        The description of what is to uniz

    .EXAMPLE
        OverwriteBackupFile 'c:\filename.zip'


#>
function CreateUnzipCommand
{
    return {
        param($targetdir, $zipfilename, $activityDescription, $removeTarget = $true)
        
        if ( ( Test-Path $zipfilename ) -eq $true )
        {
            if (!(Test-Path $targetdir))
            {
                $r = mkdir $targetdir
            }

            [Reflection.Assembly]::LoadWithPartialName( "System.IO.Compression.FileSystem" ) | Out-Null
            [Reflection.Assembly]::LoadWithPartialName( "System.IO.Path" ) | Out-Null
            
            $manifestFileName = "BackupManifest.xml"

            $zip = [System.IO.Compression.ZipFile]::OpenRead($zipfilename)

            # Check if backup has a manifest
            $backupManifestFile = $zip.Entries | Where-Object { $_.FullName -eq "$manifestFileName" }
            if ( $backupManifestFile )
            {
                # Try to read manifest from zip file
                try {
                    $ZipStream = $backupManifestFile.Open()

                    $Reader = [System.IO.StreamReader]::new($ZipStream)
                    $backupManifest = $Reader.ReadToEnd()
                }
                finally
                {                    
                    if ( $Reader ) { 
                        $Reader.Dispose(); 
                    }
                    if ( $ZipStream ) {
                        $ZipStream.Dispose();
                    }
                }
            }

            if ( $backupManifest ) {
				# Backup Manifest was found in zip file - Partial Restore
                $backupManifest = [xml]$backupManifest

				# Get Files/Folders to be restored
				$files = $backupManifest.BackupManifest.Content.File
				$folders = $backupManifest.BackupManifest.Content.Folder

				$manifestInTargetDir = Join-Path -Path "$targetdir" -ChildPath "$manifestFileName"
				if ( $removeTarget ) {

					if ( Test-Path -Path "$manifestInTargetDir" ) {
						Remove-Item -Path "$manifestInTargetDir" -Force
					}

					foreach ( $file in $files ) {
						$fileToRemove = Join-Path -Path "$targetdir" -ChildPath "$file"
						if ( Test-Path -Path "$fileToRemove" ) {
							Remove-Item -Path "$fileToRemove" -Force
						}
					}

					foreach ( $folder in $folders ) {
						$folderToRemove = Join-Path -Path "$targetdir" -ChildPath "$folder"
						if ( Test-Path -Path "$folderToRemove" ) {
							Remove-Item -Path "$folderToRemove" -Recurse -Force
						}
					}
				}
            } else {
                # No Backup Manifest was found in zip file - Full Restore
                if ( $removeTarget ) {
                    Get-ChildItem $targetdir | ForEach-Object {
                        Remove-Item $_.FullName -Recurse -Force
                    }
                }
            }

            [System.IO.Compression.ZipFile]::ExtractToDirectory($zipfilename, $targetdir);

            # remove manifest from target dir after restoring zip
            if ( $manifestInTargetDir -and ( Test-Path -Path "$manifestInTargetDir" ) ) {
                Remove-Item -Path "$manifestInTargetDir" -Force
            }
        }
        else
        {
            throw 'Specified backup file does not exist.'
        }
    }
}


<#
    .SYNOPSIS
        Creates a new object to represent a cmNavigo Report.

    .DESCRIPTION
        Returns a new custom object which contains all the required configuration properties to load a custom report to
        cmNavigo. Should be used in conjunction with Install-Reports function.

    .PARAMETER ReportPath
        The physical location of the report to upload

    .PARAMETER Database
        Default database name on which report will be ran

    .PARAMETER ReportFolder
        The folder location where the report will be uploaded

    .EXAMPLE
        New-Report -reportPath $ScriptPath'\Reports\Chemical Lab.rdl' -database 'ODS' -reportFolder '/Custom'

    .EXAMPLE
        New-Report -reportPath $ScriptPath'\Reports\Chemical Lab_Online.rdl' -database 'Online' -reportFolder '/Custom'

    .EXAMPLE
        New-Report -reportPath $ScriptPath'\Reports\Chemical Lab_DWH.rdl' -database 'DWH' -reportFolder '/Custom'


    .OUTPUTS
        A custom object representing the cmNavigo Report. The object contains the following properties:
        - ReportPath:		The physical location of the report to upload
		- Database:			The database name on which report will be ran
		- ReportFolder:     THe folder where report will be uploaded


#>
function New-Report
{
      param
    (
        [Parameter(Mandatory=$True)]
        [string] $reportPath,

        # Database
        [Parameter(Mandatory=$True)]
        [CMFDatabase] $database,

        [Parameter(Mandatory=$True)]
        [string] $reportFolder

    )

    $report = new-object PSObject

    $report| add-member -type NoteProperty -Name ReportPath -Value $reportPath
    $report| add-member -type NoteProperty -Name DataBase -Value $database
    $report| add-member -type NoteProperty -Name ReportFolder -Value $reportFolder

    return $report
}

<#
    .SYNOPSIS
        Installs cmNavigo Reports

    .DESCRIPTION
        Install one or more cmNavigo Reports. If already existing, the report will be replaced.
        Also, the datasource of the report will be updated.

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER reports
        Array containing the reports to be added. Each report should be a custom object created with the New-Report
        function.

    .EXAMPLE
       Install-Reports -env $env -reports $reports

    .NOTES
        If Report folder does not exist, it will be created.
        The datasource and report will be overwritten if send more than one time
#>
function Install-Reports
{
     param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # Reports to Install
        [Parameter(Mandatory=$True)]
        [Object[]]$reports

    )

    Use-LightBusinessObjects $env

    LogWrite " >  Starting adding Reports to server..."

    $reportServerUri = $env.ReportServerUri +  "/ReportService2010.asmx?wsdl"

    if ($env.ReportUseDefaultCredential)
    {
        $rs = New-WebServiceProxy -Uri $reportServerUri -UseDefaultCredential
    }
    else
    {
        $username = $env.ReportServerUser
        $password = $env.ReportServerPassword
        $credentials = New-Object System.Management.Automation.PSCredential -ArgumentList @($username,(ConvertTo-SecureString -String $password -AsPlainText -Force))
        $rs = New-WebServiceProxy -Uri $reportServerUri -Credential $credentials
    }
    if ($rs -ne $null)
    {

        $warnings = $null
        try
        {
            $activityDescription = "Adding reports to Report server..."
            LogWrite ("    " + $activityDescription)
            $total = $reports.Count
            $index = 0
            Write-Progress -Activity ($activityDescription) -PercentComplete 0 -CurrentOperation    "0% complete" -Status "Please wait..."
            $uploadSuccess = 0;
            foreach($reportInfo in $reports)
            {
                try
                {
                    $reportName = [System.IO.Path]::GetFileNameWithoutExtension($reportInfo.ReportPath)
                    $bytes = [System.IO.File]::ReadAllBytes($reportInfo.ReportPath)

                    [Cmf.Foundation.BusinessOrchestration.ConfigurationManagement.InputObjects.GetConfigByPathInput] $inputGetConfigPath =  New-Object Cmf.Foundation.BusinessOrchestration.ConfigurationManagement.InputObjects.GetConfigByPathInput

                    $reportRootFolder = ""
                    try
                    {
                        $inputGetConfigPath.Path = "/Cmf/System/Configuration/Reporting/Reports Root Folder/"
                        $resultGetConfigPath = $inputGetConfigPath.GetConfigByPathSync()
                    }
                    catch [Exception]{
                        Write-Host $_.Exception
                    }

                    if ($resultGetConfigPath.Config)
                    {
                        $reportRootFolder = $resultGetConfigPath.Config.Value
                    }

                    $reportInfo.ReportFolder = $reportRootFolder + $reportInfo.ReportFolder

                    $targetFolderPath = $reportInfo.ReportFolder
                    $percentage = [int](($index / $total) * 100)
                    Write-Progress -Activity ($activityDescription) -PercentComplete $percentage -CurrentOperation  "$percentage% complete" -Status (($index + 1).ToString() + " of " + $total.ToString() + ". Uploading report " + $reportName +" to " + $targetFolderPath + "...    Please wait...")

                    LogWrite("    " + "Uploading report " + $reportName +" to " + $targetFolderPath + "...")
                    $x = $reportInfo.ReportFolder.Split('/')
                    $i = 0;
                    if ($x.Count -ge 1)
                    {
                        $reportFolder = "/"

                        while ($i -lt $x.Count)
                        {
                            #Check if folder is existing, create if not found
                            try
                            {
                                $i = $i + 1
                                if ($x[1] -ne "")
                                {
                                        $reportPath = $x[$i]
                                        $reportFolderCopy = $reportFolder;
                                        if ($i -eq 1)
                                        {
                                            $reportFolder = $reportFolder + $reportPath
                                        }
                                        else
                                        {
                                            $reportFolder = $reportFolder + "/" + $reportPath
                                        }
                                    #need to do this first, because the parent folder may exists, but the child don't
                                        $ignoreMessage = $rs.CreateFolder($reportPath,$reportFolderCopy, $null)

                                }
                            }
                            catch [System.Web.Services.Protocols.SoapException]
                            {
                                if (-not( $_.Exception.Detail.InnerText -match "[^rsItemAlreadyExists400]"))
                                {
                                    $msg = "[Install-SSRSRDL()] Error creating folder: $reportFolder. Msg: '{0}'" -f $_.Exception.Detail.InnerText
                                    throw $msg
                                }
                            }
                        }
                    }

                    $report = $rs.CreateCatalogItem(

                        "Report",         # Catalog item type
                        $reportName,      # Report name
                        $targetFolderPath,# Destination folder
                        $true,            # Overwrite report if it exists?
                        $bytes,           # .rdl file contents
                        $null,            # Properties to set.
                        [ref]$warnings    # Warnings that occured while uploading.
                    )


                    # Get the names of the data sources that the
                    # uploaded report references. Note that this might be different from
                    # the name of the datasource as it is deployed on the report server!
					foreach($itemDataSource in @($rs.GetItemDataSources($report.Path)))
					{
						$referencedDataSourceName = $itemDataSource.Name
						$targetDatasourceRef = "";

						switch ($referencedDataSourceName)
						{
							"ONLINEDataSource"
							{
								$targetDatasourceRef = $env.ReportsOnlineDataSource
							}
							"ODSDataSource"
							{
								$targetDatasourceRef = $env.ReportsODSDataSouce
							}
							"DWHDataSource"
							{
								$targetDatasourceRef = $env.ReportsDWHDataSource
							}
						}
						
						if($targetDatasourceRef -ne "")
						{
							# Change the datasource for the report to $targetDatasourceRef
							# Note that we can access the types such as DataSource with the prefix
							# "SSRS" only because we specified that as our namespace when we
							# created the proxy with New-WebServiceProxy.
							$proxyNamespace = $rs.GetType().Namespace

							$myDataSource = New-Object ("$proxyNamespace.DataSource")
							$myDataSource.Name = $referencedDataSourceName
							$myDataSource.Item = New-Object ("$proxyNamespace.DataSourceReference")
							$myDataSource.Item.Reference = $targetDatasourceRef

							$rs.SetItemDataSources($report.Path,  $myDataSource)
						}
					}

                    $uploadSuccess = $uploadSuccess + 1
                }
                catch [Exception]
                {
                    LogWrite("    " + "Error uploading report " + $reportName +" to " + $targetFolderPath + ". Error: " + $_.Exception.Message) -ForeGroundColor Red
                }
                finally
                {
                    $index = $index  + 1
                    $percentage = [int](($index / $total) * 100)
                    Write-Progress -Activity ($activityDescription) -PercentComplete $percentage -CurrentOperation    "$percentage% complete" -Status "Please wait..."
                }
            }

                Write-Progress -Activity $activityDescription -Completed -Status "Reports has been uploaded to server."

            if ($uploadSuccess -eq 0)
            {
                LogWrite (" *  " + $uploadSuccess + " of " + $reports.Count   + " Reports has been uploaded to server! Errors occurred. ") -foregroundColor Red
            }
            else
            {
                if ($uploadSuccess -eq $reports.Count)
                {
                    LogWrite (" *  " + $uploadSuccess + " of " + $reports.Count   + " Reports has been uploaded to server!") -foregroundColor Green
                }
                else
                {
                    LogWrite (" *  " + $uploadSuccess + " of " + $reports.Count   + " Reports has been uploaded to server!") -foregroundColor Yellow
                }
            }
        }
        finally
        {
            $d = $rs.Dispose()
        }
    }
    else
    {
        LogWrite (" *  Error getting access to report server. No reports has been uploaded ") -foregroundColor Red
    }
}

<#
    .SYNOPSIS
        Installs reports within a given folder

    .DESCRIPTION
        Installs MES Reports.
        If already existing, the report will be replaced. Also, the datasource of the report will be updated.

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER folderPath
        Folder where the reports are located.

    .EXAMPLE
       Install-ReportsInFolder -env $env -folderPath $reportsFolderPath

    .NOTES
        If Report folder does not exist, it will be created.
        The datasource and report will be overwritten if send more than one time
#>
function Install-ReportsInFolder
{
     param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # Base folder path of the reports to load
        [Parameter(Mandatory=$True)]
        [string]$folderPath
    )

    Use-LightBusinessObjects $env

    if(Test-Path ($folderPath))
    {
        # this is to convert relative path into full path
        $folderPath = Resolve-Path $folderPath
        $folderPath = "$folderPath"     # convert back to string

        LogWrite (" >  Loading Reports from folder $($folderPath)...")

        $reports = @()

        $reportFiles = Get-ChildItem -Path $folderPath -Filter '*.rdl' -Recurse
        LogWrite ("    Number of reports found to deploy: $($reportFiles.Count)")
        if ($reportFiles.Count -gt 0)
        {
            $dataSourceNames = @("Online", "ODS", "DWH")
            foreach ($reportFile in $reportFiles)
            {
                #region Resolve Report Folder
                $reportFolder = ""
                $localPathStartIndex = $folderPath.Split("\").Count

                $folder = $folderPath.Split("\")[$localPathStartIndex - 1]

                $print = "" + $reportFile.FullName
                $splitedPath = $print.Split("\")

                $fileIndex = ($splitedPath.Count - 1)

                $foundFolder = $false
                for ($i=0; $i -lt $fileIndex; $i++)
                {
                    if ( $foundFolder -eq $true ) {
                        $reportFolder += "/" + $splitedPath[$i]
                    }

                    if ( $splitedPath[$i] -eq $folder ) {
                        $foundFolder = $true
                    }
                }
                if ($reportFolder -eq "") {
                    LogWrite (" *  Could not resolve ReportFolder") -ForegroundColor Yellow
                }
                #endregion

                #region Resolve report data source (database)
                [xml]$report = Get-Content -Path $reportFile.FullName
                $dataSources = $report.Report.DataSources.DataSource.Name
                #default data source ODS
                $dataSourceName = $dataSourceNames[1];
                foreach($dataSource in $dataSources)
                {
                    if($dataSource.Contains($dataSourceNames[1]))
                    {
                        $dataSourceName = $dataSourceNames[1]
                    }
                    elseif($dataSource.Contains($dataSourceNames[2]))
                    {
                        $dataSourceName = $dataSourceNames[2]
                    }
                    else
                    {
                        $dataSourceName = $dataSourceNames[0]
                    }
                }
                #endregion
                $reports += New-Report -reportPath $reportFile.FullName -database "$dataSourceName" -reportFolder $reportFolder
            }
            Install-Reports -env $env -reports $reports
        }
        Install-ReportServerResources -env $env -folder $folderPath
    } else {
        LogWrite (" *  Folder $($folderPath) does not exist...") -foregroundColor Yellow
    }
}

<#
    .SYNOPSIS
        Installs resources in report server

    .DESCRIPTION
        Install all resources in a specific folder. If already existing, the resource will be replaced.

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER folder
        Folder containing the resources to be added.

    .EXAMPLE
       Install-ReportServerResources -env $env -folder ".\Reports"

       Where .\Reports structure is:
            .\Reports\_Images\Logo.jpg
            .\Reports\_Images\Logo.jpg

    .NOTES
        If resource folder does not exist, it will be created.
        The resource will be overwritten if send more than one time
#>
function Install-ReportServerResources
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # Folder
        [Parameter(Mandatory=$True)]
        [string]$folder
    )

    # exclude reports [use Install-Reports instead]
    # TODO: maybe install everything in this function (?)
    if(Test-Path ($folder))
    {
        # this is to convert relative path into full path
        $folder = Resolve-Path $folder
        $folder = "$folder"     # convert back to string

        LogWrite (" >  Loading Report Resources from folder $($folder)...")
        $resourceFiles = Get-ChildItem -Path $folder -Exclude '*.rdl' -Recurse -File
        LogWrite ("    Number of report resources found to deploy: " + $resourceFiles.Count)

        if ($resourceFiles.Count -gt 0)
        {
            Use-LightBusinessObjects $env

            LogWrite " >  Starting adding resources to report server..."

            $reportServerUri = $env.ReportServerUri +  "/ReportService2010.asmx?wsdl"

            if ($env.ReportUseDefaultCredential)
            {
                $rs = New-WebServiceProxy -Uri $reportServerUri -UseDefaultCredential
            }
            else
            {
                $username = $env.ReportServerUser
                $password = $env.ReportServerPassword
                $credentials = New-Object System.Management.Automation.PSCredential -ArgumentList @($username,(ConvertTo-SecureString -String $password -AsPlainText -Force))
                $rs = New-WebServiceProxy -Uri $reportServerUri -Credential $credentials
            }
            if ($rs -ne $null)
            {

                $warnings = $null
                try
                {
                    $activityDescription = "Adding resources to Report server..."
                    LogWrite ("    " + $activityDescription)
                    $total = $resourceFiles.Count
                    $index = 0
                    Write-Progress -Activity ($activityDescription) -PercentComplete 0 -CurrentOperation    "0% complete" -Status "Please wait..."
                    $uploadSuccess = 0;

                    # Content MIME Types Map
                    $KnownMimeTypes = @{
                        ".jpg"  = "image/jpeg";
                        ".jpeg" = "image/jpeg";
                        ".gif"  = "image/gif";
                        ".png"  = "image/png";
                        ".tiff" = "image/tiff";
                        ".zip"  = "application/zip";
                        ".json" = "application/json";
                        ".xml"  = "application/xml";
                        ".rar"  = "application/x-rar-compressed";
                        ".gzip" = "application/x-gzip";
                    }


                    [Cmf.Foundation.BusinessOrchestration.ConfigurationManagement.InputObjects.GetConfigByPathInput] $inputGetConfigPath =  New-Object Cmf.Foundation.BusinessOrchestration.ConfigurationManagement.InputObjects.GetConfigByPathInput
                    $reportRootFolder = ""
                    try
                    {
                        $inputGetConfigPath.Path = "/Cmf/System/Configuration/Reporting/Reports Root Folder/"
                        $resultGetConfigPath = $inputGetConfigPath.GetConfigByPathSync()
                    }
                    catch [Exception]{
                        Write-Host $_.Exception
                    }

                    if ($resultGetConfigPath.Config)
                    {
                        $reportRootFolder = $resultGetConfigPath.Config.Value
                    }

                    $folder = Resolve-Path $folder
                    foreach($resourceFile in $resourceFiles)
                    {
                        LogWrite ( $resourceFile.FullName )
                        try
                        {
                            $resourceName = [System.IO.Path]::GetFileName($resourceFile.FullName)
                            $bytes = [System.IO.File]::ReadAllBytes($resourceFile.FullName)
                            $file = Get-ChildItem $resourceFile.FullName
                            $fileMimeType = $KnownMimeTypes[$file.Extension.ToLower()];

                            $localPathStartIndex = $folder.Split("\").Count

                            $splitedPath = ("" + $resourceFile.FullName).Split("\")

                            $fileIndex = ($splitedPath.Count - 1)

                            $resourceFolder = $reportRootFolder

                            $i = $localPathStartIndex
                            while ($i -lt $fileIndex)
                            {
                                $resourceFolder += "/" + $splitedPath[$i]
                                $i++
                            }

                            $targetFolderPath = $resourceFolder
                            $percentage = [int](($index / $total) * 100)
                            Write-Progress -Activity ($activityDescription) -PercentComplete $percentage -CurrentOperation  "$percentage% complete" -Status (($index + 1).ToString() + " of " + $total.ToString() + ". Uploading resource " + $resourceName +" to " + $targetFolderPath + "...    Please wait...")

                            LogWrite("    " + "Uploading resource " + $resourceName +" to " + $targetFolderPath + "...")
                            $x = $resourceFolder.Split('/')
                            $i = 0;
                            if ($x.Count -ge 1)
                            {
                                $reportFolder = "/"

                                while ($i -lt $x.Count)
                                {
                                    #Check if folder is existing, create if not found
                                    try
                                    {
                                        $i = $i + 1
                                        if ($x[1] -ne "")
                                        {
                                                $reportPath = $x[$i]
                                                $reportFolderCopy = $reportFolder;
                                                if ($i -eq 1)
                                                {
                                                    $reportFolder = $reportFolder + $reportPath
                                                }
                                                else
                                                {
                                                    $reportFolder = $reportFolder + "/" + $reportPath
                                                }
                                                #need to do this first, because the parent folder may exists, but the child don't
                                                $ignoreMessage = $rs.CreateFolder($reportPath,$reportFolderCopy, $null)

                                        }
                                    }
                                    catch [System.Web.Services.Protocols.SoapException]
                                    {
                                        if (-not( $_.Exception.Detail.InnerText -match "[^rsItemAlreadyExists400]"))
                                        {
                                            $msg = "[Install-SSRSRDL()] Error creating folder: $reportFolder. Msg: '{0}'" -f $_.Exception.Detail.InnerText
                                            throw $msg
                                        }
                                    }
                                }
                            }

							
						$report = $rs.CreateCatalogItem(		
							"Resource",												# Catalog item type
							$resourceName,											# resource name
							$targetFolderPath,										# Destination folder
							$true,													# Overwrite resource if it exists?
							$bytes,													# file content
							@{'Name'='MimeType'; 'Value'=$fileMimeType},            # Properties to set.
							[ref]$warnings											# Warnings that occured while uploading.
						)

                            $uploadSuccess = $uploadSuccess + 1

                        }
                        catch [Exception]
                        {
                            LogWrite("    " + "Error uploading report " + $reportName +" to " + $targetFolderPath + ". Error: " + $_.Exception.Message) -ForeGroundColor Red
                        }
                        finally
                        {
                            $index = $index  + 1
                            $percentage = [int](($index / $total) * 100)
                            Write-Progress -Activity ($activityDescription) -PercentComplete $percentage -CurrentOperation    "$percentage% complete" -Status "Please wait..."
                        }
                    }

                    Write-Progress -Activity $activityDescription -Completed -Status "resources has been uploaded to server."

                    if ($uploadSuccess -eq 0)
                    {
                        LogWrite (" *  " + $uploadSuccess + " of " + $total   + " resources has been uploaded to server! Errors occurred. ") -foregroundColor Red
                    }
                    else
                    {
                        if ($uploadSuccess -eq $total)
                        {
                            LogWrite (" *  " + $uploadSuccess + " of " + $total   + " resources has been uploaded to server!") -foregroundColor Green
                        }
                        else
                        {
                            LogWrite (" *  " + $uploadSuccess + " of " + $total   + " resources has been uploaded to server!") -foregroundColor Yellow
                        }
                    }
                }
                finally
                {
                    $d = $rs.Dispose()
                }
            }
            else
            {
                LogWrite (" *  Error getting access to report server. No resources has been uploaded ") -foregroundColor Red
            }
        }
    } else {
        LogWrite (" *  Folder $($folder) does not exist...") -foregroundColor Yellow
    }
}

<#
    .SYNOPSIS
        Remove cmNavigo Reports

    .DESCRIPTION
        Remove one or more cmNavigo Reports.

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER reports
        Array containing the reports to be added.

    .EXAMPLE
       Remove-Reports -env $env -reports $reportsToRemove

    .NOTES
        If thr Report does not exist an exception will be thrown
#>

function Remove-Reports
{
     param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # Reports to Remove
        [Parameter(Mandatory=$True)]
        [Object[]]$reports

    )

    LogWrite " >  Starting removing Reports from server..."

    $reportServerUri = $env.ReportServerUri +  "/ReportService2010.asmx?wsdl"

    if ($env.ReportUseDefaultCredential)
    {
        $rs = New-WebServiceProxy -Uri $reportServerUri -UseDefaultCredential
    }
    else
    {
        $username = $env.ReportServerUser
        $password = $env.ReportServerPassword
        $credentials = New-Object System.Management.Automation.PSCredential -ArgumentList @($username,(ConvertTo-SecureString -String $password -AsPlainText -Force))
        $rs = New-WebServiceProxy -Uri $reportServerUri -Credential $credentials
    }
    if ($rs -ne $null)
    {

        $warnings = $null
        try
        {
            $activityDescription = "Removing reports to Report server..."
            LogWrite ("    " + $activityDescription)
            $total = $reports.Count
            $index = 0
            Write-Progress -Activity ($activityDescription) -PercentComplete 0 -CurrentOperation    "0% complete" -Status "Please wait..."
            $uploadSuccess = 0;
            foreach($reportInfo in $reports)
            {
                try
                {
                    $reportName = [System.IO.Path]::GetFileNameWithoutExtension($reportInfo.ReportPath)
                    $percentage = [int](($index / $total) * 100)
                    Write-Progress -Activity ($activityDescription) -PercentComplete $percentage -CurrentOperation  "$percentage% complete" -Status (($index + 1).ToString() + " of " + $total.ToString() + ". Removing report " + $reportName +" to " + $targetFolderPath + "...    Please wait...")

                    LogWrite("    " + "Remove report " + $reportInfo + "...")

                    $rs.DeleteItem(
                        ($reportInfo)
                    )
                }
                catch [Exception]
                {
                    LogWrite("    " + "Error removing report " + $reportName +" to " + $reportInfo + ". Error: " + $_.Exception.Message) -ForeGroundColor Red
                }
                finally
                {
                    $index = $index  + 1
                    $percentage = [int](($index / $total) * 100)
                    Write-Progress -Activity ($activityDescription) -PercentComplete $percentage -CurrentOperation    "$percentage% complete" -Status "Please wait..."
                }
            }
        }
        finally
        {
            $d = $rs.Dispose()
        }
    }
    else
    {
        LogWrite (" *  Error getting access to report server. No reports has been removed ") -foregroundColor Red
    }
}



<#
     .SYNOPSIS
        Creates and optionally executes post installation actions

    .DESCRIPTION
        Creates and optionally executes post installation actions inside a folder

     .PARAMETER folderPath
        Folder containing the rules to be executed

    .PARAMETER executeRules
        Flag to control  if rules are to executed

    .PARAMETER deleteRules

    .PARAMETER continueOnError

    .EXAMPLE
       LoadProcessRules $env  -folderPath 'c:\package\database\online'
       LoadProcessRules $env  -folderPath 'c:\package\database\online' -continueOnError $true
#>
function LoadProcessRules()
{
     param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        #directory  to enumerate the files
        [Parameter(Mandatory=$True)]
        $folderPath,

        [Parameter(Mandatory=$False)]
        $executeRules = $true,

        [Parameter(Mandatory=$False)]
        $deleteRules = $true,

        [Parameter(Mandatory=$False)]
        $continueOnError = $false
    )

    if( Test-Path $folderPath )
    {
        Use-LightBusinessObjects $env
        [Reflection.Assembly]::LoadFrom((Resolve-Path "$ModulePath\References\MasterData.Logic.dll")) | Out-Null

       $listRules = Get-ChildItem $folderPath -Filter '*.cs' | Sort-Object Name
       foreach($file in $listRules )
       {
           $ruleName = $file.Name.Replace(".cs","")
           try
           {
                LogWrite "Creating rule $ruleName..."
                $action = [MasterData.Logic.DEEActionLoader]::InsertAction($file.FullName,  $ruleName, $false)

               if( $executeRules)
               {
                    LogWrite "Executing rule $ruleName..."

                    [Cmf.Foundation.BusinessOrchestration.DynamicExecutionEngineManagement.InputObjects.ExecuteActionInput] $executeInput =  New-Object Cmf.Foundation.BusinessOrchestration.DynamicExecutionEngineManagement.InputObjects.ExecuteActionInput
                    $executeInput.Action =   [MasterData.Logic.DEEActionLoader]::GetActionByName($ruleName)
                    $executeInput.ExecuteActionSync();
               }

               if( $deleteRules)
               {
                   LogWrite "Deleting rule $ruleName..."

                    [Cmf.Foundation.BusinessOrchestration.DynamicExecutionEngineManagement.InputObjects.RemoveActionInput] $removeInput =  New-Object Cmf.Foundation.BusinessOrchestration.DynamicExecutionEngineManagement.InputObjects.RemoveActionInput
                    $removeInput.Action =  [MasterData.Logic.DEEActionLoader]::GetActionByName($ruleName)
                    $removeInput.RemoveActionSync()
               }
            }
           catch [Exception]
            {
                if($continueOnError){
                    LogWrite "Error on rule  $ruleName  $_.Exception.Message " -foregroundColor Red
                    if($global:InteractiveMode -eq "" -Or $global:InteractiveMode -eq $true){
                        Write-Host $_.Exception -ForegroundColor Red|format-list -force
                    }
                }else{
                    throw $_.Exception
                }
            }
       }
    }
}


<#
    .SYNOPSIS
        Sets the customization version string inside the cmNavigo database and in AppServer HTML config file

    .DESCRIPTION
        Sets the customization version string inside the cmNavigo database and in AppServer HTML config file

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.
        
    .PARAMETER version
        The version number, in free text
        
    .PARAMETER appServerOnly
        When used the customization version will be set only on HTML config

    .EXAMPLE
        Set-CustomizationVersion $env -version 'Sprint 52'

    .EXAMPLE
        Set-CustomizationVersion $env -version '4.2.1.0'

    .EXAMPLE
        Set-CustomizationVersion $env -version '4.2.1.0' -appServerOnly
#>
function Set-CustomizationVersion
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject] $env,

        # String
        [Parameter(Mandatory=$True)]
        [string] $version,
        
        # Indicates if the version is to be apply on Application Servers only
        [Parameter(Mandatory=$False)]
        [switch] $appServerOnly
    )

    if ( -not $appServerOnly ) {    
        # Sets Customization Version on database
        $filePath =  '.\Customization.sql';
        $scriptContent = "EXEC [dbo].[P_SetCustomizationVersion] @ApplicationName = N'cmNavigo', @Version = '"+$version+"'"

        LogWrite " *  Setting customization version $version"

        Set-Content -path $filePath  -value $scriptContent
        try {
            Invoke-SQLScript $env -database 'Online' -scriptPath $filePath
        }
        finally{
            Remove-Item $filePath
        }
	}
    
    # Sets Customization Version on Application Server HTML Config file
    foreach ( $server in $env.ApplicationServers ) {
        $targetInstallation = ('\\' + $server.ServerName + '\' +  $server.InstallationPath + '\' + $CMFHTML) -replace ':', '$';
        $configFile = Join-Path -Path $targetInstallation -ChildPath "\config.json";

        # Update config.json
        if(-not (Test-Path $configFile)){
            LogWrite " !  config.json not found" -foregroundColor Red
        } else { 
            $jsonObject = Get-Content -Raw -Path $configFile | ConvertFrom-Json;

            LogWrite "    Updating config.json"
            if (($jsonObject.PSobject.Properties | Foreach {$_.Name}) -contains "customizationVersion") {
                $jsonObject.customizationVersion = $version;
            } else {
                $jsonObject | add-member -Name "customizationVersion" -value $version -MemberType NoteProperty
            }

            # Save config.json
            $jsonObject | ConvertTo-Json -Depth 10 | Out-File $configFile;
        }
    }
}



<#
    .SYNOPSIS
        Replaces tokens inside the files,  including all subfolders items.

    .DESCRIPTION
        Replaces tokens inside the files,  including  all  subfolder

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER path
        The folder to search to items

    .PARAMETER tokens
        Hashtable with the  token and values to replace

    .PARAMETER filters
        Specifies a filter in items

    .EXAMPLE
        Set-ScriptTokens $env -path 'Analytics' -filters @('*.sql','*.xmla') -tokens @{ '$(DWHInstance)' = $env.DWHClusterInstance; '$(DWHUser)' = $env.SQLDWHUser;'$(DWHUserPasswd)' = $env.SQLDWHPassword;    }
#>
function  Set-ScriptTokens()
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # Path
        [Parameter(Mandatory=$True)]
        [string] $path,


        [Parameter(Mandatory=$True)]
         $tokens,

        [Parameter(Mandatory=$False)]
         $filters
        )

    Get-ChildItem -path $path -Include $filters -Recurse | ForEach-Object {

         $fileName = $_.FullName
         $tmp =  $fileName + ".tmp"
         $tokenCount = 0
         if($global:InteractiveMode -eq "" -Or $global:InteractiveMode -eq $true){
            Write-Host "Found $fileName"
         }
        Get-Content  $fileName | ForEach-Object {
            $line = $_
             $tokens.GetEnumerator() | ForEach-Object {
                  if( $line.Contains($_.Key) ){
                    $line = $line.Replace($_.Key, $_.Value)
                    $tokenCount++;
                  }
            }
            $line
        } | Set-Content $tmp


        Move-Item $tmp $fileName -force

        if( $tokenCount -gt 0 ){
                if($global:InteractiveMode -eq "" -Or $global:InteractiveMode -eq $true){
                    Write-Host "Replaced $tokenCount token(s) at $fileName"
                }
        }
    }

    LogWrite (" *  Finished replacing tokens") -foregroundColor Green
}


function TransformXmlFile($src,$xdt,$dst)
{
    [Reflection.Assembly]::LoadFrom((Resolve-Path "$ModulePath\References\Microsoft.Web.XmlTransform.dll")) | Out-Null

  $doc = New-Object Microsoft.Web.XmlTransform.XmlTransformableDocument
  $doc.PreserveWhiteSpace = $true
  $doc.Load($src)

  $trn = New-Object Microsoft.Web.XmlTransform.XmlTransformation($xdt)

  if ($trn.Apply($doc))
  {
   $doc.Save($dst)
   if($global:InteractiveMode -eq "" -Or $global:InteractiveMode -eq $true){
    Write-Output "Output file: $dst"
    Write-Host 'applyConfigTransformation - $trn.Apply completed'
   }
  }
  else
  {
   throw ("Transformation " + $src + " with  "  + $xdt + " terminated with status False")
  }
}

<#
    .SYNOPSIS
        Transforms the business configuration files, using  XML Document Transformation

    .DESCRIPTION
        Transforms the business configuration files (Cmf.Foundation.Services.HostService.exe.config and Cmf.Foundation.Services.HostConsole.exe.config ) using  the XML Document Transformation
        Requires the reference Microsoft.Web.XmlTransform.dll

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER transformFile
        The file with  XDT instructions to apply

    .PARAMETER filters
        Specifies a filter in items

    .PARAMETER applicationServer
        If defined, transformation will only be applied to the specified application server.

    .EXAMPLE
        XDT sample
        <configuration xmlns:xdt="http://schemas.microsoft.com/XML-Document-Transform">
                <appSettings>
                        <add key="SomeKey"  xdt:Transform="InsertIfMissing" xdt:Locator="Match(key)" value="SomeValue"/>
                </appSettings>
        </configuration>
        Install-CMFBusinessConfigTransformation $env -transformFile 'changeconfig.xdt'

    .EXAMPLE
        Install-CMFBusinessConfigTransformation $env -transformFile 'changeconfig.xdt'  -applicationServer $server1

#>
function Install-CMFBusinessConfigTransformation()
{
     param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # transformFile
        [Parameter(Mandatory=$True)]
        [string] $transformFile,

        [Parameter(Mandatory=$False)]
         $applicationServer
        )

    $fileNames = @("Cmf.Foundation.Services.HostService.dll.config")

    LogWrite " >  Executing host configuration file transformation to $transformFile" -foregroundColor Green

    # Get servers to use
    $serversToUse = @()
    if ($applicationServer)
    {
        $serversToUse += $applicationServer
    }
    else
    {
        $serversToUse = $env.ApplicationServers
    }


    if($global:InteractiveMode -eq "" -Or $global:InteractiveMode -eq $true){
        Write-Host "Starting transforming config files"
    }

    $source = $filesPath
    foreach($appServer in $serversToUse)
    {
        $target = '\\' + $appServer.ServerName + '\' + $appServer.BusinessInstallationPath -replace ':', '$'

        foreach($fileName in $fileNames)
        {
           $configFile =  Join-Path $target  $fileName

          TransformXmlFile $configFile $transformFile $configFile
        }
    }

    LogWrite (" * Finished transforming config") -foregroundColor Green
}

<#
    .SYNOPSIS
        Installs customization packages on HTML5 Presentation

    .DESCRIPTION
        Copies packages to the node_modules of HTML5 installation.
        Deals with config.json

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER packageFolder
        Absolute Path to the packages to install.

    .PARAMETER prefix
        If defined, it will look for packages that begins with the given prefix. Default value is customization.

    .PARAMETER applicationServer
        Application server path

    .PARAMETER target
        Target path to install. I.e. UI\Help OR UI\HTML
        By default, it uses the UI\HTML.

    .PARAMETER doNotRemovePackages
        When $true, it will not remove the delivered packages.
		This can be used to deliver partially a package, or when a PrivateFix from the ProductTeam needs to be deployed.
		By default, it will remove the package before delivering it.

    .PARAMETER isConnectIoTPackages
        When $true, it will handle them as connect iot packages.
        When not connect iot, the packages will be added to the config under the packages section also a two level package (@mainPackage/subPackage) is expected;
		When connect iot, the packages will be added to the config under the connectiot section;

    .EXAMPLE
        Install-CMFHTMLPackages $env -packageFolder ./package/html5 -applicationServer "./html"

    .NOTES
        Requires administrative privileges on the application servers.
#>
function Install-CMFHTMLPackages()
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # Path to the folder with the customization packages
        [Parameter(Mandatory=$True)]
        [string]$packageFolder,

        # Prefix of the packages (Default is customization)
        [Parameter(Mandatory=$False)]
        [string] $prefix="customization",

        # Application Server Path
        [Parameter(Mandatory=$False)]
        [string] $applicationServer,

        # target ($CMFHTML / $CMFHelp) (Default is $CMFHTML)
        [Parameter(Mandatory=$False)]
        [string] $target,
        
        # do not remove packages (being delivered)
        [Parameter(Mandatory=$False)]
        [switch] $doNotRemovePackages,
        
        # is connect iot packages
        [Parameter(Mandatory=$False)]
        [switch] $isConnectIoTPackages        
    )

     # Get servers to use
    $serversToUse = @()
    if ($applicationServer)
    {
        $serversToUse += $applicationServer
    }
    else
    {
        $serversToUse = $env.ApplicationServers
    }

    # default target to $CMFHTML
    if (-not $target) {
        $target = "$CMFHTML"
    }

    foreach($server in $serversToUse)
    {
        #declare some paths
        $targetInstallation = ('\\' + $server.ServerName + '\' +  $server.InstallationPath + '\' + $target) -replace ':', '$';
		$mainPackageList = @();
		$packagesToInstall = @();
        $nodeModulesFolder = Join-Path -Path $targetInstallation -ChildPath "node_modules";
        $configFile = Join-Path -Path $targetInstallation -ChildPath "\config.json";
        $indexHtmlFile = Join-Path -Path $packageFolder -ChildPath "\index.html";
		
        # Get the packages to Install
        # Get only directories, as sometimes .gitkeep is present here
        Get-ChildItem -Path:$packageFolder -Directory | ForEach-Object {
			$mainPackageName =$_.Name;
			if ( $isConnectIoTPackages ) {
				$mainPackageFullPath = $_.FullName;
				$mainPackageList += $mainPackageName
				Get-ChildItem -Path "$mainPackageFullPath" -Directory | ForEach-Object {
					$packagesToInstall += "$mainPackageName/$($_.Name)"
				}
			}
			else {
				$packagesToInstall += $mainPackageName;
			}
        }
			
		if ($packagesToInstall.length -gt 0) {
			LogWrite "    Packages to install:"
			foreach ( $packageName in $packagesToInstall) {
				LogWrite "    - $packageName"
			}
        
			if (-not $doNotRemovePackages) {
				LogWrite "    Removing old packages"
				# Remove old customization packages
				Get-ChildItem $nodeModulesFolder | ForEach-Object {
					if ( $isConnectIoTPackages ) {
						if ( $mainPackageList.Contains($_.Name) ) {
							$mainPackageName = $_.Name;
							Get-ChildItem -Path "$($_.FullName)" -Directory | ForEach-Object {
								if ( $packagesToInstall.Contains("$mainPackageName/$($_.Name)") ) {
									LogWrite "    - Removed package $mainPackageName/$($_.Name)"
									Remove-Item $_.FullName -Force -Recurse;
								}
							}
						}
					}
					else {
						if ( $packagesToInstall.Contains($_.Name) ) {
							LogWrite "    - Removed package $($_.Name)"
							Remove-Item $_.FullName -Force -Recurse;
						}
					}
				}
			}
			
			# Copy packages to node_modules folder
            LogWrite "    Copying packages"
            Get-ChildItem -Path:$packageFolder -Directory | ForEach-Object {
			
				$mainFolderName = $($_.Name);
				
				if ( $isConnectIoTPackages ) {
				
					Get-ChildItem -Path "$($_.FullName)" -Directory | ForEach-Object {
					
						if($_ -like "*driver*" -and $_ -notlike "*task*") {
							LogWrite "  Is an IoT Driver so will not copy"
						} else {
							LogWrite "   Is not IoT Driver so will copy"
							$subDir = $_.FullName.Split('\') | select -Last 1;
							$destDir = -join("$nodeModulesFolder","\","$mainFolderName","\", "$_");
							Robocopy $_.FullName "`"$destDir"`" /S /IS | Out-Null;
						}
					
					}
				
				} else {
					$destDir = -join("$nodeModulesFolder","\",$_.Name);
					Robocopy $_.FullName "$destDir" /S /IS | Out-Null;
				}
			}
			
		
			# Update config.json
			if(-not (Test-Path $configFile)){
			   LogWrite " !  config.json not found" -foregroundColor Red
			}
			$jsonObject = Get-Content -Raw -Path $configFile | ConvertFrom-Json;

			LogWrite "    Updating config.json"
			# Add packages
			if (-not $isConnectIoTPackages) {				
				$packagesList = New-Object System.Collections.Generic.List[System.String];
				$jsonObject.packages.available | ForEach-Object {
					$packagesList.Add($_);
				}
				# Add new packages
				foreach ($packageToInstall in $packagesToInstall) {
					if (-not $packagesList.Contains($packageToInstall)) {
						LogWrite "    Adding $packageToInstall to config.json"
						$packagesList.Add($packageToInstall);
					}
				}
				
				$jsonObject.packages.available = $packagesList.ToArray();
			}
			else {			
				$packagesList = @()
				$jsonObject.connectiot.packages | ForEach-Object {
					$packagesList += $_;
				}
				
				foreach ($packageToInstall in $packagesToInstall) {
					if (-not ($packagesList.path -contains $packageToInstall)) {
                        if($packageToInstall -like "*driver*" -and $packageToInstall -notlike "*task*") {
                            LogWrite "    Will not add $packageToInstall to config.json (connectIot), because it's a Driver"
                        } else {
                            LogWrite "    Adding $packageToInstall to config.json (connectIot)"
                            $packagesList += ([PSCustomObject]@{ path = $packageToInstall })
                        }
					}
				}
				
				$jsonObject.connectiot.packages = $packagesList;
			}
			
			# Update config.json cacheId
			$now = Get-Date;
			$randomSuffix = "" + (Get-Random -Maximum 99999 -Minimum 10000);
			$jsonObject.cacheId = "" + $now.Year + $now.Month + $now.Day + $now.Hour + $now.Minute + $randomSuffix;

			# Save config.json
			$jsonObject | ConvertTo-Json -Depth 10 | Out-File $configFile;
			
			# Update with index.html
			if(-not (Test-Path $indexHtmlFile)){
			    LogWrite "    No index.html to publish"
			} else {
				LogWrite "    Deal with index.html"
				Copy-Item $indexHtmlFile $targetInstallation -recurse -force
			}
		}
		else {
			LogWrite "    No packages found to install..."
		}
		LogWrite (" *  $($target) installation in " + $server.ServerName  + " completed") -foregroundColor Green;
    }
}

<#
    .SYNOPSIS
        Creates a new object to represent a CM MES Config

    .DESCRIPTION
        Returns a new custom object which contains all the required configuration properties create or update a configuration entry
        cmNavigo. Should be used in conjunction with Set-Config

    .PARAMETER parentPath
        The Config path

    .PARAMETER name
        The Config name

    .PARAMETER value
        The Config value

    .PARAMETER valueType
        The Config value type. This parameter is required when creating a new config. It can take the following values:
        Boolean, Decimal, Double, Int16, Int32, Int64, String

    .EXAMPLE
        New-Config -parentPath '/Cmf/System/Configuration/ERP/' -name 'IsActive' -Value 'TRUE'
		
    .OUTPUTS
        A custom object representing the CM MES Config. The object contains the following properties:
        - ParentPath:	The config parent path
		- Name:			Name of the Config
		- Value:		The value of the config
		- ValueType:	Teh data type of the config
#>
function New-Config()
{
    param
    (
        [Parameter(Mandatory=$True)]
        [string] $parentPath,

        [Parameter(Mandatory=$True)]
        [string] $name,

        [Parameter(Mandatory=$True)]
        [string] $value,

        [Parameter(Mandatory=$False)]
        [string] $valueType
    )

    $config = new-object PSObject

    $config | add-member -type NoteProperty -Name ParentPath -Value $parentPath
    $config | add-member -type NoteProperty -Name Name -Value $name
    $config | add-member -type NoteProperty -Name Value -Value $value
    $config | add-member -type NoteProperty -Name ValueType -Value $valueType

    return $config
}

<#
    .SYNOPSIS
        Creates or updates a CM MES configuration entry

    .DESCRIPTION
        Creates and/or saves one or more CM MES Configs. If the execution fails, a message will be sent to user, but the process continues.

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER configs
        Array containing the Configs to create and/or save. Each Config should be a custom object created with the function New-Config
        function.

    .EXAMPLE
       Execute-DEEAction $env -configs $configs

#>
function Set-Config()
{

      param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # Configs to create and/or save
        [Parameter(Mandatory=$True)]
        [Object[]]$configs
    )

    LogWrite " >  Starting create and/or save of Configs..."

    $totalConfigsToCreateOrSave = $configs.Count
    $createdSavedConfigs = 0;
    $activityDescription = 'Create or Save of Configs'

    Write-Progress -Activity ($activityDescription) -PercentComplete 0 -CurrentOperation    "0% complete" -Status "Please wait..."


    Use-LightBusinessObjects $env
    $index = 0

    foreach($config in $configs)
    {
        $percentage = [int](($index / $totalConfigsToCreateOrSave) * 100)
        Write-Progress -Activity ($activityDescription) -PercentComplete $percentage -CurrentOperation  "$percentage% complete" -Status (($index + 1).ToString() + " of " + $totalConfigsToCreateOrSave.ToString() + ". Creating/saving the config " + $config.Name + " at " + $config.ParentPath + "...    Please wait...")

        try
        {

            [Cmf.Foundation.Configuration.Config] $configuration = New-Object Cmf.Foundation.Configuration.Config
            $configuration.Name = $config.Name
            $configuration.Value = $config.Value
            if($config.ValueType)
            {
                $configuration.ValueType = ("System." + $config.ValueType)
            }

            [Cmf.Foundation.BusinessOrchestration.ConfigurationManagement.InputObjects.GetConfigByPathInput] $inputGetConfigPath =  New-Object Cmf.Foundation.BusinessOrchestration.ConfigurationManagement.InputObjects.GetConfigByPathInput

            try
            {
                $inputGetConfigPath.Path = ($config.ParentPath + "/" + $config.Name)
                $resultGetConfigPath = $inputGetConfigPath.GetConfigByPathSync()
            }
            catch [Exception]{}


            if ($resultGetConfigPath.Config)
            {
                # Config exists, update it

                $resultGetConfigPath.Config.Name = $configuration.Name
                $resultGetConfigPath.Config.Value = $configuration.Value
                $resultGetConfigPath.Config.ValueType = $configuration.ValueType

                [Cmf.Foundation.BusinessOrchestration.ConfigurationManagement.InputObjects.UpdateConfigInput] $inputUpdateConfig =  New-Object Cmf.Foundation.BusinessOrchestration.ConfigurationManagement.InputObjects.UpdateConfigInput
                $inputUpdateConfig.Config = $resultGetConfigPath.Config
                $resultUpdateConfig = $inputUpdateConfig.UpdateConfigSync()

                LogWrite ("    Config " +  $config.ParentPath + "/" + $config.Name + " was saved correctly.")
            }
            else
            {
               # Config does not exist, create it

                [Cmf.Foundation.BusinessOrchestration.ConfigurationManagement.InputObjects.CreateConfigInput] $inputCreateConfig =  New-Object Cmf.Foundation.BusinessOrchestration.ConfigurationManagement.InputObjects.CreateConfigInput
                $inputCreateConfig.ParentPath = $config.ParentPath
                $inputCreateConfig.Config = $configuration
                $resultCreateConfig = $inputCreateConfig.CreateConfigSync()

                LogWrite ("    Config " + $config.ParentPath + "/" + $config.Name + " was created correctly.")
            }

            $createdSavedConfigs = $createdSavedConfigs + 1;
        }
        catch [Exception]
        {
            LogWrite $_.Exception.Message
        }
        finally
        {
            $index = $index  + 1
            $percentage = [int](($index / $totalConfigsToCreateOrSave) * 100)
            Write-Progress -Activity ($activityDescription) -PercentComplete $percentage -CurrentOperation    "$percentage% complete" -Status "Please wait..."
        }
    }

    Write-Progress -Activity $activityDescription -Completed -Status "Configs have been created/saved."

    if ($createdSavedConfigs -gt 0)
    {
        if ($createdSavedConfigs -eq $totalConfigsToCreateOrSave)
        {
            LogWrite ("    Configs created/saved with success.") -foregroundColor Green
        }
        else
        {
            LogWrite ("    Not all Configs were created/saved with success.") -foregroundColor Yellow
        }
    }
    else
    {
        LogWrite ("    Configs were not created/saved with success." ) -foregroundColor Yellow
    }

    LogWrite (" *  Finished create/save Configs!") -foregroundColor Green
}

<#
    .SYNOPSIS
        Gets a CM MES configuration entry

    .DESCRIPTION
        Return a CM MES Config. If the execution fails, a message will be sent to user, but the process continues.

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER configPath
        Path of the Config to get.

    .EXAMPLE
       Get-Config $env -configPath '/Cmf/System/Configuration/ConnectIoT/RepositoryLocation/'

#>
function Get-Config()
{

      param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # Path of the Config to get
        [Parameter(Mandatory=$True)]
        [string]$configPath
    )

    Use-LightBusinessObjects $env
    try
    {
        [Cmf.Foundation.BusinessOrchestration.ConfigurationManagement.InputObjects.GetConfigByPathInput] $inputGetConfigPath =  New-Object Cmf.Foundation.BusinessOrchestration.ConfigurationManagement.InputObjects.GetConfigByPathInput
        $inputGetConfigPath.Path = $configPath
        $resultGetConfigPath = $inputGetConfigPath.GetConfigByPathSync()

        if ($resultGetConfigPath.Config)
        {
            # Config exists, return it
            return $resultGetConfigPath.Config.Value   
        }
        else
        {
            LogWrite ("    Config $configPath was not found")
        }
    }
    catch [Exception]
    {
        LogWrite $_.Exception.Message
    }
}




<#
    .SYNOPSIS
        Backup the CMF Folder

    .DESCRIPTION
        Makes a backup of the CMF Folder to a zip.

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER backupIdentifier
        Name that identifies the backup

    .PARAMETER folderToBackup
        Application folder to back up

    .PARAMETER applicationServer
        Application server path
		
	.PARAMETER manifest
		Manifest XML file with the content to be backed-up.

    .EXAMPLE
        Backup-CMFFolder $env -backupIdentifier "Sprint01" -folderToBackup "UI\HTML" -applicationServer "./html"

    .NOTES
        Requires administrative privileges on the application servers.
#>
function Backup-CMFFolder()
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # Name that identifies the backup
        [Parameter(Mandatory=$True)]
        [string] $backupIdentifier,

        # Folder to Backup
        [Parameter(Mandatory=$True)]
        [string] $folderToBackup,

        # Application Server Path
        [Parameter(Mandatory=$False)]
        [string] $applicationServer,
		
        # backup manifest
        [Parameter(Mandatory=$False)]
        [xml] $manifest
    )
	
   LogWrite (" >  Starting " + $folderToBackup + " backup...")

   # Get servers to use
   $serversToUse = @()

   if ($applicationServer)
   {
       $serversToUse += $applicationServer
   }
   else
   {
       $serversToUse = $env.ApplicationServers
   }

   foreach($server in $serversToUse)
   {
	   $folderParsed = $folderToBackup.Split("\")[-1]
       $backupPath = Join-Path -Path ($env.BackupLocation) -ChildPath ("\"+$backupIdentifier+"\" + $server.ServerName.Split('.')[0] + "-" + $folderParsed + "-"+$env.SystemName+"-"+$backupIdentifier+".zip")

      $continueBackup = OverwriteBackupFile $backupPath
      
       if($continueBackup -eq $true)
       {
           $sc =
           {
                param($sourcedir, $zipfilename, $activityDescription, $manifest)
                Write-Progress -Activity ($activityDescription) -Status "Please wait..."
                
                # create backup folder if it doesn't exist
				$mainBackupFolder = (Split-Path -parent $zipfilename)
                New-Item -ItemType Directory -Force -Path $mainBackupFolder | Out-Null
                
			    # check if manifest was provided - and identify files and folders to backup
                if ( $manifest )
                {
                    $filesToBackup = @()
                    foreach ( $node in $manifest.BackupManifest.Content.File ) {
                        $filesToBackup += $node
                    } 
    
                    $foldersToBackup = @()
                    foreach ( $node in $manifest.BackupManifest.Content.Folder ) {
                        $foldersToBackup += $node
                    }
                }
                
                # remove previous file if it already exists
                if (Test-Path $zipfilename) { Remove-Item $zipfilename }

                [Reflection.Assembly]::LoadWithPartialName( "System.IO.Compression.FileSystem" ) | Out-Null
                $compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal
				if ( ( -not $filesToBackup ) -and ( -not $foldersToBackup ) ) {
                    # No Files and No Folders to Backup - Full Backup 
					[System.IO.Compression.ZipFile]::CreateFromDirectory( $sourcedir, $zipfilename, $compressionLevel, $false )
				}
				else {
                    # Files or Folders to Backup - Partial Backup

					#region: create temporary folder to copy files to be backed-up
					$tempBackup = "$mainBackupFolder\tmp_backup"
					if ( Test-Path $tempBackup ) { Remove-Item $tempBackup -Force -Recurse }

					New-Item -ItemType Directory -Force -Path "$tempBackup" | Out-Null
					#endregion
					if ( $filesToBackup ) {
						foreach ( $item in $filesToBackup ) {
							$fileToBackup = "$(Join-Path -Path $sourcedir -ChildPath $item)"
							if ( Test-Path -Path "$fileToBackup" ) {
								$itemPath = "$(Split-Path -Path $item)"
								if ( $itemPath -ne '.' ) {
									if ( -not (Test-Path "$tempBackup\$itemPath" ) ) {
										New-Item -Path "$tempBackup" -type Directory -Name $itemPath -Force | Out-Null
									}
								}
								
								$fileName = "$(Split-Path $fileToBackup -Leaf)"
								
								$fileToBackup = "$((Resolve-Path $fileToBackup).Path)"
								$sourceFolder = "$(	Split-Path -Path $fileToBackup)"
								
								robocopy "$sourceFolder" "$tempBackup\$itemPath" $fileName | Out-Null
							}
						}
					}

					if ( $foldersToBackup ) {
						foreach ( $folder in $foldersToBackup ) {
							$sourceFolder = "$(Join-Path -Path $sourcedir -ChildPath $folder)"

							if ( Test-Path -Path "$sourceFolder" ) {
								robocopy "$sourceFolder" "$tempBackup\$folder" /e | Out-Null
							}
						}
					}
                    
					# Save manifest into tempbackup folder to be zipped
                    $manifest.Save( "$tempBackup\BackupManifest.xml" )
                    
                    [System.IO.Compression.ZipFile]::CreateFromDirectory( $tempBackup, $zipfilename, $compressionLevel, $false )
					
					# Delete tempbackup folder after the zip
					if ( Test-Path $tempBackup ) { Remove-Item $tempBackup -Force -Recurse }
				}

                Write-Progress -Activity $activityDescription -Completed -Status "Backup has beeen done."
            }

            $activityDescription  = "Backing up "+$folderToBackup+" in " + $server.ServerName + " ..."
            LogWrite ("    " + $activityDescription )

            $FolderFullPath = (Join-Path -Path $server.InstallationPath -ChildPath ("\"+$folderToBackup))

            $password = Get-SecureStringFromEncryptedString $env.AdminPass
            $cred = new-object -typename System.Management.Automation.PSCredential -argumentlist $env.AdminUser, $password

            $userName = $env.AdminUser
			$password = Get-ClearTextFromEncryptedString $env.AdminPass
			$uri = new-object System.Uri($backupPath)
			$backupServer = "\\"+$uri.host
			$sess = New-PSSession -ComputerName $server.ServerName -Credential $cred
			Enter-PSSession -Session $sess
				Invoke-Command -Session $sess -ScriptBlock { (net use $using:backupServer $using:password /USER:$using:userName) | Out-Null   }
				Invoke-Command -Session $sess -ScriptBlock $sc `
				-ArgumentList @($FolderFullPath,$backupPath, $activityDescription, $manifest)
				Invoke-Command -Session $sess -ScriptBlock { (net use $using:backupServer /delete) | Out-Null   }
			Remove-PSSession -Session $sess
			Exit-PSSession
       }
       else
       {
           LogWrite ("    Backup of "+$folderToBackup+" in " + $server.ServerName + " was bypassed due to previous backup file...") -foregroundColor Yellow
       }
   }

   LogWrite (" *  Finished "+$folderToBackup+" backup!") -foregroundColor Green
}


<#
    .SYNOPSIS
        Restore the Folder

    .DESCRIPTION
        Makes a restore of the Folder installation given a backupFile

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER backupIdentifier
        Name that identifies the backup


    .PARAMETER applicationServer
        Application server path

    .EXAMPLE
        Restore-CMFHTML $env -backupIdentifier "Sprint01" -applicationServer "./html"

    .NOTES
        Requires administrative privileges on the application servers.
#>
function Restore-CMFFolder()
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # Name that identifies the backup
        [Parameter(Mandatory=$True)]
        [string] $backupIdentifier,

        # Folder to Backup
        [Parameter(Mandatory=$True)]
        [string] $folderToBackup,

        # Application Server Path
        [Parameter(Mandatory=$False)]
        [string] $applicationServer
    )

    LogWrite (" >  Starting "+$folderToBackup+" restore...")

    # Get servers to use
    $serversToUse = @()
    if ($applicationServer)
    {
        $serversToUse += $applicationServer
    }
    else
    {
        $serversToUse = $env.ApplicationServers
    }

    foreach($server in $serversToUse)
    {
		$folderParsed = $folderToBackup.Split("\")[-1]
        $backupPath = Join-Path -Path ($env.BackupLocation) -ChildPath ("\"+$backupIdentifier+"\" + $server.ServerName.Split('.')[0] + "-" + $folderParsed + "-"+$env.SystemName+"-"+$backupIdentifier+".zip")

        $sc = CreateUnzipCommand

        $activityDescription = "Restoring "+$folderToBackup+" in " + $server.ServerName  + "..."
        LogWrite ("    " + $activityDescription)
        try{

        $FolderFullPath = (Join-Path -Path $server.InstallationPath -ChildPath ("\"+$folderToBackup))

        $password = Get-SecureStringFromEncryptedString $env.AdminPass
        $cred = new-object -typename System.Management.Automation.PSCredential -argumentlist $env.AdminUser, $password

        $userName = $env.AdminUser
        $password = Get-ClearTextFromEncryptedString $env.AdminPass
        $uri = new-object System.Uri($backupPath)
        $backupServer = "\\"+$uri.host
        $sess = New-PSSession -ComputerName $server.ServerName -Credential $cred
        Enter-PSSession -Session $sess
            Invoke-Command -Session $sess -ScriptBlock { (net use $using:backupServer $using:password /USER:$using:userName) | Out-Null   }
            Invoke-Command -Session $sess -ScriptBlock $sc `
            -ArgumentList @($FolderFullPath,$backupPath, $activityDescription)
            Invoke-Command -Session $sess -ScriptBlock { (net use $using:backupServer /delete) | Out-Null   }
        Remove-PSSession -Session $sess
        Exit-PSSession

        }catch { throw (" *  Invoke-Command ERROR "+$_) }
    }

    LogWrite (" *  Finished "+$folderToBackup+" restore!") -foregroundColor Green
}

function Get-EncryptedStringFromClearText()
{
    param (
        [Parameter(Mandatory=$True)]
        [string] $ClearText
    )
    $SecureString = Get-SecureStringFromClearText $ClearText
    $EncryptedString = Get-EncryptedStringFromSecureString $SecureString
    return $EncryptedString
}

function Get-ClearTextFromEncryptedString()
{
    param (
        [Parameter(Mandatory=$True)]
        [string] $EncryptedString
    )
    $SecureString = Get-SecureStringFromEncryptedString $EncryptedString
    $ClearText = Get-ClearTextFromSecureString $SecureString
    return $ClearText
}

function Get-ClearTextFromSecureString()
{
    param (
        [Parameter(Mandatory=$True)]
        [SecureString] $SecureString
    )
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecureString)
    $ClearText = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    return $ClearText
}

function Get-EncryptedStringFromSecureString()
{
    param (
        [Parameter(Mandatory=$True)]
        [SecureString] $SecureString
    )
    $SecureStringKey = (3,4,2,3,56,34,254,222,1,1,2,23,42,54,33,233,1,34,2,7,6,5,35,43)
    $EncryptedString = ConvertFrom-SecureString $SecureString -Key $SecureStringKey
    return $EncryptedString
}

function Get-SecureStringFromClearText()
{
    param (
        [Parameter(Mandatory=$True)]
        [string] $ClearText
    )
    $SecureString = ConvertTo-SecureString $ClearText -AsPlainText -Force
    return $SecureString
}

function Get-SecureStringFromEncryptedString()
{
    param (
        [Parameter(Mandatory=$True)]
        [string] $EncryptedString
        )
    $SecureStringKey = (3,4,2,3,56,34,254,222,1,1,2,23,42,54,33,233,1,34,2,7,6,5,35,43)
    $SecureString = ConvertTo-SecureString $EncryptedString -Key $SecureStringKey
    return $SecureString
}


<#
    .SYNOPSIS
        Backup CMFEnvironment

    .DESCRIPTION
        By default creates a backup of the CMFEnvironment databases (online, ods, dwh) and folders (BusinessTier, HTML)

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER backupIdentifier
        Name that identifies the backup to be created

    .PARAMETER applicationServer
        Application server path

    .PARAMETER databases
        Databases to be backed up (default: online, ods, dwh)

    .PARAMETER folders
        Folders to be backed up (default: BusinessTier, HTML)

    .EXAMPLE
        Backup-CMFEnvironment $env -backupIdentifier "Sprint01" -applicationServer -applicationServer $env.ApplicationServers[0]
#>
function Backup-CMFEnvironment()
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # backupIdentifier
        [Parameter(Mandatory=$True)]
        [string] $backupIdentifier,

        # Application Server
        [Parameter(Mandatory=$False)]
        [PSObject]$applicationServer
    )

    # Nice to have utility - to be done in the future
}

<#
    .SYNOPSIS
        Restores CMFEnvironment

    .DESCRIPTION
        By default restores the CMFEnvironment databases (online, ods, dwh) and folders (BusinessTier, HTML)

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

    .PARAMETER backupIdentifier
        Name that identifies the backup to be restored

    .PARAMETER applicationServer
        Application server path

    .PARAMETER databases
        Databases to be restored (default: online, ods, dwh)

    .PARAMETER folders
        Folders to be restored (default: BusinessTier, HTML)

    .EXAMPLE
        Restore-CMFEnvironment $env -backupIdentifier "Sprint01" -applicationServer -applicationServer $env.ApplicationServers[0]

    .NOTES
        Requires administrative privileges on the application servers.
#>
function Restore-CMFEnvironment()
{
    param
    (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # backupIdentifier
        [Parameter(Mandatory=$True)]
        [string] $backupIdentifier,

        # Application Server
        [Parameter(Mandatory=$False)]
        [PSObject]$applicationServer
    )

    # Nice to have utility - to be done in the future
}


<#
    .SYNOPSIS
        Gets CMFEnvironment from config file.

    .DESCRIPTION
        Gets the CMFEnvironment file from a configuration file (ps1 or json)

    .PARAMETER environmentConfigName
        Environment configuration name.

    .PARAMETER environmentConfigPath
        Environment configuration folder path.

	.EXAMPLE
        $env = ( Get-CMFEnvironment -environmentConfigName "$EnvironmentConfigName" -environmentConfigPath "$PackageRootFolder\EnvironmentConfigs" -cmfInstallActionsPath "$PackageRootFolder\CMFInstallActions" )
#>
function Get-CMFEnvironment()
{
    param
    (
        # environmentConfigPath
        [Parameter(Mandatory=$True)]
        [string] $environmentConfigPath,
		
		# cmfInstallActionsPath
        [Parameter(Mandatory=$True)]
        [string] $cmfInstallActionsPath
    )
	
	if ( -not ( Test-Path $cmfInstallActionsPath ) ){
		throw "cmfInstallActionsPath is invalid... Folder '$cmfInstallActionsPath' does not exist..."	
	}
	if ( -not ( Test-Path $environmentConfigPath ) ){
		throw "environmentConfigPath is invalid... Folder '$environmentConfigPath' does not exist..."	
	}
		
	$cmfInstallActionsPath = (Resolve-Path -Path $cmfInstallActionsPath).Path
	$environmentConfigPath = (Resolve-Path -Path $environmentConfigPath).Path
	
	if (Test-Path $environmentConfigPath)
	{
		# Loading json configuration file
		$env = & $cmfInstallActionsPath\EnvironmentConfigParser.ps1 -ConfigFilePath $environmentConfigPath -ReferencesDir $cmfInstallActionsPath\References
	}
	else
    {
        throw "Config $environmentConfigPath not found!"
    }
	
	return $env
}

<#
    .SYNOPSIS
        Gets a variable from an Yaml file

    .DESCRIPTION
        Gets a variable from an Yaml file

    .PARAMETER FilePath
        Yaml File Path

    .PARAMETER VariableName
        Name of Variable to Get

	.EXAMPLE
        ParseYamlVariables -filePath "[PATH]\GlobalVariables.yml" -VariableName "CurrentPackage"
#>
function ParseYamlVariables()
{
    param
    (
        # File
        [Parameter(Mandatory=$True)]
        [string]$FilePath,

        # VariableName
        [Parameter(Mandatory=$True)]
        [string] $VariableName
    )
    
    foreach ($line in Get-Content $FilePath)
    { 
        if ($line -match $VariableName)
        {
            return $line.Split("'")[1];
        }
    }
}

<#
    .SYNOPSIS
        Gets Master Data Loader dependencies

    .DESCRIPTION
        Gets Master Data Loader dependencies

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.

	.EXAMPLE
        Get-MDLDependencies $env
#>
function Get-MDLDependencies()
{
    param (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env
    )

    # Copy MDL dependencies
    LogWrite ("Copy References from Master Data Loader")
    $appServer = $env.ApplicationServers[0]
    $mdlPath = $appServer.MasterDataLoaderPath;
    $mdlPath = '\\' + $appServer.ServerName + '\' + $mdlPath -replace ':', '$'

    Robocopy "$mdlPath" "$ModulePath\References" "*.dll" "MasterData.exe.config" /XF "Cmf.LightBusinessObjects.dll" /S /IS /XO | Out-Null
}

<#
    .SYNOPSIS
        Deploy IoT Packages

    .DESCRIPTION
        Deploy IoT Packages

    .PARAMETER env
        Environment configuration, created with New-CMFEnvironment function.
        
    .PARAMETER packageRootFolder
        PackageRootFolder

	.EXAMPLE
        IoTPackages-Deploy $env
#>
function IoTPackages-Deploy()
{
    param (
        # CMFEnvironment
        [Parameter(Mandatory=$True)]
        [PSObject]$env,

        # PackageRootFolder
        [Parameter(Mandatory=$True)]
        [string] $packageRootFolder
    )

    LogWrite "Publishing IoT Packages"

    $repositoryLocation = Get-Config $env -configPath '/Cmf/System/Configuration/ConnectIoT/RepositoryLocation/'
    if(-not $repositoryLocation)
    {
        throw "Missing Configuration /Cmf/System/Configuration/ConnectIoT/RepositoryLocation/"
    }
    $repositoryType = Get-Config $env -configPath '/Cmf/System/Configuration/ConnectIoT/RepositoryType/'
    if(-not $repositoryType)
    {
        LogWrite ("    Config /Cmf/System/Configuration/ConnectIoT/RepositoryType/ was not found. Fallback to RepositoryType = Npm.")
        $repositoryType = "Npm"
    }
    
    $successfullyPublishedPackagesCounter = 0
    $unsuccessfullyPublishedPackagesCounter = 0
    & cd $packageRootFolder

    if($repositoryType -eq "Directory")
    {
        foreach ($IoTPackageProjectReference in Get-ChildItem "$PackageRootFolder\AutomationCustomPackages\" -Directory)
        {
            foreach ($folder in Get-ChildItem "$PackageRootFolder\AutomationCustomPackages\$IoTPackageProjectReference\" -Directory)
            {
                cd "$PackageRootFolder\AutomationCustomPackages\$IoTPackageProjectReference\$folder"
                foreach ($package in Get-ChildItem -Depth 2 -Filter "*.tgz")
                {
                    Copy-Item -Force -Recurse -Verbose $package -Destination $repositoryLocation
                    if($LASTEXITCODE -ge 0){
                        $successfullyPublishedPackagesCounter++
                    }else
                    {
                        $unsuccessfullyPublishedPackagesCounter++
                        LogWrite "Could not publish package $($package)"
                    }
                }
            }
        }
        Invoke-Expression "& '$repositoryLocation\.rebuildDatabase.ps1'"
    }
    elseif($repositoryType -eq "Npm")
    {
        $NpmServerAddress = $repositoryLocation
        $parts = $NpmServerAddress.Split(':')
        $machineName = $parts[1].Replace('//','')
        $port = $parts[2]
        $Tag = "IoT_100"
		
        Write-Verbose -Verbose "Deploying $Tag to $NpmServerAddress..."

        $psi = New-Object System.Diagnostics.ProcessStartInfo;
        $psi.WindowStyle = [System.Diagnostics.ProcessWindowStyle]::Hidden;
        $psi.FileName = "cmd.exe"; #process file
        $psi.UseShellExecute = $false; #start the process from it's own executable file
        $psi.RedirectStandardInput = $true; #enable the process to read from standard input
        $psi.RedirectStandardOutput = $true; #enable the process to read from standard output
        $psi.RedirectStandardError = $true; #enable the process to read from standard error
        $psi.CreateNoWindow = $true;
        
        $UserName = $env.AdminUser.Split('\')[1]
        $UserPassword = Get-ClearTextFromEncryptedString $env.AdminPass
        $UserEmail = "$userName@$userName.com"

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

        foreach ($IoTPackageProjectReference in Get-ChildItem "$PackageRootFolder\AutomationCustomPackages\" -Directory)
        {
            foreach ($folder in Get-ChildItem "$PackageRootFolder\AutomationCustomPackages\$IoTPackageProjectReference" -Directory)
            {
                cd "$PackageRootFolder\AutomationCustomPackages\$IoTPackageProjectReference\$folder"
                foreach ($package in Get-ChildItem -Depth 2 -Filter "*.tgz") {
                    try 
                    {
                        $command = "npm unpublish $IoTPackageProjectReference/$folder --registry $NpmServerAddress --tag $Tag --force --loglevel=error"
                        LogWrite $command
                        powershell $command -NoNewWindow

                    }
                    catch 
                    {
                        LogWrite "Unpublished package $($package) not needed"
                    }

                    $command = "npm publish $package --registry $NpmServerAddress --tag $Tag --force --loglevel=error"
                    LogWrite $command
                    powershell $command -NoNewWindow
                    if($LASTEXITCODE -ge 0){
                        $successfullyPublishedPackagesCounter++
                    }else
                    {
                        $unsuccessfullyPublishedPackagesCounter++
                        LogWrite "Could not publish package $($package)"
                    }
                }
            }
        }		
    }
    
    LogWrite "Successfully published $($successfullyPublishedPackagesCounter) packages."
    LogWrite "$($unsuccessfullyPublishedPackagesCounter) packages were not published successfully."
}
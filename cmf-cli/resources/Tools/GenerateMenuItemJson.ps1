Param(
  [string]$basePath,
  [string]$project
)

#$basePath = ".\..\UI\Help"
						  
$packagesPath = "$basePath\src\packages"

$packageDir = get-childitem -path $packagesPath -directory -filter "cmf.docs.area.*"
write-verbose ('packageDir ' + $packageDir)
$projectName = $project
write-verbose ('projectName ' + $projectName)
$projectName = $projectName.ToLower()
$pkgName = "cmf.docs.area.${projectName}"
$assetsPath = "$packagesPath\${pkgName}\assets"
$pkgName = ${pkgName}.ToLower()

function Get-MetadataFromFolder {
	param (
		[string]$folder,
		[string]$parentFolder
)	
	$metadata = $null
	if ( $parentFolder )
	{
		write-verbose "Searching folder: $folder"
		$files = Get-ChildItem -Path "$folder" -Filter "*.md" -File
	
		foreach ( $file in $files )
		{
			$fileName = $file.BaseName
			write-verbose "File: $fileName"

			if ( $metadata )
			{
				$metadata += "," + [Environment]::NewLine
			}
			
			$fileContent = Get-Content -Path $file.FullName | Out-String
			
			$indexOfNewLine = $fileContent.indexof([Environment]::NewLine)
			if ( $indexOfNewLine -gt 0 ) {
				$fileContent = $fileContent.substring(0,$indexOfNewLine)
			}
			
			$fileContent = $fileContent -replace "#",""
			
			$fileContent = $fileContent.trim()
			
			$title = $fileContent
			
			# write-verbose $title
			
			$fileNameLower = $fileName.ToLower()
			$parentFolderLower = $parentFolder.ToLower()

			$metadata += "{" + [Environment]::NewLine
			$metadata += "   ""id"": ""$fileNameLower""," + [Environment]::NewLine
			$metadata += "   ""menuGroupId"": ""${pkgName}.${parentFolderLower}""," + [Environment]::NewLine
			$metadata += "   ""title"": ""$title""," + [Environment]::NewLine
			$metadata += "   ""actionId"": """"" + [Environment]::NewLine
			$metadata += "}"
		}
	}
	
	$folders = Get-ChildItem -Path "$folder" -Directory -Exclude "images"
	foreach ( $folder in $folders )
	{

		#write-verbose $folder
		$folderName = $folder.substring($folder.LastIndexOf([IO.Path]::DirectorySeparatorChar) + 1)
		write-verbose "getting metadata from folder: $folder - $folderName"
		$metadataFromSubFolder = Get-MetadataFromFolder $folder $folderName
		
		if ( $metadataFromSubFolder -and $metadata )
		{
			$metadata += "," + [Environment]::NewLine
		}
		$metadata += $metadataFromSubFolder

	}
	return $metadata
}



$mainFolder = $assetsPath
$metadata = Get-MetadataFromFolder $mainFolder

#metadata as array
$metadata = "[" + [Environment]::NewLine + "$metadata" + [Environment]::NewLine + "]"

$metadata | Set-Content "$assetsPath\__generatedMenuItems.json"
write-verbose ('File ''' + $assetsPath + '\__generatedMenuItems.json'' Updated')
#(Get-Content -Path "$assetsPath\$template" | Out-String ) -replace "@TableData@", $output `
#		| Set-Content "$assetsPath\$outputFile"

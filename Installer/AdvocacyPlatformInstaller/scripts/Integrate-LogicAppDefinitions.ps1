# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
Param (
	[string] $PackageDir,
	[string] $TemplateDir,
	[string] $ConfigDir,
	[string] $SourceDir
)

function Write-Log {
	Param (
		[string] $Message
	)

	Write-Host $Message
}

Write-Log "Looking for Newtonsoft.Json.dll..."
$jsonNetPath = (Get-ChildItem -Path $PackageDir -Recurse | Where-Object { $_.Name -eq 'Newtonsoft.Json.dll' })[0]

if ($null -eq $jsonNetPath) {
	Write-Log "Could not find Newtonsoft.Json.dll!"

	exit -1
}

Write-Log "Found. Loading '$jsonNetPath'..."
[Reflection.Assembly]::LoadFile($jsonNetPath.FullName)

$templateFiles = @("$TemplateDir\apiLogicApps.template.json","$TemplateDir\azuredeploy.template.json","$TemplateDir\logicApps.template.json")

ForEach($templateFile in $templateFiles) {
	Write-Log "Processing $templateFile..."

	$templateFileContents = Get-Content -Path $templateFile
	$templateJson = ($templateFileContents | ConvertFrom-Json)
	$logicAppResources = ($templateJson.resources | Where-Object { $_.type -eq "Microsoft.Logic/workflows" })

	ForEach ($logicApp in $logicAppResources) {
		$definitionReference = $logicApp.properties.definition.dref
		$definitionReferencePath = "$SourceDir\$definitionReference"
		
		if ($null -ne $definitionReference) {
			Write-Log "Processing template reference to '$definitionReferencePath' looking for '$($logicApp.name)'..."

			$definitionTemplateFileContents = Get-Content -Path $definitionReferencePath
			$definitionTemplateJson = ($definitionTemplateFileContents | ConvertFrom-Json)

			$definitionLogicAppResource = ($definitionTemplateJson.resources | Where-Object { $_.name -eq $logicApp.name })
			$logicApp.properties.definition = $definitionLogicAppResource.properties.definition
		}
	}

	$outputFileName = [System.IO.Path]::GetFileName($templateFile).Replace("template.", "")
	$outputFilePath = "$ConfigDir\$outputFileName"

	Write-Log "Saving updates to '$outputFilePath'..."
	$outputContents = ($templateJson | ConvertTo-Json -Depth 100)

	$outputContentP2 = [Newtonsoft.Json.Linq.JObject]::Parse($outputContents)
	$outputContentP3 = [Newtonsoft.Json.JsonConvert]::SerializeObject($outputContentP2, [Newtonsoft.Json.Formatting]::Indented)
	
	$outputContentP3 | Out-File -FilePath $outputFilePath -Encoding UTF8
}
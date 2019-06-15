# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
<#
.SYNOPSIS
Logs an informational message with color
.DESCRIPTION
Logs an informational message with color
.PARAMETER Message
Specifies the message to Write
.PARAMETER ForegroundColor
Specifies the name of the foreground color for the text
.PARAMETER NoNewline
Specifies to not write a newline character
#>
Param (
	[string] $ReplacementMapFilePath = ".\replacementMap.json",
	[string[]] $LogicAppTemplateFilePaths = @("..\ap-process-logicapp-wu.json","..\ap-request-logicapp-wu.json","..\ap-new-case-put-on-queue.json","..\ap-results-available-update-case.json","..\ap-address-updated-update-case.json","..\ap-records-for-retry.json"),
	[string] $PackageDir = "..\..\",
	[switch] $Verbose
)

<#
.SYNOPSIS
Writes a log message
.DESCRIPTION
Writes a log message
.PARAMETER Message
Specifies the message to Write
#>
function Write-Log {
	Param (
		[string] $Message
	)

	Write-Host $Message
}

function Write-Verbose {
	Param (
		[string] $Message
	)

	if ($Verbose) {
		Write-Log $Message
	}
}

<#
.SYNOPSIS
Loads an ARM template
.DESCRIPTION
Loads an ARM template
.PARAMETER ArmTemplateFilePath
Specifies the path to the ARM template
#>
function Load-ArmTemplate {
	Param (
		[string] $ArmTemplateFilePath
	)

	if ((Test-Path -Path $ArmTemplateFilePath) -eq $False) {
		Write-Log "Could not find '$ArmTemplateFilePath'!"

		exit
	}

	Write-Log "Loading ARM template '$ArmTemplateFilePath'..."
	$global:armTemplate = ((Get-Content -Path $ArmTemplateFilePath -Encoding UTF8).Replace("\u0027", "'") | ConvertFrom-json)
}

<#
.SYNOPSIS
Loads a file specifying replacement directives
.DESCRIPTION
Loads a file specifying replacement directives for translating the Logic Apps definitions in the Azure Resource Group project
to be parameterized in preparation for deployment
.PARAMETER FilePath
Specifies the path to the replacement map file

De-serializes to $global:replacementMap
#>
function Load-ReplacementMap {
	Param (
		[string] $FilePath
	)

	if ((Test-Path -Path $FilePath) -eq $False) {
		Write-Log "Could not find '$FilePath'!"

		exit
	}

	Write-Log "Loading replacement map '$FilePath'..."
	$global:replacementMap = (Get-Content $FilePath | ConvertFrom-Json)
}

<#
.SYNOPSIS
Writes an ARM template file back to disk
.DESCRIPTION
Writes an ARM template file back to disk
.PARAMETER ArmTemplateFilePath
Specifies the file path to write the ARM template to.

Serializes from $global:armTemplate
#>
function Write-ArmTemplate {
	Param (
		[string] $ArmTemplateFilePath
	)

	Write-Log "Writing ARM template to '$ArmTemplateFilePath'..."

	$outputContentP1 = ($global:armTemplate | ConvertTo-Json -Depth 100).Replace("\u0027", "'")

	# P2 and P3 because the output from ConvertTo-Json is very hard to read with a higher degree of nested objects
	$outputContentP2 = [Newtonsoft.Json.Linq.JObject]::Parse($outputContentP1)
	$outputContentP3 = [Newtonsoft.Json.JsonConvert]::SerializeObject($outputContentP2, [Newtonsoft.Json.Formatting]::Indented)
	
	$outputContentP3 | Out-File -FilePath $ArmTemplateFilePath -Encoding UTF8
}

function Add-Variables {
	Write-Verbose "Adding variables."

	$propNames = ($global:replacementMap.addVariables | Get-Member -MemberType *Property).Name

	if ($null -eq $global:armTemplate.variables) {
		$global:armTemplate.variables = @{}
	}

	foreach ($propName in $propNames) {
		Write-Verbose "Adding variable $propName with value $($global:replacementMap.addVariables.$propName)..."

        if ($global:armTemplate.variables.PSObject.Properties.Match($propName).Count) {
			$global:armTemplate.variables.$propName = $global:replacementMap.addVariables.$propName
		}
		else
		{
			$global:armTemplate.variables | Add-Member -MemberType NoteProperty -Name $propName -Value $global:replacementMap.addVariables.$propName -Force
		}
    }
}

function Clear-DefaultValues {
	ForEach($parameter in $global:armTemplate.parameters.PSObject.Properties) {
		$defaultValue = $parameter.Value.PSObject.Properties | Where-Object { $_.Name -eq "defaultValue" }

		if ($null -ne $defaultValue) {
			$defaultValue.Value = ""
		}
	}
}

<#
.SYNOPSIS
Performs replacements based on directives in the replacement map
.DESCRIPTION
Performs replacements based on directives in the replacement map
.PARAMETER Content
An object representing the JSON content to transform
#>
function Process-Replacements {
	Param (
		[string] $ArmTemplateFilePath
	)

	Clear-DefaultValues

	$armTemplateFileName = [System.IO.Path]::GetFileName($ArmTemplateFilePath)

	$logicAppResources = ($global:armTemplate.resources | Where-Object { $_.type -eq "Microsoft.Logic/workflows" })	

	ForEach($parameterReplacement in $global:replacementMap.parameters) {
		Write-Verbose "Looking for parameter '$($parameterReplacement.value)'..."

		ForEach($logicAppResource in $logicAppResources) {
			Write-Verbose "Looking in '$($logicAppResource.name)'..."

			$parameter = $logicAppResource.properties.definition.parameters.PSObject.Properties | Where-Object { $_.MemberType -eq "NoteProperty" -and $_.Name -eq $($parameterReplacement.key) }

			if ($null -ne $parameter) {
				if ($Verbose) {
					Write-Log "Replacing parameter '$($parameterReplacement.value)'..."
				}

				$parameter.Value.defaultValue = $parameterReplacement.value
			}
		}
	}

	$armTemplateDictionary = $global:replacementMap.dictionary.$armTemplateFileName

	if ($null -ne $armTemplateDictionary){
		if ($null -ne $armTemplateDictionary.addParameters) {
			$propNames = ($armTemplateDictionary.addParameters | Get-Member -MemberType *Property).Name

			if ($null -eq $global:armTemplate.parameters) {
				$global:armTemplate.parameters = @{}
			}

			foreach ($propName in $propNames) {
				Write-Verbose "Adding parameter $propName with value $($armTemplateDictionary.addParameters.$propName)..."

				if ($global:armTemplate.parameters.PSObject.Properties.Match($propName).Count) {
					$global:armTemplate.parameters.$propName = $armTemplateDictionary.addParameters.$propName
				}
				else
				{
					$global:armTemplate.parameters | Add-Member -MemberType NoteProperty -Name $propName -Value $armTemplateDictionary.addParameters.$propName -Force
				}
			}
		}
	}

	$jsonContent = ($global:armTemplate | ConvertTo-Json -Depth 100).Replace("\u0027", "'")

	if ($null -ne $armTemplateDictionary){
		ForEach($regexReplacement in $armTemplateDictionary.regex) {
			Write-Verbose "Replacing '$($regexReplacement.pattern)' with '$($regexReplacement.value)'..."

			$regex = [regex] $regexReplacement.pattern
			$jsonContent = $regex.Replace($jsonContent,$regexReplacement.value)
		}
	}

	ForEach($regexReplacement in $global:replacementMap.regex) {
		Write-Verbose "Replacing '$($regexReplacement.pattern)' with '$($regexReplacement.value)'..."

		$regex = [regex] $regexReplacement.pattern
		$jsonContent = $regex.Replace($jsonContent,$regexReplacement.value)
	}

	$global:armTemplate = ($jsonContent | ConvertFrom-Json)

	Add-Variables
}

Write-Log "Looking for Newtonsoft.Json.dll..."
$jsonNetPath = (Get-ChildItem -Path $PackageDir -Recurse | Where-Object { $_.Name -eq 'Newtonsoft.Json.dll' })[0]

if ($null -eq $jsonNetPath) {
	Write-Log "Could not find Newtonsoft.Json.dll!"

	exit -1
}

Write-Log "Found. Loading '$jsonNetPath'..."
[Reflection.Assembly]::LoadFile($jsonNetPath.FullName)

Load-ReplacementMap -FilePath $ReplacementMapFilePath

ForEach($logicAppTemplateFilePath in $LogicAppTemplateFilePaths) {
	Write-Log "Processing '$logicAppTemplateFilePath'..."

	Load-ArmTemplate -ArmTemplateFilePath $logicAppTemplateFilePath

	Process-Replacements -ArmTemplateFilePath $logicAppTemplateFilePath

	Write-ArmTemplate -ArmTemplateFilePath $logicAppTemplateFilePath
}
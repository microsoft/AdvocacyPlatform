# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
Param (
    [Parameter(Mandatory=$true)]
    [string]$CrmOrganizationName,
    [Parameter(Mandatory=$false)]
    [PSCredential]$Credential,
	[switch] $NoUnPack,
	[switch] $NoWebResources,
	[switch] $NoPack,
	[switch] $NoBuild,
	[switch] $IncrementMajorVersion,
	[switch] $IncrementMinorVersion,
	[switch] $IncrementBuild
)

$global:solutionFilePath = "$PSScriptRoot\AdvocacyPlatform\package\Other\Solution.xml"

function Write-Log {
	Param (
		[string] $Message
	)

	Write-Host $Message
}

function Find-MsBuild {
	Param (
		[int] $MaxVersion = 2017
	)

    $agentPath = "$Env:programfiles (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\msbuild.exe"
    $devPath = "$Env:programfiles (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\msbuild.exe"
    $proPath = "$Env:programfiles (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\msbuild.exe"
    $communityPath = "$Env:programfiles (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\msbuild.exe"
    $fallback2015Path = "${Env:ProgramFiles(x86)}\MSBuild\14.0\Bin\MSBuild.exe"
    $fallback2013Path = "${Env:ProgramFiles(x86)}\MSBuild\12.0\Bin\MSBuild.exe"
    $fallbackPath = "C:\Windows\Microsoft.NET\Framework\v4.0.30319"
		
    If ((2017 -le $MaxVersion) -And (Test-Path $agentPath)) { return $agentPath } 
    If ((2017 -le $MaxVersion) -And (Test-Path $devPath)) { return $devPath } 
    If ((2017 -le $MaxVersion) -And (Test-Path $proPath)) { return $proPath } 
    If ((2017 -le $MaxVersion) -And (Test-Path $communityPath)) { return $communityPath } 
    If ((2015 -le $MaxVersion) -And (Test-Path $fallback2015Path)) { return $fallback2015Path } 
    If ((2013 -le $MaxVersion) -And (Test-Path $fallback2013Path)) { return $fallback2013Path } 
    If (Test-Path $fallbackPath) { return $fallbackPath } 
        
    throw "Unable to find msbuild"
}

function Get-SolutionVersion {
	[xml]$global:solutionXml = Get-Content -Path $global:solutionFilePath

	$global:previousVersion = $solutionXml.ImportExportXml.SolutionManifest.Version
}

function Increment-SolutionVersion {
	Param (
		[bool] $IncrementMajorVersion,
		[bool] $IncrementMinorVersion,
		[bool] $IncrementBuild
	)

	[xml]$global:solutionXml = Get-Content -Path $global:solutionFilePath

	$versionPartsStr = $global:previousVersion.Split(@('.'))
	$versionPartsInt = ForEach($versionPart in $versionPartsStr) { [int]::Parse($versionPart) }

	if ($IncrementMajorVersion) {
		$versionPartsInt[0]++
	}

	if ($IncrementMinorVersion) {
		$versionPartsInt[1]++
	}

	if ($IncrementBuild) {
		$versionPartsInt[2]++
	}

	$versionPartsInt[3]++
	
	$ofs = "."
	$global:solutionXml.ImportExportXml.SolutionManifest.Version = [string]$versionPartsInt

	Write-Log "New version v$($global:solutionXml.ImportExportXml.SolutionManifest.Version)"

	$global:solutionXml.Save($global:solutionFilePath)
}

if ($NoUnPack -eq $False -or
    $NoWebResources -eq $False -or
	$NoPack -eq $False) {
	if ($null -eq $Credential) {
		$Credential = Get-Credential

		if ($null -eq $Credential){
			Write-Log "No credential provided!"
		
			exit
		}
	}
}

$packagePath = ".\packages"

Write-Log "Getting existing solution version.."
Get-SolutionVersion
Write-Log "v$($global:previousVersion)"

if ($NoUnPack -eq $False) {
	Write-Log "Unpacking remote Dynamics 365 CRM solution from '$CrmOrganizationName'..."
	.\AdvocacyPlatform\spkl\automated-unpack.ps1 -CrmOrganizationName $CrmOrganizationName -Credential $Credential -PackagePath $packagePath -OutputPath "AdvocacyPlatform"
	Write-Log "Done`n"
}

if ($NoWebResources -eq $False) {
	Write-Log "Downloading web resources from remote Dynamics 365 CRM solution from '$CrmOrganizationName'..."
	.\AdvocacyPlatformWebResources\spkl\automated-download-webresources.ps1 -CrmOrganizationName $CrmOrganizationName -Credential $Credential -PackagePath $packagePath -OutputPath "AdvocacyPlatformWebResources"
	Write-Log "Done`n"
}

if ($NoPack -eq $False) {
	Write-Log "Incrementing solution version.."
	Increment-SolutionVersion -IncrementMajorVersion $IncrementMajorVersion `
						      -IncrementMinorVersion $IncrementMinorVersion `
							  -IncrementBuild $IncrementBuild

	Write-Log "Re-packing local Dynamics 365 CRM solution..."
	.\AdvocacyPlatform\spkl\automated-pack.ps1 -CrmOrganizationName $CrmOrganizationName -Credential $Credential -PackagePath $packagePath -InputPath "AdvocacyPlatform"
	Write-Log "Done`n"
}

if ($NoBuild -eq $False) {
	Write-Log "Looking for msbuild..."
	$msbuildPath = Find-MsBuild

	if ($null -eq $msbuildPath) {
		Write-Log "Could not find msbuild!"

		exit
	}

	Write-Log "Building deployment package..."
	& $msbuildPath /p:Configuration=Release .\Microsoft.AdvocacyPlatform.UI.sln
}
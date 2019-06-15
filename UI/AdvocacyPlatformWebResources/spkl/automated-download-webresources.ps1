# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
Param (
    [Parameter(Mandatory=$true)]
    [string]$CrmOrganizationName,
    [Parameter(Mandatory=$true)]
    [PSCredential]$Credential,
	[Parameter(Mandatory=$true)]
	[string]$PackagePath,
	[Parameter(Mandatory=$true)]
	[string]$OutputPath
)

$spklExe = Get-ChildItem $PackagePath -Recurse | Where-Object { $_.Name -eq "spkl.exe" }

if ($spklExe -eq $null){
    Write-Host Could not find spkl.exe!
    exit
}

Write-Host "Using `"$($spklExe.FullName)`""

& "$($spklExe.FullName)" download-webresources "$OutputPath" "AuthType=Office365; Username=$($Credential.UserName); Password=$($Credential.GetNetworkCredential().Password); Url=https://$CrmOrganizationName.crm.dynamics.com"
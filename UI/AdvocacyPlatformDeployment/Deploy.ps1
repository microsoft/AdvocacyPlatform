# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
Param (
    [Parameter(Mandatory=$true)]
    [string]$OutputPath,
    [Parameter(Mandatory=$true)]
    [string]$CrmRegion,
    [Parameter(Mandatory=$true)]
    [string]$CrmOrganizationName,
    [Parameter(Mandatory=$false)]
    [string]$UserName,
    [Parameter(Mandatory=$false)]
    [System.Security.SecureString]$Password,
    [Parameter(Mandatory=$false)]
    [switch]$SkipConfirm
)

cd .\Powershell
.\RegisterXRMPackageDeployment.ps1

if ([string]::IsNullOrWhiteSpace($UserName) -or
    [string]::IsNullOrWhiteSpace($Password)){
    Write-Host No or partial credentials provided. Assuming interactive flow to get credentials.
    $credentials = Get-Credential
} else {
    Write-Host Credentials provided.
    $credentials = New-Object System.Management.Automation.PSCredential($UserName,$Password)
}

if ($credentials -eq $null){
    Write-Host Credentials invalid.

    exit
}

$orgs = Get-CrmOrganizations -DeploymentRegion $CrmRegion -OnLineType Office365 -Credential $credentials 
$org = $orgs | Where-Object { $_.UniqueName -eq $CrmOrganizationName }

if ($org -eq $null){
    Write-Host Target organization not found!
    exit
}

Write-Host
Write-Host Deploying to organization:
Write-Host `tFriendly name:    $($org.FriendlyName)
Write-Host `tOrganization:     $($CrmOrganizationName)
Write-Host `tUnique Name:      $($org.UniqueName)
Write-Host `tOrganization URL: $($org.OrganizationUrl)
Write-Host

if ($SkipConfirm){
    Write-Host Skipping confirmation.
} else {
    $confirmation = Read-Host -Prompt "Confirm deployment to this organization? (Yes/No)"

    if ([string]::Compare($confirmation,"yes",$true) -ne 0){
        Write-Host Aborting.
        exit
    }

    Write-Host Confirmation received.
}

Write-Host [System.Net.ServicePointManager]::SecurityProtocol
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

Write-Host Attempting to create connection to organization "$($org.UniqueName)" in "$CrmRegion"...
$crmConnection = Get-CrmConnection -DeploymentRegion $CrmRegion -OnlineType Office365 -OrganizationName $org.UniqueName -Credential $credentials

if ($crmConnection -eq $null){
    Write-Host Could not acquire connection! Aborting...
    exit
}

Write-Host Enumerating packages to deploy from "$OutputPath"...
$packagesToDeploy = Get-CrmPackages -PackageDirectory $OutputPath

foreach($packageToDeploy in $packagesToDeploy){
    Write-Host Deploying $($packageToDeploy.PackageFullName)...

    Import-CrmPackage -CrmConnection $crmConnection -PackageDirectory $OutputPath -PackageName $packageToDeploy.PackageAssemblyLocation
}


cd ..
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
Param (
	[PSCredential]$Credential,
	[string]$ApplicationRegistrationName
)

class ApiResource
{
	[Guid]$ResourceAppId;
	[Guid[]]$OAuth2Permissions;
	[int]$Type;
}

class ApiPermissionsRequest
{
	[Guid]$ObjectId;
	[ApiResource[]]$Resources;
}

$global:statusMessage = ""
$global:status = ""
$global:statusCode = 0
$global:appRegistration = $Null
$global:servicePrincipal = $Null

function Write-Log {
	Param (
		[string]$Message
	)

	Write-Host $Message
}

<#
.SYNOPSIS
Creates a new Azure AD application registration
.DESCRIPTION
Creates a new Azure AD application registration
.PARAMETER DisplayName
The display name for the application registration
.PARAMETER HomePage
The home page for the application registration
.PARAMETER IdentifierUris
An array of identifier URIs for the application registration
.PARAMETER Password
A client secret for the service principal
#>
function Deploy-AzureADAppRegistration {
	Param (
		[string] $DisplayName,
		[string[]] $ReplyUrls
	)

	Write-Log "Looking for application registration with a display name of '$DisplayName'..."
	$appRegistration = Get-AzADApplication -DisplayName $DisplayName `
										   -ErrorVariable IsError

	if ($null -ne $IsError.Exception) {
		Write-Log "Error encountered."

		Write-Log $IsError.Exception.Message

		$global:status = "Failure"
		$global:statusMessage = "Failed to get create Azure AD application registration!"
		$global:statusCode = -1

		return
	} elseif ($null -eq $appRegistration) {
		Write-Log "Application registration not found. Creating..."
		$appRegistration = New-AzureADApplication -DisplayName $DisplayName `
											      -ReplyUrls $ReplyUrls `
											      -AvailableToOtherTenants $True `
												  -PublicClient $True `
											      -ErrorVariable IsError

		$appRegistration

		if ($null -ne $IsError.Exception) {
			Write-Log "Error encountered."

			Write-Log $IsError.Exception.Message

			$global:status = "Failure"
			$global:statusMessage = "Failed to create Azure AD application registration!"
			$global:statusCode = -1

			return
		}
	} else {
		Write-Log "Application registration already exists."
	}
	
	$global:appRegistration = @{
		DisplayName = $appRegistration.DisplayName;
		ObjectId = $appRegistration.ObjectId.ToString();
		ApplicationId = $appRegistration.AppId.ToString();
	}

	$global:status = "Success"
	$global:statusCode = 0
}

<#
.SYNOPSIS
Creates a new Azure AD service principal
.DESCRIPTION
Creates a new Azure AD service principal
.PARAMETER ApplicationId
The application id of the application registration to create the service principal from
#>
function Deploy-AzureADServicePrincipal {
	Param (
		[string] $ApplicationId
	)

	Write-Log "Looking for service principal for application '$ApplicationId'..."
	$servicePrincipal = Get-AzADServicePrincipal -ApplicationId $ApplicationId `
												 -ErrorVariable IsError

	if ($null -ne $IsError.Exception) {
		Write-Log "Error encountered."

		Write-Log $IsError.Exception.Message

		$global:status = "Failure"
		$global:statusMessage = "Failed to get Azure AD service principal!"
		$global:statusCode = -1

		return
	}

	if ($null -eq $servicePrincipal) {
		Write-Log "Service principal does not exist. Creating..."
		New-AzADServicePrincipal -ApplicationId $ApplicationId `
								 -ErrorVariable IsError

		if ($null -ne $IsError.Exception) {
			Write-Log "Error encountered."

			Write-Log $IsError.Exception.Message

			$global:status = "Failure"
			$global:statusMessage = "Failed to create Azure AD service principal!"
			$global:statusCode = -1
		}

		$attemptCount++
	} else {
		Write-Log "Service principal already exists!"
	}
	
	$global:servicePrincipal = @{
		Id = $servicePrincipal.Id
	}

	$global:status = "Success"
	$global:statusCode = 0
}

<#
.SYNOPSIS
Grant the specified OAuth2Permission to a service principal
.DESCRIPTION
Grant the specified OAuth2Permission to a service principal
.PARAMETER ApplicationObjectId
The object id of the application registration to grant the permissions to
.PARAMETER ResourceAppId
The resource to grant permission on
.PARAMETER OAuth2Permission
The permission to grant
.PARAMETER Application
Specifies to grant as an application permission
.PARAMETER Delegated
Specifies to grant as a delegated permission
#>
function Deploy-AzureADAppRegistrationPermissions {
	Param (
		[ApiPermissionsRequest] $ApiPermissions
	)

	$requiredAccessList = New-Object "System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.RequiredResourceAccess]"

	foreach ($apiPermission in $ApiPermissions.Resources) {
		$requiredAccess = New-Object "Microsoft.Open.AzureAD.Model.RequiredResourceAccess"
		$requiredAccess.ResourceAppId = $apiPermission.ResourceAppId

		$requiredAccessResourceAccessList = New-Object "System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.ResourceAccess]"
		
		foreach($oauth2Permission in $apiPermission.OAuth2Permissions) {
	 		if ($apiPermission.Type -eq 1){
	 			$access = New-Object "Microsoft.Open.AzureAD.Model.ResourceAccess" -ArgumentList $oauth2Permission,"Role"
	 		} elseif ($apiPermission.Type -eq 0){
	 			$access = New-Object "Microsoft.Open.AzureAD.Model.ResourceAccess" -ArgumentList $oauth2Permission,"Scope"
	 		} else {
	 			Write-Log "You must specify either -Application or -Delegated!"
	 
	 			return
	 		}
			
			$requiredAccessResourceAccessList.Add($access)
		}

		$requiredAccess.ResourceAccess = $requiredAccessResourceAccessList
		$requiredAccessList.Add($requiredAccess)
	}
	
	# No Az module equivalent
	Set-AzureADApplication -ObjectId $ApiPermissions.ObjectId `
						   -RequiredResourceAccess $requiredAccessList `
						   -ErrorVariable IsError
	
	if ($null -ne $IsError.Exception) {
		Write-Log "Error encountered."
	
		Write-Log $IsError.Exception.Message
	
		$global:status = "Failure"
		$global:statusMessage = "Failed to assign OAauth2 permissions for application registration!"
		$global:statusCode = -1
	
		return
	}
	
	$global:status = "Success"
	$global:statusCode = 0
}

function CheckFor-Dependencies {
	$module = Get-Module -Name Az

	while ($module -eq $Null) {
		Import-Module -Name Az -ErrorVariable IsError -ErrorAction SilentlyContinue

		if ($IsError.Exception -ne $Null) {
			$confirm = Read-Host "It looks like the Az module is not installed. Would you like to try and install this module? [(Y)es/(N)o]"
		
			if ([string]::Compare($confirm,"yes",$True) -eq 0 -or [string]::Compare($confirm,"y",$True) -eq 0) {
				Install-Module Az
			} else {
				Write-Log "This process cannot run without the Az module. Exiting..."
				exit
			}
		}

		$module = Get-Module -Name Az
	}

	$module = Get-Module -Name AzureAD

	while ($module -eq $Null) {
		Import-Module -Name AzureAD -ErrorVariable IsError -ErrorAction SilentlyContinue

		if ($IsError.Exception -ne $Null) {
			$confirm = Read-Host "It looks like the AzureAD module is not installed. Would you like to try and install this module? [(Y)es/(N)o]"
		
			if ([string]::Compare($confirm,"yes",$True) -eq 0 -or [string]::Compare($confirm,"y",$True) -eq 0) {
				Install-Module AzureAD
			} else {
				Write-Log "This process cannot run without the AzureAD module. Exiting..."
				exit
			}
		}

		$module = Get-Module -Name AzureAD
	}
}

CheckFor-Dependencies

Write-Log "Connecting to Azure AD..."

if ($Credential -eq $Null) {
	$Credential = Get-Credential
}
	
$azContext = Connect-AzAccount -Credential $Credential -ErrorVariable IsError
$azadContext = Connect-AzureAD -Credential $Credential -ErrorVariable IsError

if ($IsError.Exception -ne $Null) {
	Write-Log "Exception encountered: $($IsError.Exception.Message)"
	Write-Log "Exiting..."

	exit
}

$random = Get-Random
$guid = [Guid]::NewGuid()
$defaultAppRegistrationName = "ap-installer-$random-aad"
$identifierUri = "https://microsoft.onmicrosoft.com/$guid"
$replyUrl = "myapp://ap-installer-$random-auth"
$lhReplyUrl = "https://localhost"

if ([string]::IsNullOrWhiteSpace($ApplicationRegistrationName)) {
	$appRegistrationName = Read-Host "Please specify the display name for the installer application registration (Default=$defaultAppRegistrationName)"
} else {
	$appRegistrationName = $ApplicationRegistrationName
} 

if ([string]::IsNullOrWhiteSpace($appRegistrationName)) {
	$appRegistrationName = $defaultAppRegistrationName
}

Deploy-AzureADAppRegistration -DisplayName $appRegistrationName `
							  -ReplyUrls @($replyUrl,$lhReplyUrl)

if ($global:statusCode -eq 0) {
	Deploy-AzureADServicePrincipal -ApplicationId $global:appRegistration.ApplicationId
}

$azureStorageResourceAppId = "e406a681-f3d4-42a8-90b6-c2b029497af1"
$azureStorageUserImpersonationPermissionId = "03e0da56-190b-40ad-a80c-ea378c433f7f"

$azureKeyVaultResourceAppId = "cfa8b339-82a2-471a-a3c9-0fc0be7a4093"
$azureKeyVaultUserImpersonationPermissionId = "f53da476-18e3-4152-8e01-aec403e6edc0"

$microsoftGraphResourceAppId = "00000003-0000-0000-c000-000000000000"
$microsoftGraphDirectoryAccessAsUserAllPermissionId = "0e263e50-5827-48a4-b97c-d940288653c7"
$microsoftGraphUserReadPermissionId = "e1fe6dd8-ba31-4d61-89e7-88639da4683d"

$dynamicsCrmResourceAppId = "00000007-0000-0000-c000-000000000000"
$dynamicsCrmUserImpersonationPermissionId = "78ce3f0f-a1ce-49c2-8cde-64b5c0896db4"

$powerAppsServiceResourceAppId = "475226c6-020e-4fb2-8a90-7a972cbfc1d4"
$powerAppsServiceUserImpersonationPermissionId = "0eb56b90-a7b5-43b5-9402-8137a8083e90"

$azureServiceManagementResourceAppId = "797f4846-ba00-4fd7-ba43-dac1f8f63013"
$azureServiceManagementUserImpersonationId = "41094075-9dad-400e-a0bd-54e686782033"

if ($global:statusCode -eq 0) {
	$apiPermissions = New-Object ApiPermissionsRequest
	$apiPermissions.ObjectId = $global:appRegistration.ObjectId
	$apiPermissions.Resources = @(
		@{
			ResourceAppId = $azureStorageResourceAppId;
			OAuth2Permissions = @(
				$azureStorageUserImpersonationPermissionId
			);
			Type = 0;
		},
		@{
			ResourceAppId = $azureKeyVaultResourceAppId;
			OAuth2Permissions = @(
				$azureKeyVaultUserImpersonationPermissionId
			);
			Type = 0;
		},
		@{
			ResourceAppId = $microsoftGraphResourceAppId;
			OAuth2Permissions = @(
				$microsoftGraphDirectoryAccessAsUserAllPermissionId,
				$microsoftGraphUserReadPermissionId
			);
			Type = 0;
		},
		@{
			ResourceAppId = $dynamicsCrmResourceAppId;
			OAuth2Permissions = @(
				$dynamicsCrmUserImpersonationPermissionId
			);
			Type = 0;
		},
		@{
			ResourceAppId = $powerAppsServiceResourceAppId;
			OAuth2Permissions = @(
				$powerAppsServiceUserImpersonationPermissionId
			);
			Type = 0;
		},
		@{
			ResourceAppId = $azureServiceManagementResourceAppId;
			OAuth2Permissions = @(
				$azureServiceManagementUserImpersonationId
			);
			Type = 0;
		}
	)

	Deploy-AzureADAppRegistrationPermissions -ApiPermissions $apiPermissions
}

Write-Log "ClientId:           $($appRegistration.ApplicationId)"
Write-Log "RedirectUri:        $($replyUrl)"
Write-Log "ConsentRedirectUri: $($lhReplyUrl)"
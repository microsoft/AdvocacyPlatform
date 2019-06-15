# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
Param (
	[string] $EnvironmentFilePath = ".\AP-PostManEnvironment.json",
	[Parameter(Mandatory=$True)]
	[string] $InstallationConfigurationFilePath,
	[PSCredential] $Credential
)

function Write-Log {
	Param (
		[string] $Message
	)

	Write-Host $Message
}

function Get-PublishingProfileCredentials {
	Param (
		[string] $ResourceGroupName, 
		[string] $WebAppName
	)
 
    $resourceType = "Microsoft.Web/sites/config"
    $resourceName = "$WebAppName/publishingcredentials"
 
    $publishingCredentials = Invoke-AzureRmResourceAction -ResourceGroupName $ResourceGroupName `
														  -ResourceType $resourceType `
														  -ResourceName $resourceName `
														  -Action list `
														  -ApiVersion 2015-08-01 `
														  -Force
 
    return $publishingCredentials
}

function Get-KuduApiAuthorisationHeaderValue {
	Param (
		[string] $ResourceGroupName, 
		[string] $WebAppName
	)
	
    $publishingCredentials = Get-PublishingProfileCredentials -ResourceGroupName $resourceGroupName `
															  -WebAppName $webAppName
 
    return ("Basic {0}" -f [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $publishingCredentials.Properties.PublishingUserName, $publishingCredentials.Properties.PublishingPassword))))
}

function Get-JwtToken {
	Param (
		[string] $AccessToken,
		[string] $WebAppName
	)

	$apiUrl = "https://$WebAppName.scm.azurewebsites.net/api/functions/admin/token"

	$jwt = Invoke-RestMethod -Uri $apiUrl `
							 -Headers @{Authorization=$AccessToken} `
							 -Method GET

	return ("Bearer {0}" -f $jwt)
}

function Get-HostAPIKeys {
	Param (
		[string] $AccessToken,
		[string] $WebAppName
	)
 
    $apiUrl = "https://$WebAppName.azurewebsites.net/admin/host/keys"

	Write-Log $AccessToken
	Write-Log $apiUrl
  
    $result = Invoke-WebRequest -Uri $apiUrl `
							    -Headers @{Authorization=$AccessToken} `
							    -Method GET
     
    return $result
}

function ConnectTo-Azure {
	Param (
		[PSCredential] $Credential
	)

	if ($null -eq $Credential) {
		$Credential = Get-Credential
	} 

	Write-Log "Connecting to Azure..."
	Connect-AzAccount -Credential $Credential -ErrorVariable IsError

	if ($null -ne $IsError.Exception) {
		Connect-AzAccount -ErrorVariable IsError

		if ($null -ne $IsError.Exception) {
			Write-Log "Could not connect to Azure!"

			exit
		}
	}

	Write-Log "Connecting to Azure AD..."
	Connect-AzureAD -Credential $Credential -ErrorVariable IsError

	if ($null -ne $IsError.Exception) {
		Connect-AzureAD -ErrorVariable IsError

		if ($null -ne $IsError.Exception) {
			Write-Log "Could not connect to Azure AD!"

			exit
		}
	}
}

function Get-KuduHostAPIKey {
	Param (
		[string] $ResourceGroupName,
		[string] $WebAppName
	)

	$AllProtocols = [System.Net.SecurityProtocolType]'Ssl3,Tls,Tls11,Tls12'
	[System.Net.ServicePointManager]::SecurityProtocol = $AllProtocols

	Write-Log "Getting Kudu API access token..."
	$basicAccessToken = Get-KuduApiAuthorisationHeaderValue -ResourceGroupName $ResourceGroupName `
															-WebAppName $WebAppName

	Write-Log "Getting JWT access token..."
	$accessToken = Get-JwtToken -AccessToken $basicAccessToken `
						        -WebAppName $WebAppName

	Write-Log "Getting Kudu host API key..."
	$hostKey = Get-HostAPIKeys -AccessToken $accessToken `
							   -WebAppName $WebAppName							   

	$global:kuduHostKeyCode =  ($hostKey.Content | ConvertFrom-Json).keys[0].value
}

function Add-AzureClientSecret {
	Param (
		[string] $AppRegistrationName
	)

	Write-Log "Looking for app registration.."
	$adApp = Get-AzureADApplication -Filter "DisplayName eq '$AppRegistrationName'" -ErrorVariable IsError

	if ($null -ne $IsError.Exception) {
		Write-Log "Error encountered!"

		exit
	}

	$global:applicationId = $adApp.AppId

	$startDate = Get-Date
	$endDate = $startDate.AddYears(1)

	Write-Log "Adding client secret..."
	$global:aadAppClientSecret = New-AzureADApplicationPasswordCredential -ObjectId $adApp.ObjectId -CustomKeyIdentifier "PostMan_$env:UserName" -StartDate $startDate -EndDate $endDate
}

$installationConfigurationContent = Get-Content -Path $InstallationConfigurationFilePath
$installationConfigurationJson = $installationConfigurationContent | ConvertFrom-Json

$appRegistrationName = $installationConfigurationJson.Azure.FunctionApp.ApplicationRegistrationName
$webAppName = $installationConfigurationJson.Azure.FunctionApp.AppName
$resourceGroupName = $installationConfigurationJson.Azure.ResourceGroupName

if ([string]::IsNullOrWhiteSpace($AppRegistrationName) -or [string]::IsNullOrWhiteSpace($webAppName) -or [string]::IsNullOrWhiteSpace($ResourceGroupName)){
	Write-Log "Insufficient information."

	exit
}

if ($null -eq $Credential) {
	$Credential = Get-Credential
}

ConnectTo-Azure -Credential $Credential

$global:tenantId = (Get-AzContext).Tenant.Id

Add-AzureClientSecret -AppRegistrationName $appRegistrationName

Get-KuduHostAPIKey -ResourceGroupName $resourceGroupName `
				   -WebAppName $webAppName

$environmentJson = "{
	`"id`": `"8d54f99b-3b61-4bcd-9a3f-adc35a3a4f89`",
	`"name`": `"AdvocacyPlatform`",
	`"values`": [
		{
			`"key`": `"tenantId`",
			`"value`": `"$global:tenantId`",
			`"enabled`": true
		},
		{
			`"key`": `"clientId`",
			`"value`": `"$global:applicationId`",
			`"enabled`": true
		},
		{
			`"key`": `"clientSecret`",
			`"value`": `"$($global:aadAppClientSecret.Value)`",
			`"enabled`": true
		},
		{
			`"key`": `"resource`",
			`"value`": `"$global:applicationId`",
			`"enabled`": true
		},
		{
			`"key`": `"token`",
			`"value`": `"`",
			`"enabled`": true
		},
		{
			`"key`": `"callSid`",
			`"value`": `"`",
			`"enabled`": true
		},
		{
			`"key`": `"recordingUri`",
			`"value`": `"`",
			`"enabled`": true
		},
		{
			`"key`": `"transcript`",
			`"value`": `"`",
			`"enabled`": true
		},
		{
			`"key`": `"functionHostCode`",
			`"value`": `"$global:kuduHostKeyCode`",
			`"enabled`": true
		},
		{
			`"key`": `"localPort`",
			`"value`": `"`",
			`"enabled`": true
		},
		{
			`"key`": `"functionAppHost`",
			`"value`": `"$webAppName`",
			`"enabled`": true
		}
	],
	`"_postman_variable_scope`": `"environment`",
	`"_postman_exported_at`": `"2019-04-30T19:00:57.665Z`",
	`"_postman_exported_using`": `"Postman/7.0.9`"
}"

$environmentJson | Set-Content $EnvironmentFilePath
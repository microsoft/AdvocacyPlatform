# Function App

## Developing Changes
Develop changes locally in Visual Studio and perform development testing via the local Function runtime. Refer to the [Work with Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local) documentation for more information.

## Testing Changes
Run all of the unit tests located in the *Microsoft.AdvocacyPlatform.Functions.Tests* project. Add additional tests if new functionality or functions are introduced.

Optionally, run specific functional tests located in the *Microsoft.AdvocacyPlatform.Functions.FunctionalTests* project. There are version of these tests to run the function implementation, call the Function App running locally, and call a remotely deployed Function App. For the latter, populate the *functionSettings.json* file with the appropriate information for your Function App resource. The following describes the information needed:

|Setting Name|Description|
|-|-|
|tenantId|The directory ID of your Azure Active Directory tenant.|
|applicationId|The application ID of the application registered with Azure Active Directory|
|clientSecret|A client secret assigned to the service principal created from the application registration. This and the two previous settings are used to obtain an access token to make calls to the Function App resource.|
|resourceId|The application ID assigned to the Function App|
|functionHostName|The Function App resource name|
|functionHostKey|The host key required to make calls to the Function App|

Additionally, add new functional tests if new functionality or functions are introduced.

## Checking in Changes
Please refer to the [Checking in Changes](../contributing/checking-in-changes.md) guide for more information on the preferred process for integrating changes.
# Configuring Logic Apps Integration
## Overview
The *Integrate-LogicApps.ps1* script reads configuration information for Logic App parameterization using a mapping file in the same directory as the script named *replacementMap.json*.

## Schema
|Property|Type|Description|
|-|-|-|
|addVariables|Dictionary|Specifies key-value pairs of variables to add to the definitions|
|dictionary|Dictionary<string, FileMap>|Specifies a set of transformations to perform with the key representing the Logic App definition file name|
|regex|RegexReplace[]|Specifies a set of patterns and replacement values|
|parameters|ParameterReplace[]|Specifies a set of parameters and the values to set|

### FileMap
|Property|Type|Description|
|-|-|-|
|regex|RegexReplace[]|Specifies a set of patterns and replacement values|

### RegexReplace
|Property|Type|Description|
|-|-|-|
|pattern|Regex|The regular expression to match on|
|value|string|The value to replace matches with|

### ParameterReplace
|Property|Type|Description|
|-|-|-|
|key|string|The name of the parameter|
|value|string|The value to set the parameter to|

## Example

```js
{
  "addVariables": {
    "tenant_id": "[subscription().tenantId]",
    "subscription_id": "[subscription().subscriptionId]"
  },
  "dictionary": {
    "ap-new-case-put-on-queue.json": {
      "regex": [
        {
          "pattern": "LogicAppName",
          "value": "newCase_logicAppName"
        }
      ]
    },
    "ap-address-updated-update-case.json": {
      "regex": [
        {
          "pattern": "LogicAppName",
          "value": "addressUpdateCase_logicAppName"
        }
      ]
    }
  },
  "regex": [
    {
      "pattern": "(@parameters\\('\\$connections'\\)\\['ap-)(.)*(-bingMaps'\\]\\['connectionId'\\])",
      "value": "[concat('@parameters(''$connections'')[''', parameters('bingmaps_Connection_Name'), '''][''connectionId'']')]"
    },
    {
      "pattern": "(@parameters\\('\\$connections'\\)\\['ap-)(.)*(-servicebus'\\]\\['connectionId'\\])",
      "value": "[concat('@parameters(''$connections'')[''', parameters('servicebus_Connection_Name'), '''][''connectionId'']')]"
    }
  ],
  "parameters": [
    {
      "key": "$aadAudience",
      "value": "[parameters('aadaudience_variable')]"
    },
    {
      "key": "$aadClientId",
      "value": "[parameters('aadclientid_variable')]"
    }
  ]
}
```
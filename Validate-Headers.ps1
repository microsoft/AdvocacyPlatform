# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
$copyrightHeader = "// Copyright (c) Microsoft Corporation. All rights reserved."
$licenseHeader = "// Licensed under the MIT License."

$expectedHeaders = @{
    CSharp = @{
        Copyright = "// Copyright (c) Microsoft Corporation. All rights reserved.";
        License = "// Licensed under the MIT License."; 
    };
    PowerShell = @{
        Copyright = "# Copyright (c) Microsoft Corporation. All rights reserved.";
        License = "# Licensed under the MIT License."; 
    }
}

$global:missingHeaders = $false

function Write-Log {
    Param (
        [string]$Message
    )

    Write-Host $message
}

function CheckFor-Headers {
    Param (
        [string]$Path,
        [string]$Extension,
        [PSObject]$Header
    )

    $files = Get-ChildItem -Path $Path -Recurse | Where-Object { $_.Name.EndsWith($Extension) -and (-Not $_.FullName.Contains("\obj") -and -Not $_.FullName.Contains("\bin") -and -Not $_.FullName.Contains("\Properties") -and -Not $_.FullName.Contains("\packages")) }

    foreach ($file in $files) {
        # Write-Log "Checking '$($file.FullName)'..."

        $content = Get-Content -Path $file.FullName -Encoding UTF8
        [string[]]$contentLines = $content.Split([Environment]::NewLine)

        [System.Collections.Generic.List[String]]$contentLinesList = $contentLines

        $copyrightLine = $contentLinesList[0]
        $licenseLine = $contentLinesList[1]

        if ([string]::Compare($Header.Copyright, $copyrightLine) -ne 0) {
            Write-Log "[$($file.FullName)]: Missing copyright header"
            Write-Log "   - $copyrightLine"
            $global:missingHeaders = $True
        }

        if ([string]::Compare($Header.License, $licenseLine) -ne 0) {
            Write-Log "[$($file.FullName)]: Missing license header"
            Write-Log "   - $licenseLine"
            $global:missingHeaders = $True
        }
    }
}

CheckFor-Headers -Path $Path -Extension ".cs" -Header $expectedHeaders.CSharp
CheckFor-Headers -Path $Path -Extension ".ps1" -Header $expectedHeaders.PowerShell

if ($global:missingHeaders -eq $True) {
    throw "Source files are missing copyright and/or license headers! View build output for more information."
}
# Customer Engagement Package

## Spkl
The AdvocacyPlatform uses SparkleXRM to facilitate a semi-automated approach to integrate changes to the Dynamics 365 CRM Customer Engagement package. Please refer to the [SparkleXRM](http://www.sparklexrm.com/s/default.html) documentation for more information.

## Exporting Changes
To export changes, perform the following steps:

1. Open a new PowerShell terminal.
2. Navigate to the following directory:

```ps
{localRepositoryDirectory}\UI\
```

### Individual Steps
#### Exporting the Customer Engagement Package
4. Run the following command:

```ps
.\Update-LocalDynamicsSolution.ps1 -NoPack -NoWebResources -NoBuild
```

5. The individual files describing the customizations will be updated in the *AdvocacyPlatform* project.

#### Exporting Web Resources
6. Run the following command:

```ps
.\Update-LocalDynamicsSolution.ps1 -NoUnPack -NoBuild
```

7. The individual web resource files will be updated in the *AdvocacyPlatformWebResources* project.

#### Generating a new Customer Engagement Package ZIP
8. Run the following command:

```ps
.\Update-LocalDynamicsSolution.ps1 -NoUnPack -NoWebResources
```

9. The solution will be re-packed, the projects will be built, and the *AdvocacyPlatformSolution_managed.zip* and *APConfigurationData.zip* files in the *AdvocacyPlatform* project will be updated.

### Single Step
Alternatively, run the following command to perform all of the actions consecutively:

```ps
.\Update-LocalDynamicsSolution.ps1
```

### Checking in Changes
Please refer to the [Checking in Changes](../contributing/checking-in-changes.md) guide for more information on the preferred process for integrating changes.
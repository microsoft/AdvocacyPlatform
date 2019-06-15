# Installer
## Contributing
### Installer UI
The installer is a simple WPF application. The following details the various components used to orchestrate the installation process.

#### InstallerWizard
The InstallerWizard manages the behavior and navigation of the installer UI.

##### WizardPageBase
Pages are all derived from the WizardPageBase abstract class which provides a set of base functionality for all pages. Derived classes can override all of this base functionality.

#### OperationRunner
The OperationRunner is a simple class to sequentially run and track a set of operations. The OperationRunner will halt execution if the current operation's validation function indicates an operation was unsuccessful.

##### Operation
Operations describe a particular action to take and how to validate the end result.

### Testing Changes
Run all unit tests in the *AdvocacyPlatformInstaller.Tests* project.

If ARM resource template changes were introduced, run all unit tests in the *AdvocacyPlatformInstaller.FunctionalTests* project.

### Checking in Changes
Please refer to the [Checking in Changes](../contributing/checking-in-changes.md) guide for more information on the preferred process for integrating changes.
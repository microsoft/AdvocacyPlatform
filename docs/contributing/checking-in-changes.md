# Checking in Changes
## Coding Conventions
Coding conventions for the Advocacy Platform project are informed and enforced by StyleCop. All projects should use the rule set located at the root of the repository named *APStyleCopRules.ruleset*. This is a slightly modified version of the Microsoft Managed Recommended Rules.

Any new projects with managed code should add the [StyleCop.Analyzers](https://www.nuget.org/packages/StyleCop.Analyzers/) NuGet package and configure the project to use the *APStyleCopRules.ruleset* rule set.

Before merging commits, please ensure all StyleCop warning have been addressed.
# Package Dependencies Validation

### What is this validation?
This validation ensures that the dependencies of your package are complete and useful.

### Why do we need this validation?
This validation is needed to ensure packages are not being published with unnecessary dependencies which could fill the User's project with unneeded packages. It also ensures that the package will pull on all the required dependencies to work.

# Errors
### Unused package dependency in manifest
Some dependencies in the package manifest are not used. Please make sure they are useful or remove them to avoid cluttering.

### Missing dependency in manifest
Some dependencies are missing from the package manifest. Please make sure that all necessary package dependencies are in the package manifest.

### Malformed guid reference
Please make sure that all guid references are in the following format "GUID:17b36165d09634a48bf5a0e4bb27f4bd". You can read more about the formatting of the assembly files here: https://docs.unity3d.com/Manual/AssemblyDefinitionFileFormat.html

### Unknown reference
Please make sure that all references are of existing packages.

### Unknown precompiled reference
Please make sure that all precompiled references are from existing packages.
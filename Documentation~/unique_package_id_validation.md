# Unique Package ID Validation
### What is this validation?
This validation ensures that the package uploader is not attempting to upload a version of their package that already exists on the Asset Store Registry.

### Why do we need this validation?
This validation is needed in order to ensure that there is a unique package id (name@version) with each new release of a package on the Asset Store. When releasing new versions of a package, it is important to update the version of the package so that users can avoid unnecessary issues with their project.

# Errors
### Package name and version must be provided
The package manifest name and version must not be null or whitespace. To resolve this error make sure that the package.json file is readable and contains fields for "name" and "version", then run the validation again. For more info visit https://docs.unity3d.com/Manual/upm-manifestPkg.html

### Package id already exists
The package id (name@version) of the package you are trying to validate already exists on the Asset Store Registry. To resolve this error, change the name of the package or increase the version of the package to one that does not exist on the registry already.
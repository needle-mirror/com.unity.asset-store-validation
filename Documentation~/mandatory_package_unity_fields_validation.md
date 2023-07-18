# Mandatory Package Unity Fields Validation

### What is this validation?
This validation ensures that a package has `unity` and `unityRelease` fields values set on the package manifest. Also, it ensures the values are greater than the minimum required version and that the format of those values is correct. 

### Why do we need this validation?
The Package Manager uses `unity` and `unityRelease` fields to establish the minimum compatible editor version that the package supports. These two fields help Package Manager decide if a package should be displayed or not on the Package Manager UI.

# Errors

### Package unity field mandatory
The `unity` field is mandatory. This field indicates the lowest Unity **version** the package is compatible with. The expected format is <MAJOR>.<MINOR> (e.g 2018.4).
To specify a minimum Unity release version, please use a combination of `unity` and `unityRelease` fields, for example:
* unity: 2018.4
* unityRelease: 0b4
To point to a specific patch, use the [unityRelease](https://docs.unity3d.com/Manual/upm-manifestPkg.html#unityRelease) property as well.

### Package unity field min version
The `unity` field value in package manifest is smaller than 2021.3, which is the minimum version that could be specified.
This value must be equal to or greater than 2021.3. Please update the value accordingly and run the validation again.  

### Pakage unityRelease field mandatory
The `unityRelease` field is mandatory. This field indicates the lowest Unity **patch** that the package is compatible with. The expected format is <UPDATE><RELEASE> (e.g. 0b4).
To specify a minimum Unity release version, please use a combination of `unity` and `unityRelease` fields, for example:
* unity: 2018.4
* unityRelease: 0b4

### Package unityRelease field min version
The `unity` field value in package manifest equals 2021.3 while the `unityRelease` field value is smaller than 26f1 (2021.3.26f1 is the minimum version that could be specified). If `unity` value is equal to 2021.3, then the `unityRelease` field value must be equal or greater than 26f1. Please update the value accordingly and run the validation again.
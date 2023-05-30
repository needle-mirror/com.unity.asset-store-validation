# Version Field Validation

### What is this validation?
The version of the package, contained in the named package.json, must be a valid [Semver](http://www.semver.org) value in order
to be consumed by the editor

### Why do we need this validation?
This validation was created in accordance with the Unity Standards US-0005.
The package, dynamic template, or feature set's versioning follows Semantic Versioning (SemVer), a versioning strategy allowing authors to provide tools with information about the type of changes included in a given version, compared with the previous version.

# Errors
## version needs to be a valid Semver
The value of the `version` field in the package.json file does not contain a valid Semver value. A valid Semver value follows the format of x.y.z[-tag], with the *-tag* section being optional.

*Examples of valid semver strings:*

* 1.0.0
* 0.0.1
* 0.1.1-20200101 
* 0.0.1-preview

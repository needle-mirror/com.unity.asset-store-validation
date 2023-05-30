# Manifest Name Field Validation

### What is this validation?
The package manifest's name field must follow some standards for the package to work properly.

### Why do we need this validation?
This validation was created in accordance with Unity Standards US-0006.

# Errors
### Package name contains uppercase letters
According to Unity Standards there should be no uppercase letters in the "name" field of a package manifest. To resolve this error make sure that there are no uppercase letters in the "name" field of your package.json file, then run the validation again.

### Package name is too long
According to Unity Standards the "name" field in a package manifest cannot exceed 214 characters. To resolve this error make sure that the "name" field in your package.json file does not exceed 214 characters, then run the validation again.

### Package name ends in forbidden extension
According to Unity Standards the "name" field in a package manifest cannot end with ".plugin", ".bundle", or ".framework". This is due to the fact that Unity may treat these packages differently than a normal package. To resolve this error make sure that the "name" field in your package.json file does not end in any of these extensions, then run the validation again.

### Package name must contain dot separator
Unity package names must follow the reverse domain name notation and, as such, contain at least one dot '.' separator in between two word characters. To resolve this error make sure that the "name" field in your package.json file contains at least one dot '.' separator in between two word characters, then run the validation again.

### Package name does not match regex
Unity package names must abide by the regex pattern "^[a-z0-9]([a-z0-9-_]|.[a-z0-9]){1,213}$". If the package you are attempting to validate does not match this pattern then it cannot be validated. To resolve this issue make sure that the "name" field in your package.json file abides by this regex pattern.

### Asset Store context contains Unity identifier
Packages that are meant for the Asset Store cannot begin with the "com.unity." identifier. To resolve this error make sure that the "name" field in your package manifest does not begin with "com.unity.", then run the validation again.

# Author Name Field Validation

### What is this validation?
The package author's name field must follow some guidelines for the package to work properly.

### Why do we need this validation?
This validation should guarantee the validity of 'Author' field in the package definition file.

# Errors
### Author is mandatory
The "Author" name field of a package definition must be specified when publishing to the Asset Store. To resolve this error make sure the package.json file has an "Author" field in one of the following formats: "author": "Author Name", or "author": {"name": "Author name"}, then run the validation again. Refer the formal documentation for more details [Required properties in Package manifest](https://docs.unity3d.com/Manual/upm-manifestPkg.html)

### Author name is too long
The "Author" name field in a package definition cannot exceed 512 characters. To resolve this error make sure that the "Author" name field in your package.json file does not exceed 512 characters, then run the validation again.

### Author is unity author name
The "Author" name field in a package definition must be something other than the following names:
-Unity
-Unity Technologies
-Unity Technologies ApS
-Unity Technologies SF
-Unity Technologies Inc.
To resolve this error make sure the package.json file has an "Author" name that is different from the mentioned names in the list above, then run the validation again.

### Email is too long
The "Email" field in a package definition cannot exceed 320 characters. To resolve this error make sure that the "Email" field in your package.json file does not exceed 320 characters, then run the validation again.

### Email has invalid format
The "Email" field in an author field of a package definition must abide by the regex pattern "\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9]{2,}(?:[a-z0-9-]*[a-z0-9])?)\Z". If the "Email" field you are attempting to validate does not match this pattern then it cannot be validated. To resolve this error make sure that the "Email" field in the author field of your package.json file abides by this regex pattern.

### Url is too long
The "URL" field in the author field of a package definition cannot exceed 2048 characters. To resolve this error make sure that the "URL" field in the author field of your package.json file does not exceed 2048 characters, then run the validation again.

### Url has invalid format
The "URL" field in the author field of a package definition must abide by the regex pattern "^http(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?([\?]?[^{}|\^~[\]` ])*$". If the "URL" field you are attempting to validate does not match this pattern then it cannot be validated. To resolve this error make sure that the "URL" field in the author field of your package.json file abides by this regex pattern.

# Warnings
### Url is unreachable
It is recommended that the "URL" field in a package definition be reachable. To resolve this warning make sure that the "URL" field in your package.json file is reachable, then run the validation again.

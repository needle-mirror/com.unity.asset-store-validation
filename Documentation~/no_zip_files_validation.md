# No Zip Files Validation
### What is this validation?
This validation ensures that there are no zip files found in a package.

### Why do we need this validation?
The upm pack operation ignores zip files when packing the package. This means that any zip files found within a package would not be uploaded to the Asset Store. This validation is here to ensure that the user knows which zip files will be ignored when packing the package.

## Errors
### Zip File Found
If you see this error, it means that one or more zip files were found within the package. If you wish to keep the contents of these files, you can convert the zip files into .unitypackage files.
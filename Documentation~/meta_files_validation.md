# Meta Files Validation

### What is this validation?
Unity generates .meta files for every file and folder in a Unity project.
It does not generate them for ignored folders, i.e. files and folders contained in folders starting with "." or ending with "~"". However, Samples in a package will require .meta files that will be copied over to the Assets folder so these are an exception.

In general, errors will appear for missing or outstanding .meta files within a package. However meta files found within ignored folders will produce warnings (With the exception of Documentation~ which will produce an error, and Samples~ which will expect meta files).

### Why do we need this validation?
This validation was created in accordance with the Unity Standards.
It ensures that the appropriate metafiles are present for all files and folders that require them while also checking to ensure no useless meta files are found.

# Errors
### Documentation~ folder should not have any metafiles inside
Unity does not generate meta files for folders ending in the '~' character (Samples~ being an exception). The 'documentation~' folder in a package should not have any meta files inside and therefore if any are found than the meta files validation will throw an error. To resolve this error simply remove the specified meta files from your package and re-run the validation.

### Did not find corresponding meta file
The package you are validating is missing a needed meta file for one of the files in your package. To resolve this error inspect the message and find where the missing meta file is needed.

### Useless meta file found in package
The package you are validating contains a meta file with no corresponding normal file. To resolve this error find the useless meta file in your package and delete it, then run the validation again.

# Warnings

### Useless meta file found in hidden folder in package
A meta file is found within a hidden folder of your package that does not need to exist. To resolve this warning simply remove the meta file from the location specified and run the validation again.
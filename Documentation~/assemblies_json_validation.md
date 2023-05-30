# Assemblies Json Validation
### What is this validation?
Assembly files (.asmdef and .asmref) contents are in the json format. Sometimes people can modify these by hand, therefore it is helpful to ensure that those files have at least a valid json format inside.

# Errors
### Assemblies must be valid Json
The package contains assembly files (.asmdef & .asmref) with improper Json formatting. This could be a missing ':' or ',' character for example. To resolve this error simply ensure that your assembly files are in proper Json formatting and re-run the validation.

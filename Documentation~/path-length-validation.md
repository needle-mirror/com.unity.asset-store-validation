# Path Length Validation

### What is this validation?
This validation ensures that the all the files and folders in the package can be properly parsed and imported by Unity, by limiting the path length to 140 characters.

### Why do we need this validation?
Unity's subsystems does not work with long path on Windows, we want to ensure that the paths are not too long and are up to 140 characters.

# Errors
### Path too long
Please ensure that the paths of your files and directories do not exceed 140 characters so they can be correctly loaded by Unity.
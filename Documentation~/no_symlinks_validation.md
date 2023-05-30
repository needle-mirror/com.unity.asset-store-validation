# No Symlinks Validation
### What is this validation?
This validation checks for the existence of symbolic links (shortcuts on Windows) within your package and will throw an error if any are detected.

### Why do we need this validation?
Using symlinks in Unity projects may cause your project to become corrupted if you create multiple references to the same asset, use recursive symlinks or use symlinks to share assets between projects used with different versions of Unity. This validation ensures that there are no symlinks within a package before publishing.

## Errors
### Symlink found within project
If you see this error, it means that one or more symlinks were found within your project. In order to resolve this error, inspect the validation output to find which specific files threw these errors, then remove them and re-run the validation.
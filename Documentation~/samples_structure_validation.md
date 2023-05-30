# Samples Structure Validation

### What is this validation?
A package can have Samples added to it. For the samples to be visible they need to For the Samples to be visible, they need 2 things: an entry in the package.json file and a folder containing the data under Samples~. The samples validation insures that the samples are in accordance with all the requirements. 

### Why do we need this validation?
This validation ensures that Samples in a package are properly defined so that they can be displayed under the Package Manager UI and then imported into the project.

# Errors
### Missing samples folder
There are samples in the samples array in the package manifest but no Samples~ folder is found.

### Empty samples folder
The Samples~ folder should contain at least 1 asset that will be imported by Unity. Hidden files such as `.file` or `file~`, or special files like `*.meta` or `.sample.json` will not be imported by the editor

### Samples array in Package manifest is empty
There are samples found in the Samples~ folder but the samples field in the package manifest is empty.

### Missing Samples entry in Package manifest
Some files have been found in the Samples~ that will not be imported as part of any samples in your package manifest. Please ensure that all files you include in Samples~ are usable by the `samples` definition in your package.json file.
Samples entries are:
{"samples": [
  { 
    "path":"Samples~/PathToSamples",
    "description": "sample description",
    "display":"sample display"
  }
]}

### Folder is missing from the samples
A folder that was not part of the samples array in the package manifest was found. Please make sure that all folders that are direct children of the Samples~ folder have either an entry in the samples array or the samples array contains an entry with only the Samples~ path i.e. "path" : "Samples~".

### Samples should be in Samples~ folder
Every sample should be in Samples~ folder. Folder names such as .Samples, Samples or .Samples~ are not supported.

### Empty property for sample
The following properties {path, displayName, description} should be filled for every sample in the samples array in the package manifest.

### Duplicated sample description
The value of the description property should be unique for the whole samples array in the package manifest.

### Duplicated sample display name
The value of the display name property should be unique for the whole samples array in the package manifest.

### Duplicated path
The value of the path property should be unique for the whole samples array in the package manifest.

### Directory does not exist.
Every value in the path property in the samples array in the package manifest should correspond to an existing folder in the file system.

# Information
### No samples found
There are no samples found in the Samples~ folder and the samples array in the package manifest is empty. No validation necessary.
 
# Warnings
### Invalid samples folder found
The samples folder should be named **Samples~**. Folder names such as **.Samples**, **Samples** or **.Samples~** are not supported. Please rename your folder to **Samples~**.

### Hidden files or folders found.
There are hidden files and/or folders in the Samples~ folder.

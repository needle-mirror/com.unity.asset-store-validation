# Documentation Validation

### What is this validation?
This validation ensures that packages contain documentation so that consumers have access to the information required to 
use the features in it.

### Why do we need this validation?
To ensure that the minimum documentation exists for your package before publishing it to users.

### What is valid documentation?
Packages for unity consider two kinds of documentation as **valid**:
1. Documentation URL: In your `package.json`, fill the field `documentationUrl` field with the URL to your online documentation
1. Offline documentation: Create a folder in your package root called `Documentation~`, and inside of it place your documentation 
files. 
The following extensions are accepted as valid documentation:
* .pdf
* .html
* .rtf
* .md
* .txt

_Note: Images are accepted inside this folder in the case that you would like to support your offline documentation with images._

# Errors
### No Documentation Found
No documentation was detected within the package. Validate that you have set a value for the `documentationUrl` field in 
your `package.json`, and/or that you have provided offline documentation inside the `Documentation~` folder. 

### Documentation Folder Capitalization
The documentation folder must be named `Documentation~` (with capital **D**). This ensures consistency for documentation folders found in Asset Store packages.

### Empty Documentation Files
At least one file within the `Documentation~` must work as the entry point for your documentation. This file must not be empty and must end in one of the extensions listed above.  

# Warnings

### Documentation Url Not Reachable
The URL specified in the `documentationUrl` field in your `package.json` is unreachable. Validate that the URL is accurate, 
that the website can be reached, or that you are not experiencing networking problems at the time of running the validation. 

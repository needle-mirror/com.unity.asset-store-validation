# Minimum Package Manifest Validation
### What is this validation?
The package manifest is contained in a file named package.json.
For a package to be consumed by the editor, the bare minimum requirement is that it contains the package manifest with a valid json of 2 properties: 
- name
- version

### Why do we need this validation?
This validation was created in accordance with the Unity Standards US-007
A valid package manifest file included in root folder must meet certain requirements.

# Errors
### Missing name and version fields
The package manifest is missing both of the "name" and "version" fields. To resolve this error make sure that both of the "name" and "version" fields in the package.json file are defined and have values.

### Missing name field
The package manifest is missing the "name" field. To resolve this error make sure that the "name" field in the package.json file is defined and has a value.

### Missing version field
The package manifest is missing "version" field. To resolve this error make sure that the "version" field in the package.json file is defined and has a value.

# Assemblies Definition Validation

### What is this validation?
This validation ensures that the structure and relationship between assembly definition files (.asmdef and .asmref) and scripts (.cs) inside a package follows Unity standards. For every script in a package, there must be an associated assembly file in order to ensure that the code gets built into a dll, and can be used properly in the project.
To learn more about assemblies in general, go to https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html

### Why do we need this validation?
To ensure that the code found in packages gets built into a dll, we need to ensure that an assembly exists (.asmdef or .asmref). This validation guarantees that all code has an associated assembly file, and that there are no useless assembly files.

# Errors
### More than one assembly definition
The assembly definition (asmdef or asmref) should be unique per folder. To resolve this error make sure there is not more than one assembly definition in the same folder. 

### Script found without asmdef or asmref associated
The script does not have an assembly definition file (asmdef or asmref) associated with it. To resolve this error, make sure each script has an assembly definition file in its folder or in an ancestor folder. However any files or folders found within 'Samples~' are allowed to exists without an associated assembly definition. 

### Assembly definition found without script associated
An assembly definition file (asmdef or asmref) does not have any scripts (.cs) associated with it. To resolve this error make sure that each assembly definition file has at least one script associated with it, or delete the assembly file if it is useless. Associated scripts can be in the same assembly definition folder or in a descendant folder. However, in the case of descendant folders some rules apply: 
- There should be no other assembly definition 'B' at any point in the folders path between the first assembly definition 'A' and the script.
- There should not be a folder 'Samples~' at any point in the folders path between the first assembly definition 'A' and the script. A script found at any level under a 'Samples~' folder will not count as an associated script for an assembly definition 'A' located in an ancestor folder. 
 

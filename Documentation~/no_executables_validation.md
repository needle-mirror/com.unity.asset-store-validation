# No Executables Validation

### What is this validation?
A package should not contain any executable files due to security concerns. All files that have the following extensions : 
```
*.bat
*.bin 
*.com 
*.csh 
*.dom 
*.dmg 
*.exe 
*.js 
*.jse 
*.lib 
*.msi 
*.msp
*.mst
*.pkg
*.ps1
*.sh
*.vb
*.vbe
*.vbs
*.vbscript
*.vs
*.vsd
*.vsh 
```
will be flagged. As well as files that are named "msp.rsp" or "csc.rsp".

### Why do we need this validation?
Why do we reject executables? There are many different security issues surrounding executables. For example, they can be used by viruses and malware to deceive users into opening the files.

# Errors
### Package contains executables
Your package must not contain an executable file, installer program or application. If your plugin requires an external program to run, please remove the installer program from your package and write the instructions on how to download and install the installer program in your documentation.

### Package contains rsp files
The following rsp files are restricted : "msp.rsp", "csc.rsp". Rsp files could change the compilation of all of the user project, not just your package. See https://docs.unity3d.com/Manual/PlatformDependentCompilation.html global custom #defines section at the end of the page to learn more.

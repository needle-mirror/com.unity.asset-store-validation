# No Jpgs Validation
### What is this validation?
This validation ensures that there are no files ending in ".jpg" or ".jpeg" found within the structure of a package. However, these files are allowed to be found in the "Documentation~" or "Tests" folders since they do not suffer as much from compression issues.

### Why do we need this validation?
The final build suffers when using assets made with compressed file formats such as ".jpg" or ".jpeg". This validation ensures that there are no images within a package (".jpg" and ".jpeg") that will suffer from these issues.

<b>* This validation prevents all jpg extension variations including:</b>

```
.jpg
.jpeg
.jpe
.jif
.jfif
.jfi
```

# Errors
### Package contains jpgs
This error occurs when one or more jpg files (".jpg" or ".jpeg") are found within the structure of your package. The prohibited files can be found by viewing the results of the validation and examining the logs. To resolve this error, remove these files or convert them to a different file format (".png" for example), then run the validation again.
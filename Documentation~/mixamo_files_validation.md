# Mixamo Files Validation

### What is this validation?
A package should not contain any Mixamo files to avoid copyright problems. Mixamo files are detected by looking for a "mixamorig*" in the model's hierarchy if in editor or by reading the header of the file and looking for Mixamo Inc.

### Why do we need this validation?
Mixamo files are owned by Adobe and as such do not comply with the following part of the EULA : Submissions do not include content that is not allowed to be resold, or not allowed to be utilized in commercial products due to its EULA. This includes content from other Asset Store products.

# Errors
### Package contains mixamo files
Package should not contain content that is not allowed to be resold, or not allowed to be utilized in commercial products due to its EULA. See the [Submission Guidelines - Section 1.2.b](https://assetstore.unity.com/publishing/submission-guidelines#1.2-legal-0Zdi)
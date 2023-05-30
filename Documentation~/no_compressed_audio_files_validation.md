# No Compressed Audio Files Validation
### What is this validation?
This validation ensures that packages published to the asset store do not contain compressed lossy audio files (.mp3 or .ogg).

### Why do we need this validation?
Lossy audio files have the potential to suffer from compression and lose their overall sound quality. In order to preserve better quality for our users, it is better to provide them with uncompressed formats such as .wav. 

# Errors
## Compressed audio file found
One or more compressed audio files (.mp3 or .ogg) were found in your project and must be removed or converted to a lossless audio format such as .wav. To resolve this error open up the validation results in order to view which files were found, then remove or convert the files, and run the validation again.

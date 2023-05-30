# Http helper documentation
### What is this documentation?
This documentation provides greater detail for errors and warnings associated with http requests.

# Errors

### Bad request
The Bad Request response status code (400) indicates that the server cannot or will not process the request due to something that is perceived to be a client error (malformed request syntax, invalid request message framing, or deceptive request routing). One possible reason for receiving this response code is that your package name or version contains illegal characters ("%" for example) and the server cannot correctly process the request. In order to avoid this error, ensure that the package name conforms to the Unity Package Manager naming convention (https://docs.unity3d.com/Manual/cus-naming.html) and that the version respects Semantic Versioning (https://semver.org/).  For more information visit https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/400.

# Warnings
### Unexpected Http status codes
This validation utilizes the Asset Store Production Registry and may encounter unexpected responses from the server from time to time. In the event that this occurs, read the logs to find the specific Http status code returned by the server, and consult https://developer.mozilla.org/en-US/docs/Web/HTTP/Status to diagnose the problem further.
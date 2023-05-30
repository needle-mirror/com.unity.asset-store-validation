# Unity Field Validation

This validation ensures that a package contains a valid `unity` field value, which is used by Package Manager to identify
a package compatibility with Unity 

# Errors

### unity is invalid
The `unity` field is an invalid value. This field indicates the lowest Unity version the package is compatible with. The expected format is <MAJOR>.<MINOR> (e.g 2018.4).
If omitted, the package is considered compatible with all Unity versions 2017.4+.
If you want to specify a minimum Unity release version, please use a combination of unity and unityRelease fields, for example:
* unity: 2018.4
* unityRelease: 0b4

### unityRelease is invalid
The `unityRelease` field is an invalid value. This field indicates the specific release of Unity that the package is compatible with. The expected format is <UPDATE><RELEASE> (e.g. 0b4).
If the unity field is omitted, this field is ignored.
If you want to specify a minimum Unity release version, please use a combination of unity and unityRelease fields, for example:
* unity: 2018.4
* unityRelease: 0b4

### unityRelease without unity
The `unityRelease` field is included, while the `unity` field is not present.
Add the `unity` field or remove the `unityRelease` field.

using System;

namespace UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite
{
    internal class PackageResponseParseException: Exception
    {
        public PackageResponseParseException(string packageName) : base($"Could not parse response for package name {packageName}") {}
    }
}
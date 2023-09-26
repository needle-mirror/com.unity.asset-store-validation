using System;
using System.Net;

namespace UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite
{
    internal class PackageRequestFailedException: Exception
    {
        public PackageRequestFailedException(string packageName, HttpStatusCode statusCode): base($"Could not get package information for package name: {packageName}. The request failed with status {statusCode}") {}
    }
}
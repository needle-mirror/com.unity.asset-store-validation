using System;

namespace UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite
{
    internal class NpmUnusableException: Exception
    {
        public NpmUnusableException(string disableMessage) : base($"Npm cannot be used because it previously failed with message: {disableMessage}") {}
    }
}
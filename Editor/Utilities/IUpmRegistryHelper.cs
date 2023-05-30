using System;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    internal interface IUpmRegistryHelper
    {
        string GetPublisherMetadata(Action<string> warning, Action<string> error);

        string RetrievePackageMetaDataFromAssetStoreRegistry(string packageName, Action<string> warning,
            Action<string> error);
    }
}

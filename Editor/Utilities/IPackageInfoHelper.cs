using System.Collections.Generic;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    interface IPackageInfoHelper
    {
        string GetPackageIdFromReferenceName(string referenceName);
        string GetPackageIdFromGuid(string referenceGuid);
        Dictionary<string, string> GetDllsFromRegisteredPackages();
    }
}

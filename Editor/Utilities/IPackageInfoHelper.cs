using System.Collections.Generic;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    interface IPackageInfoHelper
    {
        string GetPackageNameFromReferenceName(string referenceName);
        string GetPackageNameFromGuid(string referenceGuid);
        Dictionary<string, string> GetDllsFromRegisteredPackages();
    }
}

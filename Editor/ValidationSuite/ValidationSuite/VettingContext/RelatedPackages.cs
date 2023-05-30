using System;

namespace UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite
{
    class RelatedPackage
    {
        public string Name;
        public string Version;
        public string Path;

        public RelatedPackage(string name, string version, string path)
        {
            Name = name;
            Version = version;
            Path = path;
        }
    }
}

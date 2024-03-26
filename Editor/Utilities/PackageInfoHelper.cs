using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class PackageInfoHelper : IPackageInfoHelper
    {
        /// <summary>
        /// Fetch the package name by loading the PackageInfo object from the assembly related to the provided reference name
        /// </summary>
        /// <param name="referenceName"></param>
        /// <returns></returns>
        public string GetPackageIdFromReferenceName(string referenceName)
        {
            try
            {
                var assembly = Assembly.Load(referenceName);
                var packageInfo = PackageInfo.FindForAssembly(assembly);

                // not using packageId because if the package is local the packageId has the local path and not the version
                return packageInfo == null ? string.Empty : packageInfo.name + "@" + packageInfo.version;
            }
            catch (FileNotFoundException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Fetch the package name by loading the PackageInfo object from the provided GUID
        /// </summary>
        /// <param name="referenceGuid"></param>
        /// <returns></returns>
        public string GetPackageIdFromGuid(string referenceGuid)
        {
            var objectPath = AssetDatabase.GUIDToAssetPath(referenceGuid);
            if (string.IsNullOrWhiteSpace(objectPath))
            {
                return string.Empty;
            }

            var packageInfo = PackageInfo.FindForAssetPath(objectPath);
            
            // not using packageId because if the package is local the packageId has the local path and not the version
            return packageInfo == null ? string.Empty : packageInfo.name + "@" + packageInfo.version; 
        }

        /// <summary>
        /// Get the collection of DLLs existing inside packages in the project
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetDllsFromRegisteredPackages()
        {
            var registeredPackagesDlls = new Dictionary<string, string>();
            var registeredPackages = UnityConnectUtilities.GetAllRegisteredPackages();

            foreach (var registeredPackage in registeredPackages)
            {
                // Convert all guids into file paths and get only those that have a .dll extension
                var assets = AssetDatabase.FindAssets("", new[] {registeredPackage.assetPath}).
                    Select(guid => AssetDatabase.GUIDToAssetPath(guid)).
                    Where(path => Path.GetExtension(path) == ".dll");

                foreach (var path in assets)
                {
                    registeredPackagesDlls.TryAdd(Path.GetFileNameWithoutExtension(path), registeredPackage.packageId);
                }
            }

            return registeredPackagesDlls;
        }
    }
}
using UnityEditor.PackageManager.AssetStoreValidation;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;

namespace Editor.ValidationSuite.ValidationSuite.ValidationTests
{
    internal class BaseHttpValidation: BaseValidation
    {
        internal IReachable HttpUtils { get; set; }
        
        protected override void Run()
        {
            HttpUtils ??= Context.ValidationType == ValidationType.InternalTesting
                ? new OfflineHttpUtils()
                : new HttpUtils();
        }

        protected UrlStatus CheckUrlStatus(string url)
        {
            return AsvUtilities.CheckUrlStatus(url, HttpUtils);
        }
    }
}
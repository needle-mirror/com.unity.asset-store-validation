using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class AssetStorePublishOperationValidationUtility : IAssetStorePublishOperationValidation
    {
        public bool? IsAssetStoreTermsAccepted()
        {
            return UnityConnectUtilities.isAssetStoreTermsAccepted;
        }

        public bool IsPackageSizeValid(float publishPackageSizeMb, float packageSizeLimitMb)
        {
            return publishPackageSizeMb <= packageSizeLimitMb;
        }

        public bool IsPackageVersionExistsOnProduction(VettingContext vettingContext)
        {
            return vettingContext.PackageVersionExistsOnProduction;
        }

        public bool? IsUserAPublisher()
        {
            return UnityConnectUtilities.HasUserPublisherId;
        }

        public bool IsUserLogin()
        {
            return UnityConnectUtilities.IsUserLogin();
        }
    }
}

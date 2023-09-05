using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    interface IAssetStorePublishOperationValidation
    {
        public bool IsUserLoggedIn();
        public bool IsPackageVersionExistsOnProduction(VettingContext vettingContext);
        public bool? IsUserAPublisher();
        public bool? IsAssetStoreTermsAccepted();
        public bool IsPackageSizeValid(float publishPackageSizeMb, float packageSizeLimitMb);
    }
}

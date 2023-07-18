using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    interface IAssetStorePublishOperationValidation
    {
        public bool IsUserLogin();
        public bool IsPackageVersionExistsOnProduction(VettingContext vettingContext);
        public bool? IsUserAPublisher();
        public bool? IsAssetStoreTermsAccepted();
        public bool IsPackageSizeValid(float publishPackageSizeMb, float packageSizeLimitMb);
    }
}

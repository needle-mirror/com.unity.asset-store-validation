using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class IsPackageSizeValidPublishValidation : BaseValidation
    {
        IAssetStorePublishOperationValidation m_AssetStorePublishOperationValidationUtility;

        internal static readonly string k_DocsFilePath = "package_size_publish_validation";

        internal static readonly string k_PackageSizeExcceded =
            $"Package size exceeded. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-size-exceeded")}";

        internal const float k_PackageSizeLimitMb = 550;

        internal IAssetStorePublishOperationValidation AssetStorePublishOperationValidationUtility { get => m_AssetStorePublishOperationValidationUtility; set => m_AssetStorePublishOperationValidationUtility = value; }

        public IsPackageSizeValidPublishValidation()
        {
            TestName = "Package Size";
            TestDescription = $"Verify that the package size is below {k_PackageSizeLimitMb}mb.";
            TestCategory = TestCategory.ContentScan;
            SupportedValidations = new[] { ValidationType.AssetStore, ValidationType.AssetStorePublishAction, ValidationType.InternalTesting };
            m_AssetStorePublishOperationValidationUtility = new AssetStorePublishOperationValidationUtility();
        }

        protected override void Run()
        {
            TestState = TestState.Succeeded;

            if (m_AssetStorePublishOperationValidationUtility.IsPackageSizeValid(Context.PublishPackageSizeMb, k_PackageSizeLimitMb)) return;

            AddError(k_PackageSizeExcceded);
        }

    }
}

using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class IsAssetSoreTermsAcceptedPublishValidation : BaseValidation
    {
        IAssetStorePublishOperationValidation m_AssetStorePublishOperationValidationUtility;
        internal static readonly string k_DocsFilePath = "asset_store_terms_accepted_publish_validation";

        internal static readonly string k_UserHasNotAcceptedAssetStoreTerms = $"User has not accepted the Terms. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "user-has-not-accepted-the-terms")}";
        internal static readonly string k_NetworkError = $"Network failure. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "network-failure")}";

        internal IAssetStorePublishOperationValidation AssetStorePublishOperationValidationUtility { get => m_AssetStorePublishOperationValidationUtility; set => m_AssetStorePublishOperationValidationUtility = value; }

        public IsAssetSoreTermsAcceptedPublishValidation()
        {
            TestName = "Asset Store Terms Accepted Publish";
            TestDescription = "Verify that the publisher has already accepted the Asset Store terms of service.";
            TestCategory = TestCategory.ContentScan;
            SupportedValidations = new[] { ValidationType.AssetStore, ValidationType.AssetStorePublishAction };
            DependsOn = new[] { typeof(IsUserLoginPublishValidation) };
            m_AssetStorePublishOperationValidationUtility = new AssetStorePublishOperationValidationUtility();
        }

        protected override void Run()
        {
            TestState = TestState.Succeeded;
            var isTermAccepted = m_AssetStorePublishOperationValidationUtility.IsAssetStoreTermsAccepted();

            if (isTermAccepted.HasValue && isTermAccepted.Value) return;

            AddError(isTermAccepted.HasValue ? k_UserHasNotAcceptedAssetStoreTerms : k_NetworkError);
        }
    }
}

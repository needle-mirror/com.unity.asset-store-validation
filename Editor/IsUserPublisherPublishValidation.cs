using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class IsUserPublisherPublishValidation : BaseValidation
    {
        IAssetStorePublishOperationValidation m_AssetStorePublishOperationValidationUtility;

        internal static readonly string k_DocsFilePath = "publisher_account_exist_publish_validation";

        internal static readonly string k_UserHasNotPublisherAccount = $"User does not have a publisher account. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "user-has-not-a-publisher-account")}";
        internal static readonly string k_NetworkError = $"Network failure. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "network-failure")}";

        internal IAssetStorePublishOperationValidation AssetStorePublishOperationValidationUtility { get => m_AssetStorePublishOperationValidationUtility; set => m_AssetStorePublishOperationValidationUtility = value; }

        public IsUserPublisherPublishValidation()
        {
            TestName = "Publisher Account Exists";
            TestDescription = "Verify that the Asset Store publisher account exists.";
            TestCategory = TestCategory.ContentScan;
            SupportedValidations = new[] { ValidationType.AssetStore, ValidationType.AssetStorePublishAction };
            DependsOn = new[] { typeof(IsUserLoggedInPublishValidation) };
            m_AssetStorePublishOperationValidationUtility = new AssetStorePublishOperationValidationUtility();
        }

        protected override void Run()
        {
            TestState = TestState.Succeeded;

            var hasUserPublisherId = m_AssetStorePublishOperationValidationUtility.IsUserAPublisher();

            if (hasUserPublisherId.HasValue && hasUserPublisherId.Value) return;

            if (hasUserPublisherId.HasValue)
                AddError(k_UserHasNotPublisherAccount);
            else
                AddError(k_NetworkError);
        }

    }
}

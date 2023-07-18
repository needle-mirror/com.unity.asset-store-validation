using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;
namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class IsUserLoginPublishValidation : BaseValidation
    {
        IAssetStorePublishOperationValidation m_AssetStorePublishOperationValidationUtility;
        internal static readonly string k_DocsFilePath = "user_login_publish_validation";

        internal static readonly string k_UserIsNotLogin =
            $"User is not logged in. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "user-is-not-logged-in")}";

        internal IAssetStorePublishOperationValidation AssetStorePublishOperationValidationUtility { get => m_AssetStorePublishOperationValidationUtility; set => m_AssetStorePublishOperationValidationUtility = value; }

        public IsUserLoginPublishValidation()
        {
            TestName = "User logged in";
            TestDescription = "Verify that the user is logged in.";
            TestCategory = TestCategory.ContentScan;
            SupportedValidations = new[] { ValidationType.AssetStore, ValidationType.AssetStorePublishAction };
            m_AssetStorePublishOperationValidationUtility = new AssetStorePublishOperationValidationUtility();
        }

        protected override void Run()
        {
            TestState = TestState.Succeeded;

            if (m_AssetStorePublishOperationValidationUtility.IsUserLogin()) return;

            AddError(k_UserIsNotLogin);
        }
    }
}
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;
namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class IsUserLoggedInPublishValidation : BaseValidation
    {
        IAssetStorePublishOperationValidation m_AssetStorePublishOperationValidationUtility;
        internal static readonly string k_DocsFilePath = "user_login_publish_validation";

        internal static readonly string k_UserIsNotLoggedIn =
            $"User is not logged in. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "user-is-not-logged-in")}";

        internal IAssetStorePublishOperationValidation AssetStorePublishOperationValidationUtility { get => m_AssetStorePublishOperationValidationUtility; set => m_AssetStorePublishOperationValidationUtility = value; }

        public IsUserLoggedInPublishValidation()
        {
            TestName = "User logged in";
            TestDescription = "Verify that the user is logged in.";
            TestCategory = TestCategory.ContentScan;
            SupportedValidations = new[] { ValidationType.AssetStore, ValidationType.AssetStorePublishAction, ValidationType.InternalTesting };
            m_AssetStorePublishOperationValidationUtility = new AssetStorePublishOperationValidationUtility();
        }

        protected override void Run()
        {
            TestState = TestState.Succeeded;

            if (m_AssetStorePublishOperationValidationUtility.IsUserLoggedIn()) return;

            AddError(k_UserIsNotLoggedIn);
        }
    }
}
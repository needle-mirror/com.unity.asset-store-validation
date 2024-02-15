using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class DescriptionValidation : BaseValidation
    {
        internal const int k_MaxDescriptionLength = 200;
        
        internal static readonly string k_DocsFilePath = "description_validation.html";
        internal static readonly string k_DescriptionTooLongErrorMessage = $"The description of this package exceeds the limit of {k_MaxDescriptionLength} characters. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "description_too_long")}";
        
        public DescriptionValidation()
        {
            TestName = "Manifest: Description Field";
            TestDescription = "Validates that the package description field is valid.";
            TestCategory = TestCategory.DataValidation;
            SupportedValidations = new [] { ValidationType.AssetStore };
        }
        
        protected override void Run()
        {
            // Start by declaring victory
            TestState = TestState.Succeeded;
            Validate(Context.ProjectPackageInfo);
        }

        void Validate(ManifestData manifestData)
        {
            if (manifestData.description.Length > k_MaxDescriptionLength)
                AddError(k_DescriptionTooLongErrorMessage);
        }
    }
}

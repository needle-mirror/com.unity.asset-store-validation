using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class ManifestNameFieldValidation : BaseValidation
    {
        Regex m_Regex = new Regex("^[a-z0-9]([a-z0-9-_]|.[a-z0-9]){1,213}$");
        internal static readonly string k_DocsFilePath = "manifest_name_field_validation.html";
        internal const int k_MaxManifestNameLength = 214;
        internal static readonly string[] k_InvalidEnds = new string[]
        {
            ".plugin",
            ".bundle",
            ".framework"
        };

        internal static string NameHasUpperCaseLetterErrorMessage(string manifestName) => $"Unity does not allow for uppercase letters in the package name {manifestName}. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-name-contains-uppercase-letters")}";
        internal static string NameExceedMaxLengthErrorMessage(string manifestName) => $"Unity package name cannot exceed {k_MaxManifestNameLength} characters {manifestName}. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-name-is-too-long")}";
        internal static string NameHasInvalidEndErrorMessage(string manifestName) => $"Unity package name cannot end with extensions \".plugin\", \".bundle\", or \".framework\". The current package name is:  {manifestName}. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-name-ends-in-forbidden-extension")}";
        internal static string NameStartWithComDotUnityErrorMessage(string manifestName) => $"Unity package name cannot begin with \"com.unity.\" if it is being published to the asset store. Current package name: {manifestName}. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "asset-store-context-contains-unity-identifier")}";
        internal static string NameHasNoDotSeparatorErrorMessage(string manifestName) => $"Package name {manifestName} must contain at least one dot separator \".\" to be considered valid. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-name-must-contain-dot-separator")}";
        internal static string NameDoesNotMatchRegex(string manifestName) => $"Package name {manifestName} is invalid. Please ensure that the package does not contain special characters, then run the validation again. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-name-does-not-match-regex")}";

        public ManifestNameFieldValidation()
        {
            TestName = "Manifest: Name Field";
            TestDescription = "Validates that the package manifest name field is valid.";
            TestCategory = TestCategory.DataValidation;
            SupportedValidations = new[] { ValidationType.AssetStore, ValidationType.InternalTesting };
        }

        protected override void Run()
        {
            // Start by declaring victory
            TestState = TestState.Succeeded;
            CheckNameField(Context.ProjectPackageInfo.name);
        }

        void CheckNameField(string manifestName)
        {
            // Unity package names should not contain any uppercase letters
            if (manifestName.Any(char.IsUpper))
                AddError(NameHasUpperCaseLetterErrorMessage(manifestName));

            // Unity package name cannot exceed k_MaxManifestNameLength characters
            if (manifestName.Length > k_MaxManifestNameLength)
                AddError(NameExceedMaxLengthErrorMessage(manifestName));

            // Unity will try to do things to packages ending in these extensions that may be undesired
            if (k_InvalidEnds.Any(s => manifestName.EndsWith(s)))
                AddError(NameHasInvalidEndErrorMessage(manifestName));

            if (manifestName.StartsWith("com.unity."))
                AddError(NameStartWithComDotUnityErrorMessage(manifestName));

            if (manifestName.Trim('.').Split('.').Length < 2)
                AddError(NameHasNoDotSeparatorErrorMessage(manifestName));

            if (!m_Regex.IsMatch(manifestName))
                AddError(NameDoesNotMatchRegex(manifestName));
        }
    }
}

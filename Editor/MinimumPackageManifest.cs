using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class MinimumPackageManifest : BaseValidation
    {
        internal static readonly string k_DocsFilePath = "minimum_package_manifest_validation.html";

        public MinimumPackageManifest()
        {
            TestName = "Minimum Package Manifest";
            TestDescription = "A package manifest must contain at least the following fields: name, version.";
            TestCategory = TestCategory.DataValidation;
            SupportedValidations = new[] {ValidationType.Structure, ValidationType.AssetStore};
        }

        protected override void Run()
        {
            // Start by declaring victory
            TestState = TestState.Succeeded;
            ValidateManifestData(Context.ProjectPackageInfo);
        }

        void ValidateManifestData(ManifestData manifestData)
        {
            if (string.IsNullOrEmpty(manifestData.name))
            {
                if (string.IsNullOrEmpty(manifestData.version))
                {
                    AddError($"Package manifest is required to have both name and version fields. Open the package.json file and add \"name\" and \"version\" keys to the manifest. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "missing-name-and-version-fields")}");
                    return;
                }
                AddError($"Package manifest is required to have a name field. Open the package.json file and add \"name\" key to the manifest. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "missing-name-field")}");
                return;
            }
            if (string.IsNullOrEmpty(manifestData.version))
            {
                AddError($"Package manifest is required to have a version field. Open the package.json file and add \"version\" key to the manifest. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "missing-version-field")}");
            }
        }
    }
}

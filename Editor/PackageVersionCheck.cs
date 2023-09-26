using UnityEditor.PackageManager.AssetStoreValidation.Semver;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class PackageVersionCheck : BaseValidation
    {

        internal static readonly string k_DocsFilePath = "package_version_check.html";

        public PackageVersionCheck()
        {
            TestName = "Package Version";
            TestDescription = "A package version must be a valid Semver string.";
            TestCategory = TestCategory.DataValidation;
            SupportedValidations = new[] {ValidationType.Structure, ValidationType.AssetStore, ValidationType.InternalTesting};

        }

        protected override void Run()
        {
            // Start by declaring victory
            TestState = TestState.Succeeded;

            var manifestData = Context.ProjectPackageInfo;

            SemVersion version;
            // Check package version, make sure it's a valid SemVer string.
            if (!SemVersion.TryParse(manifestData.version, out version, true))
            {
                AddError($"In {manifestData.path}, \"version\" needs to be a valid \"Semver\". {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-version-check")}");
            }
        }
    }
}

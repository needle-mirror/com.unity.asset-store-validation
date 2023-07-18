using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class UniquePackageIDValidation : BaseValidation
    {
        internal static readonly string k_DocsFilePath = "unique_package_id_validation.html";
        internal IUpmRegistryHelper m_UpmRegistryHelper;

        public UniquePackageIDValidation()
        {
            TestName = "Unique Package ID";
            TestDescription = "A package id (name@version) must not already exist on the Asset Store Registry when trying to publish a new version.";
            TestCategory = TestCategory.DataValidation;
            SupportedValidations = new[] {ValidationType.AssetStore, ValidationType.AssetStorePublishAction};
            m_UpmRegistryHelper = new UpmRegistryHelper();
        }

        protected override void Run()
        {
            // Start by declaring victory
            TestState = TestState.Succeeded;

            var manifestData = Context.ProjectPackageInfo;
            if (string.IsNullOrWhiteSpace(manifestData.name) || string.IsNullOrWhiteSpace(manifestData.version))
            {
                AddError($"Package manifest name and version must not be null or whitespace. Ensure that the package.json file contains both a name and version, then rerun the validation. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-name-and-version-must-be-provided")}");
                return;
            }
            
            ValidateAssetStoreUniquePackageID(manifestData.name, manifestData.version);
        }

        void ValidateAssetStoreUniquePackageID(string packageName, string packageVersion)
        {
            var packageMetadataJsonFromAssetStoreRegistry = m_UpmRegistryHelper.RetrievePackageMetaDataFromAssetStoreRegistry(packageName, AddWarning, AddError);

            // If there is no data on the registry then the package does not exist yet and is unique.
            if (string.IsNullOrWhiteSpace(packageMetadataJsonFromAssetStoreRegistry)) return;

            var packageVersionsOnRegistry = ParseVersionsFromPackageMetaDataString(packageMetadataJsonFromAssetStoreRegistry);

            if (packageVersionsOnRegistry.Contains(packageVersion))
            {
                AddError($"A package already exists with name: {packageName} and version: {packageVersion}. Update the version number to one that does not exist on the registry, then rerun the validation. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "package-id-already-exists")}");
            }
        }

        List<string> ParseVersionsFromPackageMetaDataString(string json)
        {
            var versions = new List<string>();
            try
            {
                var packageParsedMetaData = JObject.Parse(json);
                var packageVersionsAsJson = (packageParsedMetaData["versions"] ?? "").Value<JObject>();
                versions = packageVersionsAsJson.Properties().Select(p => p.Name).ToList();
            }
            catch (JsonException e)
            {
                AddError($"An error occured while trying to parse the contents of the package on the Asset Store Registry: {e.Message}");
            }
            return versions;
        }
    }
}

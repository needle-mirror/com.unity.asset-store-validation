using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite.ValidationTests;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class AssetStorePublisherValidation : BaseValidation
    {
        static readonly string k_DocsFilePath = "asset_store_publisher_validation.html";
        internal static readonly string k_NoPublisherAccount = $"There is no publisher linked to this account. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "there_is_no_publisher_linked_to_this_account")}";
        internal static readonly string k_PackageBelongsToAnotherPublisher =
            $"This package name belongs to another publisher. {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "this_package_name_belongs_to_another_publisher")}";
        internal static readonly string k_ErrorParsingContent =
            "An error occured while trying to parse the contents of the package on the Asset Store Registry: {0}. Please try again later. If the issue persist please contact support.";
        internal IUpmRegistryHelper m_UpmRegistryHelper;

        public AssetStorePublisherValidation()
        {
            TestName = "Asset Store Publisher";
            TestDescription = "Validates that the package belongs to the publisher.";
            TestCategory = TestCategory.DataValidation;
            SupportedValidations = new ValidationType[] {ValidationType.AssetStore};
            m_UpmRegistryHelper = new UpmRegistryHelper(UnityWebRequestHandler.GetInstance());
        }

        protected override void Run()
        {
            // Start by declaring victory
            TestState = TestState.Succeeded;

            var manifestData = Context.ProjectPackageInfo;

            // when no metadata is return then the package doesn't exist in the registry
            if (string.IsNullOrWhiteSpace(m_UpmRegistryHelper.RetrievePackageMetaDataFromAssetStoreRegistry(manifestData.name, AddWarning, AddError)))
            {
                return;
            }

            var publisherMetadata = m_UpmRegistryHelper.GetPublisherMetadata(AddWarning, AddError);

            if (string.IsNullOrEmpty(publisherMetadata))
            {
                AddError(k_NoPublisherAccount);
                return;
            }

            ValidatePackageBelongsToPublisher(publisherMetadata, manifestData.name);
        }

        void ValidatePackageBelongsToPublisher(string json, string packageName)
        {
            try
            {
                var packageParsedMetaData = JObject.Parse(json);
                if (!packageParsedMetaData.TryGetValue("packages", out var packageVersionsAsJson) || !packageVersionsAsJson.HasValues || !packageVersionsAsJson.Any())
                {
                    AddError(k_PackageBelongsToAnotherPublisher);
                    return;
                }

                var packages = packageVersionsAsJson.Value<JObject>().Properties().Values();

                foreach (var package in packages)
                {
                    var publisherPackageName = package["package_name"];
                    if (publisherPackageName != null && string.Equals(publisherPackageName.ToString(), packageName,
                            StringComparison.InvariantCulture))
                    {
                        return;
                    }
                }

                AddError(k_PackageBelongsToAnotherPublisher);
            }
            catch (JsonException e)
            {
                AddError(string.Format(k_ErrorParsingContent, e.Message));
            }
        }
    }
}
using System;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class UpmRegistryHelper : IUpmRegistryHelper
    {
        const string k_LinkToAssetStoreRegistry = "https://packages.unity.com";
        const string k_LinkToStagingAssetStoreRegistry = "https://packages-v2-staging.unity.com/-/api";
        readonly string m_CurrentLinkAssetStoreRegistry;

        public UpmRegistryHelper()
        {
            m_CurrentLinkAssetStoreRegistry = UnityConnectUtilities.IsStagingEnvironment()
                ? k_LinkToStagingAssetStoreRegistry
                : k_LinkToAssetStoreRegistry;
        }

        /// <summary>
        /// Acquire the publisher metadata information from the Asset Store
        /// </summary>
        /// <param name="warning"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public string GetPublisherMetadata(Action<string> warning, Action<string> error)
        {
            return ValidationHttpClient.OnGetWithAuthToken($"{m_CurrentLinkAssetStoreRegistry}/metadata",
                CloudProjectSettings.accessToken, warning, error);
        }

        /// <summary>
        /// Retrieve the package metadata from the Asset Store
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="warning"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public string RetrievePackageMetaDataFromAssetStoreRegistry(string packageName, Action<string> warning,
            Action<string> error)
        {
            return ValidationHttpClient.OnGet($"{m_CurrentLinkAssetStoreRegistry}/{packageName}", warning, error);
        }
    }
}
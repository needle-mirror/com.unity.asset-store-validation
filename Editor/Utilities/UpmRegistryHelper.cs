using System;
using System.Net;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    class UpmRegistryHelper : IUpmRegistryHelper
    {
        const string k_LinkToAssetStoreRegistry = "https://packages.unity.com";
        const string k_LinkToStagingAssetStoreRegistry = "https://packages-v2-staging.unity.com/-/api";
        readonly string m_CurrentLinkAssetStoreRegistry;

        private IUnityWebRequestHandler m_UnityWebRequestHandler;

        public UpmRegistryHelper(IUnityWebRequestHandler unityWebRequestHandler)
        {
            m_CurrentLinkAssetStoreRegistry = UnityConnectUtilities.IsStagingEnvironment()
                ? k_LinkToStagingAssetStoreRegistry
                : k_LinkToAssetStoreRegistry;
            
            m_UnityWebRequestHandler = unityWebRequestHandler;
        }

        /// <summary>
        /// Acquire the publisher metadata information from the Asset Store
        /// </summary>
        /// <param name="warning"> Action in case of warning </param>
        /// <param name="error"> Action in case of error </param>
        /// <returns> Publisher metadata raw string. If string is empty or null, then there is no publisher account </returns>
        public string GetPublisherMetadata(Action<string> warning, Action<string> error)
        {
            var request = m_UnityWebRequestHandler.SendGetRequestWithAuth($"{m_CurrentLinkAssetStoreRegistry}/metadata", CloudProjectSettings.accessToken);
            return HandleAssetStoreHttpResponse(request, warning, error);
        }

        /// <summary>
        /// Retrieve the package metadata from the Asset Store
        /// </summary>
        /// <param name="packageName"> String for the package name </param>
        /// <param name="warning"> Action in case of warning </param>
        /// <param name="error"> Action in case of error </param>
        /// <returns> Package metadata raw string. If string is empty or null, then package does not exist </returns>
        public string RetrievePackageMetaDataFromAssetStoreRegistry(string packageName, Action<string> warning, Action<string> error)
        {
            var request = m_UnityWebRequestHandler.SendGetRequest($"{m_CurrentLinkAssetStoreRegistry}/{packageName}");
            return HandleAssetStoreHttpResponse(request, warning, error);
        }

        /// <summary>
        /// Used to handle Http response of the Asset Store
        /// </summary>
        /// <param name="request"> Unity Web Request </param>
        /// <param name="warning"> Action in case of warning </param>
        /// <param name="error"> Action in case of error </param>
        /// <returns> Http request data or empty string if request is null, or an error occured </returns>
        private string HandleAssetStoreHttpResponse(IUnityWebRequestWrapper request, Action<string> warning, Action<string> error)
        {
            if (request == null) return string.Empty;

            if (request.responseCode == (long) HttpStatusCode.OK)
                return request.downloadHandler.text;

            AssetStoreResponses.HandleHttpStatusCode((HttpStatusCode)request.responseCode, warning, error);
            return string.Empty;
        }
    }
}

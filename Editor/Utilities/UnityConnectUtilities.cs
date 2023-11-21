using Newtonsoft.Json.Linq;
using System;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    internal static class UnityConnectUtilities
    {
        internal static string s_CloudEnvironment;

        internal const string k_ProductionEnvironment = "production";
        internal const string k_StagingEnvironment = "staging";
        internal const string k_CloudEnvironmentArg = "-cloudEnvironment";

        internal const string k_AssetStorePublisherID = "https://packages-v2.unity.com/-/api/metadata";
        internal const string k_AssetStoreCheckTerms = "https://packages-v2.unity.com/-/api/terms/check";

        private static IUnityWebRequestHandler m_UnityWebRequestHandler => UnityWebRequestHandler.GetInstance();

        static UnityConnectUtilities()
        {
            SetEnvironment(Environment.GetCommandLineArgs());
        }

        internal static void SetEnvironment(string[] args)
        {
            var envPos = Array.IndexOf(args, k_CloudEnvironmentArg);
            try
            {
                s_CloudEnvironment = envPos == -1 ? k_ProductionEnvironment : args[envPos + 1];
            }
            catch (Exception)
            {
                // We want to make sure our cloud environment has a default value regardless of what happens.
                s_CloudEnvironment = k_ProductionEnvironment;
            }
        }

        /// <summary>
        /// Return true if the environment the editor is running on is Staging
        /// </summary>
        /// <returns></returns>
        public static bool IsStagingEnvironment()
        {
            return string.Equals(s_CloudEnvironment, k_StagingEnvironment,
                StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Get all the current packages in the project
        /// </summary>
        /// <returns></returns>
        public static PackageInfo[] GetAllRegisteredPackages()
        {
            return PackageInfo.GetAllRegisteredPackages();
        }

        /// <summary>
        /// This method return true if the user is logged in.
        /// </summary>
        /// <returns>Return true if the user is login, otherwise false</returns>
        public static bool IsUserLoggedIn()
        {
            return !string.IsNullOrWhiteSpace(CloudProjectSettings.userId);
        }

        /// <summary>
        /// This method return true if the user has an associated PublisherId on Asset Store
        /// Publisher Portal.
        /// </summary>
        /// <returns>Return true if the user has an associated PublisherId on Asset Store Publisher Portal, otherwise false</returns>
        public static bool? HasUserPublisherId
        {
            get
            {
                var metadataResponseMessage = m_UnityWebRequestHandler.SendGetRequestWithAuth(k_AssetStorePublisherID, getAccessToken);
                return IsPublisherIdInMetadata(metadataResponseMessage?.downloadHandler?.text);
            }
        }

        /// <summary>
        /// This method return true if the user already accepted the Terms of Asset Store Publisher Portal.
        /// </summary>
        /// <returns>Return true if the user already accepted the Terms of Asset Store Publisher Portal, otherwise false</returns>
        public static bool? isAssetStoreTermsAccepted
        {
            get
            {
                var checkTermsResponse = m_UnityWebRequestHandler.SendGetRequestWithAuth(k_AssetStoreCheckTerms, getAccessToken);
                return checkTermsResponse?.downloadHandler?.text.Contains(":true");
            }
        }

        internal static string getAccessToken => CloudProjectSettings.accessToken;

        private static bool? IsPublisherIdInMetadata(string metadataResponseMessage)
        {
            try
            {
                var stringMetadataResponse = JObject.Parse(metadataResponseMessage);
                var publisher = (JObject)stringMetadataResponse["publisher"];
                var publisherId = publisher["id"].ToString();
                return !string.IsNullOrWhiteSpace(publisherId);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

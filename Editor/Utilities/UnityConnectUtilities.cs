using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using UnityEditor.Connect;
using UnityEngine;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    static class UnityConnectUtilities
    {
        internal static string s_CloudEnvironment;

        internal const string k_ProductionEnvironment = "production";
        internal const string k_StagingEnvironment = "staging";
        internal const string k_CloudEnvironmentArg = "-cloudEnvironment";

        internal const string k_AssetStorePublisherID = "https://packages-v2.unity.com/-/api/metadata";
        internal const string k_AssetStoreCheckTerms = "https://packages-v2.unity.com/-/api/terms/check";

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
        /// This method return true if the user is login.
        /// </summary>
        /// <returns>Return true if the user is login, otherwise false</returns>
        public static bool IsUserLogin()
        {
            return (!string.IsNullOrWhiteSpace(Environment.UserName));
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
                var metadataResponseMessage = ValidationHttpClient.Get(k_AssetStorePublisherID, getAccessToken);
                return IsPublisherIdInMetadata(metadataResponseMessage);
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
                var checkTermsResponse = ValidationHttpClient.Get(k_AssetStoreCheckTerms, getAccessToken);

                if (checkTermsResponse.StatusCode != System.Net.HttpStatusCode.OK) return null;

                return checkTermsResponse.Content.ReadAsStringAsync().Result.Contains(":true");
            }
        }

        internal static string getAccessToken => CloudProjectSettings.accessToken;

        private static bool? IsPublisherIdInMetadata(HttpResponseMessage metadataResponseMessage)
        {
            string stringMetadata = metadataResponseMessage.Content.ReadAsStringAsync().Result;
            try
            {
                JObject stringMetadaResponse = JObject.Parse(stringMetadata);
                JObject publisher = (JObject)stringMetadaResponse["publisher"];
                string publisherId = publisher["id"].ToString();
                return !string.IsNullOrWhiteSpace(publisherId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static HttpClientHandler GetHttpClient()
        {
            return new HttpClientHandler();
        }
    }
}
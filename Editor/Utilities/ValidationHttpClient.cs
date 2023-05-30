using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    static class ValidationHttpClient
    {
        static readonly HttpClient k_HttpClient = new HttpClient();

        static string OnRequest(HttpRequestMessage message, Action<string> warning, Action<string> error)
        {
            using var responseFromAssetStoreRegistry = k_HttpClient.SendAsync(message).Result;
            
            if (responseFromAssetStoreRegistry.StatusCode != HttpStatusCode.OK)
            {
                AssetStoreResponses.HandleHttpStatusCode(responseFromAssetStoreRegistry.StatusCode, warning, error);
                return string.Empty;
            }

            using var content = responseFromAssetStoreRegistry.Content;
            return content.ReadAsStringAsync().Result;
        }

        /// <summary>
        /// Execute a Get request to the specified URi
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="warning"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static string OnGet(string requestUri, Action<string> warning, Action<string> error)
        {
            var httpRequestMessage = new HttpRequestMessage(
                HttpMethod.Get,
                requestUri);

            return OnRequest(httpRequestMessage, warning, error);
        }

        /// <summary>
        /// Execute a Get request to the specified Uri after attaching the provided auth token to the request headers
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="authToken"></param>
        /// <param name="warning"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static string OnGetWithAuthToken(string requestUri, string authToken, Action<string> warning,
            Action<string> error)
        {
            var httpRequestMessage = new HttpRequestMessage(
                HttpMethod.Get,
                requestUri);
            
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            return OnRequest(httpRequestMessage, warning, error);
        }
    }
}
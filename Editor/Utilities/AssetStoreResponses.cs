using System;
using System.Collections.Generic;
using System.Net;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    static class AssetStoreResponses
    {
        static string k_LinkToHttpStatusCodeDocs = "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/";
        internal static readonly string k_DocsFilePath = "http_helper_documentation.html";

        static Dictionary<HttpStatusCode, string> m_HttpResponseMessages = new Dictionary<HttpStatusCode, string>()
        {
            { HttpStatusCode.BadRequest, $"The Asset Store registry returned with code: {(int)HttpStatusCode.BadRequest} ({HttpStatusCode.BadRequest}). This error code indicates that the request could not be understood by the server."},
            { HttpStatusCode.Unauthorized, $"The Asset Store registry returned with code: {(int)HttpStatusCode.Unauthorized} ({HttpStatusCode.Unauthorized}). This error code indicates that the requested resource requires authentication."},
            { HttpStatusCode.Forbidden, $"The Asset Store registry returned with code: {(int)HttpStatusCode.Forbidden} ({HttpStatusCode.Forbidden}). This error code indicates that the server refuses to fulfill the request."},
            { HttpStatusCode.RequestTimeout, $"The Asset Store registry returned with code: {(int)HttpStatusCode.RequestTimeout} ({HttpStatusCode.RequestTimeout}). This error code indicates that the client did not send a request within the time the server was expecting the request."},
            { HttpStatusCode.Gone, $"The Asset Store registry returned with code: {(int)HttpStatusCode.Gone} ({HttpStatusCode.Gone}). This error code indicates that the requested package information is no longer available."},
            { HttpStatusCode.InternalServerError, $"The Asset Store registry returned with code: {(int)HttpStatusCode.InternalServerError} ({HttpStatusCode.InternalServerError}). This error code indicates that a generic error has occurred on the server."},
            { HttpStatusCode.BadGateway, $"The Asset Store registry returned with code: {(int)HttpStatusCode.BadGateway} ({HttpStatusCode.BadGateway}). This error code indicates that an intermediate proxy server received a bad response from another proxy or the origin server. Some possible causes are that the server might be down, there might be a connectivity issue or there’s simply too much traffic."},
            { HttpStatusCode.ServiceUnavailable, $"The Asset Store registry returned with code: {(int)HttpStatusCode.ServiceUnavailable} ({HttpStatusCode.ServiceUnavailable}). This error code indicates that the server is temporarily unavailable, usually due to high load or maintenance."},
            { HttpStatusCode.GatewayTimeout, $"The Asset Store registry returned with code: {(int)HttpStatusCode.GatewayTimeout} ({HttpStatusCode.GatewayTimeout}). This error code indicates that an intermediate proxy server timed out while waiting for a response from another proxy or the origin server. One solution may be to ensure you have a strong connection, then run the validation again."},
        };
        
        internal static string GetMessageFromStatusCode(HttpStatusCode httpStatusCode, bool linkToMozillaDocs = false)
        {
            var message = m_HttpResponseMessages[httpStatusCode];
            return linkToMozillaDocs ? $"{message} Read more about this error and potential solutions at {k_LinkToHttpStatusCodeDocs + (int) httpStatusCode}" : message;
        }
        
        internal static string UnexpectedResponseMessage(HttpStatusCode httpStatusCode)
        {
            return $"The Asset Store registry returned with code: {(int) httpStatusCode} ({httpStatusCode}). While this response is unexpected, you can read more about this error and potential solutions at {k_LinkToHttpStatusCodeDocs + (int) httpStatusCode}";
        }
        
        /// <summary>
        /// Call the given action with an user-friendly messaged based from a HttpStatusCode
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="warning"></param>
        /// <param name="error"></param>
        public static void HandleHttpStatusCode(HttpStatusCode statusCode, Action<string> warning, Action<string> error)
        {
            // Not found indicates that the package name@version doesn't exist on the registry and it is a new package.
            if (statusCode == HttpStatusCode.NotFound) return;

            if (!m_HttpResponseMessages.ContainsKey(statusCode))
            {
                warning(UnexpectedResponseMessage(statusCode));
                return;
            }

            if (statusCode == HttpStatusCode.BadRequest)
            {
                error(
                    $"{GetMessageFromStatusCode(statusCode)} {ErrorDocumentation.GetLinkMessage(k_DocsFilePath, "bad-request")}");
            }
            else
            {
                warning(GetMessageFromStatusCode(statusCode, linkToMozillaDocs: true));
            }
        }
    }
}
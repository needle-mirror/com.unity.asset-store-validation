using UnityEngine.Networking;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    // We want to use UnityWebRequests instead of HttpClient to ensure that any request made by Unity is going through a pipe that is aware of proxies and that makes sure to pass the credentials when needed.
    // This process only works 100% of the time with UnityWebRequests
    interface IUnityWebRequestHandler
    {
        public IUnityWebRequestWrapper SendWebRequest(string url, DownloadHandler downloadHandler = null, bool useDefaultDownloadHandler = true);
        public IUnityWebRequestWrapper SendGetRequest(string url, DownloadHandler downloadHandler = null, bool useDefaultDownloadHandler = true);
        public IUnityWebRequestWrapper SendGetRequestWithAuth(string url, string authToken, DownloadHandler downloadHandler = null, bool useDefaultDownloadHandler = true);
        public UrlStatus IsUrlReachable(string url);
    }
    
    internal class UnityWebRequestHandler : IUnityWebRequestHandler
    {
        private static IUnityWebRequestHandler instance;
        public static IUnityWebRequestHandler GetInstance()
        {
            return instance ??= new UnityWebRequestHandler();
        }

        public virtual IUnityWebRequestWrapper SendWebRequest(string url,  DownloadHandler downloadHandler = null, bool useDefaultDownloadHandler = true)
        {
            var unityWebRequest = CreateRequest(false, url, downloadHandler, useDefaultDownloadHandler);
            unityWebRequest.SendWebRequest();
            return unityWebRequest;
        }

        public IUnityWebRequestWrapper SendGetRequest(string url, DownloadHandler downloadHandler = null, bool useDefaultDownloadHandler = true)
        {
            var unityWebRequest = CreateRequest(true, url, downloadHandler, useDefaultDownloadHandler);
            unityWebRequest.SendWebRequest();
            return unityWebRequest;
        }

        public IUnityWebRequestWrapper SendGetRequestWithAuth(string url, string authToken, DownloadHandler downloadHandler = null, bool useDefaultDownloadHandler = true)
        {
            var unityWebRequest = CreateRequest(true, url, downloadHandler, useDefaultDownloadHandler);
            unityWebRequest.SetRequestHeader("Authorization", "Bearer " + authToken);
            unityWebRequest.SendWebRequest();
            return unityWebRequest;
        }

        public UrlStatus IsUrlReachable(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return UrlStatus.None;

            var request = SendWebRequest(url);
            return request?.result == UnityWebRequest.Result.Success ? UrlStatus.Ok : UrlStatus.Unreachable;
        }

        internal virtual IUnityWebRequestWrapper CreateRequest(bool isGetRequest, string url, DownloadHandler downloadHandler = null, bool useDefaultDownloadHandler = true)
        {
            return new UnityWebRequestWrapper(isGetRequest, url, downloadHandler, useDefaultDownloadHandler);
        }
    }
}

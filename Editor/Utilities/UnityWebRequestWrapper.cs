using UnityEngine.Networking;

namespace UnityEditor.PackageManager.AssetStoreValidation
{
    internal interface IUnityWebRequestWrapper
    {
        public DownloadHandler downloadHandler { get; }
        public UnityWebRequest.Result result { get; }
        public long responseCode { get; }

        public void SendWebRequest();
        public void SetRequestHeader(string name, string value);
    }
    
    internal class UnityWebRequestWrapper : IUnityWebRequestWrapper
    {
        private const int k_StandardTimeout = 60;
        private UnityWebRequest m_UnityWebRequest;
        
        public UnityWebRequestWrapper(bool isGetRequest, string uri, DownloadHandler downloadHandler = null, bool useDefaultDownloadHandler = true)
        {
            m_UnityWebRequest = isGetRequest ? UnityWebRequest.Get(uri) : new UnityWebRequest(uri);
            m_UnityWebRequest.timeout = k_StandardTimeout;
            if (!useDefaultDownloadHandler)
                m_UnityWebRequest.downloadHandler = downloadHandler;
        }
        
        public DownloadHandler downloadHandler => m_UnityWebRequest.downloadHandler;
        public UnityWebRequest.Result result => m_UnityWebRequest.result;
        public long responseCode => m_UnityWebRequest.responseCode;
        
        public void SendWebRequest()
        {
            var operation = m_UnityWebRequest.SendWebRequest();
            while(!operation.isDone) { }
        }

        public void SetRequestHeader(string name, string value)
        {
            m_UnityWebRequest.SetRequestHeader(name, value);
        }
    }
}
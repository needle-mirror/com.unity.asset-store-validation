namespace UnityEditor.PackageManager.AssetStoreValidation
{
    public class OfflineHttpUtils: IReachable
    {
        public bool IsURLReachable(string url, int timeoutSeconds = HttpUtils.k_TimeoutSeconds)
        {
            return false;
        }
    }
}
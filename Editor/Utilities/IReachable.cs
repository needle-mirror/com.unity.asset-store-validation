interface IReachable
{
    bool IsURLReachable(string url, int timeoutSeconds = HttpUtils.k_TimeoutSeconds);
}

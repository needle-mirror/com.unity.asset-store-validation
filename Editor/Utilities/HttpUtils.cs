using System;
using System.Net.Http;

class HttpUtils: IReachable
{
    internal const int k_TimeoutSeconds = 50; // Default timeout

    /// <summary>
    /// Makes a request to the given URL, returning true if the status code of the request is HttpStatusCode.OK
    /// </summary>
    /// <param name="url"></param>
    /// <param name="timeoutSeconds">Seconds to use as timeout setting for the HttpClient instance</param>
    /// <returns></returns>
    public bool IsURLReachable(string url, int timeoutSeconds = k_TimeoutSeconds)
    {
        using var httpclient = new HttpClient() { Timeout = TimeSpan.FromSeconds(timeoutSeconds) };
        
        try
        {
            var response = httpclient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url)).Result;
            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
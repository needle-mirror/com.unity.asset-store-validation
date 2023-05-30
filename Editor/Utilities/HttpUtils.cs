using System;
using System.Net.Http;

class HttpUtils: IReachable
{
    /// <summary>
    /// Makes a request to the given URL, returning true if the status code of the request is HttpStatusCode.OK
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public bool IsURLReachable(string url)
    {
        using (var httpclient = new HttpClient())
        {
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
}
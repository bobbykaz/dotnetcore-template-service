using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BK.DotNet.Api.Client
{
    public class HttpLog
    {
        public HttpLog(int statusCode, List<string> headers)
        {
            Headers = headers;
            StatusCode = statusCode;
        }

        public string? Method { get; set; }
        public string? Route { get; set; }
        public string? RequestBody { get; set; }

        public int StatusCode { get; set; }
        public List<string> Headers { get; set; }
        public string? ResponseBody { get; set; }

        public static async Task<HttpLog> FromHttpResponse(HttpResponseMessage response)
        {
            var result = new HttpLog((int)response.StatusCode, new List<string>());

            result.Method = response.RequestMessage?.Method.Method;
            result.Route = response.RequestMessage?.RequestUri?.ToString();
            
            if(!response.IsSuccessStatusCode && response.RequestMessage?.Content != null)
                result.RequestBody = await response.RequestMessage.Content.ReadAsStringAsync();
            
            if(!response.IsSuccessStatusCode && response.Content != null )
                result.ResponseBody = await response.Content.ReadAsStringAsync();
            
            return result;
        }
    }
}

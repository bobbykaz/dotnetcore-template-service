using BK.DotNet.Api.Model;
using Serilog;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BK.DotNet.Api.Client
{
    public class Client
    {
        protected HttpClient _Client { get; set; }
        protected ILogger _Log { get; set; }
        protected UserLog _UserLog { get; set; }

        private JsonSerializerOptions _SerializationOptions =  new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

    public Client(string baseUrl, UserLog user, ILogger log)
        {
            _Log = log;
            _UserLog = user;

            _Client = new HttpClient() { BaseAddress = new Uri(baseUrl), Timeout = TimeSpan.FromSeconds(30) };
            _Client.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        #region Auth
        public void SetAuthHeader(string accessToken)
        {
            _Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);
        }

        public async Task<OAuthToken> RefreshToken(string refreshToken)
        {
            var refreshUrl = ""; 
            var request = new HttpRequestMessage(HttpMethod.Post, refreshUrl);
            var data = new List<KeyValuePair<string, string>>();

            data.Add(new KeyValuePair<string, string>("grant_type", "refresh_token"));
            data.Add(new KeyValuePair<string, string>("refresh_token", "some_token"));
            data.Add(new KeyValuePair<string, string>("client_id", "123456"));
            data.Add(new KeyValuePair<string, string>("client_secret", "sssshhh"));

            request.Content = new FormUrlEncodedContent(data);
            
            //var strContent = await request.Content.ReadAsStringAsync();

            var response = await _Client.SendAsync(request);

            await CheckError(response);

            var token = JsonSerializer.Deserialize<OAuthToken>(await response.Content.ReadAsStringAsync());
            if (token == null)
                throw new ApiException(response, "Failed to deserialize OAuth Token");
            return token;
        }
        #endregion

        #region Helpers
        protected async Task<TResponse> Get<TResponse>(string route)
        {
            var response = await _Client.GetAsync(route);
            return await CheckErrorAndDeserialize<TResponse>(response);
        }

        protected async Task Put<TRequest>(string route, TRequest request)
        {
            var strContent = JsonSerializer.Serialize(request);
            var content = new StringContent(strContent, System.Text.Encoding.UTF8, "application/json");
            var response = await _Client.PutAsync(route, content);
            await CheckError(response);
        }

        protected async Task<TResponse> Put<TRequest, TResponse>(string route, TRequest request)
        {
            var strContent = JsonSerializer.Serialize(request);
            var content = new StringContent(strContent, System.Text.Encoding.UTF8, "application/json");
            var response = await _Client.PutAsync(route, content);
            return await CheckErrorAndDeserialize<TResponse>(response);
        }

        protected async Task Post(string route)
        {
            var response = await _Client.PostAsync(route, null);
            await CheckError(response);
        }

        protected async Task Post<TRequest>(string route, TRequest request)
        {
            var strContent = JsonSerializer.Serialize(request);
            var content = new StringContent(strContent, System.Text.Encoding.UTF8, "application/json");
            var response = await _Client.PostAsync(route, content);
            await CheckError(response);
        }

        protected async Task<TResponse> Post<TRequest, TResponse>(string route, TRequest request)
        {
            var strContent = JsonSerializer.Serialize(request);
            var content = new StringContent(strContent, System.Text.Encoding.UTF8, "application/json");
            var response = await _Client.PostAsync(route, content);

            return await CheckErrorAndDeserialize<TResponse>(response);
        }

        public async Task Log(HttpResponseMessage resp)
        {
            var httpLog = await HttpLog.FromHttpResponse(resp);
            _Log.Information("User: {@User}; ApiCall: {@ApiCall}", _UserLog, httpLog);
        }

        public async Task CheckError(HttpResponseMessage response)
        {
            await Log(response);
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(response, "A problem occurred when communicating with the api");
            }
            response.EnsureSuccessStatusCode();
        }

        public async Task<TResponse> CheckErrorAndDeserialize<TResponse>(HttpResponseMessage response)
        {
            await CheckError(response);
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObj = JsonSerializer.Deserialize<TResponse>(responseContent, _SerializationOptions);
            if (responseObj == null)
                throw new ApiException(response, $"Failed to deserialize {nameof(TResponse)}");

            return responseObj;
        }

        public static async Task UploadFileToAWS(string url, Stream contentStream, string contentType)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"{url}");
            request.Headers.Add("x-amz-server-side-encryption", "AES256");
            request.Content = new StreamContent(contentStream)
            {
                Headers =
                        {
                            ContentLength = contentStream.Length,
                            ContentType = new MediaTypeHeaderValue(contentType)
                        }
            };
            using (var client = new HttpClient())
            {
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task<List<WeatherForecast>> GetWeatherForecastsAsync()
        {
            return await Get<List<WeatherForecast>>("api/WeatherForecast");
        }

        public static async Task<Stream> DownloadFile(string url)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                return await response.Content.ReadAsStreamAsync();
            }
        }
        #endregion


    }

    public class ApiException : Exception
    {
        public ApiException(HttpResponseMessage response, string errorMessage)
        {
            Response = response;
            ErrorMessage = errorMessage;
        }

        public HttpResponseMessage Response { get; set; }
        public string ErrorMessage { get; set; }
    }
}
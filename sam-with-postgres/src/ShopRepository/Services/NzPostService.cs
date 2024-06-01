using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ShopRepository.Services
{
    public class NZPostService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<NZPostService> _logger;
        
        public NZPostService(HttpClient httpClient, IConfiguration config, ILogger<NZPostService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
            InitializeHttpClient();
        }

        private void InitializeHttpClient()
        {
            _httpClient.BaseAddress = new Uri(_config["NZPost:ApiBaseUrl"] ?? throw new Exception("NZPost API Base URL not found in config"));
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> GetAccessTokenAsync()
        {
            try
            {
                var formData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", _config["NZPost:ClientId"]),
                    new KeyValuePair<string, string>("client_secret", _config["NZPost:ClientSecret"])
                });

                _logger.LogInformation("Sending request to get access token.");
                var response = await _httpClient.PostAsync(_config["NZPost:AccessTokenEndpoint"], formData);

                _logger.LogInformation("Received response: {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();

                var tokenJson = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Access token response: {TokenJson}", tokenJson);
                var token = JsonConvert.DeserializeObject<Dictionary<string, string>>(tokenJson);

                return token["access_token"];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAccessTokenAsync");
                throw;
            }
        }

        public async Task<string> GetApiDataAsync(string accessToken, string endpoint)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> PostDataAsync(string accessToken, string endpoint, JObject data)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var jsonContent = new StringContent(data.ToString(), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, jsonContent);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PostDataAsync with endpoint {Endpoint}", endpoint);
                throw;
            }
        }

        public async Task<string> RequestDataAsync(string accessToken, string endpoint, HttpContent content = null)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response;
            if (content == null)
            {
                response = await _httpClient.GetAsync(endpoint);
            }
            else
            {
                response = await _httpClient.PostAsync(endpoint, content);
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        
        public async Task<string> SearchAddressesAsync(string accessToken, string query, int count = 5)
        {
            string endpoint = $"parceladdress/2.0/domestic/addresses?count={count}&q={query}";
            return await GetApiDataAsync(accessToken, endpoint);
        }
    


        public async Task<string> CreatePickupBookingAsync(string accessToken, JObject bookingRequest)
        {
            string endpoint = "parcelpickup/v3/bookings";
            return await PostDataAsync(accessToken, endpoint, bookingRequest);
        }

        
        // More specific methods can be added here following the same pattern...
    }
}


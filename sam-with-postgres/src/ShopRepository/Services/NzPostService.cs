using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
            var clientId = _config["NZPost:ClientId"] ?? throw new Exception("NZPost Client ID not found in config");
            var clientSecret = _config["NZPost:ClientSecret"] ?? throw new Exception("NZPost Client Secret not found in config");
            var accessTokenEndpoint = _config["NZPost:AccessTokenEndpoint"] ?? throw new Exception("NZPost Access Token Endpoint not found in config");
            _httpClient.BaseAddress = new Uri(_config["NZPost:ApiBaseUrl"] ?? throw new Exception("NZPost API Base URL not found in config"));
            
            // Configure default request headers if necessary
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var response = await _httpClient.PostAsync(_config["NZPost:AccessTokenEndpoint"], new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", _config["NZPost:ClientId"]),
                new KeyValuePair<string, string>("client_secret", _config["NZPost:ClientSecret"])
            }));

            response.EnsureSuccessStatusCode();
            var tokenJson = await response.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<Dictionary<string, string>>(tokenJson);
            return token["access_token"];
        }

        public async Task<string> GetAddressDetailAsync(string accessToken, string addressId)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync($"parceladdress/2.0/domestic/addresses/{addressId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetAddressDetailByDpIdAsync(string accessToken, string dpId)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync($"parceladdress/2.0/domestic/addresses/dpid/{dpId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetAllDomesticAddressesAsync(string accessToken)
    {
        return await GetApiDataAsync(accessToken, "parceladdress/2.0/domestic/addresses");
    }

    public async Task<string> GetAllSuburbsAsync(string accessToken)
    {
        return await GetApiDataAsync(accessToken, "parceladdress/2.0/domestic/suburbs");
    }

    public async Task<string> GetPcdLocationsAsync(string accessToken)
    {
        return await GetApiDataAsync(accessToken, "parceladdress/2.0/domestic/pcdlocations");
    }

    public async Task<string> GetAllInternationalAddressesAsync(string accessToken)
    {
        return await GetApiDataAsync(accessToken, "parceladdress/2.0/international/addresses");
    }

    public async Task<string> GetInternationalAddressDetailAsync(string accessToken, string addressId)
    {
        return await GetApiDataAsync(accessToken, $"parceladdress/2.0/international/addresses/{addressId}");
    }

    public async Task<string> GetAustraliaAddressDetailAsync(string accessToken, string addressId)
    {
        return await GetApiDataAsync(accessToken, $"parceladdress/2.0/australia/addresses/{addressId}");
    }

    private async Task<string> GetApiDataAsync(string accessToken, string endpoint)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
    }
}

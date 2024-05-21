using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace ShopRepository.Services;

public class NzPostService
{
    private readonly string _apiBaseUrl = "https://api.nzpost.co.nz/";
    private readonly string _apiKey = "YOUR_NZ_POST_API_KEY"; // 替换为你的 API 密钥
    private readonly HttpClient _httpClient;

    public NzPostService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_apiBaseUrl)
        };
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    // TODO review this works, the schema of orders has changed since this was written
    public async Task<string> GenerateDeliveryLabelAsync(string checkoutSession, string customerId)
    {
        var request = new
        {
            checkout_session = checkoutSession,
            customer_id = customerId
        };

        var response = await _httpClient.PostAsJsonAsync("path/to/generate-label", request);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var deliveryLabel = JsonConvert.DeserializeObject<string>(responseBody);
        return deliveryLabel;
    }

    public async Task<string> TrackOrderAsync(string trackingId)
    {
        var response = await _httpClient.GetAsync($"path/to/track/{trackingId}");
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var trackingInfo = JsonConvert.DeserializeObject<string>(responseBody);
        return trackingInfo;
    }

    public async Task<DateTime> EstimateDeliveryDateAsync(string shippingMethod)
    {
        var request = new
        {
            shipping_method = shippingMethod
        };

        var response = await _httpClient.PostAsJsonAsync("path/to/estimate-delivery", request);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var estimatedDeliveryDate = JsonConvert.DeserializeObject<DateTime>(responseBody);
        return estimatedDeliveryDate;
    }
}
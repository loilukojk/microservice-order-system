using OrderService.Models;
using System.Text.Json;

namespace OrderService.Services;

public class ProductServiceClient : IProductServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductServiceClient> _logger;

    public ProductServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<ProductServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var productServiceUrl = configuration["ProductService:BaseUrl"] ?? "http://product-service:8080";
        _httpClient.BaseAddress = new Uri(productServiceUrl);
    }

    public async Task<StockInfo?> CheckStockAsync(int productId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/internal/products/{productId}/stock");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Product service returned {response.StatusCode} for product {productId}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var stockInfo = JsonSerializer.Deserialize<StockInfo>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Also get product price
            var productResponse = await _httpClient.GetAsync($"/products/{productId}");
            if (productResponse.IsSuccessStatusCode)
            {
                var productContent = await productResponse.Content.ReadAsStringAsync();
                var productData = JsonSerializer.Deserialize<JsonElement>(productContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (stockInfo != null && productData.TryGetProperty("price", out var priceElement))
                {
                    stockInfo.Price = priceElement.GetDecimal();
                }
            }

            return stockInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking stock for product {productId}");
            return null;
        }
    }
}

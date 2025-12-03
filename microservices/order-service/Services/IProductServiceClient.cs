using OrderService.Models;

namespace OrderService.Services;

public interface IProductServiceClient
{
    Task<StockInfo?> CheckStockAsync(int productId);
}

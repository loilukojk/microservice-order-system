using OrderService.Models;

namespace OrderService.Repositories;

public interface IOrderRepository
{
    void InitializeDatabase();
    Task<int> CreateOrderAsync(Order order);
    Task<Order?> GetOrderByIdAsync(int id);
    Task<IEnumerable<Order>> GetAllOrdersAsync();
}

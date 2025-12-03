using InventoryService.Models;

namespace InventoryService.Repositories;

public interface IInventoryRepository
{
    void InitializeDatabase();
    Task<Inventory?> GetInventoryByProductIdAsync(int productId);
    Task<IEnumerable<Inventory>> GetAllInventoriesAsync();
    Task UpdateStockAsync(int productId, int quantity);
}

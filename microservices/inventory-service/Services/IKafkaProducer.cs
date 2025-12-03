using InventoryService.Models;

namespace InventoryService.Services;

public interface IKafkaProducer
{
    Task PublishStockUpdatedAsync(StockUpdatedEvent stockEvent);
}

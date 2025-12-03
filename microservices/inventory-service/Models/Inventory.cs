namespace InventoryService.Models;

public class Inventory
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Stock { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class OrderCreatedEvent
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class StockUpdatedEvent
{
    public int ProductId { get; set; }
    public int NewStock { get; set; }
    public DateTime UpdatedAt { get; set; }
}

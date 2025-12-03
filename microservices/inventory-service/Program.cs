using InventoryService.Services;
using InventoryService.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register services
builder.Services.AddSingleton<IInventoryRepository, InventoryRepository>();
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();
builder.Services.AddHostedService<OrderConsumerService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var inventoryRepo = scope.ServiceProvider.GetRequiredService<IInventoryRepository>();
    inventoryRepo.InitializeDatabase();
}

// Get inventory for a product
app.MapGet("/inventory/{productId}", async (int productId, IInventoryRepository repo) =>
{
    var inventory = await repo.GetInventoryByProductIdAsync(productId);
    return inventory is not null ? Results.Ok(inventory) : Results.NotFound();
})
.WithName("GetInventory")
.WithOpenApi();

// Get all inventory records
app.MapGet("/inventory", async (IInventoryRepository repo) =>
{
    var inventories = await repo.GetAllInventoriesAsync();
    return Results.Ok(inventories);
})
.WithName("GetAllInventories")
.WithOpenApi();

app.Run();

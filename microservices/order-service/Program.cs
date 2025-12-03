using OrderService.Models;
using OrderService.Services;
using OrderService.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register services
builder.Services.AddSingleton<IOrderRepository, OrderRepository>();
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();
builder.Services.AddHttpClient<IProductServiceClient, ProductServiceClient>();

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
    var orderRepo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
    orderRepo.InitializeDatabase();
}

// Create Order Endpoint
app.MapPost("/orders", async (CreateOrderRequest request, IOrderRepository orderRepo,
    IProductServiceClient productClient, IKafkaProducer kafkaProducer) =>
{
    try
    {
        // Validate stock availability
        var stockInfo = await productClient.CheckStockAsync(request.ProductId);
        if (stockInfo == null || !stockInfo.Available || stockInfo.Stock < request.Quantity)
        {
            return Results.BadRequest(new { error = "Insufficient stock available" });
        }

        // Create order
        var order = new Order
        {
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            TotalPrice = stockInfo.Price * request.Quantity,
            Status = "Created",
            CreatedAt = DateTime.UtcNow
        };

        var orderId = await orderRepo.CreateOrderAsync(order);
        order.Id = orderId;

        // Publish OrderCreated event to Kafka
        var orderEvent = new OrderCreatedEvent
        {
            OrderId = order.Id,
            ProductId = order.ProductId,
            Quantity = order.Quantity
        };

        await kafkaProducer.PublishOrderCreatedAsync(orderEvent);

        return Results.Created($"/orders/{order.Id}", order);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error creating order: {ex.Message}");
    }
})
.WithName("CreateOrder")
.WithOpenApi();

// Get Order Endpoint
app.MapGet("/orders/{id}", async (int id, IOrderRepository orderRepo) =>
{
    var order = await orderRepo.GetOrderByIdAsync(id);
    return order is not null ? Results.Ok(order) : Results.NotFound();
})
.WithName("GetOrder")
.WithOpenApi();

// Get All Orders Endpoint
app.MapGet("/orders", async (IOrderRepository orderRepo) =>
{
    var orders = await orderRepo.GetAllOrdersAsync();
    return Results.Ok(orders);
})
.WithName("GetAllOrders")
.WithOpenApi();

app.Run();

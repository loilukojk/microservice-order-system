using Confluent.Kafka;
using InventoryService.Models;
using InventoryService.Repositories;
using System.Text.Json;

namespace InventoryService.Services;

public class OrderConsumerService : BackgroundService
{
    private readonly ILogger<OrderConsumerService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    public OrderConsumerService(
        ILogger<OrderConsumerService> logger,
        IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Order Consumer Service started");

        var config = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "kafka:9092",
            GroupId = "inventory-service-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();

        var topic = _configuration["Kafka:OrderCreatedTopic"] ?? "order.created.queue";
        consumer.Subscribe(topic);

        _logger.LogInformation($"Subscribed to topic: {topic}");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    if (consumeResult?.Message?.Value != null)
                    {
                        _logger.LogInformation($"Received message: {consumeResult.Message.Value}");

                        var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(consumeResult.Message.Value);

                        if (orderEvent != null)
                        {
                            await ProcessOrderCreatedEventAsync(orderEvent);
                        }
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message from Kafka");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Order Consumer Service is stopping");
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task ProcessOrderCreatedEventAsync(OrderCreatedEvent orderEvent)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var inventoryRepo = scope.ServiceProvider.GetRequiredService<IInventoryRepository>();
            var kafkaProducer = scope.ServiceProvider.GetRequiredService<IKafkaProducer>();

            // Update stock
            await inventoryRepo.UpdateStockAsync(orderEvent.ProductId, orderEvent.Quantity);

            _logger.LogInformation($"Stock updated for ProductId={orderEvent.ProductId}, Quantity reduced by {orderEvent.Quantity}");

            // Get updated inventory
            var updatedInventory = await inventoryRepo.GetInventoryByProductIdAsync(orderEvent.ProductId);

            if (updatedInventory != null)
            {
                // Publish StockUpdated event
                var stockUpdatedEvent = new StockUpdatedEvent
                {
                    ProductId = updatedInventory.ProductId,
                    NewStock = updatedInventory.Stock,
                    UpdatedAt = DateTime.UtcNow
                };

                await kafkaProducer.PublishStockUpdatedAsync(stockUpdatedEvent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing order created event: OrderId={orderEvent.OrderId}");
        }
    }
}

using Confluent.Kafka;
using InventoryService.Models;
using System.Text.Json;

namespace InventoryService.Services;

public class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(IConfiguration configuration, ILogger<KafkaProducer> logger)
    {
        _logger = logger;
        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "kafka:9092";
        _topic = configuration["Kafka:StockUpdatedTopic"] ?? "stock.updated.queue";

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            ClientId = "inventory-service"
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishStockUpdatedAsync(StockUpdatedEvent stockEvent)
    {
        try
        {
            var message = new Message<string, string>
            {
                Key = stockEvent.ProductId.ToString(),
                Value = JsonSerializer.Serialize(stockEvent)
            };

            var result = await _producer.ProduceAsync(_topic, message);
            _logger.LogInformation($"Stock updated event published: ProductId={stockEvent.ProductId}, NewStock={stockEvent.NewStock}, Status={result.Status}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error publishing stock updated event: ProductId={stockEvent.ProductId}");
            throw;
        }
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
}

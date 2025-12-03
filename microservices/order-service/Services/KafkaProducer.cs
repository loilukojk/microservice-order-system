using Confluent.Kafka;
using OrderService.Models;
using System.Text.Json;

namespace OrderService.Services;

public class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(IConfiguration configuration, ILogger<KafkaProducer> logger)
    {
        _logger = logger;
        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "kafka:9092";
        _topic = configuration["Kafka:OrderCreatedTopic"] ?? "order.created.queue";

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            ClientId = "order-service"
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishOrderCreatedAsync(OrderCreatedEvent orderEvent)
    {
        try
        {
            var message = new Message<string, string>
            {
                Key = orderEvent.OrderId.ToString(),
                Value = JsonSerializer.Serialize(orderEvent)
            };

            var result = await _producer.ProduceAsync(_topic, message);
            _logger.LogInformation($"Order created event published: OrderId={orderEvent.OrderId}, Status={result.Status}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error publishing order created event: OrderId={orderEvent.OrderId}");
            throw;
        }
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
}

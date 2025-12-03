using OrderService.Models;

namespace OrderService.Services;

public interface IKafkaProducer
{
    Task PublishOrderCreatedAsync(OrderCreatedEvent orderEvent);
}

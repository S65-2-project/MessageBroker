using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessageBroker
{
    public class RabbitQueuePublisher : IMessageQueuePublisher
    {
        private readonly RabbitConnectionFactory _connection;

        public RabbitQueuePublisher(RabbitConnectionFactory connection)
        {
            _connection = connection;
        }

        public Task PublishMessageAsync<T>(string exchange, string routingKey, string messageType, T value)
        {
            using var channel = _connection.CreateChannel();
            
            var basicProperties = channel.CreateBasicProperties();
            basicProperties.ContentType = "application/json";
            basicProperties.DeliveryMode = 2;
            // Add a MessageType header, this part is crucial for our solution because it is our way of distinguishing messages
            basicProperties.Headers = new Dictionary<string, object> { ["MessageType"] = messageType };
            
            var body = JsonSerializer.SerializeToUtf8Bytes(value);
            
            // Publish this without a routing key to the RabbitMQ broker
            channel.BasicPublish(exchange, routingKey, true, basicProperties, body);
            
            return Task.CompletedTask;
        }
    }
}

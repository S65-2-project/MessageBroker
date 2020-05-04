using System.Threading.Tasks;

namespace MessageBroker
{
    public interface IMessageQueuePublisher
    {
        Task PublishMessageAsync<T>(string routingKey, string messageType, T value);
    }
}

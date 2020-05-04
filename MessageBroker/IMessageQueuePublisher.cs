using System.Threading.Tasks;

namespace MessageBroker
{
    public interface IMessageQueuePublisher
    {
        
        /// <summary>
        ///     Publish a message async to the message queue
        /// </summary>
        /// <param name="routingKey">The message queue routing key binding </param>
        /// <param name="messageType">The message type defined in the header</param>
        /// <param name="value">The body of the message</param>
        Task PublishMessageAsync<T>(string routingKey, string messageType, T value);
    }
}

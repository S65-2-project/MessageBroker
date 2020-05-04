﻿using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageBroker
{
    public class RabbitQueueReader : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitConnectionFactory _connection;
        private readonly QueueName _queueName;
        private readonly MessageHandlerRepository _messageHandlerRepository;
        private IModel _channel;

        public RabbitQueueReader(
            IServiceProvider serviceProvider,
            RabbitConnectionFactory connection,
            QueueName queueName,
            MessageHandlerRepository messageHandlerRepository
        )
        {
            _serviceProvider = serviceProvider;
            _connection = connection;
            _queueName = queueName;
            _messageHandlerRepository = messageHandlerRepository;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Create a channel for this reader
            _channel = _connection.CreateChannel();

            // Create a consumer for the queue. This is a method implemented by RabbitMQ.Client to easily subscribe to incoming messages on this queue
            var consumer = new EventingBasicConsumer(_channel);

            // Add an event handler for receiving messages on the queue
            consumer.Received += (evt, evt2) =>
            {
                if (HandleMessage(evt2))
                {
                    // Acknowledge this message as handled
                    _channel.BasicAck(evt2.DeliveryTag, false);
                }
                else
                {
                    // Reject the message and put it back on the queue for trying again.
                    _channel.BasicReject(evt2.DeliveryTag, true);
                }
            };

            // On the provided queue name, register our consumer as a consumer.
            _channel.BasicConsume(_queueName.Name, false, consumer);

            return Task.CompletedTask;
        }

        private bool HandleMessage(BasicDeliverEventArgs message)
        {
            // The messages within our solution should have a MessageType header. If not, log and consider this message handled.
            if (!message.BasicProperties.Headers.TryGetValue("MessageType", out var objValue) ||
                !(objValue is byte[] valueAsBytes))
            {
                // Return true, we will never be able to handle a message without MessageType, thus no point in trying.
                return true;
            }

            var messageType = Encoding.UTF8.GetString(valueAsBytes);

            if (!_messageHandlerRepository.TryGetHandlerForMessageType(messageType, out var implementingHandler))
            {
                // There is no handler for the given MessageType. This is a valid case, but we log it as information just so the developers know that they are skipping this.
                return true;
            }

            try
            {
                // Create a service provider scope. Normally this is done by Asp.NET but this is outside of the asp.net lifecycle. Thus do it ourselves
                using var scope = _serviceProvider.CreateScope();

                // Retrieve the IMessageHandler from the service provider, using this way we can easily get scoped services like DbContext.
                var handler = scope.ServiceProvider.GetService(implementingHandler) as IMessageHandler;
                handler?.HandleMessageAsync(messageType, message.Body).GetAwaiter().GetResult();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _channel?.Dispose();
            _channel = null;

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}

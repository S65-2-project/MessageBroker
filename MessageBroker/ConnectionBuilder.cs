﻿using System;
using Microsoft.Extensions.DependencyInjection;

namespace MessageBroker
{
    public static class ConnectionBuilder
    {
        /// <summary>
        ///     Dependency inject a message consumer to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="queueName">The name of the queue</param>
        /// <param name="builderFn">The message builder</param>
        public static void AddMessageConsumer(this IServiceCollection services, string queueName , Action<MessagingBuilder> builderFn = null)
        {
            var builder = new MessagingBuilder(services);
            
            services.AddHostedService<RabbitQueueReader>();
            services.AddSingleton(new MessageHandlerRepository(builder.MessageHandlers));

            builderFn?.Invoke(builder);
            
            var queueNameService = new QueueName(queueName);
            services.AddSingleton(queueNameService);
            
            var connection = new RabbitConnectionFactory();
            services.AddSingleton(connection); 
        }

        /// <summary>
        ///     Dependency inject a message publisher to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        public static void AddMessagePublisher(this IServiceCollection services)
        {
            var connection = new RabbitConnectionFactory();
            services.AddSingleton(connection);
            
            services.AddScoped<IMessageQueuePublisher, RabbitQueuePublisher>();        
        }
    }
}

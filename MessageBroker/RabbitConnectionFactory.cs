using System;
using RabbitMQ.Client;

namespace MessageBroker
{
    public class RabbitConnectionFactory
    {
        private IConnection _connection;

        public IModel CreateChannel()
        {
            var connection = GetConnection();
            return connection.CreateModel();
        }

        private IConnection GetConnection()
        {
            if (_connection != null) return _connection;
            
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://guest:5vNo0n3736hBEoFz96NPxmt7M0mUazv@rabbit.feddema.dev:9017"),
                AutomaticRecoveryEnabled =
                    true // When the connection is lost, this will automatically reconnect us when it can get back up
            };
            _connection = factory.CreateConnection();

            return _connection;
        }
    }
}

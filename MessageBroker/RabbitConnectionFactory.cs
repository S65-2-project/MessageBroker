using System;
using RabbitMQ.Client;

namespace MessageBroker
{
    public class RabbitConnectionFactory
    {
        private IConnection _connection;
        private readonly string _connectionUri;

        public RabbitConnectionFactory(string connectionUri)
        {
            _connectionUri = connectionUri;
        }

        public IModel CreateChannel()
        {
            var connection = GetConnection(_connectionUri);
            return connection.CreateModel();
        }

        private IConnection GetConnection(string connectionUri)
        {
            if (_connection != null) return _connection;
            
            var factory = new ConnectionFactory
            {
                Uri = new Uri(connectionUri),
                // When the connection is lost, this will automatically reconnect us when it can get back up
                AutomaticRecoveryEnabled = true
            };
            _connection = factory.CreateConnection();

            return _connection;
        }
    }
}

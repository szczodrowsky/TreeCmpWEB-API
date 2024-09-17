using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;
using System.Text;

namespace TreeCmpWebAPI.Services
{
    public class RabbitMqService
    {
        private readonly string _hostName = "rabbitmq";
        private readonly string _queueName = "treecmp_queue";

        public void SendCommand(string command)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",  
                Port = 5672,            
                UserName = "guest",  
                Password = "guest" 
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: _queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var body = Encoding.UTF8.GetBytes(command);

            channel.BasicPublish(exchange: "",
                                 routingKey: _queueName,
                                 basicProperties: null,
                                 body: body);
        }
    }
}
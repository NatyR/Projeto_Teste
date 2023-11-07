using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.API.Services
{
    public class RabbitService
    {
        public Common.EnvironmentsBase environmentsBase;

        public RabbitService(IConfiguration _configuration)
        {
            environmentsBase = new Common.EnvironmentsBase(_configuration);
        }

        public void Publish(string queue, string message)
        {
            ConnectionFactory _factory = new ConnectionFactory();
            if(queue == "send-email")
            {
                _factory = new ConnectionFactory()
                {
                    Uri = new Uri(environmentsBase.AWS_MQ_CONNECTIONSTRING),
                    UserName = environmentsBase.AWS_MQ_USERNAME,
                    Password = environmentsBase.AWS_MQ_PASSWORD
                };
            }
            else
            {
                _factory = new ConnectionFactory()
                {
                    Uri = new Uri(environmentsBase.MQ_CONNECTIONSTRING),
                    UserName = environmentsBase.MQ_USERNAME,
                    Password = environmentsBase.MQ_PASSWORD
                };
            }
            var body = Encoding.UTF8.GetBytes(message);
            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();
            {
                channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
                channel.BasicPublish(exchange: "", routingKey: queue, basicProperties: null, body: body);
            }

        }
    }
}

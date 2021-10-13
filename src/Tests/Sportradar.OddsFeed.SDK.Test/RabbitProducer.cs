using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Test
{
    class RabbitProducer
    {
        private readonly string _rabbitIp;
        private IConnection _connection;
        private IModel _channel;
        private bool _isRunning;
        private ITimer _timer;

        public ConcurrentQueue<RabbitMessage> Messages { get; }

        public RabbitProducer(string rabbitIp)
        {
            _rabbitIp = rabbitIp;
            Messages = new ConcurrentQueue<RabbitMessage>();
            _isRunning = false;
        }

        public void Start()
        {
            var factory = new ConnectionFactory {HostName = _rabbitIp};
            factory.Password = "testpass";
            factory.UserName = "testuser";
            factory.VirtualHost = "/virtualhost";
            //factory.Ssl.Enabled = true;
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "hello",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            _timer = new SdkTimer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            _timer.Elapsed += TimerCheckAndSend;
            _timer.Start();
            _isRunning = true;
        }

        public void Stop()
        {
            _timer.Stop();
            _channel.Close();
            _channel.Dispose();
            _connection.Close();
            _connection.Dispose();
        }

        public void Send(string message, string routingKey)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "",
                                 routingKey: routingKey,
                                 basicProperties: null,
                                 body: body);
            Console.WriteLine(" [x] Sent {0}", message);

            
        }

        public void Send(TestFeedMessage message, string routingKey)
        {

        }

        private void TimerCheckAndSend(object? sender, EventArgs e)
        {
            if (!_isRunning)
            {
                return;
            }

            foreach (var rabbitMessage in Messages)
            {
                if (rabbitMessage.Created < DateTime.Now)
                {
                    Send(rabbitMessage.Message.ToString(), "hello");
                }
            }
        }
    }

    public class RabbitMessage
    {
        public TestFeedMessage Message { get; }

        public DateTime Created { get; }

        public TimeSpan Period { get; }

        public bool RandomizeId { get; }
    }

}

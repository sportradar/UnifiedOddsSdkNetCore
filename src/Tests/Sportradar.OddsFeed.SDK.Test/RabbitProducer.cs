using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Internal;
using EasyNetQ.Management.Client;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Test
{
    class RabbitProducer
    {
        private const string UfExchange = "unifiedfeed";
        private readonly string _rabbitIp;
        private IConnection _connection;
        private IModel _channel;
        private bool _isRunning;
        private ITimer _timer;
        public ManagementClient ManagementClient { get; }

        public ConcurrentQueue<RabbitMessage> Messages { get; }

        public ConcurrentDictionary<int, DateTime> ProducersAlive { get; }

        public RabbitProducer(string rabbitIp)
        {
            _rabbitIp = rabbitIp;
            Messages = new ConcurrentQueue<RabbitMessage>();
            ProducersAlive = new ConcurrentDictionary<int, DateTime>();
            _isRunning = false;
            ManagementClient = new ManagementClient($"http://{_rabbitIp}", "guest", "guest");
        }

        public void Start()
        {
            var factory = new ConnectionFactory {HostName = _rabbitIp};
            //factory.Password = "testpass";
            //factory.UserName = "testuser";
            factory.VirtualHost = "/virtualhost";
            //factory.Ssl.Enabled = true;
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: UfExchange, type: ExchangeType.Topic, true);
            ////_channel.QueueDeclare(queue: "hello",
            //                      durable: false,
            //                      exclusive: false,
            //                      autoDelete: false,
            //                      arguments: null);

            _timer = new SdkTimer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            _timer.Elapsed += TimerCheckAndSend;
            _timer.Start();
            _isRunning = true;

            Thread.Sleep(2000);
            GetRabbitConnection();
        }

        public void Stop()
        {
            _timer.Stop();
            _channel.Close();
            _channel.Dispose();
            _connection.Close();
            _connection.Dispose();
        }

        public void Send(string message, string routingKey, long timestamp = 0)
        {
            if (timestamp == 0)
            {
                timestamp = Helper.ToEpochTime(DateTime.Now);
            }

            var body = Encoding.UTF8.GetBytes(message);

            var basicProperties = new BasicProperties();
            basicProperties.Headers = new Dictionary<string, object>();
            basicProperties.Headers.Add("timestamp_in_ms", timestamp);

            _channel.BasicPublish(exchange: UfExchange,
                                 routingKey: routingKey,
                                 basicProperties: basicProperties,
                                 body: body);
            Helper.WriteToOutput($"Routing: {routingKey}, Generated:{Helper.FromEpochTime(timestamp)},  Msg: {message}");
        }

        public void Send(TestFeedMessage message, string routingKey, long timestamp = 0)
        {
            var msgBody = nameof(message);
            if (message is alive aliveMsg)
            {
                msgBody = Helper.Serialize(aliveMsg);
            }

            Send(msgBody, routingKey, timestamp);
        }

        public void AddProducersAlive(int producerId)
        {
            if (!ProducersAlive.ContainsKey(producerId))
            {
                var task = Task.Run(() => SendPeriodicAliveForProducer(producerId));
                ProducersAlive.TryAdd(producerId, DateTime.MinValue);
            }
        }

        public void StopProducersAlive(int producerId)
        {
            if (ProducersAlive.ContainsKey(producerId))
            {
                ProducersAlive.TryRemove(producerId, out var x);
            }
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

        private void SendPeriodicAliveForProducer(int producerId, int periodInMs = 0)
        {
            Thread.Sleep(5000);
            while (ProducersAlive.ContainsKey(producerId))
            {
                var generated = DateTime.Now;
                var msgAlive = new alive { product = producerId, subscribed = 1, timestamp = Helper.ToEpochTime(generated) };
                var msgBody = Helper.Serialize(msgAlive);
                Send(msgAlive, "-.-.-.alive.-.-.-.-", msgAlive.timestamp);

                var sleep = periodInMs > 0 ? periodInMs : new Random().Next(10000);
                Thread.Sleep(sleep);
            }
        }

        private void GetRabbitConnection()
        {
            var connections = ManagementClient.GetConnectionsAsync().Result;
            if (!connections.IsNullOrEmpty())
            {

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

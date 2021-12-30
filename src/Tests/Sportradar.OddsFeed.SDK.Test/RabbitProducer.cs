using Castle.Core.Internal;
using EasyNetQ.Management.Client;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Test.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Test
{
    /// <summary>
    /// Management of rabbit server and sending messages
    /// </summary>
    /// <remarks>On rabbit server there should be additional user:testuser/testpass and virtual host: /virtualhost with read/write permission</remarks>
    class RabbitProducer
    {
        public const string RabbitIp = "192.168.64.101";
        private const string UfExchange = "unifiedfeed";
        private const string VirtualHostName = "/virtualhost";
        private IConnection _connection;
        private IModel _channel;
        private bool _isRunning;
        private ITimer _timer;
        private readonly FMessageBuilder _fMessageBuilder;
        private readonly TestProducersProvider _producersProvider;

        /// <summary>
        /// Gets the management client for getting and managing connections and channels
        /// </summary>
        /// <value>The management client.</value>
        public ManagementClient ManagementClient { get; }

        /// <summary>
        /// Gets the messages to be send
        /// </summary>
        public ConcurrentQueue<RabbitMessage> Messages { get; }

        /// <summary>
        /// Gets the list of producers for which alive messages should be periodically send
        /// </summary>
        public ConcurrentDictionary<int, DateTime> ProducersAlive { get; }

        public RabbitProducer(TestProducersProvider producersProvider)
        {
            _producersProvider = producersProvider;
            Messages = new ConcurrentQueue<RabbitMessage>();
            ProducersAlive = new ConcurrentDictionary<int, DateTime>();
            _fMessageBuilder = new FMessageBuilder(1);
            _isRunning = false;
            ManagementClient = new ManagementClient($"http://{RabbitIp}", "guest", "guest");

            //var x = ManagementClient.GetExchangesAsync().Result;

            //var rabbitUsers = ManagementClient.GetUsersAsync().Result;
            //User testUser;
            //if (rabbitUsers.Any(a => a.Name.Equals("testuser")))
            //{
            //    testUser = ManagementClient.GetUserAsync("testuser").Result;
            //}
            //else
            //{
            //    testUser = ManagementClient.CreateUserAsync(new UserInfo("testuser", "testpass")).Result;
            //}

            //var virtualHosts = ManagementClient.GetVhostsAsync().Result;
            //Vhost virtualHost;
            //if (virtualHosts.Any(a => a.Name.Equals(VirtualHostName)))
            //{
            //    virtualHost = ManagementClient.GetVhostAsync(VirtualHostName).Result;
            //}
            //else
            //{
            //    virtualHost = ManagementClient.CreateVhostAsync(VirtualHostName).Result;
            //    ManagementClient.CreatePermissionAsync(new PermissionInfo(testUser, virtualHost)).RunSynchronously();
            //    ManagementClient.CreatePermissionAsync(new PermissionInfo(ManagementClient.GetUserAsync("guest").Result, virtualHost)).RunSynchronously();
            //}
        }

        // start the connection for sending messages
        public void Start()
        {
            // clean connections
            var activeConnections = ManagementClient.GetConnections();
            foreach (var connection in activeConnections)
            {
                ManagementClient.CloseConnection(connection);
            }

            // factory uses default user: guest/guest
            var factory = new ConnectionFactory
            {
                HostName = RabbitIp,
                VirtualHost = VirtualHostName
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: UfExchange, type: ExchangeType.Topic, true);

            _timer = new SdkTimer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            _timer.Elapsed += TimerCheckAndSend;
            _timer.Start();
            _isRunning = true;
        }

        // stop the connection for sending messages
        public void Stop()
        {
            _timer.Stop();
            _channel.Close();
            _channel.Dispose();
            _connection.Close();
            _connection.Dispose();
        }

        /// <summary>
        /// Sends the message to the rabbit server
        /// </summary>
        /// <param name="message">The message should be valid xml</param>
        /// <param name="routingKey">The routing key</param>
        /// <param name="timestamp">The timestamp applied to the message or Now</param>
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
            Helper.WriteToOutput($"Generated:{Helper.FromEpochTime(timestamp).ToLongTimeString()},  Routing: {routingKey}, Msg: {message}");
        }

        /// <summary>
        /// Sends the message
        /// </summary>
        /// <param name="message">The message to be send</param>
        /// <param name="routingKey">The routing key to be applied or it will be generated based on message type</param>
        /// <param name="timestamp">The timestamp  to be applied or Now</param>
        public void Send(TestFeedMessage message, string routingKey = null, long timestamp = 0)
        {
            var msgBody = BuildMessageBody(message);

            if (routingKey.IsNullOrEmpty())
            {
                routingKey = BuildRoutingKey(message);
            }

            Send(msgBody, routingKey, timestamp);
        }

        /// <summary>
        /// Adds the producerId for periodically send alive messages. And start immediately.
        /// </summary>
        /// <param name="producerId">The producer id</param>
        /// <param name="periodInMs">The period in ms before next is send</param>
        public void AddProducersAlive(int producerId, int periodInMs = 0)
        {
            if (!ProducersAlive.ContainsKey(producerId))
            {
                Task.Run(() => SendPeriodicAliveForProducer(producerId, periodInMs));
                ProducersAlive.TryAdd(producerId, DateTime.MinValue);
            }
        }

        /// <summary>
        /// Removes the producerId for periodically send alive messages. On next iteration will stop.
        /// </summary>
        /// <param name="producerId">The producer id</param>
        public void StopProducersAlive(int producerId)
        {
            if (ProducersAlive.ContainsKey(producerId))
            {
                ProducersAlive.TryRemove(producerId, out _);
            }
        }

        /// <summary>
        /// Periodically check if there are messages to be send (in Messages queue)
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TimerCheckAndSend(object sender, EventArgs e)
        {
            if (!_isRunning)
            {
                return;
            }

            foreach (var rabbitMessage in Messages)
            {
                if (rabbitMessage.LastSent < DateTime.Now)
                {
                    Send(rabbitMessage.Message.ToString(), "hello");
                }
            }
        }

        /// <summary>
        /// Sends the periodic alive for producer (if producerId is listed in ProducersAlive).
        /// </summary>
        /// <param name="producerId">The producer id</param>
        /// <param name="periodInMs">The period in ms before next is send</param>
        private void SendPeriodicAliveForProducer(int producerId, int periodInMs = 0)
        {
            Thread.Sleep(5000);
            while (ProducersAlive.ContainsKey(producerId))
            {
                var msgAlive = _fMessageBuilder.BuildAlive(producerId, DateTime.Now, true);
                Send(msgAlive, "-.-.-.alive.-.-.-.-", msgAlive.timestamp);

                var sleep = periodInMs > 0 ? periodInMs : new Random().Next(10000);
                Thread.Sleep(sleep);
            }
        }

        /// <summary>
        /// Change rabbit user password / or create new user
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="newPassword">The password</param>
        /// <returns>If successful</returns>
        //curl -i -u guest:guest http://192.168.64.100:15672/api/vhosts
        //curl -i -u guest:guest http://192.168.64.100:15672/api/users/testuser -H "content-type:application/json"  -XDELETE -d '{"password":"testpass","tags":"administrator"}'
        //curl -i -u guest:guest http://192.168.64.100:15672/api/users/testuser -H "content-type:application/json"  -XPUT -d "{""password"":""testpass"",""tags"":""administrator""}"
        public bool RabbitChangeUserPassword(string username, string newPassword)
        {
            // Set MQ server credentials
            var networkCredential = new NetworkCredential("guest", "guest");

            // Instantiate HttpClientHandler, passing in the NetworkCredential
            var httpClientHandler = new HttpClientHandler { Credentials = networkCredential };

            // Instantiate HttpClient passing in the HttpClientHandler
            using var httpClient = new HttpClient(httpClientHandler);


            var mqUser = new MqUser
            {
                password = newPassword,
                tags = "administrator"
            };

            var info = JsonConvert.SerializeObject(mqUser);
            var content = new StringContent(info, Encoding.UTF8, "application/json");

            //HttpContent httpContent = new StringContent($"{{\"password\":\"{newPassword}\",\"tags\":\"administrator\"}}}}", Encoding.UTF8, "application/json");
            var httpResponseMessage = httpClient.PutAsync($"http://{RabbitIp}:15672/api/users/{username}", content).Result;
            if (httpResponseMessage != null)
            {
                var responseContent = string.Empty;
                if (httpResponseMessage.Content != null)
                {
                    responseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;
                }
                Helper.WriteToOutput($"Rabbit ApiCall -> change user password: StatusCode={httpResponseMessage.StatusCode}, RequestContent={content.ReadAsStringAsync().Result}, ResponseContent={responseContent}");

                return httpResponseMessage.IsSuccessStatusCode;
            }

            return false;
        }

        /// <summary>
        /// Builds the xml message body from the raw feed message (instance)
        /// </summary>
        /// <param name="message">The message to be serialized</param>
        /// <returns>Returns xml message body</returns>
        private string BuildMessageBody(TestFeedMessage message)
        {
            if (message is alive aliveMsg)
            {
                return Helper.Serialize(aliveMsg);
            }
            if (message is odds_change oddsChange)
            {
                return Helper.Serialize(oddsChange);
            }
            if (message is bet_stop betStop)
            {
                return Helper.Serialize(betStop);
            }
            if (message is fixture_change fixtureChange)
            {
                return Helper.Serialize(fixtureChange);
            }
            if (message is snapshot_complete snapshotComplete)
            {
                return Helper.Serialize(snapshotComplete);
            }
            if (message is bet_settlement betSettlement)
            {
                return Helper.Serialize(betSettlement);
            }

            return Helper.Serialize(message);
        }

        /// <summary>
        /// Builds the routing key from the message type
        /// </summary>
        /// <param name="message">The message</param>
        /// <returns>Returns the appropriate routing key</returns>
        private string BuildRoutingKey(TestFeedMessage message)
        {
            if (message.GetType() == typeof(alive))
            {
                return "-.-.-.alive.-.-.-.-";
            }
            else if (message.GetType() == typeof(odds_change))
            {
                var oddsChange = (odds_change)message;
                var sportId = 1; //oddsChange
                var urn = URN.Parse(oddsChange.event_id);
                return $"hi.{BuildSessionPartOfRoutingKey(oddsChange.product)}.odds_change.{sportId}.{urn.Prefix}:{urn.Type}.{urn.Id}.-";
            }
            else if (message.GetType() == typeof(bet_stop))
            {
                var betStop = (bet_stop)message;
                var sportId = 1;
                var urn = URN.Parse(betStop.event_id);
                return $"hi.{BuildSessionPartOfRoutingKey(betStop.product)}.bet_stop.{sportId}.{urn.Prefix}:{urn.Type}.{urn.Id}.-";
            }
            else if (message.GetType() == typeof(fixture_change))
            {
                var fixtureChange = (fixture_change)message;
                var sportId = 1;
                var urn = URN.Parse(fixtureChange.event_id);
                return $"hi.pre.live.fixture_change.{sportId}.{urn.Prefix}:{urn.Type}.{urn.Id}.-";
            }
            else if (message.GetType() == typeof(snapshot_complete))
            {
                return "-.-.-.snapshot_complete.-.-.-.-";
            }
            else if (message.GetType() == typeof(bet_settlement))
            {
                var betSettlement = (bet_settlement)message;
                var sportId = 1;
                var urn = URN.Parse(betSettlement.event_id);
                return $"lo.{BuildSessionPartOfRoutingKey(betSettlement.product)}.bet_settlement.{sportId}.{urn.Prefix}:{urn.Type}.{urn.Id}.-";
            }

            return string.Empty;
        }

        private string BuildSessionPartOfRoutingKey(int producerId)
        {
            var producer = _producersProvider.Producers.FirstOrDefault(f => f.Id == producerId);
            if (producer == null)
            {
                return "missing.missing";
            }

            if (((Producer)producer).Scope.Contains("live"))
            {
                return "-.live";
            }
            if (((Producer)producer).Scope.Contains("prematch"))
            {
                return "pre.-";
            }
            if (((Producer)producer).Scope.Contains("virtual"))
            {
                return "virt.-";
            }

            return "na.na";
        }
    }

    /// <summary>
    /// Class RabbitMessage
    /// </summary>
    public class RabbitMessage
    {
        /// <summary>
        /// Gets the message to be send
        /// </summary>
        /// <value>The message to be send</value>
        public TestFeedMessage Message { get; }

        /// <summary>
        /// Gets the value when last message was sent (used only for period messages)
        /// </summary>
        /// <value>The last message sent time</value>
        public DateTime LastSent { get; }

        /// <summary>
        /// Gets the period on which message should be send (if set)
        /// </summary>
        /// <value>The period</value>
        public TimeSpan Period { get; }

        /// <summary>
        /// Gets a value indicating whether id of the message should be randomized
        /// </summary>
        /// <value><c>true</c> if [randomize identifier]; otherwise, <c>false</c>.</value>
        public bool RandomizeId { get; }
    }

    public class MqUser
    {
        // ReSharper disable once InconsistentNaming
        public string password;
        // ReSharper disable once InconsistentNaming
        public string tags;
    }
}

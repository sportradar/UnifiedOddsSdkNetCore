// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ.Management.Client;
using EasyNetQ.Management.Client.Model;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Mock.Feed;

/// <summary>
///     Management of rabbit server and sending messages
/// </summary>
/// <remarks>On rabbit server there should be additional user:testuser/testpass and virtual host: /virtualhost with
///     read/write permission</remarks>
public class RabbitManagement
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly ProjectConfiguration _projConfig;
    private IModel _channel;
    private IConnection _connection;
    private bool _isRunning;
    private ISdkTimer _timer;

    /// <summary>
    ///     Gets the management client for getting and managing connections and channels
    /// </summary>
    /// <value>The management client.</value>
    public ManagementClient ManagementClient { get; }

    /// <summary>
    ///     Gets the messages to be sent
    /// </summary>
    public ConcurrentQueue<RabbitMessage> Messages { get; }

    /// <summary>
    ///     Gets the list of producers for which alive messages should be periodically send
    /// </summary>
    public ConcurrentDictionary<int, DateTime> ProducersAlive { get; }

    public RabbitManagement(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        Messages = new ConcurrentQueue<RabbitMessage>();
        ProducersAlive = new ConcurrentDictionary<int, DateTime>();
        _isRunning = false;

        _projConfig = new ProjectConfiguration();

        ManagementClient = new ManagementClient(new Uri($"http://{_projConfig.GetRabbitIp()}:15672"),
                                                _projConfig.DefaultAdminRabbitUserName,
                                                _projConfig.DefaultAdminRabbitPassword,
                                                TimeSpan.FromMinutes(1));
    }

    public void ResetRabbit()
    {
        var _ = ManagementClient.GetExchanges();
        var rabbitUsers = ManagementClient.GetUsers();
        if (!rabbitUsers.Any(a => a.Name.Equals(_projConfig.SdkRabbitUsername, StringComparison.OrdinalIgnoreCase)))
        {
            ManagementClient.CreateUser(UserInfo.ByPassword(_projConfig.SdkRabbitUsername, _projConfig.SdkRabbitPassword).AddTag("administrator"));
        }

        var testUser = ManagementClient.GetUser(_projConfig.SdkRabbitUsername);

        var virtualHosts = ManagementClient.GetVhosts();
        if (!virtualHosts.Any(a => a.Name.Equals(_projConfig.VirtualHostName, StringComparison.OrdinalIgnoreCase)))
        {
            ManagementClient.CreateVhost(_projConfig.VirtualHostName);
        }

        var virtualHost = ManagementClient.GetVhost(_projConfig.VirtualHostName);
        ManagementClient.CreatePermission(virtualHost, new PermissionInfo(testUser.Name));
        ManagementClient.CreatePermission(virtualHost, new PermissionInfo(_projConfig.DefaultAdminRabbitUserName));
    }

    // start the connection for sending messages
    public void Start()
    {
        ProducersAlive.Clear();

        // clean connections
        var activeConnections = ManagementClient.GetConnections();
        foreach (var connection in activeConnections)
        {
            ManagementClient.CloseConnection(connection);
        }

        _outputHelper.WriteLine($"Starting connection with HostName={_projConfig.GetRabbitIp()}");
        var factory = new ConnectionFactory { HostName = _projConfig.GetRabbitIp(), UserName = _projConfig.DefaultAdminRabbitUserName, Password = _projConfig.DefaultAdminRabbitPassword, VirtualHost = _projConfig.VirtualHostName };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(_projConfig.UfExchange, ExchangeType.Topic, true);

        _timer = new SdkTimer("RabbitCheckAndSend", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
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
    ///     Sends the message
    /// </summary>
    /// <param name="message">The message to be sent</param>
    /// <param name="routingKey">The routing key to be applied, or it will be generated based on message type</param>
    /// <param name="timestamp">The timestamp  to be applied or Now</param>
    public void Send(FeedMessage message, string routingKey = null, long timestamp = 0)
    {
        var msgBody = BuildMessageBody(message);

        if (routingKey.IsNullOrEmpty())
        {
            routingKey = BuildRoutingKey(message);
        }

        Send(msgBody, routingKey, timestamp);
    }

    /// <summary>
    ///     Sends the message to the rabbit server
    /// </summary>
    /// <param name="message">The message should be valid xml</param>
    /// <param name="routingKey">The routing key</param>
    /// <param name="timestamp">The timestamp applied to the message or Now</param>
    public void Send(string message, string routingKey, long timestamp = 0)
    {
        if (timestamp == 0)
        {
            timestamp = DateTime.Now.ToEpochTime();
        }

        var body = Encoding.UTF8.GetBytes(message);

        var basicProperties = _channel.CreateBasicProperties();
        basicProperties.Headers = new Dictionary<string, object> { { "timestamp_in_ms", timestamp } };

        _channel.BasicPublish(_projConfig.UfExchange,
                              routingKey,
                              basicProperties,
                              body);
        _outputHelper.WriteLine($"MQ sent: {timestamp.FromEpochTime().ToLongTimeString()}   {routingKey}   {message}");
    }

    /// <summary>
    ///     Adds the producerId for periodically send alive messages. And start immediately.
    /// </summary>
    /// <param name="producerId">The producer id</param>
    /// <param name="periodInMs">The period in ms before next is sent</param>
    public void AddProducersAlive(int producerId, int periodInMs = 0)
    {
        if (!ProducersAlive.ContainsKey(producerId))
        {
            Task.Run(() => SendPeriodicAliveForProducer(producerId, periodInMs));
            ProducersAlive.TryAdd(producerId, DateTime.MinValue);
        }
    }

    /// <summary>
    ///     Removes the producerId for periodically send alive messages. On next iteration will stop.
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
    ///     Periodically check if there are messages to be sent (in Messages queue)
    /// </summary>
    /// <param name="sender">The sender</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
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
    ///     Sends the periodic alive for producer (if producerId is listed in ProducersAlive).
    /// </summary>
    /// <param name="producerId">The producer id</param>
    /// <param name="periodInMs">The period in ms before next is sent</param>
    private void SendPeriodicAliveForProducer(int producerId, int periodInMs = 0)
    {
        Thread.Sleep(5000);
        while (ProducersAlive.ContainsKey(producerId))
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            var msgAlive = new FeedMessageBuilder(producerId).BuildAlive(producerId, DateTime.Now, true);
            Send(msgAlive, "-.-.-.alive.-.-.-.-", msgAlive.timestamp);

            var sleep = periodInMs > 0 ? periodInMs : StaticRandom.I(10000);
            Thread.Sleep(sleep);
        }
    }

    /// <summary>
    ///     Change rabbit user password / or create new user
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
        var networkCredential = new NetworkCredential("consumer", "consumer");

        // Instantiate HttpClientHandler, passing in the NetworkCredential
        var httpClientHandler = new HttpClientHandler { Credentials = networkCredential };

        // Instantiate HttpClient passing in the HttpClientHandler
        using var httpClient = new HttpClient(httpClientHandler);

        var mqUser = new MqUser { password = newPassword, tags = "administrator" };

        var info = JsonConvert.SerializeObject(mqUser);
        var content = new StringContent(info, Encoding.UTF8, "application/json");

        //var httpContent = new StringContent($"{{\"password\":\"{newPassword}\",\"tags\":\"administrator\"}}}}", Encoding.UTF8, "application/json");
        var httpResponseMessage = httpClient.PutAsync($"http://{_projConfig.GetRabbitIp()}:15672/api/users/{username}", content).GetAwaiter().GetResult();
        if (httpResponseMessage != null)
        {
            var responseContent = httpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            _outputHelper.WriteLine($"Rabbit ApiCall -> change user password: StatusCode={httpResponseMessage.StatusCode}, RequestContent={content.ReadAsStringAsync().GetAwaiter().GetResult()}, ResponseContent={responseContent}");

            return httpResponseMessage.IsSuccessStatusCode;
        }

        return false;
    }

    /// <summary>
    ///     Builds the xml message body from the raw feed message (instance)
    /// </summary>
    /// <param name="message">The message to be serialized</param>
    /// <returns>Returns xml message body</returns>
    private static string BuildMessageBody(FeedMessage message)
    {
        if (message is alive aliveMsg)
        {
            return MsgSerializer.SerializeToXml(aliveMsg);
        }

        if (message is odds_change oddsChange)
        {
            return MsgSerializer.SerializeToXml(oddsChange);
        }

        if (message is bet_stop betStop)
        {
            return MsgSerializer.SerializeToXml(betStop);
        }

        if (message is fixture_change fixtureChange)
        {
            return MsgSerializer.SerializeToXml(fixtureChange);
        }

        if (message is snapshot_complete snapshotComplete)
        {
            return MsgSerializer.SerializeToXml(snapshotComplete);
        }

        if (message is bet_settlement betSettlement)
        {
            return MsgSerializer.SerializeToXml(betSettlement);
        }

        return MsgSerializer.SerializeToXml(message);
    }

    /// <summary>
    ///     Builds the routing key from the message type
    /// </summary>
    /// <param name="message">The message</param>
    /// <returns>Returns the appropriate routing key</returns>
    private static string BuildRoutingKey(FeedMessage message)
    {
        if (message.GetType() == typeof(alive))
        {
            return "-.-.-.alive.-.-.-.-";
        }

        if (message.GetType() == typeof(odds_change))
        {
            var oddsChange = (odds_change)message;
            var sportId = 1; //oddsChange
            var urn = Urn.Parse(oddsChange.event_id);
            return $"hi.{BuildSessionPartOfRoutingKey(oddsChange.product)}.odds_change.{sportId}.{urn.Prefix}:{urn.Type}.{urn.Id}.-";
        }

        if (message.GetType() == typeof(bet_stop))
        {
            var betStop = (bet_stop)message;
            var sportId = 1;
            var urn = Urn.Parse(betStop.event_id);
            return $"hi.{BuildSessionPartOfRoutingKey(betStop.product)}.bet_stop.{sportId}.{urn.Prefix}:{urn.Type}.{urn.Id}.-";
        }

        if (message.GetType() == typeof(fixture_change))
        {
            var fixtureChange = (fixture_change)message;
            var sportId = 1;
            var urn = Urn.Parse(fixtureChange.event_id);
            return $"hi.pre.live.fixture_change.{sportId}.{urn.Prefix}:{urn.Type}.{urn.Id}.-";
        }

        if (message.GetType() == typeof(snapshot_complete))
        {
            return "-.-.-.snapshot_complete.-.-.-.-";
        }

        if (message.GetType() == typeof(bet_settlement))
        {
            var betSettlement = (bet_settlement)message;
            var sportId = 1;
            var urn = Urn.Parse(betSettlement.event_id);
            return $"lo.{BuildSessionPartOfRoutingKey(betSettlement.product)}.bet_settlement.{sportId}.{urn.Prefix}:{urn.Type}.{urn.Id}.-";
        }

        return string.Empty;
    }

    private static string BuildSessionPartOfRoutingKey(int producerId)
    {
        var allProducers = ProducersEndpoint.BuildAll();
        var producer = allProducers.producer.FirstOrDefault(f => f.id == producerId);
        if (producer == null)
        {
            return "missing.missing";
        }

        if (producer.scope.Contains("live", StringComparison.InvariantCultureIgnoreCase))
        {
            return "-.live";
        }

        if (producer.scope.Contains("prematch", StringComparison.InvariantCultureIgnoreCase))
        {
            return "pre.-";
        }

        if (producer.scope.Contains("virtual", StringComparison.InvariantCultureIgnoreCase))
        {
            return "virt.-";
        }

        return "na.na";
    }
}

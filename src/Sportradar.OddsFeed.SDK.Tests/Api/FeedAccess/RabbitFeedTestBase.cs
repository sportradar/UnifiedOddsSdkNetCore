// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EasyNetQ.Management.Client;
using EasyNetQ.Management.Client.Model;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Feed.Messages;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.FeedAccess;

/// <summary>
/// One broker virtual host shared by every test in a class. Each test still opens its own receiver.
/// </summary>
public sealed class RabbitFeedFixture : IAsyncLifetime
{
    private readonly ForwardingTestOutput _output = new ForwardingTestOutput();

    public ProjectConfiguration Project { get; private set; }

    public RabbitManagement Rabbit { get; private set; }

    public void UseOutput(ITestOutputHelper output)
    {
        _output.Current = output;
    }

    public async Task InitializeAsync()
    {
        Project = ProjectConfiguration.CreateWithRandomVirtualHost();
        Rabbit = new RabbitManagement(_output, Project);

        Exception last = null;
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                Rabbit.ResetAndStart();
                return;
            }
            catch (Exception e)
            {
                last = e;
                await Task.Delay(1000).ConfigureAwait(false);
            }
        }

        throw last ?? new InvalidOperationException("RabbitMQ management did not start.");
    }

    public async Task DisposeAsync()
    {
        if (Rabbit == null)
        {
            return;
        }

        await Rabbit.DeleteVirtualHostAsync().ConfigureAwait(false);
        Rabbit.Stop();
    }

    private sealed class ForwardingTestOutput : ITestOutputHelper
    {
        public ITestOutputHelper Current { get; set; }

        public void WriteLine(string message)
        {
            Current?.WriteLine(message);
        }

        public void WriteLine(string format, params object[] args)
        {
            Current?.WriteLine(format, args);
        }
    }
}

/// <summary>
/// Builds the feed from <see cref="ProjectConfiguration"/> and asserts on <see cref="RabbitMqMessageReceiver"/>.
/// </summary>
public abstract class RabbitFeedTestBase
{
    private readonly Deserializer<FeedMessage> _deserializer = new Deserializer<FeedMessage>();
    private readonly RegexRoutingKeyParser _keyParser = new RegexRoutingKeyParser();

    protected RabbitFeedTestBase(RabbitFeedFixture fixture, ITestOutputHelper output)
    {
        Fixture = fixture;
        Output = output;
        fixture.UseOutput(output);
        LoggerFactory = new XunitLoggerFactory(output);
    }

    protected RabbitFeedFixture Fixture { get; }

    protected ITestOutputHelper Output { get; }

    protected XunitLoggerFactory LoggerFactory { get; }

    protected ProjectConfiguration Project => Fixture.Project;

    protected XUnitLogger ChannelLogger => LoggerFactory.GetOrCreateLogger<RabbitMqChannel>().InnerLogger;

    protected XUnitLogger ReceiverLogger => LoggerFactory.GetOrCreateLogger(typeof(RabbitMqMessageReceiver));

    protected XUnitLogger TrafficLogger => LoggerFactory.GetOrCreateLogger(typeof(FeedTraffic));

    internal IUofConfiguration CreateConfiguration(string accessToken = "AnyAccessToken",
                                                    string username = null,
                                                    string password = null,
                                                    string host = null,
                                                    bool withBookmaker = true,
                                                    bool useSsl = false,
                                                    SdkEnvironment? environment = null,
                                                    TimeSpan? connectionTimeout = null)
    {
        var builder = new UofConfigurationBuilder()
                     .WithAccessToken(accessToken)
                     .WithRabbitConfiguration(rabbit => rabbit.WithHost(host ?? Project.GetRabbitIp())
                                                             .WithPort(Project.DefaultRabbitPort)
                                                             .WithUsername(username ?? Project.SdkRabbitUsername)
                                                             .WithPassword(password ?? Project.SdkRabbitPassword)
                                                             .WithVirtualHost(Project.VirtualHostName)
                                                             .WithUseSsl(useSsl)
                                                             .WithConnectionTimeout(connectionTimeout ?? TimeSpan.FromSeconds(ConfigLimit.RabbitConnectionTimeoutDefault))
                                                             .WithHeartbeat(TimeSpan.FromSeconds(ConfigLimit.RabbitHeartbeatDefault))
                                                             .Build());

        if (environment.HasValue)
        {
            builder.WithEnvironment(environment.Value);
        }

        if (withBookmaker)
        {
            var bookmaker = new Mock<IBookmakerDetails>();
            bookmaker.SetupGet(b => b.BookmakerId).Returns(123);
            builder.WithBookmakerDetails(bookmaker.Object);
        }

        return builder.Build();
    }

    internal OpenedFeed CreateFeed(IUofConfiguration config,
                                    TimeSpan? maxSilence = null,
                                    IRoutingKeyParser parser = null,
                                    ConfiguredConnectionFactory connection = null,
                                    bool disposeConnection = true,
                                    bool trackMessages = true,
                                    bool trackFailures = true,
                                    ISdkTimer timer = null)
    {
        var configured = connection ?? new ConfiguredConnectionFactory(ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(config),
                                                                        config,
                                                                        LoggerFactory.GetOrCreateLogger<ConfiguredConnectionFactory>());
        var channelFactory = new ChannelFactory(configured);
        timer ??= new SdkTimer("ConnectionCheckTimer", TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(100));
        var channel = new RabbitMqChannel(channelFactory, timer, maxSilence ?? TimeSpan.FromSeconds(5), config.AccessToken, LoggerFactory.GetOrCreateLogger<RabbitMqChannel>());
        var receiver = new RabbitMqMessageReceiver(channel, _deserializer, parser ?? _keyParser, TestProducerManager.Create(), config, LoggerFactory);
        return new OpenedFeed(receiver, channel, configured, disposeConnection, trackMessages, trackFailures);
    }

    internal async Task<OpenedFeed> OpenFeedAsync(IUofConfiguration config,
                                                   MessageInterest interest,
                                                   bool waitForChannel = true,
                                                   TimeSpan? maxSilence = null,
                                                   IRoutingKeyParser parser = null,
                                                   ConfiguredConnectionFactory connection = null,
                                                   bool disposeConnection = true,
                                                   IEnumerable<string> routingKeys = null,
                                                   bool trackMessages = true,
                                                   bool trackFailures = true)
    {
        var feed = CreateFeed(config, maxSilence, parser, connection, disposeConnection, trackMessages, trackFailures);
        try
        {
            feed.Receiver.Open(interest, routingKeys ?? RoutingKeysFor(interest));
            if (waitForChannel)
            {
                await WaitUntilChannelIsReady(feed.Channel).ConfigureAwait(false);
            }

            return feed;
        }
        catch
        {
            feed.Dispose();
            throw;
        }
    }

    internal static IReadOnlyList<string> RoutingKeysFor(MessageInterest interest)
    {
        if (interest == null)
        {
            return FeedRoutingKeyBuilder.GetStandardKeys().ToList();
        }

        return FeedRoutingKeyBuilder.GenerateKeys([interest]).SelectMany(keys => keys).Distinct().ToList();
    }

    internal odds_change PublishOddsChange(long matchId = 1, int sportId = 1, int product = 1, long? headerTimestamp = null)
    {
        var message = OddsChangeBuilder.Create()
                                       .WithProduct(product)
                                       .WithMatchId(matchId)
                                       .WithTimestamp(1_700_000_000_000)
                                       .AddMarket(market => market.WithMarketId(1).WithOutcome("1", 1.5).WithOutcome("2", 2.5))
                                       .Build();
        var routingKey = FeedMessageBuilder.BuildRoutingKey(message, sportId);
        if (headerTimestamp.HasValue)
        {
            Fixture.Rabbit.Send(message, routingKey, headerTimestamp.Value);
        }
        else
        {
            Fixture.Rabbit.Send(message, routingKey);
        }

        return message;
    }

    internal void Publish(FeedMessage message, string routingKey = null, IDictionary<string, object> headers = null)
    {
        routingKey ??= FeedMessageBuilder.BuildRoutingKey(message);
        var body = Encoding.UTF8.GetBytes(FeedMessageBuilder.BuildMessageBody(message));
        Fixture.Rabbit.SendRaw(body, routingKey, headers);
    }

    internal void PublishRaw(byte[] body, string routingKey, IDictionary<string, object> headers = null)
    {
        Fixture.Rabbit.SendRaw(body, routingKey, headers);
    }

    internal async Task<odds_change> PublishAndAwaitOddsChange(OpenedFeed feed, int expectedCount, long matchId = 1, int sportId = 1, int product = 1, long? headerTimestamp = null)
    {
        var sent = PublishOddsChange(matchId, sportId, product, headerTimestamp);
        AssertSameOdds(await WaitForReceipt(feed, expectedCount).ConfigureAwait(false), sent);
        return sent;
    }

    internal async Task<FeedMessage> WaitForReceipt(OpenedFeed feed, int expectedCount)
    {
        var arrived = await TestExecutionHelper.WaitToCompleteAsync(() => feed.Count >= expectedCount).ConfigureAwait(false);
        arrived.ShouldBeTrue($"Receiver did not get {expectedCount} message(s).");
        return feed.Last();
    }

    internal static void AssertSameOdds(FeedMessage received, odds_change sent)
    {
        received.ShouldBeOfType<odds_change>();
        var oddsChange = (odds_change)received;
        oddsChange.product.ShouldBe(sent.product);
        oddsChange.event_id.ShouldBe(sent.event_id);
    }

    internal static async Task WaitUntilChannelIsReady(RabbitMqChannel channel)
    {
        await TestExecutionHelper.WaitToCompleteAsync(() => channel.Channel != null && channel.ChannelStarted != DateTime.MinValue, 15).ConfigureAwait(false);
        channel.Channel.ShouldNotBeNull("Rabbit channel was not created.");
    }

    internal static async Task WaitForChannelRestart(RabbitMqChannel channel, DateTime previousStart)
    {
        var restarted = await TestExecutionHelper.WaitToCompleteAsync(() => channel.Channel != null && channel.ChannelStarted > previousStart, 50, 10000).ConfigureAwait(false);
        restarted.ShouldBeTrue("Channel was not recreated.");
    }

    internal async Task<string> WaitForConnectionName(string username)
    {
        string name = null;
        var listed = await TestExecutionHelper.WaitToCompleteAsync(() =>
                                                                    {
                                                                        name = ConnectionName(username);
                                                                        return !string.IsNullOrEmpty(name);
                                                                    },
                                                                    200,
                                                                    10000).ConfigureAwait(false);
        listed.ShouldBeTrue($"RabbitMQ management did not list a connection for '{username}'.");
        return name;
    }

    internal string ConnectionName(string username)
    {
        return Fixture.Rabbit.ManagementClient.GetConnections()
                      .FirstOrDefault(connection => string.Equals(connection.User, username, StringComparison.OrdinalIgnoreCase))
                      ?.Name;
    }

    internal int CountConnections(string username)
    {
        return Fixture.Rabbit.ManagementClient.GetConnections()
                      .Count(connection => string.Equals(connection.User, username, StringComparison.OrdinalIgnoreCase));
    }

    internal void CreateRabbitUser(string username, string password)
    {
        var client = Fixture.Rabbit.ManagementClient;
        var existing = client.GetUsers().FirstOrDefault(user => user.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            client.DeleteUser(existing);
        }

        client.CreateUser(UserInfo.ByPassword(username, password).AddTag("administrator"));
        var user = client.GetUser(username);
        var vhost = client.GetVhost(Project.VirtualHostName);
        client.CreatePermission(vhost, new PermissionInfo(user.Name));
    }

    internal static async Task WaitUntilLogged(XUnitLogger logger, string messagePart, int times = 1)
    {
        var logged = await TestExecutionHelper.WaitToCompleteAsync(() => logger.Messages.Count(message => message.Contains(messagePart)) >= times).ConfigureAwait(false);
        logged.ShouldBeTrue($"Expected {times} log(s) containing '{messagePart}'.");
    }

    internal static bool InvokeBasicConsumerShutdownEvent(RabbitMqChannel channel, string replyText)
    {
        var field = typeof(EventingBasicConsumer).GetField("Shutdown", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == null)
        {
            return false;
        }

        var handler = (EventHandler<ShutdownEventArgs>)field.GetValue(channel.BasicConsumer);
        handler?.Invoke(channel.BasicConsumer, new ShutdownEventArgs(ShutdownInitiator.Application, 1, replyText));
        return true;
    }

    internal static void InvokeChannelShutdownEvent(RabbitMqChannel channel, string replyText)
    {
        var field = channel.Channel.GetType().GetField("_recordedShutdownEventHandlers", BindingFlags.Instance | BindingFlags.NonPublic);
        if (field?.GetValue(channel.Channel) is EventHandler<ShutdownEventArgs> handler)
        {
            handler.Invoke(channel.Channel, new ShutdownEventArgs(ShutdownInitiator.Application, 1, replyText));
        }
    }

    internal sealed class OpenedFeed : IDisposable
    {
        private readonly bool _disposeConnection;
        private readonly List<FeedMessage> _received = new List<FeedMessage>();
        private readonly List<byte[]> _failedPayloads = new List<byte[]>();
        private readonly object _gate = new object();

        public OpenedFeed(RabbitMqMessageReceiver receiver, RabbitMqChannel channel, ConfiguredConnectionFactory connection, bool disposeConnection, bool trackMessages, bool trackFailures)
        {
            Receiver = receiver;
            Channel = channel;
            Connection = connection;
            _disposeConnection = disposeConnection;
            if (trackMessages)
            {
                Receiver.FeedMessageReceived += (_, args) =>
                                                {
                                                    lock (_gate)
                                                    {
                                                        _received.Add(args.Message);
                                                    }
                                                };
            }

            if (trackFailures)
            {
                Receiver.FeedMessageDeserializationFailed += (_, args) =>
                                                             {
                                                                 lock (_gate)
                                                                 {
                                                                     _failedPayloads.Add(args.RawData as byte[] ?? args.RawData.ToArray());
                                                                 }
                                                             };
            }
        }

        public RabbitMqMessageReceiver Receiver { get; }

        public RabbitMqChannel Channel { get; }

        public ConfiguredConnectionFactory Connection { get; }

        public int Count
        {
            get
            {
                lock (_gate)
                {
                    return _received.Count;
                }
            }
        }

        public int FailedCount
        {
            get
            {
                lock (_gate)
                {
                    return _failedPayloads.Count;
                }
            }
        }

        public FeedMessage Last()
        {
            lock (_gate)
            {
                return _received[^1];
            }
        }

        public byte[] LastFailedPayload()
        {
            lock (_gate)
            {
                return _failedPayloads[^1];
            }
        }

        public bool ReceivedEvent(string eventId)
        {
            lock (_gate)
            {
                return _received.Any(message => message.EventId == eventId);
            }
        }

        public void Dispose()
        {
            try
            {
                if (Receiver.IsChannelOpened)
                {
                    Receiver.Close();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e);
            }

            if (_disposeConnection)
            {
                Connection.Dispose();
            }
        }
    }
}

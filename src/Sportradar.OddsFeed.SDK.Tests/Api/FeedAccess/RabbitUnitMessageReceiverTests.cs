// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Feed.Messages;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Api.FeedAccess;

/// <summary>
/// Receiver gates the broker cannot produce: a delivery before consume, a null routing key, or headers flagged present while null.
/// </summary>
public class RabbitUnitMessageReceiverTests
{
    private readonly Mock<IRabbitMqChannel> _channel = new Mock<IRabbitMqChannel>();
    private readonly RabbitMqMessageReceiver _receiver;

    public RabbitUnitMessageReceiverTests()
    {
        _receiver = new RabbitMqMessageReceiver(_channel.Object,
                                                new Deserializer<FeedMessage>(),
                                                new RegexRoutingKeyParser(),
                                                TestProducerManager.Create(),
                                                TestConfiguration.GetConfig(),
                                                new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory());
    }

    [Fact]
    public void EventsAreIgnoredBeforeTheReceiverIsOpened()
    {
        var received = false;
        var failed = false;
        _receiver.FeedMessageReceived += (_, _) => received = true;
        _receiver.FeedMessageDeserializationFailed += (_, _) => failed = true;

        Deliver(OddsBody(), routingKey: null);
        Deliver(new byte[] { 1 }, routingKey: null);

        received.ShouldBeFalse();
        failed.ShouldBeFalse();
    }

    [Fact]
    public void EventsAreIgnoredAfterTheReceiverIsClosed()
    {
        _receiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());
        _receiver.Close();
        var received = false;
        var failed = false;
        _receiver.FeedMessageReceived += (_, _) => received = true;
        _receiver.FeedMessageDeserializationFailed += (_, _) => failed = true;

        Deliver(OddsBody(), routingKey: null);
        Deliver(new byte[] { 1 }, routingKey: null);

        received.ShouldBeFalse();
        failed.ShouldBeFalse();
    }

    [Fact]
    public void NullRoutingKeyStillDeliversTheMessageWithoutSportId()
    {
        FeedMessage message = null;
        _receiver.FeedMessageReceived += (_, args) => message = args.Message;
        _receiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        Deliver(OddsBody(), routingKey: null);

        message.ShouldNotBeNull();
        message.SportId.ShouldBeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MissingOrEmptyBodyDoesNotRaiseDeserializationFailed(bool missingBody)
    {
        var failed = false;
        _receiver.FeedMessageDeserializationFailed += (_, _) => failed = true;
        _receiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        Deliver(missingBody ? null : Array.Empty<byte>(), routingKey: null);

        failed.ShouldBeFalse();
    }

    [Fact]
    public void HeadersPresentButNullDoNotThrowAndSentAtFallsBack()
    {
        FeedMessage message = null;
        var oddsChange = OddsChange();
        var properties = new Mock<IBasicProperties>();
        properties.Setup(property => property.IsHeadersPresent()).Returns(true);
        properties.Setup(property => property.Headers).Returns((IDictionary<string, object>)null);
        _receiver.FeedMessageReceived += (_, args) => message = args.Message;
        _receiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        Deliver(Encoding.UTF8.GetBytes(FeedMessageBuilder.BuildMessageBody(oddsChange)), "hi.-.live.odds_change.1.sr:match.1.-", properties.Object);

        message.ShouldNotBeNull();
        message.GeneratedAt.ShouldBe(oddsChange.GeneratedAt);
        message.SentAt.ShouldBe(oddsChange.GeneratedAt + 1);
    }

    private void Deliver(byte[] body, string routingKey, IBasicProperties properties = null)
    {
        _channel.Raise(channel => channel.Received += null, new BasicDeliverEventArgs("", 1, false, ",", routingKey, properties, body));
    }

    private static byte[] OddsBody()
    {
        return Encoding.UTF8.GetBytes(FeedMessageBuilder.BuildMessageBody(OddsChange()));
    }

    private static odds_change OddsChange()
    {
        return OddsChangeBuilder.Create()
                                .WithProduct(1)
                                .WithMatchId(1)
                                .WithTimestamp(1_700_000_000_000)
                                .AddMarket(market => market.WithMarketId(1).WithOutcome("1", 1.5).WithOutcome("2", 2.5))
                                .Build();
    }
}

// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Api.FeedAccess;

public class RabbitMqMessageReceiverTests
{
    private readonly IMessageReceiver _messageReceiver;
    private readonly Mock<IRabbitMqChannel> _mock;

    public RabbitMqMessageReceiverTests()
    {
        var deserializer = new Deserializer<FeedMessage>();
        var keyParser = new RegexRoutingKeyParser();
        _mock = new Mock<IRabbitMqChannel>();

        _messageReceiver = new RabbitMqMessageReceiver(_mock.Object, deserializer, keyParser, TestProducerManager.Create(), TestConfiguration.GetConfig());
    }

    private static byte[] GetFileContent(string fileName)
    {
        var stream = FileHelper.OpenFile(TestData.FeedXmlPath, fileName);
        using (var ms = new MemoryStream())
        {
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }

    [Fact]
    public void NullOrEmptyDataDoesNotRaiseDeserializationFailedEvent()
    {
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        var deserializationFailed = false;

        _messageReceiver.FeedMessageDeserializationFailed += (sender, args) => { deserializationFailed = true; };
        _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, null));
        _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, Array.Empty<byte>()));

        Assert.False(deserializationFailed, "deserializationFailed should be false");
    }

    [Fact]
    public void EventsAreNotRaisedBeforeTheReceiverIsOpened()
    {
        var messageReceived = false;
        var deserializationFailed = false;

        var messageData = GetFileContent("odds_change.xml");
        _messageReceiver.FeedMessageReceived += (sender, args) => { messageReceived = true; };
        _messageReceiver.FeedMessageDeserializationFailed += (sender, args) => { deserializationFailed = false; };

        _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, messageData));
        _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, new[] { (byte)1 }));

        Assert.False(messageReceived, "messageReceived should be false");
        Assert.False(deserializationFailed, "deserializationFailed should be false");
    }

    [Fact]
    public void EventsAreNotRaisedAfterTheReceiverIsClosed()
    {
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());
        _messageReceiver.Close();
        EventsAreNotRaisedBeforeTheReceiverIsOpened();
    }

    [Fact]
    public void EventsAreRaisedWhenTheReceiverIsOpened()
    {
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        var messageReceived = false;
        var deserializationFailed = false;

        var messageData = GetFileContent("odds_change.xml");
        _messageReceiver.FeedMessageReceived += (sender, args) => { messageReceived = true; };
        _messageReceiver.FeedMessageDeserializationFailed += (sender, args) => { deserializationFailed = true; };

        _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, messageData));
        _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, new[] { (byte)1 }));

        Assert.True(messageReceived, "messageReceived should be true");
        Assert.True(deserializationFailed, "deserializationFailed should be true");
    }

    [Fact]
    public void EventsAreRaisedAfterReceiverIsReOpened()
    {
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());
        _messageReceiver.Close();
        EventsAreRaisedWhenTheReceiverIsOpened();
    }

    [Fact]
    public void MessageReceivedEventIsRaisedForOddsChange()
    {
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());
        const string routingKey = "hi.-.live.odds_change.2.sr:match.9900415";
        FeedMessage message = null;

        _messageReceiver.FeedMessageReceived += (sender, args) => { message = args.Message; };
        _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", routingKey, null, GetFileContent("odds_change.xml")));

        Assert.NotNull(message);
        var expectedSportId = Urn.Parse("sr:sport:2");
        Assert.Equal(expectedSportId, message.SportId);
    }

    [Fact]
    public void MessageReceivedEventIsRaisedForNullRoutingKey()
    {
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());
        var eventRaised = false;

        _messageReceiver.FeedMessageReceived += (sender, args) => { eventRaised = true; };
        _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, GetFileContent("odds_change.xml")));

        Assert.True(eventRaised, "eventRaised flag should be set to true");
    }

    [Fact]
    public void MessageReceivedEventIsRaisedForUnparsableRoutingKey()
    {
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());
        var eventRaised = false;

        _messageReceiver.FeedMessageReceived += (sender, args) => { eventRaised = true; };
        _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", "routing_key", null, GetFileContent("odds_change.xml")));

        Assert.True(eventRaised, "eventRaised flag should be set to true");
    }

    [Fact]
    public void CorrectDataIsPassedToDeserializationFailedEvent()
    {
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        byte[] data = null;
        var messageData = new byte[] { 1, 2, 3, 4, 100, 99 };

        _messageReceiver.FeedMessageDeserializationFailed += (sender, args) => { data = args.RawData.ToArray(); };
        _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, messageData));

        var areEqual = messageData.SequenceEqual(data);
        Assert.True(areEqual, "both arrays should be equal");
    }

    [Fact]
    public void FeedMessageGetsTimestampSentAtFromHeaderTimestamp()
    {
        FeedMessage receivedMessage = null;
        var sentTimestamp = DateTime.Now.Ticks;
        var messageData = GetFileContent("odds_change.xml");
        var basicProperties = GetBasicProperties(new Dictionary<string, object> { { "timestamp_in_ms", sentTimestamp } });
        _messageReceiver.FeedMessageReceived += (_, args) => receivedMessage = args.Message;

        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, basicProperties, messageData));

        Assert.Equal(1487254396715, receivedMessage.GeneratedAt);
        Assert.Equal(sentTimestamp, receivedMessage.SentAt);
    }

    [Fact]
    public void FeedMessageWithWrongHeaderKeySetsTimestampSentAtToGeneratedPlus1()
    {
        FeedMessage receivedMessage = null;
        var sentTimestamp = DateTime.Now.Ticks;
        var messageData = GetFileContent("odds_change.xml");
        var basicProperties = GetBasicProperties(new Dictionary<string, object> { { "time_in_ms", sentTimestamp } });
        _messageReceiver.FeedMessageReceived += (_, args) => receivedMessage = args.Message;

        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, basicProperties, messageData));

        Assert.NotNull(receivedMessage);
        Assert.Equal(receivedMessage.GeneratedAt + 1, receivedMessage.SentAt);
    }

    [Fact]
    public void FeedMessageWithWrongHeaderValueGetsTimestampSentAtFromGeneratedAtPlus1()
    {
        FeedMessage receivedMessage = null;
        var sentTimestamp = "some-sent-timestamp";
        var messageData = GetFileContent("odds_change.xml");
        var basicProperties = GetBasicProperties(new Dictionary<string, object> { { "timestamp_in_ms", sentTimestamp } });
        _messageReceiver.FeedMessageReceived += (_, args) => receivedMessage = args.Message;

        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        _mock.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, basicProperties, messageData));

        Assert.NotNull(receivedMessage);
        Assert.Equal(receivedMessage.GeneratedAt + 1, receivedMessage.SentAt);
    }

    private IBasicProperties GetBasicProperties(IDictionary<string, object> headers = null)
    {
        var basicPropertiesMock = new Mock<IBasicProperties>();

        if (!headers.IsNullOrEmpty())
        {
            basicPropertiesMock.Setup(s => s.IsHeadersPresent()).Returns(true);
            basicPropertiesMock.Setup(s => s.Headers).Returns(headers);
        }

        return basicPropertiesMock.Object;
    }
}

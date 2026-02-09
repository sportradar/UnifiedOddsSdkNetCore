// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.FeedAccess;

public class RabbitMqMessageReceiverTests
{
    private const int UnknownProducerId = 9999;
    private readonly IMessageReceiver _messageReceiver;
    private readonly Mock<IRabbitMqChannel> _mockRabbitChannel;
    private readonly XunitLoggerFactory _loggerFactory;

    public RabbitMqMessageReceiverTests(ITestOutputHelper testOutputHelper)
    {
        var deserializer = new Deserializer<FeedMessage>();
        var keyParser = new RegexRoutingKeyParser();
        _mockRabbitChannel = new Mock<IRabbitMqChannel>();
        _loggerFactory = new XunitLoggerFactory(testOutputHelper);

        _messageReceiver = new RabbitMqMessageReceiver(_mockRabbitChannel.Object, deserializer, keyParser, TestProducerManager.Create(), TestConfiguration.GetConfig(), _loggerFactory);
    }

    [Fact]
    public void WhenReceiverIsOpenedAndChannelIsOpenedThenItIndicatesViaOpenedProperty()
    {
        var isOpened = false;
        _mockRabbitChannel.Setup(x => x.Open(It.IsAny<MessageInterest>(), It.IsAny<IEnumerable<string>>()))
            .Callback(() => isOpened = true);
        _mockRabbitChannel.Setup(x => x.IsOpened).Returns(() => isOpened);

        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        _messageReceiver.IsChannelOpened.ShouldBeTrue();
    }

    [Fact]
    public void NullBodyDataDoesNotRaiseDeserializationFailedEvent()
    {
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());
        var deserializationFailed = false;
        _messageReceiver.FeedMessageDeserializationFailed += (_, _) => { deserializationFailed = true; };

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetBasicDeliverEventArgsWithBodyAndNullRoutingKey(null, null));

        deserializationFailed.ShouldBeFalse("deserializationFailed should be false");
    }

    [Fact]
    public void EmptyBodyDataDoesNotRaiseDeserializationFailedEvent()
    {
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());
        var deserializationFailed = false;
        _messageReceiver.FeedMessageDeserializationFailed += (_, _) => { deserializationFailed = true; };

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetBasicDeliverEventArgsWithBodyAndNullRoutingKey(null, Array.Empty<byte>()));

        deserializationFailed.ShouldBeFalse("deserializationFailed should be false");
    }

    [Fact]
    public void ReceivedEventCanNotBeRaisedBeforeTheReceiverIsOpened()
    {
        var messageReceived = false;
        var messageData = GetFeedMessageBodyForOddsChange();
        _messageReceiver.FeedMessageReceived += (_, _) => { messageReceived = true; };

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetBasicDeliverEventArgsWithBodyAndNullRoutingKey(null, messageData));

        messageReceived.ShouldBeFalse("messageReceived should be false");
    }

    [Fact]
    public void DeserializationFailedEventCanNotBeRaisedBeforeTheReceiverIsOpened()
    {
        var deserializationFailed = false;
        _messageReceiver.FeedMessageDeserializationFailed += (_, _) => { deserializationFailed = false; };

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetBasicDeliverEventArgsWithBodyAndNullRoutingKey(null, new[] { (byte)1 }));

        deserializationFailed.ShouldBeFalse("deserializationFailed should be false");
    }

    [Fact]
    public void EventsAreNotRaisedAfterTheReceiverIsClosed()
    {
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());
        _messageReceiver.Close();
        ReceivedEventCanNotBeRaisedBeforeTheReceiverIsOpened();
        DeserializationFailedEventCanNotBeRaisedBeforeTheReceiverIsOpened();
    }

    [Fact]
    public void ReceivedEventCanBeRaisedWhenTheReceiverIsOpened()
    {
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        var messageReceived = false;
        var messageData = GetFeedMessageBodyForOddsChange();
        _messageReceiver.FeedMessageReceived += (_, _) => { messageReceived = true; };

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetBasicDeliverEventArgsWithBodyAndNullRoutingKey(null, messageData));

        messageReceived.ShouldBeTrue("messageReceived should be true");
    }

    [Fact]
    public void DeserializationEventCanBeRaisedWhenTheReceiverIsOpened()
    {
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        var deserializationFailed = false;
        _messageReceiver.FeedMessageDeserializationFailed += (_, _) => { deserializationFailed = true; };

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetBasicDeliverEventArgsWithBodyAndNullRoutingKey(null, new[] { (byte)1 }));

        deserializationFailed.ShouldBeTrue("deserializationFailed should be true");
    }

    [Fact]
    public void EventsAreRaisedAfterReceiverIsReOpened()
    {
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());
        _messageReceiver.Close();
        ReceivedEventCanBeRaisedWhenTheReceiverIsOpened();
    }

    [Fact]
    public void MessageSportIdIsExtractedFromRoutingKey()
    {
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());
        const string routingKey = "hi.-.live.odds_change.2.sr:match.9900415";
        var expectedSportId = Urn.Parse("sr:sport:2");
        FeedMessage message = null;

        _messageReceiver.FeedMessageReceived += (_, args) => { message = args.Message; };
        _mockRabbitChannel.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", routingKey, null, GetFeedMessageBodyForOddsChange()));

        message.ShouldNotBeNull();
        message.SportId.ShouldBe(expectedSportId);
    }

    [Fact]
    public void WhenNullRoutingKeyThenMessageIsStillReceivedAndSportIdIsNull()
    {
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());
        FeedMessage message = null;

        _messageReceiver.FeedMessageReceived += (_, args) => { message = args.Message; };
        _mockRabbitChannel.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", null, null, GetFeedMessageBodyForOddsChange()));

        message.ShouldNotBeNull();
        message.SportId.ShouldBeNull();
    }

    [Fact]
    public void MessageReceivedEventIsRaisedForUnparsableRoutingKey()
    {
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());
        var eventRaised = false;

        _messageReceiver.FeedMessageReceived += (_, _) => { eventRaised = true; };
        _mockRabbitChannel.Raise(mock => mock.Received += null, GetBasicDeliverEventArgsWithBodyAndNullRoutingKey(null, GetFeedMessageBodyForOddsChange()));

        eventRaised.ShouldBeTrue("eventRaised flag should be set to true");
    }

    [Fact]
    public void CorrectDataIsPassedToDeserializationFailedEvent()
    {
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        byte[] data = null;
        var messageData = new byte[] { 1, 2, 3, 4, 100, 99 };

        _messageReceiver.FeedMessageDeserializationFailed += (_, args) => { data = args.RawData.ToArray(); };
        _mockRabbitChannel.Raise(mock => mock.Received += null, GetBasicDeliverEventArgsWithBodyAndNullRoutingKey(null, messageData));

        data.ShouldBe(messageData);
    }

    [Fact]
    public void FeedMessageGetsTimestampSentAtFromHeaderTimestamp()
    {
        FeedMessage receivedMessage = null;
        var sentTimestamp = DateTime.Now.Ticks;
        var oddsChange = GetOddsChangeWithSingleMarket();
        var xmlBody = FeedMessageBuilder.BuildMessageBody(oddsChange);
        var messageData = Encoding.UTF8.GetBytes(xmlBody);
        var basicProperties = GetBasicPropertiesWithHeader(new Dictionary<string, object> { { "timestamp_in_ms", sentTimestamp } });
        _messageReceiver.FeedMessageReceived += (_, args) => receivedMessage = args.Message;

        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetBasicDeliverEventArgsWithBodyAndNullRoutingKey(basicProperties, messageData));

        receivedMessage.GeneratedAt.ShouldBe(oddsChange.GeneratedAt);
        receivedMessage.SentAt.ShouldBe(sentTimestamp);
    }

    [Fact]
    public void FeedMessageWithWrongHeaderKeySetsTimestampSentAtToGeneratedPlus1()
    {
        FeedMessage receivedMessage = null;
        var sentTimestamp = DateTime.Now.Ticks;
        var messageData = GetFeedMessageBodyForOddsChange();
        var basicProperties = GetBasicPropertiesWithHeader(new Dictionary<string, object> { { "time_in_ms", sentTimestamp } });
        _messageReceiver.FeedMessageReceived += (_, args) => receivedMessage = args.Message;

        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetBasicDeliverEventArgsWithBodyAndNullRoutingKey(basicProperties, messageData));

        receivedMessage.ShouldNotBeNull();
        receivedMessage.SentAt.ShouldBe(receivedMessage.GeneratedAt + 1);
    }

    [Fact]
    public void FeedMessageWithWrongHeaderValueGetsTimestampSentAtFromGeneratedAtPlus1()
    {
        FeedMessage receivedMessage = null;
        var sentTimestamp = "some-sent-timestamp";
        var messageData = GetFeedMessageBodyForOddsChange();
        var basicProperties = GetBasicPropertiesWithHeader(new Dictionary<string, object> { { "timestamp_in_ms", sentTimestamp } });
        _messageReceiver.FeedMessageReceived += (_, args) => receivedMessage = args.Message;

        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetBasicDeliverEventArgsWithBodyAndNullRoutingKey(basicProperties, messageData));

        receivedMessage.ShouldNotBeNull();
        receivedMessage.SentAt.ShouldBe(receivedMessage.GeneratedAt + 1);
    }

    [Fact]
    public void WhenReceiverIsOpenedWithoutMessageInterestThenIsSystem()
    {
        FeedMessage receivedMessage = null;
        var aliveMessage = FeedMessageBuilder.Create(1).BuildAlive(1);
        _messageReceiver.FeedMessageReceived += (_, args) => receivedMessage = args.Message;

        _messageReceiver.Open(null, FeedRoutingKeyBuilder.GetStandardKeys());

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetValidBasicDeliverEventArgsForFeedMessage(aliveMessage));

        receivedMessage.ShouldNotBeNull();
        var execLogger = _loggerFactory.GetOrCreateLogger(typeof(FeedTraffic));
        execLogger.Messages.ShouldBeOfSize(1);
        execLogger.Messages.Count(m => m.Contains("<~> system <~>")).ShouldBe(1);
    }

    [Fact]
    public void WhenBetStopProcessingIsLongThenIsLogged()
    {
        FeedMessage receivedMessage = null;
        var betStop = FeedMessageBuilder.Create(1).BuildBetStop(1);
        var messageReceiver = OpenMessageReceiverWithLongProcessing();
        messageReceiver.FeedMessageReceived += (_, args) => receivedMessage = args.Message;

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetValidBasicDeliverEventArgsForFeedMessage(betStop));

        receivedMessage.ShouldNotBeNull();
        var execLogger = _loggerFactory.GetOrCreateLogger(typeof(RabbitMqMessageReceiver));
        execLogger.Messages.ShouldBeOfSize(2);
        execLogger.Messages.Count(m => m.Contains("Deserialization of")).ShouldBe(1);
        execLogger.Messages.Count(m => m.Contains("Markets=0")).ShouldBe(1);
        execLogger.Messages.Count(m => m.Contains("Outcomes=0")).ShouldBe(1);
    }

    [Fact]
    public void WhenOddsChangeProcessingIsLongThenIsLoggedWithMarkets()
    {
        FeedMessage receivedMessage = null;
        var oddsChange = GetOddsChangeWithSingleMarket();
        var messageReceiver = OpenMessageReceiverWithLongProcessing();
        messageReceiver.FeedMessageReceived += (_, args) => receivedMessage = args.Message;

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetValidBasicDeliverEventArgsForFeedMessage(oddsChange));

        receivedMessage.ShouldNotBeNull();
        var execLogger = _loggerFactory.GetOrCreateLogger(typeof(RabbitMqMessageReceiver));
        execLogger.Messages.ShouldBeOfSize(2);
        execLogger.Messages.Count(m => m.Contains("Deserialization of")).ShouldBe(1);
        execLogger.Messages.Count(m => m.Contains("Markets=1")).ShouldBe(1);
        execLogger.Messages.Count(m => m.Contains("Outcomes=2")).ShouldBe(1);
    }

    [Fact]
    public void WhenOddsChangeProcessingIsLongAndHasNoMarketsThenIsLoggedWithMarkets()
    {
        FeedMessage receivedMessage = null;
        var oddsChange = GetOddsChangeWithSingleMarket();
        oddsChange.odds.market = null;
        var messageReceiver = OpenMessageReceiverWithLongProcessing();
        messageReceiver.FeedMessageReceived += (_, args) => receivedMessage = args.Message;

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetValidBasicDeliverEventArgsForFeedMessage(oddsChange));

        receivedMessage.ShouldNotBeNull();
        var execLogger = _loggerFactory.GetOrCreateLogger(typeof(RabbitMqMessageReceiver));
        execLogger.Messages.ShouldBeOfSize(2);
        execLogger.Messages.Count(m => m.Contains("Deserialization of")).ShouldBe(1);
        execLogger.Messages.Count(m => m.Contains("Markets=0")).ShouldBe(1);
        execLogger.Messages.Count(m => m.Contains("Outcomes=0")).ShouldBe(1);
    }

    [Fact]
    public void WhenOddsChangeProcessingIsLongAndHasNoOddsThenIsLoggedWithMarkets()
    {
        FeedMessage receivedMessage = null;
        var oddsChange = GetOddsChangeWithSingleMarket();
        oddsChange.odds = null;
        var messageReceiver = OpenMessageReceiverWithLongProcessing();
        messageReceiver.FeedMessageReceived += (_, args) => receivedMessage = args.Message;

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetValidBasicDeliverEventArgsForFeedMessage(oddsChange));

        receivedMessage.ShouldNotBeNull();
        var execLogger = _loggerFactory.GetOrCreateLogger(typeof(RabbitMqMessageReceiver));
        execLogger.Messages.ShouldBeOfSize(2);
        execLogger.Messages.Count(m => m.Contains("Deserialization of")).ShouldBe(1);
        execLogger.Messages.Count(m => m.Contains("Markets=0")).ShouldBe(1);
        execLogger.Messages.Count(m => m.Contains("Outcomes=0")).ShouldBe(1);
    }

    [Fact]
    public void WhenBetSettlementProcessingIsLongThenIsLoggedWithMarkets()
    {
        FeedMessage receivedMessage = null;
        var betSettlement = BetSettlementBuilder.CreateForSportEvent("sr:match:12345")
                                                .WithSportId(UrnCreate.SportId(1))
                                                .WithProducerId(1)
                                                .WithTimestamp(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                                                .AddMarket(1, "type=correct_score")
                                                .AddOutcome(1, 1)
                                                .AddOutcome(2, 2)
                                                .AddOutcome(3, 5)
                                                .Build();
        var messageReceiver = OpenMessageReceiverWithLongProcessing();
        messageReceiver.FeedMessageReceived += (_, args) => receivedMessage = args.Message;

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetValidBasicDeliverEventArgsForFeedMessage(betSettlement));

        receivedMessage.ShouldNotBeNull();
        var execLogger = _loggerFactory.GetOrCreateLogger(typeof(RabbitMqMessageReceiver));
        execLogger.Messages.ShouldBeOfSize(2);
        execLogger.Messages.Count(m => m.Contains("Deserialization of")).ShouldBe(1);
        execLogger.Messages.Count(m => m.Contains("Markets=1")).ShouldBe(1);
        execLogger.Messages.Count(m => m.Contains("Outcomes=3")).ShouldBe(1);
    }

    [Fact]
    public void WhenBetSettlementProcessingIsLongAndHasNoMarketsThenIsLoggedWithMarkets()
    {
        FeedMessage receivedMessage = null;
        var betSettlement = BetSettlementBuilder.CreateForSportEvent("sr:match:12345")
                                                .WithSportId(UrnCreate.SportId(1))
                                                .WithProducerId(1)
                                                .WithTimestamp(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                                                .Build();
        var messageReceiver = OpenMessageReceiverWithLongProcessing();
        messageReceiver.FeedMessageReceived += (_, args) => receivedMessage = args.Message;

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetValidBasicDeliverEventArgsForFeedMessage(betSettlement));

        receivedMessage.ShouldNotBeNull();
        var execLogger = _loggerFactory.GetOrCreateLogger(typeof(RabbitMqMessageReceiver));
        execLogger.Messages.ShouldBeOfSize(2);
        execLogger.Messages.Count(m => m.Contains("Deserialization of")).ShouldBe(1);
        execLogger.Messages.Count(m => m.Contains("Markets=0")).ShouldBe(1);
        execLogger.Messages.Count(m => m.Contains("Outcomes=0")).ShouldBe(1);
    }

    [Fact]
    public void WhenBetSettlementProcessingIsLongAndHasNoOutcomesThenIsLoggedWithMarkets()
    {
        FeedMessage receivedMessage = null;
        var betSettlement = BetSettlementBuilder.CreateForSportEvent("sr:match:12345")
                                                .WithSportId(UrnCreate.SportId(1))
                                                .WithProducerId(1)
                                                .WithTimestamp(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                                                .Build();
        betSettlement.outcomes = null;
        var messageReceiver = OpenMessageReceiverWithLongProcessing();
        messageReceiver.FeedMessageReceived += (_, args) => receivedMessage = args.Message;

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetValidBasicDeliverEventArgsForFeedMessage(betSettlement));

        receivedMessage.ShouldNotBeNull();
        var execLogger = _loggerFactory.GetOrCreateLogger(typeof(RabbitMqMessageReceiver));
        execLogger.Messages.ShouldBeOfSize(2);
        execLogger.Messages.Count(m => m.Contains("Deserialization of")).ShouldBe(1);
        execLogger.Messages.Count(m => m.Contains("Markets=0")).ShouldBe(1);
        execLogger.Messages.Count(m => m.Contains("Outcomes=0")).ShouldBe(1);
    }

    [Fact]
    public void WhenOddsChangeComeForUnavailableProducerThenIsLoggedAndNotDispatched()
    {
        FeedMessage receivedMessage = null;
        var oddsChange = GetOddsChangeWithSingleMarket();
        oddsChange.product = TestProducerManager.Create().Producers.First(f => !f.IsAvailable).Id;
        _messageReceiver.FeedMessageReceived += (_, args) => receivedMessage = args.Message;
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetValidBasicDeliverEventArgsForFeedMessage(oddsChange));

        receivedMessage.ShouldBeNull();
        var execLogger = _loggerFactory.GetOrCreateLogger(typeof(RabbitMqMessageReceiver));
        execLogger.Messages.ShouldBeOfSize(1);
        execLogger.Messages.Count(m => m.Contains("A message for producer which is disabled was received.")).ShouldBe(1);
    }

    [Fact]
    public void WhenOddsChangeComeForUnknownProducerThenIsLoggedAndNotDispatched()
    {
        FeedMessage receivedMessage = null;
        var oddsChange = GetOddsChangeWithSingleMarket();
        oddsChange.product = UnknownProducerId;
        _messageReceiver.FeedMessageReceived += (_, args) => receivedMessage = args.Message;
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetValidBasicDeliverEventArgsForFeedMessage(oddsChange));

        receivedMessage.ShouldBeNull();
        var execLogger = _loggerFactory.GetOrCreateLogger(typeof(RabbitMqMessageReceiver));
        execLogger.Messages.ShouldBeOfSize(1);
        execLogger.Messages.Count(m => m.Contains("A message for producer which is not defined was received.")).ShouldBe(1);
    }

    [Fact]
    public void WhenOddsChangeComeForUnavailableProducerAndEnvironmentIsReplayThenIsDispatched()
    {
        var mockConfig = new Mock<IUofConfiguration>();
        mockConfig.Setup(c => c.Environment).Returns(SdkEnvironment.Replay);
        FeedMessage receivedMessage = null;
        var oddsChange = GetOddsChangeWithSingleMarket();
        oddsChange.product = TestProducerManager.Create().Producers.First(f => !f.IsAvailable).Id;

        var messageReceiver = new RabbitMqMessageReceiver(_mockRabbitChannel.Object, new Deserializer<FeedMessage>(), new RegexRoutingKeyParser(), TestProducerManager.Create(), mockConfig.Object, _loggerFactory);
        messageReceiver.FeedMessageReceived += (_, args) => receivedMessage = args.Message;

        messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetValidBasicDeliverEventArgsForFeedMessage(oddsChange));

        receivedMessage.ShouldNotBeNull();
        var execLogger = _loggerFactory.GetOrCreateLogger(typeof(RabbitMqMessageReceiver));
        execLogger.Messages.ShouldBeOfSize(1);
    }

    [Fact]
    public void WhenOddsChangeComeForUnknownProducerIsReceivedAndEnvironmentIsReplayThenIsNotDispatched()
    {
        var mockConfig = new Mock<IUofConfiguration>();
        mockConfig.Setup(c => c.Environment).Returns(SdkEnvironment.Replay);
        FeedMessage receivedMessage = null;
        var oddsChange = GetOddsChangeWithSingleMarket();
        oddsChange.product = UnknownProducerId;

        var messageReceiver = new RabbitMqMessageReceiver(_mockRabbitChannel.Object, new Deserializer<FeedMessage>(), new RegexRoutingKeyParser(), TestProducerManager.Create(), mockConfig.Object, _loggerFactory);
        messageReceiver.FeedMessageReceived += (_, args) => receivedMessage = args.Message;

        messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetValidBasicDeliverEventArgsForFeedMessage(oddsChange));

        receivedMessage.ShouldBeNull();
        var execLogger = _loggerFactory.GetOrCreateLogger(typeof(RabbitMqMessageReceiver));
        execLogger.Messages.ShouldBeOfSize(1);
        execLogger.Messages.Count(m => m.Contains("A message for producer which is not defined was received.")).ShouldBe(1);
    }

    [Fact]
    public void WhenNonDeserializableExceptionHappensThenErrorIsLoggedAndDeserializationFailedEventIsInvoked()
    {
        FeedMessage receivedMessage = null;
        var deserializationFailed = false;
        var oddsChange = GetOddsChangeWithSingleMarket();
        var mockRoutingKeyParser = new Mock<IRoutingKeyParser>();
        var sportId = UrnCreate.SportId(1);
        mockRoutingKeyParser.Setup(s => s.TryGetSportId(It.IsAny<string>(), It.IsAny<string>(), out sportId))
                            .Throws<InvalidOperationException>();

        var messageReceiver = new RabbitMqMessageReceiver(_mockRabbitChannel.Object, new Deserializer<FeedMessage>(), mockRoutingKeyParser.Object, TestProducerManager.Create(), TestConfiguration.GetConfig(), _loggerFactory);
        messageReceiver.FeedMessageReceived += (_, args) => receivedMessage = args.Message;
        messageReceiver.FeedMessageDeserializationFailed += (_, _) => deserializationFailed = true;

        messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetValidBasicDeliverEventArgsForFeedMessage(oddsChange));

        receivedMessage.ShouldBeNull();
        deserializationFailed.ShouldBeTrue();
        var execLogger = _loggerFactory.GetOrCreateLogger(typeof(RabbitMqMessageReceiver));
        execLogger.Messages.ShouldBeOfSize(1);
        execLogger.Messages.Count(m => m.Contains("Error consuming feed message.")).ShouldBe(1);
    }

    [Fact]
    public void WhenNonDeserializableExceptionHappensAndDeserializationEventIsNotHandledThenErrorIsLoggedAndDeserializationFailedEventIsNotInvoked()
    {
        FeedMessage receivedMessage = null;
        var oddsChange = GetOddsChangeWithSingleMarket();
        var mockRoutingKeyParser = new Mock<IRoutingKeyParser>();
        var sportId = UrnCreate.SportId(1);
        mockRoutingKeyParser.Setup(s => s.TryGetSportId(It.IsAny<string>(), It.IsAny<string>(), out sportId))
                            .Throws<InvalidOperationException>();

        var messageReceiver = new RabbitMqMessageReceiver(_mockRabbitChannel.Object, new Deserializer<FeedMessage>(), mockRoutingKeyParser.Object, TestProducerManager.Create(), TestConfiguration.GetConfig(), _loggerFactory);
        messageReceiver.FeedMessageReceived += (_, args) => receivedMessage = args.Message;

        messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetValidBasicDeliverEventArgsForFeedMessage(oddsChange));

        receivedMessage.ShouldBeNull();
        var execLogger = _loggerFactory.GetOrCreateLogger(typeof(RabbitMqMessageReceiver));
        execLogger.Messages.ShouldBeOfSize(1);
        execLogger.Messages.Count(m => m.Contains("Error consuming feed message.")).ShouldBe(1);
    }

    [Fact]
    public void WhenReceivedIsNotSubscribedToThenReceivedEventIsNotInvoked()
    {
        var rawEventInvoked = false;
        var oddsChange = GetOddsChangeWithSingleMarket();
        _messageReceiver.RawFeedMessageReceived += (_, _) => rawEventInvoked = true;
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetValidBasicDeliverEventArgsForFeedMessage(oddsChange));

        rawEventInvoked.ShouldBeTrue();
    }

    [Fact]
    public void WhenRawEventIsSubscribedToThenRawAndReceivedEventAreInvoked()
    {
        var receivedEventInvoked = false;
        var rawEventInvoked = false;
        var oddsChange = GetOddsChangeWithSingleMarket();
        _messageReceiver.FeedMessageReceived += (_, _) => receivedEventInvoked = true;
        _messageReceiver.RawFeedMessageReceived += (_, _) => rawEventInvoked = true;
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetValidBasicDeliverEventArgsForFeedMessage(oddsChange));

        receivedEventInvoked.ShouldBeTrue();
        rawEventInvoked.ShouldBeTrue();
    }

    [Fact]
    public void WhenRawEventIsSubscribedToAndUserProcessingThrowsThenReceivedEventIsStillInvoked()
    {
        var receivedEventInvoked = false;
        var rawEventInvoked = false;
        var oddsChange = GetOddsChangeWithSingleMarket();
        _messageReceiver.FeedMessageReceived += (_, _) => receivedEventInvoked = true;
        _messageReceiver.RawFeedMessageReceived += (_, _) =>
                                                   {
                                                       rawEventInvoked = true;
                                                       throw new InvalidOperationException();
                                                   };
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetValidBasicDeliverEventArgsForFeedMessage(oddsChange));

        receivedEventInvoked.ShouldBeTrue();
        rawEventInvoked.ShouldBeTrue();
        var execLogger = _loggerFactory.GetOrCreateLogger(typeof(RabbitMqMessageReceiver));
        execLogger.Messages.ShouldBeOfSize(2);
        execLogger.Messages.Count(m => m.Contains("Error dispatching raw message for")).ShouldBe(1);
    }

    [Fact]
    public void WhenMessageAssociatedWithEventThenEventUrnIsPopulated()
    {
        FeedMessage receivedMessage = null;
        var oddsChange = GetOddsChangeWithSingleMarket();
        _messageReceiver.FeedMessageReceived += (_, args) => receivedMessage = args.Message;
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetValidBasicDeliverEventArgsForFeedMessage(oddsChange));

        receivedMessage.ShouldNotBeNull();
        receivedMessage.EventUrn.ShouldNotBeNull();
        receivedMessage.IsEventRelated.ShouldBeTrue();
    }

    [Fact]
    public void WhenMessageNotAssociatedWithEventThenEventUrnIsNotPopulated()
    {
        FeedMessage receivedMessage = null;
        var alive = FeedMessageBuilder.Create(1).BuildAlive(1);
        _messageReceiver.FeedMessageReceived += (_, args) => receivedMessage = args.Message;
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        _mockRabbitChannel.Raise(mock => mock.Received += null, GetValidBasicDeliverEventArgsForFeedMessage(alive));

        receivedMessage.ShouldNotBeNull();
        receivedMessage.EventUrn.ShouldBeNull();
        receivedMessage.IsEventRelated.ShouldBeFalse();
    }

    [Fact]
    public void WhenMessageAssociatedWithEmptyEventIdThenEventUrnIsNoyPopulated()
    {
        FeedMessage receivedMessage = null;
        const string routingKey = "hi.-.live.odds_change.2.sr:match.9900415";
        var oddsChange = GetOddsChangeWithSingleMarket();
        oddsChange.event_id = string.Empty;
        var xmlBody = FeedMessageBuilder.BuildMessageBody(oddsChange);
        var messageBody = Encoding.UTF8.GetBytes(xmlBody);
        _messageReceiver.FeedMessageReceived += (_, args) => receivedMessage = args.Message;
        _messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());

        _mockRabbitChannel.Raise(mock => mock.Received += null, new BasicDeliverEventArgs("", 1, false, ",", routingKey, null, messageBody));

        receivedMessage.ShouldNotBeNull();
        receivedMessage.EventUrn.ShouldBeNull();
        receivedMessage.IsEventRelated.ShouldBeTrue();
    }

    private static odds_change GetOddsChangeWithSingleMarket()
    {
        return OddsChangeBuilder.Create()
                                .WithProduct(1)
                                .ForEventId(TestConsts.AnyMatchId)
                                .AddMarket(market => market.WithMarketId(1).WithOutcome("1", 1.5).WithOutcome("2", 2.5))
                                .Build();
    }

    private static byte[] GetFeedMessageBodyForOddsChange()
    {
        var oddsChange = GetOddsChangeWithSingleMarket();
        var xmlBody = FeedMessageBuilder.BuildMessageBody(oddsChange);

        return Encoding.UTF8.GetBytes(xmlBody);
    }

    private static IBasicProperties GetBasicPropertiesWithHeader(IDictionary<string, object> headers = null)
    {
        var basicPropertiesMock = new Mock<IBasicProperties>();

        if (!headers.IsNullOrEmpty())
        {
            basicPropertiesMock.Setup(s => s.IsHeadersPresent()).Returns(true);
            basicPropertiesMock.Setup(s => s.Headers).Returns(headers);
        }

        return basicPropertiesMock.Object;
    }

    private static BasicDeliverEventArgs GetBasicDeliverEventArgsWithBodyAndNullRoutingKey(IBasicProperties basicProperties, ReadOnlyMemory<byte> body)
    {
        return new BasicDeliverEventArgs("", 1, false, ",", null, basicProperties, body);
    }

    private static BasicDeliverEventArgs GetValidBasicDeliverEventArgsForFeedMessage(FeedMessage message)
    {
        var xmlBody = FeedMessageBuilder.BuildMessageBody(message);
        var messageData = Encoding.UTF8.GetBytes(xmlBody);
        var sentTimestamp = DateTime.Now.Ticks;
        var basicProperties = GetBasicPropertiesWithHeader(new Dictionary<string, object> { { "timestamp_in_ms", sentTimestamp } });
        var routingKey = FeedMessageBuilder.BuildRoutingKey(message);
        return new BasicDeliverEventArgs("", 1, false, ",", routingKey, basicProperties, messageData);
    }

    private RabbitMqMessageReceiver OpenMessageReceiverWithLongProcessing()
    {
        var sportId = UrnCreate.SportId(1);
        var mockRoutingKeyParser = new Mock<IRoutingKeyParser>();
        mockRoutingKeyParser.Setup(s => s.TryGetSportId(It.IsAny<string>(), It.IsAny<string>(), out sportId))
                            .Returns(() =>
                                     {
                                         Thread.Sleep(TimeSpan.FromMilliseconds(300));
                                         return true;
                                     });

        var messageReceiver = new RabbitMqMessageReceiver(_mockRabbitChannel.Object, new Deserializer<FeedMessage>(), mockRoutingKeyParser.Object, TestProducerManager.Create(), TestConfiguration.GetConfig(), _loggerFactory);

        messageReceiver.Open(MessageInterest.AllMessages, FeedRoutingKeyBuilder.GetStandardKeys());
        return messageReceiver;
    }
}

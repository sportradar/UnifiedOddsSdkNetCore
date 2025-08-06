// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Messages.Internal;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Api;

public class UofSessionTests
{
    private readonly StubMessageReceiver _stubMessageReceiver;
    private readonly StubMessageProcessor _stubFeedMessageProcessor;
    private readonly Mock<IFeedMessageMapper> _mockFeedMessageMapper;
    private readonly Mock<IFeedMessageValidator> _mockFeedMessageValidator;
    private readonly Mock<IMessageDataExtractor> _mockMessageDataExtractor;
    private readonly Mock<IDispatcherStore> _mockDispatcherStore;

    public UofSessionTests()
    {
        _stubMessageReceiver = new StubMessageReceiver(MessageInterest.AllMessages);
        _stubFeedMessageProcessor = new StubMessageProcessor();
        _mockFeedMessageMapper = new Mock<IFeedMessageMapper>();
        _mockFeedMessageValidator = new Mock<IFeedMessageValidator>();
        _mockMessageDataExtractor = new Mock<IMessageDataExtractor>();
        _mockDispatcherStore = new Mock<IDispatcherStore>();
    }

    [Fact]
    public void Constructor_FullData()
    {
        var uofSession = new UofSession(_stubMessageReceiver,
            _stubFeedMessageProcessor,
            _mockFeedMessageMapper.Object,
            _mockFeedMessageValidator.Object,
            _mockMessageDataExtractor.Object,
            _mockDispatcherStore.Object,
            MessageInterest.AllMessages,
            new[] { new CultureInfo("en") },
            GetRoutingKeys);

        Assert.NotNull(uofSession);
        Assert.False(uofSession.Name.IsNullOrEmpty());
        Assert.Equal("All", uofSession.Name);
    }

    [Fact]
    public void Constructor_SessionsHasUniqueName()
    {
        var session1 = new UofSession(_stubMessageReceiver,
            _stubFeedMessageProcessor,
            _mockFeedMessageMapper.Object,
            _mockFeedMessageValidator.Object,
            _mockMessageDataExtractor.Object,
            _mockDispatcherStore.Object,
            MessageInterest.LiveMessagesOnly,
            new[] { new CultureInfo("en") },
            GetRoutingKeys);

        var session2 = new UofSession(_stubMessageReceiver,
            _stubFeedMessageProcessor,
            _mockFeedMessageMapper.Object,
            _mockFeedMessageValidator.Object,
            _mockMessageDataExtractor.Object,
            _mockDispatcherStore.Object,
            MessageInterest.PrematchMessagesOnly,
            new[] { new CultureInfo("en") },
            GetRoutingKeys);

        Assert.NotNull(session1);
        Assert.False(session1.Name.IsNullOrEmpty());
        Assert.NotNull(session2);
        Assert.False(session2.Name.IsNullOrEmpty());
        Assert.NotEqual(session1.Name, session2.Name);
    }

    [Fact]
    public void Constructor_MissingMessageReceiver_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new UofSession(null,
            _stubFeedMessageProcessor,
            _mockFeedMessageMapper.Object,
            _mockFeedMessageValidator.Object,
            _mockMessageDataExtractor.Object,
            _mockDispatcherStore.Object,
            MessageInterest.AllMessages,
            new[] { new CultureInfo("en") },
            GetRoutingKeys));
    }

    [Fact]
    public void Constructor_MissingFeedMessageProcessor_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new UofSession(_stubMessageReceiver,
            null,
            _mockFeedMessageMapper.Object,
            _mockFeedMessageValidator.Object,
            _mockMessageDataExtractor.Object,
            _mockDispatcherStore.Object,
            MessageInterest.AllMessages,
            new[] { new CultureInfo("en") },
            GetRoutingKeys));
    }

    [Fact]
    public void Constructor_MissingFeedMessageMapper_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new UofSession(_stubMessageReceiver,
            _stubFeedMessageProcessor,
            null,
            _mockFeedMessageValidator.Object,
            _mockMessageDataExtractor.Object,
            _mockDispatcherStore.Object,
            MessageInterest.AllMessages,
            new[] { new CultureInfo("en") },
            GetRoutingKeys));
    }

    [Fact]
    public void Constructor_MissingMessageValidator_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new UofSession(_stubMessageReceiver,
            _stubFeedMessageProcessor,
            _mockFeedMessageMapper.Object,
            null,
            _mockMessageDataExtractor.Object,
            _mockDispatcherStore.Object,
            MessageInterest.AllMessages,
            new[] { new CultureInfo("en") },
            GetRoutingKeys));
    }

    [Fact]
    public void Constructor_MissingMessageDataExtractor_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new UofSession(_stubMessageReceiver,
            _stubFeedMessageProcessor,
            _mockFeedMessageMapper.Object,
            _mockFeedMessageValidator.Object,
            null,
            _mockDispatcherStore.Object,
            MessageInterest.AllMessages,
            new[] { new CultureInfo("en") },
            GetRoutingKeys));
    }

    [Fact]
    public void Constructor_MissingDispatcherStore_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new UofSession(_stubMessageReceiver,
            _stubFeedMessageProcessor,
            _mockFeedMessageMapper.Object,
            _mockFeedMessageValidator.Object,
            _mockMessageDataExtractor.Object,
            null,
            MessageInterest.AllMessages,
            new[] { new CultureInfo("en") },
            GetRoutingKeys));
    }

    [Fact]
    public void Constructor_MissingMessageInterest_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new UofSession(_stubMessageReceiver,
            _stubFeedMessageProcessor,
            _mockFeedMessageMapper.Object,
            _mockFeedMessageValidator.Object,
            _mockMessageDataExtractor.Object,
            _mockDispatcherStore.Object,
            null,
            new[] { new CultureInfo("en") },
            GetRoutingKeys));
    }

    [Fact]
    public void Constructor_MissingDefaultCulture_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new UofSession(_stubMessageReceiver,
            _stubFeedMessageProcessor,
            _mockFeedMessageMapper.Object,
            _mockFeedMessageValidator.Object,
            _mockMessageDataExtractor.Object,
            _mockDispatcherStore.Object,
            MessageInterest.AllMessages,
            null,
            GetRoutingKeys));
    }

    [Fact]
    public void Constructor_EmptyDefaultCulture_Throws()
    {
        Assert.Throws<ArgumentException>(() => new UofSession(_stubMessageReceiver,
            _stubFeedMessageProcessor,
            _mockFeedMessageMapper.Object,
            _mockFeedMessageValidator.Object,
            _mockMessageDataExtractor.Object,
            _mockDispatcherStore.Object,
            MessageInterest.AllMessages,
            new List<CultureInfo>(),
            GetRoutingKeys));
    }

    [Fact]
    public void Constructor_MissingRoutingKeysFunction_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new UofSession(_stubMessageReceiver,
            _stubFeedMessageProcessor,
            _mockFeedMessageMapper.Object,
            _mockFeedMessageValidator.Object,
            _mockMessageDataExtractor.Object,
            _mockDispatcherStore.Object,
            MessageInterest.AllMessages,
            new[] { new CultureInfo("en") },
            null));
    }

    [Fact]
    public void Dispatch_OnUnparsableMessageReceived()
    {
        var messageIsDispatched = false;
        var session = GetDefaultSession();
        session.OnUnparsableMessageReceived += (_, _) => messageIsDispatched = true;
        _mockMessageDataExtractor.Setup(x => x.GetBasicMessageData(It.IsAny<byte[]>())).Returns(new BasicMessageData(MessageType.OddsChange, "1", "sr:match:1234"));

        session.Open();
        _stubMessageReceiver.SimulateDispatchFeedMessageDeserializationFailed(new odds_change());

        Assert.True(messageIsDispatched);
    }

    [Fact]
    public void Dispatch_OnUnparsableMessageReceived_WithoutProducer()
    {
        var messageIsDispatched = false;
        var session = GetDefaultSession();
        session.OnUnparsableMessageReceived += (_, _) => messageIsDispatched = true;
        _mockMessageDataExtractor.Setup(x => x.GetBasicMessageData(It.IsAny<byte[]>())).Returns(new BasicMessageData(MessageType.OddsChange, string.Empty, "sr:match:1234"));

        session.Open();
        _stubMessageReceiver.SimulateDispatchFeedMessageDeserializationFailed(new odds_change());

        Assert.True(messageIsDispatched);
    }

    [Fact]
    public void Dispatch_OnUnparsableMessageReceived_WithoutMessageData()
    {
        var messageIsDispatched = false;
        var session = GetDefaultSession();
        session.OnUnparsableMessageReceived += (_, _) => messageIsDispatched = true;
        _mockMessageDataExtractor.Setup(x => x.GetBasicMessageData(It.IsAny<byte[]>())).Returns(new BasicMessageData(MessageType.Unknown, null, null));

        session.Open();
        _stubMessageReceiver.SimulateDispatchFeedMessageDeserializationFailed(new odds_change());

        Assert.True(messageIsDispatched);
    }

    [Fact]
    public void Dispatch_OnUnparsableMessageReceived_NoHandler()
    {
        var messageIsDispatched = false;
        var session = GetDefaultSession();
        _mockMessageDataExtractor.Setup(x => x.GetBasicMessageData(It.IsAny<byte[]>())).Returns(new BasicMessageData(MessageType.OddsChange, "1", "sr:match:1234"));

        session.Open();
        _stubMessageReceiver.SimulateDispatchFeedMessageDeserializationFailed(new odds_change());

        Assert.False(messageIsDispatched);
    }

    [Fact]
    public void Dispatch_OnUnparsableMessageReceived_HandlerThrows()
    {
        var messageIsDispatched = false;
        var session = GetDefaultSession();
        session.OnUnparsableMessageReceived += (_, _) =>
        {
            messageIsDispatched = true;
            throw new InvalidOperationException();
        };
        _mockMessageDataExtractor.Setup(x => x.GetBasicMessageData(It.IsAny<byte[]>())).Returns(new BasicMessageData(MessageType.OddsChange, "1", "sr:match:1234"));

        session.Open();
        _stubMessageReceiver.SimulateDispatchFeedMessageDeserializationFailed(new odds_change());

        Assert.True(messageIsDispatched);
    }

    [Fact]
    public void OnMessageReceived_ValidationFailure()
    {
        var messageIsDispatched = false;
        var feedMessage = new odds_change();
        var session = GetDefaultSession();
        session.OnUnparsableMessageReceived += (_, _) => messageIsDispatched = true;
        _mockFeedMessageValidator.Setup(x => x.Validate(It.IsAny<FeedMessage>())).Returns(ValidationResult.Failure);
        _mockMessageDataExtractor.Setup(x => x.GetBasicMessageData(It.IsAny<byte[]>())).Returns(new BasicMessageData(MessageType.OddsChange, "1", "sr:match:1234"));

        session.Open();
        _stubMessageReceiver.SimulateDispatchMessage(feedMessage);

        Assert.True(messageIsDispatched);
    }

    [Fact]
    public void OnMessageReceived_ValidationProblemsDetected()
    {
        var messageIsDispatched = false;
        var feedMessage = new odds_change() { event_id = "sr:match:1234", EventUrn = Urn.Parse("sr:match:1234"), SportId = Urn.Parse("sr:sport:1"), product = 1 };
        var session = GetDefaultSession();
        session.OnOddsChange += (_, _) => messageIsDispatched = true;
        _mockFeedMessageValidator.Setup(x => x.Validate(It.IsAny<FeedMessage>())).Returns(ValidationResult.ProblemsDetected);
        _mockMessageDataExtractor.Setup(x => x.GetBasicMessageData(It.IsAny<byte[]>())).Returns(new BasicMessageData(MessageType.OddsChange, "1", "sr:match:1234"));
        _mockDispatcherStore.Setup(x => x.Get(It.IsAny<Urn>(), It.IsAny<Urn>())).Returns((ISpecificEntityDispatcherInternal)null);

        session.Open();
        _stubMessageReceiver.SimulateDispatchMessage(feedMessage);

        Assert.True(messageIsDispatched);
    }

    [Fact]
    public void OnMessageReceived_ValidationSucceeded()
    {
        var messageIsDispatched = false;
        var feedMessage = new odds_change() { event_id = "sr:match:1234", EventUrn = Urn.Parse("sr:match:1234"), SportId = Urn.Parse("sr:sport:1"), product = 1 };
        var session = GetDefaultSession();
        session.OnOddsChange += (_, _) => messageIsDispatched = true;
        _mockFeedMessageValidator.Setup(x => x.Validate(It.IsAny<FeedMessage>())).Returns(ValidationResult.Success);
        _mockMessageDataExtractor.Setup(x => x.GetBasicMessageData(It.IsAny<byte[]>())).Returns(new BasicMessageData(MessageType.OddsChange, "1", "sr:match:1234"));
        _mockDispatcherStore.Setup(x => x.Get(It.IsAny<Urn>(), It.IsAny<Urn>())).Returns((ISpecificEntityDispatcherInternal)null);

        session.Open();
        _stubMessageReceiver.SimulateDispatchMessage(feedMessage);

        Assert.True(messageIsDispatched);
    }

    [Fact]
    public void DispatchOddsChangeMessage()
    {
        var messageIsDispatched = false;
        var feedMessage = new odds_change();
        var messageBytes = JsonSerializer.SerializeToUtf8Bytes(feedMessage);
        var session = GetDefaultSession();
        session.OnOddsChange += (_, _) => messageIsDispatched = true;

        session.Dispatch(feedMessage, messageBytes);

        Assert.True(messageIsDispatched);
    }

    [Fact]
    public void DispatchOddsChangeMessage_WithoutHandler()
    {
        var messageIsDispatched = false;
        var feedMessage = new odds_change();
        var messageBytes = JsonSerializer.SerializeToUtf8Bytes(feedMessage);
        var session = GetDefaultSession();

        session.Dispatch(feedMessage, messageBytes);

        Assert.False(messageIsDispatched);
    }

    [Fact]
    public void DispatchOddsChangeMessage_WithHandlerThrowingError()
    {
        var messageIsDispatched = false;
        var feedMessage = new odds_change();
        var messageBytes = JsonSerializer.SerializeToUtf8Bytes(feedMessage);
        var session = GetDefaultSession();
        session.OnOddsChange += (_, _) =>
        {
            messageIsDispatched = true;
            throw new InvalidOperationException();
        };

        session.Dispatch(feedMessage, messageBytes);

        Assert.True(messageIsDispatched);
    }

    [Fact]
    public void DispatchFixtureChangeMessage()
    {
        var messageIsDispatched = false;
        var feedMessage = new fixture_change();
        var messageBytes = JsonSerializer.SerializeToUtf8Bytes(feedMessage);
        var session = GetDefaultSession();
        session.OnFixtureChange += (_, _) => messageIsDispatched = true;

        session.Dispatch(feedMessage, messageBytes);

        Assert.True(messageIsDispatched);
    }

    [Fact]
    public void DispatchBetStopMessage()
    {
        var messageIsDispatched = false;
        var feedMessage = new bet_stop();
        var messageBytes = JsonSerializer.SerializeToUtf8Bytes(feedMessage);
        var session = GetDefaultSession();
        session.OnBetStop += (_, _) => messageIsDispatched = true;

        session.Dispatch(feedMessage, messageBytes);

        Assert.True(messageIsDispatched);
    }

    [Fact]
    public void DispatchBetCancelMessage()
    {
        var messageIsDispatched = false;
        var feedMessage = new bet_cancel();
        var messageBytes = JsonSerializer.SerializeToUtf8Bytes(feedMessage);
        var session = GetDefaultSession();
        session.OnBetCancel += (_, _) => messageIsDispatched = true;

        session.Dispatch(feedMessage, messageBytes);

        Assert.True(messageIsDispatched);
    }

    [Fact]
    public void DispatchBetSettlementMessage()
    {
        var messageIsDispatched = false;
        var feedMessage = new bet_settlement();
        var messageBytes = JsonSerializer.SerializeToUtf8Bytes(feedMessage);
        var session = GetDefaultSession();
        session.OnBetSettlement += (_, _) => messageIsDispatched = true;

        session.Dispatch(feedMessage, messageBytes);

        Assert.True(messageIsDispatched);
    }

    [Fact]
    public void DispatchRollbackBetCancelMessage()
    {
        var messageIsDispatched = false;
        var feedMessage = new rollback_bet_cancel();
        var messageBytes = JsonSerializer.SerializeToUtf8Bytes(feedMessage);
        var session = GetDefaultSession();
        session.OnRollbackBetCancel += (_, _) => messageIsDispatched = true;

        session.Dispatch(feedMessage, messageBytes);

        Assert.True(messageIsDispatched);
    }

    [Fact]
    public void DispatchRollbackBetSettlementMessage()
    {
        var messageIsDispatched = false;
        var feedMessage = new rollback_bet_settlement();
        var messageBytes = JsonSerializer.SerializeToUtf8Bytes(feedMessage);
        var session = GetDefaultSession();
        session.OnRollbackBetSettlement += (_, _) => messageIsDispatched = true;

        session.Dispatch(feedMessage, messageBytes);

        Assert.True(messageIsDispatched);
    }

    private UofSession GetDefaultSession()
    {
        return new UofSession(_stubMessageReceiver,
                _stubFeedMessageProcessor,
                _mockFeedMessageMapper.Object,
                _mockFeedMessageValidator.Object,
                _mockMessageDataExtractor.Object,
                _mockDispatcherStore.Object,
                MessageInterest.AllMessages,
                new[] { new CultureInfo("en") },
                GetRoutingKeys);
    }

    private IEnumerable<string> GetRoutingKeys(UofSession session)
    {
        return new[] { "*" };
    }
}

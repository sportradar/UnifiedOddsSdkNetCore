using System;
using System.Globalization;
using Moq;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Api;

public class EntityDispatcherTests
{
    private readonly Mock<IFeedMessageMapper> _mockFeedMessageMapper;

    public EntityDispatcherTests()
    {
        _mockFeedMessageMapper = new Mock<IFeedMessageMapper>();
    }

    [Fact]
    public void SpecificEntityDispatcher_Constructor_Initialization()
    {
        var dispatcher = new SpecificEntityDispatcher<ISportEvent>(_mockFeedMessageMapper.Object, new[] { TestData.Culture });

        Assert.NotNull(dispatcher);
    }

    [Fact]
    public void SpecificEntityDispatcher_NullMessageReceiver_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SpecificEntityDispatcher<ISportEvent>(null, new[] { TestData.Culture }));
    }

    [Fact]
    public void SpecificEntityDispatcher_NullDefaultCulture_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SpecificEntityDispatcher<ISportEvent>(_mockFeedMessageMapper.Object, null));
    }

    [Fact]
    public void SpecificEntityDispatcher_EmptyDefaultCulture_Throws()
    {
        Assert.Throws<ArgumentException>(() => new SpecificEntityDispatcher<ISportEvent>(_mockFeedMessageMapper.Object, Array.Empty<CultureInfo>()));
    }

    [Fact]
    public void Dispatcher_Open_IsOpened()
    {
        var dispatcher = GetDefaultSpecificEntityDispatcher<ISportEvent>();
        dispatcher.Open();

        Assert.True(dispatcher.IsOpened);
    }

    [Fact]
    public void Dispatcher_OpenedTwice_Throws()
    {
        var dispatcher = GetDefaultSpecificEntityDispatcher<ISportEvent>();
        dispatcher.Open();
        Assert.True(dispatcher.IsOpened);

        Assert.Throws<InvalidOperationException>(() => dispatcher.Open());
    }

    [Fact]
    public void Dispatcher_CloseOpenedInstance_IsOpened()
    {
        var dispatcher = GetDefaultSpecificEntityDispatcher<ISportEvent>();
        dispatcher.Open();
        Assert.True(dispatcher.IsOpened);

        dispatcher.Close();
        Assert.False(dispatcher.IsOpened);
    }

    [Fact]
    public void Dispatcher_ClosingClosedInstance_Throws()
    {
        var dispatcher = GetDefaultSpecificEntityDispatcher<ISportEvent>();
        Assert.False(dispatcher.IsOpened);

        Assert.Throws<InvalidOperationException>(() => dispatcher.Close());
    }

    [Fact]
    public void Dispatch_NonFeedMessage_Throws()
    {
        var dispatcher = GetDefaultSpecificEntityDispatcher<ISportEvent>();

        Assert.Throws<ArgumentException>(() => dispatcher.Dispatch(new FakeMessage(), new byte[1]));
    }

    [Fact]
    public void DispatchFeedMessage_WithoutHandler()
    {
        var dispatcher = GetDefaultSpecificEntityDispatcher<ISportEvent>();
        dispatcher.Dispatch(new odds_change(), new byte[1]);

        // TODO: how to test that nothing happens - maybe logs
        Assert.NotNull(dispatcher);
    }

    [Fact]
    public void DispatchFeedMessage_WithHandler()
    {
        var msgIsDispatched = false;
        var dispatcher = GetDefaultSpecificEntityDispatcher<ISportEvent>();
        dispatcher.OnOddsChange += (_, _) => msgIsDispatched = true;
        dispatcher.Dispatch(new odds_change(), new byte[1]);

        Assert.True(msgIsDispatched);
    }

    [Fact]
    public void DispatchFeedMessage_WithHandlerThrowingError()
    {
        var msgIsDispatched = false;
        var dispatcher = GetDefaultSpecificEntityDispatcher<ISportEvent>();
        dispatcher.OnOddsChange += (_, _) =>
        {
            msgIsDispatched = true;
            throw new InvalidOperationException();
        };
        dispatcher.Dispatch(new odds_change(), new byte[1]);

        Assert.True(msgIsDispatched);
    }

    [Fact]
    public void Dispatch_OnCloseInvoked_CloseEvent()
    {
        var msgIsDispatched = false;
        var dispatcher = GetDefaultSpecificEntityDispatcher<ISportEvent>();
        dispatcher.OnClosed += (_, _) => msgIsDispatched = true;

        dispatcher.Open();
        dispatcher.Close();

        Assert.True(msgIsDispatched);
    }

    [Fact]
    public void Dispatch_OnCloseInvoked_WithoutHandler()
    {
        var dispatcher = GetDefaultSpecificEntityDispatcher<ISportEvent>();

        dispatcher.Open();
        dispatcher.Close();

        // TODO: how to test that nothing happens - maybe logs
        Assert.NotNull(dispatcher);
    }

    [Fact]
    public void Dispatch_OnCloseInvoked_WithHandlerThrowingError()
    {
        var msgIsDispatched = false;
        var dispatcher = GetDefaultSpecificEntityDispatcher<ISportEvent>();
        dispatcher.OnClosed += (_, _) =>
        {
            msgIsDispatched = true;
            throw new InvalidOperationException();
        };

        dispatcher.Open();
        Assert.Throws<InvalidOperationException>(() => dispatcher.Close());

        Assert.True(msgIsDispatched);
    }

    [Fact]
    public void DispatchOddsChangeMessage_WithHandler_InvokesProperEvent()
    {
        var oddsChangeIsDispatched = false;
        var fixtureChangeIsDispatched = false;
        var betStopIsDispatched = false;
        var betCancelIsDispatched = false;
        var betSettlementIsDispatched = false;
        var rollbackBetCancelIsDispatched = false;
        var rollbackBetSettlementIsDispatched = false;

        var dispatcher = GetDefaultSpecificEntityDispatcher<ISportEvent>();
        dispatcher.OnOddsChange += (_, _) => oddsChangeIsDispatched = true;
        dispatcher.OnFixtureChange += (_, _) => fixtureChangeIsDispatched = true;
        dispatcher.OnBetStop += (_, _) => betStopIsDispatched = true;
        dispatcher.OnBetCancel += (_, _) => betCancelIsDispatched = true;
        dispatcher.OnBetSettlement += (_, _) => betSettlementIsDispatched = true;
        dispatcher.OnRollbackBetCancel += (_, _) => rollbackBetCancelIsDispatched = true;
        dispatcher.OnRollbackBetSettlement += (_, _) => rollbackBetSettlementIsDispatched = true;

        dispatcher.Dispatch(new odds_change(), new byte[1]);

        Assert.True(oddsChangeIsDispatched);
        Assert.False(fixtureChangeIsDispatched);
        Assert.False(betStopIsDispatched);
        Assert.False(betCancelIsDispatched);
        Assert.False(betSettlementIsDispatched);
        Assert.False(rollbackBetCancelIsDispatched);
        Assert.False(rollbackBetSettlementIsDispatched);
    }

    [Fact]
    public void DispatchFixtureChangeMessage_WithHandler_InvokesProperEvent()
    {
        var oddsChangeIsDispatched = false;
        var fixtureChangeIsDispatched = false;
        var betStopIsDispatched = false;
        var betCancelIsDispatched = false;
        var betSettlementIsDispatched = false;
        var rollbackBetCancelIsDispatched = false;
        var rollbackBetSettlementIsDispatched = false;

        var dispatcher = GetDefaultSpecificEntityDispatcher<ISportEvent>();
        dispatcher.OnOddsChange += (_, _) => oddsChangeIsDispatched = true;
        dispatcher.OnFixtureChange += (_, _) => fixtureChangeIsDispatched = true;
        dispatcher.OnBetStop += (_, _) => betStopIsDispatched = true;
        dispatcher.OnBetCancel += (_, _) => betCancelIsDispatched = true;
        dispatcher.OnBetSettlement += (_, _) => betSettlementIsDispatched = true;
        dispatcher.OnRollbackBetCancel += (_, _) => rollbackBetCancelIsDispatched = true;
        dispatcher.OnRollbackBetSettlement += (_, _) => rollbackBetSettlementIsDispatched = true;

        dispatcher.Dispatch(new fixture_change(), new byte[1]);

        Assert.False(oddsChangeIsDispatched);
        Assert.True(fixtureChangeIsDispatched);
        Assert.False(betStopIsDispatched);
        Assert.False(betCancelIsDispatched);
        Assert.False(betSettlementIsDispatched);
        Assert.False(rollbackBetCancelIsDispatched);
        Assert.False(rollbackBetSettlementIsDispatched);
    }

    [Fact]
    public void DispatchBetStopMessage_WithHandler_InvokesProperEvent()
    {
        var oddsChangeIsDispatched = false;
        var fixtureChangeIsDispatched = false;
        var betStopIsDispatched = false;
        var betCancelIsDispatched = false;
        var betSettlementIsDispatched = false;
        var rollbackBetCancelIsDispatched = false;
        var rollbackBetSettlementIsDispatched = false;

        var dispatcher = GetDefaultSpecificEntityDispatcher<ISportEvent>();
        dispatcher.OnOddsChange += (_, _) => oddsChangeIsDispatched = true;
        dispatcher.OnFixtureChange += (_, _) => fixtureChangeIsDispatched = true;
        dispatcher.OnBetStop += (_, _) => betStopIsDispatched = true;
        dispatcher.OnBetCancel += (_, _) => betCancelIsDispatched = true;
        dispatcher.OnBetSettlement += (_, _) => betSettlementIsDispatched = true;
        dispatcher.OnRollbackBetCancel += (_, _) => rollbackBetCancelIsDispatched = true;
        dispatcher.OnRollbackBetSettlement += (_, _) => rollbackBetSettlementIsDispatched = true;

        dispatcher.Dispatch(new bet_stop(), new byte[1]);

        Assert.False(oddsChangeIsDispatched);
        Assert.False(fixtureChangeIsDispatched);
        Assert.True(betStopIsDispatched);
        Assert.False(betCancelIsDispatched);
        Assert.False(betSettlementIsDispatched);
        Assert.False(rollbackBetCancelIsDispatched);
        Assert.False(rollbackBetSettlementIsDispatched);
    }

    [Fact]
    public void DispatchBetCancelMessage_WithHandler_InvokesProperEvent()
    {
        var oddsChangeIsDispatched = false;
        var fixtureChangeIsDispatched = false;
        var betStopIsDispatched = false;
        var betCancelIsDispatched = false;
        var betSettlementIsDispatched = false;
        var rollbackBetCancelIsDispatched = false;
        var rollbackBetSettlementIsDispatched = false;

        var dispatcher = GetDefaultSpecificEntityDispatcher<ISportEvent>();
        dispatcher.OnOddsChange += (_, _) => oddsChangeIsDispatched = true;
        dispatcher.OnFixtureChange += (_, _) => fixtureChangeIsDispatched = true;
        dispatcher.OnBetStop += (_, _) => betStopIsDispatched = true;
        dispatcher.OnBetCancel += (_, _) => betCancelIsDispatched = true;
        dispatcher.OnBetSettlement += (_, _) => betSettlementIsDispatched = true;
        dispatcher.OnRollbackBetCancel += (_, _) => rollbackBetCancelIsDispatched = true;
        dispatcher.OnRollbackBetSettlement += (_, _) => rollbackBetSettlementIsDispatched = true;

        dispatcher.Dispatch(new bet_cancel(), new byte[1]);

        Assert.False(oddsChangeIsDispatched);
        Assert.False(fixtureChangeIsDispatched);
        Assert.False(betStopIsDispatched);
        Assert.True(betCancelIsDispatched);
        Assert.False(betSettlementIsDispatched);
        Assert.False(rollbackBetCancelIsDispatched);
        Assert.False(rollbackBetSettlementIsDispatched);
    }

    [Fact]
    public void DispatchBetSettlementMessage_WithHandler_InvokesProperEvent()
    {
        var oddsChangeIsDispatched = false;
        var fixtureChangeIsDispatched = false;
        var betStopIsDispatched = false;
        var betCancelIsDispatched = false;
        var betSettlementIsDispatched = false;
        var rollbackBetCancelIsDispatched = false;
        var rollbackBetSettlementIsDispatched = false;

        var dispatcher = GetDefaultSpecificEntityDispatcher<ISportEvent>();
        dispatcher.OnOddsChange += (_, _) => oddsChangeIsDispatched = true;
        dispatcher.OnFixtureChange += (_, _) => fixtureChangeIsDispatched = true;
        dispatcher.OnBetStop += (_, _) => betStopIsDispatched = true;
        dispatcher.OnBetCancel += (_, _) => betCancelIsDispatched = true;
        dispatcher.OnBetSettlement += (_, _) => betSettlementIsDispatched = true;
        dispatcher.OnRollbackBetCancel += (_, _) => rollbackBetCancelIsDispatched = true;
        dispatcher.OnRollbackBetSettlement += (_, _) => rollbackBetSettlementIsDispatched = true;

        dispatcher.Dispatch(new bet_settlement(), new byte[1]);

        Assert.False(oddsChangeIsDispatched);
        Assert.False(fixtureChangeIsDispatched);
        Assert.False(betStopIsDispatched);
        Assert.False(betCancelIsDispatched);
        Assert.True(betSettlementIsDispatched);
        Assert.False(rollbackBetCancelIsDispatched);
        Assert.False(rollbackBetSettlementIsDispatched);
    }

    [Fact]
    public void DispatchRollbackBetCancelMessage_WithHandler_InvokesProperEvent()
    {
        var oddsChangeIsDispatched = false;
        var fixtureChangeIsDispatched = false;
        var betStopIsDispatched = false;
        var betCancelIsDispatched = false;
        var betSettlementIsDispatched = false;
        var rollbackBetCancelIsDispatched = false;
        var rollbackBetSettlementIsDispatched = false;

        var dispatcher = GetDefaultSpecificEntityDispatcher<ISportEvent>();
        dispatcher.OnOddsChange += (_, _) => oddsChangeIsDispatched = true;
        dispatcher.OnFixtureChange += (_, _) => fixtureChangeIsDispatched = true;
        dispatcher.OnBetStop += (_, _) => betStopIsDispatched = true;
        dispatcher.OnBetCancel += (_, _) => betCancelIsDispatched = true;
        dispatcher.OnBetSettlement += (_, _) => betSettlementIsDispatched = true;
        dispatcher.OnRollbackBetCancel += (_, _) => rollbackBetCancelIsDispatched = true;
        dispatcher.OnRollbackBetSettlement += (_, _) => rollbackBetSettlementIsDispatched = true;

        dispatcher.Dispatch(new rollback_bet_cancel(), new byte[1]);

        Assert.False(oddsChangeIsDispatched);
        Assert.False(fixtureChangeIsDispatched);
        Assert.False(betStopIsDispatched);
        Assert.False(betCancelIsDispatched);
        Assert.False(betSettlementIsDispatched);
        Assert.True(rollbackBetCancelIsDispatched);
        Assert.False(rollbackBetSettlementIsDispatched);
    }

    [Fact]
    public void DispatchRollbackBetSettlementMessage_WithHandler_InvokesProperEvent()
    {
        var oddsChangeIsDispatched = false;
        var fixtureChangeIsDispatched = false;
        var betStopIsDispatched = false;
        var betCancelIsDispatched = false;
        var betSettlementIsDispatched = false;
        var rollbackBetCancelIsDispatched = false;
        var rollbackBetSettlementIsDispatched = false;

        var dispatcher = GetDefaultSpecificEntityDispatcher<ISportEvent>();
        dispatcher.OnOddsChange += (_, _) => oddsChangeIsDispatched = true;
        dispatcher.OnFixtureChange += (_, _) => fixtureChangeIsDispatched = true;
        dispatcher.OnBetStop += (_, _) => betStopIsDispatched = true;
        dispatcher.OnBetCancel += (_, _) => betCancelIsDispatched = true;
        dispatcher.OnBetSettlement += (_, _) => betSettlementIsDispatched = true;
        dispatcher.OnRollbackBetCancel += (_, _) => rollbackBetCancelIsDispatched = true;
        dispatcher.OnRollbackBetSettlement += (_, _) => rollbackBetSettlementIsDispatched = true;

        dispatcher.Dispatch(new rollback_bet_settlement(), new byte[1]);

        Assert.False(oddsChangeIsDispatched);
        Assert.False(fixtureChangeIsDispatched);
        Assert.False(betStopIsDispatched);
        Assert.False(betCancelIsDispatched);
        Assert.False(betSettlementIsDispatched);
        Assert.False(rollbackBetCancelIsDispatched);
        Assert.True(rollbackBetSettlementIsDispatched);
    }

    [Fact]
    public void DispatchGlobalEvent_CloseInvoked_Invokes()
    {
        var msgIsDispatched = false;
        var dispatcher = new StubEntityDispatcher();
        dispatcher.OnClosed += (_, _) => msgIsDispatched = true;

        dispatcher.CallCloseEvent(new FeedCloseEventArgs("any"), "OnClose", 1);

        Assert.True(msgIsDispatched);
    }

    [Fact]
    public void DispatchGlobalEvent_CloseInvoked_WithoutHandler()
    {
        var msgIsDispatched = false;
        var dispatcher = new StubEntityDispatcher();

        dispatcher.CallCloseEvent(new FeedCloseEventArgs("any"), "OnClose", 1);

        Assert.False(msgIsDispatched);
    }

    [Fact]
    public void DispatchGlobalEvent_CloseInvoked_WithHandlerThrowingError()
    {
        var msgIsDispatched = false;
        var dispatcher = new StubEntityDispatcher();
        dispatcher.OnClosed += (_, _) =>
        {
            msgIsDispatched = true;
            throw new InvalidOperationException();
        };

        dispatcher.CallCloseEvent(new FeedCloseEventArgs("any"), "OnClose", 1);

        Assert.True(msgIsDispatched);
    }

    private SpecificEntityDispatcher<T> GetDefaultSpecificEntityDispatcher<T>() where T : ISportEvent
    {
        return new SpecificEntityDispatcher<T>(_mockFeedMessageMapper.Object, new[] { TestData.Culture });
    }

    private class FakeMessage : FeedMessage
    {
        public override string Name { get; }
        public override int ProducerId { get; }
        public override long? RequestId { get; }
        public override PropertyUsage RequestIdUsage { get; }
        public override bool IsEventRelated { get; }
        public override bool IsStateful { get; }
        public override string EventId { get; }
        public override long GeneratedAt { get; }
        public override long SentAt { get; set; }
        public override long ReceivedAt { get; set; }
    }

    private sealed class StubEntityDispatcher : EntityDispatcherBase
    {
        public event EventHandler<FeedCloseEventArgs> OnClosed;

        public void CallCloseEvent(FeedCloseEventArgs eventArgs, string eventHandlerName, int producerId)
        {
            Dispatch(OnClosed, eventArgs, eventHandlerName, producerId);
        }
    }
}

/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Represent a root object of the unified odds feed
    /// </summary>
    public interface IOddsFeed : IDisposable
    {
        /// <summary>
        /// Raised when the current instance of <see cref="IOddsFeed"/> loses connection to the feed
        /// </summary>
        event EventHandler<EventArgs> Disconnected;

        /// <summary>
        /// Occurs when feed is closed
        /// </summary>
        event EventHandler<FeedCloseEventArgs> Closed;

        /// <summary>
        /// Raised when the current <see cref="IOddsFeed"/> instance determines that the <see cref="IProducer"/> associated with the odds feed went down
        /// </summary>
        event EventHandler<ProducerStatusChangeEventArgs> ProducerDown;

        /// <summary>
        /// Raised when the current <see cref="IOddsFeed"/> instance determines that the <see cref="IProducer"/> associated with the odds feed went up (back online)
        /// </summary>
        event EventHandler<ProducerStatusChangeEventArgs> ProducerUp;

        /// <summary>
        /// Occurs when a recovery initiation completes
        /// </summary>
        event EventHandler<RecoveryInitiatedEventArgs> RecoveryInitiated;

        /// <summary>
        /// Gets a <see cref="IEventRecoveryRequestIssuer"/> instance used to issue recovery requests to the feed
        /// </summary>
        IEventRecoveryRequestIssuer EventRecoveryRequestIssuer { get; }

        /// <summary>
        /// Gets the <see cref="ICashOutProbabilitiesProvider"/> instance used to retrieve cash out probabilities for betting markets
        /// </summary>
        ICashOutProbabilitiesProvider CashOutProbabilitiesProvider { get; }

        /// <summary>
        /// Gets a <see cref="ISportDataProvider"/> instance used to retrieve sport related data from the feed
        /// </summary>
        ISportDataProvider SportDataProvider { get; }

        /// <summary>
        /// Gets a <see cref="IProducerManager"/> instance used to retrieve producer related data
        /// </summary>
        IProducerManager ProducerManager { get; }

        /// <summary>
        /// Gets a <see cref="IBookingManager"/> instance used to perform various booking calendar operations
        /// </summary>
        IBookingManager BookingManager { get; }

        /// <summary>
        /// Constructs and returns a new instance of <see cref="IOddsFeedSessionBuilder"/>
        /// </summary>
        /// <returns>Constructed instance of the <see cref="IOddsFeedSessionBuilder"/></returns>
        IOddsFeedSessionBuilder CreateBuilder();

        /// <summary>
        /// Opens the current feed by opening all created sessions
        /// </summary>
        void Open();

        /// <summary>
        /// Returns an indicator if the feed instance is opened or not
        /// </summary>
        bool IsOpen();

        /// <summary>
        /// Closes the current feed by closing all created sessions and disposing of all resources associated with the current instance
        /// </summary>
        void Close();

        /// <summary>
        /// Gets a <see cref="IBookmakerDetails"/> instance used to get info about bookmaker and token used
        /// </summary>
        IBookmakerDetails BookmakerDetails { get; }

        /// <summary>
        /// Gets a <see cref="IMarketDescriptionManager"/> instance used to get info about available markets, and to get translations for markets and outcomes including outrights
        /// </summary>
        IMarketDescriptionManager MarketDescriptionManager { get; }

        /// <summary>
        /// Gets a <see cref="ICustomBetManager"/> instance used to perform various custom bet operations
        /// </summary>
        ICustomBetManager CustomBetManager { get; }

        /// <summary>
        /// Occurs when a requested event recovery completes
        /// </summary>
        event EventHandler<EventRecoveryCompletedEventArgs> EventRecoveryCompleted;

        /// <summary>
        /// Occurs when an exception occurs in the connection loop
        /// </summary>
        event EventHandler<ConnectionExceptionEventArgs> ConnectionException;

        /// <summary>
        /// Gets a <see cref="IEventChangeManager"/> instance used to automatically receive fixture and result changes
        /// </summary>
        IEventChangeManager EventChangeManager => null;
    }
}

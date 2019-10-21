/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.API.EventArguments;
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
        /// Closes the current feed by closing all created sessions and disposing of all resources associated with the current instance
        /// </summary>
        void Close();
    }
}

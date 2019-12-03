/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Represents a session to the odds feed
    /// </summary>
    public interface IOddsFeedSession : IEntityDispatcher<ISportEvent>
    {
        /// <summary>
        /// Gets the name of the session
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Raised when a message which cannot be parsed is received
        /// </summary>
        event EventHandler<UnparsableMessageEventArgs> OnUnparsableMessageReceived;

        /// <summary>
        /// Constructs and returns a sport-specific <see cref="ISpecificEntityDispatcher{T}"/> instance allowing
        /// processing of messages containing entity specific information
        /// </summary>
        /// <typeparam name="T">A <see cref="ICompetition"/> derived type specifying the entities associated with the created <see cref="IEntityDispatcher{T}"/> instance</typeparam>
        /// <returns>The constructed <see cref="ISpecificEntityDispatcher{T}"/></returns>
        ISpecificEntityDispatcher<T> CreateSportSpecificMessageDispatcher<T>() where T : ISportEvent;
    }
}

/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Defines a contract implemented by bet-stop messages
    /// </summary>
    /// <typeparam name="T">A <see cref="ICompetition"/> derived type specifying the type of the associated sport event</typeparam>
    public interface IBetStop<out T> : IEventMessage<T> where T : ISportEvent
    {

        /// <summary>
        /// Gets a <see cref="MarketStatus"/> specifying the new status of the associated markets
        /// </summary>
        /// <value>The market status.</value>
        MarketStatus MarketStatus { get; }

        /// <summary>
        /// Get a list of strings specifying which market groups needs to be stopped
        /// </summary>
        IEnumerable<string> Groups { get; }
    }
}

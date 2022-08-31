/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Messages;
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST.CustomBet
{
    /// <summary>
    /// Provides an available selections for a particular event
    /// </summary>
    public interface IAvailableSelectionsFilter
    {
        /// <summary>
        /// Gets the <see cref="URN"/> of the event
        /// </summary>
        URN Event { get; }

        /// <summary>
        /// Gets the list of markets for this event
        /// </summary>
        IEnumerable<IMarketFilter> Markets { get; }
    }
}

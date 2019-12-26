/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.CustomBet
{
    /// <summary>
    /// Provides an available selections for a particular event
    /// </summary>
    public interface IAvailableSelections
    {
        /// <summary>
        /// Gets the <see cref="URN"/> of the event
        /// </summary>
        URN Event { get; }

        /// <summary>
        /// Gets the list of markets for this event
        /// </summary>
        IEnumerable<IMarket> Markets { get; }
    }
}

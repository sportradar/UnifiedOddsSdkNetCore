/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet
{
    /// <summary>
    /// Provides an available selections for a particular event
    /// </summary>
    public interface IAvailableSelectionsFilter
    {
        /// <summary>
        /// Gets the <see cref="Urn"/> of the event
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Allowed - not to introduce breaking change")]
        Urn Event { get; }

        /// <summary>
        /// Gets the list of markets for this event
        /// </summary>
        IEnumerable<IMarketFilter> Markets { get; }
    }
}

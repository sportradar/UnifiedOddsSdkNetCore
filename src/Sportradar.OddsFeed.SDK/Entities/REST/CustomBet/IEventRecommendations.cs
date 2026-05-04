// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet
{
    /// <summary>
    /// Represents a single event recommendations entry in prebuilt bets response.
    /// </summary>
    public interface IEventRecommendations
    {
        /// <summary>
        /// Returns the event URN
        /// </summary>
        Urn EventId { get; }

        /// <summary>
        /// Returns the number of provided recommendations
        /// </summary>
        int ProvidedRecommendations { get; }

        /// <summary>
        /// Returns the source of the recommendations
        /// </summary>
        string Source { get; }

        /// <summary>
        /// Returns the list of recommendations
        /// </summary>
        IRecommendation[] Recommendations { get; }
    }
}

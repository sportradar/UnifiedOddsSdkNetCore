// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet
{
    /// <summary>
    /// Represents a request for prebuilt bet recommendations
    /// from the Custom Bet API.
    /// </summary>
    public interface IPrebuiltBetsRequest
    {
        /// <summary>
        /// Gets the event id for which prebuilt bets should be returned.
        /// </summary>
        Urn EventId { get; }

        /// <summary>
        /// Gets the sub-bookmaker id.
        /// </summary>
        int SubBookmakerId { get; }

        /// <summary>
        /// Gets the number of recommendations to request.
        /// </summary>
        int? Count { get; }

        /// <summary>
        /// Gets the requested selections length.
        /// </summary>
        int? Length { get; }

        /// <summary>
        /// Gets the user identifier.
        /// </summary>
        string User { get; }
    }
}

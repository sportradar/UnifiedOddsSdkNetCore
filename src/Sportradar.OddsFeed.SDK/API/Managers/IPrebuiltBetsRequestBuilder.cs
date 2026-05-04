// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;

namespace Sportradar.OddsFeed.SDK.Api.Managers
{
    /// <summary>
    /// Builder for creating <see cref="IPrebuiltBetsRequest"/> instances
    /// used to request prebuilt bets from the Custom Bet API.
    /// </summary>
    public interface IPrebuiltBetsRequestBuilder
    {
        /// <summary>
        /// Sets the number of recommendations to request.
        /// </summary>
        /// <param name="count">The requested recommendation count</param>
        /// <returns>The same builder instance</returns>
        IPrebuiltBetsRequestBuilder WithCount(int count);

        /// <summary>
        /// Sets the length of selections in requested recommendations.
        /// </summary>
        /// <param name="length">The requested length</param>
        /// <returns>The same builder instance</returns>
        IPrebuiltBetsRequestBuilder WithLength(int length);

        /// <summary>
        /// Sets the user value.
        /// </summary>
        /// <param name="user">The user identifier</param>
        /// <returns>The same builder instance</returns>
        IPrebuiltBetsRequestBuilder WithUser(string user);

        /// <summary>
        /// Sets the event identifier for which the prebuilt bets should be requested.
        /// </summary>
        /// <param name="eventId">The event identifier</param>
        /// <returns>The same builder instance</returns>
        IPrebuiltBetsRequestBuilder WithEvent(Urn eventId);

        /// <summary>
        /// Sets the sub-bookmaker identifier for which the prebuilt bets should be requested.
        /// </summary>
        /// <param name="subBookmakerId">The sub-bookmaker identifier</param>
        /// <returns>The same builder instance</returns>
        IPrebuiltBetsRequestBuilder WithSubBookmakerId(int subBookmakerId);

        /// <summary>
        /// Builds an immutable <see cref="IPrebuiltBetsRequest"/> instance
        /// using the values provided to this builder.
        /// </summary>
        /// <returns>
        /// A fully constructed <see cref="IPrebuiltBetsRequest"/> instance.
        /// </returns>
        IPrebuiltBetsRequest Build();
    }
}

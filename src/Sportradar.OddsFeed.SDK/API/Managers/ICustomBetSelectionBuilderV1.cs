// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;

namespace Sportradar.OddsFeed.SDK.Api.Managers
{
    /// <summary>
    /// Defines methods used to build selections
    /// </summary>
    public interface ICustomBetSelectionBuilderV1 : ICustomBetSelectionBuilder
    {
        /// <summary>
        /// Sets outcome odds to the provided value
        /// </summary>
        /// <param name="odds">A value representing the outcome odds</param>
        /// <returns>The <see cref="ICustomBetSelectionBuilder"/> instance used to set additional values</returns>
        ICustomBetSelectionBuilder SetOdds(double odds);

        /// <summary>
        /// Builds and returns a <see cref="ISelection"/> instance
        /// </summary>
        /// <param name="eventId">A <see cref="Urn"/> representing the event id</param>
        /// <param name="marketId">A value representing the market id</param>
        /// <param name="specifiers">A value representing the specifiers</param>
        /// <param name="outcomeId">A value representing the outcome id</param>
        /// <param name="odds">A odds value for the outcome</param>
        /// <returns>The constructed <see cref="ISelection"/> instance</returns>
        ISelection Build(Urn eventId, int marketId, string specifiers, string outcomeId, double? odds);
    }
}

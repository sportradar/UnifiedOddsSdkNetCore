// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;

namespace Sportradar.OddsFeed.SDK.Api.Managers
{
    /// <summary>
    /// Defines methods used to build selections
    /// </summary>
    public interface ICustomBetSelectionBuilder
    {
        /// <summary>
        /// Sets event id to the provided <see cref="Urn"/>
        /// </summary>
        /// <param name="eventId">A <see cref="Urn"/> representing the event id</param>
        /// <returns>The <see cref="ICustomBetSelectionBuilder"/> instance used to set additional values</returns>
        ICustomBetSelectionBuilder SetEventId(Urn eventId);

        /// <summary>
        /// Sets market id to the provided value
        /// </summary>
        /// <param name="marketId">A value representing the market id</param>
        /// <returns>The <see cref="ICustomBetSelectionBuilder"/> instance used to set additional values</returns>
        ICustomBetSelectionBuilder SetMarketId(int marketId);

        /// <summary>
        /// Sets specifiers to the provided value
        /// </summary>
        /// <param name="specifiers">A value representing the specifiers</param>
        /// <returns>The <see cref="ICustomBetSelectionBuilder"/> instance used to set additional values</returns>
        ICustomBetSelectionBuilder SetSpecifiers(string specifiers);

        /// <summary>
        /// Sets outcome id to the provided value
        /// </summary>
        /// <param name="outcomeId">A value representing the outcome id</param>
        /// <returns>The <see cref="ICustomBetSelectionBuilder"/> instance used to set additional values</returns>
        ICustomBetSelectionBuilder SetOutcomeId(string outcomeId);

        /// <summary>
        /// Builds and returns a <see cref="ISelection"/> instance
        /// </summary>
        /// <returns>The constructed <see cref="ISelection"/> instance</returns>
        ISelection Build();

        /// <summary>
        /// Builds and returns a <see cref="ISelection"/> instance
        /// </summary>
        /// <param name="eventId">A <see cref="Urn"/> representing the event id</param>
        /// <param name="marketId">A value representing the market id</param>
        /// <param name="specifiers">A value representing the specifiers</param>
        /// <param name="outcomeId">A value representing the outcome id</param>
        /// <param name="odds">A odds value for the outcome</param>
        /// <returns>The constructed <see cref="ISelection"/> instance</returns>
        ISelection Build(Urn eventId, int marketId, string specifiers, string outcomeId, double? odds = null);

        /// <summary>
        /// Sets outcome odds to the provided value
        /// </summary>
        /// <param name="odds">A value representing the outcome odds</param>
        /// <returns>The <see cref="ICustomBetSelectionBuilder"/> instance used to set additional values</returns>
        ICustomBetSelectionBuilder SetOdds(double odds);
    }
}

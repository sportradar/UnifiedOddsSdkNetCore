/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using Sportradar.OddsFeed.SDK.Entities.REST.CustomBet;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Defines methods used to build selections
    /// </summary>
    public interface ICustomBetSelectionBuilder
    {
        /// <summary>
        /// Sets event id to the provided <see cref="URN"/>
        /// </summary>
        /// <param name="eventId">A <see cref="URN"/> representing the event id.</param>
        /// <returns>The <see cref="ICustomBetSelectionBuilder"/> instance used to set additional values.</returns>
        ICustomBetSelectionBuilder SetEventId(URN eventId);

        /// <summary>
        /// Sets market id to the provided value
        /// </summary>
        /// <param name="marketId">A value representing the market id.</param>
        /// <returns>The <see cref="ICustomBetSelectionBuilder"/> instance used to set additional values.</returns>
        ICustomBetSelectionBuilder SetMarketId(int marketId);

        /// <summary>
        /// Sets specifiers to the provided value
        /// </summary>
        /// <param name="specifiers">A value representing the specifiers.</param>
        /// <returns>The <see cref="ICustomBetSelectionBuilder"/> instance used to set additional values.</returns>
        ICustomBetSelectionBuilder SetSpecifiers(string specifiers);

        /// <summary>
        /// Sets outcome id to the provided value
        /// </summary>
        /// <param name="outcomeId">A value representing the outcome id.</param>
        /// <returns>The <see cref="ICustomBetSelectionBuilder"/> instance used to set additional values.</returns>
        ICustomBetSelectionBuilder SetOutcomeId(string outcomeId);

        /// <summary>
        /// Builds and returns a <see cref="ISelection"/> instance
        /// </summary>
        /// <returns>The constructed <see cref="ISelection"/> instance.</returns>
        ISelection Build();

        /// <summary>
        /// Builds and returns a <see cref="ISelection"/> instance
        /// </summary>
        /// <returns>The constructed <see cref="ISelection"/> instance.</returns>
        ISelection Build(URN eventId, int marketId, string specifiers, string outcomeId);
    }
}

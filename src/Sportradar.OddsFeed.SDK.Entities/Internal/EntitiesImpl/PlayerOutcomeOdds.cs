/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents the odds for a player outcome
    /// </summary>
    /// <seealso cref="OutcomeOdds" />
    /// <seealso cref="IPlayerOutcomeOdds" />
    internal class PlayerOutcomeOdds : OutcomeOdds, IPlayerOutcomeOdds
    {
        /// <summary>
        /// A value indicating whether the player is associated with home or away team - 1 : HomeTeam, 2 : AwayTeam
        /// </summary>
        private readonly int _teamFlag;

        /// <summary>
        /// A <see cref="IMatch"/> representing the the match associated with the outcome / market
        /// </summary>
        private readonly IMatch _match;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerOutcomeOdds" /> class
        /// </summary>
        /// <param name="id">the value uniquely identifying the current <see cref="PlayerOutcomeOdds" /> instance</param>
        /// <param name="active">
        ///     a value indicating whether the current <see cref="OutcomeOdds" /> is active - i.e. should bets on
        ///     it be accepted
        /// </param>
        /// <param name="odds">the odds for the current <see cref="OutcomeOdds" /> instance</param>
        /// <param name="probabilities">the probabilities for the current <see cref="OutcomeOdds" /> instance</param>
        /// <param name="nameProvider">A <see cref="INameProvider"/> used to generate the outcome name(s)</param>
        /// <param name="mappingProvider">A <see cref="IMarketMappingProvider"/> instance used for providing mapping ids of markets and outcomes</param>
        /// <param name="match">A <see cref="IMatch"/> representing the the match associated with the outcome / market</param>
        /// <param name="teamFlag">A value indicating whether the player is associated with home or away team - 1 : HomeTeam, 2 : AwayTeam</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying languages the current instance supports</param>
        /// <param name="outcomeDefinition">The associated <see cref="IOutcomeDefinition"/></param>
        internal PlayerOutcomeOdds(string id,
                                   bool? active,
                                   double odds,
                                   double? probabilities,
                                   INameProvider nameProvider,
                                   IMarketMappingProvider mappingProvider,
                                   IMatch match,
                                   int teamFlag,
                                   IEnumerable<CultureInfo> cultures,
                                   IOutcomeDefinition outcomeDefinition)
            : base(id, active, odds, probabilities, nameProvider, mappingProvider, cultures, outcomeDefinition)
        {
            Contract.Requires(match != null);
            Contract.Requires(teamFlag >= 1 && teamFlag <= 2);

            _teamFlag = teamFlag;
            _match = match;
        }

        /// <summary>
        /// Asynchronously gets the team to which the associated player belongs to.
        /// </summary>
        /// <returns>A <see cref="Task{ITeamCompetitor}" /> representing the async operation.</returns>
        public Task<ITeamCompetitor> GetCompetitorAsync()
        {
            return _teamFlag == 1
                ? _match.GetHomeCompetitorAsync()
                : _match.GetAwayCompetitorAsync();
        }

        /// <summary>
        /// Gets the value indicating whether the associated team is home or away
        /// </summary>
        /// <value>The value indicating whether the associated team is home or away</value>
        public HomeAway HomeOrAwayTeam => _teamFlag == 1 ? HomeAway.Home : HomeAway.Away;
    }
}
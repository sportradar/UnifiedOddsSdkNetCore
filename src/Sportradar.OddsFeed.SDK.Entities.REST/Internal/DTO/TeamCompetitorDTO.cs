/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// Provides information about team competitor
    /// </summary>
    public class TeamCompetitorDTO : CompetitorDTO
    {
        /// <summary>
        /// Gets the competitor's qualifier
        /// </summary>
        public string Qualifier { get; }

        /// <summary>
        /// Gets the division
        /// </summary>
        /// <value>The division</value>
        public int? Division { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamCompetitorDTO"/> class
        /// </summary>
        /// <param name="record">A <see cref="teamCompetitor"/> containing information about the team</param>
        internal TeamCompetitorDTO(teamCompetitor record)
            : base(record)
        {
            Qualifier = record.qualifier;
            Division = record.divisionSpecified
                           ? record.division
                           : (int?) null;
        }
    }
}
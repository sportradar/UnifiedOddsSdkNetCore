// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// Provides information about team competitor
    /// </summary>
    internal class TeamCompetitorDto : CompetitorDto
    {
        /// <summary>
        /// Gets the competitor's qualifier
        /// </summary>
        public string Qualifier { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamCompetitorDto"/> class
        /// </summary>
        /// <param name="record">A <see cref="teamCompetitor"/> containing information about the team</param>
        internal TeamCompetitorDto(teamCompetitor record)
            : base(record)
        {
            Qualifier = record.qualifier;
        }
    }
}

// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object representing competitor's (simple team's) profile
    /// </summary>
    internal class SimpleTeamProfileDto
    {
        /// <summary>
        /// A <see cref="CompetitorDto"/> representing the competitor represented by the current profile
        /// </summary>
        public readonly CompetitorDto Competitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleTeamProfileDto"/> class
        /// </summary>
        /// <param name="record">A <see cref="simpleTeamProfileEndpoint"/> containing information about the profile</param>
        public SimpleTeamProfileDto(simpleTeamProfileEndpoint record)
        {
            Guard.Argument(record, nameof(record)).NotNull();
            Guard.Argument(record.competitor, nameof(record.competitor)).NotNull();

            Competitor = new CompetitorDto(record.competitor);
        }
    }
}

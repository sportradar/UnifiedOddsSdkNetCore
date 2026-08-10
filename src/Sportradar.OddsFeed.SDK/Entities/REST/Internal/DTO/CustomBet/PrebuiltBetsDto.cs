// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet
{
    /// <summary>
    /// Defines a data-transfer-object for prebuilt bets response
    /// </summary>
    internal class PrebuiltBetsDto
    {
        public PrebuiltBetsDto(PreBuiltBetsType prebuiltBets)
        {
            if (prebuiltBets.@event == null)
            {
                Events = Array.Empty<EventRecommendationsDto>();
            }
            else
            {
                Events = prebuiltBets.@event
                                    .Select(e => new EventRecommendationsDto(e))
                                    .ToArray();
            }
            RequestedRecommendations = prebuiltBets.requested_recommendations;
            GeneratedAt = prebuiltBets.generated_at;
        }

        public EventRecommendationsDto[] Events { get; }
        public int RequestedRecommendations { get; }
        public string GeneratedAt { get; }
    }
}

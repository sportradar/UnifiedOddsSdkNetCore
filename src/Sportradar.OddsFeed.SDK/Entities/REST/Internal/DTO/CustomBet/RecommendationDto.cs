// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet
{
    internal class RecommendationDto
    {
        public RecommendationDto(RecommendationsType recommendation)
        {
            Selections = (recommendation.selection ?? Array.Empty<PreBuiltBetsSelectionType>())
                         .Select(s => new PrebuiltBetSelectionDto(s))
                         .ToArray();
            Odds = recommendation.odds;
            Probability = recommendation.probability;
        }

        public PrebuiltBetSelectionDto[] Selections { get; }
        public double Odds { get; }
        public double Probability { get; }
    }
}

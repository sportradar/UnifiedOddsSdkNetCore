// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl.CustomBet
{
    internal class Recommendation : IRecommendation
    {
        public Recommendation(RecommendationDto recommendation)
        {
            Selections = recommendation.Selections.Select(s => new PrebuiltBetSelection(s)).ToArray<IPrebuiltBetSelection>();
            Odds = recommendation.Odds;
            Probability = recommendation.Probability;
        }

        public IPrebuiltBetSelection[] Selections { get; }
        public double Odds { get; }
        public double Probability { get; }
    }
}

// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Linq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl.CustomBet
{
    internal class EventRecommendations : IEventRecommendations
    {
        public EventRecommendations(EventRecommendationsDto eventRecommendations)
        {
            EventId = eventRecommendations.EventId;
            ProvidedRecommendations = eventRecommendations.ProvidedRecommendations;
            Source = eventRecommendations.Source;
            Recommendations = eventRecommendations.Recommendations.Select(r => new Recommendation(r)).ToArray<IRecommendation>();
        }

        public Urn EventId { get; }
        public int ProvidedRecommendations { get; }
        public string Source { get; }
        public IRecommendation[] Recommendations { get; }
    }
}

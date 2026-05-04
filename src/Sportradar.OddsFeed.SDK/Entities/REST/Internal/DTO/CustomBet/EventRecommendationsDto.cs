// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet
{
    internal class EventRecommendationsDto
    {
        public EventRecommendationsDto(EventRecommendationsType eventRecommendations)
        {
            EventId = Urn.Parse(eventRecommendations.id);
            ProvidedRecommendations = eventRecommendations.provided_recommendation;
            Source = eventRecommendations.source;
            Recommendations = (eventRecommendations.recommendations ?? Array.Empty<RecommendationsType>())
                             .Select(r => new RecommendationDto(r))
                             .ToArray();
        }

        public Urn EventId { get; }
        public int ProvidedRecommendations { get; }
        public string Source { get; }
        public RecommendationDto[] Recommendations { get; }
    }
}

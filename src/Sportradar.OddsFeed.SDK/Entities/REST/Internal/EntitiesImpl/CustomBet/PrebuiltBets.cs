// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl.CustomBet
{
    /// <summary>
    /// Implements methods used to access prebuilt bets recommendations
    /// </summary>
    internal class PrebuiltBets : IPrebuiltBets
    {
        public PrebuiltBets(PrebuiltBetsDto prebuiltBetsDto)
        {
            Events = prebuiltBetsDto.Events.Select(e => new EventRecommendations(e)).ToArray<IEventRecommendations>();
            RequestedRecommendations = prebuiltBetsDto.RequestedRecommendations;
            GeneratedAt = SdkInfo.ParseDate(prebuiltBetsDto.GeneratedAt);
        }

        public IEventRecommendations[] Events { get; }
        public int RequestedRecommendations { get; }
        public DateTime? GeneratedAt { get; }
    }
}

// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl.CustomBet
{
    internal sealed class PrebuiltBetsRequest : IPrebuiltBetsRequest
    {
        public Urn EventId { get; }
        public int SubBookmakerId { get; }
        public int? Count { get; }
        public int? Length { get; }
        public string User { get; }

        public PrebuiltBetsRequest(Urn eventId, int subBookmakerId, int? count, int? length, string user)
        {
            EventId = eventId;
            SubBookmakerId = subBookmakerId;
            Count = count;
            Length = length;
            User = user;
        }
    }
}

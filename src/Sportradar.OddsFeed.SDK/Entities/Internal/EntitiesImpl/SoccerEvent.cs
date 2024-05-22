// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    internal class SoccerEvent : Match, ISoccerEvent
    {
        public SoccerEvent(Urn id,
                            Urn sportId,
                            ISportEntityFactory sportEntityFactory,
                            ISportEventCache sportEventCache,
                            ISportEventStatusCache sportEventStatusCache,
                            ILocalizedNamedValueCache matchStatusCache,
                            IReadOnlyCollection<CultureInfo> cultures,
                            ExceptionHandlingStrategy exceptionStrategy)
            : base(id, sportId, sportEntityFactory, sportEventCache, sportEventStatusCache, matchStatusCache, cultures, exceptionStrategy)
        {
        }

        public new async Task<ISoccerStatus> GetStatusAsync()
        {
            var item = await base.GetStatusAsync().ConfigureAwait(false);

            return item == null ? null : new SoccerStatus(((CompetitionStatus)item).SportEventStatusCacheItem, MatchStatusCache);
        }
    }
}

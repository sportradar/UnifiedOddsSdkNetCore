/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    internal class SoccerEvent : Match, ISoccerEvent
    {
        public SoccerEvent(URN id,
                            URN sportId,
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

            return item == null ? null : new SoccerStatus(((CompetitionStatus)item).SportEventStatusCI, MatchStatusCache);
        }
    }
}

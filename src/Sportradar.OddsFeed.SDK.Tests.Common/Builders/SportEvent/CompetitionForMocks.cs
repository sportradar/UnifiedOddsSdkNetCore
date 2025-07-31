// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Tests.Common.Helpers.Stubs;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.SportEvent;

internal class CompetitionForMocks : Competition
{
    private CompetitionForMocks(ILogger executionLog,
                                Urn id,
                                Urn sportId,
                                ISportEntityFactory sportEntityFactory,
                                ISportEventStatusCache sportEventStatusCache,
                                ISportEventCache sportEventCache,
                                IReadOnlyCollection<CultureInfo> cultures,
                                ExceptionHandlingStrategy exceptionStrategy,
                                ILocalizedNamedValueCache matchStatusesCache)
        : base(executionLog, id, sportId, sportEntityFactory, sportEventStatusCache, sportEventCache, cultures, exceptionStrategy, matchStatusesCache)
    {
    }

    // Required for mocking purposes
    // ReSharper disable once UnusedMember.Global
    public CompetitionForMocks()
        : this(NullLogger.Instance,
            TestConsts.AnyMatchId,
            TestConsts.AnySportId,
            NullSportEntityFactory.Instance,
            NullSportEventStatusCache.Instance,
            NullSportEventCache.Instance,
            TestConsts.Cultures1,
            ExceptionHandlingStrategy.Throw,
            NullLocalizedNamedValueCache.Instance)
    {
    }
}

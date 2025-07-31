// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Helpers.Stubs;

internal class NullSportEntityFactory : ISportEntityFactory
{
    public static NullSportEntityFactory Instance { get; } = new NullSportEntityFactory();

    public Task<IEnumerable<ISport>> BuildSportsAsync(IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy)
    {
        throw new NotImplementedException();
    }

    public Task<ISport> BuildSportAsync(Urn sportId, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy)
    {
        throw new NotImplementedException();
    }

    public Task<IPlayer> BuildPlayerAsync(Urn playerId, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<IPlayer>> BuildPlayersAsync(IReadOnlyCollection<Urn> playersIds, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy, IDictionary<Urn, int> playersJerseyNumbers)
    {
        throw new NotImplementedException();
    }

    public T BuildSportEvent<T>(Urn eventId, Urn sportId, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy) where T : ISportEvent
    {
        throw new NotImplementedException();
    }

    public ICompetitor BuildCompetitor(CompetitorCacheItem competitorCacheItem, IReadOnlyCollection<CultureInfo> cultures, ICompetitionCacheItem rootCompetitionCacheItem, ExceptionHandlingStrategy exceptionStrategy)
    {
        throw new NotImplementedException();
    }

    public ICompetitor BuildCompetitor(Urn competitorId, IReadOnlyCollection<CultureInfo> cultures, IDictionary<Urn, ReferenceIdCacheItem> competitorsReferences, ExceptionHandlingStrategy exceptionStrategy)
    {
        throw new NotImplementedException();
    }

    public ICompetitor BuildCompetitor(CompetitorCacheItem competitorCacheItem, IReadOnlyCollection<CultureInfo> cultures, IDictionary<Urn, ReferenceIdCacheItem> competitorsReferences, ExceptionHandlingStrategy exceptionStrategy)
    {
        throw new NotImplementedException();
    }

    public ITeamCompetitor BuildTeamCompetitor(TeamCompetitorCacheItem teamCompetitorCacheItem, IReadOnlyCollection<CultureInfo> cultures, ICompetitionCacheItem rootCompetitionCacheItem, ExceptionHandlingStrategy exceptionStrategy)
    {
        throw new NotImplementedException();
    }

    public Task<ICompetitor> BuildCompetitorAsync(Urn competitorId, IReadOnlyCollection<CultureInfo> cultures, ICompetitionCacheItem rootCompetitionCacheItem, ExceptionHandlingStrategy exceptionStrategy)
    {
        throw new NotImplementedException();
    }

    public Task<ICompetitor> BuildCompetitorAsync(Urn competitorId, IReadOnlyCollection<CultureInfo> cultures, IDictionary<Urn, ReferenceIdCacheItem> competitorsReferences, ExceptionHandlingStrategy exceptionStrategy)
    {
        throw new NotImplementedException();
    }

    public Task<ITeamCompetitor> BuildTeamCompetitorAsync(Urn teamCompetitorId, IReadOnlyCollection<CultureInfo> cultures, ICompetitionCacheItem rootCompetitionCacheItem, ExceptionHandlingStrategy exceptionStrategy)
    {
        throw new NotImplementedException();
    }

    public Task<ICategorySummary> BuildCategoryAsync(Urn categoryId, IReadOnlyCollection<CultureInfo> cultures)
    {
        throw new NotImplementedException();
    }
}

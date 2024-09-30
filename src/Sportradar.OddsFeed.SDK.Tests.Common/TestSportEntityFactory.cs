// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit.Abstractions;

// ReSharper disable NotAccessedField.Local

namespace Sportradar.OddsFeed.SDK.Tests.Common;

internal class TestSportEntityFactory : ISportEntityFactory
{
    private readonly ITestOutputHelper _outputHelper;

    [SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Allowed")]
    private readonly ISportDataCache _sportDataCache;
    [SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Allowed")]
    private readonly ISportEventStatusCache _eventStatusCache;
    [SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Allowed")]
    private readonly ILocalizedNamedValueCache _matchStatusCache;
    private readonly IProfileCache _profileCache;
    [SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Allowed")]
    private readonly IReadOnlyCollection<Urn> _soccerSportUrns;

    private readonly TestCacheStoreManager _testCacheStoreManager;

    public TestSportEntityFactory(ITestOutputHelper outputHelper,
        ISportDataCache sportDataCache = null,
        ISportEventCache sportEventCache = null,
        ISportEventStatusCache eventStatusCache = null,
        ILocalizedNamedValueCache matchStatusCache = null,
        IProfileCache profileCache = null,
        IReadOnlyCollection<Urn> soccerSportUrns = null)
    {
        var loggerFactory = new XunitLoggerFactory(outputHelper);

        _testCacheStoreManager = new TestCacheStoreManager();
        var profileMemoryCache = _testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForProfileCache);

        _outputHelper = outputHelper;
        _sportDataCache = sportDataCache;
        _eventStatusCache = eventStatusCache;
        _matchStatusCache = matchStatusCache;
        var dataRouterManager = new TestDataRouterManager(_testCacheStoreManager.CacheManager, outputHelper);
        _profileCache = profileCache ?? new ProfileCache(profileMemoryCache, dataRouterManager, _testCacheStoreManager.CacheManager, sportEventCache, loggerFactory);
        _soccerSportUrns = soccerSportUrns ?? SdkInfo.SoccerSportUrns;
    }

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
        var playerNames = cultures.ToDictionary(culture => culture, culture => $"PlayerName {culture.TwoLetterISOLanguageName}");
        var player = new Player(playerId, playerNames);
        return Task.FromResult((IPlayer)player);
    }

    public async Task<IEnumerable<IPlayer>> BuildPlayersAsync(IReadOnlyCollection<Urn> playersIds, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy, IDictionary<Urn, int> playersJerseyNumbers)
    {
        if (playersIds.IsNullOrEmpty())
        {
            return new List<IPlayer>();
        }

        var players = new List<IPlayer>();
        foreach (var playersId in playersIds)
        {
            var playerCi = await _profileCache.GetPlayerProfileAsync(playersId, cultures, true).ConfigureAwait(false);
            if (playerCi != null)
            {
                players.Add(new Player(playerCi.Id, playerCi.Names));
            }
            else
            {
                _outputHelper.WriteLine($"No player for {playersId}");
            }
        }
        return players;
    }

    public async Task<IEnumerable<ICompetitorPlayer>> BuildCompetitorPlayersAsync(IReadOnlyCollection<Urn> playersIds, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy, IDictionary<Urn, int> playersJerseyNumbers)
    {
        if (playersIds.IsNullOrEmpty())
        {
            return new List<ICompetitorPlayer>();
        }

        var players = new List<ICompetitorPlayer>();
        foreach (var playersId in playersIds)
        {
            var playerCi = await _profileCache.GetPlayerProfileAsync(playersId, cultures, true).ConfigureAwait(false);
            if (playerCi != null)
            {
                var playerJerseyNumber = playersJerseyNumbers.TryGetValue(playerCi.Id, out var jerseyNumber) ? jerseyNumber : (int?)null;
                players.Add(new CompetitorPlayer(playerCi, cultures, playerJerseyNumber));
            }
            else
            {
                _outputHelper.WriteLine($"No player for {playersId}");
            }
        }
        return players;
    }

    public T BuildSportEvent<T>(Urn eventId, Urn sportId, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy) where T : ISportEvent
    {
        ICompetition competition;
        switch (eventId.TypeGroup)
        {
            case ResourceTypeGroup.Match:
                {
                    competition = new TestMatch(eventId);
                    break;
                }
            case ResourceTypeGroup.Stage:
                {
                    competition = new TestStage(eventId);
                    break;
                }
            case ResourceTypeGroup.Tournament:
            case ResourceTypeGroup.BasicTournament:
            case ResourceTypeGroup.Season:
            case ResourceTypeGroup.Other:
            case ResourceTypeGroup.Unknown:
            case ResourceTypeGroup.Draw:
            case ResourceTypeGroup.Lottery:
            default:
                {
                    throw new ArgumentException($"ResourceTypeGroup '{eventId.TypeGroup}' is not supported.", nameof(eventId));
                }
        }
        return (T)competition;
    }

    public ICompetitor BuildCompetitor(Urn competitorId, IReadOnlyCollection<CultureInfo> cultures, IDictionary<Urn, ReferenceIdCacheItem> competitorsReferences,
        ExceptionHandlingStrategy exceptionStrategy)
    {
        return new Competitor(competitorId, _profileCache, cultures, this, exceptionStrategy, competitorsReferences);
    }

    public ICompetitor BuildCompetitor(CompetitorCacheItem competitorCacheItem, IReadOnlyCollection<CultureInfo> cultures, ICompetitionCacheItem rootCompetitionCacheItem, ExceptionHandlingStrategy exceptionStrategy)
    {
        return new Competitor(competitorCacheItem, _profileCache, cultures, this, exceptionStrategy, rootCompetitionCacheItem);
    }

    public ICompetitor BuildCompetitor(CompetitorCacheItem competitorCacheItem, IReadOnlyCollection<CultureInfo> cultures, IDictionary<Urn, ReferenceIdCacheItem> competitorsReferences, ExceptionHandlingStrategy exceptionStrategy)
    {
        return new Competitor(competitorCacheItem, _profileCache, cultures, this, exceptionStrategy, competitorsReferences);
    }

    public ITeamCompetitor BuildTeamCompetitor(TeamCompetitorCacheItem teamCompetitorCacheItem, IReadOnlyCollection<CultureInfo> cultures, ICompetitionCacheItem rootCompetitionCacheItem, ExceptionHandlingStrategy exceptionStrategy)
    {
        var teamCompetitor = new TeamCompetitor(teamCompetitorCacheItem, cultures, this, exceptionStrategy, _profileCache, rootCompetitionCacheItem);
        return teamCompetitor;
    }

    /// <summary>
    /// Builds the instance of the <see cref="ICompetitor"/> class
    /// </summary>
    /// <param name="competitorId">A <see cref="Urn"/> of the <see cref="CompetitorCacheItem"/> used to create new instance</param>
    /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCacheItem"/></param>
    /// <param name="rootCompetitionCacheItem">A root <see cref="CompetitionCacheItem"/> to which this competitor belongs to</param>
    /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
    /// <returns>The constructed <see cref="ICompetitor"/> instance</returns>
    public Task<ICompetitor> BuildCompetitorAsync(Urn competitorId, IReadOnlyCollection<CultureInfo> cultures, ICompetitionCacheItem rootCompetitionCacheItem, ExceptionHandlingStrategy exceptionStrategy)
    {
        var competitor = new Competitor(competitorId, _profileCache, cultures, this, exceptionStrategy, rootCompetitionCacheItem.GetCompetitorsReferencesAsync().GetAwaiter().GetResult());
        return Task.FromResult((ICompetitor)competitor);
    }

    /// <summary>
    /// Builds the instance of the <see cref="ICompetitor"/> class
    /// </summary>
    /// <param name="competitorId">A <see cref="Urn"/> of the <see cref="CompetitorCacheItem"/> used to create new instance</param>
    /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCacheItem"/></param>
    /// <param name="competitorsReferences">The dictionary of competitor references (associated with specific match)</param>
    /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
    /// <returns>The constructed <see cref="ICompetitor"/> instance</returns>
    public Task<ICompetitor> BuildCompetitorAsync(Urn competitorId, IReadOnlyCollection<CultureInfo> cultures, IDictionary<Urn, ReferenceIdCacheItem> competitorsReferences, ExceptionHandlingStrategy exceptionStrategy)
    {
        var competitor = new Competitor(competitorId, _profileCache, cultures, this, exceptionStrategy, competitorsReferences);
        return Task.FromResult((ICompetitor)competitor);
    }

    /// <summary>
    /// Builds the instance of the <see cref="ITeamCompetitor"/> class
    /// </summary>
    /// <param name="teamCompetitorId">A <see cref="Urn"/> of the <see cref="TeamCompetitorCacheItem"/> used to create new instance</param>
    /// <param name="cultures">A cultures of the current instance of <see cref="TeamCompetitorCacheItem"/></param>
    /// <param name="rootCompetitionCacheItem">A root <see cref="CompetitionCacheItem"/> to which this competitor belongs to</param>
    /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
    /// <returns>The constructed <see cref="ITeamCompetitor"/> instance</returns>
    public Task<ITeamCompetitor> BuildTeamCompetitorAsync(Urn teamCompetitorId, IReadOnlyCollection<CultureInfo> cultures, ICompetitionCacheItem rootCompetitionCacheItem, ExceptionHandlingStrategy exceptionStrategy)
    {
        var teamCompetitor = MessageFactorySdk.GetTeamCompetitor((int)teamCompetitorId.Id);
        return Task.FromResult(teamCompetitor);
    }

    /// <summary>
    /// Builds the instance of the <see cref="ICategorySummary"/> class
    /// </summary>
    /// <param name="categoryId">A <see cref="Urn"/> of the <see cref="ICategorySummary"/> used to create new instance</param>
    /// <param name="cultures">A culture of the current instance of <see cref="ICategorySummary"/></param>
    /// <returns>The constructed <see cref="ITeamCompetitor"/> instance</returns>
    public Task<ICategorySummary> BuildCategoryAsync(Urn categoryId, IReadOnlyCollection<CultureInfo> cultures)
    {
        var category = MessageFactorySdk.GetCategory((int)categoryId.Id);
        var categorySummary = new CategorySummary(category.Id, category.Names, category.CountryCode);
        return Task.FromResult((ICategorySummary)categorySummary);
    }
}

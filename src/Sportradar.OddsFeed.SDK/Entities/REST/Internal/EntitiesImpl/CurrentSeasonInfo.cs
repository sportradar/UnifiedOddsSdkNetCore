// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    internal class CurrentSeasonInfo : ICurrentSeasonInfo
    {
        public Urn Id { get; }
        public IReadOnlyDictionary<CultureInfo, string> Names { get; }
        public string Year { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public string GetName(CultureInfo culture)
        {
            Names.TryGetValue(culture, out var result);
            return result;
        }
        public ISeasonCoverage Coverage { get; }
        public IEnumerable<IGroup> Groups { get; }
        public IRound CurrentRound { get; }
        public IEnumerable<ICompetitor> Competitors { get; }
        public IEnumerable<ISportEvent> Schedule { get; }

        public CurrentSeasonInfo(CurrentSeasonInfoCacheItem cacheItem,
                                 IReadOnlyCollection<CultureInfo> cultures,
                                 ISportEntityFactory sportEntityFactory,
                                 ExceptionHandlingStrategy exceptionHandlingStrategy,
                                 IDictionary<Urn, ReferenceIdCacheItem> competitorsReferenceIds)
        {
            Guard.Argument(cacheItem, nameof(cacheItem)).NotNull();

            Id = cacheItem.Id;
            Names = cacheItem.Name as IReadOnlyDictionary<CultureInfo, string>;
            Year = cacheItem.Year;
            StartDate = cacheItem.StartDate;
            EndDate = cacheItem.EndDate;
            Coverage = cacheItem.SeasonCoverage == null
                           ? null
                           : new SeasonCoverage(cacheItem.SeasonCoverage);
            Groups = cacheItem.Groups == null
                         ? null
                         : cacheItem.Groups.Select(s => new Group(s, cultures, sportEntityFactory, exceptionHandlingStrategy, competitorsReferenceIds));
            CurrentRound = cacheItem.CurrentRound == null
                               ? null
                               : new Round(cacheItem.CurrentRound, cultures);
            Competitors = cacheItem.CompetitorsIds == null
                              ? null
                              : cacheItem.CompetitorsIds.Select(s => sportEntityFactory.BuildCompetitor(s, cultures, competitorsReferenceIds, exceptionHandlingStrategy));
            Schedule = cacheItem.Schedule == null
                           ? null
                           : cacheItem.Schedule.Select(s => sportEntityFactory.BuildSportEvent<ISportEvent>(s, null, cultures, exceptionHandlingStrategy));
        }

        public CurrentSeasonInfo(ITournamentInfoCacheItem currentSeasonCacheItem,
                                 IReadOnlyCollection<CultureInfo> cultures,
                                 ISportEntityFactory sportEntityFactory,
                                 ExceptionHandlingStrategy exceptionStrategy,
                                 IDictionary<Urn, ReferenceIdCacheItem> competitorsReferenceIds)
        {
            Id = currentSeasonCacheItem.Id;
            Names = currentSeasonCacheItem.GetNamesAsync(cultures).GetAwaiter().GetResult();
            Year = currentSeasonCacheItem.GetYearAsync().GetAwaiter().GetResult();
            StartDate = currentSeasonCacheItem.GetScheduledAsync().GetAwaiter().GetResult() ?? DateTime.MinValue;
            EndDate = currentSeasonCacheItem.GetScheduledEndAsync().GetAwaiter().GetResult() ?? DateTime.MinValue;
            Coverage = currentSeasonCacheItem.GetSeasonCoverageAsync().GetAwaiter().GetResult() == null
                           ? null
                           : new SeasonCoverage(currentSeasonCacheItem.GetSeasonCoverageAsync().GetAwaiter().GetResult());
            Groups = currentSeasonCacheItem.GetGroupsAsync(cultures).GetAwaiter().GetResult() == null
                         ? null
                         : currentSeasonCacheItem.GetGroupsAsync(cultures).GetAwaiter().GetResult().Select(s => new Group(s, cultures, sportEntityFactory, exceptionStrategy, competitorsReferenceIds));
            CurrentRound = currentSeasonCacheItem.GetCurrentRoundAsync(cultures).GetAwaiter().GetResult() == null
                               ? null
                               : new Round(currentSeasonCacheItem.GetCurrentRoundAsync(cultures).GetAwaiter().GetResult(), cultures);
            Competitors = currentSeasonCacheItem.GetCompetitorsIdsAsync(cultures).GetAwaiter().GetResult() == null
                              ? null
                              : currentSeasonCacheItem.GetCompetitorsIdsAsync(cultures).GetAwaiter().GetResult().Select(s => sportEntityFactory.BuildCompetitor(s, cultures, competitorsReferenceIds, exceptionStrategy));
            Schedule = currentSeasonCacheItem.GetScheduleAsync(cultures).GetAwaiter().GetResult() == null
                           ? null
                           : currentSeasonCacheItem.GetScheduleAsync(cultures)
                                                   .GetAwaiter()
                                                   .GetResult()
                                                   .Select(s => sportEntityFactory.BuildSportEvent<ISportEvent>(s, currentSeasonCacheItem.GetSportIdAsync().GetAwaiter().GetResult(), cultures, exceptionStrategy));
        }
    }
}

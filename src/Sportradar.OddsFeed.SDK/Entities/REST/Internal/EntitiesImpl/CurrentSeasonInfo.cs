/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    internal class CurrentSeasonInfo : ICurrentSeasonInfo
    {
        public URN Id { get; }
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

        public CurrentSeasonInfo(CurrentSeasonInfoCI cacheItem,
                                 IReadOnlyCollection<CultureInfo> cultures,
                                 ISportEntityFactory sportEntityFactory,
                                 ExceptionHandlingStrategy exceptionHandlingStrategy,
                                 IDictionary<URN, ReferenceIdCI> competitorsReferenceIds)
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

        public CurrentSeasonInfo(ITournamentInfoCI currentSeasonCI,
                                 IReadOnlyCollection<CultureInfo> cultures,
                                 ISportEntityFactory sportEntityFactory,
                                 ExceptionHandlingStrategy exceptionStrategy,
                                 IDictionary<URN, ReferenceIdCI> competitorsReferenceIds)
        {
            Id = currentSeasonCI.Id;
            Names = currentSeasonCI.GetNamesAsync(cultures).Result;
            Year = currentSeasonCI.GetYearAsync().Result;
            StartDate = currentSeasonCI.GetScheduledAsync().Result ?? DateTime.MinValue;
            EndDate = currentSeasonCI.GetScheduledEndAsync().Result ?? DateTime.MinValue;
            Coverage = currentSeasonCI.GetSeasonCoverageAsync().Result == null
                ? null
                : new SeasonCoverage(currentSeasonCI.GetSeasonCoverageAsync().Result);
            Groups = currentSeasonCI.GetGroupsAsync(cultures).Result == null
                ? null
                : currentSeasonCI.GetGroupsAsync(cultures).Result.Select(s => new Group(s, cultures, sportEntityFactory, exceptionStrategy, competitorsReferenceIds));
            CurrentRound = currentSeasonCI.GetCurrentRoundAsync(cultures).Result == null
                ? null
                : new Round(currentSeasonCI.GetCurrentRoundAsync(cultures).Result, cultures);
            Competitors = currentSeasonCI.GetCompetitorsIdsAsync(cultures).Result == null
                ? null
                : currentSeasonCI.GetCompetitorsIdsAsync(cultures).Result.Select(s => sportEntityFactory.BuildCompetitor(s, cultures, competitorsReferenceIds, exceptionStrategy));
            Schedule = currentSeasonCI.GetScheduleAsync(cultures).Result == null
                ? null
                : currentSeasonCI.GetScheduleAsync(cultures).Result.Select(s => sportEntityFactory.BuildSportEvent<ISportEvent>(s, currentSeasonCI.GetSportIdAsync().Result, cultures, exceptionStrategy));
        }
    }
}

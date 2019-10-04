/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    public class CurrentSeasonInfo : ICurrentSeasonInfo
    {
        public URN Id { get; }
        public IReadOnlyDictionary<CultureInfo, string> Names { get; }
        public string Year { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public string GetName(CultureInfo culture)
        {
            string result;
            Names.TryGetValue(culture, out result);
            return result;
        }
        public ISeasonCoverage Coverage { get; }
        public IEnumerable<IGroup> Groups { get; }
        public IRound CurrentRound { get; }
        public IEnumerable<ICompetitor> Competitors { get; }
        public IEnumerable<ISportEvent> Schedule { get; }

        public CurrentSeasonInfo(CurrentSeasonInfoCI cacheItem,
                                 IEnumerable<CultureInfo> cultures,
                                 ISportEntityFactory sportEntityFactory,
                                 ExceptionHandlingStrategy exceptionHandlingStrategy,
                                 IDictionary<URN, ReferenceIdCI> competitorsReferenceIds)
        {
            Contract.Requires(cacheItem != null);
            //Contract.Requires(sportEntityFactory != null);

            var cultureInfos = cultures as IList<CultureInfo> ?? cultures.ToList();
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
                : cacheItem.Groups.Select(s => new Group(s, cultureInfos, sportEntityFactory, competitorsReferenceIds));
            CurrentRound = cacheItem.CurrentRound == null
                ? null
                : new Round(cacheItem.CurrentRound, cultureInfos);
            Competitors = cacheItem.Competitors == null
                ? null
                : cacheItem.Competitors.Select(s => sportEntityFactory.BuildCompetitor(s, cultureInfos, competitorsReferenceIds));
            Schedule = cacheItem.Schedule == null
                ? null
                : cacheItem.Schedule.Select(s => sportEntityFactory.BuildSportEvent<ISportEvent>(s, null, cultureInfos, exceptionHandlingStrategy));
        }

        public CurrentSeasonInfo(ITournamentInfoCI currentSeasonCI,
                                 IEnumerable<CultureInfo> cultures,
                                 ISportEntityFactory _sportEntityFactory,
                                 ExceptionHandlingStrategy exceptionStrategy,
                                 IDictionary<URN, ReferenceIdCI> competitorsReferenceIds)
        {
            var cultureInfos = cultures as IList<CultureInfo> ?? cultures.ToList();
            Id = currentSeasonCI.Id;
            Names = currentSeasonCI.GetNamesAsync(cultureInfos).Result;
            Year = currentSeasonCI.GetYearAsync().Result;
            StartDate = currentSeasonCI.GetScheduledAsync().Result ?? DateTime.MinValue;
            EndDate = currentSeasonCI.GetScheduledEndAsync().Result ?? DateTime.MinValue;
            Coverage = currentSeasonCI.GetSeasonCoverageAsync().Result == null
                ? null
                : new SeasonCoverage(currentSeasonCI.GetSeasonCoverageAsync().Result);
            Groups = currentSeasonCI.GetGroupsAsync(cultureInfos).Result == null
                ? null
                : currentSeasonCI.GetGroupsAsync(cultureInfos).Result.Select(s => new Group(s, cultureInfos, _sportEntityFactory, competitorsReferenceIds));
            CurrentRound = currentSeasonCI.GetCurrentRoundAsync(cultureInfos).Result == null
                ? null
                : new Round(currentSeasonCI.GetCurrentRoundAsync(cultureInfos).Result, cultureInfos);
            Competitors = currentSeasonCI.GetCompetitorsAsync(cultureInfos).Result == null
                ? null
                : currentSeasonCI.GetCompetitorsAsync(cultureInfos).Result.Select(s => _sportEntityFactory.BuildCompetitor(s, cultureInfos, competitorsReferenceIds));
            Schedule = currentSeasonCI.GetScheduleAsync(cultureInfos).Result == null
                ? null
                : currentSeasonCI.GetScheduleAsync(cultureInfos).Result.Select(s => _sportEntityFactory.BuildSportEvent<ISportEvent>(s, null, cultureInfos, exceptionStrategy));
        }
    }
}

/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Test.Shared
{
    public class TestSportEventFactory : ISportEntityFactory
    {
        public Task<IEnumerable<ISport>> BuildSportsAsync(IEnumerable<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy)
        {
            throw new NotImplementedException();
        }

        public Task<ISport> BuildSportAsync(URN id, IEnumerable<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy)
        {
            throw new NotImplementedException();
        }

        public Task<IPlayer> BuildPlayerAsync(URN id, IEnumerable<CultureInfo> cultures)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IPlayer>> BuildPlayersAsync(IEnumerable<URN> ids, IEnumerable<CultureInfo> cultures)
        {
            throw new NotImplementedException();
        }

        public T BuildSportEvent<T>(URN id, URN sportId, IEnumerable<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy) where T : ISportEvent
        {
            ICompetition competition;
            switch (id.TypeGroup)
            {
                case ResourceTypeGroup.MATCH:
                {
                    competition = new TestMatch(id);
                    break;
                }
                case ResourceTypeGroup.STAGE:
                {
                    competition = new TestStage(id);
                    break;
                }
                default:
                    throw new ArgumentException($"ResourceTypeGroup '{id.TypeGroup}' is not supported.", nameof(id));

            }
            return (T)competition;
        }

        public ICompetitor BuildCompetitor(CompetitorCI ci, IEnumerable<CultureInfo> cultures, ICompetitionCI rootCompetitionCI)
        {
            return new Competitor(ci, null, cultures, this, rootCompetitionCI);
        }

        public ICompetitor BuildCompetitor(CompetitorCI ci, IEnumerable<CultureInfo> cultures, IDictionary<URN, ReferenceIdCI> competitorsReferences)
        {
            return new Competitor(ci, null, cultures, this, competitorsReferences);
        }

        public ITeamCompetitor BuildTeamCompetitor(TeamCompetitorCI ci, IEnumerable<CultureInfo> culture, ICompetitionCI rootCompetitionCI)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds the instance of the <see cref="ICompetitor"/> class
        /// </summary>
        /// <param name="competitorId">A <see cref="URN"/> of the <see cref="CompetitorCI"/> used to create new instance</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCI"/></param>
        /// <param name="rootCompetitionCI">A root <see cref="CompetitionCI"/> to which this competitor belongs to</param>
        /// <returns>The constructed <see cref="ICompetitor"/> instance</returns>
        public Task<ICompetitor> BuildCompetitorAsync(URN competitorId, IEnumerable<CultureInfo> cultures, ICompetitionCI rootCompetitionCI)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds the instance of the <see cref="ICompetitor"/> class
        /// </summary>
        /// <param name="competitorId">A <see cref="URN"/> of the <see cref="CompetitorCI"/> used to create new instance</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCI"/></param>
        /// <param name="competitorsReferences">The dictionary of competitor references (associated with specific match)</param>
        /// <returns>The constructed <see cref="ICompetitor"/> instance</returns>
        public Task<ICompetitor> BuildCompetitorAsync(URN competitorId, IEnumerable<CultureInfo> cultures, IDictionary<URN, ReferenceIdCI> competitorsReferences)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds the instance of the <see cref="ITeamCompetitor"/> class
        /// </summary>
        /// <param name="teamCompetitorId">A <see cref="URN"/> of the <see cref="TeamCompetitorCI"/> used to create new instance</param>
        /// <param name="culture">A culture of the current instance of <see cref="TeamCompetitorCI"/></param>
        /// <param name="rootCompetitionCI">A root <see cref="CompetitionCI"/> to which this competitor belongs to</param>
        /// <returns>The constructed <see cref="ITeamCompetitor"/> instance</returns>
        public Task<ITeamCompetitor> BuildTeamCompetitorAsync(URN teamCompetitorId, IEnumerable<CultureInfo> culture, ICompetitionCI rootCompetitionCI)
        {
            throw new NotImplementedException();
        }
    }

    internal class TestSportEventStatusMapper : SportEventStatusMapperBase
    {
        public static ISportEventStatus GetTestEventStatus()
        {
            return new SportEventStatus(new TestSportEventStatusMapper().CreateNotStarted(), new TestLocalizedNamedValueCache());
        }
    }

    public class TestCompetition : ICompetition
    {
        public TestCompetition(URN id)
        {
            Id = id;
        }

        public URN Id { get; }

        public Task<ICompetitionStatus> GetStatusAsync()
        {
            return Task.FromResult<ICompetitionStatus>(null);
        }

        public Task<BookingStatus?> GetBookingStatusAsync()
        {
            return Task.FromResult<BookingStatus?>(null);
        }

        public Task<URN> GetSportIdAsync()
        {
            return Task.FromResult<URN>(null);
        }

        public Task<DateTime?> GetScheduledTimeAsync()
        {
            return Task.FromResult<DateTime?>(null);
        }

        public Task<DateTime?> GetScheduledEndTimeAsync()
        {
            return Task.FromResult<DateTime?>(null);
        }

        public Task<URN> GetReplacedByAsync()
        {
            return Task.FromResult<URN>(null);
        }

        public Task<DateTime?> GetNextLiveTimeAsync()
        {
            return Task.FromResult<DateTime?>(null);
        }

        public Task<bool?> GetStartTimeTBDAsync()
        {
            return Task.FromResult<bool?>(null);
        }

        public Task<IEnumerable<ITeamCompetitor>> GetCompetitorsAsync()
        {
            return Task.FromResult<IEnumerable<ITeamCompetitor>>(null);
        }

        public Task<IGroup> GetGroupAsync()
        {
            return Task.FromResult<IGroup>(null);
        }

        public Task<ITournament> GetTournamentAsync()
        {
            return Task.FromResult<ITournament>(null);
        }

        public Task<IRound> GetTournamentRoundAsync()
        {
            return Task.FromResult<IRound>(null);
        }

        public Task<IVenue> GetVenueAsync()
        {
            return Task.FromResult<IVenue>(null);
        }

        public Task<IFixture> GetFixtureAsync()
        {
            return Task.FromResult<IFixture>(null);
        }

        public Task<ISportEventConditions> GetConditionsAsync()
        {
            return Task.FromResult<ISportEventConditions>(null);
        }

        Task<IEnumerable<ICompetitor>> ICompetition.GetCompetitorsAsync()
        {
            return Task.FromResult<IEnumerable<ICompetitor>>(null);
        }

        public Task<string> GetNameAsync(CultureInfo culture)
        {
            return Task.FromResult<string>(null);
        }
    }

    public class TestTournament : ITournament
    {
        public TestTournament(URN id)
        {
            Id = id;
        }

        public URN Id { get; }

        public Task<URN> GetSportIdAsync()
        {
            return Task.FromResult<URN>(null);
        }

        public Task<DateTime?> GetScheduledTimeAsync()
        {
            return Task.FromResult<DateTime?>(null);
        }

        public Task<DateTime?> GetScheduledEndTimeAsync()
        {
            return Task.FromResult<DateTime?>(null);
        }

        public Task<URN> GetReplacedByAsync()
        {
            return Task.FromResult<URN>(null);
        }

        public Task<DateTime?> GetNextLiveTimeAsync()
        {
            return Task.FromResult<DateTime?>(null);
        }

        public Task<bool?> GetStartTimeTBDAsync()
        {
            return Task.FromResult<bool?>(null);
        }

        public Task<string> GetNameAsync(CultureInfo culture)
        {
            return Task.FromResult<string>(null);
        }

        public Task<ISportSummary> GetSportAsync()
        {
            return Task.FromResult<ISportSummary>(null);
        }

        public Task<ITournamentCoverage> GetTournamentCoverage()
        {
            return Task.FromResult<ITournamentCoverage>(null);
        }

        public Task<ICategorySummary> GetCategoryAsync()
        {
            return Task.FromResult<ICategorySummary>(null);
        }

        public Task<ICurrentSeasonInfo> GetCurrentSeasonAsync()
        {
            return Task.FromResult<ICurrentSeasonInfo>(null);
        }

        public Task<IEnumerable<ISeason>> GetSeasonsAsync()
        {
            return Task.FromResult<IEnumerable<ISeason>>(null);
        }

        public Task<ISeasonCoverage> GetCoverageAsync()
        {
            return Task.FromResult<ISeasonCoverage>(null);
        }

        public Task<ISeason> GetSeasonAsync()
        {
            return Task.FromResult<ISeason>(null);
        }

        public Task<IEnumerable<IGroup>> GetGroupsAsync()
        {
            return Task.FromResult<IEnumerable<IGroup>>(null);
        }

        public Task<IRound> GetCurrentRoundAsync()
        {
            return Task.FromResult<IRound>(null);
        }

        public Task<IEnumerable<ICompetition>> GetScheduleAsync()
        {
            return Task.FromResult<IEnumerable<ICompetition>>(null);
        }
    }

    public class TestMatch : IMatch
    {
        public TestMatch(URN id)
        {
            Id = id;
        }

        public URN Id { get; }

        public Task<URN> GetSportIdAsync()
        {
            return Task.FromResult<URN>(null);
        }

        public Task<DateTime?> GetScheduledTimeAsync()
        {
            return Task.FromResult<DateTime?>(null);
        }

        public Task<DateTime?> GetScheduledEndTimeAsync()
        {
            return Task.FromResult<DateTime?>(null);
        }

        Task<ICompetitionStatus> ICompetition.GetStatusAsync()
        {
            return Task.FromResult<ICompetitionStatus>(null);
        }

        Task<IMatchStatus> IMatch.GetStatusAsync()
        {
            return Task.FromResult<IMatchStatus>(null);
        }

        public Task<BookingStatus?> GetBookingStatusAsync()
        {
            return Task.FromResult<BookingStatus?>(null);
        }

        public Task<ILongTermEvent> GetTournamentAsync()
        {
            return Task.FromResult<ILongTermEvent>(null);
        }

        public Task<IVenue> GetVenueAsync()
        {
            return Task.FromResult<IVenue>(null);
        }

        public Task<IFixture> GetFixtureAsync()
        {
            return Task.FromResult<IFixture>(null);
        }

        public Task<IEventTimeline> GetEventTimelineAsync()
        {
            return Task.FromResult<IEventTimeline>(null);
        }

        public Task<IDelayedInfo> GetDelayedInfoAsync()
        {
            return Task.FromResult<IDelayedInfo>(null);
        }

        public Task<ISportEventConditions> GetConditionsAsync()
        {
            return Task.FromResult<ISportEventConditions>(null);
        }

        public Task<IEnumerable<ICompetitor>> GetCompetitorsAsync()
        {
            var competitors = new List<ICompetitor>()
            {
                new TestCompetitor(URN.Parse("sr:competitor:1"), "First competitor", new CultureInfo("en")),
                new TestCompetitor(URN.Parse("sr:competitor:2"), "Second competitor", new CultureInfo("en"))
            };
            return Task.FromResult<IEnumerable<ICompetitor>>(competitors);
        }

        public Task<string> GetNameAsync(CultureInfo culture)
        {
            return Task.FromResult<string>(null);
        }

        public Task<ITeamCompetitor> GetHomeCompetitorAsync()
        {
            return Task.FromResult<ITeamCompetitor>(null);
        }

        public Task<ITeamCompetitor> GetAwayCompetitorAsync()
        {
            return Task.FromResult<ITeamCompetitor>(null);
        }

        Task<ISeasonInfo> IMatch.GetSeasonAsync()
        {
            return Task.FromResult<ISeasonInfo>(null);
        }

        public Task<IRound> GetTournamentRoundAsync()
        {
            return Task.FromResult<IRound>(null);
        }

        public Task<URN> GetReplacedByAsync()
        {
            return Task.FromResult<URN>(null);
        }

        public Task<DateTime?> GetNextLiveTimeAsync()
        {
            return Task.FromResult<DateTime?>(null);
        }

        public Task<bool?> GetStartTimeTBDAsync()
        {
            return Task.FromResult<bool?>(null);
        }
    }

    public class TestStage : IStage
    {
        public TestStage(URN id)
        {
            Id = id;
        }

        public URN Id { get; }

        public Task<URN> GetSportIdAsync()
        {
            return Task.FromResult<URN>(null);
        }

        public Task<DateTime?> GetScheduledTimeAsync()
        {
            return Task.FromResult<DateTime?>(null);
        }

        public Task<DateTime?> GetScheduledEndTimeAsync()
        {
            return Task.FromResult<DateTime?>(null);
        }

        public Task<ICompetitionStatus> GetStatusAsync()
        {
            return Task.FromResult<ICompetitionStatus>(null);
        }

        public Task<BookingStatus?> GetBookingStatusAsync()
        {
            return Task.FromResult<BookingStatus?>(null);
        }

        public Task<ITournament> GetTournamentAsync()
        {
            return Task.FromResult<ITournament>(null);
        }

        public Task<IVenue> GetVenueAsync()
        {
            return Task.FromResult<IVenue>(null);
        }

        public Task<IFixture> GetFixtureAsync()
        {
            return Task.FromResult<IFixture>(null);
        }

        public Task<ISportEventConditions> GetConditionsAsync()
        {
            return Task.FromResult<ISportEventConditions>(null);
        }

        public Task<string> GetNameAsync(CultureInfo culture)
        {
            return Task.FromResult<string>(null);
        }

        public Task<IEnumerable<ICompetitor>> GetCompetitorsAsync()
        {
            return Task.FromResult<IEnumerable<ICompetitor>>(null);
        }

        public Task<ISportSummary> GetSportAsync()
        {
            return Task.FromResult<ISportSummary>(null);
        }

        public Task<ICategorySummary> GetCategoryAsync()
        {
            return Task.FromResult<ICategorySummary>(null);
        }

        public Task<IStage> GetParentStageAsync()
        {
            return Task.FromResult<IStage>(null);
        }

        public Task<IEnumerable<IStage>> GetStagesAsync()
        {
            return Task.FromResult<IEnumerable<IStage>>(null);
        }

        public Task<StageType> GetStageTypeAsync()
        {
            return Task.FromResult(StageType.Parent);
        }

        public Task<URN> GetReplacedByAsync()
        {
            return Task.FromResult<URN>(null);
        }

        public Task<DateTime?> GetNextLiveTimeAsync()
        {
            return Task.FromResult<DateTime?>(null);
        }

        public Task<bool?> GetStartTimeTBDAsync()
        {
            return Task.FromResult<bool?>(null);
        }
    }

    public class TestCompetitor : ICompetitor
    {
        public TestCompetitor(URN id, string name, CultureInfo culture)
        {
            Id = id;
            Names = new Dictionary<CultureInfo, string> {{culture, name}};
        }

        /// <summary>
        /// Returns a string that represents the current object
        /// </summary>
        /// <param name="formatProvider">A format provider used to format the output string</param>
        /// <returns>A string that represents the current object.</returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return Id.ToString();
        }

        /// <summary>Formats the value of the current instance using the specified format.</summary>
        /// <returns>The value of the current instance in the specified format.</returns>
        /// <param name="format">The format to use.-or- A null reference (Nothing in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation. </param>
        /// <param name="formatProvider">The provider to use to format the value.-or- A null reference (Nothing in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system. </param>
        public string ToString(string format, IFormatProvider formatProvider = null)
        {
            return Id.ToString();
        }

        /// <summary>
        /// Gets the <see cref="URN"/> uniquely identifying the current <see cref="ICompetitor" /> instance
        /// </summary>
        public URN Id { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing player names in different languages
        /// </summary>
        public IReadOnlyDictionary<CultureInfo, string> Names { get; }

        /// <summary>
        /// Gets the name of the player in the specified language or a null reference
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned name</param>
        /// <returns>The name of the player in the specified language or a null reference.</returns>
        public string GetName(CultureInfo culture)
        {
            return Names[culture];
        }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing competitor's country names in different languages
        /// </summary>
        public IReadOnlyDictionary<CultureInfo, string> Countries { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing competitor's abbreviations in different languages
        /// </summary>
        public IReadOnlyDictionary<CultureInfo, string> Abbreviations { get; }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="ICompetitor"/> is virtual - i.e.
        /// competes in a virtual sport
        /// </summary>
        public bool IsVirtual { get; }

        /// <summary>
        /// Gets the reference ids
        /// </summary>
        public IReference References { get; }

        /// <summary>
        /// Gets the competitor's country name in the specified language or a null reference.
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the country name.</param>
        /// <returns>The competitor's country name in the specified language or a null reference.</returns>
        public string GetCountry(CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the competitor's abbreviation in the specified language or a null reference.
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the abbreviation.</param>
        /// <returns>The competitor's abbreviation in the specified language or a null reference.</returns>
        public string GetAbbreviation(CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the country code
        /// </summary>
        /// <value>The country code</value>
        public string CountryCode { get; }

        /// <summary>
        /// Gets the list of associated player ids
        /// </summary>
        /// <value>The associated player ids</value>
        public IEnumerable<IPlayer> AssociatedPlayers { get; }

        /// <summary>
        /// Gets the jerseys of known competitors
        /// </summary>
        /// <value>The jerseys</value>
        public IEnumerable<IJersey> Jerseys { get; }

        /// <summary>
        /// Gets the manager
        /// </summary>
        /// <value>The manager</value>
        public IManager Manager { get; }

        /// <summary>
        /// Gets the venue
        /// </summary>
        /// <value>The venue</value>
        public IVenue Venue { get; }
    }
}

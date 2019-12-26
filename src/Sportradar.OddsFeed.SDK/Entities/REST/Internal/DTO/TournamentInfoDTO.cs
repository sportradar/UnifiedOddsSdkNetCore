/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-access-object containing tournament information (when fetching summary or fixture)
    /// </summary>
    public class TournamentInfoDTO : SportEventSummaryDTO
    {
        /// <summary>
        /// Gets the tournament coverage
        /// </summary>
        /// <value>The tournament coverage</value>
        public TournamentCoverageDTO TournamentCoverage { get; }

        /// <summary>
        /// Gets the category
        /// </summary>
        /// <value>The category</value>
        public CategorySummaryDTO Category { get; private set; }

        /// <summary>
        /// Gets the sport
        /// </summary>
        /// <value>The sport</value>
        public SportEntityDTO Sport { get; }

        /// <summary>
        /// Gets the competitors
        /// </summary>
        /// <value>The competitors</value>
        public IEnumerable<CompetitorDTO> Competitors { get; }

        /// <summary>
        /// Gets a <see cref="SeasonDTO"/> representing the current season of the tournament
        /// </summary>
        public CurrentSeasonInfoDTO CurrentSeason { get; private set; }

        /// <summary>
        /// Gets a <see cref="SeasonDTO"/> representing the season of the tournament
        /// </summary>
        public CurrentSeasonInfoDTO Season { get; private set; }

        /// <summary>
        /// Gets the season coverage
        /// </summary>
        /// <value>The season coverage</value>
        public SeasonCoverageDTO SeasonCoverage { get; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{GroupDTO}"/> representing tournament groups
        /// </summary>
        public IEnumerable<GroupDTO> Groups { get; }

        /// <summary>
        /// Gets the list of all <see cref="CompetitionDTO"/> that belongs to the season schedule
        /// </summary>
        /// <returns>The list of all <see cref="CompetitionDTO"/> that belongs to the season schedule</returns>
        public IEnumerable<CompetitionDTO> Schedule { get; }

        /// <summary>
        /// Gets a <see cref="RoundDTO"/> representing current tournament round
        /// </summary>
        public RoundDTO CurrentRound { get; }

        /// <summary>
        /// Gets a <see cref="string"/> representation of the current season year
        /// </summary>
        public string Year { get; }

        /// <summary>
        /// Gets the tournament information for the season
        /// </summary>
        /// <value>The tournament information for the season</value>
        public TournamentInfoDTO TournamentInfo { get; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> specifying when the associated message was generated (on the server side)
        /// </summary>
        public DateTime? GeneratedAt { get; }

        /// <summary>
        /// Gets the <see cref="bool"/> specifying if the tournament is exhibition game
        /// </summary>
        public bool? ExhibitionGames { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentInfoDTO"/> class
        /// </summary>
        /// <param name="sportEvent">A <see cref="sportEvent"/> containing basic tournament info</param>
        internal TournamentInfoDTO(sportEvent sportEvent)
            : base(sportEvent)
        {
            TournamentCoverage = null;

            Category = sportEvent.tournament.category == null
                ? null
                : new CategorySummaryDTO(sportEvent.tournament.category);

            Sport = sportEvent.tournament.sport == null
                ? null
                : new SportEntityDTO(sportEvent.tournament.sport.id, sportEvent.tournament.sport.name);

            Competitors = sportEvent.competitors == null
                ? null
                : new ReadOnlyCollection<CompetitorDTO>(sportEvent.competitors.Select(c => new CompetitorDTO(c)).ToList());

            CurrentSeason = null;

            Season = sportEvent.season == null
                ? null
                : new CurrentSeasonInfoDTO(sportEvent.season);

            SeasonCoverage = null;

            Groups = null;

            Schedule = null;

            CurrentRound = sportEvent.tournament_round == null
                ? null
                : new RoundDTO(sportEvent.tournament_round);

            Year = null;

            TournamentInfo = sportEvent.tournament == null
                ? null
                : new TournamentInfoDTO(sportEvent.tournament);

            //TODO: missing year
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentInfoDTO"/> class
        /// </summary>
        /// <param name="tournament">A <see cref="sportEvent"/> containing basic tournament info</param>
        internal TournamentInfoDTO(tournament tournament)
            : base(new sportEvent
            {

                id = tournament.id,
                name = tournament.name,
                scheduledSpecified = IsTournamentScheduleSpecified(tournament, true),
                scheduled = GetTournamentSchedule(tournament, true),
                scheduled_endSpecified = IsTournamentScheduleSpecified(tournament, false),
                scheduled_end = GetTournamentSchedule(tournament, false),
                tournament = tournament
            })
        {
            TournamentCoverage = null;

            Category = tournament.category == null
                ? null
                : new CategorySummaryDTO(tournament.category);

            Sport = tournament.sport == null
                ? null
                : new SportEntityDTO(tournament.sport.id, tournament.sport.name);

            Competitors = null;

            CurrentSeason = null;

            Season = null;

            SeasonCoverage = null;

            Groups = null;

            Schedule = null;

            CurrentRound = null;

            Year = null;

            TournamentInfo = null;

            ExhibitionGames = tournament.exhibition_gamesSpecified ? tournament.exhibition_games : (bool?) null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentInfoDTO"/> class
        /// </summary>
        /// <param name="tournament">A <see cref="tournament"/> containing detailed tournament info</param>
        internal TournamentInfoDTO(tournamentInfoEndpoint tournament)
            : base(new sportEvent
            {

                id = tournament.tournament.id,
                name = tournament.tournament.name,
                scheduledSpecified = IsTournamentScheduleSpecified(tournament.tournament, true) || tournament.tournament.current_season != null,
                scheduled = GetExtendedTournamentSchedule(tournament.tournament, true),
                scheduled_endSpecified = IsTournamentScheduleSpecified(tournament.tournament, false) || tournament.tournament.current_season != null,
                scheduled_end = GetExtendedTournamentSchedule(tournament.tournament, false),
                tournament = tournament.tournament
            })
        {
            TournamentCoverage = tournament.coverage_info == null
                ? null
                : new TournamentCoverageDTO(tournament.coverage_info);

            Category = tournament.tournament.category == null
                ? null
                : new CategorySummaryDTO(tournament.tournament.category);

            Sport = tournament.tournament.sport == null
                ? null
                : new SportEntityDTO(tournament.tournament.sport.id, tournament.tournament.sport.name);

            Competitors = tournament.competitors != null
                ? new ReadOnlyCollection<CompetitorDTO>(tournament.competitors.Select(c => new CompetitorDTO(c)).ToList())
                : tournament.tournament?.competitors == null // used for stage events
                    ? null
                    : new ReadOnlyCollection<CompetitorDTO>(tournament.tournament.competitors.Select(c => new CompetitorDTO(c)).ToList());

            CurrentSeason = tournament.tournament.current_season == null
                ? null
                : new CurrentSeasonInfoDTO(tournament.tournament.current_season);

            Season = tournament.season == null
                ? null
                : new CurrentSeasonInfoDTO(tournament.season);

            SeasonCoverage = tournament.season_coverage_info == null
                ? null
                : new SeasonCoverageDTO(tournament.season_coverage_info);

            Groups = tournament.groups == null
                ? null
                : new ReadOnlyCollection<GroupDTO>(tournament.groups.Select(g => new GroupDTO(g)).ToList());

            Schedule = null;

            CurrentRound = tournament.round == null
                ? null
                : new RoundDTO(tournament.round);

            Year = tournament.tournament?.current_season == null
                ? tournament.season?.year
                : tournament.tournament.current_season.year;

            TournamentInfo = null;

            if (tournament.tournament?.category != null)
            {
                var sportEvent = new sportEvent
                {
                    id = tournament.tournament.id,
                    name = tournament.tournament.name,
                    scheduledSpecified = IsTournamentScheduleSpecified(tournament.tournament, true) || tournament.tournament.current_season != null,
                    scheduled = GetExtendedTournamentSchedule(tournament.tournament, true),
                    scheduled_endSpecified = IsTournamentScheduleSpecified(tournament.tournament, false) || tournament.tournament.current_season != null,
                    scheduled_end = GetExtendedTournamentSchedule(tournament.tournament, false),
                    tournament = tournament.tournament
                };
                TournamentInfo = new TournamentInfoDTO(sportEvent)
                {
                    Category = new CategorySummaryDTO(tournament.tournament.category.id, tournament.tournament.category.name, tournament.tournament.category.country_code),
                    CurrentSeason = tournament.tournament.current_season == null
                        ? null
                        : new CurrentSeasonInfoDTO(tournament.tournament.current_season)
                };
            }

            GeneratedAt = tournament.generated_atSpecified ? tournament.generated_at : (DateTime?) null;

            ExhibitionGames = tournament.tournament.exhibition_gamesSpecified
                ? tournament.tournament.exhibition_games
                : (bool?) null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentDTO"/> class
        /// </summary>
        /// <param name="tournament">A <see cref="TournamentDTO"/> containing basic tournament info</param>
        internal TournamentInfoDTO(TournamentDTO tournament)
            : base(new sportEvent
            {
                id = tournament.Id.ToString(),
                name = tournament.Name,
                scheduledSpecified = tournament.Scheduled != null,
                scheduled = tournament.Scheduled.GetValueOrDefault(DateTime.MinValue),
                scheduled_endSpecified = tournament.ScheduledEnd != null,
                scheduled_end = tournament.ScheduledEnd.GetValueOrDefault(DateTime.MinValue),
                tournament = new tournament
                {
                    id = tournament.Id.ToString(),
                    sport = new sport
                    {
                        id = tournament.Sport.Id.ToString(),
                        name = tournament.Sport.Name
                    }
                }
            })
        {
            TournamentCoverage = null;

            Category = tournament.Category;

            Sport = tournament.Sport == null
                ? null
                : tournament.Sport;

            Competitors = null;

            CurrentSeason = tournament.CurrentSeason == null
                ? null
                : new CurrentSeasonInfoDTO(tournament.CurrentSeason);

            Season = null;

            SeasonCoverage = tournament.SeasonCoverage;

            Groups = null;

            Schedule = null;

            CurrentRound = null;

            Year = null;

            TournamentInfo = null;

            //TODO: missing year, tournamentInfo
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentDTO"/> class
        /// </summary>
        /// <param name="season">A <see cref="SeasonDTO"/> containing basic tournament info</param>
        internal TournamentInfoDTO(SeasonDTO season)
            : base(new sportEvent
            {
                id = season.Id.ToString(),
                name = season.Name,
                scheduledSpecified = true,
                scheduled = season.StartDate,
                scheduled_endSpecified = true,
                scheduled_end = season.EndDate
            })
        {
            TournamentCoverage = null;

            Category = null;

            Sport = null;

            Competitors = null;

            CurrentSeason = null;

            Season = null;

            SeasonCoverage = null;

            Groups = null;

            Schedule = null;

            CurrentRound = null;

            Year = season.Year;

            TournamentInfo = null;

            //TODO: missing year, tournamentInfo
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentInfoDTO"/> class
        /// </summary>
        /// <param name="fixture">The fixture</param>
        public TournamentInfoDTO(FixtureDTO fixture)
        : base(new sportEvent
        {
            id = fixture.Id.ToString(),
            name = fixture.Name,
            scheduledSpecified = fixture.Scheduled.HasValue,
            scheduled = fixture.Scheduled.GetValueOrDefault(DateTime.MinValue),
            scheduled_endSpecified = fixture.ScheduledEnd.HasValue,
            scheduled_end = fixture.ScheduledEnd.GetValueOrDefault(DateTime.MinValue),
            tournament = new tournament
            {
                id = fixture.Tournament?.Id.ToString(),
                sport = new sport
                {
                    id = fixture.Tournament?.Sport.Id.ToString(),
                    name = fixture.Tournament?.Sport.Name
                }
            },
            status = fixture.StatusOnEvent
        })
        {
            TournamentCoverage = fixture.CoverageInfo == null
                ? null
                : new TournamentCoverageDTO(new tournamentLiveCoverageInfo { live_coverage = fixture.CoverageInfo.IsLive.ToString().ToLower() });

            Category = fixture.Tournament?.Category;

            Sport = fixture.Tournament?.Sport;

            Competitors = fixture.Competitors;

            CurrentSeason = null;

            Season = null;

            SeasonCoverage = null;

            Groups = null;

            Schedule = null;

            CurrentRound = fixture.Round;

            Year = fixture.Tournament?.CurrentSeason?.Year;

            TournamentInfo = null;

            //TODO: missing tournamentInfo
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentInfo"/> class
        /// </summary>
        /// <param name="tournament">A <see cref="TournamentInfoDTO"/> containing basic tournament info</param>
        /// <param name="overwriteSeason">Overwrite tournament base data with season info</param>
        /// <param name="overwriteCurrentSeason">Overwrite tournament base data with current season info</param>
        internal TournamentInfoDTO(TournamentInfoDTO tournament, bool overwriteSeason, bool overwriteCurrentSeason)
            : base(new sportEvent
            {
                id = overwriteSeason
                    ? tournament.Season.Id.ToString()
                    : overwriteCurrentSeason
                        ? tournament.CurrentSeason.Id.ToString()
                        : tournament.Id.ToString(),
                name = overwriteSeason
                    ? tournament.Season.Name
                    : overwriteCurrentSeason
                        ? tournament.CurrentSeason.Name
                        : tournament.Name,
                scheduledSpecified = overwriteSeason
                    ? tournament.Season.StartDate > DateTime.MinValue
                    : overwriteCurrentSeason
                        ? tournament.CurrentSeason.StartDate > DateTime.MinValue
                        : tournament.Scheduled != null,
                scheduled = overwriteSeason
                    ? tournament.Season.StartDate
                    : overwriteCurrentSeason
                        ? tournament.CurrentSeason.StartDate
                        : tournament.Scheduled.GetValueOrDefault(DateTime.MinValue),
                scheduled_endSpecified = overwriteSeason
                    ? tournament.Season.EndDate > DateTime.MinValue
                    : overwriteCurrentSeason
                        ? tournament.CurrentSeason.EndDate > DateTime.MinValue
                        : tournament.ScheduledEnd != null,
                scheduled_end = overwriteSeason
                    ? tournament.Season.EndDate
                    : overwriteCurrentSeason
                        ? tournament.CurrentSeason.EndDate
                        : tournament.ScheduledEnd.GetValueOrDefault(DateTime.MinValue),
                tournament = new tournament
                {
                    id = tournament.Id.ToString(),
                    sport = new sport
                    {
                        id = tournament.Sport.Id.ToString(),
                        name = tournament.Sport.Name
                    }
                },
                status = tournament.StatusOnEvent
            })
        {
            TournamentCoverage = tournament.TournamentCoverage;

            Category = tournament.Category;

            Sport = tournament.Sport;

            Competitors = tournament.Competitors;

            CurrentSeason = tournament.CurrentSeason;

            Season = tournament.Season;

            SeasonCoverage = tournament.SeasonCoverage;

            Groups = tournament.Groups;

            Schedule = tournament.Schedule;

            CurrentRound = tournament.CurrentRound;

            Year = overwriteSeason
                ? tournament.Season.Year
                : overwriteCurrentSeason
                    ? tournament.CurrentSeason.Year
                    : tournament.Year;

            TournamentInfo = tournament.TournamentInfo;

            GeneratedAt = tournament.GeneratedAt;

            ExhibitionGames = tournament.ExhibitionGames;
        }

        private static bool IsTournamentScheduleSpecified(tournament tournament, bool useStartTime)
        {
            if (useStartTime)
            {
                return tournament.scheduledSpecified || tournament.tournament_length != null && tournament.tournament_length.start_dateSpecified;
            }
            return tournament.scheduled_endSpecified || tournament.tournament_length != null && tournament.tournament_length.end_dateSpecified;
        }

        private static DateTime GetTournamentSchedule(tournament tournament, bool useStartTime)
        {
            if (useStartTime)
            {
                return tournament.scheduledSpecified
                    ? tournament.scheduled
                    : tournament.tournament_length != null && tournament.tournament_length.start_dateSpecified
                        ? tournament.tournament_length.start_date
                        : tournament.scheduled;
            }

            return tournament.scheduled_endSpecified
                ? tournament.scheduled_end
                : tournament.tournament_length != null && tournament.tournament_length.end_dateSpecified
                    ? tournament.tournament_length.end_date
                    : tournament.scheduled;
        }

        private static DateTime GetExtendedTournamentSchedule(tournamentExtended tournament, bool useStartTime)
        {
            if (useStartTime)
            {
                return IsTournamentScheduleSpecified(tournament, true)
                    ? GetTournamentSchedule(tournament, true)
                    : tournament.current_season != null
                        ? tournament.current_season.start_date
                        : tournament.scheduled;
            }

            return IsTournamentScheduleSpecified(tournament, false)
                ? GetTournamentSchedule(tournament, false)
                : tournament.current_season != null
                    ? tournament.current_season.end_date
                    : tournament.scheduled_end;
        }
    }
}

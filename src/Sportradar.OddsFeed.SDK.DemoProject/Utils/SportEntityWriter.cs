﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Dawn;
using log4net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;

namespace Sportradar.OddsFeed.SDK.DemoProject.Utils;

/// <summary>
/// A class used to construct string representations of the <see cref="ISportEvent"/> instances
/// </summary>
internal class SportEntityWriter
{
    /// <summary>
    /// A <see cref="ILog"/> used to log sport entities data
    /// </summary>
    private readonly ILogger _log;

    /// <summary>
    /// A <see cref="TaskProcessor"/> used for processing of asynchronous requests
    /// </summary>
    private readonly TaskProcessor _taskProcessor;

    /// <summary>
    /// A <see cref="CultureInfo"/> specifying which language of the translatable properties to take
    /// </summary>
    private readonly CultureInfo _culture;

    /// <summary>
    /// A <see cref="CultureInfo"/> specifying which language of the translatable properties to take
    /// </summary>
    private readonly bool _writeNotCacheableData;

    /// <summary>
    /// Initializes a new instance of the <see cref="SportEntityWriter"/> class
    /// </summary>
    /// <param name="taskProcessor">A <see cref="TaskProcessor"/> used for processing of asynchronous requests</param>
    /// <param name="culture"> A <see cref="CultureInfo"/> specifying which language of the translatable properties to take</param>
    /// <param name="writeNotCacheableData">A <see cref="bool"/> indicating whether data not cached by the SDK should also be retrieved & written</param>
    /// <param name="log">A <see cref="ILogger" /> instance used for logging</param>
    internal SportEntityWriter(TaskProcessor taskProcessor, CultureInfo culture, bool writeNotCacheableData = false, ILogger log = null)
    {
        Guard.Argument(taskProcessor, nameof(taskProcessor)).NotNull();
        Guard.Argument(culture, nameof(culture)).NotNull();

        _taskProcessor = taskProcessor;
        _culture = culture;
        _writeNotCacheableData = writeNotCacheableData;
        _log = log ?? new NullLogger<SportEntityWriter>();
    }

    /// <summary>
    /// Adds the info about the passed <see cref="ISportEvent"/> to the provided <see cref="StringBuilder"/>
    /// </summary>
    /// <param name="sportEvent">The <see cref="ISportEvent"/> containing data to display</param>
    /// <param name="builder">The <see cref="StringBuilder"/> to which to add the data</param>
    private void AddEntityData(ISportEvent sportEvent, StringBuilder builder)
    {
        Guard.Argument(sportEvent, nameof(sportEvent)).NotNull();
        Guard.Argument(builder, nameof(builder)).NotNull();

        var scheduled = _taskProcessor.GetTaskResult(sportEvent.GetScheduledTimeAsync());
        var scheduledEnd = _taskProcessor.GetTaskResult(sportEvent.GetScheduledEndTimeAsync());
        var name = _taskProcessor.GetTaskResult(sportEvent.GetNameAsync(_culture));
        var sportId = _taskProcessor.GetTaskResult(sportEvent.GetSportIdAsync());

        builder.Append("Id=").Append(sportEvent.Id);
        builder.Append(" Name=").Append(name);
        builder.Append(" SportId=").Append(sportId);
        builder.Append(" Scheduled=").Append(scheduled?.ToShortDateString() ?? "null");
        builder.Append(" ScheduledEnd=").Append(scheduledEnd?.ToShortDateString() ?? "null");
    }

    /// <summary>
    /// Adds the info about the passed <see cref="ICompetition"/> to the provided <see cref="StringBuilder"/>
    /// </summary>
    /// <param name="competition">The <see cref="ICompetition"/> containing data to display</param>
    /// <param name="builder">The <see cref="StringBuilder"/> to which to add the data</param>
    private void AddSportEventData(ICompetition competition, StringBuilder builder)
    {
        Guard.Argument(competition, nameof(competition)).NotNull();
        Guard.Argument(builder, nameof(builder)).NotNull();

        AddEntityData(competition, builder);

        var bookingStatus = _taskProcessor.GetTaskResult(competition.GetBookingStatusAsync());
        var venue = _taskProcessor.GetTaskResult(competition.GetVenueAsync());
        var competitionStatus = _taskProcessor.GetTaskResult(competition.GetStatusAsync());
        var conditions = _taskProcessor.GetTaskResult(competition.GetConditionsAsync());
        var competitors = _taskProcessor.GetTaskResult(competition.GetCompetitorsAsync());
        var comps = competitors == null
                        ? "null"
                        : string.Join(", ", competitors.Select(s => s.ToString("f")));

        builder.Append(" CompetitionStatus=").Append(competitionStatus?.Status)
               .Append(" BookingStatus=").Append(bookingStatus == null ? "null" : Enum.GetName(typeof(BookingStatus), bookingStatus))
               .Append(" Venue=").Append(venue?.Id.ToString() ?? "null")
               .Append(" Conditions=").Append(conditions?.EventMode)
               .Append(" Competitors=[").Append(comps).Append("]");
    }

    /// <summary>
    /// Writes the data of the provided <see cref="IStage"/> to the <see cref="StringBuilder"/>
    /// </summary>
    /// <param name="stage">The <see cref="IStage"/> whose data is to be written</param>
    /// <returns>A <see cref="StringBuilder"/> containing string representation of the provided stage</returns>
    private StringBuilder WriteStageData(IStage stage)
    {
        Guard.Argument(stage, nameof(stage)).NotNull();

        var builder = new StringBuilder();
        AddSportEventData(stage, builder);

        var competitors = _taskProcessor.GetTaskResult(stage.GetCompetitorsAsync());
        var competitorsStr = competitors == null
                                 ? "null"
                                 : string.Join(",", competitors.Select(s => s.Id));
        var parent = _taskProcessor.GetTaskResult(stage.GetParentStageAsync());
        var stages = _taskProcessor.GetTaskResult(stage.GetStagesAsync());
        var category = _taskProcessor.GetTaskResult(stage.GetCategoryAsync());
        var sport = _taskProcessor.GetTaskResult(stage.GetSportAsync());

        var competitionStatus = _taskProcessor.GetTaskResult(stage.GetStatusAsync());
        var eventResults = string.Empty;
        var eventStatus = string.Empty;
        if (competitionStatus != null)
        {
            eventStatus = competitionStatus.Status.ToString();
            if (competitionStatus.EventResults != null && competitionStatus.EventResults.Any())
            {
                eventResults = string.Join(",", competitionStatus.EventResults.Select(s => $"E{s.Id}={s.HomeScore}:{s.AwayScore};{s.GetMatchStatusAsync(_culture)}/{s.Status}"));
                var eventResultMatchStatus = _taskProcessor.GetTaskResult(competitionStatus.EventResults.First().GetMatchStatusAsync(_culture));
                var x = eventResultMatchStatus?.Id;
            }
        }

        builder.Append(" Competitors=").Append(competitorsStr)
               .Append(" Parent=").Append(parent?.Id.ToString() ?? "null")
               .Append(" Stages=").Append(stages?.Count().ToString() ?? "null")
               .Append(" Sport=").Append(sport?.Id)
               .Append(" Category=").Append(category?.Id)
               .Append(" EventStatus=").Append(eventStatus)
               .Append(" EventResults=[").Append(eventResults).Append("]");

        return builder;
    }

    /// <summary>
    /// Writes the data of the provided <see cref="IMatch"/> to the <see cref="StringBuilder"/>
    /// </summary>
    /// <param name="match">The <see cref="IMatch"/> whose data is to be written</param>
    /// <returns>A <see cref="StringBuilder"/> containing string representation of the provided match</returns>
    private StringBuilder WriteMatchData(IMatch match)
    {
        Guard.Argument(match, nameof(match)).NotNull();

        var builder = new StringBuilder();
        AddSportEventData(match, builder);

        var homeTeam = _taskProcessor.GetTaskResult(match.GetHomeCompetitorAsync());
        var awayTeam = _taskProcessor.GetTaskResult(match.GetAwayCompetitorAsync());
        var season = _taskProcessor.GetTaskResult(match.GetSeasonAsync());
        var round = _taskProcessor.GetTaskResult(match.GetTournamentRoundAsync());
        var matchStatus = _taskProcessor.GetTaskResult(match.GetStatusAsync());
        var periodScores = string.Empty;
        var eventResults = string.Empty;
        var eventStatus = string.Empty;
        if (matchStatus != null)
        {
            var matchStatusStatuses = _taskProcessor.GetTaskResult(matchStatus.GetMatchStatusAsync(_culture));
            if (matchStatusStatuses != null)
            {
                builder.Append(" MatchStatus=")
                       .Append(matchStatusStatuses.Id)
                       .Append(" - ")
                       .Append(matchStatusStatuses.Description);
            }
            if (matchStatus.PeriodScores != null && matchStatus.PeriodScores.Any())
            {
                periodScores = string.Join(",", matchStatus.PeriodScores.Select(s => $"P{s.Number}={s.HomeScore}:{s.AwayScore};{s.GetMatchStatusAsync(_culture)}/{s.Type}"));
                var periodScoreMatchStatus = _taskProcessor.GetTaskResult(matchStatus.PeriodScores.First().GetMatchStatusAsync(_culture));
                var x = periodScoreMatchStatus?.Id;
            }
            if (matchStatus.EventResults != null && matchStatus.EventResults.Any())
            {
                eventResults = string.Join(",", matchStatus.EventResults.Select(s => $"E{s.Id}={s.HomeScore}:{s.AwayScore};{s.GetMatchStatusAsync(_culture)}/{s.Status}"));
                var eventResultMatchStatus = _taskProcessor.GetTaskResult(matchStatus.EventResults.First().GetMatchStatusAsync(_culture));
                var x = eventResultMatchStatus?.Id;
            }
            eventStatus = matchStatus.Status.ToString();
        }
        var fixture = _taskProcessor.GetTaskResult(match.GetFixtureAsync());
        var longTermEvent = _taskProcessor.GetTaskResult(match.GetTournamentAsync());
        var homeRef = homeTeam?.References;
        var awayRef = awayTeam?.References;
        var timelineIds = string.Empty;
        if (_writeNotCacheableData)
        {
            var matchTimeline = _taskProcessor.GetTaskResult(match.GetEventTimelineAsync());
            if (matchTimeline?.TimelineEvents != null && matchTimeline.TimelineEvents.Any())
            {
                timelineIds = string.Join(",", matchTimeline.TimelineEvents.Select(s => s.Id));
            }
        }
        var delayedInfo = _taskProcessor.GetTaskResult(match.GetDelayedInfoAsync());

        builder.Append(" HomeTeam=").Append(homeTeam?.Id)
               .Append(" HomeTeamReferences=").Append(homeRef == null ? "null" : WriteReference(homeRef))
               .Append(" AwayTeam=").Append(awayTeam?.Id)
               .Append(" AwayTeamReferences=").Append(awayRef == null ? "null" : WriteReference(awayRef))
               .Append(" Season=").Append(season?.Id.ToString() ?? "null")
               .Append(" Round=").Append(round?.GetName(_culture) ?? "null")
               .Append(" Fixture=").Append(fixture?.StartTime)
               .Append(" Tournament=").Append(longTermEvent)
               .Append(" EventStatus=").Append(eventStatus)
               .Append(" Timeline=[").Append(timelineIds).Append("]")
               .Append(" PeriodScores=[").Append(periodScores).Append("]")
               .Append(" EventResults=[").Append(eventResults).Append("]")
               .Append(" DelayedInfo=").Append(delayedInfo == null ? "null" : $"{delayedInfo.Id}-{delayedInfo.GetDescription(_culture)}");

        if (longTermEvent != null)
        {
            try
            {
                if (longTermEvent.Id.TypeGroup == ResourceTypeGroup.BasicTournament)
                {
                    var basicTournament = longTermEvent as IBasicTournament;
                    builder.Append(" Tournament=").Append(WriteBasicTournamentData(basicTournament));
                }
                else if (longTermEvent.Id.TypeGroup == ResourceTypeGroup.Tournament)
                {
                    var tournament = longTermEvent as ITournament;
                    builder.Append(" Tournament=").Append(WriteTournamentData(tournament));
                }
                else if (longTermEvent.Id.TypeGroup == ResourceTypeGroup.Season)
                {
                    var seasonTour = longTermEvent as ISeason;
                    builder.Append(" Tournament=").Append(WriteSeasonData(seasonTour));
                }
            }
            catch (Exception e)
            {
                _log.LogWarning(e, "Error obtaining tournament data {TournamentId}", longTermEvent.Id);
            }
        }

        return builder;
    }

    /// <summary>
    /// Writes the data of the provided <see cref="IBasicTournament"/> to the <see cref="StringBuilder"/>
    /// </summary>
    /// <param name="tournament">The <see cref="IBasicTournament"/> whose data is to be written</param>
    /// <returns>A <see cref="StringBuilder"/> containing string representation of the provided season</returns>
    private StringBuilder WriteBasicTournamentData(IBasicTournament tournament)
    {
        Guard.Argument(tournament, nameof(tournament)).NotNull();

        var builder = new StringBuilder();
        AddEntityData(tournament, builder);

        var sport = _taskProcessor.GetTaskResult(tournament.GetSportAsync());
        var category = _taskProcessor.GetTaskResult(tournament.GetCategoryAsync());
        var competitors = _taskProcessor.GetTaskResult(tournament.GetCompetitorsAsync());
        var competitorsStr = competitors == null
                                 ? "null"
                                 : string.Join(",", competitors.Select(s => s.Id));
        var tournamentCoverage = _taskProcessor.GetTaskResult(tournament.GetTournamentCoverage());

        builder.Append(" Competitors=").Append(competitorsStr)
               .Append(" Sport=").Append(sport)
               .Append(" Category=").Append(category)
               .Append(" TournamentCoverage=").Append(tournamentCoverage?.LiveCoverage);

        return builder;
    }

    /// <summary>
    /// Writes the data of the provided <see cref="ITournament"/> to the <see cref="StringBuilder"/>
    /// </summary>
    /// <param name="tournament">The <see cref="ITournament"/> whose data is to be written</param>
    /// <returns>A <see cref="StringBuilder"/> containing string representation of the provided tournament</returns>
    private StringBuilder WriteTournamentData(ITournament tournament)
    {
        Guard.Argument(tournament, nameof(tournament)).NotNull();

        var builder = new StringBuilder();
        AddEntityData(tournament, builder);

        var sport = _taskProcessor.GetTaskResult(tournament.GetSportAsync());
        var category = _taskProcessor.GetTaskResult(tournament.GetCategoryAsync());
        var currentSeasonInfo = _taskProcessor.GetTaskResult(tournament.GetCurrentSeasonAsync());
        var tournamentCoverage = _taskProcessor.GetTaskResult(tournament.GetTournamentCoverage());
        var seasons = _taskProcessor.GetTaskResult(tournament.GetSeasonsAsync());
        var seasonsIds = seasons == null
                             ? "null"
                             : string.Join(",", seasons.Select(s => s.Id));
        var currentSeasonStr = string.Empty;
        if (currentSeasonInfo != null)
        {
            currentSeasonStr = $"[{WriteCurrentSeasonInfoData(currentSeasonInfo)}]";
        }

        builder.Append(" Sport=").Append(sport)
               .Append(" Category=").Append(category)
               .Append(" CurrentSeasonInfo=").Append(currentSeasonStr)
               .Append(" TournamentCoverage=").Append(tournamentCoverage?.LiveCoverage)
               .Append(" Seasons=[").Append(seasonsIds).Append("]");

        return builder;
    }

    /// <summary>
    /// Writes the data of the provided <see cref="ISeason"/> to the <see cref="StringBuilder"/>
    /// </summary>
    /// <param name="season">The <see cref="ISeason"/> whose data is to be written</param>
    /// <returns>A <see cref="StringBuilder"/> containing string representation of the provided season</returns>
    private StringBuilder WriteSeasonData(ISeason season)
    {
        Guard.Argument(season, nameof(season)).NotNull();

        var builder = new StringBuilder();
        AddEntityData(season, builder);

        var sport = _taskProcessor.GetTaskResult(season.GetSportAsync());
        var groups = _taskProcessor.GetTaskResult(season.GetGroupsAsync());
        var scheduleString = string.Empty;
        if (_writeNotCacheableData)
        {
            var schedule = _taskProcessor.GetTaskResult(season.GetScheduleAsync());
            scheduleString = schedule == null
                                 ? "null"
                                 : string.Join(",", schedule.Select(s => s.Id));
        }
        var groupsStr = groups == null
                            ? "null"
                            : string.Join(",", groups.Select((s, index) => $"{index}-{s.Name}:C{s.Competitors?.Count()}"));
        var competitors = _taskProcessor.GetTaskResult(season.GetCompetitorsAsync());
        var competitorsStr = competitors == null
                                 ? "null"
                                 : string.Join(",", competitors.Select(s => s.Id));
        var year = _taskProcessor.GetTaskResult(season.GetYearAsync());
        var seasonCoverage = _taskProcessor.GetTaskResult(season.GetSeasonCoverageAsync());
        var round = _taskProcessor.GetTaskResult(season.GetCurrentRoundAsync());
        var tour = _taskProcessor.GetTaskResult(season.GetTournamentInfoAsync());
        var tourStr = string.Empty;
        if (tour != null)
        {
            tourStr = $"[{WriteTournamentInfoData(tour)}]";
        }

        builder.Append(" Competitors=").Append(competitors?.Count() ?? 0)
               .Append(" Sport=").Append(sport)
               .Append(" Schedule=[").Append(scheduleString).Append("]")
               .Append(" Groups=").Append(groupsStr)
               .Append(" Competitions=[").Append(scheduleString).Append("]")
               .Append(" Year=").Append(year)
               .Append(" SeasonCoverage=").Append(seasonCoverage?.SeasonId)
               .Append(" Round=").Append(round?.GetName(_culture))
               .Append(" Tournament=").Append(tourStr);

        return builder;
    }

    /// <summary>
    /// Writes the data of the provided <see cref="ITournamentInfo"/> to the <see cref="StringBuilder"/>
    /// </summary>
    /// <param name="tournament">The <see cref="ITournamentInfo"/> whose data is to be written</param>
    /// <returns>A <see cref="StringBuilder"/> containing string representation of the provided tournament</returns>
    private string WriteTournamentInfoData(ITournamentInfo tournament)
    {
        Guard.Argument(tournament, nameof(tournament)).NotNull();

        var tourSeasonStr = string.Empty;
        if (tournament.CurrentSeason != null)
        {
            tourSeasonStr = $"[{WriteCurrentSeasonInfoData(tournament.CurrentSeason)}]";
        }

        var builder = new StringBuilder();
        builder.Append("Id=").Append(tournament.Id)
               .Append(" Name=").Append(tournament.GetName(_culture))
               .Append(" Category=").Append(tournament.Category)
               .Append(" CurrentSeasonInfo=").Append(tourSeasonStr);

        return builder.ToString();
    }

    /// <summary>
    /// Writes the data of the provided <see cref="ICurrentSeasonInfo"/> to the <see cref="StringBuilder"/>
    /// </summary>
    /// <param name="season">The <see cref="ICurrentSeasonInfo"/> whose data is to be written</param>
    /// <returns>A <see cref="StringBuilder"/> containing string representation of the provided season</returns>
    private string WriteCurrentSeasonInfoData(ICurrentSeasonInfo season)
    {
        if (season == null)
        {
            return null;
        }

        var groups = season.Groups == null
                         ? "null"
                         : string.Join(",", season.Groups.Select((s, index) => $"{index}-{s.Name}:C{s.Competitors?.Count()}"));
        var competitors = season.Competitors == null
                              ? "null"
                              : string.Join(",", season.Competitors.Select(s => s.Id));
        var schedule = season.Schedule == null
                           ? "null"
                           : string.Join(",", season.Schedule.Select(s => s.Id));

        var builder = new StringBuilder();
        builder.Append("Id=").Append(season.Id)
               .Append(" Name=").Append(season.GetName(_culture))
               .Append(" StartDate=").Append(season.StartDate)
               .Append(" EndDate=").Append(season.EndDate)
               .Append(" Coverage=").Append(season.Coverage?.SeasonId)
               .Append(" CurrentRound=").Append(season.CurrentRound?.GetName(_culture))
               .Append(" Groups=").Append(groups)
               .Append(" Schedule=").Append(schedule)
               .Append(" Competitors=").Append(season.Competitors?.Count()).Append("::").Append(competitors);

        return builder.ToString();
    }

    /// <summary>
    /// Writes the data of the provided <see cref="IDraw"/> to the <see cref="StringBuilder"/>
    /// </summary>
    /// <param name="draw">The <see cref="IDraw"/> whose data is to be written</param>
    /// <returns>A <see cref="StringBuilder"/> containing string representation of the provided draw</returns>
    private StringBuilder WriteDrawData(IDraw draw)
    {
        Guard.Argument(draw, nameof(draw)).NotNull();

        var builder = new StringBuilder();
        AddEntityData(draw, builder);

        var lotteryId = _taskProcessor.GetTaskResult(draw.GetLotteryIdAsync());
        var drawStatus = _taskProcessor.GetTaskResult(draw.GetStatusAsync());
        var drawResults = _taskProcessor.GetTaskResult(draw.GetResultsAsync());
        var drawResultsString = drawResults == null
                                    ? "null"
                                    : string.Join(",", drawResults.Select(s => s.Value));

        builder.Append(" LotteryId=").Append(lotteryId)
               .Append(" DrawStatus=").Append(drawStatus)
               .Append(" DrawResults=[").Append(drawResultsString).Append("]");

        return builder;
    }

    /// <summary>
    /// Writes the data of the provided <see cref="ILottery"/> to the <see cref="StringBuilder"/>
    /// </summary>
    /// <param name="lottery">The <see cref="ILottery"/> whose data is to be written</param>
    /// <returns>A <see cref="StringBuilder"/> containing string representation of the provided lottery</returns>
    private StringBuilder WriteLotteryData(ILottery lottery)
    {
        Guard.Argument(lottery, nameof(lottery)).NotNull();

        var builder = new StringBuilder();
        AddEntityData(lottery, builder);

        var sportId = _taskProcessor.GetTaskResult(lottery.GetSportIdAsync());
        var sport = _taskProcessor.GetTaskResult(lottery.GetSportAsync());
        var category = _taskProcessor.GetTaskResult(lottery.GetCategoryAsync());
        var tournamentCoverage = _taskProcessor.GetTaskResult(lottery.GetTournamentCoverage());
        var bonusInfo = _taskProcessor.GetTaskResult(lottery.GetBonusInfoAsync());
        var drawInfo = _taskProcessor.GetTaskResult(lottery.GetDrawInfoAsync());
        var draws = _taskProcessor.GetTaskResult(lottery.GetScheduledDrawsAsync());
        var drawsString = draws == null
                              ? "null"
                              : string.Join(",", draws.Select(s => s));

        builder.Append(" SportId=").Append(sportId)
               .Append(" Sport=").Append(sport)
               .Append(" Category=").Append(category)
               .Append(" TournamentCoverage=").Append(tournamentCoverage?.LiveCoverage)
               .Append(" BonusInfo=").Append(bonusInfo)
               .Append(" DrawInfo=").Append(drawInfo)
               .Append(" Draws=[").Append(drawsString).Append("]");

        return builder;
    }

    private string WriteReference(IReference reference)
    {
        if (reference == null)
        {
            return string.Empty;
        }
        var props = reference.References == null
                        ? string.Empty
                        : reference.References.Aggregate(string.Empty, (s, pair) => $"{s}|{pair.Key}={pair.Value}");
        if (props.Length > 2)
        {
            props = props.Substring(1);
        }
        return $"[BetradarId={reference.BetradarId}, BetfairId={reference.BetfairId}, Refs={props}]";
    }

    /// <summary>
    /// Writes the data of the provided <see cref="ISportEvent"/> to the log
    /// </summary>
    /// <typeparam name="T">A <see cref="ISportEvent"/> derived type whose data is to be written</typeparam>
    /// <param name="entity">The <see cref="T"/> whose data is to be written</param>
    public void WriteData<T>(T entity) where T : ISportEvent
    {
        if (entity == null)
        {
            return;
        }

        try
        {
            var match = entity as IMatch;
            if (match != null)
            {
                _log.LogInformation(WriteMatchData(match).ToString());
                return;
            }

            var stage = entity as IStage;
            if (stage != null)
            {
                _log.LogInformation(WriteStageData(stage).ToString());
                return;
            }

            var basicTournament = entity as IBasicTournament;
            if (basicTournament != null)
            {
                _log.LogInformation(WriteBasicTournamentData(basicTournament).ToString());
                return;
            }

            var tournament = entity as ITournament;
            if (tournament != null)
            {
                _log.LogInformation(WriteTournamentData(tournament).ToString());
                return;
            }

            var season = entity as ISeason;
            if (season != null)
            {
                _log.LogInformation(WriteSeasonData(season).ToString());
                return;
            }

            var draw = entity as IDraw;
            if (draw != null)
            {
                _log.LogInformation(WriteDrawData(draw).ToString());
                return;
            }

            var lottery = entity as ILottery;
            if (lottery != null)
            {
                _log.LogInformation(WriteLotteryData(lottery).ToString());
                return;
            }

            // If non of the above, just write the ISportEvent properties
            var builder = new StringBuilder();
            AddEntityData(entity, builder);
            _log.LogInformation(builder.ToString());
        }
        catch (FeedSdkException)
        {
            _log.LogError("Error fetching info for eventId: {EventId}", entity.Id);
        }
        catch (ObjectDisposedException)
        {
            _log.LogWarning("Error fetching info for eventId: {EventId} due to SDK being disposed", entity.Id);
        }
        catch (AggregateException ex)
        {
            var communicationException = ex.InnerExceptions.FirstOrDefault(inner => inner is CommunicationException);
            if (communicationException != null)
            {
                _log.LogWarning(communicationException, "Communication exception occurred while fetching info for eventId: {EventId}", entity.Id);
                return;
            }
            _log.LogError(ex, "An unexpected exception occurred while fetching info for eventId: {EventId}", entity.Id);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "An unhandled exception occurred while fetching info for eventId: {EventId}", entity.Id);
        }
    }
}
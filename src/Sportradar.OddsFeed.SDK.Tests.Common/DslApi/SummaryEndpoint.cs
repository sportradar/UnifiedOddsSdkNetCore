// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using SR = Sportradar.OddsFeed.SDK.Tests.Common.StaticRandom;

namespace Sportradar.OddsFeed.SDK.Tests.Common.DslApi;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class SummaryEndpoint
{
    public static MatchSummaryEndpoint AsMatch => new MatchSummaryEndpoint();
    public static StageSummaryEndpoint AsStage => new StageSummaryEndpoint();
    public static ParentStageSummaryEndpoint AsParentStage => new ParentStageSummaryEndpoint();
    public static TournamentInfoSummaryEndpoint AsTournamentInfo => new TournamentInfoSummaryEndpoint();
    public static FixtureEndpoint AsFixture => new FixtureEndpoint();
    public static SportEventSummaryEndpoint AsSportEvent => new SportEventSummaryEndpoint();

    private static fixture BuildFixture(int id = 0, int subItemCount = 0)
    {
        if (subItemCount == 0)
        {
            subItemCount = SR.I(10);
        }

        var infos = new List<info>();
        for (var j = 0; j < subItemCount; j++)
        {
            var info = BuildInfo();
            if (infos.Find(i => i.key == info.key) == null)
            {
                infos.Add(info);
            }
        }

        var references = new List<referenceIdsReference_id>();
        for (var j = 0; j < subItemCount; j++)
        {
            var rc = BuildReference();
            if (references.Find(i => i.name == rc.name) == null)
            {
                references.Add(rc);
            }
        }

        var msg = new fixture
        {
            id = id == 0 ? SR.Urn("match", 10000).ToString() : SR.Urn(id, "match").ToString(),
            name = "Fixture " + SR.S1000,
            competitors = BuildTeamCompetitorList(subItemCount).ToArray(),
            coverage_info = GetCoverageInfo(),
            delayed_info = BuildDelayedInfo(),
            extra_info = infos.ToArray(),
            liveodds = SR.S1000,
            next_live_time = DateTime.Today.ToString(SdkInfo.Iso860124HFullFormat, CultureInfo.InvariantCulture), // should be like "2020-08-18T10:30:00+00:00"
            start_time_tbdSpecified = true,
            start_time_tbd = true,
            start_timeSpecified = true,
            start_time = DateTime.Today,
            scheduledSpecified = true,
            scheduled = DateTime.Today.AddDays(3),
            scheduled_endSpecified = true,
            scheduled_end = DateTime.Today.AddDays(4),
            reference_ids = references.ToArray(),
            replaced_by = SR.I100 < 70 ? $"sr:match:{SR.I1000}" : null,
            status = SR.S10000P
        };

        return msg;
    }

    private static matchSummaryEndpoint BuildMatchSummaryEndpoint(int id = 0, int subItemCount = 2)
    {
        var sportEvent = new sportEvent
        {
            competitors = BuildTeamCompetitorList(subItemCount).ToArray(),
            id = id == 0 ? SR.Urn("match", 10000).ToString() : SR.Urn(id, "match").ToString(),
            liveodds = "booked",
            scheduledSpecified = true,
            scheduled = new DateTime(2017, 2, 17),
            scheduled_endSpecified = true,
            scheduled_end = new DateTime(2017, 2, 18)
        };

        return new matchSummaryEndpoint
        {
            coverage_info = GetCoverageInfo(subItemCount),
            sport_event = sportEvent,
            sport_event_conditions = BuildSportEventConditions()
        };
    }

    public static sportEventConditions BuildSportEventConditions(int id = 0)
    {
        return new sportEventConditions
        {
            referee = BuildReferee(),
            attendance = SR.S1000,
            match_mode = SR.S1000,
            venue = BuildVenue(id),
            weather_info = BuildWeatherInfo()
        };
    }

    private static sportEvent BuildSportEventEndpoint(int id = 0, int subItemCount = 2)
    {
        var sportEvent = new sportEvent
        {
            competitors = BuildTeamCompetitorList(subItemCount).ToArray(),
            id = id == 0 ? SR.Urn("match", 10000).ToString() : SR.Urn(id, "match").ToString(),
            liveodds = "booked",
            scheduledSpecified = true,
            scheduled = new DateTime(DateTime.Now.Year, 2, 17),
            scheduled_endSpecified = true,
            scheduled_end = new DateTime(DateTime.Now.Year, 2, 18)
        };

        return sportEvent;
    }

    private static stageSummaryEndpoint BuildStageSummaryEndpoint(int id = 0, int subItemCount = 10)
    {
        var sportEvent = new sportEvent
        {
            competitors = BuildTeamCompetitorList(subItemCount).ToArray(),
            id = id == 0 ? SR.Urn("stage", 10000).ToString() : SR.Urn(id, "stage").ToString(),
            liveodds = "booked",
            scheduledSpecified = true,
            scheduled = new DateTime(DateTime.Now.Year, 2, 17),
            scheduled_endSpecified = true,
            scheduled_end = new DateTime(DateTime.Now.Year, 2, 18),
            stage_type = "race",
            status = "live"
        };

        return new stageSummaryEndpoint
        {
            sport_event = sportEvent,
            generated_at = DateTime.Now,
            generated_atSpecified = true
        };
    }

    private static parentStage BuildParentStageSummaryEndpoint(int id = 0)
    {
        return new parentStage
        {
            id = id == 0 ? SR.Urn("stage", 10000).ToString() : SR.Urn(id, "stage").ToString(),
            scheduledSpecified = true,
            scheduled = new DateTime(DateTime.Now.Year, 2, 17),
            scheduled_endSpecified = true,
            scheduled_end = new DateTime(DateTime.Now.Year, 2, 18),
            stage_type = "parent",
            type = "parent",
            name = "Parent stage " + id
        };
    }

    private static tournamentInfoEndpoint BuildTournamentInfoEndpoint(int id = 0, int subItemCount = 0)
    {
        if (subItemCount == -1)
        {
            subItemCount = SR.I(20);
        }

        var groups = new List<tournamentGroup>();
        for (var j = 0; j < subItemCount; j++)
        {
            groups.Add(BuildTournamentGroup(subItemCount));
        }

        var msg = new tournamentInfoEndpoint
        {
            tournament = BuildTournamentExtendedList(10).First(),
            competitors = BuildTournamentExtendedList(10).First().competitors,
            generated_at = DateTime.Today,
            generated_atSpecified = true,
            groups = groups.ToArray(),
            round = BuildMatchRound(),
            season = BuildSeasonExtended(),
            season_coverage_info = BuildSeasonCoverageInfo()
        };
        return msg;
    }

    public static List<tournamentExtended> BuildTournamentExtendedList(int count)
    {
        var tours = new List<tournamentExtended>();
        for (var i = 0; i < count; i++)
        {
            var tour = BuildTournamentExtended(i * 100);
            if (tours.Find(f => f.id == tour.id) == null)
            {
                tours.Add(tour);
            }
        }
        return tours;
    }

    public static tournamentExtended BuildTournamentExtended(int id = 0, int subItemCount = 0)
    {
        if (subItemCount == -1)
        {
            subItemCount = SR.I(20);
        }
        var msg = new tournamentExtended
        {
            id = id == 0 ? SR.Urn("tournament").ToString() : SR.Urn(id, "tournament").ToString(),
            name = "Tournament name " + SR.S(1000),
            scheduled = DateTime.Now,
            scheduledSpecified = true,
            scheduled_end = DateTime.Today,
            scheduled_endSpecified = true,
            category = BuildCategory(id * id),
            sport = BuildSport(id),
            tournament_length = BuildTournamentLength(),
            current_season = BuildCurrentSeason(id * 3),
            season_coverage_info = BuildSeasonCoverageInfo(id * 15),
            competitors = BuildTeamList(subItemCount).ToArray()
        };
        return msg;
    }

    public static category BuildCategory(int id = 0)
    {
        id = id == 0 ? SR.I1000 : id;
        return new category
        {
            id = SR.Urn(id, "category").ToString(),
            name = $"Category name {id}",
            country_code = "en"
        };
    }

    public static sport BuildSport(int id = 0)
    {
        return new sport
        {
            id = id == 0 ? SR.Urn("sport", 100).ToString() : SR.Urn(id, "sport").ToString(),
            name = $"Sport {SR.S1000}"
        };
    }

    public static currentSeason BuildCurrentSeason(int id = 0)
    {
        return new currentSeason
        {
            id = id == 0 ? SR.Urn("season", 100000).ToString() : SR.Urn(id, "season").ToString(),
            name = $"Season name {SR.S1000}"
        };
    }

    public static tournamentGroup BuildTournamentGroup(int subItemCount = 0)
    {
        if (subItemCount == -1)
        {
            subItemCount = SR.I(20);
        }
        return new tournamentGroup
        {
            name = "Group " + SR.S1000,
            competitor = BuildTeamList(subItemCount).ToArray()
        };
    }

    public static seasonCoverageInfo BuildSeasonCoverageInfo(int id = 0)
    {
        return new seasonCoverageInfo
        {
            season_id = id == 0 ? SR.Urn("season", 10000).ToString() : SR.Urn(id, "season").ToString(),
            max_coverage_level = $"{SR.S1000}",
            max_covered = SR.I1000,
            scheduled = SR.I1000,
            max_coveredSpecified = true,
            min_coverage_level = SR.S1000,
            played = SR.I1000
        };
    }

    public static seasonExtended BuildSeasonExtended(int id = 0)
    {
        return new seasonExtended
        {
            id = id == 0 ? SR.Urn("season", 10000).ToString() : SR.Urn(id, "season").ToString(),
            name = "Season " + SR.S1000,
            end_date = DateTime.Now.AddDays(SR.I100),
            start_date = DateTime.Now,
            tournament_id = SR.Urn("tournament", SR.I100).ToString(),
            year = DateTime.Now.Year.ToString()
        };
    }

    public static tournamentLength BuildTournamentLength()
    {
        return new tournamentLength
        {
            start_date = DateTime.Now,
            start_dateSpecified = true,
            end_date = DateTime.Today.AddDays(1),
            end_dateSpecified = true
        };
    }

    private static info BuildInfo()
    {
        return new info
        {
            key = SR.S10000P,
            value = SR.S1000
        };
    }

    public static matchRound BuildMatchRound(int id = 0)
    {
        return new matchRound
        {
            name = "Match round " + (id == 0 ? SR.S10000 : id.ToString()),
            cup_round_match_number = 1,
            cup_round_match_numberSpecified = true,
            cup_round_matches = 2,
            cup_round_matchesSpecified = true,
            number = SR.I100,
            numberSpecified = true,
            type = "match type 1"
        };
    }

    public static tvChannel BuildTvChannel(bool startTimeSpecified = true)
    {
        return new tvChannel
        {
            name = "Name " + SR.S10000P,
            start_time = DateTime.Now,
            start_timeSpecified = startTimeSpecified
        };
    }

    public static venue BuildVenue(int id = 0)
    {
        if (id == 0)
        {
            id = SR.I1000;
        }
        return new venue
        {
            id = SR.Urn(id, "venue").ToString(),
            name = "Venue name " + id,
            capacity = SR.I1000,
            capacitySpecified = true,
            city_name = "City " + SR.S1000,
            country_name = "Country " + SR.S1000,
            map_coordinates = "Coordinates" + SR.S1000,
            state = "PA"
        };
    }

    public static weatherInfo BuildWeatherInfo()
    {
        var msg = new weatherInfo
        {
            pitch = SR.S1000,
            temperature_celsius = SR.I100,
            temperature_celsiusSpecified = true,
            weather_conditions = SR.S1000,
            wind_advantage = SR.S1000,
            wind = SR.S1000
        };
        return msg;
    }

    public static List<teamCompetitor> BuildTeamCompetitorList(int count)
    {
        var teams = new List<teamCompetitor>();
        for (var j = 0; j < count; j++)
        {
            var team = CompetitorProfileEndpoint.BuildTeamCompetitor(j + 1);
            if (teams.Find(i => i.id == team.id) == null)
            {
                teams.Add(team);
            }
        }
        return teams;
    }

    public static List<teamCompetitor> BuildTeamCompetitorWithPlayersList(int count, int playerSize)
    {
        var teams = BuildTeamCompetitorList(count);
        var i = 1;
        foreach (var teamCompetitor in teams)
        {
            teamCompetitor.players = BuildPlayerCompetitorList(i, playerSize);
            i += playerSize;
        }
        return teams;
    }

    public static List<team> BuildTeamList(int count)
    {
        var teams = new List<team>();
        for (var j = 0; j < count; j++)
        {
            var team = CompetitorProfileEndpoint.AsCompetitorProfile.Id(j + 1).Raw.competitor;
            if (teams.Find(i => i.id == team.id) == null)
            {
                teams.Add(team);
            }
        }
        return teams;
    }

    public static playerCompetitor BuildPlayerCompetitor(int id = 1)
    {
        var playerId = id == 0 ? SR.Urn("player").ToString() : SR.Urn(id, "player").ToString();
        return new playerCompetitor
        {
            id = playerId,
            name = "Player " + playerId,
            abbreviation = "P" + playerId,
            nationality = "nat" + SR.S1000
        };
    }

    public static playerCompetitor[] BuildPlayerCompetitorList(int startId = 1, int size = 1)
    {
        var players = new Collection<playerCompetitor>();
        for (var i = startId; i < startId + size; i++)
        {
            players.Add(BuildPlayerCompetitor(i));
        }
        return players.ToArray();
    }

    public static coverage GetCoverage()
    {
        return new coverage
        {
            includes = "coverage includes " + SR.S1000
        };
    }

    public static coverageInfo GetCoverageInfo(int subItemCount = 0)
    {
        if (subItemCount == 0)
        {
            subItemCount = SR.I(20);
        }
        var items = new List<coverage>();
        for (var j = 0; j < subItemCount; j++)
        {
            items.Add(GetCoverage());
        }

        return new coverageInfo
        {
            level = SR.S100,
            live_coverage = true,
            coverage = items.ToArray(),
            covered_from = "tv"
        };
    }

    public static delayedInfo BuildDelayedInfo(int id = 0)
    {
        return new delayedInfo
        {
            id = id == 0 ? SR.I1000 : id,
            description = SR.S1000
        };
    }

    public static referenceIdsReference_id BuildReference()
    {
        return new referenceIdsReference_id
        {
            name = SR.S10000P,
            value = SR.S10000
        };
    }

    public static referee BuildReferee(int id = 0)
    {
        return new referee
        {
            id = id == 0 ? SR.Urn("player").ToString() : SR.Urn(id, "player").ToString(),
            name = "Name " + SR.S1000,
            nationality = "Nationality " + SR.S100
        };
    }

    public class MatchSummaryEndpoint
    {
        public matchSummaryEndpoint Raw { get; } = BuildMatchSummaryEndpoint(1);
        internal MatchDto Dto => new MatchDto(Raw);
    }

    public class StageSummaryEndpoint
    {
        public stageSummaryEndpoint Raw { get; } = BuildStageSummaryEndpoint(1);
        internal StageDto Dto => new StageDto(Raw);
    }

    public class ParentStageSummaryEndpoint
    {
        public parentStage Raw { get; } = BuildParentStageSummaryEndpoint(1);
        internal StageDto Dto => new StageDto(Raw);
    }

    public class SportEventSummaryEndpoint
    {
        public sportEvent Raw { get; } = BuildSportEventEndpoint(1);
        internal MatchDto Dto => new MatchDto(Raw);
    }

    public class TournamentInfoSummaryEndpoint
    {
        public tournamentInfoEndpoint Raw { get; private set; } = BuildTournamentInfoEndpoint(1, 0);
        internal TournamentInfoDto Dto => new TournamentInfoDto(Raw);

        public TournamentInfoSummaryEndpoint Group(int i)
        {
            Raw = BuildTournamentInfoEndpoint(1, i);
            return this;
        }
    }

    public class FixtureEndpoint
    {
        public fixture Raw { get; } = BuildFixture(1);
        internal FixtureDto Dto => new FixtureDto(Raw, DateTime.Now);
    }
}

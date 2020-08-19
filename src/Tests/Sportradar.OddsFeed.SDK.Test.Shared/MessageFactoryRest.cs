/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Messages.REST;
using SR = Sportradar.OddsFeed.SDK.Test.Shared.StaticRandom;
// ReSharper disable UnusedMember.Local

namespace Sportradar.OddsFeed.SDK.Test.Shared
{
    /// <summary>
    /// Class used to manually create REST entities
    /// </summary>
    public static class MessageFactoryRest
    {
        public static bookmaker_details GetBookmakerDetails(int id = 0, bool idSpecified = true, bool expireAtSpecified = true, bool responseCodeSpecified = true)
        {
            return new bookmaker_details
            {
                bookmaker_id = id == 0 ? SR.I1000 : id,
                bookmaker_idSpecified = idSpecified,
                message = SR.S1000,
                expire_at = DateTime.Now,
                expire_atSpecified = expireAtSpecified,
                response_code = response_code.ACCEPTED,
                response_codeSpecified = responseCodeSpecified,
                virtual_host = SR.S1000
            };
        }

        public static category GetCategory(int id = 0)
        {
            return new category
            {
                id = id == 0 ? SR.Urn("category").ToString() : SR.Urn(id, "category").ToString(),
                name = $"Category name {SR.S1000}"
            };
        }

        public static currentSeason GetCurrentSeason(int id = 0)
        {
            return new currentSeason
            {
                id = id == 0 ? SR.Urn("season", 100000).ToString() : SR.Urn(id, "season").ToString(),
                name = $"Season name {SR.S1000}"
            };
        }

        public static competitorProfileEndpoint GetCompetitorProfileEndpoint(int id = 0, int playerCount = 0, IDictionary<string, string> referenceIds = null)
        {
            if (playerCount == 0)
            {
                playerCount = SR.I(20);
            }

            var players = new List<playerExtended>();
            for (var j = 0; j < playerCount; j++)
            {
                players.Add(GetPlayerExtended());
            }

            return new competitorProfileEndpoint
            {
                competitor = new teamExtended
                {
                    id = id == 0 ? SR.Urn("competitor", 100000).ToString() : SR.Urn(id, "competitor").ToString(),
                    name = "my name",
                    abbreviation = SR.S1000,
                    @virtual = true,
                    virtualSpecified = true,
                    country = SR.S1000,
                    state = "PA",
                    reference_ids = referenceIds?.Select(s => new competitorReferenceIdsReference_id { name = s.Key, value = s.Value }).ToArray()
                },
                generated_at = DateTime.Today,
                generated_atSpecified = true,
                players = players.ToArray()
            };
        }

        public static simpleTeamProfileEndpoint GetSimpleTeamCompetitorProfileEndpoint(int id = 0, IDictionary<string, string> referenceIds = null)
        {
            return new simpleTeamProfileEndpoint
            {
                competitor = new team
                {
                    id = id == 0 ? SR.Urn("simpleteam", 100000).ToString() : SR.Urn(id, "simpleteam").ToString(),
                    name = "my name",
                    abbreviation = SR.S1000,
                    @virtual = true,
                    virtualSpecified = true,
                    country = SR.S1000,
                    state = "PA",
                    reference_ids = referenceIds?.Select(s => new competitorReferenceIdsReference_id { name = s.Key, value = s.Value }).ToArray()
                },
                generated_at = DateTime.Today,
                generated_atSpecified = true,
            };
        }

        public static coverage GetCoverage()
        {
            return new coverage {includes = "coverage includes " + SR.S1000};
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

        public static delayedInfo GetDelayedInfo(int id = 0)
        {
            return new delayedInfo
            {
                id = id == 0 ? SR.I1000 : id,
                description = SR.S1000
            };
        }

        public static desc_outcomesOutcome GetDescOutcomesOutcome(int id = 0)
        {
            return new desc_outcomesOutcome
            {
                id = id == 0 ? SR.Urn("market").ToString() : SR.Urn(id, "market").ToString(),
                name = "Name " + SR.S(),
                description = "Description " + SR.S1000
            };
        }

        public static desc_specifiersSpecifier GetDescSpecifiersSpecifier()
        {
            return new desc_specifiersSpecifier
            {
                type = "Type " + SR.S100,
                name = "Name " + SR.S(),
                description = "Description " + SR.S1000
            };
        }

        public static desc_market GetDescMarket(int id = 0, int subItemCount = 0)
        {
            if (subItemCount == 0)
            {
                subItemCount = SR.I(20);
            }

            var mappings = new List<mappingsMapping>();
            var outcomes = new List<desc_outcomesOutcome>();
            var specifiers = new List<desc_specifiersSpecifier>();
            for (var i = 0; i < subItemCount; i++)
            {
                mappings.Add(GetMappingsMapping());
                outcomes.Add(GetDescOutcomesOutcome());
                specifiers.Add(GetDescSpecifiersSpecifier());
            }

            var msg = new desc_market
            {
                id = id == 0 ? SR.I1000 : id,
                name = "Market " + SR.S1000,
                description = "Description " + SR.S1000,
                variant = "Variant " + SR.S1000,
                mappings = mappings.ToArray(),
                outcomes = outcomes.ToArray(),
                specifiers = specifiers.ToArray()
            };
            return msg;
        }

        public static desc_market GetDescMarket(desc_market baseMsg)
        {
            var newMsg = new desc_market
            {
                id = baseMsg.id,
                name = "Market " + SR.S1000,
                description = "Description " + SR.S1000,
                variant = baseMsg.variant,
                mappings = new mappingsMapping[baseMsg.mappings.Length],
                outcomes = new desc_outcomesOutcome[baseMsg.outcomes.Length],
                specifiers = new desc_specifiersSpecifier[baseMsg.specifiers.Length]
            };
            baseMsg.mappings.CopyTo(newMsg.mappings, 0);
            baseMsg.outcomes.CopyTo(newMsg.outcomes, 0);
            baseMsg.specifiers.CopyTo(newMsg.specifiers, 0);
            foreach (var m in newMsg.outcomes)
            {
                m.name = "Outcome " + SR.S();
                m.description = "Desc " + SR.S();
            }
            foreach (var m in newMsg.specifiers)
            {
                m.name = "Specifier name " + SR.S();
                m.description = "Desc " + SR.S();
            }
            return newMsg;
        }

        public static fixture GetFixture(int id = 0, int subItemCount = 0)
        {
            if (subItemCount == 0)
            {
                subItemCount = SR.I(10);
            }

            var infos = new List<info>();
            for (var j = 0; j < subItemCount; j++)
            {
                var info = GetInfo();
                if (infos.Find(i => i.key == info.key) == null)
                {
                    infos.Add(info);
                }
            }

            var references = new List<referenceIdsReference_id>();
            for (var j = 0; j < subItemCount; j++)
            {
                var rc = GetReference();
                if (references.Find(i => i.name == rc.name) == null)
                {
                    references.Add(rc);
                }
            }

            var msg = new fixture
            {
                id = id == 0 ? SR.Urn("match", 10000).ToString() : SR.Urn(id, "match").ToString(),
                name = "Fixture " + SR.S1000,
                competitors = GetTeamCompetitorList(subItemCount).ToArray(),
                coverage_info = GetCoverageInfo(),
                delayed_info = GetDelayedInfo(),
                extra_info = infos.ToArray(),
                liveodds = SR.S1000,
                next_live_time = DateTime.Today.ToString(SdkInfo.ISO8601_24H_FullFormat, CultureInfo.InvariantCulture), // should be like "2020-08-18T10:30:00+00:00"
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

        public static matchSummaryEndpoint GetMatchSummaryEndpoint(int id = 0, int subItemCount = 2)
        {
            var sportEvent = new sportEvent
            {
                competitors = GetTeamCompetitorList(subItemCount).ToArray(),
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
                sport_event_conditions = GetSportEventConditions()
            };
        }

        public static tournamentGroup GetGroup()
        {
            return new tournamentGroup
            {
                name = "Group " + SR.S1000
            };
        }

        public static info GetInfo()
        {
            return new info
            {
                key = SR.S10000P,
                value = SR.S1000
            };
        }

        public static referenceIdsReference_id GetReference()
        {
            return new referenceIdsReference_id
            {
                name = SR.S10000P,
                value = SR.S10000
            };
        }

        public static competitorReferenceIdsReference_id GetReferenceCompetitor()
        {
            return new competitorReferenceIdsReference_id
            {
                name = SR.S10000P,
                value = SR.S10000
            };
        }

        public static matchRound GetMatchRound(int id = 0)
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

        public static mappingsMapping GetMappingsMapping(int id = 0, int subItemCount = 0)
        {
            if (subItemCount == 0)
            {
                subItemCount = SR.I(20);
            }

            var outcomes = new List<mappingsMappingMapping_outcome>();
            for (var j = 0; j < subItemCount; j++)
            {
                outcomes.Add(GetMappingsMappingMappingOutcome());
            }
            var producerId = SR.I(10);
            return new mappingsMapping
            {
                market_id = id == 0 ? SR.S1000 : id.ToString(),
                product_id = producerId,
                product_ids = $"{producerId}|{producerId+1}",
                sport_id = SR.Urn("sport", SR.I100).ToString(),
                sov_template = SR.S1000,
                valid_for = "setnr=" + SR.I100,
                mapping_outcome = SR.I100 > 50 ? null : outcomes.ToArray()
            };
        }

        public static mappingsMappingMapping_outcome GetMappingsMappingMappingOutcome(int id = 0)
        {
            return new mappingsMappingMapping_outcome
            {
                outcome_id = SR.S10000,
                product_outcome_id = SR.S1000,
                product_outcome_name = SR.S1000
            };
        }

        public static playerExtended GetPlayerExtended(int id = 0)
        {
            return new playerExtended
            {
                name = SR.S1000,
                id = id == 0 ? SR.Urn("player").ToString() : SR.Urn(id, "player").ToString(),
                weight = SR.I(150),
                heightSpecified = true,
                jersey_number = 60,
                jersey_numberSpecified = true,
                nationality = "nat " + SR.S1000,
                type = SR.S1000,
                height = SR.I(200),
                date_of_birth = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                weightSpecified = true
            };
        }

        public static referee GetReferee(int id = 0)
        {
            return new referee
            {
                id = id == 0 ? SR.Urn("player").ToString() : SR.Urn(id, "player").ToString(),
                name = "Name " + SR.S1000,
                nationality = "Nationality " + SR.S100,
            };
        }

        public static seasonCoverageInfo GetSeasonCoverageInfo(int id = 0)
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

        public static seasonExtended GetSeasonExtended(int id = 0)
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

        public static sport GetSport(int id = 0)
        {
            return new sport
            {
                id = id == 0 ? SR.Urn("sport", 100).ToString() : SR.Urn(id, "sport").ToString(),
                name = $"Sport {SR.S1000}",
            };
        }

        public static sportEventConditions GetSportEventConditions(int id = 0)
        {
            return new sportEventConditions
            {
                referee = GetReferee(),
                attendance = SR.S1000,
                match_mode = SR.S1000,
                venue = GetVenue(),
                weather_info = GetWeatherInfo()
            };
        }

        public static team GetTeam(int id = 0)
        {
            return new team
            {
                id = id == 0 ? SR.Urn("team", 100000).ToString() : SR.Urn(id, "team").ToString(),
                name = "Team " + SR.I1000,
                abbreviation = SR.S1000,
                @virtual = true,
                virtualSpecified = true,
                country = SR.S1000,
                state = "PA"
            };
        }

        public static teamCompetitor GetTeamCompetitor(int id = 0)
        {
            var references = new List<competitorReferenceIdsReference_id>();
            for (var j = 0; j < 5; j++)
            {
                var rc = GetReferenceCompetitor();
                if (references.Find(i => i.name == rc.name) == null)
                {
                    references.Add(rc);
                }
            }

            return new teamCompetitor
            {
                id = id == 0 ? SR.Urn("competitor", 100000).ToString() : SR.Urn(id, "competitor").ToString(),
                name = "Competitor " + SR.S1000,
                abbreviation = SR.S1000,
                @virtual = true,
                country = SR.S1000,
                virtualSpecified = true,
                qualifier = SR.S1000,
                country_code = SR.S1000,
                reference_ids = references.ToArray(),
                divisionSpecified = true,
                division = SR.I100,
                state = "PA"
            };
        }

        public static teamExtended GetTeamExtended(int id = 0)
        {
            return new teamExtended
            {
                id = id == 0 ? SR.Urn("competitor", 1000).ToString() : SR.Urn(id, "competitor").ToString(),
                name = "Team " + SR.I1000,
                abbreviation = SR.S1000,
                @virtual = true,
                virtualSpecified = true,
                country = SR.S1000,
                state = "PA"
            };
        }

        public static tournament GetTournament(int id = 0)
        {
            return new tournament
            {
                id = id == 0 ? SR.Urn("tournament").ToString() : SR.Urn(id, "tournament").ToString(),
                name = "Tournament name " + SR.S100,
                scheduled = DateTime.Now,
                scheduledSpecified = true,
                scheduled_end = DateTime.Today,
                scheduled_endSpecified = true,
                category = GetCategory(),
                sport = GetSport()
            };
        }

        public static tournamentExtended GetTournamentExtended(int id = 0, int subItemCount = 0)
        {
            if (subItemCount == 0)
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
                category = GetCategory(),
                sport = GetSport(),
                tournament_length = GetTournamentLength(),
                current_season = GetCurrentSeason(),
                season_coverage_info = GetSeasonCoverageInfo(),
                competitors = GetTeamList(subItemCount).ToArray()
            };
            return msg;
        }

        public static tournamentGroup GetTournamentGroup(int subItemCount = 0)
        {
            if (subItemCount == 0)
            {
                subItemCount = SR.I(20);
            }
            return new tournamentGroup
            {
                name = "Group " + SR.S1000,
                competitor = GetTeamList(subItemCount).ToArray()
            };
        }

        public static tournamentLength GetTournamentLength()
        {
            return new tournamentLength
            {
                start_date = DateTime.Now,
                start_dateSpecified = true,
                end_date = DateTime.Today.AddDays(1),
                end_dateSpecified = true
            };
        }

        public static tournamentInfoEndpoint GeTournamentInfoEndpoint(int id = 0, int subItemCount = 0)
        {
            if (subItemCount == 0)
            {
                subItemCount = SR.I(20);
            }

            var groups = new List<tournamentGroup>();
            for (var j = 0; j < subItemCount; j++)
            {
                groups.Add(GetTournamentGroup());
            }

            var msg = new tournamentInfoEndpoint
            {
                tournament = GetTournamentExtendedList(10).First(),
                competitors = GetTournamentExtendedList(10).First().competitors,
                generated_at = DateTime.Today,
                generated_atSpecified = true,
                groups = groups.ToArray(),
                round = GetMatchRound(),
                season = GetSeasonExtended(),
                season_coverage_info = GetSeasonCoverageInfo()
            };
            return msg;
        }

        public static tvChannel GetTvChannel(bool startTimeSpecified = true)
        {
            return new tvChannel
            {
                name = "Name " + SR.S10000P,
                start_time = DateTime.Now,
                start_timeSpecified = startTimeSpecified
            };
        }

        public static venue GetVenue(int id = 0)
        {
            var msg = new venue
            {
                id = id == 0 ? SR.Urn("venue").ToString() : SR.Urn(id, "venue").ToString(),
                name = "Venue name " + SR.S1000,
                capacity = SR.I1000,
                capacitySpecified = true,
                city_name = "City " + SR.S1000,
                country_name = "Country " + SR.S1000,
                map_coordinates = "Coordinates" + SR.S1000,
                state = "PA"
            };
            return msg;
        }

        public static weatherInfo GetWeatherInfo()
        {
            var msg = new weatherInfo
            {
                pitch = SR.S1000,
                temperature_celsius = SR.I100,
                temperature_celsiusSpecified = true,
                weather_conditions = SR.S1000,
                wind_advantage = SR.S1000,
                wind = SR.S1000,
            };
            return msg;
        }

        public static List<tournamentExtended> GetTournamentExtendedList(int count)
        {
            var tours = new List<tournamentExtended>();
            for (var i = 0; i < count; i++)
            {
                var tour = GetTournamentExtended();
                if (tours.Find(f => f.id == tour.id) == null)
                {
                    tours.Add(tour);
                }
            }
            return tours;
        }
        public static List<teamCompetitor> GetTeamCompetitorList(int count)
        {
            var teams = new List<teamCompetitor>();
            for (var j = 0; j < count; j++)
            {
                var team = GetTeamCompetitor();
                if (teams.Find(i => i.id == team.id) == null)
                {
                    teams.Add(team);
                }
            }
            return teams;
        }

        public static List<team> GetTeamList(int count)
        {
            var teams = new List<team>();
            for (var j = 0; j < count; j++)
            {
                var team = GetTeam();
                if (teams.Find(i => i.id == team.id) == null)
                {
                    teams.Add(team);
                }
            }
            return teams;
        }
    }
}

/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;
using RMF = Sportradar.OddsFeed.SDK.Test.Shared.MessageFactoryRest;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class RestMessageMappingTest
    {
        [TestMethod]
        public void BookmakerDetailsDTOMappingTest()
        {
            var msg1 = RMF.GetBookmakerDetails(10);
            var msg2 = RMF.GetBookmakerDetails(0, false, false, false);

            var details1 = new BookmakerDetails(new BookmakerDetailsDTO(msg1, TimeSpan.Zero));
            var details2 = new BookmakerDetails(new BookmakerDetailsDTO(msg2, TimeSpan.Zero));

            Assert.AreEqual(msg1.bookmaker_id, details1.BookmakerId);
            Assert.AreEqual(msg1.message, details1.Message);
            Assert.AreEqual(msg1.expire_at, details1.ExpireAt);
            Assert.AreEqual(msg1.response_code.ToString().ToLower(), details1.ResponseCode.ToString().ToLower());
            Assert.AreEqual(msg1.virtual_host, details1.VirtualHost);

            Assert.AreEqual(msg2.bookmaker_id, details2.BookmakerId);
            Assert.AreNotEqual(0, details2.BookmakerId);
            Assert.AreEqual(msg2.message, details2.Message);
            Assert.AreNotEqual(msg2.expire_at, details2.ExpireAt);
            Assert.AreEqual(DateTime.MinValue, details2.ExpireAt);
            Assert.AreNotEqual(msg2.response_code.ToString(), details2.ResponseCode);
            Assert.IsNull(details2.ResponseCode);
            Assert.AreEqual(msg2.virtual_host, details2.VirtualHost);
        }

        [TestMethod]
        public void CategoryDTOMappingTest()
        {
            var tours = RMF.GetTournamentExtendedList(100);
            var dto = new CategoryDTO("sr:category:1", "category name", "en", tours);

            Assert.AreEqual("sr:category:1", dto.Id.ToString());
            Assert.AreEqual("category name", dto.Name);
            Assert.AreEqual(tours.Count, dto.Tournaments.Count());

            for (var i = 0; i < tours.Count; i++)
            {
                ValidateTournamentExtended(tours[i], dto.Tournaments.ToArray()[i]);
            }
        }

        [TestMethod]
        public void CompetitorDTOMappingTest()
        {
            var msg = RMF.GetTeam();
            ValidateTeam(msg, new CompetitorDTO(msg));
        }

        [TestMethod]
        public void CompetitorProfileDTOMappingTest()
        {
            var msg = RMF.GetCompetitorProfileEndpoint(0, 10);

            var dto = new CompetitorProfileDTO(msg);
            ValidateTeamExtended(msg.competitor, dto.Competitor); //TODO: missing extended properties
            Assert.AreEqual(msg.players.Length, dto.Players.Count());

            for (var i = 0; i < msg.players.Length; i++)
            {
                ValidatePlayerExtended(msg.players[i], dto.Players.ToArray()[i]);
            }
        }

        [TestMethod]
        public void CoverageInfoDTOMappingTest()
        {
            var coverageInfoFeed = RMF.GetCoverageInfo(10);
            var coverageInfo = new CoverageInfo(new CoverageInfoDTO(coverageInfoFeed));

            Assert.IsNotNull(coverageInfo);
            Assert.AreEqual(coverageInfoFeed.level, coverageInfo.Level);
            Assert.AreEqual(coverageInfoFeed.live_coverage, coverageInfo.IsLive);
            Assert.AreEqual(CoveredFrom.Tv, coverageInfo.CoveredFrom);
            Assert.AreEqual(coverageInfoFeed.coverage.Length, coverageInfo.Includes.Count());
            Assert.AreNotEqual(coverageInfo.Includes.ToList()[0], coverageInfo.Includes.ToList()[2]);
        }

        [TestMethod]
        public void FixtureDTOMappingTest()
        {
            var msg = RMF.GetFixture();
            var dto = new FixtureDTO(msg, null);

            Assert.AreEqual(msg.id, dto.Id.ToString());
            Assert.AreEqual(msg.name, dto.Name);
            //Assert.AreEqual(msg.liveodds, dto.LiveOdds); //TODO: missing liveodds
            Assert.AreEqual(SdkInfo.ParseDate(msg.next_live_time), dto.NextLiveTime);
            Assert.AreEqual(!string.IsNullOrEmpty(msg.replaced_by) ? URN.Parse(msg.replaced_by) : null, dto.ReplacedBy);
            Assert.AreEqual(msg.start_time_tbdSpecified ? (bool?)msg.start_time_tbd : null, dto.StartTimeTBD);
            Assert.AreEqual(msg.competitors.Length, dto.Competitors.Count());
            for (var i = 0; i < msg.competitors.Length; i++)
            {
                ValidateTeamCompetitor(msg.competitors[i], dto.Competitors.ToList()[i]);
            }
            ValidateCoverageInfo(msg.coverage_info, dto.CoverageInfo);
            Assert.AreEqual(msg.delayed_info.id, dto.DelayedInfo.Id);

            Assert.AreEqual(msg.extra_info.Length, dto.ExtraInfo.ToList().Count);
            for (var i = 0; i < msg.extra_info.Length; i++)
            {
                Assert.IsTrue(dto.ExtraInfo.ContainsKey(msg.extra_info[i].key));
                string eiValue;
                dto.ExtraInfo.TryGetValue(msg.extra_info[i].key, out eiValue);
                Assert.AreEqual(msg.extra_info[i].value, eiValue);
            }
        }

        [TestMethod]
        public void GroupDTOMappingTest()
        {
            var msg = RMF.GetTournamentGroup();
            var dto = new GroupDTO(msg);

            ValidateTournamentGroup(msg, dto);
        }

        [TestMethod]
        public void MarketDescriptionDTOMappingTest()
        {
            var msg = RMF.GetDescMarket();
            var dto = new MarketDescriptionDTO(msg);

            Assert.AreEqual(msg.id, dto.Id);
            Assert.AreEqual(msg.name, dto.Name);
            Assert.AreEqual(msg.description, dto.Description);
            //Assert.AreEqual(msg.variant, dto.Variant); //TODO: check if variant is missing in dto?
            Assert.AreEqual(msg.mappings.Length, dto.Mappings.Count());
            Assert.AreEqual(msg.outcomes.Length, dto.Outcomes.Count());
            Assert.AreEqual(msg.specifiers.Length, dto.Specifiers.Count());

            for (var i = 0; i < msg.mappings.Length; i++)
            {
                ValidateMapping(msg.mappings[i], dto.Mappings.ToArray()[i]);
            }

            for (var i = 0; i < msg.outcomes.Length; i++)
            {
                ValidateOutcome(msg.outcomes[i], dto.Outcomes.ToArray()[i]);
            }

            for (var i = 0; i < msg.specifiers.Length; i++)
            {
                ValidateSpecifier(msg.specifiers[i], dto.Specifiers.ToArray()[i]);
            }
        }

        [TestMethod]
        public void MarketMappingDTOMappingTest()
        {
            var msg = RMF.GetMappingsMapping();
            var dto = new MarketMappingDTO(msg);

            ValidateMapping(msg, dto);
        }

        [TestMethod]
        public void OutcomeDescriptionDTOMappingTest()
        {
            var msg = RMF.GetDescOutcomesOutcome();
            var dto = new OutcomeDescriptionDTO(msg);

            ValidateOutcome(msg, dto);
        }

        [TestMethod]
        public void PlayerDTOTest()
        {
            var sportEntityDTO = new SportEntityDTO("sr:player:1", "Sport Entity Name");
            var playerCI = new SportEntityCI(sportEntityDTO);

            Assert.IsNotNull(playerCI);
            Assert.AreEqual(sportEntityDTO.Id, playerCI.Id);
        }

        [TestMethod]
        public void PlayerProfileDTOTest()
        {
            var msg = RMF.GetPlayerExtended();
            var dto = new PlayerProfileDTO(msg, null);

            ValidatePlayerExtended(msg, dto);
        }

        [TestMethod]
        public void ProductInfoTest()
        {
            new CacheItemMergeTest().ProductInfoTest();
        }

        [TestMethod]
        public void ProductInfoLinkTest()
        {
            new CacheItemMergeTest().ProductInfoLinkTest();
        }

        [TestMethod]
        public void RefereeTest()
        {
            new CacheItemMergeTest().RefereeTest();
        }

        [TestMethod]
        public void RoundTest()
        {
            new CacheItemMergeTest().RoundMergeTest();
        }

        [TestMethod]
        public void SeasonCoverageInfoDTOTest()
        {
            var msg = RMF.GetSeasonCoverageInfo();
            var dto = new SeasonCoverageDTO(msg);

            ValidateSeasonCoverageInfo(msg, dto);
        }

        [TestMethod]
        public void SeasonTest()
        {
            new CacheItemMergeTest().SeasonTest();
        }

        [TestMethod]
        public void SpecifierDTOMappingTest()
        {
            var msg = RMF.GetDescSpecifiersSpecifier();
            var dto = new SpecifierDTO(msg);

            ValidateSpecifier(msg, dto);
        }

        [TestMethod]
        public void SportDTOMappingTest()
        {
            var tours = RMF.GetTournamentExtendedList(100);

            var dto = new SportDTO("sr:sport:1", "name", tours);

            Assert.AreEqual(tours.Count, dto.Categories.Select(c => c.Tournaments).Count());

            foreach (var t in tours)
            {
                var cat = dto.Categories.ToList().Find(c => c.Id.ToString() == t.category.id);
                Assert.IsNotNull(cat, "Category missing.");
                Assert.AreEqual(t.category.id, cat.Id.ToString(), "Category id wrong.");
                Assert.AreEqual(t.category.name, cat.Name, "Category name wrong.");

                ValidateTournamentExtended(t, cat.Tournaments.First(c => c.Id.ToString() == t.id));
            }
        }

        [TestMethod]
        public void StreamingChannelTest()
        {
            new CacheItemMergeTest().StreamingChannelTest();
        }

        [TestMethod]
        public void TeamCompetitorDTOMappingTest()
        {
            var msg = RMF.GetTeamCompetitor();
            var dto = new TeamCompetitorDTO(msg);

            var ci = new TeamCompetitorCI(dto, new CultureInfo("en"), new TestDataRouterManager(new CacheManager()));

            ValidateTeamCompetitor(msg, dto);
            ValidateTeamCompetitor(msg, ci, new CultureInfo("en"));
        }

        [TestMethod]
        public void TournamentDTOMappingTest()
        {
            var msg = RMF.GetTournament();
            var dto = new TournamentDTO(msg);

            ValidateTournament(msg, dto);
        }

        [TestMethod]
        public void TvChannelDTOMappingTest()
        {
            var msg = RMF.GetTvChannel();
            var dto = new TvChannelDTO(msg);

            Assert.AreEqual(msg.name, dto.Name);
            Assert.AreEqual(msg.start_time, dto.StartTime);

            msg = RMF.GetTvChannel(false);
            dto = new TvChannelDTO(msg);

            Assert.AreEqual(msg.name, dto.Name);
            Assert.IsNull(dto.StartTime);
        }

        [TestMethod]
        public void VenueDTOMappingTest()
        {
            new CacheItemMergeTest().VenueMergeTest();
        }

        [TestMethod]
        public void WeatherInfoDTOMappingTest()
        {
            new CacheItemMergeTest().WeatherInfoTest();
        }

        private static void ValidateMapping(mappingsMapping msg, MarketMappingDTO dto)
        {
            var dtoMarketId = dto.MarketSubTypeId == null ? dto.MarketTypeId.ToString() : $"{dto.MarketTypeId}:{dto.MarketSubTypeId}";
            Assert.AreEqual(msg.market_id, dtoMarketId);
            Assert.AreEqual(msg.product_id, dto.ProducerId);
            Assert.AreEqual(msg.sport_id, dto.SportId.ToString());
            Assert.AreEqual(msg.sov_template, dto.SovTemplate);
            Assert.AreEqual(msg.valid_for, dto.ValidFor);

            if (msg.mapping_outcome != null)
            {
                Assert.AreEqual(msg.mapping_outcome.Length, dto.OutcomeMappings.Count());

                for (var i = 0; i < msg.mapping_outcome.Length; i++)
                {
                    ValidateMappingOutcome(msg.mapping_outcome[i], dto.OutcomeMappings.ToArray()[i]);
                }
            }
        }

        private static void ValidateMappingOutcome(mappingsMappingMapping_outcome msg, OutcomeMappingDTO dto)
        {
            Assert.AreEqual(msg.outcome_id, dto.OutcomeId);
            Assert.AreEqual(msg.product_outcome_id, dto.ProducerOutcomeId);
            Assert.AreEqual(msg.product_outcome_name, dto.ProducerOutcomeName);
        }

        private static void ValidateCoverageInfo(coverageInfo msg, CoverageInfoDTO dto)
        {
            Assert.AreEqual(msg.level, dto.Level);
            Assert.AreEqual(msg.live_coverage, dto.IsLive);
            Assert.AreEqual(CoveredFrom.Tv, dto.CoveredFrom);
            Assert.AreEqual(msg.coverage.Length, dto.Includes.Count());
            for (var i = 0; i < msg.coverage.Length; i++)
            {
                Assert.AreEqual(msg.coverage[i].includes, dto.Includes.ToArray()[i]);
            }
        }

        private static void ValidateTournamentGroup(tournamentGroup msg, GroupDTO dto)
        {
            Assert.AreEqual(msg.name, dto.Name);
            Assert.AreEqual(msg.competitor.Length, dto.Competitors.ToList().Count);
            for (var i = 0; i < msg.competitor.Length; i++)
            {
                ValidateTeam(msg.competitor[i], dto.Competitors.ToArray()[i]);
            }
        }

        private static void ValidateOutcome(desc_outcomesOutcome msg, OutcomeDescriptionDTO dto)
        {
            Assert.AreEqual(msg.id, dto.Id);
            Assert.AreEqual(msg.name, dto.Name);
            Assert.AreEqual(msg.description, dto.Description);
        }

        private static void ValidateSpecifier(desc_specifiersSpecifier msg, SpecifierDTO dto)
        {
            Assert.AreEqual(msg.type, dto.Type);
            Assert.AreEqual(msg.name, dto.Name);
        }

        private static void ValidateTeam(team msg, CompetitorDTO dto)
        {
            Assert.AreEqual(msg.id, dto.Id.ToString());
            Assert.AreEqual(msg.name, dto.Name);
            Assert.AreEqual(msg.abbreviation, dto.Abbreviation);
            Assert.AreEqual(msg.country, dto.CountryName);
            Assert.AreEqual(msg.@virtual, dto.IsVirtual);
        }

        private static void ValidateTeamExtended(teamExtended msg, CompetitorDTO dto)
        {
            Assert.AreEqual(msg.id, dto.Id.ToString());
            Assert.AreEqual(msg.name, dto.Name);
            Assert.AreEqual(msg.abbreviation, dto.Abbreviation);
            Assert.AreEqual(msg.country, dto.CountryName);
            Assert.AreEqual(msg.@virtual, dto.IsVirtual);
        }

        private static void ValidateTeamCompetitor(teamCompetitor msg, CompetitorDTO dto)
        {
            Assert.AreEqual(msg.id, dto.Id.ToString());
            Assert.AreEqual(msg.name, dto.Name);
            Assert.AreEqual(msg.abbreviation, dto.Abbreviation);
            Assert.AreEqual(msg.country, dto.CountryName);
            Assert.AreEqual(msg.@virtual, dto.IsVirtual);
            //Assert.AreEqual(msg.@qualifier, dto.); //TODO: qualifier missing in dto
        }

        private static void ValidateTeamCompetitor(teamCompetitor msg, TeamCompetitorCI ci, CultureInfo culture)
        {
            Assert.AreEqual(msg.id, ci.Id.ToString());
            Assert.AreEqual(msg.name, ci.GetName(culture));
            Assert.AreEqual(msg.abbreviation, ci.GetAbbreviation(culture));
            Assert.AreEqual(msg.country, ci.GetCountry(culture));
            Assert.AreEqual(msg.@virtual, ci.IsVirtual);
            Assert.AreEqual(msg.qualifier, ci.Qualifier);
            Assert.AreEqual(msg.divisionSpecified, ci.Division.HasValue);
            Assert.AreEqual(msg.division, ci.Division);
        }

        private static void ValidatePlayerExtended(playerExtended msg, PlayerProfileDTO dto)
        {
            Assert.AreEqual(msg.id, dto.Id.ToString());
            Assert.AreEqual(msg.name, dto.Name);
            Assert.AreEqual(msg.date_of_birth, dto.DateOfBirth?.ToString("yyyy-MM-dd"));
            Assert.AreEqual(msg.height, dto.Height);
            Assert.AreEqual(msg.nationality, dto.Nationality);
            Assert.AreEqual(msg.type, dto.Type);
            Assert.AreEqual(msg.weight, dto.Weight); //TODO: missing jersey size
            if (msg.jersey_numberSpecified)
                Assert.AreEqual(msg.jersey_number, dto.JerseyNumber.Value);
            else
                Assert.IsFalse(dto.JerseyNumber.HasValue);
            Assert.AreEqual(msg.gender, dto.Gender);
        }

        private static void ValidateSeasonCoverageInfo(seasonCoverageInfo msg, SeasonCoverageDTO dto)
        {
            Assert.AreEqual(msg.max_coverage_level, dto.MaxCoverageLevel);
            Assert.AreEqual(msg.max_covered, dto.MaxCovered);
            Assert.AreEqual(msg.min_coverage_level, dto.MinCoverageLevel);
            Assert.AreEqual(msg.played, dto.Played);
            Assert.AreEqual(msg.scheduled, dto.Scheduled);
        }

        private static void ValidateSeasonExtended(seasonExtended msg, SeasonDTO dto)
        {
            Assert.AreEqual(msg.id, dto.Id.ToString());
            Assert.AreEqual(msg.name, dto.Name);
            Assert.AreEqual(msg.start_date, dto.StartDate);
            Assert.AreEqual(msg.end_date, dto.EndDate);
            Assert.AreEqual(msg.year, dto.Year);
        }

        private static void ValidateMatchRound(matchRound msg, RoundDTO dto)
        {
            Assert.AreEqual(msg.name, dto.Name);
            Assert.AreEqual(msg.name, dto.Name);
            Assert.AreEqual(msg.cup_round_match_number, dto.CupRoundMatchNumber);
            Assert.AreEqual(msg.cup_round_matches, dto.CupRoundMatches);
            Assert.AreEqual(msg.number, dto.Number);
            Assert.AreEqual(msg.type, dto.Type);
        }

        private static void ValidateTournament(tournament msg, TournamentDTO dto)
        {
            Assert.AreEqual(msg.id, dto.Id.ToString());
            Assert.AreEqual(msg.name, dto.Name);
            Assert.AreEqual(msg.category.id, dto.Category.Id.ToString());
            Assert.AreEqual(msg.category.name, dto.Category.Name);
            Assert.AreEqual(msg.sport.id, dto.Sport.Id.ToString());
            Assert.AreEqual(msg.sport.name, dto.Sport.Name);
            Assert.AreEqual(msg.scheduled, dto.Scheduled);
            Assert.AreEqual(msg.scheduled_end, dto.ScheduledEnd);
            //Assert.AreEqual(msg.tournament_length, dto.tour); //TODO: tournament_length missing
        }

        private static void ValidateTournamentExtended(tournamentExtended msg, TournamentDTO dto)
        {
            Assert.AreEqual(msg.id, dto.Id.ToString());
            Assert.AreEqual(msg.name, dto.Name);
            Assert.AreEqual(msg.category.id, dto.Category.Id.ToString());
            Assert.AreEqual(msg.category.name, dto.Category.Name);
            Assert.AreEqual(msg.sport.id, dto.Sport.Id.ToString());
            Assert.AreEqual(msg.sport.name, dto.Sport.Name);
            Assert.AreEqual(msg.scheduled, dto.Scheduled);
            Assert.AreEqual(msg.scheduled_end, dto.ScheduledEnd);

            Assert.AreEqual(msg.current_season.id, dto.CurrentSeason.Id.ToString()); // TODO: extended properties are lost
        }
    }
}

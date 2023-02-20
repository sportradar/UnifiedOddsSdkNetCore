/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Entities.REST.CacheItems;
using Xunit;
using Xunit.Abstractions;
using RMF = Sportradar.OddsFeed.SDK.Tests.Common.MessageFactoryRest;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    public class RestMessageMappingTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public RestMessageMappingTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void BookmakerDetailsDTOMappingTest()
        {
            var msg1 = RMF.GetBookmakerDetails(10);
            var msg2 = RMF.GetBookmakerDetails(0, false, false, false);

            var details1 = new BookmakerDetails(new BookmakerDetailsDTO(msg1, TimeSpan.Zero));
            var details2 = new BookmakerDetails(new BookmakerDetailsDTO(msg2, TimeSpan.Zero));
            Assert.NotNull(details1);
            Assert.NotNull(details1.ResponseCode);

            Assert.Equal(msg1.bookmaker_id, details1.BookmakerId);
            Assert.Equal(msg1.message, details1.Message);
            Assert.Equal(msg1.expire_at, details1.ExpireAt);
            Assert.Equal(msg1.response_code.ToString().ToLower(), details1.ResponseCode?.ToString().ToLower());
            Assert.Equal(msg1.virtual_host, details1.VirtualHost);

            Assert.Equal(msg2.bookmaker_id, details2.BookmakerId);
            Assert.NotEqual(0, details2.BookmakerId);
            Assert.Equal(msg2.message, details2.Message);
            Assert.NotEqual(msg2.expire_at, details2.ExpireAt);
            Assert.Equal(DateTime.MinValue, details2.ExpireAt);
            Assert.NotEqual(msg2.response_code.ToString(), details2.ResponseCode.ToString());
            Assert.Null(details2.ResponseCode);
            Assert.Equal(msg2.virtual_host, details2.VirtualHost);
        }

        [Fact]
        public void CategoryDTOMappingTest()
        {
            var tours = RMF.GetTournamentExtendedList(100);
            var dto = new CategoryDTO("sr:category:1", "category name", "en", tours);

            Assert.Equal("sr:category:1", dto.Id.ToString());
            Assert.Equal("category name", dto.Name);
            Assert.Equal(tours.Count, dto.Tournaments.Count());

            for (var i = 0; i < tours.Count; i++)
            {
                ValidateTournamentExtended(tours[i], dto.Tournaments.ToArray()[i]);
            }
        }

        [Fact]
        public void CompetitorDTOMappingTest()
        {
            var msg = RMF.GetTeam();
            ValidateTeam(msg, new CompetitorDTO(msg));
        }

        [Fact]
        public void CompetitorProfileDTOMappingTest()
        {
            var msg = RMF.GetCompetitorProfileEndpoint(0, 10);

            var dto = new CompetitorProfileDTO(msg);
            ValidateTeamExtended(msg.competitor, dto.Competitor); //TODO: missing extended properties
            Assert.Equal(msg.players.Length, dto.Players.Count());

            for (var i = 0; i < msg.players.Length; i++)
            {
                ValidatePlayerExtended(msg.players[i], dto.Players.ToArray()[i]);
            }
        }

        [Fact]
        public void CoverageInfoDTOMappingTest()
        {
            var coverageInfoFeed = RMF.GetCoverageInfo(10);
            var coverageInfo = new CoverageInfo(new CoverageInfoCI(new CoverageInfoDTO(coverageInfoFeed)));

            Assert.NotNull(coverageInfo);
            Assert.Equal(coverageInfoFeed.level, coverageInfo.Level);
            Assert.Equal(coverageInfoFeed.live_coverage, coverageInfo.IsLive);
            Assert.Equal(CoveredFrom.Tv, coverageInfo.CoveredFrom);
            Assert.Equal(coverageInfoFeed.coverage.Length, coverageInfo.Includes.Count());
            Assert.NotEqual(coverageInfo.Includes.ToList()[0], coverageInfo.Includes.ToList()[2]);
        }

        [Fact]
        public void FixtureDTOMappingTest()
        {
            var msg = RMF.GetFixture();
            var dto = new FixtureDTO(msg, null);

            Assert.Equal(msg.id, dto.Id.ToString());
            Assert.Equal(msg.name, dto.Name);
            Assert.Equal(msg.liveodds, dto.LiveOdds);
            Assert.Equal(SdkInfo.ParseDate(msg.next_live_time), dto.NextLiveTime);
            Assert.Equal(msg.start_time, dto.StartTime);
            Assert.Equal(msg.scheduled, dto.Scheduled);
            Assert.Equal(msg.scheduled_end, dto.ScheduledEnd);
            Assert.Equal(!string.IsNullOrEmpty(msg.replaced_by) ? URN.Parse(msg.replaced_by) : null, dto.ReplacedBy);
            Assert.Equal(msg.start_time_tbdSpecified ? (bool?)msg.start_time_tbd : null, dto.StartTimeTbd);
            Assert.Equal(msg.competitors.Length, dto.Competitors.Count());

            for (var i = 0; i < msg.competitors.Length; i++)
            {
                ValidateTeamCompetitor(msg.competitors[i], dto.Competitors.ToList()[i]);
            }
            ValidateCoverageInfo(msg.coverage_info, dto.Coverage);
            Assert.Equal(msg.delayed_info.id, dto.DelayedInfo.Id);

            Assert.Equal(msg.extra_info.Length, dto.ExtraInfo.ToList().Count);
            for (var i = 0; i < msg.extra_info.Length; i++)
            {
                Assert.True(dto.ExtraInfo.ContainsKey(msg.extra_info[i].key));
                dto.ExtraInfo.TryGetValue(msg.extra_info[i].key, out var eiValue);
                Assert.Equal(msg.extra_info[i].value, eiValue);
            }
        }

        [Fact]
        public void GroupDTOMappingTest()
        {
            var msg = RMF.GetTournamentGroup();
            var dto = new GroupDTO(msg);

            ValidateTournamentGroup(msg, dto);
        }

        [Fact]
        public void MarketDescriptionDTOMappingTest()
        {
            var msg = RMF.GetDescMarket();
            var dto = new MarketDescriptionDTO(msg);

            Assert.Equal(msg.id, dto.Id);
            Assert.Equal(msg.name, dto.Name);
            Assert.Equal(msg.description, dto.Description);
            //Assert.Equal(msg.variant, dto.Variant); //TODO: check if variant is missing in dto?
            Assert.Equal(msg.mappings.Length, dto.Mappings.Count());
            Assert.Equal(msg.outcomes.Length, dto.Outcomes.Count());
            Assert.Equal(msg.specifiers.Length, dto.Specifiers.Count());

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

        [Fact]
        public void MarketMappingDTOMappingTest()
        {
            var msg = RMF.GetMappingsMapping();
            var dto = new MarketMappingDTO(msg);

            ValidateMapping(msg, dto);
        }

        [Fact]
        public void OutcomeDescriptionDTOMappingTest()
        {
            var msg = RMF.GetDescOutcomesOutcome();
            var dto = new OutcomeDescriptionDTO(msg);

            ValidateOutcome(msg, dto);
        }

        [Fact]
        public void PlayerDTOTest()
        {
            var sportEntityDTO = new SportEntityDTO("sr:player:1", "Sport Entity Name");
            var playerCI = new SportEntityCI(sportEntityDTO);

            Assert.NotNull(playerCI);
            Assert.Equal(sportEntityDTO.Id, playerCI.Id);
        }

        [Fact]
        public void PlayerProfileDTOTest()
        {
            var msg = RMF.GetPlayerExtended();
            var dto = new PlayerProfileDTO(msg, null);

            ValidatePlayerExtended(msg, dto);
        }

        [Fact]
        public void ProductInfoTest()
        {
            new CacheItemMergeTests(_outputHelper).ProductInfoTest();
        }

        [Fact]
        public void ProductInfoLinkTest()
        {
            new CacheItemMergeTests(_outputHelper).ProductInfoLinkTest();
        }

        [Fact]
        public void RefereeTest()
        {
            new CacheItemMergeTests(_outputHelper).RefereeTest();
        }

        [Fact]
        public void RoundTest()
        {
            new CacheItemMergeTests(_outputHelper).RoundMergeTest();
        }

        [Fact]
        public void SeasonCoverageInfoDTOTest()
        {
            var msg = RMF.GetSeasonCoverageInfo();
            var dto = new SeasonCoverageDTO(msg);

            ValidateSeasonCoverageInfo(msg, dto);
        }

        [Fact]
        public void SeasonTest()
        {
            new CacheItemMergeTests(_outputHelper).SeasonTest();
        }

        [Fact]
        public void SpecifierDTOMappingTest()
        {
            var msg = RMF.GetDescSpecifiersSpecifier();
            var dto = new SpecifierDTO(msg);

            ValidateSpecifier(msg, dto);
        }

        [Fact]
        public void SportDTOMappingTest()
        {
            var tours = RMF.GetTournamentExtendedList(100);
            Assert.Equal(100, tours.Count);

            var dto = new SportDTO("sr:sport:1", "name", tours);

            Assert.Equal(tours.Count, dto.Categories.Select(c => c.Tournaments).Count());

            foreach (var t in tours)
            {
                var cat = dto.Categories.ToList().Find(c => c.Id.ToString() == t.category.id);
                Assert.NotNull(cat);
                Assert.Equal(t.category.id, cat.Id.ToString());
                Assert.Equal(t.category.name, cat.Name);

                ValidateTournamentExtended(t, cat.Tournaments.First(c => c.Id.ToString() == t.id));
            }
        }

        [Fact]
        public void StreamingChannelTest()
        {
            new CacheItemMergeTests(_outputHelper).StreamingChannelTest();
        }

        [Fact]
        public void TeamCompetitorDTOMappingTest()
        {
            var msg = RMF.GetTeamCompetitor();
            var dto = new TeamCompetitorDTO(msg);

            var ci = new TeamCompetitorCI(dto, new CultureInfo("en"), new TestDataRouterManager(new CacheManager(), _outputHelper));

            ValidateTeamCompetitor(msg, dto);
            ValidateTeamCompetitor(msg, ci, new CultureInfo("en"));
        }

        [Fact]
        public void TournamentDTOMappingTest()
        {
            var msg = RMF.GetTournament();
            var dto = new TournamentDTO(msg);

            ValidateTournament(msg, dto);
        }

        [Fact]
        public void TvChannelDTOMappingTest()
        {
            var msg = RMF.GetTvChannel();
            var dto = new TvChannelDTO(msg);

            Assert.Equal(msg.name, dto.Name);
            Assert.Equal(msg.start_time, dto.StartTime);

            msg = RMF.GetTvChannel(false);
            dto = new TvChannelDTO(msg);

            Assert.Equal(msg.name, dto.Name);
            Assert.Null(dto.StartTime);
        }

        [Fact]
        public void VenueDTOMappingTest()
        {
            new CacheItemMergeTests(_outputHelper).VenueMergeTest();
        }

        [Fact]
        public void WeatherInfoDTOMappingTest()
        {
            new CacheItemMergeTests(_outputHelper).WeatherInfoTest();
        }

        private static void ValidateMapping(mappingsMapping msg, MarketMappingDTO dto)
        {
            var dtoMarketId = dto.MarketSubTypeId == null ? dto.MarketTypeId.ToString() : $"{dto.MarketTypeId}:{dto.MarketSubTypeId}";
            Assert.Equal(msg.market_id, dtoMarketId);
            Assert.Equal(msg.sport_id, dto.SportId.ToString());
            Assert.Equal(msg.sov_template, dto.SovTemplate);
            Assert.Equal(msg.valid_for, dto.ValidFor);

            if (msg.mapping_outcome != null)
            {
                Assert.Equal(msg.mapping_outcome.Length, dto.OutcomeMappings.Count());

                for (var i = 0; i < msg.mapping_outcome.Length; i++)
                {
                    ValidateMappingOutcome(msg.mapping_outcome[i], dto.OutcomeMappings.ToArray()[i]);
                }
            }
        }

        private static void ValidateMappingOutcome(mappingsMappingMapping_outcome msg, OutcomeMappingDTO dto)
        {
            Assert.Equal(msg.outcome_id, dto.OutcomeId);
            Assert.Equal(msg.product_outcome_id, dto.ProducerOutcomeId);
            Assert.Equal(msg.product_outcome_name, dto.ProducerOutcomeName);
        }

        private static void ValidateCoverageInfo(coverageInfo msg, CoverageInfoDTO dto)
        {
            Assert.Equal(msg.level, dto.Level);
            Assert.Equal(msg.live_coverage, dto.IsLive);
            Assert.Equal(CoveredFrom.Tv, dto.CoveredFrom);
            Assert.Equal(msg.coverage.Length, dto.Includes.Count());
            for (var i = 0; i < msg.coverage.Length; i++)
            {
                Assert.Equal(msg.coverage[i].includes, dto.Includes.ToArray()[i]);
            }
        }

        private static void ValidateTournamentGroup(tournamentGroup msg, GroupDTO dto)
        {
            Assert.Equal(msg.name, dto.Name);
            Assert.Equal(msg.competitor.Length, dto.Competitors.ToList().Count);
            for (var i = 0; i < msg.competitor.Length; i++)
            {
                ValidateTeam(msg.competitor[i], dto.Competitors.ToArray()[i]);
            }
        }

        private static void ValidateOutcome(desc_outcomesOutcome msg, OutcomeDescriptionDTO dto)
        {
            Assert.Equal(msg.id, dto.Id);
            Assert.Equal(msg.name, dto.Name);
            Assert.Equal(msg.description, dto.Description);
        }

        private static void ValidateSpecifier(desc_specifiersSpecifier msg, SpecifierDTO dto)
        {
            Assert.Equal(msg.type, dto.Type);
            Assert.Equal(msg.name, dto.Name);
        }

        private static void ValidateTeam(team msg, CompetitorDTO dto)
        {
            Assert.Equal(msg.id, dto.Id.ToString());
            Assert.Equal(msg.name, dto.Name);
            Assert.Equal(msg.abbreviation, dto.Abbreviation);
            Assert.Equal(msg.country, dto.CountryName);
            Assert.Equal(msg.state, dto.State);
            Assert.Equal(msg.@virtual, dto.IsVirtual);
        }

        private static void ValidateTeamExtended(teamExtended msg, CompetitorDTO dto)
        {
            Assert.Equal(msg.id, dto.Id.ToString());
            Assert.Equal(msg.name, dto.Name);
            Assert.Equal(msg.abbreviation, dto.Abbreviation);
            Assert.Equal(msg.country, dto.CountryName);
            Assert.Equal(msg.state, dto.State);
            Assert.Equal(msg.@virtual, dto.IsVirtual);
        }

        private static void ValidateTeamCompetitor(teamCompetitor msg, CompetitorDTO dto)
        {
            Assert.Equal(msg.id, dto.Id.ToString());
            Assert.Equal(msg.name, dto.Name);
            Assert.Equal(msg.abbreviation, dto.Abbreviation);
            Assert.Equal(msg.country, dto.CountryName);
            Assert.Equal(msg.state, dto.State);
            Assert.Equal(msg.@virtual, dto.IsVirtual);
            //Assert.Equal(msg.@qualifier, dto.); //TODO: qualifier missing in dto
        }

        private static void ValidateTeamCompetitor(teamCompetitor msg, TeamCompetitorCI ci, CultureInfo culture)
        {
            Assert.Equal(msg.id, ci.Id.ToString());
            Assert.Equal(msg.name, ci.GetName(culture));
            Assert.Equal(msg.abbreviation, ci.GetAbbreviation(culture));
            Assert.Equal(msg.country, ci.GetCountry(culture));
            Assert.Equal(msg.@virtual, ci.IsVirtual);
            Assert.Equal(msg.qualifier, ci.Qualifier);
            Assert.Equal(msg.divisionSpecified, ci.Division.HasValue);
            Assert.Equal(msg.division, ci.Division);
            Assert.Equal(msg.state, ci.State);
        }

        private static void ValidatePlayerExtended(playerExtended msg, PlayerProfileDTO dto)
        {
            Assert.Equal(msg.id, dto.Id.ToString());
            Assert.Equal(msg.name, dto.Name);
            Assert.Equal(msg.date_of_birth, dto.DateOfBirth?.ToString("yyyy-MM-dd"));
            Assert.Equal(msg.height, dto.Height);
            Assert.Equal(msg.nationality, dto.Nationality);
            Assert.Equal(msg.type, dto.Type);
            Assert.Equal(msg.weight, dto.Weight); //TODO: missing jersey size
            if (msg.jersey_numberSpecified)
            {
                Assert.NotNull(dto.JerseyNumber);
                Assert.Equal(msg.jersey_number, dto.JerseyNumber.Value);
            }
            else
            {
                Assert.False(dto.JerseyNumber.HasValue);
            }
            Assert.Equal(msg.gender, dto.Gender);
        }

        private static void ValidateSeasonCoverageInfo(seasonCoverageInfo msg, SeasonCoverageDTO dto)
        {
            Assert.Equal(msg.max_coverage_level, dto.MaxCoverageLevel);
            Assert.Equal(msg.max_covered, dto.MaxCovered);
            Assert.Equal(msg.min_coverage_level, dto.MinCoverageLevel);
            Assert.Equal(msg.played, dto.Played);
            Assert.Equal(msg.scheduled, dto.Scheduled);
        }

        private static void ValidateTournament(tournament msg, TournamentDTO dto)
        {
            Assert.Equal(msg.id, dto.Id.ToString());
            Assert.Equal(msg.name, dto.Name);
            Assert.Equal(msg.category.id, dto.Category.Id.ToString());
            Assert.Equal(msg.category.name, dto.Category.Name);
            Assert.Equal(msg.sport.id, dto.Sport.Id.ToString());
            Assert.Equal(msg.sport.name, dto.Sport.Name);
            Assert.Equal(msg.scheduled, dto.Scheduled);
            Assert.Equal(msg.scheduled_end, dto.ScheduledEnd);
            //Assert.Equal(msg.tournament_length, dto.tour); //TODO: tournament_length missing
        }

        private static void ValidateTournamentExtended(tournamentExtended msg, TournamentDTO dto)
        {
            Assert.Equal(msg.id, dto.Id.ToString());
            Assert.Equal(msg.name, dto.Name);
            Assert.Equal(msg.category.id, dto.Category.Id.ToString());
            Assert.Equal(msg.category.name, dto.Category.Name);
            Assert.Equal(msg.sport.id, dto.Sport.Id.ToString());
            Assert.Equal(msg.sport.name, dto.Sport.Name);
            Assert.Equal(msg.scheduled, dto.Scheduled);
            Assert.Equal(msg.scheduled_end, dto.ScheduledEnd);

            Assert.Equal(msg.current_season.id, dto.CurrentSeason.Id.ToString()); // TODO: extended properties are lost
        }
    }
}

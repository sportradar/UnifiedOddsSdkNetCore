/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    public class ProfileCacheTests
    {
        private readonly TestSportEntityFactoryBuilder _sportEntityFactoryBuilder;

        public ProfileCacheTests(ITestOutputHelper outputHelper)
        {
            _sportEntityFactoryBuilder = new TestSportEntityFactoryBuilder(outputHelper, ScheduleData.Cultures3.ToList());
        }

        private static URN CreatePlayerUrn(int playerId)
        {
            return new URN("sr", "player", playerId);
        }

        private static URN CreateCompetitorUrn(int competitorId)
        {
            return new URN("sr", "competitor", competitorId);
        }

        [Fact]
        public void PlayerProfileGetsCached()
        {
            Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Empty(_sportEntityFactoryBuilder.ProfileMemoryCache);

            var player = _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            ValidatePlayer(player, TestData.Culture);
            Assert.Single(_sportEntityFactoryBuilder.ProfileMemoryCache);

            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

            //if we call again, should not fetch again
            player = _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            ValidatePlayer(player, TestData.Culture);
            Assert.Single(_sportEntityFactoryBuilder.ProfileMemoryCache);

            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [Fact]
        public void PlayerProfileFetchesOnlyMissingCultures()
        {
            Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Empty(_sportEntityFactoryBuilder.ProfileMemoryCache);

            var player = _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), new[] { TestData.Culture }, true).GetAwaiter().GetResult();
            ValidatePlayer(player, TestData.Culture);
            Assert.Single(_sportEntityFactoryBuilder.ProfileMemoryCache);

            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

            //if we call again, should fetch only missing cultures
            player = _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            ValidatePlayer(player, TestData.Culture);
            Assert.Single(_sportEntityFactoryBuilder.ProfileMemoryCache);

            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [Fact]
        public void NumberOfPlayerProfileEndpointCallsIsCorrect()
        {
            var player1 = _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.NotNull(player1);
            ValidatePlayer(player1, TestData.Culture);
            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

            var player2 = _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(CreatePlayerUrn(2), TestData.Cultures, true).GetAwaiter().GetResult();
            ValidatePlayer(player2, TestData.Culture);

            Assert.NotEqual(player1.Id, player2.Id);
            Assert.Equal(TestData.Cultures.Count * 2, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(TestData.Cultures.Count * 2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [Fact]
        public void PreloadedFromCompetitorPlayerProfileEndpointCallsIsCorrect()
        {
            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.NotNull(competitorNames);
            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            var player = _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(CreatePlayerUrn(30401), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.NotNull(player);
            Assert.Equal("Smithies, Alex", player.GetName(TestData.Culture));
            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [Fact]
        public void PartiallyPreloadedFromCompetitorPlayerProfileEndpointCallsCompetitorProfileInsteadOfPlayers()
        {
            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), new[] { TestData.Culture }, true).GetAwaiter().GetResult();
            Assert.NotNull(competitorNames);
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            var player = _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(CreatePlayerUrn(30401), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.NotNull(player);
            Assert.Equal("Smithies, Alex", player.GetName(TestData.Culture));
            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [Fact]
        public void CompetitorProfileGetsCached()
        {
            Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Empty(_sportEntityFactoryBuilder.ProfileMemoryCache);

            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();

            Assert.NotNull(competitorNames);
            Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            //if we call again, should not fetch again
            competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.NotNull(competitorNames);
            Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [Fact]
        public void CompetitorProfileFetchesOnlyMissingCultures()
        {
            Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Empty(_sportEntityFactoryBuilder.ProfileMemoryCache);

            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), new[] { TestData.Culture }, true).GetAwaiter().GetResult();

            Assert.NotNull(competitorNames);
            Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            //if we call again, should not fetch again
            competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.NotNull(competitorNames);
            Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [Fact]
        public void NumberOfCompetitorProfileProviderCallsMatchIsCorrect()
        {
            var competitorNames1 = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.NotNull(competitorNames1);

            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            var competitorNames2 = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(2), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.NotNull(competitorNames2);

            Assert.NotEqual(competitorNames1.Id, competitorNames2.Id);
            Assert.Equal(TestData.Cultures.Count * 2, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(TestData.Cultures.Count * 2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [Fact]
        public void PreloadedFromMatchCompetitorProfileProviderCallsIsCorrect()
        {
            var matchId = URN.Parse("sr:match:1");
            var cultures = TestData.Cultures3.ToList();
            _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(matchId, cultures[0], null).GetAwaiter().GetResult();
            _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(matchId, cultures[1], null).GetAwaiter().GetResult();
            _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(matchId, cultures[2], null).GetAwaiter().GetResult();
            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            var competitorNames1 = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.NotNull(competitorNames1);
            Assert.Equal(TestData.Cultures.Count * 2, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [Fact]
        public void PartiallyPreloadedFromMatchCompetitorProfileProviderCallsIsCorrect()
        {
            var matchId = URN.Parse("sr:match:1");
            var cultures = TestData.Cultures3.ToList();
            _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(matchId, cultures[0], null).GetAwaiter().GetResult();
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            var competitorNames1 = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.NotNull(competitorNames1);
            Assert.Equal(TestData.Cultures.Count + 1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Equal(TestData.Cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [Fact]
        public void MissingPlayerNameForAllCulturesWhenSetToFetchIfMissingFetchesMissingPlayerProfiles()
        {
            var cultures = TestData.Cultures3.ToList();
            Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Empty(_sportEntityFactoryBuilder.ProfileMemoryCache);

            var playerNames = _sportEntityFactoryBuilder.ProfileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), cultures, true).GetAwaiter().GetResult();
            Assert.NotNull(playerNames);
            Assert.NotEqual(string.Empty, playerNames[cultures[0]]);
            Assert.NotEqual(string.Empty, playerNames[cultures[1]]);
            Assert.NotEqual(string.Empty, playerNames[cultures[2]]);
            Assert.Single(_sportEntityFactoryBuilder.ProfileMemoryCache);
            Assert.Equal(cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(cultures.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [Fact]
        public void MissingPlayerNameForOneCultureWhenSetToFetchIfMissingFetchesOneMissingPlayerProfile()
        {
            Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Empty(_sportEntityFactoryBuilder.ProfileMemoryCache);

            var playerNames = _sportEntityFactoryBuilder.ProfileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), TestData.Cultures1, true).GetAwaiter().GetResult();
            Assert.NotNull(playerNames);
            Assert.NotEqual(string.Empty, playerNames[TestData.Cultures1.First()]);
            Assert.Single(_sportEntityFactoryBuilder.ProfileMemoryCache);
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [Fact]
        public void MissingPlayerNameForAllCulturesWhenSetNotToFetchIfMissingDoesNotFetchesMissingPlayerProfiles()
        {
            var cultures = TestData.Cultures3.ToList();
            Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Empty(_sportEntityFactoryBuilder.ProfileMemoryCache);

            var playerNames = _sportEntityFactoryBuilder.ProfileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), cultures, false).GetAwaiter().GetResult();
            Assert.NotNull(playerNames);
            Assert.True(playerNames.All(a => string.IsNullOrEmpty(a.Value)));
            Assert.Empty(_sportEntityFactoryBuilder.ProfileMemoryCache);
            Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [Fact]
        public void MissingPlayerNameForAllCultureWhenSetNotToFetchIfMissingDoesNotFetchesMissingPlayerProfile()
        {
            _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), TestData.Cultures1, true).GetAwaiter().GetResult();
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.Single(_sportEntityFactoryBuilder.ProfileMemoryCache);

            var playerNames = _sportEntityFactoryBuilder.ProfileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), TestData.Cultures, false).GetAwaiter().GetResult();
            Assert.NotNull(playerNames);
            Assert.NotEqual(string.Empty, playerNames[TestData.Cultures3.First()]);
            Assert.Equal(string.Empty, playerNames[TestData.Cultures3.Skip(1).First()]);
            Assert.Equal(string.Empty, playerNames[TestData.Cultures3.Skip(2).First()]);
            Assert.Single(_sportEntityFactoryBuilder.ProfileMemoryCache);
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [Fact]
        public void PopulatedFromCompetitorMissingPlayerNameForAllCultureWhenSetToFetchIfMissingFetchesMissingCompetitorProfile()
        {
            _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures1, true).GetAwaiter().GetResult();
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var playerNames = _sportEntityFactoryBuilder.ProfileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.NotNull(playerNames);
            Assert.NotEqual(string.Empty, playerNames[TestData.Cultures3.First()]);
            Assert.NotEqual(string.Empty, playerNames[TestData.Cultures3.Skip(1).First()]);
            Assert.NotEqual(string.Empty, playerNames[TestData.Cultures3.Skip(2).First()]);
            Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [Fact]
        public void PopulatedFromCompetitorMissingPlayerNameForAllCultureWhenSetNotToFetchIfMissingDoesNotFetchesMissingProfile()
        {
            _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures1, true).GetAwaiter().GetResult();
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var playerNames = _sportEntityFactoryBuilder.ProfileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), TestData.Cultures, false).GetAwaiter().GetResult();
            Assert.NotNull(playerNames);
            Assert.NotEqual(string.Empty, playerNames[TestData.Cultures3.First()]);
            Assert.Equal(string.Empty, playerNames[TestData.Cultures3.Skip(1).First()]);
            Assert.Equal(string.Empty, playerNames[TestData.Cultures3.Skip(2).First()]);
            Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [Fact]
        public void MissingPlayerNameForAllCultureWhenSetNotToFetchIfMissingDoesNotOverridePlayerProfile()
        {
            _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures1, true).GetAwaiter().GetResult();
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var playerNames = _sportEntityFactoryBuilder.ProfileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), TestData.Cultures, false).GetAwaiter().GetResult();
            Assert.NotNull(playerNames);
            Assert.NotEqual(string.Empty, playerNames[TestData.Cultures3.First()]);
            Assert.Equal(string.Empty, playerNames[TestData.Cultures3.Skip(1).First()]);
            Assert.Equal(string.Empty, playerNames[TestData.Cultures3.Skip(2).First()]);
            Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

            playerNames = _sportEntityFactoryBuilder.ProfileCache.GetPlayerNamesAsync(CreatePlayerUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.NotNull(playerNames);
            Assert.NotEqual(string.Empty, playerNames[TestData.Cultures3.First()]);
            Assert.NotEqual(string.Empty, playerNames[TestData.Cultures3.Skip(1).First()]);
            Assert.NotEqual(string.Empty, playerNames[TestData.Cultures3.Skip(2).First()]);
            Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }

        [Fact]
        public void MissingCompetitorNameForAllCulturesWhenSetToFetchIfMissingFetchesMissingCompetitorProfiles()
        {
            Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Empty(_sportEntityFactoryBuilder.ProfileMemoryCache);

            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), TestData.Cultures3, true).GetAwaiter().GetResult();
            Assert.NotNull(competitorNames);
            Assert.NotEqual(string.Empty, competitorNames[TestData.Cultures3.First()]);
            Assert.NotEqual(string.Empty, competitorNames[TestData.Cultures3.Skip(1).First()]);
            Assert.NotEqual(string.Empty, competitorNames[TestData.Cultures3.Skip(2).First()]);
            Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.Equal(TestData.Cultures3.Count, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(TestData.Cultures3.Count, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [Fact]
        public void MissingCompetitorNameForOneCultureWhenSetToFetchIfMissingFetchesOneMissingCompetitorProfile()
        {
            Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Empty(_sportEntityFactoryBuilder.ProfileMemoryCache);

            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), TestData.Cultures1, true).GetAwaiter().GetResult();
            Assert.NotNull(competitorNames);
            Assert.NotEqual(string.Empty, competitorNames[TestData.Cultures1.First()]);
            Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [Fact]
        public void MissingCompetitorNameForAllCulturesWhenSetNotToFetchIfMissingDoesNotFetchesMissingCompetitorProfiles()
        {
            Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Empty(_sportEntityFactoryBuilder.ProfileMemoryCache);

            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), TestData.Cultures3, false).GetAwaiter().GetResult();
            Assert.NotNull(competitorNames);
            Assert.True(competitorNames.All(a => string.IsNullOrEmpty(a.Value)));
            Assert.Empty(_sportEntityFactoryBuilder.ProfileMemoryCache);
            Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [Fact]
        public void MissingCompetitorNameForAllCultureWhenSetNotToFetchIfMissingDoesNotFetchesMissingCompetitorProfile()
        {
            _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures1, true).GetAwaiter().GetResult();
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), TestData.Cultures, false).GetAwaiter().GetResult();
            Assert.NotNull(competitorNames);
            Assert.NotEqual(string.Empty, competitorNames[TestData.Cultures3.First()]);
            Assert.Equal(string.Empty, competitorNames[TestData.Cultures3.Skip(1).First()]);
            Assert.Equal(string.Empty, competitorNames[TestData.Cultures3.Skip(2).First()]);
            Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [Fact]
        public void PopulatedFromMatchMissingCompetitorNameForAllCultureWhenSetToFetchIfMissingFetchesMissingSummaryEndpoint()
        {
            _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(URN.Parse("sr:match:1"), TestData.Culture, null).GetAwaiter().GetResult();
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Equal(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.NotNull(competitorNames);
            Assert.NotEqual(string.Empty, competitorNames[TestData.Cultures.First()]);
            Assert.NotEqual(string.Empty, competitorNames[TestData.Cultures.Skip(1).First()]);
            Assert.NotEqual(string.Empty, competitorNames[TestData.Cultures.Skip(2).First()]);
            Assert.Equal(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        }

        [Fact]
        public void PopulatedFromMatchMissingCompetitorNameForAllCultureWhenSetNotToFetchIfMissingDoesNotFetchesMissingCompetitorProfile()
        {
            _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(URN.Parse("sr:match:1"), TestData.Culture, null).GetAwaiter().GetResult();
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Equal(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), TestData.Cultures, false).GetAwaiter().GetResult();
            Assert.NotNull(competitorNames);
            Assert.NotEqual(string.Empty, competitorNames[TestData.Cultures.First()]);
            Assert.Equal(string.Empty, competitorNames[TestData.Cultures.Skip(1).First()]);
            Assert.Equal(string.Empty, competitorNames[TestData.Cultures.Skip(2).First()]);
            Assert.Equal(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        }

        [Fact]
        public void MissingCompetitorNameForAllCultureWhenSetNotToFetchIfMissingDoesNotOverrideProfile()
        {
            _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures1, true).GetAwaiter().GetResult();
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());

            var competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), TestData.Cultures, false).GetAwaiter().GetResult();
            Assert.NotNull(competitorNames);
            Assert.NotEqual(string.Empty, competitorNames[TestData.Cultures.First()]);
            Assert.Equal(string.Empty, competitorNames[TestData.Cultures.Skip(1).First()]);
            Assert.Equal(string.Empty, competitorNames[TestData.Cultures.Skip(2).First()]);
            Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            competitorNames = _sportEntityFactoryBuilder.ProfileCache.GetCompetitorNamesAsync(CreateCompetitorUrn(1), TestData.Cultures, true).GetAwaiter().GetResult();
            Assert.NotNull(competitorNames);
            Assert.NotEqual(string.Empty, competitorNames[TestData.Cultures.First()]);
            Assert.NotEqual(string.Empty, competitorNames[TestData.Cultures.Skip(1).First()]);
            Assert.NotEqual(string.Empty, competitorNames[TestData.Cultures.Skip(2).First()]);
            Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
            Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
            Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        private static void ValidatePlayer(PlayerProfileCI player, CultureInfo culture)
        {
            Assert.NotNull(player);
            if (player.Id.Id == 1)
            {
                Assert.Equal("van Persie, Robin", player.GetName(culture));
                Assert.Equal("Netherlands", player.GetNationality(culture));
                Assert.Null(player.Gender);
                Assert.Equal("forward", player.Type);
                Assert.Equal(183, player.Height);
                Assert.Equal(71, player.Weight);
                Assert.Equal("NLD", player.CountryCode);
                Assert.NotNull(player.DateOfBirth);
                Assert.Equal("1983-08-06", player.DateOfBirth.Value.ToString("yyyy-MM-dd"));
            }
            else if (player.Id.Id == 2)
            {
                Assert.Equal("Cole, Ashley", player.GetName(culture));
                Assert.Equal("England", player.GetNationality(culture));
                Assert.Null(player.Gender);
                Assert.Equal("defender", player.Type);
                Assert.Equal(176, player.Height);
                Assert.Equal(67, player.Weight);
                Assert.Equal("ENG", player.CountryCode);
                Assert.NotNull(player.DateOfBirth);
                Assert.Equal("1980-12-20", player.DateOfBirth.Value.ToString("yyyy-MM-dd"));
            }
        }
    }
}

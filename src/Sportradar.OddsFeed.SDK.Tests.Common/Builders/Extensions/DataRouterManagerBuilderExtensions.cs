// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Moq;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.Lottery;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;

internal static class DataRouterManagerBuilderExtensions
{
    public static DataRouterManagerBuilder WithMockedCacheManager(this DataRouterManagerBuilder builder)
    {
        var mockCacheManager = new Mock<ICacheManager>();
        return builder.WithCacheManager(mockCacheManager.Object);
    }

    public static DataRouterManagerBuilder WithMockedProducerManager(this DataRouterManagerBuilder builder)
    {
        var mockProducerManager = new Mock<IProducerManager>();
        return builder.WithProducerManager(mockProducerManager.Object);
    }

    public static DataRouterManagerBuilder WithMockedConfiguration(this DataRouterManagerBuilder builder)
    {
        var mockConfiguration = new Mock<IUofConfiguration>();
        return builder.WithConfiguration(mockConfiguration.Object);
    }

    public static DataRouterManagerBuilder WithMockedCalculateProbabilityProvider(this DataRouterManagerBuilder builder)
    {
        var mockCalculateProbabilityProvider = new Mock<ICalculateProbabilityProvider>();
        return builder.WithCalculateProbabilityProvider(mockCalculateProbabilityProvider.Object);
    }

    public static DataRouterManagerBuilder WithMockedCalculateProbabilityFilteredProvider(this DataRouterManagerBuilder builder)
    {
        var mockCalculateProbabilityFilteredProvider = new Mock<ICalculateProbabilityFilteredProvider>();
        return builder.WithCalculateProbabilityFilteredProvider(mockCalculateProbabilityFilteredProvider.Object);
    }

    public static DataRouterManagerBuilder WithAllMockedDataProviders(this DataRouterManagerBuilder builder)
    {
        var mockSportEventSummaryProvider = new Mock<IDataProvider<SportEventSummaryDto>>();
        var mockSportEventFixtureProvider = new Mock<IDataProvider<FixtureDto>>();
        var mockSportEventFixtureChangeFixtureProvider = new Mock<IDataProvider<FixtureDto>>();
        var mockAllTournamentsForAllSportsProvider = new Mock<IDataProvider<EntityList<SportDto>>>();
        var mockAllSportsProvider = new Mock<IDataProvider<EntityList<SportDto>>>();
        var mockSportEventsForDateProvider = new Mock<IDataProvider<EntityList<SportEventSummaryDto>>>();
        var mockSportEventsForTournamentProvider = new Mock<IDataProvider<EntityList<SportEventSummaryDto>>>();
        var mockPlayerProfileProvider = new Mock<IDataProvider<PlayerProfileDto>>();
        var mockCompetitorProvider = new Mock<IDataProvider<CompetitorProfileDto>>();
        var mockSimpleTeamProvider = new Mock<IDataProvider<SimpleTeamProfileDto>>();
        var mockTournamentSeasonsProvider = new Mock<IDataProvider<TournamentSeasonsDto>>();
        var mockOngoingSportEventProvider = new Mock<IDataProvider<MatchTimelineDto>>();
        var mockSportCategoriesProvider = new Mock<IDataProvider<SportCategoriesDto>>();
        var mockInvariantMarketDescriptionsProvider = new Mock<IDataProvider<EntityList<MarketDescriptionDto>>>();
        var mockVariantMarketDescriptionProvider = new Mock<IDataProvider<MarketDescriptionDto>>();
        var mockVariantDescriptionsProvider = new Mock<IDataProvider<EntityList<VariantDescriptionDto>>>();
        var mockDrawSummaryProvider = new Mock<IDataProvider<DrawDto>>();
        var mockDrawFixtureProvider = new Mock<IDataProvider<DrawDto>>();
        var mockLotteryScheduleProvider = new Mock<IDataProvider<LotteryDto>>();
        var mockLotteryListProvider = new Mock<IDataProvider<EntityList<LotteryDto>>>();
        var mockAvailableSelectionsProvider = new Mock<IDataProvider<AvailableSelectionsDto>>();
        var mockFixtureChangesProvider = new Mock<IDataProvider<EntityList<FixtureChangeDto>>>();
        var mockResultChangesProvider = new Mock<IDataProvider<EntityList<ResultChangeDto>>>();
        var mockListSportEventProvider = new Mock<IDataProvider<EntityList<SportEventSummaryDto>>>();
        var mockAvailableSportTournamentsProvider = new Mock<IDataProvider<EntityList<TournamentInfoDto>>>();
        var mockSportEventFixtureForTournamentProvider = new Mock<IDataProvider<TournamentInfoDto>>();
        var mockSportEventFixtureChangeFixtureForTournamentProvider = new Mock<IDataProvider<TournamentInfoDto>>();
        var mockStagePeriodSummaryProvider = new Mock<IDataProvider<PeriodSummaryDto>>();
        var mockSportEventsForRaceTournamentProvider = new Mock<IDataProvider<EntityList<SportEventSummaryDto>>>();

        return builder
              .WithSportEventSummaryProvider(mockSportEventSummaryProvider.Object)
              .WithSportEventFixtureProvider(mockSportEventFixtureProvider.Object)
              .WithSportEventFixtureChangeFixtureProvider(mockSportEventFixtureChangeFixtureProvider.Object)
              .WithAllTournamentsForAllSportsProvider(mockAllTournamentsForAllSportsProvider.Object)
              .WithAllSportsProvider(mockAllSportsProvider.Object)
              .WithSportEventsForDateProvider(mockSportEventsForDateProvider.Object)
              .WithSportEventsForTournamentProvider(mockSportEventsForTournamentProvider.Object)
              .WithPlayerProfileProvider(mockPlayerProfileProvider.Object)
              .WithCompetitorProvider(mockCompetitorProvider.Object)
              .WithSimpleTeamProvider(mockSimpleTeamProvider.Object)
              .WithTournamentSeasonsProvider(mockTournamentSeasonsProvider.Object)
              .WithOngoingSportEventProvider(mockOngoingSportEventProvider.Object)
              .WithSportCategoriesProvider(mockSportCategoriesProvider.Object)
              .WithInvariantMarketDescriptionsProvider(mockInvariantMarketDescriptionsProvider.Object)
              .WithVariantMarketDescriptionProvider(mockVariantMarketDescriptionProvider.Object)
              .WithVariantDescriptionsProvider(mockVariantDescriptionsProvider.Object)
              .WithDrawSummaryProvider(mockDrawSummaryProvider.Object)
              .WithDrawFixtureProvider(mockDrawFixtureProvider.Object)
              .WithLotteryScheduleProvider(mockLotteryScheduleProvider.Object)
              .WithLotteryListProvider(mockLotteryListProvider.Object)
              .WithAvailableSelectionsProvider(mockAvailableSelectionsProvider.Object)
              .WithFixtureChangesProvider(mockFixtureChangesProvider.Object)
              .WithResultChangesProvider(mockResultChangesProvider.Object)
              .WithListSportEventProvider(mockListSportEventProvider.Object)
              .WithAvailableSportTournamentsProvider(mockAvailableSportTournamentsProvider.Object)
              .WithSportEventFixtureForTournamentProvider(mockSportEventFixtureForTournamentProvider.Object)
              .WithSportEventFixtureChangeFixtureForTournamentProvider(mockSportEventFixtureChangeFixtureForTournamentProvider.Object)
              .WithStagePeriodSummaryProvider(mockStagePeriodSummaryProvider.Object)
              .WithSportEventsForRaceTournamentProvider(mockSportEventsForRaceTournamentProvider.Object);
    }

    public static DataRouterManagerBuilder AddMockedDependencies(this DataRouterManagerBuilder builder)
    {
        return builder
              .WithMockedCacheManager()
              .WithMockedProducerManager()
              .WithMockedConfiguration()
              .WithAllMockedDataProviders()
              .WithMockedCalculateProbabilityProvider()
              .WithMockedCalculateProbabilityFilteredProvider();
    }
}

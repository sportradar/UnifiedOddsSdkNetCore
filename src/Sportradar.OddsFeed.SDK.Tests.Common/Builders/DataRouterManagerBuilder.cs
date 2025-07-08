// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.Lottery;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders;

internal class DataRouterManagerBuilder
{
    private IDataProvider<EntityList<SportDto>> _allSportsProvider;
    private IDataProvider<EntityList<SportDto>> _allTournamentsForAllSportsProvider;
    private IDataProvider<AvailableSelectionsDto> _availableSelectionsProvider;
    private IDataProvider<EntityList<TournamentInfoDto>> _availableSportTournamentsProvider;
    private ICacheManager _cacheManager;
    private ICalculateProbabilityFilteredProvider _calculateProbabilityFilteredProvider;
    private ICalculateProbabilityProvider _calculateProbabilityProvider;
    private IDataProvider<CompetitorProfileDto> _competitorProvider;
    private IUofConfiguration _configuration;
    private IDataProvider<DrawDto> _drawFixtureProvider;
    private IDataProvider<DrawDto> _drawSummaryProvider;
    private IDataProvider<EntityList<FixtureChangeDto>> _fixtureChangesProvider;
    private IDataProvider<EntityList<MarketDescriptionDto>> _invariantMarketDescriptionsProvider;
    private IDataProvider<EntityList<SportEventSummaryDto>> _listSportEventProvider;
    private IDataProvider<EntityList<LotteryDto>> _lotteryListProvider;
    private IDataProvider<LotteryDto> _lotteryScheduleProvider;
    private IDataProvider<MatchTimelineDto> _ongoingSportEventProvider;
    private IDataProvider<PlayerProfileDto> _playerProfileProvider;
    private IProducerManager _producerManager;
    private IDataProvider<EntityList<ResultChangeDto>> _resultChangesProvider;
    private IDataProvider<SimpleTeamProfileDto> _simpleTeamProvider;
    private IDataProvider<SportCategoriesDto> _sportCategoriesProvider;
    private IDataProvider<TournamentInfoDto> _sportEventFixtureChangeFixtureForTournamentProvider;
    private IDataProvider<FixtureDto> _sportEventFixtureChangeFixtureProvider;
    private IDataProvider<TournamentInfoDto> _sportEventFixtureForTournamentProvider;
    private IDataProvider<FixtureDto> _sportEventFixtureProvider;
    private IDataProvider<EntityList<SportEventSummaryDto>> _sportEventsForDateProvider;
    private IDataProvider<EntityList<SportEventSummaryDto>> _sportEventsForRaceTournamentProvider;
    private IDataProvider<EntityList<SportEventSummaryDto>> _sportEventsForTournamentProvider;
    private IExecutionPathDataProvider<SportEventSummaryDto> _sportEventSummaryProvider;
    private IDataProvider<PeriodSummaryDto> _stagePeriodSummaryProvider;
    private IDataProvider<TournamentSeasonsDto> _tournamentSeasonsProvider;
    private IDataProvider<EntityList<VariantDescriptionDto>> _variantDescriptionsProvider;
    private IDataProvider<MarketDescriptionDto> _variantMarketDescriptionProvider;

    public DataRouterManagerBuilder WithCacheManager(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
        return this;
    }

    public DataRouterManagerBuilder WithProducerManager(IProducerManager producerManager)
    {
        _producerManager = producerManager;
        return this;
    }

    public DataRouterManagerBuilder WithConfiguration(IUofConfiguration configuration)
    {
        _configuration = configuration;
        return this;
    }

    public DataRouterManagerBuilder WithSportEventSummaryProvider(IExecutionPathDataProvider<SportEventSummaryDto> provider)
    {
        _sportEventSummaryProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithSportEventFixtureProvider(IDataProvider<FixtureDto> provider)
    {
        _sportEventFixtureProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithSportEventFixtureChangeFixtureProvider(IDataProvider<FixtureDto> provider)
    {
        _sportEventFixtureChangeFixtureProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithAllTournamentsForAllSportsProvider(IDataProvider<EntityList<SportDto>> provider)
    {
        _allTournamentsForAllSportsProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithAllSportsProvider(IDataProvider<EntityList<SportDto>> provider)
    {
        _allSportsProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithSportEventsForDateProvider(IDataProvider<EntityList<SportEventSummaryDto>> provider)
    {
        _sportEventsForDateProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithSportEventsForTournamentProvider(IDataProvider<EntityList<SportEventSummaryDto>> provider)
    {
        _sportEventsForTournamentProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithPlayerProfileProvider(IDataProvider<PlayerProfileDto> provider)
    {
        _playerProfileProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithCompetitorProvider(IDataProvider<CompetitorProfileDto> provider)
    {
        _competitorProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithSimpleTeamProvider(IDataProvider<SimpleTeamProfileDto> provider)
    {
        _simpleTeamProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithTournamentSeasonsProvider(IDataProvider<TournamentSeasonsDto> provider)
    {
        _tournamentSeasonsProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithOngoingSportEventProvider(IDataProvider<MatchTimelineDto> provider)
    {
        _ongoingSportEventProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithSportCategoriesProvider(IDataProvider<SportCategoriesDto> provider)
    {
        _sportCategoriesProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithInvariantMarketDescriptionsProvider(IDataProvider<EntityList<MarketDescriptionDto>> provider)
    {
        _invariantMarketDescriptionsProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithVariantMarketDescriptionProvider(IDataProvider<MarketDescriptionDto> provider)
    {
        _variantMarketDescriptionProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithVariantDescriptionsProvider(IDataProvider<EntityList<VariantDescriptionDto>> provider)
    {
        _variantDescriptionsProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithDrawSummaryProvider(IDataProvider<DrawDto> provider)
    {
        _drawSummaryProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithDrawFixtureProvider(IDataProvider<DrawDto> provider)
    {
        _drawFixtureProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithLotteryScheduleProvider(IDataProvider<LotteryDto> provider)
    {
        _lotteryScheduleProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithLotteryListProvider(IDataProvider<EntityList<LotteryDto>> provider)
    {
        _lotteryListProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithAvailableSelectionsProvider(IDataProvider<AvailableSelectionsDto> provider)
    {
        _availableSelectionsProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithCalculateProbabilityProvider(ICalculateProbabilityProvider provider)
    {
        _calculateProbabilityProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithCalculateProbabilityFilteredProvider(ICalculateProbabilityFilteredProvider provider)
    {
        _calculateProbabilityFilteredProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithFixtureChangesProvider(IDataProvider<EntityList<FixtureChangeDto>> provider)
    {
        _fixtureChangesProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithResultChangesProvider(IDataProvider<EntityList<ResultChangeDto>> provider)
    {
        _resultChangesProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithListSportEventProvider(IDataProvider<EntityList<SportEventSummaryDto>> provider)
    {
        _listSportEventProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithAvailableSportTournamentsProvider(IDataProvider<EntityList<TournamentInfoDto>> provider)
    {
        _availableSportTournamentsProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithSportEventFixtureForTournamentProvider(IDataProvider<TournamentInfoDto> provider)
    {
        _sportEventFixtureForTournamentProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithSportEventFixtureChangeFixtureForTournamentProvider(IDataProvider<TournamentInfoDto> provider)
    {
        _sportEventFixtureChangeFixtureForTournamentProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithStagePeriodSummaryProvider(IDataProvider<PeriodSummaryDto> provider)
    {
        _stagePeriodSummaryProvider = provider;
        return this;
    }

    public DataRouterManagerBuilder WithSportEventsForRaceTournamentProvider(IDataProvider<EntityList<SportEventSummaryDto>> provider)
    {
        _sportEventsForRaceTournamentProvider = provider;
        return this;
    }

    public DataRouterManager Build()
    {
        return new DataRouterManager(_cacheManager,
                                     _producerManager,
                                     _configuration,
                                     _sportEventSummaryProvider,
                                     _sportEventFixtureProvider,
                                     _sportEventFixtureChangeFixtureProvider,
                                     _allTournamentsForAllSportsProvider,
                                     _allSportsProvider,
                                     _sportEventsForDateProvider,
                                     _sportEventsForTournamentProvider,
                                     _playerProfileProvider,
                                     _competitorProvider,
                                     _simpleTeamProvider,
                                     _tournamentSeasonsProvider,
                                     _ongoingSportEventProvider,
                                     _sportCategoriesProvider,
                                     _invariantMarketDescriptionsProvider,
                                     _variantMarketDescriptionProvider,
                                     _variantDescriptionsProvider,
                                     _drawSummaryProvider,
                                     _drawFixtureProvider,
                                     _lotteryScheduleProvider,
                                     _lotteryListProvider,
                                     _availableSelectionsProvider,
                                     _calculateProbabilityProvider,
                                     _calculateProbabilityFilteredProvider,
                                     _fixtureChangesProvider,
                                     _resultChangesProvider,
                                     _listSportEventProvider,
                                     _availableSportTournamentsProvider,
                                     _sportEventFixtureForTournamentProvider,
                                     _sportEventFixtureChangeFixtureForTournamentProvider,
                                     _stagePeriodSummaryProvider,
                                     _sportEventsForRaceTournamentProvider);
    }
}

// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Enums;
using Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    internal class MarketFactory : IMarketFactory
    {
        private readonly IMarketCacheProvider _marketCacheProvider;
        private readonly INameProviderFactory _nameProviderFactory;
        private readonly IMarketMappingProviderFactory _mappingProviderFactory;
        private readonly INamedValuesProvider _namedValuesProvider;
        private readonly INamedValueCache _voidReasonCache;
        private readonly ExceptionHandlingStrategy _externalExceptionStrategy;

        public MarketFactory(IMarketCacheProvider marketCacheProvider,
                             INameProviderFactory nameProviderFactory,
                             IMarketMappingProviderFactory mappingProviderFactory,
                             INamedValuesProvider namedValuesProvider,
                             INamedValueCache voidReasonCache,
                             ExceptionHandlingStrategy externalExceptionStrategy)
        {
            _ = Guard.Argument(nameProviderFactory, nameof(nameProviderFactory)).NotNull();
            _ = Guard.Argument(namedValuesProvider, nameof(namedValuesProvider)).NotNull();
            _ = Guard.Argument(marketCacheProvider, nameof(marketCacheProvider)).NotNull();
            _ = Guard.Argument(voidReasonCache, nameof(voidReasonCache)).NotNull();

            _nameProviderFactory = nameProviderFactory;
            _mappingProviderFactory = mappingProviderFactory;
            _namedValuesProvider = namedValuesProvider;
            _marketCacheProvider = marketCacheProvider;
            _voidReasonCache = voidReasonCache;
            _externalExceptionStrategy = externalExceptionStrategy;
        }

        public IMarketWithOdds GetMarketWithOdds(ISportEvent sportEvent, oddsChangeMarket marketOddsChange, int producerId, Urn sportId, IReadOnlyCollection<CultureInfo> cultures)
        {
            var specifiers = string.IsNullOrEmpty(marketOddsChange.specifiers)
                ? null
                : FeedMapperHelper.GetSpecifiers(marketOddsChange.specifiers);

            var additionalInfo = string.IsNullOrEmpty(marketOddsChange.extended_specifiers)
                ? null
                : FeedMapperHelper.GetSpecifiers(marketOddsChange.extended_specifiers);

            var nameProvider = _nameProviderFactory.BuildNameProvider(sportEvent, marketOddsChange.id, specifiers);
            var mappingProvider = _mappingProviderFactory.BuildMarketMappingProvider(sportEvent, marketOddsChange.id, specifiers, producerId, sportId);

            var outcomes = marketOddsChange.outcome?.Select(outcome =>
                GetOutcomeWithOdds(sportEvent,
                    nameProvider,
                    mappingProvider,
                    outcome,
                    new OutcomeDefinition(marketOddsChange.id, outcome.id, _marketCacheProvider, specifiers, cultures, _externalExceptionStrategy), cultures));

            var marketDefinition = new MarketDefinition(marketOddsChange.id, _marketCacheProvider, sportId, producerId, specifiers, cultures, _externalExceptionStrategy);

            return new MarketWithOdds(marketOddsChange.id,
                specifiers,
                additionalInfo,
                nameProvider,
                mappingProvider,
                MessageMapperHelper.GetEnumValue<MarketStatus>(marketOddsChange.status),
                marketOddsChange.cashout_statusSpecified ? (CashoutStatus?)MessageMapperHelper.GetEnumValue<CashoutStatus>(marketOddsChange.cashout_status) : null,
                marketOddsChange.favouriteSpecified && marketOddsChange.favourite == 1,
                outcomes,
                GetMarketMetadata(marketOddsChange.market_metadata),
                marketDefinition,
                cultures);
        }

        public IMarketWithProbabilities GetMarketWithProbabilities(ISportEvent sportEvent, oddsChangeMarket marketOddsChange, int producerId, Urn sportId, IReadOnlyCollection<CultureInfo> cultures)
        {
            var specifiers = string.IsNullOrEmpty(marketOddsChange.specifiers)
                ? null
                : FeedMapperHelper.GetSpecifiers(marketOddsChange.specifiers);

            var additionalInfo = string.IsNullOrEmpty(marketOddsChange.extended_specifiers)
                ? null
                : FeedMapperHelper.GetSpecifiers(marketOddsChange.extended_specifiers);

            var nameProvider = _nameProviderFactory.BuildNameProvider(sportEvent, marketOddsChange.id, specifiers);
            var mappingProvider = _mappingProviderFactory.BuildMarketMappingProvider(sportEvent, marketOddsChange.id, specifiers, producerId, sportId);

            var marketDefinition = new MarketDefinition(marketOddsChange.id, _marketCacheProvider, sportId, producerId, specifiers, cultures, _externalExceptionStrategy);

            return new MarketWithProbabilities(marketOddsChange.id,
                specifiers,
                additionalInfo,
                nameProvider,
                mappingProvider,
                MessageMapperHelper.GetEnumValue<MarketStatus>(marketOddsChange.status),
                marketOddsChange.outcome?.Select(outcomeOdds =>
                    new OutcomeProbabilities(outcomeOdds.id,
                        IsOutcomeActive(outcomeOdds),
                        GetOutcomeProbabilities(outcomeOdds),
                        nameProvider,
                        mappingProvider,
                        cultures,
                        new OutcomeDefinition(marketOddsChange.id, outcomeOdds.id, _marketCacheProvider, specifiers, cultures, _externalExceptionStrategy),
                        GetAdditionalProbabilities(outcomeOdds))),
                marketDefinition,
                cultures,
                marketOddsChange.cashout_statusSpecified ? (CashoutStatus?)MessageMapperHelper.GetEnumValue<CashoutStatus>(marketOddsChange.cashout_status) : null,
                GetMarketMetadata(marketOddsChange.market_metadata));
        }

        public IMarketCancel GetMarketCancel(ISportEvent sportEvent, market market, int producerId, Urn sportId, IReadOnlyCollection<CultureInfo> cultures)
        {
            _ = Guard.Argument(sportEvent, nameof(sportEvent)).NotNull();
            _ = Guard.Argument(market, nameof(market)).NotNull();

            var specifiers = string.IsNullOrEmpty(market.specifiers)
                ? null
                : FeedMapperHelper.GetSpecifiers(market.specifiers);

            var additionalInfo = string.IsNullOrEmpty(market.extended_specifiers)
                ? null
                : FeedMapperHelper.GetSpecifiers(market.extended_specifiers);

            var marketDefinition = new MarketDefinition(market.id, _marketCacheProvider, sportId, producerId, specifiers, cultures, _externalExceptionStrategy);

            return new MarketCancel(market.id,
                specifiers,
                additionalInfo,
                _nameProviderFactory.BuildNameProvider(sportEvent, market.id, specifiers),
                _mappingProviderFactory.BuildMarketMappingProvider(sportEvent, market.id, specifiers, producerId, sportId),
                marketDefinition,
                market.void_reasonSpecified ? market.void_reason : (int?)null,
                _voidReasonCache,
                cultures);
        }

        public IMarketWithSettlement GetMarketWithResults(ISportEvent sportEvent, betSettlementMarket marketSettlement, int producerId, Urn sportId, IReadOnlyCollection<CultureInfo> cultures)
        {
            _ = Guard.Argument(sportEvent, nameof(sportEvent)).NotNull("SportEvent missing");
            _ = Guard.Argument(marketSettlement, nameof(marketSettlement)).NotNull("marketSettlement != null");

            var specifiers = string.IsNullOrEmpty(marketSettlement.specifiers)
                ? null
                : FeedMapperHelper.GetSpecifiers(marketSettlement.specifiers);

            var additionalInfo = string.IsNullOrEmpty(marketSettlement.extended_specifiers)
                ? null
                : FeedMapperHelper.GetSpecifiers(marketSettlement.extended_specifiers);

            var nameProvider = _nameProviderFactory.BuildNameProvider(sportEvent, marketSettlement.id, specifiers);
            var mappingProvider = _mappingProviderFactory.BuildMarketMappingProvider(sportEvent, marketSettlement.id, specifiers, producerId, sportId);
            var outcomes = (IEnumerable<IOutcomeSettlement>)marketSettlement.Items?.Select(outcome => new OutcomeSettlement(
                               outcome.dead_heat_factorSpecified ? (double?)outcome.dead_heat_factor : null,
                               outcome.id,
                               outcome.result,
                               MessageMapperHelper.GetVoidFactor(outcome.void_factorSpecified, outcome.void_factor),
                               nameProvider,
                               mappingProvider,
                               cultures,
                               new OutcomeDefinition(marketSettlement.id, outcome.id, _marketCacheProvider, specifiers, cultures, _externalExceptionStrategy)))
                           ?? new List<IOutcomeSettlement>();

            var marketDefinition = new MarketDefinition(marketSettlement.id, _marketCacheProvider, sportId, producerId, specifiers, cultures, _externalExceptionStrategy);

            return new MarketWithSettlement(marketSettlement.id,
                specifiers,
                additionalInfo,
                outcomes.ToList(),
                nameProvider,
                mappingProvider,
                marketDefinition,
                marketSettlement.void_reasonSpecified ? (int?)marketSettlement.void_reason : null,
                _namedValuesProvider.VoidReasons,
                cultures);
        }

        private static IOutcomeOdds GetOutcomeWithOdds(ISportEvent sportEvent,
                                                       INameProvider nameProvider,
                                                       IMarketMappingProvider mappingProvider,
                                                       oddsChangeMarketOutcome outcome,
                                                       IOutcomeDefinition outcomeDefinition,
                                                       IReadOnlyCollection<CultureInfo> cultures)
        {
            var isValidPlayerOutcome = false;
            IMatch match = null;

            if (outcome.teamSpecified)
            {
                match = sportEvent as IMatch;
                isValidPlayerOutcome = !(match == null || outcome.team < 1 || outcome.team > 2);
            }

            return isValidPlayerOutcome
                ? new PlayerOutcomeOdds(outcome.id,
                    IsOutcomeActive(outcome),
                    outcome.odds,
                    GetOutcomeProbabilities(outcome),
                    nameProvider,
                    mappingProvider,
                    match,
                    outcome.team,
                    cultures,
                    outcomeDefinition,
                    GetAdditionalProbabilities(outcome))
                : new OutcomeOdds(outcome.id,
                    IsOutcomeActive(outcome),
                    outcome.odds,
                    GetOutcomeProbabilities(outcome),
                    nameProvider,
                    mappingProvider,
                    cultures,
                    outcomeDefinition,
                    GetAdditionalProbabilities(outcome));

        }

        private static bool? IsOutcomeActive(oddsChangeMarketOutcome outcome)
        {
            return outcome.activeSpecified ? (bool?)(outcome.active != 0) : null;
        }

        private static double? GetOutcomeProbabilities(oddsChangeMarketOutcome outcome)
        {
            return outcome.probabilitiesSpecified ? (double?)outcome.probabilities : null;
        }

        private static AdditionalProbabilities GetAdditionalProbabilities(oddsChangeMarketOutcome outcome)
        {
            return HasOutcomeAnyAdditionalProbabilities(outcome)
                ? CreateAdditionalProbabilities(outcome)
                : null;
        }

        private static bool HasOutcomeAnyAdditionalProbabilities(oddsChangeMarketOutcome outcome)
        {
            return outcome.win_probabilitiesSpecified
                   || outcome.lose_probabilitiesSpecified
                   || outcome.half_win_probabilitiesSpecified
                   || outcome.half_lose_probabilitiesSpecified
                   || outcome.refund_probabilitiesSpecified;
        }

        private static AdditionalProbabilities CreateAdditionalProbabilities(oddsChangeMarketOutcome outcome)
        {
            return new AdditionalProbabilities(
                outcome.win_probabilitiesSpecified ? outcome.win_probabilities : (double?)null,
                outcome.lose_probabilitiesSpecified ? outcome.lose_probabilities : (double?)null,
                outcome.half_win_probabilitiesSpecified ? outcome.half_win_probabilities : (double?)null,
                outcome.half_lose_probabilitiesSpecified ? outcome.half_lose_probabilities : (double?)null,
                outcome.refund_probabilitiesSpecified ? outcome.refund_probabilities : (double?)null);
        }

        [SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
        private static MarketMetadata GetMarketMetadata(marketMetadata marketMetadata)
        {
            return marketMetadata == null ||
                   (!marketMetadata.aams_idSpecified
                    && !marketMetadata.start_timeSpecified
                    && !marketMetadata.end_timeSpecified
                    && !marketMetadata.next_betstopSpecified)
                ? null
                : new MarketMetadata(marketMetadata);

        }
    }
}

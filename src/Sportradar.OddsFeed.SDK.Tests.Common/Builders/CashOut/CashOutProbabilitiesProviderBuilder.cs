// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Moq;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Managers;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.CashOut;

internal sealed class CashOutProbabilitiesProviderBuilder
{
    private const string UriFormat = "https://localhost/v1/probabilities/{0}";

    private IUofConfiguration _configuration;
    private IDataFetcher _cashOutFetcher;
    private ISportEntityFactory _sportEntityFactory;
    private IMarketFactory _marketFactory;
    private IProducerManager _producerManager;
    private INamedValueCache _betstopReasons;
    private INamedValueCache _bettingStatus;

    private CashOutProbabilitiesProviderBuilder()
    {
    }

    public static CashOutProbabilitiesProviderBuilder Create()
    {
        return new CashOutProbabilitiesProviderBuilder();
    }

    public CashOutProbabilitiesProviderBuilder WithConfiguration(IUofConfiguration configuration)
    {
        _configuration = configuration;
        return this;
    }

    public CashOutProbabilitiesProviderBuilder WithCashOutFetcher(IDataFetcher cashOutFetcher)
    {
        _cashOutFetcher = cashOutFetcher;
        return this;
    }

    public CashOutProbabilitiesProviderBuilder WithSportEntityFactory(ISportEntityFactory sportEntityFactory)
    {
        _sportEntityFactory = sportEntityFactory;
        return this;
    }

    public CashOutProbabilitiesProviderBuilder WithMarketFactory(IMarketFactory marketFactory)
    {
        _marketFactory = marketFactory;
        return this;
    }

    public CashOutProbabilitiesProviderBuilder WithProducerManager(IProducerManager producerManager)
    {
        _producerManager = producerManager;
        return this;
    }

    public CashOutProbabilitiesProviderBuilder WithBetstopReasonsCache(INamedValueCache betstopReasons)
    {
        _betstopReasons = betstopReasons;
        return this;
    }

    public CashOutProbabilitiesProviderBuilder WithBettingStatusCache(INamedValueCache bettingStatus)
    {
        _bettingStatus = bettingStatus;
        return this;
    }

    public ICashOutProbabilitiesProvider Build()
    {
        var cacheOutDeserializer = new Deserializer<cashout>();
        var dataProvider = new NonMappingDataProvider<cashout>(UriFormat, _cashOutFetcher, cacheOutDeserializer);

        var feedMessageMapper = new FeedMessageMapper(_sportEntityFactory,
                                                      _marketFactory,
                                                      _producerManager,
                                                      new NamedValuesProvider(new Mock<INamedValueCache>().Object, _betstopReasons, _bettingStatus, new Mock<ILocalizedNamedValueCache>().Object),
                                                      _configuration.ExceptionHandlingStrategy);

        return new CashOutProbabilitiesProvider(dataProvider, feedMessageMapper, _configuration);
    }
}

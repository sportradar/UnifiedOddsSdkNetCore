// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.SportEvent;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.OutMarket;

[SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
public class MarketNameSetup
{
    private readonly Mock<ILogHttpDataFetcher> _invariantFetcherMock = new();
    private readonly Mock<ILogHttpDataFetcher> _variantListFetcherMock = new();
    private readonly Mock<ILogHttpDataFetcherFastFailing> _singleVariantFetcherMock = new();
    internal readonly CultureInfo DefaultLanguage = TestConsts.CultureEn;
    internal readonly Mock<ISportEvent> AnySportEventMock = new();
    internal readonly Mock<CompetitionForMocks> CompetitionMock = new();
    private readonly Mock<IProfileCache> _profileCacheMock = new();
    internal readonly IMarketFactory MarketFactory;
    internal readonly IMarketFactory MarketFactoryWithCatchStrategy;

    protected MarketNameSetup(ITestOutputHelper testOutputHelper)
    {
        ILoggerFactory loggerFactory = new XunitLoggerFactory(testOutputHelper);

        var cacheManager = new CacheManager();
        var dataRouterManager = new DataRouterManagerBuilder()
            .AddMockedDependencies()
            .WithCacheManager(cacheManager)
            .WithInvariantMarketDescriptionsFetcher(_invariantFetcherMock.Object)
            .WithVariantDescriptionsFetcher(_variantListFetcherMock.Object)
            .WithVariantMarketDescriptionFetcher(_singleVariantFetcherMock.Object)
            .Build();

        var marketCacheProvider = MarketCacheProviderBuilder.Create()
                                                            .WithCacheManager(cacheManager)
                                                            .WithDataRouterManager(dataRouterManager)
                                                            .WithLanguages([DefaultLanguage])
                                                            .WithLoggerFactory(loggerFactory)
                                                            .WithProfileCache(_profileCacheMock.Object)
                                                            .Build();

        var nameExpressionFactory = new NameExpressionFactory(new OperandFactory(), _profileCacheMock.Object);
        var nameProviderFactory = new NameProviderFactory(marketCacheProvider,
            _profileCacheMock.Object,
            nameExpressionFactory,
            ExceptionHandlingStrategy.Throw,
            loggerFactory);
        var marketMappingProviderFactory = new MarketMappingProviderFactory(marketCacheProvider, new Mock<ISportEventStatusCache>().Object, ExceptionHandlingStrategy.Throw);

        var namedValuesCacheMock = new Mock<INamedValueCache>();
        namedValuesCacheMock.Setup(x => x.IsValueDefined(It.IsAny<int>())).Returns(true);
        namedValuesCacheMock.Setup(x => x.GetNamedValue(It.IsAny<int>())).Returns((int id) => new NamedValue(id, "somevalue"));

        var namedValuesProviderMock = new Mock<INamedValuesProvider>();
        namedValuesProviderMock.Setup(x => x.VoidReasons).Returns(namedValuesCacheMock.Object);
        namedValuesProviderMock.Setup(x => x.BetStopReasons).Returns(namedValuesCacheMock.Object);
        namedValuesProviderMock.Setup(x => x.BettingStatuses).Returns(namedValuesCacheMock.Object);

        MarketFactory = new MarketFactory(marketCacheProvider, nameProviderFactory, marketMappingProviderFactory, namedValuesProviderMock.Object, namedValuesCacheMock.Object, ExceptionHandlingStrategy.Throw);

        var nameProviderFactoryCatch = new NameProviderFactory(marketCacheProvider,
            _profileCacheMock.Object,
            nameExpressionFactory,
            ExceptionHandlingStrategy.Catch,
            loggerFactory);
        var marketMappingProviderFactoryCatch = new MarketMappingProviderFactory(marketCacheProvider, new Mock<ISportEventStatusCache>().Object, ExceptionHandlingStrategy.Catch);
        MarketFactoryWithCatchStrategy = new MarketFactory(marketCacheProvider, nameProviderFactoryCatch, marketMappingProviderFactoryCatch, namedValuesProviderMock.Object, namedValuesCacheMock.Object, ExceptionHandlingStrategy.Catch);
    }

    protected static void SetupDataInMarketDescription(desc_market apiMarketDescription, string marketNameTemplate, IDictionary<string, string> newOutcomes)
    {
        if (!marketNameTemplate.IsNullOrEmpty())
        {
            apiMarketDescription.name = marketNameTemplate;
        }
        if (newOutcomes.IsNullOrEmpty())
        {
            return;
        }
        foreach (var newOutcome in newOutcomes)
        {
            var apiOutcome = apiMarketDescription.outcomes.FirstOrDefault(f => f.id == newOutcome.Key);
            if (apiOutcome != null)
            {
                apiOutcome.name = newOutcome.Value;
            }
        }
    }

    protected void SetupInvariantMarketListEndpoint(desc_market marketDescription, CultureInfo language)
    {
        var xmlResponse = MarketDescriptionEndpoint.GetInvariantList(marketDescription);
        _invariantFetcherMock
           .Setup(f => f.GetDataAsync(It.IsAny<Uri>()))
           .Returns(() => Task.FromResult(DeserializerHelper.SerializeToMemoryStream(xmlResponse)));
    }

    protected void SetupVariantMarketListEndpoint(desc_variant marketDescription, CultureInfo language)
    {
        var xmlResponse = MarketDescriptionEndpoint.GetVariantList(marketDescription);
        _variantListFetcherMock
           .Setup(f => f.GetDataAsync(It.IsAny<Uri>()))
           .Returns(() => Task.FromResult(DeserializerHelper.SerializeToMemoryStream(xmlResponse)));
    }

    protected void SetupSingleVariantMarketEndpoint(desc_market marketDescription, CultureInfo language)
    {
        var xmlResponse = MarketDescriptionEndpoint.GetSingleVariantList(marketDescription);
        _singleVariantFetcherMock
           .Setup(f => f.GetDataAsync(It.IsAny<Uri>()))
           .Returns(() => Task.FromResult(DeserializerHelper.SerializeToMemoryStream(xmlResponse)));
    }

    protected void SetupSingleVariantMarketEndpointSequence(CultureInfo language, params desc_market[] descriptors)
    {
        var sequence = _singleVariantFetcherMock.SetupSequence(f => f.GetDataAsync(It.IsAny<Uri>()));
        foreach (var descriptor in descriptors)
        {
            var xmlResponse = MarketDescriptionEndpoint.GetSingleVariantList(descriptor);
            sequence.Returns(() => Task.FromResult(DeserializerHelper.SerializeToMemoryStream(xmlResponse)));
        }
    }

    protected void SetupCompetitionWithCompetitorIds(matchSummaryEndpoint endpoint, CultureInfo language = null)
    {
        var competitorIds = endpoint.sport_event.competitors.Select(s => Urn.Parse(s.id)).ToList();
        if (language == null)
        {
            CompetitionMock.Setup(s => s.GetCompetitorIdsAsync(null)).ReturnsAsync(competitorIds);
        }
        else
        {
            CompetitionMock.Setup(s => s.GetCompetitorIdsAsync(language)).ReturnsAsync(competitorIds);
        }
    }

    protected void SetupProfileCacheToReturnCompetitorNameWithFetching(competitorProfileEndpoint endpoint, CultureInfo language)
    {
        var competitorId = Urn.Parse(endpoint.competitor.id);
        _profileCacheMock.Setup(s => s.GetCompetitorNameAsync(competitorId, language, true)).ReturnsAsync(endpoint.competitor.name);
    }

    protected void SetupProfileCacheToReturnCompetitorNameWithoutFetching(competitorProfileEndpoint endpoint, CultureInfo language)
    {
        var competitorId = Urn.Parse(endpoint.competitor.id);
        _profileCacheMock.Setup(s => s.GetCompetitorNameAsync(competitorId, language, false)).ReturnsAsync(endpoint.competitor.name);
    }

    protected void SetupProfileCacheToReturnPlayerNameWithFetching(playerProfileEndpoint endpoint, CultureInfo language)
    {
        var playerId = Urn.Parse(endpoint.player.id);
        _profileCacheMock.Setup(s => s.GetPlayerNameAsync(playerId, language, true)).ReturnsAsync(endpoint.player.name);
    }

    protected void SetupProfileCacheToReturnPlayerNameWithoutFetching(playerProfileEndpoint endpoint, CultureInfo language)
    {
        var playerId = Urn.Parse(endpoint.player.id);
        _profileCacheMock.Setup(s => s.GetPlayerNameAsync(playerId, language, false)).ReturnsAsync(endpoint.player.name);
    }

    protected static matchSummaryEndpoint ReplaceMatchCompetitorPrefixFromSrToBg(matchSummaryEndpoint apiMatch)
    {
        foreach (var competitor in apiMatch.sport_event.competitors)
        {
            competitor.id = competitor.id.Replace("sr:competitor:", "bg:competitor:");
        }
        return apiMatch;
    }

    protected void VerifySingleVariantProviderWasCalled(Times times)
    {
        _singleVariantFetcherMock.Verify(f => f.GetDataAsync(It.IsAny<Uri>()), times);
    }
}

// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
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
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
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
    private readonly Mock<IDataProvider<EntityList<MarketDescriptionDto>>> _invariantMdProviderMock = new Mock<IDataProvider<EntityList<MarketDescriptionDto>>>();
    private readonly Mock<IDataProvider<EntityList<VariantDescriptionDto>>> _variantMdProviderMock = new Mock<IDataProvider<EntityList<VariantDescriptionDto>>>();
    private readonly Mock<IDataProvider<MarketDescriptionDto>> _singleVariantMdProviderMock = new Mock<IDataProvider<MarketDescriptionDto>>();
    internal readonly CultureInfo DefaultLanguage = TestConsts.CultureEn;
    internal readonly Mock<ISportEvent> AnySportEventMock = new Mock<ISportEvent>();
    internal readonly Mock<CompetitionForMocks> CompetitionMock = new Mock<CompetitionForMocks>();
    private readonly Mock<IProfileCache> _profileCacheMock = new Mock<IProfileCache>();
    internal readonly IMarketFactory MarketFactory;

    protected MarketNameSetup(ITestOutputHelper testOutputHelper)
    {
        ILoggerFactory loggerFactory = new XunitLoggerFactory(testOutputHelper);

        var cacheManager = new CacheManager();
        var dataRouterManager = new DataRouterManagerBuilder()
            .AddMockedDependencies()
            .WithCacheManager(cacheManager)
            .WithInvariantMarketDescriptionsProvider(_invariantMdProviderMock.Object)
            .WithVariantDescriptionsProvider(_variantMdProviderMock.Object)
            .WithVariantMarketDescriptionProvider(_singleVariantMdProviderMock.Object)
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
        _invariantMdProviderMock
           .Setup(s => s.GetDataAsync(It.Is(language.TwoLetterISOLanguageName, StringComparer.OrdinalIgnoreCase)))
           .ReturnsAsync(MarketDescriptionEndpoint.GetInvariantDto(marketDescription));
    }

    protected void SetupVariantMarketListEndpoint(desc_variant marketDescription, CultureInfo language)
    {
        _variantMdProviderMock
           .Setup(s => s.GetDataAsync(It.Is(language.TwoLetterISOLanguageName, StringComparer.OrdinalIgnoreCase)))
           .ReturnsAsync(MarketDescriptionEndpoint.GetVariantDto(marketDescription));
    }

    protected void SetupSingleVariantMarketEndpoint(desc_market marketDescription, CultureInfo language)
    {
        _singleVariantMdProviderMock
           .Setup(s => s.GetDataAsync(marketDescription.id.ToString(), It.Is(language.TwoLetterISOLanguageName, StringComparer.OrdinalIgnoreCase), marketDescription.variant))
           .ReturnsAsync(MarketDescriptionEndpoint.GetSingleVariantDto(marketDescription));
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
        _singleVariantMdProviderMock.Verify(v => v.GetDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), times);
    }
}

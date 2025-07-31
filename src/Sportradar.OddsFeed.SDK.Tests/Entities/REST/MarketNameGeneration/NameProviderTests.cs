// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.MarketNameGeneration;

[SuppressMessage("ReSharper", "ConvertToPrimaryConstructor")]
public class NameProviderTests
{
    private readonly IMarketCacheProvider _marketCacheProvider;
    private readonly IProfileCache _profileCache;
    private readonly ILogger _logger;
    private readonly Dictionary<string, string> _defaultSpecifiers = new Dictionary<string, string> { { "inningnr", "5" }, { "runnr", "2" } };
    private readonly Mock<IDataProvider<EntityList<MarketDescriptionDto>>> _invariantMdProviderMock;
    private readonly Mock<IDataProvider<EntityList<VariantDescriptionDto>>> _variantMdProviderMock;
    private readonly Mock<IDataProvider<MarketDescriptionDto>> _singleVariantMdProviderMock;

    public NameProviderTests(ITestOutputHelper outputHelper)
    {
        _logger = new XUnitLogger("NameProvider", outputHelper);
        var testLoggerFactory = new XunitLoggerFactory(outputHelper);

        _invariantMdProviderMock = new Mock<IDataProvider<EntityList<MarketDescriptionDto>>>();
        _variantMdProviderMock = new Mock<IDataProvider<EntityList<VariantDescriptionDto>>>();
        _singleVariantMdProviderMock = new Mock<IDataProvider<MarketDescriptionDto>>();

        _profileCache = new Mock<IProfileCache>().Object;

        var cacheManager = new CacheManager();
        var dataRouterManager = new DataRouterManagerBuilder()
            .AddMockedDependencies()
            .WithCacheManager(cacheManager)
            .WithInvariantMarketDescriptionsProvider(_invariantMdProviderMock.Object)
            .WithVariantDescriptionsProvider(_variantMdProviderMock.Object)
            .WithVariantMarketDescriptionProvider(_singleVariantMdProviderMock.Object)
            .Build();

        _marketCacheProvider = MarketCacheProviderBuilder.Create()
            .WithCacheManager(cacheManager)
            .WithDataRouterManager(dataRouterManager)
            .WithLanguages(TestData.Cultures1)
            .WithLoggerFactory(testLoggerFactory)
            .WithProfileCache(_profileCache)
            .Build();
    }

    [Theory]
    [InlineData(null, "When will the 2nd run be scored (incl. extra innings)")]
    [InlineData("When will the goal be scored", "When will the goal be scored")]
    [InlineData("When will the {runnr} run be scored", "When will the 2 run be scored")]
    [InlineData("When will the {!runnr} run be scored", "When will the 2nd run be scored")]
    [InlineData("When will the {!inningnr} run be scored", "When will the 5th run be scored")]
    [InlineData("When will the {+runnr} run be scored", "When will the +2 run be scored")]
    [InlineData("When will the {-runnr} run be scored", "When will the -2 run be scored")]
    [InlineData("When will the {!runnr} run in {inningnr} be scored", "When will the 2nd run in 5 be scored")]
    [InlineData("When will the {(runnr+1)} run be scored", "When will the 3 run be scored")]
    [InlineData("When will the {(runnr-1)} run be scored", "When will the 1 run be scored")]
    [InlineData("When will the {!(runnr+1)} run be scored", "When will the 3rd run be scored")]
    [InlineData("When will the {!(runnr-1)} run be scored", "When will the 1st run be scored")]
    [InlineData("When will the {(1+runnr)} run be scored", "When will the 3 run be scored")]
    [InlineData("When will the {(4-runnr)} run be scored", "When will the 2 run be scored")]
    [InlineData("When will the {!(1+runnr)} run be scored", "When will the 3rd run be scored")]
    [InlineData("When will the {!(5-runnr)} run be scored", "When will the 3rd run be scored")]
    public async Task WhenMarketNameIsRequestedThenReturnsCorrectName(string marketNameTemplate, string expectedMarketName)
    {
        var apiMarketDescription = MdForInvariantList.GetOrdinalMarket739();
        SetupDataInMarketDescription(apiMarketDescription, 1, marketNameTemplate, null);
        _invariantMdProviderMock.Setup(s => s.GetDataAsync(It.IsAny<string>())).ReturnsAsync(MarketDescriptionEndpoint.GetInvariantDto(apiMarketDescription));

        var nameProvider = GetNameProviderForMarketWithDefaultSpecifiers(apiMarketDescription.id);

        var marketName = await nameProvider.GetMarketNameAsync(TestData.Culture);

        marketName.Should().Be(expectedMarketName);
    }

    [Theory]
    [InlineData("1826", "other inning or no run", "other inning or no run")]
    [InlineData("1826", "{inningnr} inning", "5 inning")]
    [InlineData("1826", "{+inningnr} inning", "+5 inning")]
    [InlineData("1826", "{-inningnr} inning", "-5 inning")]
    [InlineData("1826", "{!inningnr} inning", "5th inning")]
    [InlineData("1826", "{!inningnr} inning and {runnr}", "5th inning and 2")]
    [InlineData("1826", "{(inningnr+1)} inning", "6 inning")]
    [InlineData("1826", "{(inningnr-1)} inning", "4 inning")]
    [InlineData("1826", "{(1+inningnr)} inning", "6 inning")]
    [InlineData("1826", "{(10-inningnr)} inning", "5 inning")]
    [InlineData("1826", "{!(inningnr+1)} inning", "6th inning")]
    [InlineData("1826", "{!(inningnr-1)} inning", "4th inning")]
    [InlineData("1826", "{!(1+inningnr)} inning", "6th inning")]
    [InlineData("1826", "{!(10-inningnr)} inning", "5th inning")]
    public async Task WhenOutcomeNameIsRequestedThenReturnsCorrectName(string outcomeId, string outcomeNameTemplate, string expectedOutcomeName)
    {
        var newOutcomes = new Dictionary<string, string> { { outcomeId, outcomeNameTemplate } };
        var apiMarketDescription = MdForInvariantList.GetOrdinalMarket739();
        SetupDataInMarketDescription(apiMarketDescription, 1, null, newOutcomes);
        _invariantMdProviderMock.Setup(s => s.GetDataAsync(It.IsAny<string>())).ReturnsAsync(MarketDescriptionEndpoint.GetInvariantDto(apiMarketDescription));

        var nameProvider = GetNameProviderForMarketWithDefaultSpecifiers(apiMarketDescription.id);

        var outcomeName = await nameProvider.GetOutcomeNameAsync(outcomeId, TestData.Culture);

        outcomeName.Should().Be(expectedOutcomeName);
    }

    [Fact]
    public async Task GetSingleVariantMarketName()
    {
        var specifiers = new Dictionary<string, string> { { "variant", "pre:markettext:289339" } };
        var apiInvariantMd = MdForInvariantList.GetPreOutcomeTextMarket535();
        var apiSingleVariantMd = MdForVariantSingle.GetPreMarketTextMarket535();
        _invariantMdProviderMock.Setup(s => s.GetDataAsync(It.IsAny<string>())).ReturnsAsync(MarketDescriptionEndpoint.GetInvariantDto(apiInvariantMd));
        _singleVariantMdProviderMock.Setup(s => s.GetDataAsync(apiInvariantMd.id.ToString(), TestData.Culture.TwoLetterISOLanguageName, specifiers.First().Value)).ReturnsAsync(new MarketDescriptionDto(apiSingleVariantMd));

        var nameProvider = GetNameProviderForMarket(apiInvariantMd.id, specifiers);

        var marketName = await nameProvider.GetMarketNameAsync(TestData.Culture);

        marketName.Should().Be(apiSingleVariantMd.name);
    }

    [Fact]
    public async Task GetVariantListMarketName()
    {
        var specifiers = new Dictionary<string, string> { { "variant", "sr:correct_score:bestof:12" } };
        var apiInvariantMd = MdForInvariantList.GetMarket199ForVariantList();
        var apiVariantMd = MdForVariantList.GetCorrectScoreBestOf12();
        _invariantMdProviderMock.Setup(s => s.GetDataAsync(It.IsAny<string>())).ReturnsAsync(MarketDescriptionEndpoint.GetInvariantDto(apiInvariantMd));
        _variantMdProviderMock.Setup(s => s.GetDataAsync(It.IsAny<string>())).ReturnsAsync(MarketDescriptionEndpoint.GetVariantDto(apiVariantMd));

        var nameProvider = GetNameProviderForMarket(apiInvariantMd.id, specifiers);

        var marketName = await nameProvider.GetMarketNameAsync(TestData.Culture);

        marketName.Should().Be(apiInvariantMd.name);
    }

    [Fact]
    public async Task GetSingleVariantMarketNameWhenNotFoundThenThrows()
    {
        var specifiers = new Dictionary<string, string> { { "variant", "pre:markettext:289339" } };
        var apiInvariantMd = MdForInvariantList.GetPreOutcomeTextMarket535();
        _invariantMdProviderMock.Setup(s => s.GetDataAsync(It.IsAny<string>())).ReturnsAsync(MarketDescriptionEndpoint.GetInvariantDto(apiInvariantMd));
        _singleVariantMdProviderMock.Setup(s => s.GetDataAsync(apiInvariantMd.id.ToString(), TestData.Culture.TwoLetterISOLanguageName, specifiers.First().Value)).ReturnsAsync((MarketDescriptionDto)null);

        var nameProvider = GetNameProviderForMarket(apiInvariantMd.id, specifiers);

        var exception = await Assert.ThrowsAsync<NameGenerationException>(() => nameProvider.GetMarketNameAsync(TestData.Culture));

        exception.Message.Should().Be("Missing market descriptor");
    }

    private NameProvider GetNameProviderForMarketWithDefaultSpecifiers(int marketId)
    {
        return GetNameProviderForMarket(marketId, _defaultSpecifiers);
    }

    private NameProvider GetNameProviderForMarket(int marketId, Dictionary<string, string> specifiers)
    {
        return new NameProvider(_marketCacheProvider,
            _profileCache,
            new NameExpressionFactory(new OperandFactory(), _profileCache),
            new Mock<IMatch>().Object,
            marketId,
            specifiers,
            ExceptionHandlingStrategy.Throw,
            _logger);
    }

    private static void SetupDataInMarketDescription(desc_market apiMarketDescription, int marketId, string marketNameTemplate, IDictionary<string, string> newOutcomes)
    {
        apiMarketDescription.id = marketId;
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
}

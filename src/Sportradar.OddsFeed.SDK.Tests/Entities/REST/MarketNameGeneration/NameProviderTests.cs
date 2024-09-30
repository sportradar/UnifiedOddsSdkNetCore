// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.InternalEntities;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.MarketNameGeneration;

[SuppressMessage("ReSharper", "ConvertToPrimaryConstructor")]
public class NameProviderTests
{
    private readonly Mock<IMarketCacheProvider> _marketCacheProviderMock;
    private readonly Mock<IProfileCache> _profileCacheMock;
    private readonly ILogger _logger;
    private readonly Dictionary<string, string> _defaultSpecifiers = new Dictionary<string, string> { { "inningnr", "5" }, { "runnr", "2" } };

    public NameProviderTests(ITestOutputHelper outputHelper)
    {
        _logger = new XUnitLogger("NameProvider", outputHelper);
        _marketCacheProviderMock = new Mock<IMarketCacheProvider>();
        _profileCacheMock = new Mock<IProfileCache>();
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
        var apiMarketDescription = CreateDefaultApiMarketDescription();
        SetupDataInMarketDescription(apiMarketDescription, 1, marketNameTemplate, null);
        SetupMarketCacheProviderMock(apiMarketDescription);

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
        var apiMarketDescription = CreateDefaultApiMarketDescription();
        SetupDataInMarketDescription(apiMarketDescription, 1, null, newOutcomes);
        SetupMarketCacheProviderMock(apiMarketDescription);

        var nameProvider = GetNameProviderForMarketWithDefaultSpecifiers(apiMarketDescription.id);

        var outcomeName = await nameProvider.GetOutcomeNameAsync(outcomeId, TestData.Culture);

        outcomeName.Should().Be(expectedOutcomeName);
    }

    private NameProvider GetNameProviderForMarketWithDefaultSpecifiers(int marketId)
    {
        return new NameProvider(_marketCacheProviderMock.Object,
                                _profileCacheMock.Object,
                                new NameExpressionFactory(new OperandFactory(), _profileCacheMock.Object),
                                new Mock<IMatch>().Object,
                                marketId,
                                _defaultSpecifiers,
                                ExceptionHandlingStrategy.Throw,
                                _logger);
    }

    private void SetupMarketCacheProviderMock(desc_market apiMarketDescription)
    {
        var marketDescriptionDto = new MarketDescriptionDto(apiMarketDescription);
        var marketDescriptionCacheItem = MarketDescriptionCacheItem.Build(marketDescriptionDto, new MappingValidatorFactory(), TestData.Culture, "mock");
        var marketDescription = new MarketDescription(marketDescriptionCacheItem, TestData.Cultures1);

        _marketCacheProviderMock.Setup(x => x.GetMarketDescriptionAsync(apiMarketDescription.id, It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<IReadOnlyCollection<CultureInfo>>(), It.IsAny<bool>()))
                                .ReturnsAsync(marketDescription);
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

    private static desc_market CreateDefaultApiMarketDescription()
    {
        return new desc_market
        {
            id = 739, name = "When will the {!runnr} run be scored (incl. extra innings)", groups = "all|score|incl_ei", outcomes =
                   [
                       new desc_outcomesOutcome { id = "1826", name = "{!inningnr} inning" },
                       new desc_outcomesOutcome { id = "1828", name = "{!(inningnr+1)} inning" },
                       new desc_outcomesOutcome { id = "1829", name = "{!(inningnr+2)} inning" },
                       new desc_outcomesOutcome { id = "1830", name = "other inning or no run" }
                   ],
            specifiers =
                   [
                       new desc_specifiersSpecifier { name = "inningnr", type = "integer" },
                       new desc_specifiersSpecifier { name = "runnr", type = "integer" }
                   ]
        };
    }
}

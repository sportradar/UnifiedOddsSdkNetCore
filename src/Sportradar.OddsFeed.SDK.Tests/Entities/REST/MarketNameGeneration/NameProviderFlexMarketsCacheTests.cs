// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.MarketNameGeneration;

public class NameProviderFlexMarketsCacheTests
{
    private readonly INameProvider _nameProvider;

    public NameProviderFlexMarketsCacheTests(ITestOutputHelper outputHelper)
    {
        var specifiers = new Dictionary<string, string> { { "score", "1:1" } };

        var loggerFactory = new XunitLoggerFactory(outputHelper);
        var testLoggerFactory = new XunitLoggerFactory(outputHelper);

        var profileCache = new Mock<IProfileCache>().Object;

        _ = new MappingValidatorFactory();
        var cacheManager = new CacheManager();
        var dataRouterManager = new DataRouterManagerBuilder()
            .AddMockedDependencies()
            .WithCacheManager(cacheManager)
            .WithDefaultMarketListProviders()
            .Build();

        var marketCacheProvider = MarketCacheProviderBuilder.Create()
            .WithCacheManager(cacheManager)
            .WithDataRouterManager(dataRouterManager)
            .WithLanguages(TestData.Cultures1)
            .WithLoggerFactory(testLoggerFactory)
            .WithProfileCache(profileCache)
            .Build();

        _nameProvider = new NameProvider(
            marketCacheProvider,
            new Mock<IProfileCache>().Object,
            new Mock<INameExpressionFactory>().Object,
            new Mock<ISportEvent>().Object,
            41,
            specifiers,
            ExceptionHandlingStrategy.Throw,
            loggerFactory.CreateLogger<NameProvider>()
        );
    }

    [Theory]
    [InlineData("110", "1:1")]
    [InlineData("114", "2:1")]
    [InlineData("116", "3:1")]
    [InlineData("118", "4:1")]
    [InlineData("120", "5:1")]
    [InlineData("122", "6:1")]
    [InlineData("124", "7:1")]
    [InlineData("126", "1:2")]
    [InlineData("128", "2:2")]
    [InlineData("130", "3:2")]
    [InlineData("132", "4:2")]
    [InlineData("134", "5:2")]
    [InlineData("136", "6:2")]
    [InlineData("138", "1:3")]
    [InlineData("140", "2:3")]
    [InlineData("142", "3:3")]
    [InlineData("144", "4:3")]
    [InlineData("146", "5:3")]
    [InlineData("148", "1:4")]
    [InlineData("150", "2:4")]
    [InlineData("152", "3:4")]
    [InlineData("154", "4:4")]
    [InlineData("156", "1:5")]
    [InlineData("158", "2:5")]
    [InlineData("160", "3:5")]
    [InlineData("162", "1:6")]
    [InlineData("164", "2:6")]
    [InlineData("166", "1:7")]
    public async Task OutcomeNamesForFlexMarketAreCorrect(string outcomeId, string expected)
    {
        var result = await _nameProvider.GetOutcomeNameAsync(outcomeId, TestData.Culture);

        result.Should().Be(expected);
    }
}

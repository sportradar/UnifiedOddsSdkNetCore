// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.OutMarket;

public class OutcomeTests
{
    private readonly Mock<INameProvider> _mockNameProvider = new Mock<INameProvider>();
    private readonly Mock<IMarketMappingProvider> _mockMarketMappingProvider = new Mock<IMarketMappingProvider>();

    [Fact]
    public async Task GetNameWhenNameProviderReturnsValidNameThenReturnName()
    {
        const string expectedName = "some-name";
        NameProviderReturnsExpectedName(expectedName);
        var outcome = GetDefaultOutcome();

        var name = await outcome.GetNameAsync(ScheduleData.Cultures1.First());

        Assert.Equal(expectedName, name);
    }

    [Fact]
    public async Task GetNameWhenNameProviderThrowsThenThrow()
    {
        NameProviderThrows();
        var outcome = GetDefaultOutcome();

        await Assert.ThrowsAsync<CacheItemNotFoundException>(() => outcome.GetNameAsync(ScheduleData.Cultures1.First()));
    }

    [Fact]
    public async Task GetNameWhenRetryForValidNameThenReturnNameProviderIsCalledOnlyOnce()
    {
        const string expectedName = "some-name";
        NameProviderReturnsExpectedName(expectedName);
        var outcome = GetDefaultOutcome();

        var name1 = await outcome.GetNameAsync(ScheduleData.Cultures1.First());
        var name2 = await outcome.GetNameAsync(ScheduleData.Cultures1.First());

        Assert.Equal(expectedName, name1);
        Assert.Equal(expectedName, name2);
        _mockNameProvider.Verify(v => v.GetOutcomeNameAsync(outcome.Id, It.IsAny<CultureInfo>()), Times.Once);
    }

    [Fact]
    public async Task GetNameWhenAfterThrowRetryTrowsThenTwoCallAreMade()
    {
        NameProviderThrows();
        var outcome = GetDefaultOutcome();

        await Assert.ThrowsAsync<CacheItemNotFoundException>(() => outcome.GetNameAsync(ScheduleData.Cultures1.First()));
        await Assert.ThrowsAsync<CacheItemNotFoundException>(() => outcome.GetNameAsync(ScheduleData.Cultures1.First()));

        _mockNameProvider.Verify(v => v.GetOutcomeNameAsync(outcome.Id, It.IsAny<CultureInfo>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetNameWhenMarketNameIsNullThenReturnNull()
    {
        NameProviderReturnsExpectedName(null);
        var outcome = GetDefaultOutcome();

        var name = await outcome.GetNameAsync(ScheduleData.Cultures1.First());

        Assert.Null(name);
    }

    [Fact]
    public async Task GetNameWhenMarketNameIsNullThenOnRetryNameProviderIsCalledAgain()
    {
        NameProviderReturnsExpectedName(null);
        var outcome = GetDefaultOutcome();

        var name1 = await outcome.GetNameAsync(ScheduleData.Cultures1.First());
        var name2 = await outcome.GetNameAsync(ScheduleData.Cultures1.First());

        Assert.Null(name1);
        Assert.Null(name2);
        _mockNameProvider.Verify(v => v.GetOutcomeNameAsync(outcome.Id, It.IsAny<CultureInfo>()), Times.Exactly(2));
    }

    private IOutcome GetDefaultOutcome(string outcomeId = "12")
    {
        return new OutcomeOdds(outcomeId, true, 1.1, 0.456, _mockNameProvider.Object, _mockMarketMappingProvider.Object, ScheduleData.Cultures1, null, null);
    }

    private void NameProviderReturnsExpectedName(string name)
    {
        _mockNameProvider.Setup(s => s.GetMarketNameAsync(It.IsAny<CultureInfo>())).ReturnsAsync(name);
        _mockNameProvider.Setup(s => s.GetOutcomeNameAsync(It.IsAny<string>(), It.IsAny<CultureInfo>())).ReturnsAsync(name);
    }

    private void NameProviderThrows()
    {
        _mockNameProvider.Setup(s => s.GetMarketNameAsync(It.IsAny<CultureInfo>())).Throws<CacheItemNotFoundException>();
        _mockNameProvider.Setup(s => s.GetOutcomeNameAsync(It.IsAny<string>(), It.IsAny<CultureInfo>())).Throws<CacheItemNotFoundException>();
    }
}

// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock;
using Sportradar.OddsFeed.SDK.Tests.Common.TestUnits.EntityUnit;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities;

public class LotteryTests
{
    private readonly ITestOutputHelper _outputHelper;

    public LotteryTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task GetLotteriesAsyncWhenLotteriesEndpointReturnsLotteriesThenAllLotteriesAreReturned()
    {
        var apiLotteries = Wns.Lotteries();
        var sportDataProvider = SportEventUnitBuilder.Create()
                                                     .WithLoggerFactory(_outputHelper)
                                                     .WithConfiguration(UofConfigurations.SingleLanguage)
                                                     .WithLotteriesFetcherReturning(apiLotteries)
                                                     .Build()
                                                     .SportDataProvider;

        var lotteries = (await sportDataProvider.GetLotteriesAsync()).ToList();

        lotteries.Select(lottery => lottery.Id.ToString()).ShouldBe(apiLotteries.lottery.Select(lottery => lottery.id));
    }
}

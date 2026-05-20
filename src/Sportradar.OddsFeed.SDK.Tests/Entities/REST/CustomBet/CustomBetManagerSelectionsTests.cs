// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl.CustomBet;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.CustomBet;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl;
using Xunit;
using static Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.CustomBet.CalculationEndpoint;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.CustomBet;

public class CustomBetManagerSelectionsTests
{
    private const int AnyMarketId = 10;
    private readonly Mock<IDataPoster> _dataPosterMock;

    public CustomBetManagerSelectionsTests()
    {
        _dataPosterMock = new Mock<IDataPoster>();
    }

    [Fact]
    public async Task CalculateProbabilityWhenRequestIsNullThenThrowsArgumentNullException()
    {
        var manager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                                  .AddDefaultDependencies()
                                                                  .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                                  .WithDataPoster(_dataPosterMock.Object)
                                                                  .Build();

        var exception = await Should.ThrowAsync<ArgumentNullException>(
            () => manager.CalculateProbabilityAsync(null));

        exception.ParamName.ShouldBe("request");
    }

    [Fact]
    public async Task CalculateProbabilityWhenRequestWasNotCreatedByManagerThenThrowsArgumentException()
    {
        var manager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                                  .AddDefaultDependencies()
                                                                  .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                                  .WithDataPoster(_dataPosterMock.Object)
                                                                  .Build();
        var foreignBuilder = new FakeCalculateRequestBuilder();

        var exception = await Should.ThrowAsync<ArgumentException>(
            () => manager.CalculateProbabilityAsync(foreignBuilder));

        exception.ParamName.ShouldBe("request");
    }

    [Fact]
    public async Task CalculateProbabilityWhenRequestContainsOnlyAndSelectionsThenProbabilitiesAreReturned()
    {
        const int marketIdForAndSelection = 55;
        var expectedResponse = GetValidCalculationResponse();
        _dataPosterMock
           .Setup(s => s.PostDataAsync(
                                       It.Is<Uri>(uri => uri.ToString().EndsWith("/calculate")),
                                       It.Is<HttpContent>(content => content.ReadAsStringAsync().Result.Contains(marketIdForAndSelection.ToString()))))
           .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
           {
               Content = new StringContent(MsgSerializer.SerializeToXml(expectedResponse))
           });
        var manager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                                  .AddDefaultDependencies()
                                                                  .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                                  .WithDataPoster(_dataPosterMock.Object)
                                                                  .Build();

        var requestCombiningAndSelectionsWithOrSelection = BuildRequestContainingOnlyAndSelections(manager, marketIdForAndSelection);
        var result = await manager.CalculateProbabilityAsync(requestCombiningAndSelectionsWithOrSelection);

        result.ShouldNotBeNull();
        result.Odds.ShouldBe(expectedResponse.calculation.odds);
        result.Probability.ShouldBe(expectedResponse.calculation.probability);
    }

    [Fact]
    public async Task CalculateProbabilityWhenRequestContainsOnlyOrSelectionsThenProbabilitiesAreReturned()
    {
        const int marketIdForOrSelection = 65;
        var expectedResponse = GetValidCalculationResponse();
        _dataPosterMock
           .Setup(s => s.PostDataAsync(
                                       It.Is<Uri>(uri => uri.ToString().EndsWith("/calculate")),
                                       It.Is<HttpContent>(content => content.ReadAsStringAsync().Result.Contains(marketIdForOrSelection.ToString()))))
           .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
           {
               Content = new StringContent(MsgSerializer.SerializeToXml(expectedResponse))
           });
        var manager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                                  .AddDefaultDependencies()
                                                                  .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                                  .WithDataPoster(_dataPosterMock.Object)
                                                                  .Build();

        var requestContainingOnlyOrSelections = BuildRequestContainingOnlyOrSelections(manager, marketIdForOrSelection);
        var result = await manager.CalculateProbabilityAsync(requestContainingOnlyOrSelections);

        result.ShouldNotBeNull();
        result.Odds.ShouldBe(expectedResponse.calculation.odds);
        result.Probability.ShouldBe(expectedResponse.calculation.probability);
    }

    [Fact]
    public async Task CalculateProbabilityWhenRequestCombinesAndSelectionWithOrSelectionsThenProbabilitiesAreReturned()
    {
        const int marketIdForAndSelection = 10;
        const int marketIdForOrSelections = 20;
        var expectedResponse = GetValidCalculationResponse();
        _dataPosterMock
           .Setup(s => s.PostDataAsync(
                                       It.Is<Uri>(uri => uri.ToString().EndsWith("/calculate")),
                                       It.Is<HttpContent>(content => content.ReadAsStringAsync().Result.Contains(marketIdForAndSelection.ToString()) &&
                                                                     content.ReadAsStringAsync().Result.Contains(marketIdForOrSelections.ToString()))))
           .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
           {
               Content = new StringContent(MsgSerializer.SerializeToXml(expectedResponse))
           });
        var manager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                                  .AddDefaultDependencies()
                                                                  .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                                  .WithDataPoster(_dataPosterMock.Object)
                                                                  .Build();

        var requestCombiningAndSelectionsWithOrSelection = BuildRequestCombiningAndSelectionsWithOrSelection(manager, marketIdForAndSelection, marketIdForOrSelections);
        var result = await manager.CalculateProbabilityAsync(requestCombiningAndSelectionsWithOrSelection);

        result.ShouldNotBeNull();
        result.Odds.ShouldBe(expectedResponse.calculation.odds);
        result.Probability.ShouldBe(expectedResponse.calculation.probability);
    }

    [Fact]
    public async Task CalculateProbabilityWhenUnderlyingCallFailsAndStrategyIsCatchThenReturnsNull()
    {
        _dataPosterMock
           .Setup(s => s.PostDataAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>()))
           .ThrowsAsync(new CommunicationException("Not found", "/v1/custombet/calculate", HttpStatusCode.NotFound, null));
        var manager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                                  .AddDefaultDependencies()
                                                                  .WithConfigurationProvidingAnyBookmakerIdAndStrategyCatch()
                                                                  .WithDataPoster(_dataPosterMock.Object)
                                                                  .Build();

        var result = await manager.CalculateProbabilityAsync(BuildRequestContainingOnlyAndSelections(manager, AnyMarketId));

        result.ShouldBeNull();
    }

    [Fact]
    public async Task CalculateProbabilityWhenUnderlyingCallFailsAndStrategyIsThrowThenThrowsCommunicationException()
    {
        _dataPosterMock
           .Setup(s => s.PostDataAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>()))
           .ThrowsAsync(new CommunicationException("Not found", "/v1/custombet/calculate", HttpStatusCode.NotFound, null));
        var manager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                                  .AddDefaultDependencies()
                                                                  .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                                  .WithDataPoster(_dataPosterMock.Object)
                                                                  .Build();

        await Should.ThrowAsync<CommunicationException>(
            () => manager.CalculateProbabilityAsync(BuildRequestContainingOnlyAndSelections(manager, AnyMarketId)));
    }

    [Fact]
    public void AndSelectionWhenSelectionIsNullThenThrowsArgumentNullException()
    {
        var manager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                                  .AddDefaultDependencies()
                                                                  .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                                  .WithDataPoster(_dataPosterMock.Object)
                                                                  .Build();
        var builder = manager.GetCalculateRequestBuilder();

        var exception = Should.Throw<ArgumentNullException>(() => builder.AndSelection(null));

        exception.ParamName.ShouldBe("selection");
    }

    [Fact]
    public void AndAnyOfSelectionsWhenSelectionsIsNullThenThrowsArgumentException()
    {
        var manager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                                  .AddDefaultDependencies()
                                                                  .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                                  .WithDataPoster(_dataPosterMock.Object)
                                                                  .Build();
        var builder = manager.GetCalculateRequestBuilder();

        var exception = Should.Throw<ArgumentException>(() => builder.AndAnyOfSelections(null));

        exception.ParamName.ShouldBe("selections");
    }

    [Fact]
    public void AndAnyOfSelectionsWhenSelectionsIsEmptyThenThrowsArgumentException()
    {
        var manager = (ICustomBetManagerV2)CustomBetManagerBuilder.Create()
                                                                  .AddDefaultDependencies()
                                                                  .WithConfigurationProvidingAnyBookmakerIdAndStrategyThrow()
                                                                  .WithDataPoster(_dataPosterMock.Object)
                                                                  .Build();
        var builder = manager.GetCalculateRequestBuilder();

        var exception = Should.Throw<ArgumentException>(() => builder.AndAnyOfSelections());

        exception.ParamName.ShouldBe("selections");
    }

    private static ICalculateRequestBuilder BuildRequestContainingOnlyAndSelections(ICustomBetManagerV2 manager, int marketId)
    {
        return manager.GetCalculateRequestBuilder()
                      .AndSelection(new Selection(TestConsts.AnyMatchId, marketId, "9", null))
                      .AndSelection(new Selection(TestConsts.AnyMatchId, marketId, "74", null));
    }

    private static ICalculateRequestBuilder BuildRequestCombiningAndSelectionsWithOrSelection(ICustomBetManagerV2 manager, int marketId1, int marketId2)
    {
        return manager.GetCalculateRequestBuilder()
                      .AndSelection(new Selection(TestConsts.AnyMatchId, marketId1, "9", null))
                      .AndAnyOfSelections(
                                          new Selection(TestConsts.AnyMatchId, marketId2, "1", null),
                                          new Selection(TestConsts.AnyMatchId, marketId2, "2", null));
    }

    private static ICalculateRequestBuilder BuildRequestContainingOnlyOrSelections(ICustomBetManagerV2 manager, int marketId)
    {
        return manager.GetCalculateRequestBuilder()
                      .AndAnyOfSelections(
                                          new Selection(TestConsts.AnyMatchId, marketId, "1", null),
                                          new Selection(TestConsts.AnyMatchId, marketId, "2", null));
    }
}

// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Managers;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest;

public class CustomBetManagerTests
{
    private const string DefaultAvailableSelectionsXml = "available_selections.xml";
    private const string DefaultCalculationXml = "calculate_response.xml";
    private const string DefaultCalculationFilterXml = "calculate_filter_response.xml";
    private const string DefaultExceptionMessage = "Not found for id";
    private readonly ICustomBetManager _customBetManagerThrow;
    private readonly ICustomBetManager _customBetManagerCatch;
    private readonly Mock<IDataPoster> _dataPosterMock;
    private readonly Mock<IDataFetcher> _dataFetcherMock;

    public CustomBetManagerTests()
    {
        _dataPosterMock = new Mock<IDataPoster>();
        _dataFetcherMock = new Mock<IDataFetcher>();
        var uofConfigurationThrow = new Mock<IUofConfiguration>();
        uofConfigurationThrow.Setup(x => x.ExceptionHandlingStrategy).Returns(ExceptionHandlingStrategy.Throw);
        uofConfigurationThrow.Setup(x => x.DefaultLanguage).Returns(TestData.Culture);
        var uofConfigurationCatch = new Mock<IUofConfiguration>();
        uofConfigurationCatch.Setup(x => x.ExceptionHandlingStrategy).Returns(ExceptionHandlingStrategy.Catch);
        uofConfigurationCatch.Setup(x => x.DefaultLanguage).Returns(TestData.Culture);

        var availableSelectionsProvider =
            new DataProvider<AvailableSelectionsType, AvailableSelectionsDto>("/v1/custombet/available-selections", _dataFetcherMock.Object, new Deserializer<AvailableSelectionsType>(), new AvailableSelectionsMapperFactory());
        var calculateProbabilityProvider = new CalculateProbabilityProvider("/v1/custombet/calculate", _dataPosterMock.Object, new Deserializer<CalculationResponseType>(), new CalculationMapperFactory());
        var calculateFilterProbabilityProvider = new CalculateProbabilityFilteredProvider("/v1/custombet/calculate-filter", _dataPosterMock.Object, new Deserializer<FilteredCalculationResponseType>(), new CalculationFilteredMapperFactory());

        var productManagerMock = GetProductManagerMock();
        var dataRouterManagerThrow = new DataRouterManagerBuilder()
                                    .AddMockedDependencies()
                                    .WithConfiguration(uofConfigurationThrow.Object)
                                    .WithProducerManager(productManagerMock.Object)
                                    .WithAvailableSelectionsProvider(availableSelectionsProvider)
                                    .WithCalculateProbabilityProvider(calculateProbabilityProvider)
                                    .WithCalculateProbabilityFilteredProvider(calculateFilterProbabilityProvider)
                                    .Build();
        var dataRouterManagerCatch = new DataRouterManagerBuilder()
                                    .AddMockedDependencies()
                                    .WithConfiguration(uofConfigurationCatch.Object)
                                    .WithProducerManager(productManagerMock.Object)
                                    .WithAvailableSelectionsProvider(availableSelectionsProvider)
                                    .WithCalculateProbabilityProvider(calculateProbabilityProvider)
                                    .WithCalculateProbabilityFilteredProvider(calculateFilterProbabilityProvider)
                                    .Build();

        _customBetManagerThrow = new CustomBetManager(dataRouterManagerThrow, uofConfigurationThrow.Object, new CustomBetSelectionBuilderFactory());
        _customBetManagerCatch = new CustomBetManager(dataRouterManagerCatch, uofConfigurationCatch.Object, new CustomBetSelectionBuilderFactory());
    }

    [Fact]
    public void CustomBetManagerSetupCorrectly()
    {
        Assert.NotNull(_customBetManagerThrow);
        Assert.NotNull(_customBetManagerCatch);
    }

    [Fact]
    public void CustomBetSelectionBuilderWhenRequestTwoThenEachIsUnique()
    {
        var customBetSelectionBuilder1 = _customBetManagerThrow.CustomBetSelectionBuilder;
        var customBetSelectionBuilder2 = _customBetManagerThrow.CustomBetSelectionBuilder;

        Assert.NotNull(customBetSelectionBuilder1);
        Assert.NotNull(customBetSelectionBuilder2);
        Assert.NotEqual(customBetSelectionBuilder1, customBetSelectionBuilder2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(1d)]
    [InlineData(1.2345)]
    [InlineData(0.2345)]
    public void CustomBetSelectionBuilderWhenConstructedThenSelectionIsCreatedCorrectly(double? wantedOdds)
    {
        var cbSelectionBuilder = _customBetManagerThrow.CustomBetSelectionBuilder as ICustomBetSelectionBuilderV1;
        cbSelectionBuilder.Should().NotBeNull();
        cbSelectionBuilder.SetEventId(TestData.EventMatchId)
            .SetMarketId(1)
            .SetOutcomeId("123")
            .SetSpecifiers("value=1");
        if (wantedOdds.HasValue)
        {
            cbSelectionBuilder.SetOdds(wantedOdds.Value);
        }

        var selection = cbSelectionBuilder.Build();

        selection.Should().NotBeNull();
        selection.EventId.Should().Be(TestData.EventMatchId);
        selection.MarketId.Should().Be(1);
        selection.OutcomeId.Should().Be("123");
        selection.Specifiers.Should().Be("value=1");
        var selectionV1 = selection as ISelectionV1;
        selectionV1.Should().NotBeNull();
        selectionV1.Odds.Should().Be(wantedOdds);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(1d)]
    [InlineData(1.2345)]
    [InlineData(0.2345)]
    public void CustomBetSelectionBuilderWhenConstructedDirectlyThenSelectionIsCreatedCorrectly(double? wantedOdds)
    {
        var cbSelectionBuilder = _customBetManagerThrow.CustomBetSelectionBuilder as ICustomBetSelectionBuilderV1;
        cbSelectionBuilder.Should().NotBeNull();

        var selection = cbSelectionBuilder.Build(TestData.EventMatchId, 1, "value=1", "123", wantedOdds);

        selection.Should().NotBeNull();
        selection.EventId.Should().Be(TestData.EventMatchId);
        selection.MarketId.Should().Be(1);
        selection.OutcomeId.Should().Be("123");
        selection.Specifiers.Should().Be("value=1");
        var selectionV1 = selection as ISelectionV1;
        selectionV1.Should().NotBeNull();
        selectionV1.Odds.Should().Be(wantedOdds);
    }

    [Fact]
    public async Task AvailableSelectionReturnsMarkets()
    {
        _dataFetcherMock.Setup(s => s.GetDataAsync(It.IsAny<Uri>())).ReturnsAsync(FileHelper.GetResource(DefaultAvailableSelectionsXml));

        var availableSelections = await _customBetManagerThrow.GetAvailableSelectionsAsync(ScheduleData.MatchId);

        Assert.NotNull(availableSelections);
        Assert.Equal(Urn.Parse("sr:match:31561675"), availableSelections.Event);
        Assert.NotNull(availableSelections.Markets);
        Assert.NotEmpty(availableSelections.Markets);
    }

    [Fact]
    public async Task AvailableSelectionCallsCorrectEndpoint()
    {
        _dataFetcherMock.Setup(s => s.GetDataAsync(It.IsAny<Uri>())).ReturnsAsync(FileHelper.GetResource(DefaultAvailableSelectionsXml));

        await _customBetManagerThrow.GetAvailableSelectionsAsync(ScheduleData.MatchId);

        _dataFetcherMock.Verify(v => v.GetDataAsync(It.IsAny<Uri>()), Times.Once);
        _dataFetcherMock.Verify(v => v.GetDataAsync(It.Is<Uri>(uri => uri.ToString().EndsWith("/available-selections", StringComparison.Ordinal))), Times.Once);
    }

    [Fact]
    public async Task AvailableSelectionCallRespectExceptionHandlingStrategyCatch()
    {
        _dataFetcherMock.Setup(s => s.GetDataAsync(It.IsAny<Uri>())).ThrowsAsync(new CommunicationException(DefaultExceptionMessage, DefaultAvailableSelectionsXml, HttpStatusCode.NotFound, null));

        var availableSelections = await _customBetManagerCatch.GetAvailableSelectionsAsync(ScheduleData.MatchId);

        Assert.Null(availableSelections);
        _dataFetcherMock.Verify(v => v.GetDataAsync(It.IsAny<Uri>()), Times.Once);
    }

    [Fact]
    public async Task AvailableSelectionCallRespectExceptionHandlingStrategyThrow()
    {
        _dataFetcherMock.Setup(s => s.GetDataAsync(It.IsAny<Uri>())).ThrowsAsync(new CommunicationException(DefaultExceptionMessage, DefaultAvailableSelectionsXml, HttpStatusCode.NotFound, null));
        IAvailableSelections availableSelections = null;

        var exception = await Assert.ThrowsAsync<CommunicationException>(async () => availableSelections = await _customBetManagerThrow.GetAvailableSelectionsAsync(ScheduleData.MatchId));

        Assert.Null(availableSelections);
        Assert.NotNull(exception);
        Assert.Equal(HttpStatusCode.NotFound, exception.ResponseCode);
        Assert.Equal(DefaultExceptionMessage, exception.Message);
        Assert.Equal(DefaultAvailableSelectionsXml, exception.Url);
        _dataFetcherMock.Verify(v => v.GetDataAsync(It.IsAny<Uri>()), Times.Once);
    }

    [Fact]
    public async Task CalculateProbabilityReturnsCalculation()
    {
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
        httpResponseMessage.Content = new StringContent(FileHelper.GetFileContent(DefaultCalculationXml));
        _dataPosterMock.Setup(s => s.PostDataAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>())).ReturnsAsync(httpResponseMessage);

        var calculation = await _customBetManagerThrow.CalculateProbabilityAsync(GenerateAnySelections());

        Assert.NotNull(calculation);
        Assert.True(calculation.Odds > 0);
        Assert.True(calculation.Probability > 0);
        Assert.NotNull(calculation.AvailableSelections);
        Assert.NotEmpty(calculation.AvailableSelections);
    }

    [Fact]
    public async Task CalculateProbabilityCallsCorrectEndpoint()
    {
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
        httpResponseMessage.Content = new StringContent(FileHelper.GetFileContent(DefaultCalculationXml));
        _dataPosterMock.Setup(s => s.PostDataAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>())).ReturnsAsync(httpResponseMessage);

        await _customBetManagerThrow.CalculateProbabilityAsync(GenerateAnySelections());

        _dataPosterMock.Verify(v => v.PostDataAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>()), Times.Once);
        _dataPosterMock.Verify(v => v.PostDataAsync(It.Is<Uri>(uri => uri.ToString().EndsWith("/calculate", StringComparison.Ordinal)), It.IsAny<HttpContent>()), Times.Once);
    }

    [Fact]
    public async Task CalculateProbabilityWhenCalledWithOddsReturnHarmonizationIsTrue()
    {
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
        httpResponseMessage.Content = new StringContent(GetCapiCalculationResponse());
        _dataPosterMock.Setup(s => s.PostDataAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>())).ReturnsAsync(httpResponseMessage);

        var calculation = await _customBetManagerThrow.CalculateProbabilityAsync(GenerateAnySelectionsWithOdds());
        var calculationV1 = calculation as ICalculationV1;

        calculationV1.Should().NotBeNull();
        calculationV1.Harmonization.Should().BeTrue();
    }

    [Fact]
    public async Task CalculateProbabilityCallRespectExceptionHandlingStrategyCatch()
    {
        _dataPosterMock.Setup(s => s.PostDataAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>()))
                       .ThrowsAsync(new CommunicationException(DefaultExceptionMessage, DefaultCalculationXml, HttpStatusCode.NotFound, null));

        var calculation = await _customBetManagerCatch.CalculateProbabilityAsync(GenerateAnySelections());

        Assert.Null(calculation);
        _dataPosterMock.Verify(v => v.PostDataAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>()), Times.Once);
        _dataPosterMock.Verify(v => v.PostDataAsync(It.Is<Uri>(uri => uri.ToString().EndsWith("/calculate", StringComparison.Ordinal)), It.IsAny<HttpContent>()), Times.Once);
    }

    [Fact]
    public async Task CalculateProbabilityCallRespectExceptionHandlingStrategyThrow()
    {
        _dataPosterMock.Setup(s => s.PostDataAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>()))
                       .ThrowsAsync(new CommunicationException(DefaultExceptionMessage, DefaultCalculationXml, HttpStatusCode.NotFound, null));

        ICalculation calculation = null;
        var exception = await Assert.ThrowsAsync<CommunicationException>(async () => calculation = await _customBetManagerThrow.CalculateProbabilityAsync(GenerateAnySelections()));

        Assert.Null(calculation);
        Assert.NotNull(exception);
        Assert.Equal(HttpStatusCode.NotFound, exception.ResponseCode);
        Assert.Equal(DefaultExceptionMessage, exception.Message);
        Assert.Equal(DefaultCalculationXml, exception.Url);
        _dataPosterMock.Verify(v => v.PostDataAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>()), Times.Once);
        _dataPosterMock.Verify(v => v.PostDataAsync(It.Is<Uri>(uri => uri.ToString().EndsWith("/calculate", StringComparison.Ordinal)), It.IsAny<HttpContent>()), Times.Once);
    }

    [Fact]
    public async Task CalculateProbabilityWhenErrorResponseIsReturnedThenThrow()
    {
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
        httpResponseMessage.Content = new StringContent(GetNotFoundResponse());
        _dataPosterMock.Setup(s => s.PostDataAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>())).ReturnsAsync(httpResponseMessage);

        ICalculation calculation = null;
        var exception = await Assert.ThrowsAsync<CommunicationException>(async () => calculation = await _customBetManagerThrow.CalculateProbabilityAsync(GenerateAnySelections()));

        Assert.Null(calculation);
        Assert.NotNull(exception);
        Assert.Equal(HttpStatusCode.NotFound, exception.ResponseCode);
        exception.Message.Contains("Getting probability calculations failed with message", StringComparison.OrdinalIgnoreCase);
        exception.Message.Contains("No data for the event", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CalculateProbabilityFilterReturnsCalculation()
    {
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
        httpResponseMessage.Content = new StringContent(FileHelper.GetFileContent(DefaultCalculationFilterXml));
        _dataPosterMock.Setup(s => s.PostDataAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>())).ReturnsAsync(httpResponseMessage);

        var calculation = await _customBetManagerThrow.CalculateProbabilityFilterAsync(GenerateAnySelections());

        Assert.NotNull(calculation);
        Assert.True(calculation.Odds > 0);
        Assert.True(calculation.Probability > 0);
        Assert.NotNull(calculation.AvailableSelections);
        Assert.NotEmpty(calculation.AvailableSelections);
    }

    [Fact]
    public async Task CalculateProbabilityFilterCallsCorrectEndpoint()
    {
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
        httpResponseMessage.Content = new StringContent(FileHelper.GetFileContent(DefaultCalculationFilterXml));
        _dataPosterMock.Setup(s => s.PostDataAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>())).ReturnsAsync(httpResponseMessage);

        await _customBetManagerThrow.CalculateProbabilityFilterAsync(GenerateAnySelections());

        _dataPosterMock.Verify(v => v.PostDataAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>()), Times.Once);
        _dataPosterMock.Verify(v => v.PostDataAsync(It.Is<Uri>(uri => uri.ToString().EndsWith("/calculate-filter", StringComparison.Ordinal)), It.IsAny<HttpContent>()), Times.Once);
    }

    [Fact]
    public async Task CalculateProbabilityFilterWhenCalledWithOddsReturnHarmonizationIsTrue()
    {
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
        httpResponseMessage.Content = new StringContent(GetCapiCalculationFilterResponse());
        _dataPosterMock.Setup(s => s.PostDataAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>())).ReturnsAsync(httpResponseMessage);

        var calculation = await _customBetManagerThrow.CalculateProbabilityFilterAsync(GenerateAnySelectionsWithOdds());
        var calculationV1 = calculation as ICalculationFilterV1;

        calculationV1.Should().NotBeNull();
        calculationV1.Harmonization.Should().BeTrue();
    }

    [Fact]
    public async Task CalculateProbabilityFilterCallRespectExceptionHandlingStrategyCatch()
    {
        _dataPosterMock.Setup(s => s.PostDataAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>()))
                       .ThrowsAsync(new CommunicationException(DefaultExceptionMessage, DefaultCalculationFilterXml, HttpStatusCode.NotFound, null));

        var calculation = await _customBetManagerCatch.CalculateProbabilityFilterAsync(GenerateAnySelections());

        Assert.Null(calculation);
    }

    [Fact]
    public async Task CalculateProbabilityFilterCallRespectExceptionHandlingStrategyThrow()
    {
        _dataPosterMock.Setup(s => s.PostDataAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>()))
                       .ThrowsAsync(new CommunicationException(DefaultExceptionMessage, DefaultCalculationFilterXml, HttpStatusCode.NotFound, null));

        ICalculationFilter calculation = null;
        var exception = await Assert.ThrowsAsync<CommunicationException>(async () => calculation = await _customBetManagerThrow.CalculateProbabilityFilterAsync(GenerateAnySelections()));

        Assert.Null(calculation);
        Assert.NotNull(exception);
        Assert.Equal(HttpStatusCode.NotFound, exception.ResponseCode);
        Assert.Equal(DefaultExceptionMessage, exception.Message);
        Assert.Equal(DefaultCalculationFilterXml, exception.Url);
    }

    [Fact]
    public async Task CalculateProbabilityFilterWhenErrorResponseIsReturnedThenThrow()
    {
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
        httpResponseMessage.Content = new StringContent(GetNotFoundResponse());
        _dataPosterMock.Setup(s => s.PostDataAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>())).ReturnsAsync(httpResponseMessage);

        ICalculationFilter calculation = null;
        var exception = await Assert.ThrowsAsync<CommunicationException>(async () => calculation = await _customBetManagerThrow.CalculateProbabilityFilterAsync(GenerateAnySelections()));

        Assert.Null(calculation);
        Assert.NotNull(exception);
        Assert.Equal(HttpStatusCode.NotFound, exception.ResponseCode);
        exception.Message.Contains("Getting probability calculations failed with message", StringComparison.OrdinalIgnoreCase);
        exception.Message.Contains("No data for the event", StringComparison.OrdinalIgnoreCase);
    }

    private static List<ISelection> GenerateAnySelections()
    {
        return
            [
                new Selection(ScheduleData.MatchId, 10, "9", null),
                new Selection(ScheduleData.MatchId, 891, "74", null)
            ];
    }

    private static List<ISelection> GenerateAnySelectionsWithOdds()
    {
        return
            [
                new Selection(ScheduleData.MatchId, 10, "9", null, 1.234),
                new Selection(ScheduleData.MatchId, 891, "74", null, 4.321)
            ];
    }

    private static string GetCapiCalculationResponse()
    {
        var calculateEndpoint = CustomBetEndpoint.CalculateEndpoint.Create()
                                                 .WithEventId(TestData.EventMatchId)
                                                 .WithOdds(2.345678d)
                                                 .WithProbability(0.123456d)
                                                 .WithHarmonization(true);
        return MsgSerializer.SerializeToXml(calculateEndpoint.Build());
    }

    private static string GetCapiCalculationFilterResponse()
    {
        var calculateEndpoint = CustomBetEndpoint.CalculateFilterEndpoint.Create()
                                                 .WithEventId(TestData.EventMatchId)
                                                 .WithOdds(2.345678d)
                                                 .WithProbability(0.123456d)
                                                 .WithHarmonization(true);
        return MsgSerializer.SerializeToXml(calculateEndpoint.Build());
    }

    private static string GetNotFoundResponse()
    {
        var response = ErrorResponseEndpoint.Create()
                                            .WithResponseCode(response_code.NOT_FOUND)
                                            .WithMessage("No data for the event: sr:match:123:prematch")
                                            .Build();
        return MsgSerializer.SerializeToXml(response);
    }

    private static Mock<IProducerManager> GetProductManagerMock()
    {
        var productManagerMock = new Mock<IProducerManager>();
        var producerMock = new Mock<IProducer>();
        producerMock.Setup(m => m.IsAvailable).Returns(false);

        productManagerMock.Setup(m => m.GetProducer(It.IsAny<int>())).Returns(producerMock.Object);

        return productManagerMock;
    }
}

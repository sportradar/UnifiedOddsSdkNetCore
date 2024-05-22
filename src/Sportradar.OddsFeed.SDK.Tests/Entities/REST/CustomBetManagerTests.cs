// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Managers;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl.CustomBet;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest;

public class CustomBetManagerTests
{
    private readonly TestDataRouterManager _dataRouterManager;
    private readonly ICustomBetManager _customBetManagerThrow;
    private readonly ICustomBetManager _customBetManagerCatch;

    public CustomBetManagerTests(ITestOutputHelper outputHelper)
    {
        _dataRouterManager = new TestDataRouterManager(new CacheManager(), outputHelper);
        var customBetSelectionBuilder = new CustomBetSelectionBuilder();
        _customBetManagerThrow = new CustomBetManager(_dataRouterManager, customBetSelectionBuilder, TestConfiguration.GetConfig());
        _customBetManagerCatch = new CustomBetManager(_dataRouterManager, customBetSelectionBuilder, TestConfiguration.GetConfig(null, ExceptionHandlingStrategy.Catch));
    }

    [Fact]
    public void CustomBetManagerSetupCorrectly()
    {
        Assert.NotNull(_customBetManagerThrow);
        Assert.NotNull(_customBetManagerCatch);
    }

    [Fact]
    public async Task AvailableSelectionReturnsMarkets()
    {
        var availableSelections = await _customBetManagerThrow.GetAvailableSelectionsAsync(ScheduleData.MatchId);
        Assert.NotNull(availableSelections);
        Assert.Equal(ScheduleData.MatchId, availableSelections.Event);
        Assert.NotNull(availableSelections.Markets);
        Assert.NotEmpty(availableSelections.Markets);
    }

    [Fact]
    public async Task AvailableSelectionCallsCorrectEndpoint()
    {
        Assert.Empty(_dataRouterManager.RestMethodCalls);
        Assert.Empty(_dataRouterManager.RestUrlCalls);

        await _customBetManagerThrow.GetAvailableSelectionsAsync(ScheduleData.MatchId);

        Assert.Single(_dataRouterManager.RestMethodCalls);
        Assert.Equal(TestDataRouterManager.EndpointAvailableSelections, _dataRouterManager.RestMethodCalls.First().Key);
    }

    [Fact]
    public async Task AvailableSelectionCallRespectExceptionHandlingStrategyCatch()
    {
        const string xml = "available_selections.xml";
        _dataRouterManager.ExceptionHandlingStrategy = ExceptionHandlingStrategy.Catch;
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>(xml, new CommunicationException("Not found for id", xml, HttpStatusCode.NotFound, null)));

        var availableSelections = await _customBetManagerCatch.GetAvailableSelectionsAsync(ScheduleData.MatchId);
        Assert.Null(availableSelections);
        Assert.Single(_dataRouterManager.RestMethodCalls);
        Assert.Equal(TestDataRouterManager.EndpointAvailableSelections, _dataRouterManager.RestMethodCalls.First().Key);
    }

    [Fact]
    public async Task AvailableSelectionCallRespectExceptionHandlingStrategyThrow()
    {
        const string exceptionMessage = "Not found for id";
        const string xml = "available_selections.xml";
        _dataRouterManager.ExceptionHandlingStrategy = ExceptionHandlingStrategy.Throw;
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>(xml, new CommunicationException(exceptionMessage, xml, HttpStatusCode.NotFound, null)));

        IAvailableSelections availableSelections = null;
        var exception = await Assert.ThrowsAsync<CommunicationException>(async () => availableSelections = await _customBetManagerThrow.GetAvailableSelectionsAsync(ScheduleData.MatchId));
        Assert.Null(availableSelections);
        Assert.NotNull(exception);
        Assert.Equal(HttpStatusCode.NotFound, exception.ResponseCode);
        Assert.Equal(exceptionMessage, exception.Message);
        Assert.Equal(xml, exception.Url);
        Assert.Single(_dataRouterManager.RestMethodCalls);
        Assert.Equal(TestDataRouterManager.EndpointAvailableSelections, _dataRouterManager.RestMethodCalls.First().Key);
    }

    [Fact]
    public async Task CalculateProbabilityReturnsCalculation()
    {
        var selections = new List<ISelection> { new Selection(ScheduleData.MatchId, 10, "9", null), new Selection(ScheduleData.MatchId, 891, "74", null) };
        var calculation = await _customBetManagerThrow.CalculateProbabilityAsync(selections);
        Assert.NotNull(calculation);
        Assert.True(calculation.Odds > 0);
        Assert.True(calculation.Probability > 0);
        Assert.NotNull(calculation.AvailableSelections);
        Assert.NotEmpty(calculation.AvailableSelections);
    }

    [Fact]
    public async Task CalculateProbabilityCallsCorrectEndpoint()
    {
        Assert.Empty(_dataRouterManager.RestMethodCalls);
        Assert.Empty(_dataRouterManager.RestUrlCalls);

        var selections = new List<ISelection> { new Selection(ScheduleData.MatchId, 10, "9", null), new Selection(ScheduleData.MatchId, 891, "74", null) };
        await _customBetManagerThrow.CalculateProbabilityAsync(selections);

        Assert.Single(_dataRouterManager.RestMethodCalls);
        Assert.Equal(TestDataRouterManager.EndpointCalculateProbability, _dataRouterManager.RestMethodCalls.First().Key);
    }

    [Fact]
    public async Task CalculateProbabilityCallRespectExceptionHandlingStrategyCatch()
    {
        const string xml = "calculate_response.xml";
        _dataRouterManager.ExceptionHandlingStrategy = ExceptionHandlingStrategy.Catch;
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>(xml, new CommunicationException("Not found for id", xml, HttpStatusCode.NotFound, null)));

        var selections = new List<ISelection> { new Selection(ScheduleData.MatchId, 10, "9", null), new Selection(ScheduleData.MatchId, 891, "74", null) };
        var calculation = await _customBetManagerCatch.CalculateProbabilityAsync(selections);
        Assert.Null(calculation);
        Assert.Single(_dataRouterManager.RestMethodCalls);
        Assert.Equal(TestDataRouterManager.EndpointCalculateProbability, _dataRouterManager.RestMethodCalls.First().Key);
    }

    [Fact]
    public async Task CalculateProbabilityCallRespectExceptionHandlingStrategyThrow()
    {
        const string exceptionMessage = "Not found for id";
        const string xml = "calculate_response.xml";
        _dataRouterManager.ExceptionHandlingStrategy = ExceptionHandlingStrategy.Throw;
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>(xml, new CommunicationException(exceptionMessage, xml, HttpStatusCode.NotFound, null)));

        ICalculation calculation = null;
        var selections = new List<ISelection> { new Selection(ScheduleData.MatchId, 10, "9", null), new Selection(ScheduleData.MatchId, 891, "74", null) };
        var exception = await Assert.ThrowsAsync<CommunicationException>(async () => calculation = await _customBetManagerThrow.CalculateProbabilityAsync(selections));
        Assert.Null(calculation);
        Assert.NotNull(exception);
        Assert.Equal(HttpStatusCode.NotFound, exception.ResponseCode);
        Assert.Equal(exceptionMessage, exception.Message);
        Assert.Equal(xml, exception.Url);
        Assert.Single(_dataRouterManager.RestMethodCalls);
        Assert.Equal(TestDataRouterManager.EndpointCalculateProbability, _dataRouterManager.RestMethodCalls.First().Key);
    }

    [Fact]
    public async Task CalculateProbabilityFilterReturnsCalculation()
    {
        var selections = new List<ISelection> { new Selection(ScheduleData.MatchId, 10, "9", null), new Selection(ScheduleData.MatchId, 891, "74", null) };
        var calculation = await _customBetManagerThrow.CalculateProbabilityFilterAsync(selections);
        Assert.NotNull(calculation);
        Assert.True(calculation.Odds > 0);
        Assert.True(calculation.Probability > 0);
        Assert.NotNull(calculation.AvailableSelections);
        Assert.NotEmpty(calculation.AvailableSelections);
    }

    [Fact]
    public async Task CalculateProbabilityFilterCallsCorrectEndpoint()
    {
        Assert.Empty(_dataRouterManager.RestMethodCalls);
        Assert.Empty(_dataRouterManager.RestUrlCalls);

        var selections = new List<ISelection> { new Selection(ScheduleData.MatchId, 10, "9", null), new Selection(ScheduleData.MatchId, 891, "74", null) };
        await _customBetManagerThrow.CalculateProbabilityFilterAsync(selections);

        Assert.Single(_dataRouterManager.RestMethodCalls);
        Assert.Equal(TestDataRouterManager.EndpointCalculateProbabilityFiltered, _dataRouterManager.RestMethodCalls.First().Key);
    }

    [Fact]
    public async Task CalculateProbabilityFilterCallRespectExceptionHandlingStrategyCatch()
    {
        const string xml = "calculate_filter_response.xml";
        _dataRouterManager.ExceptionHandlingStrategy = ExceptionHandlingStrategy.Catch;
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>(xml, new CommunicationException("Not found for id", xml, HttpStatusCode.NotFound, null)));

        var selections = new List<ISelection> { new Selection(ScheduleData.MatchId, 10, "9", null), new Selection(ScheduleData.MatchId, 891, "74", null) };
        var calculation = await _customBetManagerCatch.CalculateProbabilityFilterAsync(selections);
        Assert.Null(calculation);
        Assert.Single(_dataRouterManager.RestMethodCalls);
        Assert.Equal(TestDataRouterManager.EndpointCalculateProbabilityFiltered, _dataRouterManager.RestMethodCalls.First().Key);
    }

    [Fact]
    public async Task CalculateProbabilityFilterCallRespectExceptionHandlingStrategyThrow()
    {
        const string exceptionMessage = "Not found for id";
        const string xml = "calculate_filter_response.xml";
        _dataRouterManager.ExceptionHandlingStrategy = ExceptionHandlingStrategy.Throw;
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>(xml, new CommunicationException(exceptionMessage, xml, HttpStatusCode.NotFound, null)));

        ICalculationFilter calculation = null;
        var selections = new List<ISelection> { new Selection(ScheduleData.MatchId, 10, "9", null), new Selection(ScheduleData.MatchId, 891, "74", null) };
        var exception = await Assert.ThrowsAsync<CommunicationException>(async () => calculation = await _customBetManagerThrow.CalculateProbabilityFilterAsync(selections));
        Assert.Null(calculation);
        Assert.NotNull(exception);
        Assert.Equal(HttpStatusCode.NotFound, exception.ResponseCode);
        Assert.Equal(exceptionMessage, exception.Message);
        Assert.Equal(xml, exception.Url);
        Assert.Single(_dataRouterManager.RestMethodCalls);
        Assert.Equal(TestDataRouterManager.EndpointCalculateProbabilityFiltered, _dataRouterManager.RestMethodCalls.First().Key);
    }
}

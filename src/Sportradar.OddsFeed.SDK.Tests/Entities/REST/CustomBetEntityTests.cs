// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Managers;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest;

[SuppressMessage("Usage", "xUnit1031:Do not use blocking task operations in test method")]
public class CustomBetEntityTests
{
    private readonly IDataRouterManager _dataRouterManager;
    private readonly ICustomBetSelectionBuilder _customBetSelectionBuilder;

    public CustomBetEntityTests(ITestOutputHelper outputHelper)
    {
        _dataRouterManager = new TestDataRouterManager(new CacheManager(), outputHelper);
        _customBetSelectionBuilder = new CustomBetSelectionBuilder();
    }

    [Fact]
    public void AvailableSelectionsMap()
    {
        var availableSelectionsType = MessageFactoryRest.GetAvailableSelections(Urn.Parse("sr:match:1000"));
        var resultAvailableSelections = MessageFactorySdk.GetAvailableSelections(availableSelectionsType);

        AvailableSelectionsCompare(availableSelectionsType, resultAvailableSelections);
    }

    [Fact]
    public void AvailableSelectionsEmptyMap()
    {
        var availableSelectionsType = MessageFactoryRest.GetAvailableSelections(Urn.Parse("sr:match:1000"), 0);
        Assert.True(availableSelectionsType.@event.markets.IsNullOrEmpty());
        var resultAvailableSelections = MessageFactorySdk.GetAvailableSelections(availableSelectionsType);
        Assert.True(resultAvailableSelections.Markets.IsNullOrEmpty());

        AvailableSelectionsCompare(availableSelectionsType, resultAvailableSelections);
    }

    [Fact]
    public void CalculationEmptyMap()
    {
        var calculationResponseType = MessageFactoryRest.GetCalculationResponse(Urn.Parse("sr:match:1000"), 0);
        var calculation = MessageFactorySdk.GetCalculation(calculationResponseType);

        Assert.NotNull(calculationResponseType);
        Assert.NotNull(calculation);
        Assert.True(calculationResponseType.available_selections.IsNullOrEmpty());
        Assert.True(calculation.AvailableSelections.IsNullOrEmpty());

        Assert.Equal(calculationResponseType.calculation.odds, calculation.Odds);
        Assert.Equal(calculationResponseType.calculation.probability, calculation.Probability);
        if (calculationResponseType.generated_at.IsNullOrEmpty())
        {
            Assert.Null(calculation.GeneratedAt);
        }
        else
        {
            Assert.Equal(SdkInfo.ParseDate(calculationResponseType.generated_at), calculation.GeneratedAt);
        }
    }

    [Fact]
    public void CalculationMap()
    {
        var calculationResponseType = MessageFactoryRest.GetCalculationResponse(Urn.Parse("sr:match:1000"), 7);
        var calculation = MessageFactorySdk.GetCalculation(calculationResponseType);

        Assert.NotNull(calculationResponseType);
        Assert.NotNull(calculation);

        Assert.Equal(calculationResponseType.calculation.odds, calculation.Odds);
        Assert.Equal(calculationResponseType.calculation.probability, calculation.Probability);
        if (calculationResponseType.generated_at.IsNullOrEmpty())
        {
            Assert.Null(calculation.GeneratedAt);
        }
        else
        {
            Assert.Equal(SdkInfo.ParseDate(calculationResponseType.generated_at), calculation.GeneratedAt);
        }

        Assert.Equal(calculationResponseType.available_selections.Length, calculation.AvailableSelections.Count());
        for (var i = 0; i < calculationResponseType.available_selections.Length; i++)
        {
            var sourceAvailableSelection = calculationResponseType.available_selections[i];
            var resultAvailableSelection = calculation.AvailableSelections.ElementAt(i);

            Assert.Equal(Urn.Parse(sourceAvailableSelection.id), resultAvailableSelection.Event);
            foreach (var sourceMarket in sourceAvailableSelection.markets)
            {
                var resultMarket = resultAvailableSelection.Markets.First(f => f.Id == sourceMarket.id);
                Assert.NotNull(resultMarket);
                MarketCompare(sourceMarket, resultMarket);
            }
        }
        var marketCount = calculation.AvailableSelections.SelectMany(s => s.Markets).Count();
        var outcomeCount = calculation.AvailableSelections.SelectMany(s => s.Markets).SelectMany(o => o.Outcomes).Count();
        Trace.WriteLine($"Calculation has {marketCount} markets and {outcomeCount} outcomes.");
    }

    [Fact]
    public void CalculationFilterEmptyMap()
    {
        var calculationResponseType = MessageFactoryRest.GetFilteredCalculationResponse(Urn.Parse("sr:match:1000"), 0);
        var calculation = MessageFactorySdk.GetCalculationFilter(calculationResponseType);

        Assert.NotNull(calculationResponseType);
        Assert.NotNull(calculation);
        Assert.True(calculationResponseType.available_selections.IsNullOrEmpty());
        Assert.True(calculation.AvailableSelections.IsNullOrEmpty());

        Assert.Equal(calculationResponseType.calculation.odds, calculation.Odds);
        Assert.Equal(calculationResponseType.calculation.probability, calculation.Probability);
        if (calculationResponseType.generated_at.IsNullOrEmpty())
        {
            Assert.Null(calculation.GeneratedAt);
        }
        else
        {
            Assert.Equal(SdkInfo.ParseDate(calculationResponseType.generated_at), calculation.GeneratedAt);
        }
        var marketCount = calculation.AvailableSelections.SelectMany(s => s.Markets).Count();
        var outcomeCount = calculation.AvailableSelections.SelectMany(s => s.Markets).SelectMany(o => o.Outcomes).Count();
        Trace.WriteLine($"Calculation has {marketCount} markets and {outcomeCount} outcomes.");
    }

    [Fact]
    public void CalculationFilterMap()
    {
        var calculationResponseType = MessageFactoryRest.GetFilteredCalculationResponse(Urn.Parse("sr:match:1000"), 7);
        var calculation = MessageFactorySdk.GetCalculationFilter(calculationResponseType);

        Assert.NotNull(calculationResponseType);
        Assert.NotNull(calculation);

        Assert.Equal(calculationResponseType.calculation.odds, calculation.Odds);
        Assert.Equal(calculationResponseType.calculation.probability, calculation.Probability);
        if (calculationResponseType.generated_at.IsNullOrEmpty())
        {
            Assert.Null(calculation.GeneratedAt);
        }
        else
        {
            Assert.Equal(SdkInfo.ParseDate(calculationResponseType.generated_at), calculation.GeneratedAt);
        }

        Assert.Equal(calculationResponseType.available_selections.Length, calculation.AvailableSelections.Count());
        for (var i = 0; i < calculationResponseType.available_selections.Length; i++)
        {
            var sourceAvailableSelection = calculationResponseType.available_selections[i];
            var resultAvailableSelection = calculation.AvailableSelections.ElementAt(i);

            Assert.Equal(Urn.Parse(sourceAvailableSelection.id), resultAvailableSelection.Event);
            foreach (var sourceMarket in sourceAvailableSelection.markets)
            {
                var resultMarket = resultAvailableSelection.Markets.First(f => f.Id == sourceMarket.id);
                Assert.NotNull(resultMarket);
                MarketCompare(sourceMarket, resultMarket);
            }
        }
        var marketCount = calculation.AvailableSelections.SelectMany(s => s.Markets).Count();
        var outcomeCount = calculation.AvailableSelections.SelectMany(s => s.Markets).SelectMany(o => o.Outcomes).Count();
        Trace.WriteLine($"Calculation has {marketCount} markets and {outcomeCount} outcomes.");
    }

    [Fact]
    public void GetAvailableSelections()
    {
        var eventId = Urn.Parse("sr:match:31561675");
        var resultAvailableSelections = _dataRouterManager.GetAvailableSelectionsAsync(eventId).GetAwaiter().GetResult();

        Assert.NotNull(resultAvailableSelections);
        Assert.Equal(eventId, resultAvailableSelections.Event);
        Assert.False(resultAvailableSelections.Markets.IsNullOrEmpty());
    }

    [Fact]
    public void GetCalculation()
    {
        var eventId = Urn.Parse("sr:match:31561675");
        var availableSelections = _dataRouterManager.GetAvailableSelectionsAsync(eventId).GetAwaiter().GetResult();
        Assert.NotNull(availableSelections);

        var matchSelections = new List<ISelection>();
        var market = availableSelections.Markets.First();
        var selection = _customBetSelectionBuilder.SetEventId(eventId).SetMarketId(market.Id).SetOutcomeId(market.Outcomes.First()).SetSpecifiers(market.Specifiers).Build();
        matchSelections.Add(selection);
        market = availableSelections.Markets.Last();
        selection = _customBetSelectionBuilder.SetEventId(eventId).SetMarketId(market.Id).SetOutcomeId(market.Outcomes.First()).SetSpecifiers(market.Specifiers).Build();
        matchSelections.Add(selection);

        var calculation = _dataRouterManager.CalculateProbabilityAsync(matchSelections).GetAwaiter().GetResult();

        Assert.NotNull(calculation);
        Assert.Equal(eventId, calculation.AvailableSelections.First().Event);
        Assert.False(calculation.AvailableSelections.IsNullOrEmpty());
    }

    [Fact]
    public void GetCalculationFilter()
    {
        var eventId = Urn.Parse("sr:match:31561675");
        var availableSelections = _dataRouterManager.GetAvailableSelectionsAsync(eventId).GetAwaiter().GetResult();
        Assert.NotNull(availableSelections);

        var matchSelections = new List<ISelection>();
        var market = availableSelections.Markets.First();
        var selection = _customBetSelectionBuilder.SetEventId(eventId).SetMarketId(market.Id).SetOutcomeId(market.Outcomes.First()).SetSpecifiers(market.Specifiers).Build();
        matchSelections.Add(selection);
        market = availableSelections.Markets.Last();
        selection = _customBetSelectionBuilder.SetEventId(eventId).SetMarketId(market.Id).SetOutcomeId(market.Outcomes.First()).SetSpecifiers(market.Specifiers).Build();
        matchSelections.Add(selection);

        var calculation = _dataRouterManager.CalculateProbabilityFilteredAsync(matchSelections).GetAwaiter().GetResult();

        Assert.NotNull(calculation);
        Assert.Equal(eventId, calculation.AvailableSelections.First().Event);
        Assert.False(calculation.AvailableSelections.IsNullOrEmpty());
    }

    private void AvailableSelectionsCompare(AvailableSelectionsType source, IAvailableSelections result)
    {
        Assert.NotNull(source);
        Assert.NotNull(result);

        Assert.NotNull(source.@event);
        Assert.False(string.IsNullOrEmpty(source.generated_at));

        Assert.Equal(source.@event.id, result.Event.ToString());

        if (source.@event.markets.IsNullOrEmpty())
        {
            Assert.True(result.Markets.IsNullOrEmpty());
            return;
        }

        Assert.Equal(source.@event.markets.Length, result.Markets.Count());
        foreach (var sourceMarket in source.@event.markets)
        {
            var resultMarket = result.Markets.First(f => f.Id == sourceMarket.id);
            Assert.NotNull(resultMarket);
            MarketCompare(sourceMarket, resultMarket);
        }
    }

    private void MarketCompare(MarketType source, IMarket result)
    {
        Assert.NotNull(source);
        Assert.NotNull(result);

        Assert.Equal(source.id, result.Id);
        Assert.Equal(source.specifiers, result.Specifiers);

        if (source.outcome.IsNullOrEmpty())
        {
            Assert.True(result.Outcomes.IsNullOrEmpty());
            return;
        }

        Assert.Equal(source.outcome.Length, result.Outcomes.Count());
        foreach (var outcomeType in source.outcome)
        {
            Assert.Contains(outcomeType.id, result.Outcomes);
        }
    }

    private void MarketCompare(FilteredMarketType source, IMarketFilter result)
    {
        Assert.NotNull(source);
        Assert.NotNull(result);

        Assert.Equal(source.id, result.Id);
        Assert.Equal(source.specifiers, result.Specifiers);

        if (source.outcome.IsNullOrEmpty())
        {
            Assert.True(result.Outcomes.IsNullOrEmpty());
            return;
        }

        Assert.Equal(source.outcome.Length, result.Outcomes.Count());
        foreach (var sourceOutcome in source.outcome)
        {
            var resultOutcome = result.Outcomes.First(f => f.Id == sourceOutcome.id);
            Assert.NotNull(resultOutcome);
            Assert.Equal(sourceOutcome.id, resultOutcome.Id);
            if (sourceOutcome.conflictSpecified)
            {
                Assert.Equal(sourceOutcome.conflict, resultOutcome.IsConflict);
            }
            else
            {
                Assert.Null(resultOutcome.IsConflict);
            }
        }
    }
}

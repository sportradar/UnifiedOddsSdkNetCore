// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Managers;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl.CustomBet;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest;

public class CustomBetEntityTests
{
    private readonly IDataRouterManager _dataRouterManager;
    private readonly ICustomBetSelectionBuilder _customBetSelectionBuilder;

    // ReSharper disable once ConvertToPrimaryConstructor
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
    public void AvailableSelectionsFilterMap()
    {
        var availableFilteredEventType = MessageFactoryRest.GetFilteredEventType(Urn.Parse("sr:match:1000"));
        var resultAvailableSelectionsFilter = new AvailableSelectionsFilter(new FilteredAvailableSelectionsDto(availableFilteredEventType));

        AvailableSelectionsFilterCompare(availableFilteredEventType, resultAvailableSelectionsFilter);
    }

    [Fact]
    public void AvailableSelectionsFilterEmptyMap()
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

            Assert.Equal(Urn.Parse(sourceAvailableSelection.id), resultAvailableSelection.EventId);
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

            Assert.Equal(Urn.Parse(sourceAvailableSelection.id), resultAvailableSelection.EventId);
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
    public async Task GetAvailableSelections()
    {
        var eventId = Urn.Parse("sr:match:31561675");
        var resultAvailableSelections = await _dataRouterManager.GetAvailableSelectionsAsync(eventId);

        Assert.NotNull(resultAvailableSelections);
        Assert.Equal(eventId, resultAvailableSelections.EventId);
        Assert.False(resultAvailableSelections.Markets.IsNullOrEmpty());
    }

    [Fact]
    public async Task GetCalculation()
    {
        var eventId = Urn.Parse("sr:match:31561675");
        var availableSelections = await _dataRouterManager.GetAvailableSelectionsAsync(eventId);
        Assert.NotNull(availableSelections);

        var matchSelections = new List<ISelection>();
        var market = availableSelections.Markets.First();
        var selection = _customBetSelectionBuilder.SetEventId(eventId).SetMarketId(market.Id).SetOutcomeId(market.Outcomes.First()).SetSpecifiers(market.Specifiers).Build();
        matchSelections.Add(selection);
        market = availableSelections.Markets.Last();
        selection = _customBetSelectionBuilder.SetEventId(eventId).SetMarketId(market.Id).SetOutcomeId(market.Outcomes.First()).SetSpecifiers(market.Specifiers).Build();
        matchSelections.Add(selection);

        var calculation = await _dataRouterManager.CalculateProbabilityAsync(matchSelections);

        Assert.NotNull(calculation);
        Assert.Equal(eventId, calculation.AvailableSelections.First().EventId);
        Assert.False(calculation.AvailableSelections.IsNullOrEmpty());
    }

    [Fact]
    public async Task GetCalculationFilter()
    {
        var eventId = Urn.Parse("sr:match:31561675");
        var availableSelections = await _dataRouterManager.GetAvailableSelectionsAsync(eventId);
        Assert.NotNull(availableSelections);

        var matchSelections = new List<ISelection>();
        var market = availableSelections.Markets.First();
        var selection = _customBetSelectionBuilder.SetEventId(eventId).SetMarketId(market.Id).SetOutcomeId(market.Outcomes.First()).SetSpecifiers(market.Specifiers).Build();
        matchSelections.Add(selection);
        market = availableSelections.Markets.Last();
        selection = _customBetSelectionBuilder.SetEventId(eventId).SetMarketId(market.Id).SetOutcomeId(market.Outcomes.First()).SetSpecifiers(market.Specifiers).Build();
        matchSelections.Add(selection);

        var calculation = await _dataRouterManager.CalculateProbabilityFilteredAsync(matchSelections);

        Assert.NotNull(calculation);
        Assert.Equal(eventId, calculation.AvailableSelections.First().EventId);
        Assert.False(calculation.AvailableSelections.IsNullOrEmpty());
    }

    [Theory]
    [InlineData(null)]
    [InlineData(true)]
    [InlineData(false)]
    public void CalculationWhenHarmonizationSetThenMapCorrectly(bool? harmonizationValue)
    {
        var calculationResponseType = MessageFactoryRest.GetFilteredCalculationResponse(Urn.Parse("sr:match:1000"), 7);
        if (harmonizationValue == null)
        {
            calculationResponseType.calculation.harmonizationSpecified = false;
        }
        else
        {
            calculationResponseType.calculation.harmonization = (bool)harmonizationValue;
            calculationResponseType.calculation.harmonizationSpecified = true;
        }

        var calculation = MessageFactorySdk.GetCalculationFilter(calculationResponseType);

        calculation.Should().NotBeNull();
        calculation.Harmonization.Should().Be(harmonizationValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(true)]
    [InlineData(false)]
    public void CalculationFilterWhenHarmonizationSetThenMapCorrectly(bool? harmonizationValue)
    {
        var calculationResponseType = MessageFactoryRest.GetCalculationResponse(Urn.Parse("sr:match:1000"), 7);
        if (harmonizationValue == null)
        {
            calculationResponseType.calculation.harmonizationSpecified = false;
        }
        else
        {
            calculationResponseType.calculation.harmonization = (bool)harmonizationValue;
            calculationResponseType.calculation.harmonizationSpecified = true;
        }

        var calculation = MessageFactorySdk.GetCalculation(calculationResponseType);

        calculation.Should().BeAssignableTo<ICalculation>();
        calculation.Should().NotBeNull();
        calculation.Harmonization.Should().Be(harmonizationValue);
    }

    [Fact]
    public void AvailableSelectionsDtoWhenInputNullThenThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new AvailableSelectionsDto(null));
    }

    [Fact]
    public void FilteredAvailableSelectionsDtoWhenInputNullThenThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new FilteredAvailableSelectionsDto(null));
    }

    [Fact]
    public void CalculationDtoWhenInputNullThenThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new CalculationDto(null));
    }

    [Fact]
    public void FilteredCalculationDtoWhenInputNullThenThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new FilteredCalculationDto(null));
    }

    [Fact]
    public void MarketDtoWhenInputNullThenThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new MarketDto(null));
    }

    [Fact]
    public void FilteredMarketDtoWhenInputNullThenThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new FilteredMarketDto(null));
    }

    [Fact]
    public void FilteredOutcomeDtoWhenInputNullThenThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new FilteredOutcomeDto(null));
    }

    [Fact]
    public void AvailableSelectionsWhenInputNullThenThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new AvailableSelections(null));
    }

    [Fact]
    public void AvailableSelectionsFilterWhenInputNullThenThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new AvailableSelectionsFilter(null));
    }

    [Fact]
    public void CalculationWhenInputNullThenThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new Calculation(null));
    }

    [Fact]
    public void FilteredCalculationWhenInputNullThenThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new CalculationFilter(null));
    }

    [Fact]
    public void SelectionConstructionWhenEventIdMissingThenThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new Selection(null, 1, "123", "value=1", 1.234));
    }

    [Fact]
    public void SelectionConstructionWhenMarketIdZeroThenThrow()
    {
        Assert.Throws<ArgumentException>(() => new Selection(TestData.EventMatchId, 0, "123", "value=1", 1.234));
    }

    [Fact]
    public void SelectionConstructionWhenOutcomeIdIdMissingThenThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new Selection(TestData.EventMatchId, 1, null, "value=1", 1.234));
    }

    [Fact]
    public void SelectionConstructionWhenSpecifiersMissingThenOk()
    {
        var selection = new Selection(TestData.EventMatchId, 1, "123", null, 1.234);

        selection.Should().NotBeNull();
        selection.Specifiers.Should().BeNull();
    }

    [Fact]
    public void SelectionConstructionWhenOddsMissingThenOk()
    {
        var selection = new Selection(TestData.EventMatchId, 1, "123", null);

        selection.Should().NotBeNull();
        selection.Odds.Should().BeNull();
    }

    [Fact]
    public void SelectionConstructionWhenAllFieldsSetThenPopulateAll()
    {
        var selection = new Selection(TestData.EventMatchId, 111, "123", "value=1", 1.234);

        selection.EventId.Should().Be(TestData.EventMatchId);
        selection.MarketId.Should().Be(111);
        selection.OutcomeId.Should().Be("123");
        selection.Specifiers.Should().Be("value=1");
        selection.Odds.Should().Be(1.234);
    }

    [Fact]
    public void SelectionConstructionWhenOddsNotSetThenOddsNull()
    {
        var selection = new Selection(TestData.EventMatchId, 111, "123", "value=1");

        selection.Odds.Should().BeNull();
    }

    private static void AvailableSelectionsCompare(AvailableSelectionsType source, IAvailableSelections result)
    {
        Assert.NotNull(source);
        Assert.NotNull(result);

        Assert.NotNull(source.@event);
        Assert.False(string.IsNullOrEmpty(source.generated_at));

        Assert.Equal(source.@event.id, result.EventId.ToString());

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

    private static void AvailableSelectionsFilterCompare(FilteredEventType source, IAvailableSelectionsFilter result)
    {
        Assert.NotNull(source);
        Assert.NotNull(result);

        Assert.NotNull(source.id);
        Assert.Equal(source.id, result.EventId.ToString());

        if (source.markets.IsNullOrEmpty())
        {
            Assert.True(result.Markets.IsNullOrEmpty());
            return;
        }

        Assert.Equal(source.markets.Length, result.Markets.Count());
        foreach (var sourceMarket in source.markets)
        {
            var resultMarket = result.Markets.First(f => f.Id == sourceMarket.id);
            Assert.NotNull(resultMarket);
            MarketCompare(sourceMarket, resultMarket);
        }
    }

    private static void MarketCompare(MarketType source, IMarket result)
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

    private static void MarketCompare(FilteredMarketType source, IMarketFilter result)
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

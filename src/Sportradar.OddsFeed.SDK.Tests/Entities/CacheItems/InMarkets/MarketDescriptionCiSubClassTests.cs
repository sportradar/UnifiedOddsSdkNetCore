// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InMarkets;

public class MarketDescriptionCiSubClassTests
{
    private readonly desc_outcomesOutcome _apiDescOutcomes;
    private readonly OutcomeDescriptionDto _dtoOutcomeDescription;
    private readonly MarketOutcomeCacheItem _ciMarketOutcome;

    public MarketDescriptionCiSubClassTests()
    {
        _apiDescOutcomes = new desc_outcomesOutcome { id = "some-id", description = "some-description-en", name = "some-name-en" };
        _dtoOutcomeDescription = new OutcomeDescriptionDto(_apiDescOutcomes);
        _ciMarketOutcome = new MarketOutcomeCacheItem(_dtoOutcomeDescription, ScheduleData.CultureEn);
    }

    [Fact]
    public void MarketOutcomeConstructWhenDtoValid()
    {
        Assert.NotNull(_ciMarketOutcome);
        Assert.Equal(_apiDescOutcomes.id, _ciMarketOutcome.Id);
        Assert.Equal(_apiDescOutcomes.name, _ciMarketOutcome.GetName(ScheduleData.CultureEn));
        Assert.Equal(_apiDescOutcomes.description, _ciMarketOutcome.GetDescription(ScheduleData.CultureEn));
    }

    [Fact]
    public void MarketOutcomeConstructWhenNullDtoThenThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new MarketOutcomeCacheItem(null, ScheduleData.CultureEn));
    }

    [Fact]
    public void MarketOutcomeConstructWhenNullLanguageThenThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new MarketOutcomeCacheItem(_dtoOutcomeDescription, null));
    }

    [Fact]
    public void MarketOutcomeConstructWhenDtoMissingDescription()
    {
        var apiDescOutcomes = new desc_outcomesOutcome { id = "some-id", name = "some-name" };
        var outcomeDescriptionDto = new OutcomeDescriptionDto(apiDescOutcomes);

        var marketOutcomeCacheItem = new MarketOutcomeCacheItem(outcomeDescriptionDto, ScheduleData.CultureEn);

        Assert.NotNull(marketOutcomeCacheItem);
        Assert.Null(marketOutcomeCacheItem.GetDescription(ScheduleData.CultureEn));
    }

    [Fact]
    public void MarketOutcomeConstructWhenEmptyDescription()
    {
        var apiDescOutcomes = new desc_outcomesOutcome { id = "some-id", description = string.Empty, name = "some-name" };
        var outcomeDescriptionDto = new OutcomeDescriptionDto(apiDescOutcomes);

        var marketOutcomeCacheItem = new MarketOutcomeCacheItem(outcomeDescriptionDto, ScheduleData.CultureEn);

        Assert.NotNull(marketOutcomeCacheItem);
        Assert.Null(marketOutcomeCacheItem.GetDescription(ScheduleData.CultureEn));
    }

    [Fact]
    public void MarketOutcomeWhenRequestingNameWithoutCultureThenThrows()
    {
        var marketOutcomeCacheItem = new MarketOutcomeCacheItem(_dtoOutcomeDescription, ScheduleData.CultureEn);

        Assert.Throws<ArgumentNullException>(() => marketOutcomeCacheItem.GetName(null));
    }

    [Fact]
    public void MarketOutcomeWhenRequestingNameForUnknownLanguageThenReturnNull()
    {
        var marketOutcomeCacheItem = new MarketOutcomeCacheItem(_dtoOutcomeDescription, ScheduleData.CultureEn);

        Assert.Null(marketOutcomeCacheItem.GetName(ScheduleData.CultureDe));
    }

    [Fact]
    public void MarketOutcomeWhenRequestingDescriptionWithoutCultureThenThrows()
    {
        var marketOutcomeCacheItem = new MarketOutcomeCacheItem(_dtoOutcomeDescription, ScheduleData.CultureEn);

        Assert.Throws<ArgumentNullException>(() => marketOutcomeCacheItem.GetDescription(null));
    }

    [Fact]
    public void MarketOutcomeWhenRequestingDescriptionForUnknownLanguageThenReturnNull()
    {
        var marketOutcomeCacheItem = new MarketOutcomeCacheItem(_dtoOutcomeDescription, ScheduleData.CultureEn);

        Assert.Null(marketOutcomeCacheItem.GetDescription(ScheduleData.CultureDe));
    }

    [Fact]
    public void MarketOutcomeWhenMergeSecondLanguageForSameId()
    {
        var apiDescOutcomes = new desc_outcomesOutcome { id = "some-id", description = "some-description-de", name = "some-name-de" };
        var outcomeDescriptionDto = new OutcomeDescriptionDto(apiDescOutcomes);

        _ciMarketOutcome.Merge(outcomeDescriptionDto, ScheduleData.CultureDe);

        Assert.Equal(_apiDescOutcomes.id, _ciMarketOutcome.Id);
        Assert.Equal(_apiDescOutcomes.name, _ciMarketOutcome.GetName(ScheduleData.CultureEn));
        Assert.Equal(apiDescOutcomes.name, _ciMarketOutcome.GetName(ScheduleData.CultureDe));
        Assert.Equal(_apiDescOutcomes.description, _ciMarketOutcome.GetDescription(ScheduleData.CultureEn));
        Assert.Equal(apiDescOutcomes.description, _ciMarketOutcome.GetDescription(ScheduleData.CultureDe));
    }

    [Fact]
    public void MarketOutcomeWhenMergeSecondLanguageForDifferentIdThenIdIsNotOverridden()
    {
        var apiDescOutcomes = new desc_outcomesOutcome { id = "some-id-de", description = "some-description-de", name = "some-name-de" };
        var outcomeDescriptionDto = new OutcomeDescriptionDto(apiDescOutcomes);

        _ciMarketOutcome.Merge(outcomeDescriptionDto, ScheduleData.CultureDe);

        Assert.Equal(_apiDescOutcomes.id, _ciMarketOutcome.Id);
        Assert.NotEqual(apiDescOutcomes.id, _ciMarketOutcome.Id);
    }

    [Fact]
    public void MarketOutcomeWhenMergeEmptyDescriptionThenIsNotMerged()
    {
        var apiDescOutcomes = new desc_outcomesOutcome { id = "some-id", description = string.Empty, name = "some-name-de" };
        var outcomeDescriptionDto = new OutcomeDescriptionDto(apiDescOutcomes);

        _ciMarketOutcome.Merge(outcomeDescriptionDto, ScheduleData.CultureDe);

        Assert.Equal(_apiDescOutcomes.description, _ciMarketOutcome.GetDescription(ScheduleData.CultureEn));
        Assert.Null(_ciMarketOutcome.GetDescription(ScheduleData.CultureDe));
    }

    [Fact]
    public void MarketSpecifierConstructWhenValidDto()
    {
        var apiSpecifier = new desc_specifiersSpecifier { name = "some-name", type = "some-type", description = "some-description" };
        var dtoSpecifier = new SpecifierDto(apiSpecifier);

        var ciMarketSpecifier = new MarketSpecifierCacheItem(dtoSpecifier);

        Assert.NotNull(dtoSpecifier);
        Assert.Equal(apiSpecifier.name, ciMarketSpecifier.Name);
        Assert.Equal(apiSpecifier.type, ciMarketSpecifier.Type);
        Assert.Equal(apiSpecifier.description, ciMarketSpecifier.Description);
    }

    [Fact]
    public void MarketSpecifierConstructWhenNullDtoThenThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new MarketSpecifierCacheItem(null));
    }

    [Fact]
    public void MarketAttributeConstructWhenValidDto()
    {
        var apiMarketAttribute = new attributesAttribute { name = "some-name", description = "some-description" };
        var dtoMarketAttribute = new MarketAttributeDto(apiMarketAttribute);

        var ciMarketAttribute = new MarketAttributeCacheItem(dtoMarketAttribute);

        Assert.NotNull(ciMarketAttribute);
        Assert.Equal(apiMarketAttribute.name, ciMarketAttribute.Name);
        Assert.Equal(apiMarketAttribute.description, ciMarketAttribute.Description);
    }

    [Fact]
    public void MarketAttributeConstructWhenNullDtoThenThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new MarketAttributeCacheItem(null));
    }

    [Fact]
    public void MarketMergeResultWhenNoProblemsReportedThenReturnsAllMergedSuccessful()
    {
        var mergeResult = new MarketMergeResult();

        Assert.True(mergeResult.IsAllMerged());
    }

    [Fact]
    public void MarketMergeResultWhenNoProblemsReportedThenReturnsNoOutcomeProblems()
    {
        var mergeResult = new MarketMergeResult();

        Assert.Null(mergeResult.GetOutcomeProblem());
    }

    [Fact]
    public void MarketMergeResultWhenNoProblemsReportedThenReturnsNoMappingProblems()
    {
        var mergeResult = new MarketMergeResult();

        Assert.Null(mergeResult.GetMappingProblem());
    }

    [Fact]
    public void MarketMergeResultWhenOutcomeProblemReportedThenReturnsAllMergedFail()
    {
        var mergeResult = new MarketMergeResult();

        mergeResult.AddOutcomeProblem("some-outcome-id");

        Assert.False(mergeResult.IsAllMerged());
    }

    [Fact]
    public void MarketMergeResultWhenOutcomeProblemReportedThenReturnsProblems()
    {
        var mergeResult = new MarketMergeResult();

        mergeResult.AddOutcomeProblem("some-outcome-id");

        Assert.Single(mergeResult.GetOutcomeProblem());
    }

    [Fact]
    public void MarketMergeResultWhenMappingProblemReportedThenReturnsAllMergedFail()
    {
        var mergeResult = new MarketMergeResult();

        mergeResult.AddMappingProblem("some-mapping-id");

        Assert.False(mergeResult.IsAllMerged());
    }

    [Fact]
    public void MarketMergeResultWhenMappingProblemReportedThenReturnsProblems()
    {
        var mergeResult = new MarketMergeResult();

        mergeResult.AddMappingProblem("some-mapping-id");

        Assert.Single(mergeResult.GetMappingProblem());
    }
}

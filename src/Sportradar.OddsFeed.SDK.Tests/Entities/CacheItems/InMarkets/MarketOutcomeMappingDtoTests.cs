// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InMarkets;

public class MarketOutcomeMappingDtoTests
{
    private readonly mappingsMappingMapping_outcome _apiOutcomeMapping;
    private readonly variant_mappingsMappingMapping_outcome _apiVariantOutcomeMapping;
    private const string MarketId = "11";

    public MarketOutcomeMappingDtoTests()
    {
        _apiOutcomeMapping = new mappingsMappingMapping_outcome { outcome_id = "13", product_outcome_id = "2528", product_outcome_name = "under" };
        _apiVariantOutcomeMapping = new variant_mappingsMappingMapping_outcome { outcome_id = "13", product_outcome_id = "2528", product_outcome_name = "under" };
    }

    [Fact]
    public void FromOutcomeMapping_Valid()
    {
        var outcomeMappingDto = new OutcomeMappingDto(_apiOutcomeMapping, MarketId);

        Assert.NotNull(outcomeMappingDto);
        Assert.Equal("13", outcomeMappingDto.OutcomeId);
        Assert.Equal("2528", outcomeMappingDto.ProducerOutcomeId);
        Assert.Equal("under", outcomeMappingDto.ProducerOutcomeName);
        Assert.Equal(MarketId, outcomeMappingDto.MarketId);
    }

    [Fact]
    public void FromOutcomeMapping_WhenNullThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new OutcomeMappingDto((mappingsMappingMapping_outcome)null, MarketId));
    }

    [Fact]
    public void FromOutcomeMapping_WhenNullMarketIdThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new OutcomeMappingDto(_apiOutcomeMapping, null));
    }

    [Fact]
    public void FromOutcomeMapping_WhenEmptyMarketIdThrows()
    {
        Assert.Throws<ArgumentException>(() => new OutcomeMappingDto(_apiOutcomeMapping, string.Empty));
    }

    [Fact]
    public void FromOutcomeMapping_WhenNullOutcomeId()
    {
        _apiOutcomeMapping.outcome_id = null;

        var outcomeMappingDto = new OutcomeMappingDto(_apiOutcomeMapping, MarketId);

        Assert.Null(outcomeMappingDto.OutcomeId);
    }

    [Fact]
    public void FromOutcomeMapping_WhenNullProductOutcomeId()
    {
        _apiOutcomeMapping.product_outcome_id = null;

        var outcomeMappingDto = new OutcomeMappingDto(_apiOutcomeMapping, MarketId);

        Assert.Null(outcomeMappingDto.ProducerOutcomeId);
    }

    [Fact]
    public void FromOutcomeMapping_WhenNullProductOutcomeName()
    {
        _apiOutcomeMapping.product_outcome_name = null;

        var outcomeMappingDto = new OutcomeMappingDto(_apiOutcomeMapping, MarketId);

        Assert.Null(outcomeMappingDto.ProducerOutcomeName);
    }

    [Fact]
    public void FromVariantOutcomeMapping_Valid()
    {
        var outcomeMappingDto = new OutcomeMappingDto(_apiVariantOutcomeMapping, MarketId);

        Assert.NotNull(outcomeMappingDto);
        Assert.Equal("13", outcomeMappingDto.OutcomeId);
        Assert.Equal("2528", outcomeMappingDto.ProducerOutcomeId);
        Assert.Equal("under", outcomeMappingDto.ProducerOutcomeName);
        Assert.Equal(MarketId, outcomeMappingDto.MarketId);
    }

    [Fact]
    public void FromVariantOutcomeMapping_WhenNullThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new OutcomeMappingDto((variant_mappingsMappingMapping_outcome)null, MarketId));
    }

    [Fact]
    public void FromVariantOutcomeMapping_WhenNullMarketIdThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new OutcomeMappingDto(_apiVariantOutcomeMapping, null));
    }

    [Fact]
    public void FromVariantOutcomeMapping_WhenEmptyMarketIdThrows()
    {
        Assert.Throws<ArgumentException>(() => new OutcomeMappingDto(_apiVariantOutcomeMapping, string.Empty));
    }

    [Fact]
    public void FromVariantOutcomeMapping_WhenNullOutcomeId()
    {
        _apiVariantOutcomeMapping.outcome_id = null;

        var outcomeMappingDto = new OutcomeMappingDto(_apiVariantOutcomeMapping, MarketId);

        Assert.Null(outcomeMappingDto.OutcomeId);
    }

    [Fact]
    public void FromVariantOutcomeMapping_WhenNullProductOutcomeId()
    {
        _apiVariantOutcomeMapping.product_outcome_id = null;

        var outcomeMappingDto = new OutcomeMappingDto(_apiVariantOutcomeMapping, MarketId);

        Assert.Null(outcomeMappingDto.ProducerOutcomeId);
    }

    [Fact]
    public void FromVariantOutcomeMapping_WhenNullProductOutcomeName()
    {
        _apiVariantOutcomeMapping.product_outcome_name = null;

        var outcomeMappingDto = new OutcomeMappingDto(_apiVariantOutcomeMapping, MarketId);

        Assert.Null(outcomeMappingDto.ProducerOutcomeName);
    }
}

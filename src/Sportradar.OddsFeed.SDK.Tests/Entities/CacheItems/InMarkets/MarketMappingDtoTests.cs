// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InMarkets;

public class MarketMappingDtoTests
{
    // standard market description mapping node
    //<mappings>
    //  <mapping product_id = "1" product_ids="1|4" sport_id="sr:sport:3" market_id="8:232" sov_template="{total}">
    //      <mapping_outcome outcome_id = "13" product_outcome_id="2528" product_outcome_name="under"/>
    //      <mapping_outcome outcome_id = "12" product_outcome_id="2530" product_outcome_name="over"/>
    //  </mapping>
    //</mappings>
    private readonly mappingsMapping _apiMarketMapping;

    public MarketMappingDtoTests()
    {
        var mappingOutcome1 = new mappingsMappingMapping_outcome { outcome_id = "13", product_outcome_id = "2528", product_outcome_name = "under" };
        var mappingOutcome2 = new mappingsMappingMapping_outcome { outcome_id = "12", product_outcome_id = "2530", product_outcome_name = "over" };
        _apiMarketMapping = new mappingsMapping
        {
            market_id = "8:232",
            product_id = 1,
            product_ids = "1|4",
            sport_id = "sr:sport:3",
            sov_template = "{total}",
            valid_for = "mapnr=1",
            mapping_outcome = new[] { mappingOutcome1, mappingOutcome2 }
        };
    }

    [Fact]
    public void FromMappingsMapping_Valid()
    {
        var marketMappingDto = new MarketMappingDto(_apiMarketMapping);

        Assert.NotNull(marketMappingDto);
        Assert.Equal(8, marketMappingDto.MarketTypeId);
        Assert.Equal(232, marketMappingDto.MarketSubTypeId);
        Assert.Equal(2, marketMappingDto.ProducerIds.Count());
        Assert.Contains(1, marketMappingDto.ProducerIds);
        Assert.Contains(4, marketMappingDto.ProducerIds);
        Assert.Equal(_apiMarketMapping.sport_id, marketMappingDto.SportId.ToString());
        Assert.Equal(_apiMarketMapping.sov_template, marketMappingDto.SovTemplate);
        Assert.Equal(_apiMarketMapping.valid_for, marketMappingDto.ValidFor);
        Assert.Null(marketMappingDto.OrgMarketId);
        ValidateMappingOutcome(_apiMarketMapping.mapping_outcome[0], marketMappingDto.OutcomeMappings.ElementAt(0));
        ValidateMappingOutcome(_apiMarketMapping.mapping_outcome[1], marketMappingDto.OutcomeMappings.ElementAt(1));
    }

    [Fact]
    public void FromMappingsMapping_WhenNullThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new MarketMappingDto((mappingsMapping)null));
    }

    [Fact]
    public void FromMappingsMapping_WhenNullMarketIdThrows()
    {
        _apiMarketMapping.market_id = null;

        Assert.Throws<ArgumentNullException>(() => new MarketMappingDto(_apiMarketMapping));
    }

    [Fact]
    public void FromMappingsMapping_WhenEmptyMarketIdThrows()
    {
        _apiMarketMapping.market_id = string.Empty;

        Assert.Throws<ArgumentException>(() => new MarketMappingDto(_apiMarketMapping));
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("0")]
    [InlineData("2")]
    [InlineData("01")]
    public void FromMappingsMapping_WhenValidMarketId(string marketId)
    {
        _apiMarketMapping.market_id = marketId;

        var marketMappingDto = new MarketMappingDto(_apiMarketMapping);

        Assert.Equal(int.Parse(marketId), marketMappingDto.MarketTypeId);
        Assert.Null(marketMappingDto.MarketSubTypeId);
    }

    [Theory]
    [InlineData("-1", "1")]
    [InlineData("0", "1")]
    [InlineData("2", "5")]
    [InlineData("2", "-5")]
    [InlineData("-2", "-5")]
    [InlineData("0", "-1")]
    public void FromMappingsMapping_WhenValidMarketIdAndSubTypeThrows(string typeId, string subTypeId)
    {
        _apiMarketMapping.market_id = $"{typeId}:{subTypeId}";

        var marketMappingDto = new MarketMappingDto(_apiMarketMapping);

        Assert.Equal(int.Parse(typeId), marketMappingDto.MarketTypeId);
        Assert.Equal(int.Parse(subTypeId), marketMappingDto.MarketSubTypeId);
    }

    [Theory]
    [InlineData("2:a")]
    [InlineData("2:")]
    [InlineData("2|123")]
    [InlineData("2-245")]
    [InlineData("2:123:123")]
    public void FromMappingsMapping_WhenInvalidMarketIdThrows(string marketId)
    {
        _apiMarketMapping.market_id = marketId;

        Assert.Throws<FormatException>(() => new MarketMappingDto(_apiMarketMapping));
    }

    [Fact]
    public void FromMappingsMapping_WhenMissingProducerIdThrows()
    {
        _apiMarketMapping.product_id = 0;

        Assert.Throws<ArgumentOutOfRangeException>(() => new MarketMappingDto(_apiMarketMapping));
    }

    [Fact]
    public void FromMappingsMapping_WhenNegativeProductIdThrows()
    {
        _apiMarketMapping.product_id = -1;

        Assert.Throws<ArgumentOutOfRangeException>(() => new MarketMappingDto(_apiMarketMapping));
    }

    [Fact]
    public void FromMappingsMapping_WhenMissingProducerIdsThenProductIdIsUsed()
    {
        _apiMarketMapping.product_ids = string.Empty;

        var marketMappingDto = new MarketMappingDto(_apiMarketMapping);

        Assert.Single(marketMappingDto.ProducerIds);
        Assert.Contains(_apiMarketMapping.product_id, marketMappingDto.ProducerIds);
    }

    [Fact]
    public void FromMappingsMapping_WhenNullProducerIdsThenProductIdIsUsed()
    {
        _apiMarketMapping.product_ids = null;

        var marketMappingDto = new MarketMappingDto(_apiMarketMapping);

        Assert.Single(marketMappingDto.ProducerIds);
        Assert.Contains(_apiMarketMapping.product_id, marketMappingDto.ProducerIds);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1")]
    [InlineData("1|2")]
    [InlineData("1|3|5|6|7|8")]
    public void FromMappingsMapping_WhenValidProductIds(string producerIds)
    {
        _apiMarketMapping.product_ids = producerIds;

        var marketMappingDto = new MarketMappingDto(_apiMarketMapping);

        Assert.NotNull(marketMappingDto);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("1:3")]
    [InlineData("1|-3")]
    [InlineData("1|5|-2")]
    [InlineData("-1:-2")]
    [InlineData("-112:2453")]
    [InlineData("2-245")]
    [InlineData("2|a")]
    [InlineData("2|123|5|1|-5")]
    public void FromMappingsMapping_WhenInvalidProductIdsThrows(string producerIds)
    {
        _apiMarketMapping.product_ids = producerIds;

        Assert.Throws<FormatException>(() => new MarketMappingDto(_apiMarketMapping));
    }

    [Fact]
    public void FromMappingsMapping_WhenNullSportIdThrows()
    {
        _apiMarketMapping.sport_id = null;

        Assert.Throws<ArgumentNullException>(() => new MarketMappingDto(_apiMarketMapping));
    }

    [Fact]
    public void FromMappingsMapping_WhenEmptySportIdThrows()
    {
        _apiMarketMapping.sport_id = string.Empty;

        Assert.Throws<ArgumentException>(() => new MarketMappingDto(_apiMarketMapping));
    }

    [Fact]
    public void FromMappingsMapping_WhenInvalidSportIdThrows()
    {
        _apiMarketMapping.sport_id = "sr:sport-1";

        Assert.Throws<FormatException>(() => new MarketMappingDto(_apiMarketMapping));
    }

    [Fact]
    public void FromMappingsMapping_WhenNullSovTemplate()
    {
        _apiMarketMapping.sov_template = null;

        var marketMappingDto = new MarketMappingDto(_apiMarketMapping);

        Assert.Null(marketMappingDto.SovTemplate);
    }

    [Fact]
    public void FromMappingsMapping_WhenEmptySovTemplate()
    {
        _apiMarketMapping.sov_template = string.Empty;

        var marketMappingDto = new MarketMappingDto(_apiMarketMapping);

        Assert.Empty(marketMappingDto.SovTemplate);
    }

    [Fact]
    public void FromMappingsMapping_WhenValidValidFor()
    {
        _apiMarketMapping.valid_for = "periodnr=1|total~*.5";

        var marketMappingDto = new MarketMappingDto(_apiMarketMapping);

        Assert.Equal(_apiMarketMapping.valid_for, marketMappingDto.ValidFor);
    }

    [Fact]
    public void FromMappingsMapping_WhenNullValidFor()
    {
        _apiMarketMapping.valid_for = null;

        var marketMappingDto = new MarketMappingDto(_apiMarketMapping);

        Assert.Null(marketMappingDto.ValidFor);
    }

    [Fact]
    public void FromMappingsMapping_WhenEmptyValidFor()
    {
        _apiMarketMapping.valid_for = string.Empty;

        var marketMappingDto = new MarketMappingDto(_apiMarketMapping);

        Assert.Empty(marketMappingDto.ValidFor);
    }

    [Fact]
    public void FromMappingsMapping_WhenNullOutcomeMappings()
    {
        _apiMarketMapping.mapping_outcome = null;

        var marketMappingDto = new MarketMappingDto(_apiMarketMapping);

        Assert.Empty(marketMappingDto.OutcomeMappings);
    }

    [Fact]
    public void FromMappingsMapping_WhenEmptyOutcomeMappings()
    {
        _apiMarketMapping.mapping_outcome = Array.Empty<mappingsMappingMapping_outcome>();

        var marketMappingDto = new MarketMappingDto(_apiMarketMapping);

        Assert.Empty(marketMappingDto.OutcomeMappings);
    }

    private static void ValidateMappingOutcome(mappingsMappingMapping_outcome msg, OutcomeMappingDto dto)
    {
        Assert.Equal(msg.outcome_id, dto.OutcomeId);
        Assert.Equal(msg.product_outcome_id, dto.ProducerOutcomeId);
        Assert.Equal(msg.product_outcome_name, dto.ProducerOutcomeName);
    }
}

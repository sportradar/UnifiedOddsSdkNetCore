// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InMarkets;

public class MarketMappingFromVariantDtoTests
{
    // standard market description mapping node
    //<mappings>
    //  <mapping product_id = "1" product_ids="1|4" sport_id="sr:sport:3" market_id="8:232" sov_template="{total}">
    //      <mapping_outcome outcome_id = "13" product_outcome_id="2528" product_outcome_name="under"/>
    //      <mapping_outcome outcome_id = "12" product_outcome_id="2530" product_outcome_name="over"/>
    //  </mapping>
    //</mappings>
    // from variant market description mapping node
    //<mapping product_id = "1" product_ids="1|4" sport_id="sr:sport:20" market_id="239" product_market_id="8:68">
    //    <mapping_outcome outcome_id = "sr:decided_by_extra_points:bestof:5:53" product_outcome_id="219" product_outcome_name="0"/>
    //    <mapping_outcome outcome_id = "sr:decided_by_extra_points:bestof:5:54" product_outcome_id="220" product_outcome_name="1"/>
    //    <mapping_outcome outcome_id = "sr:decided_by_extra_points:bestof:5:55" product_outcome_id="221" product_outcome_name="2"/>
    //    <mapping_outcome outcome_id = "sr:decided_by_extra_points:bestof:5:56" product_outcome_id="222" product_outcome_name="3"/>
    //    <mapping_outcome outcome_id = "sr:decided_by_extra_points:bestof:5:57" product_outcome_id="223" product_outcome_name="4"/>
    //    <mapping_outcome outcome_id = "sr:decided_by_extra_points:bestof:5:58" product_outcome_id="224" product_outcome_name="5"/>
    //</mapping>
    private readonly variant_mappingsMapping _apiVariantMarketMapping;

    public MarketMappingFromVariantDtoTests()
    {
        var mappingOutcome1 = new variant_mappingsMappingMapping_outcome { outcome_id = "13", product_outcome_id = "2528", product_outcome_name = "under" };
        var mappingOutcome2 = new variant_mappingsMappingMapping_outcome { outcome_id = "12", product_outcome_id = "2530", product_outcome_name = "over" };
        _apiVariantMarketMapping = new variant_mappingsMapping
        {
            market_id = "5:232",
            product_id = 1,
            product_ids = "1|4",
            sport_id = "sr:sport:3",
            sov_template = "{total}",
            valid_for = "mapnr=1",
            mapping_outcome = new[] { mappingOutcome1, mappingOutcome2 },
            product_market_id = "8:68"
        };
    }

    [Fact]
    public void FromMappingsMappingIsValid()
    {
        var marketMappingDto = new MarketMappingDto(_apiVariantMarketMapping);

        Assert.NotNull(marketMappingDto);
        Assert.Equal(8, marketMappingDto.MarketTypeId);
        Assert.Equal(68, marketMappingDto.MarketSubTypeId);
        Assert.Equal(2, marketMappingDto.ProducerIds.Count());
        Assert.Contains(1, marketMappingDto.ProducerIds);
        Assert.Contains(4, marketMappingDto.ProducerIds);
        Assert.Equal(_apiVariantMarketMapping.sport_id, marketMappingDto.SportId.ToString());
        Assert.Equal(_apiVariantMarketMapping.sov_template, marketMappingDto.SovTemplate);
        Assert.Equal(_apiVariantMarketMapping.valid_for, marketMappingDto.ValidFor);
        Assert.Equal(_apiVariantMarketMapping.market_id, marketMappingDto.OrgMarketId);
        ValidateMappingOutcome(_apiVariantMarketMapping.mapping_outcome[0], marketMappingDto.OutcomeMappings.ElementAt(0));
        ValidateMappingOutcome(_apiVariantMarketMapping.mapping_outcome[1], marketMappingDto.OutcomeMappings.ElementAt(1));
    }

    [Fact]
    public void FromMappingsMappingWhenNullThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new MarketMappingDto((variant_mappingsMapping)null));
    }

    [Fact]
    public void FromMappingsMappingWhenNullMarketIdThrows()
    {
        _apiVariantMarketMapping.market_id = null;

        Assert.Throws<ArgumentNullException>(() => new MarketMappingDto(_apiVariantMarketMapping));
    }

    [Fact]
    public void FromMappingsMappingWhenEmptyMarketIdThrows()
    {
        _apiVariantMarketMapping.market_id = string.Empty;

        Assert.Throws<ArgumentException>(() => new MarketMappingDto(_apiVariantMarketMapping));
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("0")]
    [InlineData("2")]
    [InlineData("01")]
    public void FromMappingsMappingWhenValidMarketId(string marketId)
    {
        _apiVariantMarketMapping.market_id = marketId;
        _apiVariantMarketMapping.product_market_id = null;

        var marketMappingDto = new MarketMappingDto(_apiVariantMarketMapping);

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
    public void FromMappingsMappingWhenValidMarketIdAndSubType(string typeId, string subTypeId)
    {
        _apiVariantMarketMapping.market_id = $"{typeId}:{subTypeId}";
        _apiVariantMarketMapping.product_market_id = null;

        var marketMappingDto = new MarketMappingDto(_apiVariantMarketMapping);

        Assert.Equal(int.Parse(typeId), marketMappingDto.MarketTypeId);
        Assert.Equal(int.Parse(subTypeId), marketMappingDto.MarketSubTypeId);
    }

    [Theory]
    [InlineData("2:a")]
    [InlineData("2:")]
    [InlineData("2|123")]
    [InlineData("2-245")]
    [InlineData("2:123:123")]
    public void FromMappingsMappingWhenInvalidMarketIdThenThrows(string marketId)
    {
        _apiVariantMarketMapping.market_id = marketId;
        _apiVariantMarketMapping.product_market_id = null;

        Assert.Throws<FormatException>(() => new MarketMappingDto(_apiVariantMarketMapping));
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("0")]
    [InlineData("2")]
    [InlineData("01")]
    public void FromMappingsMappingWhenValidMarketIdExistsAndProductMarketIdHasPrecedence(string marketId)
    {
        _apiVariantMarketMapping.product_market_id = marketId;

        var marketMappingDto = new MarketMappingDto(_apiVariantMarketMapping);

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
    public void FromMappingsMappingWhenValidMarketIdAndSubTypeAndProductMarketIdHasPrecedence(string typeId, string subTypeId)
    {
        _apiVariantMarketMapping.product_market_id = $"{typeId}:{subTypeId}";

        var marketMappingDto = new MarketMappingDto(_apiVariantMarketMapping);

        Assert.Equal(int.Parse(typeId), marketMappingDto.MarketTypeId);
        Assert.Equal(int.Parse(subTypeId), marketMappingDto.MarketSubTypeId);
    }

    [Theory]
    [InlineData("2:a")]
    [InlineData("2:")]
    [InlineData("2|123")]
    [InlineData("2-245")]
    [InlineData("2:123:123")]
    public void FromMappingsMappingWhenValidMarketIdAndInvalidProductMarketIdHasPrecedenceThrows(string marketId)
    {
        _apiVariantMarketMapping.product_market_id = marketId;

        Assert.Throws<FormatException>(() => new MarketMappingDto(_apiVariantMarketMapping));
    }

    [Fact]
    public void FromMappingsMappingWhenSimpleMarketIdAndNoProductMarketIdPresent()
    {
        _apiVariantMarketMapping.market_id = "35";
        _apiVariantMarketMapping.product_market_id = null;

        var marketMappingDto = new MarketMappingDto(_apiVariantMarketMapping);

        Assert.Equal(35, marketMappingDto.MarketTypeId);
        Assert.Null(marketMappingDto.MarketSubTypeId);
    }

    [Fact]
    public void FromMappingsMappingWhenMarketIdWithSubTypeAndNoProductMarketIdPresent()
    {
        _apiVariantMarketMapping.market_id = "35:123";
        _apiVariantMarketMapping.product_market_id = null;

        var marketMappingDto = new MarketMappingDto(_apiVariantMarketMapping);

        Assert.Equal(35, marketMappingDto.MarketTypeId);
        Assert.Equal(123, marketMappingDto.MarketSubTypeId);
    }

    [Fact]
    public void FromMappingsMappingWhenMarketIdAndProductMarketId_ProductMarketIdHasPriority()
    {
        _apiVariantMarketMapping.market_id = "35";
        _apiVariantMarketMapping.product_market_id = "22";

        var marketMappingDto = new MarketMappingDto(_apiVariantMarketMapping);

        Assert.Equal(22, marketMappingDto.MarketTypeId);
        Assert.Null(marketMappingDto.MarketSubTypeId);
    }

    [Fact]
    public void FromMappingsMappingWhenMarketIdWithSubTypeAndProductMarketId_ProductMarketIdHasPriority()
    {
        _apiVariantMarketMapping.market_id = "35:123";
        _apiVariantMarketMapping.product_market_id = "22";

        var marketMappingDto = new MarketMappingDto(_apiVariantMarketMapping);

        Assert.Equal(22, marketMappingDto.MarketTypeId);
        Assert.Null(marketMappingDto.MarketSubTypeId);
    }

    [Fact]
    public void FromMappingsMappingWhenMarketIdWithSubTypeAndProductMarketIdWithSubTypeThenProductMarketIdHasPriority()
    {
        _apiVariantMarketMapping.market_id = "35:312";
        _apiVariantMarketMapping.product_market_id = "22:123";

        var marketMappingDto = new MarketMappingDto(_apiVariantMarketMapping);

        Assert.Equal(22, marketMappingDto.MarketTypeId);
        Assert.Equal(123, marketMappingDto.MarketSubTypeId);
    }

    [Fact]
    public void FromMappingsMappingWhenMissingProducerIdThenThrows()
    {
        _apiVariantMarketMapping.product_id = 0;

        Assert.Throws<ArgumentOutOfRangeException>(() => new MarketMappingDto(_apiVariantMarketMapping));
    }

    [Fact]
    public void FromMappingsMappingWhenNegativeProductIdThenThrows()
    {
        _apiVariantMarketMapping.product_id = -1;

        Assert.Throws<ArgumentOutOfRangeException>(() => new MarketMappingDto(_apiVariantMarketMapping));
    }

    [Fact]
    public void FromMappingsMappingWhenMissingProducerIdsThenProductIdIsUsed()
    {
        _apiVariantMarketMapping.product_ids = string.Empty;

        var marketMappingDto = new MarketMappingDto(_apiVariantMarketMapping);

        Assert.Single(marketMappingDto.ProducerIds);
        Assert.Contains(_apiVariantMarketMapping.product_id, marketMappingDto.ProducerIds);
    }

    [Fact]
    public void FromMappingsMappingWhenNullProducerIdsThenProductIdIsUsed()
    {
        _apiVariantMarketMapping.product_ids = null;

        var marketMappingDto = new MarketMappingDto(_apiVariantMarketMapping);

        Assert.Single(marketMappingDto.ProducerIds);
        Assert.Contains(_apiVariantMarketMapping.product_id, marketMappingDto.ProducerIds);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1")]
    [InlineData("1|2")]
    [InlineData("1|3|5|6|7|8")]
    public void FromMappingsMappingWhenValidProductIds(string producerIds)
    {
        _apiVariantMarketMapping.product_ids = producerIds;

        var marketMappingDto = new MarketMappingDto(_apiVariantMarketMapping);

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
    public void FromMappingsMappingWhenInvalidProductIdsThenThrows(string producerIds)
    {
        _apiVariantMarketMapping.product_ids = producerIds;

        Assert.Throws<FormatException>(() => new MarketMappingDto(_apiVariantMarketMapping));
    }

    [Fact]
    public void FromMappingsMappingWhenNullSportIdThenThrows()
    {
        _apiVariantMarketMapping.sport_id = null;

        Assert.Throws<ArgumentNullException>(() => new MarketMappingDto(_apiVariantMarketMapping));
    }

    [Fact]
    public void FromMappingsMappingWhenEmptySportIdThenThrows()
    {
        _apiVariantMarketMapping.sport_id = string.Empty;

        Assert.Throws<ArgumentException>(() => new MarketMappingDto(_apiVariantMarketMapping));
    }

    [Fact]
    public void FromMappingsMappingWhenInvalidSportIdThenThrows()
    {
        _apiVariantMarketMapping.sport_id = "sr:sport-1";

        Assert.Throws<FormatException>(() => new MarketMappingDto(_apiVariantMarketMapping));
    }

    [Fact]
    public void FromMappingsMappingWhenNullSovTemplate()
    {
        _apiVariantMarketMapping.sov_template = null;

        var marketMappingDto = new MarketMappingDto(_apiVariantMarketMapping);

        Assert.Null(marketMappingDto.SovTemplate);
    }

    [Fact]
    public void FromMappingsMappingWhenEmptySovTemplate()
    {
        _apiVariantMarketMapping.sov_template = string.Empty;

        var marketMappingDto = new MarketMappingDto(_apiVariantMarketMapping);

        Assert.Empty(marketMappingDto.SovTemplate);
    }

    [Fact]
    public void FromMappingsMappingWhenValidValidFor()
    {
        _apiVariantMarketMapping.valid_for = "periodnr=1|total~*.5";

        var marketMappingDto = new MarketMappingDto(_apiVariantMarketMapping);

        Assert.Equal(_apiVariantMarketMapping.valid_for, marketMappingDto.ValidFor);
    }

    [Fact]
    public void FromMappingsMappingWhenNullValidFor()
    {
        _apiVariantMarketMapping.valid_for = null;

        var marketMappingDto = new MarketMappingDto(_apiVariantMarketMapping);

        Assert.Null(marketMappingDto.ValidFor);
    }

    [Fact]
    public void FromMappingsMappingWhenEmptyValidFor()
    {
        _apiVariantMarketMapping.valid_for = string.Empty;

        var marketMappingDto = new MarketMappingDto(_apiVariantMarketMapping);

        Assert.Empty(marketMappingDto.ValidFor);
    }

    [Fact]
    public void FromMappingsMappingWhenNullOutcomeMappings()
    {
        _apiVariantMarketMapping.mapping_outcome = null;

        var marketMappingDto = new MarketMappingDto(_apiVariantMarketMapping);

        Assert.Empty(marketMappingDto.OutcomeMappings);
    }

    [Fact]
    public void FromMappingsMappingWhenEmptyOutcomeMappings()
    {
        _apiVariantMarketMapping.mapping_outcome = Array.Empty<variant_mappingsMappingMapping_outcome>();

        var marketMappingDto = new MarketMappingDto(_apiVariantMarketMapping);

        Assert.Empty(marketMappingDto.OutcomeMappings);
    }

    private static void ValidateMappingOutcome(variant_mappingsMappingMapping_outcome msg, OutcomeMappingDto dto)
    {
        Assert.Equal(msg.outcome_id, dto.OutcomeId);
        Assert.Equal(msg.product_outcome_id, dto.ProducerOutcomeId);
        Assert.Equal(msg.product_outcome_name, dto.ProducerOutcomeName);
    }
}

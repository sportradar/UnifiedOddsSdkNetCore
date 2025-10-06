// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InMarkets;

public class VariantDescriptionDtoTests
{
    private readonly desc_variant _apiMd;
    private VariantDescriptionDto _dtoMd;

    public VariantDescriptionDtoTests()
    {
        var variantDescriptionXml =
            @"<variant_descriptions response_code='OK'>
                <variant id='sr:correct_score:bestof:12'>
                    <outcomes>
                        <outcome id='sr:correct_score:bestof:12:192' name='7:0'/>
                        <outcome id='sr:correct_score:bestof:12:193' name='7:1'/>
                        <outcome id='sr:correct_score:bestof:12:194' name='7:2'/>
                        <outcome id='sr:correct_score:bestof:12:195' name='7:3'/>
                        <outcome id='sr:correct_score:bestof:12:196' name='7:4'/>
                        <outcome id='sr:correct_score:bestof:12:197' name='7:5'/>
                        <outcome id='sr:correct_score:bestof:12:198' name='6:6'/>
                        <outcome id='sr:correct_score:bestof:12:199' name='5:7'/>
                        <outcome id='sr:correct_score:bestof:12:200' name='4:7'/>
                        <outcome id='sr:correct_score:bestof:12:201' name='3:7'/>
                        <outcome id='sr:correct_score:bestof:12:202' name='2:7'/>
                        <outcome id='sr:correct_score:bestof:12:203' name='1:7'/>
                        <outcome id='sr:correct_score:bestof:12:204' name='0:7'/>
                    </outcomes>
                    <mappings>
                        <mapping product_id='3' product_ids='3' sport_id='sr:sport:22' market_id='374' product_market_id='455'>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:192' product_outcome_id='33' product_outcome_name='7:0'/>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:193' product_outcome_id='34' product_outcome_name='7:1'/>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:194' product_outcome_id='318' product_outcome_name='7:2'/>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:195' product_outcome_id='320' product_outcome_name='7:3'/>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:196' product_outcome_id='322' product_outcome_name='7:4'/>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:197' product_outcome_id='116' product_outcome_name='7:5'/>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:198' product_outcome_id='602' product_outcome_name='6:6'/>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:199' product_outcome_id='121' product_outcome_name='5:7'/>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:200' product_outcome_id='323' product_outcome_name='4:7'/>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:201' product_outcome_id='321' product_outcome_name='3:7'/>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:202' product_outcome_id='319' product_outcome_name='2:7'/>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:203' product_outcome_id='57' product_outcome_name='1:7'/>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:204' product_outcome_id='56' product_outcome_name='0:7'/>
                        </mapping>
                    </mappings>
                </variant>
            </variant_descriptions>";
        var apiMarketDescriptions = DeserializerHelper.GetDeserializedApiMessage<variant_descriptions>(variantDescriptionXml);
        _apiMd = apiMarketDescriptions.variant[0];
        _dtoMd = new VariantDescriptionDto(_apiMd);
    }

    [Fact]
    public void ConstructWithNullApiData_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new VariantDescriptionDto(null));
    }

    [Fact]
    public void IsValid()
    {
        Assert.NotNull(_dtoMd);
    }

    [Fact]
    public void MappedId()
    {
        Assert.Equal(_apiMd.id, _dtoMd.Id);
    }

    [Fact]
    public void MapWhenNullId()
    {
        _apiMd.id = null;

        Assert.Throws<ArgumentNullException>(() => new VariantDescriptionDto(_apiMd));
    }

    [Fact]
    public void MapWhenEmptyId()
    {
        _apiMd.id = string.Empty;

        Assert.Throws<ArgumentException>(() => new VariantDescriptionDto(_apiMd));
    }

    [Fact]
    public void MappedMappingsNull()
    {
        _apiMd.mappings = null;
        _dtoMd = new VariantDescriptionDto(_apiMd);

        Assert.Null(_dtoMd.Mappings);
    }

    [Fact]
    public void MappedMappingsEmpty()
    {
        _apiMd.mappings = Array.Empty<variant_mappingsMapping>();
        _dtoMd = new VariantDescriptionDto(_apiMd);

        Assert.Empty(_dtoMd.Mappings);
    }

    [Fact]
    public void MappedMappings()
    {
        Assert.Equal(_apiMd.mappings.Length, _dtoMd.Mappings.Count);

        for (var i = 0; i < _apiMd.mappings.Length; i++)
        {
            ValidateMapping(_apiMd.mappings[i], _dtoMd.Mappings.ToArray()[i]);
        }
    }

    [Fact]
    public void MappedOutcomesNull()
    {
        _apiMd.outcomes = null;
        _dtoMd = new VariantDescriptionDto(_apiMd);

        Assert.Null(_dtoMd.Outcomes);
    }

    [Fact]
    public void MappedOutcomesEmpty()
    {
        _apiMd.outcomes = Array.Empty<desc_variant_outcomesOutcome>();
        _dtoMd = new VariantDescriptionDto(_apiMd);

        Assert.Empty(_dtoMd.Outcomes);
    }

    [Fact]
    public void MappedValidOutcomes()
    {
        Assert.Equal(_apiMd.outcomes.Length, _dtoMd.Outcomes.Count);

        for (var i = 0; i < _apiMd.outcomes.Length; i++)
        {
            ValidateOutcome(_apiMd.outcomes[i], _dtoMd.Outcomes.ToArray()[i]);
        }
    }

    [Fact]
    public void DeserializeWhenVariantIdNullThenThrows()
    {
        const string apiXml =
            @"<variant_descriptions response_code='OK'>
                <variant>
                    <outcomes>
                        <outcome id='sr:correct_score:bestof:12:192' name='7:0'/>
                        <outcome id='sr:correct_score:bestof:12:193' name='7:1'/>
                        <outcome id='sr:correct_score:bestof:12:194' name='7:2'/>
                        <outcome id='sr:correct_score:bestof:12:195' name='7:3'/>
                        <outcome id='sr:correct_score:bestof:12:196' name='7:4'/>
                        <outcome id='sr:correct_score:bestof:12:197' name='7:5'/>
                    </outcomes>
                    <mappings>
                        <mapping product_id='3' product_ids='3' sport_id='sr:sport:22' market_id='374' product_market_id='455'>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:192' product_outcome_id='33' product_outcome_name='7:0'/>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:193' product_outcome_id='34' product_outcome_name='7:1'/>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:194' product_outcome_id='318' product_outcome_name='7:2'/>
                        </mapping>
                    </mappings>
                </variant>
            </variant_descriptions>";
        var apiMarketDescriptions = DeserializerHelper.GetDeserializedApiMessage<variant_descriptions>(apiXml);

        Assert.Throws<ArgumentNullException>(() => new VariantDescriptionDto(apiMarketDescriptions.variant[0]));
    }

    [Fact]
    public void DeserializeWhenVariantIdIsEmptyThenThrows()
    {
        const string apiXml =
            @"<variant_descriptions response_code='OK'>
                <variant id=''>
                    <outcomes>
                        <outcome id='sr:correct_score:bestof:12:192' name='7:0'/>
                        <outcome id='sr:correct_score:bestof:12:193' name='7:1'/>
                        <outcome id='sr:correct_score:bestof:12:194' name='7:2'/>
                        <outcome id='sr:correct_score:bestof:12:195' name='7:3'/>
                        <outcome id='sr:correct_score:bestof:12:196' name='7:4'/>
                        <outcome id='sr:correct_score:bestof:12:197' name='7:5'/>
                    </outcomes>
                    <mappings>
                        <mapping product_id='3' product_ids='3' sport_id='sr:sport:22' market_id='374' product_market_id='455'>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:192' product_outcome_id='33' product_outcome_name='7:0'/>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:193' product_outcome_id='34' product_outcome_name='7:1'/>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:194' product_outcome_id='318' product_outcome_name='7:2'/>
                        </mapping>
                    </mappings>
                </variant>
            </variant_descriptions>";
        var apiMarketDescriptions = DeserializerHelper.GetDeserializedApiMessage<variant_descriptions>(apiXml);

        Assert.Throws<ArgumentException>(() => new VariantDescriptionDto(apiMarketDescriptions.variant[0]));
    }

    [Fact]
    public void DeserializeWhenOutcomeNameNullThenReturnNullName()
    {
        const string apiXml =
            @"<variant_descriptions response_code='OK'>
                <variant id='sr:correct_score:bestof:12'>
                    <outcomes>
                        <outcome id='sr:correct_score:bestof:12:192' name='7:0'/>
                        <outcome id='sr:correct_score:bestof:12:193' name='7:1'/>
                        <outcome id='sr:correct_score:bestof:12:194' name='7:2'/>
                        <outcome id='sr:correct_score:bestof:12:195' name='7:3'/>
                        <outcome id='sr:correct_score:bestof:12:196' name='7:4'/>
                        <outcome id='sr:correct_score:bestof:12:197'/>
                    </outcomes>
                    <mappings>
                        <mapping product_id='3' product_ids='3' sport_id='sr:sport:22' market_id='374' product_market_id='455'>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:192' product_outcome_id='33' product_outcome_name='7:0'/>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:193' product_outcome_id='34' product_outcome_name='7:1'/>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:194' product_outcome_id='318' product_outcome_name='7:2'/>
                        </mapping>
                    </mappings>
                </variant>
            </variant_descriptions>";
        var apiMarketDescriptions = DeserializerHelper.GetDeserializedApiMessage<variant_descriptions>(apiXml);

        _dtoMd = new VariantDescriptionDto(apiMarketDescriptions.variant[0]);

        Assert.NotNull(_dtoMd);
        var faultyOutcome = _dtoMd.Outcomes.First(f => f.Id.Equals("sr:correct_score:bestof:12:197", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(faultyOutcome);
        Assert.Null(faultyOutcome.Name);
    }

    [Fact]
    public void DeserializeWhenOutcomeNameEmptyThenReturnEmptyName()
    {
        const string apiXml =
            @"<variant_descriptions response_code='OK'>
                <variant id='sr:correct_score:bestof:12'>
                    <outcomes>
                        <outcome id='sr:correct_score:bestof:12:192' name='7:0'/>
                        <outcome id='sr:correct_score:bestof:12:193' name='7:1'/>
                        <outcome id='sr:correct_score:bestof:12:194' name='7:2'/>
                        <outcome id='sr:correct_score:bestof:12:195' name='7:3'/>
                        <outcome id='sr:correct_score:bestof:12:196' name=''/>
                        <outcome id='sr:correct_score:bestof:12:197' name='7:5'/>
                    </outcomes>
                    <mappings>
                        <mapping product_id='3' product_ids='3' sport_id='sr:sport:22' market_id='374' product_market_id='455'>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:192' product_outcome_id='33' product_outcome_name='7:0'/>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:193' product_outcome_id='34' product_outcome_name='7:1'/>
                            <mapping_outcome outcome_id='sr:correct_score:bestof:12:194' product_outcome_id='318' product_outcome_name='7:2'/>
                        </mapping>
                    </mappings>
                </variant>
            </variant_descriptions>";
        var apiMarketDescriptions = DeserializerHelper.GetDeserializedApiMessage<variant_descriptions>(apiXml);

        _dtoMd = new VariantDescriptionDto(apiMarketDescriptions.variant[0]);

        Assert.NotNull(_dtoMd);
        var faultyOutcome = _dtoMd.Outcomes.First(f => f.Id.Equals("sr:correct_score:bestof:12:196", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(faultyOutcome);
        Assert.Empty(faultyOutcome.Name);
    }

    private static void ValidateMapping(variant_mappingsMapping msg, MarketMappingDto dto)
    {
        var ciMarketId = dto.MarketSubTypeId == null ? dto.MarketTypeId.ToString() : $"{dto.MarketTypeId}:{dto.MarketSubTypeId}";
        Assert.Equal(msg.product_market_id, ciMarketId);
        Assert.Equal(msg.sport_id, dto.SportId.ToString());
        Assert.Equal(msg.sov_template, dto.SovTemplate);
        Assert.Equal(msg.valid_for, dto.ValidFor);

        ValidateMappingProducerIds(msg, dto);
        ValidateMappingOutcomes(msg, dto);
    }

    private static void ValidateMappingProducerIds(variant_mappingsMapping msg, MarketMappingDto dto)
    {
        if (!string.IsNullOrEmpty(msg.product_ids))
        {
            var ids = msg.product_ids.Split(new[] { SdkInfo.MarketMappingProductsDelimiter }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
            Assert.Equal(ids.Count, dto.ProducerIds.Count());
            foreach (var id in ids)
            {
                Assert.True(dto.ProducerIds.Contains(id), $"Missing producerId:{id}.");
            }
        }
        else
        {
            Assert.Single(dto.ProducerIds);
            Assert.Equal(msg.product_id, dto.ProducerIds.First());
        }
    }

    private static void ValidateMappingOutcomes(variant_mappingsMapping msg, MarketMappingDto dto)
    {
        if (msg.mapping_outcome != null)
        {
            Assert.Equal(msg.mapping_outcome.Length, dto.OutcomeMappings.Count());

            for (var i = 0; i < msg.mapping_outcome.Length; i++)
            {
                ValidateMappingOutcome(msg.mapping_outcome[i], dto.OutcomeMappings.ToArray()[i]);
            }
        }
        else
        {
            Assert.Empty(dto.OutcomeMappings);
        }
    }

    private static void ValidateMappingOutcome(variant_mappingsMappingMapping_outcome msg, OutcomeMappingDto dto)
    {
        Assert.Equal(msg.outcome_id, dto.OutcomeId);
        Assert.Equal(msg.product_outcome_id, dto.ProducerOutcomeId);
        Assert.Equal(msg.product_outcome_name, dto.ProducerOutcomeName);
    }

    private static void ValidateOutcome(desc_variant_outcomesOutcome msg, OutcomeDescriptionDto dto)
    {
        Assert.Equal(msg.id, dto.Id);
        Assert.Equal(msg.name, dto.Name);
        Assert.Null(dto.Description);
    }
}

// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.ObjectModel;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using RMF = Sportradar.OddsFeed.SDK.Tests.Common.MessageFactoryRest;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InMarkets;

public class MarketDescriptionDtoTests
{
    private readonly desc_market _apiMd;
    private MarketDescriptionDto _dtoMd;

    public MarketDescriptionDtoTests()
    {
        _apiMd = RMF.GetDescMarket(10);
        _dtoMd = new MarketDescriptionDto(_apiMd);
    }

    [Fact]
    public void ConstructWithNullApiData_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new MarketDescriptionDto(null));
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
    public void MappedName()
    {
        Assert.Equal(_apiMd.name, _dtoMd.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("some-name")]
    [InlineData("Innings 1 to 5th top - {$competitor1} total")]
    [InlineData("{!mapnr} map - player with most deaths (incl. overtime)")]
    public void ValidNameIsMapped(string marketName)
    {
        _apiMd.name = marketName;

        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Equal(_apiMd.name, _dtoMd.Name);
    }

    [Fact]
    public void MappedDescription()
    {
        Assert.Equal(_apiMd.description, _dtoMd.Description);
    }

    [Fact]
    public void MappedIncludeOutcomesOfType()
    {
        Assert.Equal(_apiMd.includes_outcomes_of_type, _dtoMd.OutcomeType);
    }

    [Fact]
    public void MappedGroupsNull()
    {
        _apiMd.groups = null;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Null(_dtoMd.Groups);
    }

    [Fact]
    public void MappedGroupsEmpty()
    {
        _apiMd.groups = string.Empty;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Empty(_dtoMd.Groups);
    }

    [Fact]
    public void MappedGroupsSingle()
    {
        _apiMd.groups = "demogroup";
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Single(_dtoMd.Groups);
        Assert.Equal(_apiMd.groups, _dtoMd.Groups.First());
    }

    [Fact]
    public void MappedGroupsMultiple()
    {
        _apiMd.groups = "group1|group2|group3";
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Equal(3, _dtoMd.Groups.Count);
        Assert.Equal("group1", _dtoMd.Groups.ElementAt(0));
        Assert.Equal("group2", _dtoMd.Groups.ElementAt(1));
        Assert.Equal("group3", _dtoMd.Groups.ElementAt(2));
    }

    [Fact]
    public void MappedGroupsMultipleInvalidDelimiter()
    {
        _apiMd.groups = "group1-group2-group3";
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Single(_dtoMd.Groups);
        Assert.Equal(_apiMd.groups, _dtoMd.Groups.First());
    }

    [Fact]
    public void MappedAttributesNull()
    {
        _apiMd.attributes = null;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Null(_dtoMd.Attributes);
    }

    [Fact]
    public void MappedAttributesEmpty()
    {
        _apiMd.attributes = Array.Empty<attributesAttribute>();
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Empty(_dtoMd.Attributes);
    }

    [Fact]
    public void MappedAttributesSingle()
    {
        var attribute1 = new attributesAttribute { name = "some-attribute-name", description = "some-description" };
        _apiMd.attributes = new[] { attribute1 };
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Single(_dtoMd.Attributes);
        Assert.Equal(attribute1.name, _dtoMd.Attributes.First().Name);
        Assert.Equal(attribute1.description, _dtoMd.Attributes.First().Description);
    }

    [Fact]
    public void MappedAttributesMultiple()
    {
        var attribute1 = new attributesAttribute { name = "some-attribute-name-1", description = "some-description-1" };
        var attribute2 = new attributesAttribute { name = "some-attribute-name-2", description = "some-description-2" };
        var attribute3 = new attributesAttribute { name = "some-attribute-name-3", description = "some-description-3" };
        _apiMd.attributes = new[] { attribute1, attribute2, attribute3 };
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Equal(3, _dtoMd.Attributes.Count);
        Assert.Equal(attribute1.name, _dtoMd.Attributes.ElementAt(0).Name);
        Assert.Equal(attribute1.description, _dtoMd.Attributes.ElementAt(0).Description);
        Assert.Equal(attribute2.name, _dtoMd.Attributes.ElementAt(1).Name);
        Assert.Equal(attribute2.description, _dtoMd.Attributes.ElementAt(1).Description);
        Assert.Equal(attribute3.name, _dtoMd.Attributes.ElementAt(2).Name);
        Assert.Equal(attribute3.description, _dtoMd.Attributes.ElementAt(2).Description);
    }

    [Fact]
    public void MappedAttributes_AttributeNameIsNull()
    {
        var attribute1 = new attributesAttribute { name = null, description = "some-description" };
        _apiMd.attributes = new[] { attribute1 };
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Single(_dtoMd.Attributes);
        Assert.Null(_dtoMd.Attributes.First().Name);
        Assert.Equal(attribute1.description, _dtoMd.Attributes.First().Description);
    }

    [Fact]
    public void MappedAttributes_AttributeDescriptionIsNull()
    {
        var attribute1 = new attributesAttribute { name = "some-attribute-name", description = null };
        _apiMd.attributes = new[] { attribute1 };
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Single(_dtoMd.Attributes);
        Assert.Equal(attribute1.name, _dtoMd.Attributes.First().Name);
        Assert.Null(_dtoMd.Attributes.First().Description);
    }

    [Fact]
    public void MappedMappingsNull()
    {
        _apiMd.mappings = null;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Null(_dtoMd.Mappings);
    }

    [Fact]
    public void MappedMappingsEmpty()
    {
        _apiMd.mappings = Array.Empty<mappingsMapping>();
        _dtoMd = new MarketDescriptionDto(_apiMd);

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
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Null(_dtoMd.Outcomes);
    }

    [Fact]
    public void MappedOutcomesEmpty()
    {
        _apiMd.outcomes = Array.Empty<desc_outcomesOutcome>();
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Empty(_dtoMd.Outcomes);
    }

    [Fact]
    public void MappedOutcomes()
    {
        Assert.Equal(_apiMd.outcomes.Length, _dtoMd.Outcomes.Count);

        for (var i = 0; i < _apiMd.outcomes.Length; i++)
        {
            ValidateOutcome(_apiMd.outcomes[i], _dtoMd.Outcomes.ToArray()[i]);
        }
    }

    [Fact]
    public void MappedSpecifiersNull()
    {
        _apiMd.specifiers = null;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Null(_dtoMd.Specifiers);
    }

    [Fact]
    public void MappedSpecifiersEmpty()
    {
        _apiMd.specifiers = Array.Empty<desc_specifiersSpecifier>();
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Empty(_dtoMd.Specifiers);
    }

    [Fact]
    public void MappedSpecifiers()
    {
        Assert.Equal(_apiMd.specifiers.Length, _dtoMd.Specifiers.Count);

        for (var i = 0; i < _apiMd.specifiers.Length; i++)
        {
            ValidateSpecifier(_apiMd.specifiers[i], _dtoMd.Specifiers.ToArray()[i]);
        }
    }

    [Fact]
    public void MappedVariantNull()
    {
        _apiMd.variant = null;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Null(_dtoMd.Variant);
    }

    [Fact]
    public void MappedVariantEmpty()
    {
        _apiMd.variant = string.Empty;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Empty(_dtoMd.Variant);
    }

    [Fact]
    public void MappedVariant()
    {
        _apiMd.variant = "some-variant";
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Equal(_apiMd.variant, _dtoMd.Variant);
    }

    [Fact]
    public void MappedOutcomeTypeNullAndIncludesOutcomesOfTypeNull()
    {
        _apiMd.outcome_type = null;
        _apiMd.includes_outcomes_of_type = null;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Null(_dtoMd.OutcomeType);
    }

    [Fact]
    public void MappedOutcomeTypeEmptyAndIncludesOutcomesOfTypeNull()
    {
        _apiMd.outcome_type = string.Empty;
        _apiMd.includes_outcomes_of_type = null;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Empty(_dtoMd.OutcomeType);
    }

    [Fact]
    public void MappedOutcomeTypeValidAndIncludesOutcomesOfTypeNull()
    {
        _apiMd.outcome_type = "some-type";
        _apiMd.includes_outcomes_of_type = null;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Equal(_apiMd.outcome_type, _dtoMd.OutcomeType);
    }

    [Fact]
    public void MappedOutcomeTypeNotEmptyAndIncludesOutcomesOfTypeNotEmpty()
    {
        _apiMd.outcome_type = "some-type";
        _apiMd.includes_outcomes_of_type = "some-includes-outcomes";
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Equal(_apiMd.outcome_type, _dtoMd.OutcomeType);
    }

    [Fact]
    public void MappedOutcomeTypeNullAndIncludesOutcomesOfTypeIsOutcomeTextVariantValue()
    {
        _apiMd.outcome_type = null;
        _apiMd.includes_outcomes_of_type = SdkInfo.OutcomeTextVariantValue;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Equal(SdkInfo.FreeTextVariantValue, _dtoMd.OutcomeType);
    }

    [Fact]
    public void MappedOutcomeTypeNullAndIncludesOutcomesOfTypesWithSrPrefix()
    {
        _apiMd.outcome_type = null;
        _apiMd.includes_outcomes_of_type = "sr:player";
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Equal("player", _dtoMd.OutcomeType);
    }

    [Fact]
    public void MappedOutcomeTypeNullAndIncludesOutcomesOfTypesIsUnknown()
    {
        _apiMd.outcome_type = null;
        _apiMd.includes_outcomes_of_type = "custom-type";
        _dtoMd = new MarketDescriptionDto(_apiMd);

        Assert.Null(_dtoMd.OutcomeType);
    }

    [Fact]
    public void OverrideIdWithPositiveId()
    {
        const int newId = 123456;

        _dtoMd.OverrideId(newId);

        Assert.Equal(newId, _dtoMd.Id);
    }

    [Fact]
    public void OverrideIdWithZeroId()
    {
        const int newId = 0;

        _dtoMd.OverrideId(newId);

        Assert.Equal(_apiMd.id, _dtoMd.Id);
    }

    [Fact]
    public void OverrideIdWithNegativeId()
    {
        const int newId = -1;

        _dtoMd.OverrideId(newId);

        Assert.Equal(_apiMd.id, _dtoMd.Id);
    }

    [Fact]
    public void OverrideGroupsWitNewGroups()
    {
        var newGroups = new Collection<string>() { "newGroup1" };

        _dtoMd.OverrideGroups(newGroups);

        Assert.Equal(newGroups.Count, _dtoMd.Groups.Count);
        Assert.Equal(newGroups[0], _dtoMd.Groups.First());
    }

    [Fact]
    public void OverrideGroupsWithNullGroups()
    {
        _apiMd.groups = "group1|group2|group3";
        _dtoMd = new MarketDescriptionDto(_apiMd);
        Assert.Equal(3, _dtoMd.Groups.Count);

        _dtoMd.OverrideGroups(null);

        Assert.Null(_dtoMd.Groups);
    }

    [Fact]
    public void OverrideGroupsWithEmptyGroups()
    {
        _apiMd.groups = "group1|group2|group3";
        _dtoMd = new MarketDescriptionDto(_apiMd);
        Assert.Equal(3, _dtoMd.Groups.Count);

        _dtoMd.OverrideGroups(Array.Empty<string>());

        Assert.Empty(_dtoMd.Groups);
    }

    [Fact]
    public void DeserializeValidXml()
    {
        const string apiXml =
            @"<market_descriptions response_code='OK'>
                <market id='282' name='Innings 1 to 5th top - {$competitor1} total' groups='all|score|4.5_innings'>
                    <outcomes>
                        <outcome id='13' name='under {total}'/>
                        <outcome id='12' name='over {total}'/>
                    </outcomes>
                    <specifiers>
                        <specifier name='total' type='decimal'/>
                    </specifiers>
                    <mappings>
                        <mapping product_id='1' product_ids='1|4' sport_id='sr:sport:3' market_id='8:232' sov_template='{total}'>
                            <mapping_outcome outcome_id='13' product_outcome_id='2528' product_outcome_name='under'/>
                            <mapping_outcome outcome_id='12' product_outcome_id='2530' product_outcome_name='over'/>
                        </mapping>
                    </mappings>
                </market>
            </market_descriptions>";
        var apiMarketDescriptions = DeserializerHelper.GetDeserializedApiMessage<market_descriptions>(apiXml);

        _dtoMd = new MarketDescriptionDto(apiMarketDescriptions.market[0]);

        Assert.NotNull(_dtoMd);
    }

    [Fact]
    public void DeserializeWhenMarketNameNotPresent()
    {
        const string apiXml =
            @"<market_descriptions response_code='OK'>
                <market id='282' groups='all|score|4.5_innings'>
                    <outcomes>
                        <outcome id='13' name='under {total}'/>
                        <outcome id='12' name='over {total}'/>
                    </outcomes>
                    <specifiers>
                        <specifier name='total' type='decimal'/>
                    </specifiers>
                </market>
            </market_descriptions>";
        var apiMarketDescriptions = DeserializerHelper.GetDeserializedApiMessage<market_descriptions>(apiXml);

        _dtoMd = new MarketDescriptionDto(apiMarketDescriptions.market[0]);

        Assert.NotNull(_dtoMd);
        Assert.Null(_dtoMd.Name);
    }

    [Fact]
    public void DeserializeWhenMarketNameEmpty()
    {
        const string apiXml =
            @"<market_descriptions response_code='OK'>
                <market id='282' name='' groups='all|score|4.5_innings'>
                    <outcomes>
                        <outcome id='13' name='under {total}'/>
                        <outcome id='12' name='over {total}'/>
                    </outcomes>
                    <specifiers>
                        <specifier name='total' type='decimal'/>
                    </specifiers>
                </market>
            </market_descriptions>";
        var apiMarketDescriptions = DeserializerHelper.GetDeserializedApiMessage<market_descriptions>(apiXml);

        _dtoMd = new MarketDescriptionDto(apiMarketDescriptions.market[0]);

        Assert.NotNull(_dtoMd);
        Assert.Empty(_dtoMd.Name);
    }

    [Fact]
    public void DeserializeWhenOutcomeNameNotPresent()
    {
        const string apiXml =
            @"<market_descriptions response_code='OK'>
                <market id='282' name='Innings 1 to 5th top - {$competitor1} total' groups='all|score|4.5_innings'>
                    <outcomes>
                        <outcome id='13' name='under {total}'/>
                        <outcome id='12'/>
                    </outcomes>
                    <specifiers>
                        <specifier name='total' type='decimal'/>
                    </specifiers>
                </market>
            </market_descriptions>";
        var apiMarketDescriptions = DeserializerHelper.GetDeserializedApiMessage<market_descriptions>(apiXml);

        _dtoMd = new MarketDescriptionDto(apiMarketDescriptions.market[0]);

        Assert.NotNull(_dtoMd);
        Assert.Null(_dtoMd.Outcomes.ElementAt(1).Name);
    }

    [Fact]
    public void DeserializeWhenOutcomeNameMissing()
    {
        const string apiXml =
            @"<market_descriptions response_code='OK'>
                <market id='282' name='Innings 1 to 5th top - {$competitor1} total' groups='all|score|4.5_innings'>
                    <outcomes>
                        <outcome id='13' name='under {total}'/>
                        <outcome id='12' name=''/>
                    </outcomes>
                    <specifiers>
                        <specifier name='total' type='decimal'/>
                    </specifiers>
                </market>
            </market_descriptions>";
        var apiMarketDescriptions = DeserializerHelper.GetDeserializedApiMessage<market_descriptions>(apiXml);

        _dtoMd = new MarketDescriptionDto(apiMarketDescriptions.market[0]);

        Assert.NotNull(_dtoMd);
        Assert.Empty(_dtoMd.Outcomes.ElementAt(1).Name);
    }

    private static void ValidateMapping(mappingsMapping msg, MarketMappingDto dto)
    {
        var ciMarketId = dto.MarketSubTypeId == null ? dto.MarketTypeId.ToString() : $"{dto.MarketTypeId}:{dto.MarketSubTypeId}";
        Assert.Equal(msg.market_id, ciMarketId);
        Assert.Equal(msg.sport_id, dto.SportId.ToString());
        Assert.Equal(msg.sov_template, dto.SovTemplate);
        Assert.Equal(msg.valid_for, dto.ValidFor);

        ValidateMappingProducerIds(msg, dto);
        ValidateMappingOutcomes(msg, dto);
    }

    private static void ValidateMappingProducerIds(mappingsMapping msg, MarketMappingDto dto)
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

    private static void ValidateMappingOutcomes(mappingsMapping msg, MarketMappingDto dto)
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

    private static void ValidateMappingOutcome(mappingsMappingMapping_outcome msg, OutcomeMappingDto dto)
    {
        Assert.Equal(msg.outcome_id, dto.OutcomeId);
        Assert.Equal(msg.product_outcome_id, dto.ProducerOutcomeId);
        Assert.Equal(msg.product_outcome_name, dto.ProducerOutcomeName);
    }

    private static void ValidateOutcome(desc_outcomesOutcome msg, OutcomeDescriptionDto dto)
    {
        Assert.Equal(msg.id, dto.Id);
        Assert.Equal(msg.name, dto.Name);
        Assert.Equal(msg.description, dto.Description);
    }

    private static void ValidateSpecifier(desc_specifiersSpecifier msg, SpecifierDto dto)
    {
        Assert.Equal(msg.type, dto.Type);
        Assert.Equal(msg.name, dto.Name);
    }
}

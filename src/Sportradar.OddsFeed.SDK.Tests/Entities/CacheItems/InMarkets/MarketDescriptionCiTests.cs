// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using RMF = Sportradar.OddsFeed.SDK.Tests.Common.MessageFactoryRest;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InMarkets;

public class MarketDescriptionCiTests
{
    private readonly desc_market _apiMd;
    private MarketDescriptionDto _dtoMd;
    private MarketDescriptionCacheItem _ciMd;
    private readonly IMappingValidatorFactory _mappingValidatorFactory;
    private const string CacheItemSource = "TestSource";

    public MarketDescriptionCiTests()
    {
        _mappingValidatorFactory = new MappingValidatorFactory();
        _apiMd = RMF.GetDescMarket(10);
        _dtoMd = new MarketDescriptionDto(_apiMd);
        _ciMd = MarketDescriptionCacheItem.Build(_dtoMd, _mappingValidatorFactory, ScheduleData.CultureEn, CacheItemSource);
    }

    [Fact]
    public void ConstructionWhenValidDtoThenSucceeds()
    {
        Assert.NotNull(_ciMd);
    }

    [Fact]
    public void ConstructWhenNullDtoThenThrows()
    {
        _ = Assert.Throws<ArgumentNullException>(() => MarketDescriptionCacheItem.Build(null, _mappingValidatorFactory, ScheduleData.CultureEn, CacheItemSource));
    }

    [Fact]
    public void ConstructWhenNullMappingValidatorThenThrows()
    {
        _ = Assert.Throws<ArgumentNullException>(() => MarketDescriptionCacheItem.Build(_dtoMd, null, ScheduleData.CultureEn, CacheItemSource));
    }

    [Fact]
    public void ConstructWhenNullCultureThenThrows()
    {
        _ = Assert.Throws<ArgumentNullException>(() => MarketDescriptionCacheItem.Build(_dtoMd, _mappingValidatorFactory, null, CacheItemSource));
    }

    [Fact]
    public void ConstructionWhenNullSourceThenSucceeds()
    {
        _ciMd = MarketDescriptionCacheItem.Build(_dtoMd, _mappingValidatorFactory, ScheduleData.CultureEn, null);

        Assert.NotNull(_ciMd);
        Assert.Null(_ciMd.SourceCache);
    }

    [Fact]
    public void ConstructionWhenEmptySourceThenSucceeds()
    {
        _ciMd = MarketDescriptionCacheItem.Build(_dtoMd, _mappingValidatorFactory, ScheduleData.CultureEn, string.Empty);

        Assert.NotNull(_ciMd);
        Assert.Empty(_ciMd.SourceCache);
    }

    [Fact]
    public void IdIsMapped()
    {
        Assert.Equal(_apiMd.id, _ciMd.Id);
    }

    [Fact]
    public void NameIsMapped()
    {
        Assert.Equal(_apiMd.name, _ciMd.GetName(ScheduleData.CultureEn));
    }

    [Fact]
    public void NameForDifferentLanguageDoesNotExists()
    {
        Assert.Null(_ciMd.GetName(ScheduleData.CultureDe));
    }

    [Fact]
    public void WhenInputNullName()
    {
        _apiMd.name = null;

        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Null(_ciMd.GetName(ScheduleData.CultureEn));
    }

    [Fact]
    public void WhenInputEmptyName()
    {
        _apiMd.name = string.Empty;

        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Empty(_ciMd.GetName(ScheduleData.CultureEn));
    }

    [Fact]
    public void WhenRequestingNameWithNullCultureThenThrows()
    {
        _ = Assert.Throws<ArgumentNullException>(() => _ciMd.GetName(null));
    }

    [Fact]
    public void DescriptionIsMapped()
    {
        Assert.Equal(_apiMd.description, _ciMd.GetDescription(ScheduleData.CultureEn));
    }

    [Fact]
    public void WhenRequestingDescriptionForDifferentLanguageWhichDoesNotExistsThenReturnNull()
    {
        Assert.Null(_ciMd.GetDescription(ScheduleData.CultureDe));
    }

    [Fact]
    public void WhenInputNullDescriptionThenReturnNull()
    {
        _apiMd.description = null;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Null(_ciMd.GetDescription(ScheduleData.CultureEn));
    }

    [Fact]
    public void WhenInputEmptyDescriptionThenReturnsNull()
    {
        _apiMd.description = string.Empty;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Null(_ciMd.GetDescription(ScheduleData.CultureEn));
    }

    [Fact]
    public void WhenRequestingDescriptionWithNullCultureThenThrows()
    {
        _ = Assert.Throws<ArgumentNullException>(() => _ciMd.GetDescription(null));
    }

    [Fact]
    public void WhenInputIncludeOutcomesOfTypeSpecified()
    {
        Assert.Equal(_apiMd.includes_outcomes_of_type, _ciMd.OutcomeType);
    }

    [Fact]
    public void WhenInputGroupsNull()
    {
        _apiMd.groups = null;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Null(_ciMd.Groups);
    }

    [Fact]
    public void WhenInputGroupsEmpty()
    {
        _apiMd.groups = string.Empty;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Empty(_ciMd.Groups);
    }

    [Fact]
    public void WhenInputSingleGroups()
    {
        _apiMd.groups = "demogroup";
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        _ = Assert.Single(_ciMd.Groups);
        Assert.Equal(_dtoMd.Groups.First(), _ciMd.Groups.First());
    }

    [Fact]
    public void WhenInputMultipleGroups()
    {
        _apiMd.groups = "group1|group2|group3";
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Equal(3, _ciMd.Groups.Count);
        Assert.Equal("group1", _ciMd.Groups.ElementAt(0));
        Assert.Equal("group2", _ciMd.Groups.ElementAt(1));
        Assert.Equal("group3", _ciMd.Groups.ElementAt(2));
    }

    [Fact]
    public void WhenInputAttributesIsNull()
    {
        _apiMd.attributes = null;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Null(_ciMd.Attributes);
    }

    [Fact]
    public void WhenInputAttributesIsEmptyArray()
    {
        _apiMd.attributes = Array.Empty<attributesAttribute>();
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Empty(_ciMd.Attributes);
    }

    [Fact]
    public void WhenInputSingleAttribute()
    {
        var attribute1 = new attributesAttribute { name = "some-attribute-name", description = "some-description" };
        _apiMd.attributes = new[] { attribute1 };
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        _ = Assert.Single(_ciMd.Attributes);
        Assert.Equal(attribute1.name, _ciMd.Attributes.First().Name);
        Assert.Equal(attribute1.description, _ciMd.Attributes.First().Description);
    }

    [Fact]
    public void WhenInputMultipleAttributes()
    {
        var attribute1 = new attributesAttribute { name = "some-attribute-name-1", description = "some-description-1" };
        var attribute2 = new attributesAttribute { name = "some-attribute-name-2", description = "some-description-2" };
        var attribute3 = new attributesAttribute { name = "some-attribute-name-3", description = "some-description-3" };
        _apiMd.attributes = new[] { attribute1, attribute2, attribute3 };
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Equal(3, _ciMd.Attributes.Count);
        Assert.Equal(attribute1.name, _ciMd.Attributes.ElementAt(0).Name);
        Assert.Equal(attribute1.description, _ciMd.Attributes.ElementAt(0).Description);
        Assert.Equal(attribute2.name, _ciMd.Attributes.ElementAt(1).Name);
        Assert.Equal(attribute2.description, _ciMd.Attributes.ElementAt(1).Description);
        Assert.Equal(attribute3.name, _ciMd.Attributes.ElementAt(2).Name);
        Assert.Equal(attribute3.description, _ciMd.Attributes.ElementAt(2).Description);
    }

    [Fact]
    public void WhenInputAttributesWithAttributeNameIsNull()
    {
        var attribute1 = new attributesAttribute { name = null, description = "some-description" };
        _apiMd.attributes = new[] { attribute1 };
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        _ = Assert.Single(_ciMd.Attributes);
        Assert.Null(_ciMd.Attributes.First().Name);
        Assert.Equal(attribute1.description, _ciMd.Attributes.First().Description);
    }

    [Fact]
    public void WhenInputAttributesAndAttributeDescriptionIsNull()
    {
        var attribute1 = new attributesAttribute { name = "some-attribute-name", description = null };
        _apiMd.attributes = new[] { attribute1 };
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        _ = Assert.Single(_ciMd.Attributes);
        Assert.Equal(attribute1.name, _ciMd.Attributes.First().Name);
        Assert.Null(_ciMd.Attributes.First().Description);
    }

    [Fact]
    public void WhenInputMappingsNull()
    {
        _apiMd.mappings = null;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Null(_ciMd.Mappings);
    }

    [Fact]
    public void WhenInputMappingsEmpty()
    {
        _apiMd.mappings = Array.Empty<mappingsMapping>();
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Empty(_ciMd.Mappings);
    }

    [Fact]
    public void WhenInputMappings()
    {
        Assert.Equal(_apiMd.mappings.Length, _ciMd.Mappings.Count);

        for (var i = 0; i < _apiMd.mappings.Length; i++)
        {
            ValidateMapping(_apiMd.mappings[i], _ciMd.Mappings.ElementAt(i), ScheduleData.CultureEn);
        }
    }

    [Fact]
    public void WhenInputOutcomesNull()
    {
        _apiMd.outcomes = null;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Null(_ciMd.Outcomes);
    }

    [Fact]
    public void WhenInputOutcomesEmpty()
    {
        _apiMd.outcomes = Array.Empty<desc_outcomesOutcome>();
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Empty(_ciMd.Outcomes);
    }

    [Fact]
    public void WhenInputOutcomes()
    {
        Assert.Equal(_apiMd.outcomes.Length, _ciMd.Outcomes.Count);

        for (var i = 0; i < _apiMd.outcomes.Length; i++)
        {
            ValidateOutcome(_apiMd.outcomes[i], _ciMd.Outcomes.ToArray()[i], ScheduleData.CultureEn);
        }
    }

    [Fact]
    public void WhenInputSpecifiersNull()
    {
        _apiMd.specifiers = null;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Null(_ciMd.Specifiers);
    }

    [Fact]
    public void MappedSpecifiersEmpty()
    {
        _apiMd.specifiers = Array.Empty<desc_specifiersSpecifier>();
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Empty(_ciMd.Specifiers);
    }

    [Fact]
    public void WhenInputSpecifiers()
    {
        Assert.Equal(_apiMd.specifiers.Length, _ciMd.Specifiers.Count);

        for (var i = 0; i < _apiMd.specifiers.Length; i++)
        {
            ValidateSpecifier(_apiMd.specifiers[i], _ciMd.Specifiers.ToArray()[i]);
        }
    }

    [Fact]
    public void WhenInputVariantNull()
    {
        _apiMd.variant = null;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Null(_ciMd.Variant);
    }

    [Fact]
    public void WhenInputVariantEmpty()
    {
        _apiMd.variant = string.Empty;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Empty(_ciMd.Variant);
    }

    [Fact]
    public void WhenInputVariant()
    {
        _apiMd.variant = "some-variant";
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Equal(_apiMd.variant, _ciMd.Variant);
    }

    [Fact]
    public void WhenInputOutcomeTypeNullAndIncludesOutcomesOfTypeNull()
    {
        _apiMd.outcome_type = null;
        _apiMd.includes_outcomes_of_type = null;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Null(_ciMd.OutcomeType);
    }

    [Fact]
    public void WhenInputOutcomeTypeEmptyAndIncludesOutcomesOfTypeNull()
    {
        _apiMd.outcome_type = string.Empty;
        _apiMd.includes_outcomes_of_type = null;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Empty(_ciMd.OutcomeType);
    }

    [Fact]
    public void WhenInputOutcomeTypeValidAndIncludesOutcomesOfTypeNull()
    {
        _apiMd.outcome_type = "some-type";
        _apiMd.includes_outcomes_of_type = null;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Equal(_apiMd.outcome_type, _ciMd.OutcomeType);
    }

    [Fact]
    public void WhenInputOutcomeTypeNotEmptyAndIncludesOutcomesOfTypeNotEmpty()
    {
        _apiMd.outcome_type = "some-type";
        _apiMd.includes_outcomes_of_type = "some-includes-outcomes";
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Equal(_apiMd.outcome_type, _ciMd.OutcomeType);
    }

    [Fact]
    public void WhenInputOutcomeTypeNullAndIncludesOutcomesOfTypeIsOutcomeTextVariantValue()
    {
        _apiMd.outcome_type = null;
        _apiMd.includes_outcomes_of_type = SdkInfo.OutcomeTextVariantValue;
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Equal(SdkInfo.FreeTextVariantValue, _ciMd.OutcomeType);
    }

    [Fact]
    public void WhenInputOutcomeTypeNullAndIncludesOutcomesOfTypesWithSrPrefix()
    {
        _apiMd.outcome_type = null;
        _apiMd.includes_outcomes_of_type = "sr:player";
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Equal("player", _ciMd.OutcomeType);
    }

    [Fact]
    public void WhenInputOutcomeTypeNullAndIncludesOutcomesOfTypesIsUnknown()
    {
        _apiMd.outcome_type = null;
        _apiMd.includes_outcomes_of_type = "custom-type";
        _dtoMd = new MarketDescriptionDto(_apiMd);

        _ciMd = BuildMarketDescriptionCacheItem(_dtoMd);

        Assert.Null(_ciMd.OutcomeType);
    }

    [Fact]
    public void HasTranslationsForReturnsTrueForExisting()
    {
        Assert.True(_ciMd.HasTranslationsFor(ScheduleData.CultureEn));
    }

    [Fact]
    public void HasTranslationsForReturnsFalseForNonExisting()
    {
        Assert.False(_ciMd.HasTranslationsFor(ScheduleData.CultureDe));
    }

    [Fact]
    public void HasTranslationsWhenNullLanguageThenThrow()
    {
        Assert.Throws<ArgumentNullException>(() => _ciMd.HasTranslationsFor(null));
    }

    [Fact]
    public void SetFetchInfoOverridesSource()
    {
        const string newSource = "some-new-source";

        _ciMd.SetFetchInfo(newSource);

        Assert.Equal(newSource, _ciMd.SourceCache);
    }

    [Fact]
    public void SetFetchInfoWhenSourceEmptyThenDoNotOverride()
    {
        _ciMd.SetFetchInfo(string.Empty);

        Assert.Equal(CacheItemSource, _ciMd.SourceCache);
    }

    [Fact]
    public void SetFetchInfoWhenSourceNullThenDoNotOverride()
    {
        _ciMd.SetFetchInfo(null);

        Assert.Equal(CacheItemSource, _ciMd.SourceCache);
    }

    [Collection(NonParallelCollectionFixture.NonParallelTestCollection)]
    public class MergeTests
    {
        private readonly desc_market _apiMd1;
        private readonly desc_market _apiMd2;
        private MarketDescriptionDto _dtoMd1;
        private MarketDescriptionDto _dtoMd2;
        private MarketDescriptionCacheItem _ciMd;
        private readonly IMappingValidatorFactory _mappingValidatorFactory;

        public MergeTests()
        {
            var mdXml1 = @"<market_descriptions response_code='OK'>
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
            var mdXml2 = @"<market_descriptions response_code='OK'>
                            <market id='282' name='Innings 1 bis Anfang 5tes - {$competitor1} Total' groups='all|score|4.5_innings'>
                                <outcomes>
                                    <outcome id='13' name='unter {total}'/>
                                    <outcome id='12' name='über {total}'/>
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
            _mappingValidatorFactory = new MappingValidatorFactory();
            _apiMd1 = DeserializerHelper.GetDeserializedApiMessage<market_descriptions>(mdXml1).market[0];
            _apiMd2 = DeserializerHelper.GetDeserializedApiMessage<market_descriptions>(mdXml2).market[0];
            _dtoMd1 = new MarketDescriptionDto(_apiMd1);
            _dtoMd2 = new MarketDescriptionDto(_apiMd2);
            _ciMd = MarketDescriptionCacheItem.Build(_dtoMd1, _mappingValidatorFactory, ScheduleData.CultureEn, CacheItemSource);
        }

        [Fact]

        public void InitialSetupHasOneLanguage()
        {
            Assert.NotNull(_ciMd);
            Assert.Equal(_apiMd1.name, _ciMd.GetName(ScheduleData.CultureEn));
            Assert.Null(_ciMd.GetName(ScheduleData.CultureDe));
            Assert.Equal(2, _ciMd.Outcomes.Count);
            Assert.Equal(_apiMd1.outcomes[0].name, _ciMd.Outcomes.First().GetName(ScheduleData.CultureEn));
            Assert.Null(_ciMd.Outcomes.First().GetName(ScheduleData.CultureDe));
        }

        [Fact]
        public void MergeWhenDtoNullThenThrow()
        {
            _ = Assert.Throws<ArgumentNullException>(() => _ciMd.Merge(null, ScheduleData.CultureDe));
        }

        [Fact]
        public void MergeWhenLanguageNullThenThrow()
        {
            _ = Assert.Throws<ArgumentNullException>(() => _ciMd.Merge(_dtoMd2, null));
        }

        [Fact]
        public void AddingNewDataIsMerged()
        {
            _ = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.Equal(_apiMd1.name, _ciMd.GetName(ScheduleData.CultureEn));
            Assert.Equal(_apiMd2.name, _ciMd.GetName(ScheduleData.CultureDe));
            Assert.Equal(2, _ciMd.Outcomes.Count);
            Assert.Equal(_apiMd1.outcomes[0].name, _ciMd.Outcomes.First().GetName(ScheduleData.CultureEn));
            Assert.Equal(_apiMd2.outcomes[0].name, _ciMd.Outcomes.First().GetName(ScheduleData.CultureDe));
            _ = Assert.Single(_ciMd.Mappings);
            Assert.Equal(2, _ciMd.Mappings.First().OutcomeMappings.Count);
        }

        [Fact]
        public void SuccessfulMergeThenReportsNoProblem()
        {
            var mergeResult = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.True(mergeResult.IsAllMerged());
        }

        [Fact]
        public void FetchedLanguagesWhenMergingThenLanguageIsAdded()
        {
            _ = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.Equal(2, _ciMd.Names.Keys.Count);
        }

        [Fact]
        public void MergingEmptyNameIsSaved()
        {
            _apiMd2.name = string.Empty;
            _dtoMd2 = new MarketDescriptionDto(_apiMd2);

            _ = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.Equal(_apiMd1.name, _ciMd.GetName(ScheduleData.CultureEn));
            Assert.Empty(_ciMd.GetName(ScheduleData.CultureDe));
        }

        [Fact]
        public void MergingNewDescriptionIsSaved()
        {
            _apiMd2.description = "new-description-de";
            _dtoMd2 = new MarketDescriptionDto(_apiMd2);

            _ = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.Equal(_apiMd1.description, _ciMd.GetDescription(ScheduleData.CultureEn));
            Assert.Equal(_apiMd2.description, _ciMd.GetDescription(ScheduleData.CultureDe));
        }

        [Fact]
        public void MergingEmptyDescriptionIsNotSavedThenReturnsNull()
        {
            _apiMd2.description = string.Empty;
            _dtoMd2 = new MarketDescriptionDto(_apiMd2);

            _ = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.Equal(_apiMd1.description, _ciMd.GetDescription(ScheduleData.CultureEn));
            Assert.Null(_ciMd.GetDescription(ScheduleData.CultureDe));
        }

        [Fact]
        public void MergingVariantOverridesPreexistingValue()
        {
            _apiMd1.variant = "some-variant";
            _dtoMd1 = new MarketDescriptionDto(_apiMd1);
            _ciMd = MarketDescriptionCacheItem.Build(_dtoMd1, _mappingValidatorFactory, ScheduleData.CultureEn, CacheItemSource);

            _apiMd2.variant = string.Empty;
            _dtoMd2 = new MarketDescriptionDto(_apiMd2);

            _ = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.Empty(_ciMd.Variant);
        }

        [Fact]
        public void MergingOutcomeTypeOverridesPreexistingValue()
        {
            _apiMd1.outcome_type = "some-outcome";
            _dtoMd1 = new MarketDescriptionDto(_apiMd1);
            _ciMd = MarketDescriptionCacheItem.Build(_dtoMd1, _mappingValidatorFactory, ScheduleData.CultureEn, CacheItemSource);

            _apiMd2.outcome_type = string.Empty;
            _dtoMd2 = new MarketDescriptionDto(_apiMd2);

            _ = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.Empty(_ciMd.OutcomeType);
        }

        [Fact]
        public void MergingGroupsOverridesPreexistingValue()
        {
            _apiMd1.groups = "group1|group2|group3";
            _dtoMd1 = new MarketDescriptionDto(_apiMd1);
            _ciMd = MarketDescriptionCacheItem.Build(_dtoMd1, _mappingValidatorFactory, ScheduleData.CultureEn, CacheItemSource);
            Assert.Equal(3, _ciMd.Groups.Count);

            _apiMd2.groups = string.Empty;
            _dtoMd2 = new MarketDescriptionDto(_apiMd2);

            _ = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.Empty(_ciMd.Groups);
        }

        [Fact]
        public void MergingOutcomesNormal()
        {
            _ = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.Equal(_apiMd1.outcomes.Length, _ciMd.Outcomes.Count);

            for (var i = 0; i < _apiMd1.outcomes.Length; i++)
            {
                ValidateOutcome(_apiMd1.outcomes[i], _ciMd.Outcomes.ElementAt(i), ScheduleData.CultureEn);
            }

            for (var i = 0; i < _apiMd1.outcomes.Length; i++)
            {
                ValidateOutcome(_apiMd2.outcomes[i], _ciMd.Outcomes.ElementAt(i), ScheduleData.CultureDe);
            }
        }

        [Fact]
        public void MergingOutcomesWhenMissingOutcomesDataThenDoNothing()
        {
            _apiMd2.outcomes = null;
            _dtoMd2 = new MarketDescriptionDto(_apiMd2);

            _ = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.Equal(_apiMd1.outcomes.Length, _ciMd.Outcomes.Count);

            for (var i = 0; i < _apiMd1.outcomes.Length; i++)
            {
                ValidateOutcome(_apiMd1.outcomes[i], _ciMd.Outcomes.ElementAt(i), ScheduleData.CultureEn);
                Assert.Null(_ciMd.Outcomes.ElementAt(i).GetName(ScheduleData.CultureDe));
            }
        }

        [Fact]
        public void MergingOutcomesWhenUnknownOutcomesDataThenIsNotMerged()
        {
            _apiMd2.outcomes[0].id = "123";
            _apiMd2.outcomes[1].id = "321";
            _dtoMd2 = new MarketDescriptionDto(_apiMd2);

            _ = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.Equal(_apiMd1.outcomes.Length, _ciMd.Outcomes.Count);

            for (var i = 0; i < _apiMd1.outcomes.Length; i++)
            {
                ValidateOutcome(_apiMd1.outcomes[i], _ciMd.Outcomes.ElementAt(i), ScheduleData.CultureEn);
                Assert.Null(_ciMd.Outcomes.ElementAt(i).GetName(ScheduleData.CultureDe));
            }
        }

        [Fact]
        public void MergingOutcomesWhenUnknownOutcomesDataThenProblemAreReported()
        {
            _apiMd2.outcomes[0].id = "123";
            _apiMd2.outcomes[1].id = "321";
            _dtoMd2 = new MarketDescriptionDto(_apiMd2);

            var mergeResult = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.False(mergeResult.IsAllMerged());
            Assert.Equal(2, mergeResult.GetOutcomeProblem().Count);
        }

        [Fact]
        public void MergingMappingsNormal()
        {
            _ = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.Equal(_apiMd1.mappings.Length, _ciMd.Mappings.Count);

            for (var i = 0; i < _apiMd1.mappings.Length; i++)
            {
                ValidateMapping(_apiMd1.mappings[i], _ciMd.Mappings.ElementAt(i), ScheduleData.CultureEn);
            }

            for (var i = 0; i < _apiMd1.mappings.Length; i++)
            {
                ValidateMapping(_apiMd2.mappings[i], _ciMd.Mappings.ElementAt(i), ScheduleData.CultureDe);
            }
        }

        [Fact]
        public void MergingMappingsWhenMissingMappingsDataThenDoNothing()
        {
            _apiMd2.mappings = null;
            _dtoMd2 = new MarketDescriptionDto(_apiMd2);

            _ = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.Equal(_apiMd1.mappings.Length, _ciMd.Mappings.Count);

            for (var i = 0; i < _apiMd1.mappings.Length; i++)
            {
                ValidateMapping(_apiMd1.mappings[i], _ciMd.Mappings.ElementAt(i), ScheduleData.CultureEn);
                Assert.True(_ciMd.Mappings.ElementAt(i).OutcomeMappings.All(a => a.ProducerOutcomeNames.Count == 1));
            }
        }

        [Fact]
        public void MergingMappingsWhenMissingMappingsDataThenNoProblemIsReported()
        {
            _apiMd2.mappings = null;
            _dtoMd2 = new MarketDescriptionDto(_apiMd2);

            var mergeResult = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.True(mergeResult.IsAllMerged());
        }

        [Fact]
        public void MergingMappingsWhenMarketIdDoNotMatchThenIsNotMerged()
        {
            _apiMd2.mappings[0].market_id = "18:232";
            _dtoMd2 = new MarketDescriptionDto(_apiMd2);

            _ = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.Equal(_apiMd1.mappings.Length, _ciMd.Mappings.Count);

            for (var i = 0; i < _apiMd1.mappings.Length; i++)
            {
                ValidateMapping(_apiMd1.mappings[i], _ciMd.Mappings.ElementAt(i), ScheduleData.CultureEn);
                Assert.True(_ciMd.Mappings.ElementAt(i).OutcomeMappings.All(a => a.ProducerOutcomeNames.Count == 1));
            }
        }

        [Fact]
        public void MergingMappingsWhenMarketIdDoNotMatchThenMergeProblemIsReported()
        {
            _apiMd2.mappings[0].market_id = "18:232";
            _dtoMd2 = new MarketDescriptionDto(_apiMd2);

            var mergeResult = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.False(mergeResult.IsAllMerged());
            _ = Assert.Single(mergeResult.GetMappingProblem());
        }

        [Fact]
        public void MergingMappingsWhenSportIdDoNotMatchThenIsNotMerged()
        {
            _apiMd2.mappings[0].sport_id = "sr:sport:123";
            _dtoMd2 = new MarketDescriptionDto(_apiMd2);

            _ = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            for (var i = 0; i < _apiMd1.mappings.Length; i++)
            {
                ValidateMapping(_apiMd1.mappings[i], _ciMd.Mappings.ElementAt(i), ScheduleData.CultureEn);
                Assert.True(_ciMd.Mappings.ElementAt(i).OutcomeMappings.All(a => a.ProducerOutcomeNames.Count == 1));
            }
        }

        [Fact]
        public void MergingMappingsWhenSportIdDoNotMatchThenProblemsIsReported()
        {
            _apiMd2.mappings[0].sport_id = "sr:sport:123";
            _dtoMd2 = new MarketDescriptionDto(_apiMd2);

            var mergeResult = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.False(mergeResult.IsAllMerged());
            _ = Assert.Single(mergeResult.GetMappingProblem());
        }

        [Fact]
        public void MergingMappingsWhenProducerIdsDoNotMatchThenIsNotMerged()
        {
            _apiMd2.mappings[0].product_ids = "1|7";
            _dtoMd2 = new MarketDescriptionDto(_apiMd2);

            _ = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            for (var i = 0; i < _apiMd1.mappings.Length; i++)
            {
                ValidateMapping(_apiMd1.mappings[i], _ciMd.Mappings.ElementAt(i), ScheduleData.CultureEn);
                Assert.True(_ciMd.Mappings.ElementAt(i).OutcomeMappings.All(a => a.ProducerOutcomeNames.Count == 1));
            }
        }

        [Fact]
        public void MergingMappingsWhenProducerIdsDoNotMatchThenProblemsIsReported()
        {
            _apiMd2.mappings[0].product_ids = "1|7";
            _dtoMd2 = new MarketDescriptionDto(_apiMd2);

            var mergeResult = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.False(mergeResult.IsAllMerged());
            _ = Assert.Single(mergeResult.GetMappingProblem());
        }

        [Fact]
        public void MergingMappingsWhenValidForDoNotMatchThenIsNotMerged()
        {
            _apiMd2.mappings[0].valid_for = "mapnr=1";
            _dtoMd2 = new MarketDescriptionDto(_apiMd2);

            _ = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            for (var i = 0; i < _apiMd1.mappings.Length; i++)
            {
                ValidateMapping(_apiMd1.mappings[i], _ciMd.Mappings.ElementAt(i), ScheduleData.CultureEn);
                Assert.True(_ciMd.Mappings.ElementAt(i).OutcomeMappings.All(a => a.ProducerOutcomeNames.Count == 1));
            }
        }

        [Fact]
        public void MergingMappingsWhenValidForDoNotMatchThenProblemsIsReported()
        {
            _apiMd2.mappings[0].valid_for = "mapnr=1";
            _dtoMd2 = new MarketDescriptionDto(_apiMd2);

            var mergeResult = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.False(mergeResult.IsAllMerged());
            _ = Assert.Single(mergeResult.GetMappingProblem());
        }

        [Fact]
        public void GetFaultyLanguagesWhenNormalMarketThenReturnEmptyList()
        {
            var faultyLanguages = _ciMd.GetFaultyLanguages();

            Assert.Empty(faultyLanguages);
        }

        [Fact]
        public void GetFaultyLanguagesWhenNormalMarketThenDoNotReturnNull()
        {
            var faultyLanguages = _ciMd.GetFaultyLanguages();

            Assert.NotNull(faultyLanguages);
        }

        [Fact]
        public void GetFaultyLanguagesWhenMissingMarketName()
        {
            _apiMd2.name = string.Empty;
            _dtoMd2 = new MarketDescriptionDto(_apiMd2);
            _ = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            var faultyLanguages = _ciMd.GetFaultyLanguages();

            _ = Assert.Single(faultyLanguages);
            Assert.Equal(ScheduleData.CultureDe, faultyLanguages.First());
        }

        [Fact]
        public void GetFaultyLanguagesWhenMissingOutcomeName()
        {
            _apiMd2.outcomes[1].name = string.Empty;
            _dtoMd2 = new MarketDescriptionDto(_apiMd2);
            _ = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            var faultyLanguages = _ciMd.GetFaultyLanguages();

            _ = Assert.Single(faultyLanguages);
            Assert.Equal(ScheduleData.CultureDe, faultyLanguages.First());
        }

        [Fact]
        public void GetFaultyLanguagesWhenOutcomeNameNotPresentForExpectedLanguage()
        {
            _apiMd2.outcomes = Array.Empty<desc_outcomesOutcome>();
            _dtoMd2 = new MarketDescriptionDto(_apiMd2);
            _ = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            var faultyLanguages = _ciMd.GetFaultyLanguages();

            _ = Assert.Single(faultyLanguages);
            Assert.Equal(ScheduleData.CultureDe, faultyLanguages.First());
        }

        [Fact]
        public void GetFaultyLanguagesWhenNoOutcomesThenCheckOnlyName()
        {
            _apiMd1.outcomes = null;
            _dtoMd1 = new MarketDescriptionDto(_apiMd1);
            _ciMd = MarketDescriptionCacheItem.Build(_dtoMd1, _mappingValidatorFactory, ScheduleData.CultureEn, CacheItemSource);

            var faultyLanguages = _ciMd.GetFaultyLanguages();

            Assert.Empty(faultyLanguages);
        }
    }

    private static void ValidateMapping(mappingsMapping msg, MarketMappingCacheItem ci, CultureInfo culture)
    {
        var ciMarketId = ci.MarketSubTypeId == null ? ci.MarketTypeId.ToString() : $"{ci.MarketTypeId}:{ci.MarketSubTypeId}";
        Assert.Equal(msg.market_id, ciMarketId);
        Assert.Equal(msg.sport_id, ci.SportId.ToString());
        Assert.Equal(msg.sov_template, ci.SovTemplate);
        Assert.Equal(msg.valid_for, ci.ValidFor);

        ValidateMappingProducerIds(msg, ci);
        ValidateMappingOutcomes(msg, ci, culture);
    }

    private static void ValidateMappingProducerIds(mappingsMapping msg, MarketMappingCacheItem ci)
    {
        if (!string.IsNullOrEmpty(msg.product_ids))
        {
            var ids = msg.product_ids.Split(new[] { SdkInfo.MarketMappingProductsDelimiter }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
            Assert.Equal(ids.Count, ci.ProducerIds.Count());
            foreach (var id in ids)
            {
                Assert.True(ci.ProducerIds.Contains(id), $"Missing producerId:{id}.");
            }
        }
        else
        {
            _ = Assert.Single(ci.ProducerIds);
            Assert.Equal(msg.product_id, ci.ProducerIds.First());
        }
    }

    private static void ValidateMappingOutcomes(mappingsMapping msg, MarketMappingCacheItem ci, CultureInfo culture)
    {
        if (msg.mapping_outcome != null)
        {
            Assert.Equal(msg.mapping_outcome.Length, ci.OutcomeMappings.Count());

            for (var i = 0; i < msg.mapping_outcome.Length; i++)
            {
                ValidateMappingOutcome(msg.mapping_outcome[i], ci.OutcomeMappings[i], culture);
            }
        }
        else
        {
            Assert.Empty(ci.OutcomeMappings);
        }
    }

    private MarketDescriptionCacheItem BuildMarketDescriptionCacheItem(MarketDescriptionDto dto, CultureInfo culture = null)
    {
        return MarketDescriptionCacheItem.Build(dto, _mappingValidatorFactory, culture ?? ScheduleData.CultureEn, CacheItemSource);
    }

    private static void ValidateMappingOutcome(mappingsMappingMapping_outcome msg, OutcomeMappingCacheItem ci, CultureInfo culture)
    {
        Assert.Equal(msg.outcome_id, ci.OutcomeId);
        Assert.Equal(msg.product_outcome_id, ci.ProducerOutcomeId);
        Assert.Equal(msg.product_outcome_name, ci.ProducerOutcomeNames[culture]);
    }

    private static void ValidateOutcome(desc_outcomesOutcome msg, MarketOutcomeCacheItem ci, CultureInfo culture)
    {
        Assert.Equal(msg.id, ci.Id);
        Assert.Equal(msg.name, ci.GetName(culture));
        Assert.Equal(msg.description, ci.GetDescription(culture));
    }

    private static void ValidateSpecifier(desc_specifiersSpecifier msg, MarketSpecifierCacheItem ci)
    {
        Assert.Equal(msg.type, ci.Type);
        Assert.Equal(msg.name, ci.Name);
    }
}

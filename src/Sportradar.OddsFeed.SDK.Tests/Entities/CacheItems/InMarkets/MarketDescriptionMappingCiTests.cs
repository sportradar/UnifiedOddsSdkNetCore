// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InMarkets;

public class MarketDescriptionMappingCiTests
{
    private readonly mappingsMapping _apiMarketMapping;
    private readonly MarketMappingCacheItem _ciMarketMapping;
    private readonly IMappingValidatorFactory _mappingValidatorFactory;

    public MarketDescriptionMappingCiTests()
    {
        _mappingValidatorFactory = new MappingValidatorFactory();
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
        var dtoMarketMapping = new MarketMappingDto(_apiMarketMapping);
        _ciMarketMapping = MarketMappingCacheItem.Build(dtoMarketMapping, _mappingValidatorFactory, ScheduleData.CultureEn);
    }

    [Fact]
    public void ConstructWhenValidDto()
    {
        Assert.NotNull(_ciMarketMapping);
        Assert.Equal(8, _ciMarketMapping.MarketTypeId);
        Assert.Equal(232, _ciMarketMapping.MarketSubTypeId);
        Assert.Null(_ciMarketMapping.OrgMarketId);
        Assert.Equal(2, _ciMarketMapping.ProducerIds.Count());
        Assert.Contains(1, _ciMarketMapping.ProducerIds);
        Assert.Contains(4, _ciMarketMapping.ProducerIds);
        Assert.Equal(_apiMarketMapping.sport_id, _ciMarketMapping.SportId.ToString());
        Assert.Equal(_apiMarketMapping.sov_template, _ciMarketMapping.SovTemplate);
        Assert.Equal(_apiMarketMapping.valid_for, _ciMarketMapping.ValidFor);
    }

    [Fact]
    public void ConstructWhenHasMappingOutcomes()
    {
        Assert.Equal(2, _ciMarketMapping.OutcomeMappings.Count);
    }

    [Fact]
    public void ConstructWhenValidForPresentThenHasValidator()
    {
        Assert.NotNull(_ciMarketMapping.ValidFor);
        Assert.NotNull(_ciMarketMapping.Validator);
    }

    [Fact]
    public void ConstructWhenValidForNullThenValidatorIsNull()
    {
        _apiMarketMapping.valid_for = null;

        var ciMarketMapping = MarketMappingCacheItem.Build(new MarketMappingDto(_apiMarketMapping), _mappingValidatorFactory, ScheduleData.CultureEn);

        Assert.Null(ciMarketMapping.ValidFor);
        Assert.Null(ciMarketMapping.Validator);
    }

    [Fact]
    public void ConstructWhenValidForEmptyThenValidatorIsNull()
    {
        _apiMarketMapping.valid_for = string.Empty;

        var ciMarketMapping = MarketMappingCacheItem.Build(new MarketMappingDto(_apiMarketMapping), _mappingValidatorFactory, ScheduleData.CultureEn);

        Assert.Empty(ciMarketMapping.ValidFor);
        Assert.Null(ciMarketMapping.Validator);
    }

    [Fact]
    public void ConstructWhenNoOutcomeMappingThenReturnEmptyList()
    {
        _apiMarketMapping.mapping_outcome = null;

        var ciMarketMapping = MarketMappingCacheItem.Build(new MarketMappingDto(_apiMarketMapping), _mappingValidatorFactory, ScheduleData.CultureEn);

        Assert.Empty(ciMarketMapping.OutcomeMappings);
    }

    [Fact]
    public void MergeNewData()
    {
        var dtoMarketMapping = new MarketMappingDto(_apiMarketMapping);

        _ciMarketMapping.Merge(dtoMarketMapping, ScheduleData.CultureDe);

        Assert.True(_ciMarketMapping.OutcomeMappings.All(a => a.ProducerOutcomeNames.Count == 2));
    }

    [Fact]
    public void MergeWhenOutcomeMappingAreNullThenSkipMerging()
    {
        _apiMarketMapping.mapping_outcome = null;
        var dtoMarketMapping = new MarketMappingDto(_apiMarketMapping);

        _ciMarketMapping.Merge(dtoMarketMapping, ScheduleData.CultureDe);

        Assert.True(_ciMarketMapping.OutcomeMappings.All(a => a.ProducerOutcomeNames.Count == 1));
    }

    [Fact]
    public void MergeWhenOutcomeMappingOutcomeIdIsNewThenAddMappingOutcomes()
    {
        _apiMarketMapping.mapping_outcome[0].outcome_id = "23";
        _apiMarketMapping.mapping_outcome[1].outcome_id = "22";
        var dtoMarketMapping = new MarketMappingDto(_apiMarketMapping);

        _ciMarketMapping.Merge(dtoMarketMapping, ScheduleData.CultureDe);

        Assert.Equal(4, _ciMarketMapping.OutcomeMappings.Count);
        Assert.True(_ciMarketMapping.OutcomeMappings.All(a => a.ProducerOutcomeNames.Count == 1));
    }

    [Fact]
    public void ConstructWhenNullDtoThenThrows()
    {
        Assert.Throws<ArgumentNullException>(() => MarketMappingCacheItem.Build(null, _mappingValidatorFactory, ScheduleData.CultureEn));
    }

    [Fact]
    public void ConstructWhenNullValidationFactoryThenThrows()
    {
        Assert.Throws<ArgumentNullException>(() => MarketMappingCacheItem.Build(new MarketMappingDto(_apiMarketMapping), null, ScheduleData.CultureEn));
    }

    [Fact]
    public void ConstructWhenNullLanguageThenThrows()
    {
        Assert.Throws<ArgumentNullException>(() => MarketMappingCacheItem.Build(new MarketMappingDto(_apiMarketMapping), _mappingValidatorFactory, null));
    }

    [Fact]
    public void OutcomeMappingConstructWhenValidDto()
    {
        const string marketId = "123";
        var apiMappingOutcome = new mappingsMappingMapping_outcome { outcome_id = "13", product_outcome_id = "2528", product_outcome_name = "under" };
        var dtoOutcomeMapping = new OutcomeMappingDto(apiMappingOutcome, marketId);

        var ciOutcomeMapping = new OutcomeMappingCacheItem(dtoOutcomeMapping, ScheduleData.CultureEn);

        Assert.NotNull(ciOutcomeMapping);
        Assert.Equal(marketId, ciOutcomeMapping.MarketId);
        Assert.Equal(apiMappingOutcome.outcome_id, ciOutcomeMapping.OutcomeId);
        Assert.Equal(apiMappingOutcome.product_outcome_id, ciOutcomeMapping.ProducerOutcomeId);
        Assert.Single(ciOutcomeMapping.ProducerOutcomeNames);
        Assert.Equal(apiMappingOutcome.product_outcome_name, ciOutcomeMapping.ProducerOutcomeNames[ScheduleData.CultureEn]);
    }

    [Fact]
    public void OutcomeMappingConstructWhenNullDtoThenThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new OutcomeMappingCacheItem(null, ScheduleData.CultureEn));
    }

    [Fact]
    public void OutcomeMappingConstructWhenNullLanguageThenThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new OutcomeMappingCacheItem(null, null));
    }
}

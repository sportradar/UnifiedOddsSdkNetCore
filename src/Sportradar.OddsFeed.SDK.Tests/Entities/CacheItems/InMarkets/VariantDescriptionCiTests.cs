// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InMarkets;

public class VariantDescriptionCiTests
{
    private readonly desc_variant _apiMd;
    private VariantDescriptionDto _dtoMd;
    private VariantDescriptionCacheItem _ciMd;
    private readonly IMappingValidatorFactory _mappingValidatorFactory;
    private const string CacheItemSource = "TestSource";

    public VariantDescriptionCiTests()
    {
        _mappingValidatorFactory = new MappingValidatorFactory();
        const string variantDescriptionXml = @"<variant_descriptions response_code='OK'>
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
        _ciMd = VariantDescriptionCacheItem.Build(_dtoMd, _mappingValidatorFactory, ScheduleData.CultureEn, CacheItemSource);
    }

    [Fact]
    public void ConstructionWhenValidDtoThenSucceeds()
    {
        Assert.NotNull(_ciMd);
    }

    [Fact]
    public void ConstructWhenNullDtoThenThrows()
    {
        Assert.Throws<ArgumentNullException>(() => VariantDescriptionCacheItem.Build(null, _mappingValidatorFactory, ScheduleData.CultureEn, CacheItemSource));
    }

    [Fact]
    public void ConstructWhenNullMappingValidatorThenThrows()
    {
        Assert.Throws<ArgumentNullException>(() => VariantDescriptionCacheItem.Build(_dtoMd, null, ScheduleData.CultureEn, CacheItemSource));
    }

    [Fact]
    public void ConstructWhenNullCultureThenThrows()
    {
        Assert.Throws<ArgumentNullException>(() => VariantDescriptionCacheItem.Build(_dtoMd, _mappingValidatorFactory, null, CacheItemSource));
    }

    [Fact]
    public void ConstructionWhenNullSourceThenSucceeds()
    {
        _ciMd = VariantDescriptionCacheItem.Build(_dtoMd, _mappingValidatorFactory, ScheduleData.CultureEn, null);

        Assert.NotNull(_ciMd);
        Assert.Null(_ciMd.SourceCache);
    }

    [Fact]
    public void ConstructionWhenEmptySourceThenSucceeds()
    {
        _ciMd = VariantDescriptionCacheItem.Build(_dtoMd, _mappingValidatorFactory, ScheduleData.CultureEn, string.Empty);

        Assert.NotNull(_ciMd);
        Assert.Empty(_ciMd.SourceCache);
    }

    [Fact]
    public void MappedId()
    {
        Assert.Equal(_apiMd.id, _dtoMd.Id);
    }

    [Fact]
    public void WhenInputMappingsNull()
    {
        _apiMd.mappings = null;
        _dtoMd = new VariantDescriptionDto(_apiMd);

        _ciMd = BuildVariantDescriptionCacheItem(_dtoMd);

        Assert.Null(_ciMd.Mappings);
    }

    [Fact]
    public void WhenInputMappingsEmpty()
    {
        _apiMd.mappings = Array.Empty<variant_mappingsMapping>();
        _dtoMd = new VariantDescriptionDto(_apiMd);

        _ciMd = BuildVariantDescriptionCacheItem(_dtoMd);

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
        _dtoMd = new VariantDescriptionDto(_apiMd);

        _ciMd = BuildVariantDescriptionCacheItem(_dtoMd);

        Assert.Null(_ciMd.Outcomes);
    }

    [Fact]
    public void WhenInputOutcomesEmpty()
    {
        _apiMd.outcomes = Array.Empty<desc_variant_outcomesOutcome>();
        _dtoMd = new VariantDescriptionDto(_apiMd);

        _ciMd = BuildVariantDescriptionCacheItem(_dtoMd);

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
        Assert.False(_ciMd.HasTranslationsFor(null));
    }

    [Collection(NonParallelCollectionFixture.NonParallelTestCollection)]
    public class MergeTests
    {
        private readonly desc_variant _apiMd1;
        private readonly desc_variant _apiMd2;
        private VariantDescriptionDto _dtoMd2;
        private readonly VariantDescriptionCacheItem _ciMd;

        public MergeTests()
        {
            const string mdXml1 = @"<variant_descriptions response_code='OK'>
                            <variant id='sr:winning_margin:6+'>
                                <outcomes>
                                    <outcome id='sr:winning_margin:6+:120' name='{$competitor1} by 6+'/>
                                    <outcome id='sr:winning_margin:6+:121' name='{$competitor2} by 6+'/>
                                    <outcome id='sr:winning_margin:6+:122' name='other'/>
                                </outcomes>
                                <mappings>
                                    <mapping product_id='3' product_ids='3' sport_id='sr:sport:2' market_id='290' product_market_id='492'>
                                        <mapping_outcome outcome_id='sr:winning_margin:6+:120' product_outcome_id='1' product_outcome_name='1'/>
                                        <mapping_outcome outcome_id='sr:winning_margin:6+:121' product_outcome_id='3' product_outcome_name='2'/>
                                        <mapping_outcome outcome_id='sr:winning_margin:6+:122' product_outcome_id='2' product_outcome_name='X'/>
                                    </mapping>
                                    <mapping product_id='1' product_ids='1|4' sport_id='sr:sport:2' market_id='15' product_market_id='6:972'>
                                        <mapping_outcome outcome_id='sr:winning_margin:6+:120' product_outcome_id='14' product_outcome_name='1'/>
                                        <mapping_outcome outcome_id='sr:winning_margin:6+:121' product_outcome_id='16' product_outcome_name='2'/>
                                        <mapping_outcome outcome_id='sr:winning_margin:6+:122' product_outcome_id='15' product_outcome_name='x'/>
                                    </mapping>
                                </mappings>
                            </variant>
                        </variant_descriptions>";
            const string mdXml2 = @"<variant_descriptions response_code='OK'>
                            <variant id='sr:winning_margin:6+'>
                                <outcomes>
                                    <outcome id='sr:winning_margin:6+:120' name='{$competitor1} mit 6+'/>
                                    <outcome id='sr:winning_margin:6+:121' name='{$competitor2} mit 6+'/>
                                    <outcome id='sr:winning_margin:6+:122' name='andere'/>
                                </outcomes>
                                <mappings>
                                    <mapping product_id='3' product_ids='3' sport_id='sr:sport:2' market_id='290' product_market_id='492'>
                                        <mapping_outcome outcome_id='sr:winning_margin:6+:120' product_outcome_id='1' product_outcome_name='1'/>
                                        <mapping_outcome outcome_id='sr:winning_margin:6+:121' product_outcome_id='3' product_outcome_name='2'/>
                                        <mapping_outcome outcome_id='sr:winning_margin:6+:122' product_outcome_id='2' product_outcome_name='X'/>
                                    </mapping>
                                    <mapping product_id='1' product_ids='1|4' sport_id='sr:sport:2' market_id='15' product_market_id='6:972'>
                                        <mapping_outcome outcome_id='sr:winning_margin:6+:120' product_outcome_id='14' product_outcome_name='1'/>
                                        <mapping_outcome outcome_id='sr:winning_margin:6+:121' product_outcome_id='16' product_outcome_name='2'/>
                                        <mapping_outcome outcome_id='sr:winning_margin:6+:122' product_outcome_id='15' product_outcome_name='x'/>
                                    </mapping>
                                </mappings>
                            </variant>
                        </variant_descriptions>";
            var mappingValidatorFactory = new MappingValidatorFactory();
            _apiMd1 = DeserializerHelper.GetDeserializedApiMessage<variant_descriptions>(mdXml1).variant[0];
            _apiMd2 = DeserializerHelper.GetDeserializedApiMessage<variant_descriptions>(mdXml2).variant[0];
            var dtoMd1 = new VariantDescriptionDto(_apiMd1);
            _dtoMd2 = new VariantDescriptionDto(_apiMd2);
            _ciMd = VariantDescriptionCacheItem.Build(dtoMd1, mappingValidatorFactory, ScheduleData.CultureEn, CacheItemSource);
        }

        [Fact]
        public void InitialSetupHasOneLanguage()
        {
            Assert.NotNull(_ciMd);
            Assert.NotNull(_ciMd.Id);
            Assert.Single(_ciMd.FetchedLanguages);
            Assert.Contains(ScheduleData.CultureEn, _ciMd.FetchedLanguages);
            Assert.True(_ciMd.HasTranslationsFor(ScheduleData.CultureEn));

            Assert.Equal(3, _ciMd.Outcomes.Count);
            Assert.Equal(_apiMd1.outcomes[0].name, _ciMd.Outcomes.First().GetName(ScheduleData.CultureEn));
            Assert.Null(_ciMd.Outcomes.First().GetName(ScheduleData.CultureDe));

            Assert.Equal(2, _ciMd.Mappings.Count);
        }

        [Fact]
        public void MergeWhenDtoNullThenThrow()
        {
            Assert.Throws<ArgumentNullException>(() => _ciMd.Merge(null, ScheduleData.CultureDe));
        }

        [Fact]
        public void MergeWhenLanguageNullThenThrow()
        {
            Assert.Throws<ArgumentNullException>(() => _ciMd.Merge(_dtoMd2, null));
        }

        [Fact]
        public void AddingNewDataIsMerged()
        {
            _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.Equal(2, _ciMd.FetchedLanguages.Count);
            Assert.Contains(ScheduleData.CultureEn, _ciMd.FetchedLanguages);
            Assert.Contains(ScheduleData.CultureDe, _ciMd.FetchedLanguages);
            Assert.True(_ciMd.HasTranslationsFor(ScheduleData.CultureEn));
            Assert.True(_ciMd.HasTranslationsFor(ScheduleData.CultureDe));
            Assert.Equal(3, _ciMd.Outcomes.Count);
            Assert.Equal(_apiMd1.outcomes[0].name, _ciMd.Outcomes.First().GetName(ScheduleData.CultureEn));
            Assert.Equal(_apiMd2.outcomes[0].name, _ciMd.Outcomes.First().GetName(ScheduleData.CultureDe));
            Assert.Equal(2, _ciMd.Mappings.Count);
            Assert.Equal(3, _ciMd.Mappings.First().OutcomeMappings.Count);
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
            _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.Equal(2, _ciMd.FetchedLanguages.Count);
        }

        [Fact]
        public async Task MergingDataUpdatesLastFetchedData()
        {
            var currentLastFetchedData = _ciMd.LastDataReceived;
            await Task.Delay(10);

            _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.True(_ciMd.LastDataReceived > currentLastFetchedData);
        }

        [Fact]
        public void MergingOutcomesNormal()
        {
            _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

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
            _dtoMd2 = new VariantDescriptionDto(_apiMd2);

            _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

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
            _apiMd2.outcomes[2].id = "other";
            _dtoMd2 = new VariantDescriptionDto(_apiMd2);

            _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

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
            _apiMd2.outcomes[2].id = "other";
            _dtoMd2 = new VariantDescriptionDto(_apiMd2);

            var mergeResult = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.False(mergeResult.IsAllMerged());
            Assert.Equal(3, mergeResult.GetOutcomeProblem().Count);
        }

        [Fact]
        public void MergingOutcomesWhenOnlyOneOfSeveralOutcomeIdIsWrongThenProblemIsReported()
        {
            _apiMd2.outcomes[0].id = "123";
            _dtoMd2 = new VariantDescriptionDto(_apiMd2);

            var mergeResult = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.False(mergeResult.IsAllMerged());
            Assert.Single(mergeResult.GetOutcomeProblem());
        }

        [Fact]
        public void MergingMappingsNormal()
        {
            _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

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
            _dtoMd2 = new VariantDescriptionDto(_apiMd2);

            _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

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
            _dtoMd2 = new VariantDescriptionDto(_apiMd2);

            var mergeResult = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.True(mergeResult.IsAllMerged());
        }

        [Fact]
        public void MergingMappingsWhenMarketIdDoNotMatchThenIsNotMerged()
        {
            _apiMd2.mappings[0].product_market_id = "123";
            _apiMd2.mappings[1].product_market_id = "18:232";
            _dtoMd2 = new VariantDescriptionDto(_apiMd2);

            _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

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
            _apiMd2.mappings[0].product_market_id = "123";
            _apiMd2.mappings[1].product_market_id = "18:232";
            _dtoMd2 = new VariantDescriptionDto(_apiMd2);

            var mergeResult = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.False(mergeResult.IsAllMerged());
            Assert.Equal(2, mergeResult.GetMappingProblem().Count);
        }

        [Fact]
        public void MergingMappingsWhenSportIdDoNotMatchThenIsNotMerged()
        {
            _apiMd2.mappings[0].sport_id = "sr:sport:123";
            _apiMd2.mappings[1].sport_id = "sr:sport:321";
            _dtoMd2 = new VariantDescriptionDto(_apiMd2);

            _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

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
            _apiMd2.mappings[1].sport_id = "sr:sport:321";
            _dtoMd2 = new VariantDescriptionDto(_apiMd2);

            var mergeResult = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.False(mergeResult.IsAllMerged());
            Assert.Equal(2, mergeResult.GetMappingProblem().Count);
        }

        [Fact]
        public void MergingMappingsWhenProducerIdsDoNotMatchThenIsNotMerged()
        {
            _apiMd2.mappings[0].product_ids = "1|7";
            _apiMd2.mappings[1].product_ids = "10";
            _dtoMd2 = new VariantDescriptionDto(_apiMd2);

            _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

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
            _apiMd2.mappings[1].product_ids = "10";
            _dtoMd2 = new VariantDescriptionDto(_apiMd2);

            var mergeResult = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.False(mergeResult.IsAllMerged());
            Assert.Equal(2, mergeResult.GetMappingProblem().Count);
        }

        [Fact]
        public void MergingMappingsWhenValidForDoNotMatchThenIsNotMerged()
        {
            _apiMd2.mappings[0].valid_for = "mapnr=1";
            _apiMd2.mappings[1].valid_for = "mapnr=2";
            _dtoMd2 = new VariantDescriptionDto(_apiMd2);

            _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

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
            _apiMd2.mappings[1].valid_for = "mapnr=2";
            _dtoMd2 = new VariantDescriptionDto(_apiMd2);

            var mergeResult = _ciMd.Merge(_dtoMd2, ScheduleData.CultureDe);

            Assert.False(mergeResult.IsAllMerged());
            Assert.Equal(2, mergeResult.GetMappingProblem().Count);
        }
    }

    private static void ValidateMapping(variant_mappingsMapping msg, MarketMappingCacheItem ci, CultureInfo culture)
    {
        var ciMarketId = ci.MarketSubTypeId == null ? ci.MarketTypeId.ToString() : $"{ci.MarketTypeId}:{ci.MarketSubTypeId}";
        Assert.Equal(msg.product_market_id, ciMarketId);
        Assert.Equal(msg.sport_id, ci.SportId.ToString());
        Assert.Equal(msg.sov_template, ci.SovTemplate);
        Assert.Equal(msg.valid_for, ci.ValidFor);

        ValidateMappingProducerIds(msg, ci);
        ValidateMappingOutcomes(msg, ci, culture);
    }

    private static void ValidateMappingProducerIds(variant_mappingsMapping msg, MarketMappingCacheItem ci)
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
            Assert.Single(ci.ProducerIds);
            Assert.Equal(msg.product_id, ci.ProducerIds.First());
        }
    }

    private static void ValidateMappingOutcomes(variant_mappingsMapping msg, MarketMappingCacheItem ci, CultureInfo culture)
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

    private VariantDescriptionCacheItem BuildVariantDescriptionCacheItem(VariantDescriptionDto dto, CultureInfo culture = null)
    {
        return VariantDescriptionCacheItem.Build(dto, _mappingValidatorFactory, culture ?? ScheduleData.CultureEn, CacheItemSource);
    }

    private static void ValidateMappingOutcome(variant_mappingsMappingMapping_outcome msg, OutcomeMappingCacheItem ci, CultureInfo culture)
    {
        Assert.Equal(msg.outcome_id, ci.OutcomeId);
        Assert.Equal(msg.product_outcome_id, ci.ProducerOutcomeId);
        Assert.Equal(msg.product_outcome_name, ci.ProducerOutcomeNames[culture]);
    }

    private static void ValidateOutcome(desc_variant_outcomesOutcome msg, MarketOutcomeCacheItem ci, CultureInfo culture)
    {
        Assert.Equal(msg.id, ci.Id);
        Assert.Equal(msg.name, ci.GetName(culture));
    }
}

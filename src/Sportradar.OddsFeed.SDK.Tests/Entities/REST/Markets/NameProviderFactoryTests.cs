using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Castle.Core.Internal;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.Markets
{
    public class NameProviderFactoryTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly TestSportEntityFactoryBuilder _sportEntityFactory;
        private readonly IMarketDescriptionCache _invariantMarketDescriptionCache;
        private readonly IVariantDescriptionCache _variantDescriptionListCache;
        private readonly IMarketDescriptionCache _variantMarketDescriptionCache;
        private readonly IMarketCacheProvider _marketCacheProvider;

        private readonly MemoryCache _invariantMarketDescriptionMemoryCache;
        private readonly MemoryCache _variantMarketDescriptionListMemoryCache;
        private readonly MemoryCache _variantMarketDescriptionMemoryCache;
        private readonly INameProviderFactory _nameProviderFactory;
        private readonly IMatch _match;
        private readonly ScheduleData _scheduleData;

        public NameProviderFactoryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _scheduleData = new ScheduleData(new TestSportEntityFactoryBuilder(outputHelper, ScheduleData.Cultures3), outputHelper);
            _sportEntityFactory = new TestSportEntityFactoryBuilder(_outputHelper, ScheduleData.Cultures3);

            _invariantMarketDescriptionMemoryCache = new MemoryCache("invariantMarketDescriptionCache");
            _variantMarketDescriptionListMemoryCache = new MemoryCache("variantDescriptionCache");
            _variantMarketDescriptionMemoryCache = new MemoryCache("variantMarketDescriptionCache");

            MessageFactorySdk.SetOutputHelper(_outputHelper);

            IMappingValidatorFactory mappingValidatorFactory = new MappingValidatorFactory();

            var timerVdl = new TestTimer(true);
            var timerIdl = new TestTimer(true);
            _invariantMarketDescriptionCache = new InvariantMarketDescriptionCache(_invariantMarketDescriptionMemoryCache, _sportEntityFactory.DataRouterManager, mappingValidatorFactory, timerIdl, _sportEntityFactory.Cultures, _sportEntityFactory.CacheManager);
            _variantDescriptionListCache = new VariantDescriptionListCache(_variantMarketDescriptionListMemoryCache, _sportEntityFactory.DataRouterManager, mappingValidatorFactory, timerVdl, _sportEntityFactory.Cultures, _sportEntityFactory.CacheManager);
            _variantMarketDescriptionCache = new VariantMarketDescriptionCache(_variantMarketDescriptionMemoryCache, _sportEntityFactory.DataRouterManager, mappingValidatorFactory, _sportEntityFactory.CacheManager);
            _marketCacheProvider = new MarketCacheProvider(_invariantMarketDescriptionCache, _variantMarketDescriptionCache, _variantDescriptionListCache);

            _nameProviderFactory = new NameProviderFactory(_marketCacheProvider,
                                                           _sportEntityFactory.ProfileCache,
                                                           new NameExpressionFactory(new OperandFactory(), _sportEntityFactory.ProfileCache),
                                                           ExceptionHandlingStrategy.THROW);

            _match = _sportEntityFactory.GetMatch(ScheduleData.MatchId.Id, ScheduleData.MatchSportId.Id);
        }

        [Fact]
        public void InitialCallsDone()
        {
            Assert.NotNull(_outputHelper);
            Assert.NotEmpty(_invariantMarketDescriptionMemoryCache);
            Assert.NotEmpty(_variantMarketDescriptionListMemoryCache);
            Assert.Empty(_variantMarketDescriptionMemoryCache);
            Assert.Empty(_sportEntityFactory.ProfileMemoryCache);

            Assert.NotNull(_invariantMarketDescriptionCache);
            Assert.NotNull(_variantDescriptionListCache);
            Assert.NotNull(_variantMarketDescriptionCache);
            Assert.NotNull(_sportEntityFactory.ProfileCache);

            Assert.NotNull(_marketCacheProvider);
            Assert.NotNull(_nameProviderFactory);

            Assert.Equal(_sportEntityFactory.Cultures.Count * 2, _sportEntityFactory.DataRouterManager.TotalRestCalls);
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

            Assert.Equal(TestData.InvariantListCacheCount, _invariantMarketDescriptionMemoryCache.GetCount());
            Assert.Equal(TestData.VariantListCacheCount, _variantMarketDescriptionListMemoryCache.GetCount());

            Assert.NotNull(_match);
            Assert.Equal(_scheduleData.MatchCompetitor1.Id, _match.GetHomeCompetitorAsync().GetAwaiter().GetResult().Id);
            Assert.Equal(_scheduleData.MatchCompetitor2.Id, _match.GetAwayCompetitorAsync().GetAwaiter().GetResult().Id);
        }

        [Fact]
        public async Task GetNameFromInvariantMarketDescription()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary("milestone=1|maxovers=3"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, 701, specifiers);
            var name0 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[0]);
            var name1 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[1]);
            var name2 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[2]);

            Assert.NotNull(name0);
            Assert.NotNull(name1);
            Assert.NotNull(name2);
            Assert.NotEqual(name0, name1);
            Assert.NotEqual(name0, name2);
            Assert.NotEqual(name1, name2);

            var outcome1 = await nameProvider.GetOutcomeNameAsync("74", _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome1);
            Assert.Equal("yes", outcome1);
            var outcome2 = await nameProvider.GetOutcomeNameAsync("76", _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome2);
            Assert.Equal("no", outcome2);

            outcome1 = await nameProvider.GetOutcomeNameAsync("74", _sportEntityFactory.Cultures[1]);
            Assert.NotNull(outcome1);
            Assert.Equal("ja", outcome1);
            outcome2 = await nameProvider.GetOutcomeNameAsync("76", _sportEntityFactory.Cultures[1]);
            Assert.NotNull(outcome2);
            Assert.Equal("nein", outcome2);

            Assert.Equal(_sportEntityFactory.Cultures.Count * 2, _sportEntityFactory.DataRouterManager.TotalRestCalls);
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [Fact]
        public void GetMarketNameFromNonExistingInvariantMarketDescription()
        {
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, 2820, null);
            string name0 = null;
            Action action = () => name0 = nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[0]).GetAwaiter().GetResult();
            action.Should().Throw<NameGenerationException>();
            Assert.Null(name0);
        }

        [Fact]
        public async Task GetUnExistingOutcomeNameFromExistingInvariantMarketDescription()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary("milestone=4|maxovers=2"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, 701, specifiers);
            var name0 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[0]);
            Assert.NotNull(name0);

            var outcome1 = await nameProvider.GetOutcomeNameAsync("74", _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome1);
            Assert.Equal("yes", outcome1);

            Action action = () => outcome1 = nameProvider.GetOutcomeNameAsync("75", _sportEntityFactory.Cultures[0]).GetAwaiter().GetResult();
            action.Should().Throw<NameGenerationException>();
        }

        [Fact]
        public async Task GetMarketNamesFromVariantMarketDescriptionFromList()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary("setnr=1|variant=sr:correct_score:bestof:12"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, 374, specifiers);

            var name0 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[0]);
            var name1 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[1]);
            var name2 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[2]);

            Assert.NotNull(name0);
            Assert.NotNull(name1);
            Assert.NotNull(name2);
            Assert.NotEqual(name0, name1);
            Assert.NotEqual(name0, name2);
            Assert.NotEqual(name1, name2);

            var outcome1 = await nameProvider.GetOutcomeNameAsync("sr:correct_score:bestof:12:192", _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome1);
            Assert.Equal("7:0", outcome1);
            var outcome2 = await nameProvider.GetOutcomeNameAsync("sr:correct_score:bestof:12:200", _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome2);
            Assert.Equal("4:7", outcome2);

            Assert.Equal(_sportEntityFactory.Cultures.Count * 2, _sportEntityFactory.DataRouterManager.TotalRestCalls);
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [Fact]
        public async Task GetMarketNamesFromVariantMarketDescriptionFromListMultipleMappings()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary("variant=sr:decided_by_extra_points:bestof:5"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, 239, specifiers);

            var name0 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[0]);
            var name1 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[1]);
            var name2 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[2]);

            Assert.NotNull(name0);
            Assert.NotNull(name1);
            Assert.NotNull(name2);
            Assert.NotEqual(name0, name1);
            Assert.NotEqual(name0, name2);
            Assert.NotEqual(name1, name2);

            var outcome1 = await nameProvider.GetOutcomeNameAsync("sr:decided_by_extra_points:bestof:5:53", _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome1);
            Assert.Equal("0", outcome1);
            var outcome2 = await nameProvider.GetOutcomeNameAsync("sr:decided_by_extra_points:bestof:5:58", _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome2);
            Assert.Equal("5", outcome2);

            Assert.Equal(_sportEntityFactory.Cultures.Count * 2, _sportEntityFactory.DataRouterManager.TotalRestCalls);
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [Fact]
        public async Task GetMarketNamesFromVariantMarketDescriptionFromSinglePreOutcomeText()
        {
            const int marketId = 534;
            const string variantSpecifier = "pre:markettext:168883";
            _sportEntityFactory.DataRouterManager.UriReplacements.Add(new Tuple<string, string>($"{marketId}_{variantSpecifier}", $"variant_market_description_{marketId}_culture.xml"));

            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary($"variant={variantSpecifier}"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, marketId, specifiers);

            Assert.Empty(_variantMarketDescriptionMemoryCache);

            var name0 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[0]);
            var name1 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[1]);
            var name2 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[2]);

            Assert.Single(_variantMarketDescriptionMemoryCache);

            Assert.NotNull(name0);
            Assert.NotNull(name1);
            Assert.NotNull(name2);
            Assert.NotEqual(name0, name1);
            Assert.NotEqual(name0, name2);
            Assert.NotEqual(name1, name2);

            var outcome1 = await nameProvider.GetOutcomeNameAsync("pre:outcometext:108719", _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome1);
            Assert.Equal("7 Points", outcome1);
            var outcome2 = await nameProvider.GetOutcomeNameAsync("pre:outcometext:108726", _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome2);
            Assert.Equal("1 Point", outcome2);

            Assert.Equal(_sportEntityFactory.Cultures.Count * 3, _sportEntityFactory.DataRouterManager.TotalRestCalls);
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [Fact]
        public async Task GetMarketNamesForVariantMarketDescriptionFromSinglePlayerProps()
        {
            const int marketId = 768;
            const string variantSpecifier = "pre:playerprops:35432179:608000";
            _sportEntityFactory.DataRouterManager.UriReplacements.Add(new Tuple<string, string>($"{marketId}_{variantSpecifier}", $"variant_market_description_{marketId}_culture.xml"));

            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary($"variant={variantSpecifier}"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, marketId, specifiers);

            Assert.Empty(_variantMarketDescriptionMemoryCache);

            var name0 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[0]);
            var name1 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[1]);
            var name2 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[2]);

            Assert.Single(_variantMarketDescriptionMemoryCache);

            Assert.NotNull(name0);
            Assert.NotNull(name1);
            Assert.NotNull(name2);
            Assert.NotEqual(name0, name1);
            Assert.NotEqual(name0, name2);
            Assert.NotEqual(name1, name2);

            var outcome1 = await nameProvider.GetOutcomeNameAsync("pre:playerprops:35432179:608000:10", _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome1);
            Assert.Equal("Kyle Anderson 10+", outcome1);
            var outcome2 = await nameProvider.GetOutcomeNameAsync("pre:playerprops:35432179:608000:20", _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome2);
            Assert.Equal("Kyle Anderson 20+", outcome2);

            Assert.Equal(_sportEntityFactory.Cultures.Count * 3, _sportEntityFactory.DataRouterManager.TotalRestCalls);
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [Fact]
        public async Task GetMarketNamesForVariantMarketDescriptionFromSingleUnsupportedCompetitorProps()
        {
            const int marketId = 1768;
            const string variantSpecifier = "pre:competitorprops:35432179:608000";
            await LoadNewInvariantMarketDescriptionAsync(768, marketId, 1, _sportEntityFactory.Cultures).ConfigureAwait(false);
            _sportEntityFactory.DataRouterManager.UriReplacements.Add(new Tuple<string, string>($"{marketId}_{variantSpecifier}", $"variant_market_description_{marketId}_cp_culture.xml"));
            Assert.Equal(TestData.InvariantListCacheCount + 1, _invariantMarketDescriptionMemoryCache.GetCount());
            Assert.Empty(_variantMarketDescriptionMemoryCache);

            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary($"variant={variantSpecifier}"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, marketId, specifiers);

            Assert.Empty(_variantMarketDescriptionMemoryCache);

            var name0 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[0]);
            var name1 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[1]);
            var name2 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[2]);

            Assert.Single(_variantMarketDescriptionMemoryCache);

            Assert.NotNull(name0);
            Assert.NotNull(name1);
            Assert.NotNull(name2);
            Assert.NotEqual(name0, name1);
            Assert.NotEqual(name0, name2);
            Assert.NotEqual(name1, name2);

            var outcome1 = await nameProvider.GetOutcomeNameAsync("pre:competitorprops:35432179:608000:10", _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome1);
            Assert.Equal("Kyle Anderson 10+", outcome1);
            var outcome2 = await nameProvider.GetOutcomeNameAsync("pre:competitorprops:35432179:608000:20", _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome2);
            Assert.Equal("Kyle Anderson 20+", outcome2);

            Assert.Equal(_sportEntityFactory.Cultures.Count * 3, _sportEntityFactory.DataRouterManager.TotalRestCalls);
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [Fact]
        public async Task GetMarketNamesForMarketDescriptionForOutcomeTypePlayerForSingleCulture()
        {
            Assert.Equal(_sportEntityFactory.Cultures.Count * 2, _sportEntityFactory.DataRouterManager.TotalRestCalls);
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));
            Assert.Empty(_variantMarketDescriptionMemoryCache);

            const int marketId = 679;
            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary("maxovers=20|type=live|inningnr=1"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, marketId, specifiers);
            Assert.Equal(_sportEntityFactory.Cultures.Count * 2, _sportEntityFactory.DataRouterManager.TotalRestCalls);
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));
            Assert.Empty(_variantMarketDescriptionMemoryCache);
            _sportEntityFactory.DataRouterManager.ResetMethodCall();

            var marketName = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[0]);
            Assert.Empty(_variantMarketDescriptionMemoryCache);
            Assert.NotNull(marketName);
            Assert.False(string.IsNullOrEmpty(marketName));
            Assert.Equal(1, _sportEntityFactory.DataRouterManager.TotalRestCalls);
            Assert.Equal(1, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            var outcome1 = await nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitor1PlayerId1.ToString(), _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome1);
            Assert.Equal(_scheduleData.MatchCompetitor1Player1.GetName(_sportEntityFactory.Cultures[0]), outcome1);
            var outcome2 = await nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitor1PlayerId2.ToString(), _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome2);
            Assert.Equal(_scheduleData.MatchCompetitor1Player2.GetName(_sportEntityFactory.Cultures[0]), outcome2);

            Assert.Equal(3, _sportEntityFactory.DataRouterManager.TotalRestCalls);
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
            Assert.Equal(1, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.Equal(2, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [Fact]
        public async Task GetMarketNamesForMarketDescriptionForOutcomeTypePlayerForSingleCultureWhenCompetitorPreloaded()
        {
            await _sportEntityFactory.DataRouterManager.GetCompetitorAsync(ScheduleData.MatchCompetitorId1, _sportEntityFactory.Cultures[0], null).ConfigureAwait(false);
            await _sportEntityFactory.DataRouterManager.GetCompetitorAsync(ScheduleData.MatchCompetitorId2, _sportEntityFactory.Cultures[0], null).ConfigureAwait(false);

            const int marketId = 679;
            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary("maxovers=20|type=live|inningnr=1"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, marketId, specifiers);
            Assert.Empty(_variantMarketDescriptionMemoryCache);

            _sportEntityFactory.DataRouterManager.ResetMethodCall();

            var marketName = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[0]);
            Assert.Empty(_variantMarketDescriptionMemoryCache);
            Assert.NotNull(marketName);
            Assert.False(string.IsNullOrEmpty(marketName));
            Assert.Equal(1, _sportEntityFactory.DataRouterManager.TotalRestCalls);
            Assert.Equal(1, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            Assert.Equal(1, _sportEntityFactory.DataRouterManager.TotalRestCalls);
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
            Assert.Equal(1, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [Fact]
        public async Task GetPlayerOutcomeNamesForMarketDescriptionForOutcomeTypePlayerForSingleCultureWhenCompetitorPreloaded()
        {
            await _sportEntityFactory.DataRouterManager.GetCompetitorAsync(ScheduleData.MatchCompetitorId1, _sportEntityFactory.Cultures[0], null).ConfigureAwait(false);
            await _sportEntityFactory.DataRouterManager.GetCompetitorAsync(ScheduleData.MatchCompetitorId2, _sportEntityFactory.Cultures[0], null).ConfigureAwait(false);

            const int marketId = 679;
            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary("maxovers=20|type=live|inningnr=1"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, marketId, specifiers);
            Assert.Empty(_variantMarketDescriptionMemoryCache);

            _sportEntityFactory.DataRouterManager.ResetMethodCall();

            var outcome1 = await nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitor2PlayerId2.ToString(), _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome1);
            Assert.Equal(_scheduleData.MatchCompetitor2Player2.GetName(_sportEntityFactory.Cultures[0]), outcome1);
            var outcome2 = await nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitor2PlayerId1.ToString(), _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome2);
            Assert.Equal(_scheduleData.MatchCompetitor2Player1.GetName(_sportEntityFactory.Cultures[0]), outcome2);

            Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [Fact]
        public async Task GetMarketNamesForMarketDescriptionForOutcomeTypePlayer()
        {
            const int marketId = 679;
            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary("maxovers=20|type=live|inningnr=1"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, marketId, specifiers);
            Assert.Empty(_variantMarketDescriptionMemoryCache);

            _sportEntityFactory.DataRouterManager.ResetMethodCall();

            var name0 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[0]);
            var name1 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[1]);
            var name2 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[2]);

            Assert.Empty(_variantMarketDescriptionMemoryCache);

            Assert.NotNull(name0);
            Assert.NotNull(name1);
            Assert.NotNull(name2);
            Assert.NotEqual(name0, name1);
            Assert.NotEqual(name0, name2);
            Assert.NotEqual(name1, name2);

            var outcome1 = await nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitor2PlayerId2.ToString(), _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome1);
            Assert.Equal(_scheduleData.MatchCompetitor2Player2.GetName(_sportEntityFactory.Cultures[0]), outcome1);
            var outcome2 = await nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitor1PlayerId2.ToString(), _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome2);
            Assert.Equal(_scheduleData.MatchCompetitor1Player2.GetName(_sportEntityFactory.Cultures[0]), outcome2);

            Assert.Equal(5, _sportEntityFactory.DataRouterManager.TotalRestCalls);
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
            Assert.Equal(3, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.Equal(2, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [Fact]
        public async Task GetMarketNamesForMarketDescriptionForOutComeTypeCompetitor()
        {
            const int marketId = 1109;
            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary("lapnr=5"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, marketId, specifiers);
            Assert.Empty(_variantMarketDescriptionMemoryCache);

            _sportEntityFactory.DataRouterManager.ResetMethodCall();

            var name0 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[0]);
            var name1 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[1]);
            var name2 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[2]);

            Assert.Empty(_variantMarketDescriptionMemoryCache);

            Assert.NotNull(name0);
            Assert.NotNull(name1);
            Assert.NotNull(name2);
            Assert.NotEqual(name0, name1);
            Assert.NotEqual(name0, name2);
            Assert.NotEqual(name1, name2);

            var outcome1 = await nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId1.ToString(), _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome1);
            Assert.Equal(_scheduleData.MatchCompetitor1.GetName(_sportEntityFactory.Cultures[0]), outcome1);
            var outcome2 = await nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId2.ToString(), _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome2);
            Assert.Equal(_scheduleData.MatchCompetitor2.GetName(_sportEntityFactory.Cultures[0]), outcome2);

            Assert.Equal(1, _sportEntityFactory.DataRouterManager.TotalRestCalls);
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
            Assert.Equal(1, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [Fact]
        public async Task GetMarketNamesForMarketDescriptionWithCompetitorInOutcomeTemplate()
        {
            const int marketId = 303;
            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary("quarternr=4|hcp=-3.5"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, marketId, specifiers);

            Assert.Empty(_variantMarketDescriptionMemoryCache);

            var name0 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[0]);
            var name1 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[1]);
            var name2 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[2]);

            Assert.Empty(_variantMarketDescriptionMemoryCache);

            Assert.NotNull(name0);
            Assert.NotNull(name1);
            Assert.NotNull(name2);
            Assert.NotEqual(name0, name1);
            Assert.NotEqual(name0, name2);
            Assert.NotEqual(name1, name2);

            var outcome1 = await nameProvider.GetOutcomeNameAsync("1714", _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome1);
            Assert.Equal($"{_scheduleData.MatchCompetitor1.GetName(_sportEntityFactory.Cultures[0])} (-3.5)", outcome1);
            var outcome2 = await nameProvider.GetOutcomeNameAsync("1715", _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome2);
            Assert.Equal($"{_scheduleData.MatchCompetitor2.GetName(_sportEntityFactory.Cultures[0])} (+3.5)", outcome2);

            Assert.Equal((_sportEntityFactory.Cultures.Count * 2) + 1, _sportEntityFactory.DataRouterManager.TotalRestCalls);
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
            Assert.Equal(1, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [Fact]
        public async Task GetMarketNamesForMarketDescriptionWithScoreInNameTemplate()
        {
            const int marketId = 41;
            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary("score=2:0"));
            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, marketId, specifiers);

            Assert.Empty(_variantMarketDescriptionMemoryCache);

            var name0 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[0]);
            var name1 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[1]);
            var name2 = await nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[2]);

            Assert.Empty(_variantMarketDescriptionMemoryCache);

            Assert.NotNull(name0);
            Assert.NotNull(name1);
            Assert.NotNull(name2);
            Assert.NotEqual(name0, name1);
            Assert.NotEqual(name0, name2);
            Assert.NotEqual(name1, name2);

            var outcome1 = await nameProvider.GetOutcomeNameAsync("134", _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome1);
            Assert.Equal("6:1", outcome1);
            var outcome2 = await nameProvider.GetOutcomeNameAsync("144", _sportEntityFactory.Cultures[0]);
            Assert.NotNull(outcome2);
            Assert.Equal("5:2", outcome2);

            Assert.Equal(_sportEntityFactory.Cultures.Count * 2, _sportEntityFactory.DataRouterManager.TotalRestCalls);
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        [Fact]
        public void GetMarketNamesForVariantMarketDescriptionFromSinglePreOutcomeTextInvalidOutcome()
        {
            const int marketId = 534;
            const string variantSpecifier = "pre:markettext:168883";
            var specifiers = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary($"variant={variantSpecifier}"));
            _sportEntityFactory.DataRouterManager.UriReplacements.Add(new Tuple<string, string>($"{marketId}_{variantSpecifier}", $"variant_market_description_invalid_{marketId}_culture.xml"));

            var nameProvider = _nameProviderFactory.BuildNameProvider(_match, marketId, specifiers);

            Assert.Empty(_variantMarketDescriptionMemoryCache);

            string name0 = null;
            Action action = () => name0 = nameProvider.GetMarketNameAsync(_sportEntityFactory.Cultures[0]).GetAwaiter().GetResult();
            action.Should().Throw<NameGenerationException>();
            Assert.Null(name0);

            Assert.Empty(_variantMarketDescriptionMemoryCache);

            Assert.Equal(7, _sportEntityFactory.DataRouterManager.TotalRestCalls);
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));
            Assert.Equal(_sportEntityFactory.Cultures.Count, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));
            Assert.Equal(1, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
            Assert.Equal(0, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        }

        private async Task LoadNewInvariantMarketDescriptionAsync(int existingId, int newId, int doAction, IReadOnlyCollection<CultureInfo> cultures)
        {
            foreach (var culture in cultures)
            {
                await LoadNewInvariantMarketDescriptionAsync(existingId, newId, doAction, culture).ConfigureAwait(false);
            }
        }

        private async Task LoadNewInvariantMarketDescriptionAsync(int existingId, int newId, int doAction, CultureInfo culture)
        {
            var restDeserializer = new Deserializer<market_descriptions>();
            var mapper = new MarketDescriptionsMapperFactory();

            var resourceName = $"invariant_market_descriptions_{culture.TwoLetterISOLanguageName}.xml";
            await using var stream = FileHelper.GetResource(resourceName);

            if (stream != null)
            {
                var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
                if (result?.Items.IsNullOrEmpty() == false)
                {
                    var existingMd = result.Items.First(f => f.Id.Equals(existingId));
                    existingMd.OverrideId(newId);
                    switch (doAction)
                    {
                        case 1:
                            {
                                // switch player_props to unknown competitor_props
                                var newGroups = existingMd.Groups.Where(s => !s.Contains("player_props")).ToList();
                                newGroups.Add("competitor_props");
                                existingMd.OverrideGroups(newGroups);
                                break;
                            }
                        default:
                            return;
                    }

                    var items = new EntityList<MarketDescriptionDTO>(new List<MarketDescriptionDTO> { existingMd });
                    await _sportEntityFactory.CacheManager.SaveDtoAsync(URN.Parse("sr:markets:1"), items, culture, DtoType.MarketDescriptionList, null).ConfigureAwait(false);
                }
                else
                {
                    _outputHelper.WriteLine($"No results found for {resourceName}");
                }
            }
        }
    }
}
